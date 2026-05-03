using BitmapOsBuilder;
using System.Buffers.Binary;
using System.Drawing;
using System.Text;

namespace DragonOSBuilder;

internal static class Program
{
    private const int SectorSize = 512;
    private const int IsoSectorSize = 204860;
    private const int SectorsPerTrack = 18;
    private const int HeadsPerCylinder = 2;
    private const int TargetWidth = 320;
    private const int TargetHeight = 200;
    private const int ImageBufferSegment = 0x2000;
    private const int Stage2LoadOffset = 0x8000;
	private const int FloppySize = 1474560;

    private static int Main(string[] args)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var assetsDir = Path.Combine(projectRoot, "assets");
            var buildDir = Path.Combine(projectRoot, "build");

            Directory.CreateDirectory(assetsDir);
            Directory.CreateDirectory(buildDir);

            var bitmapPath = args.Length > 0
                ? Path.GetFullPath(args[0])
                : ResolvePreferredAsset(assetsDir, "splash", ".png", ".bmp");
            var cursorPath = ResolvePreferredAsset(assetsDir, "mouse_normal", ".png", ".bmp");
            var setupIconPath = ResolvePreferredAsset(assetsDir, "Icon_Happy", ".png", ".bmp");

            if (bitmapPath is null || !File.Exists(bitmapPath))
            {
                bitmapPath = Path.Combine(assetsDir, "splash.bmp");
                Console.WriteLine($"No splash asset found, generating a sample image at {bitmapPath}.");
                SampleBitmapWriter.Write(bitmapPath, TargetWidth, TargetHeight);
            }

            var splashSource = ImageAssetLoader.Load(bitmapPath);
            var legacyResized = ImageTools.ResizeNearest(splashSource, TargetWidth, TargetHeight);
            var cursorSource = cursorPath is not null && File.Exists(cursorPath)
                ? ImageAssetLoader.Load(cursorPath)
                : DefaultCursorBuilder.Create();
            var setupIconSource = setupIconPath is not null && File.Exists(setupIconPath)
                ? ImageAssetLoader.Load(setupIconPath)
                : splashSource;
            var packedImage = ImageTools.PackMode13Asset(legacyResized);
            var imageSectorCount = DivideRoundUp(packedImage.Length, SectorSize);
            var runtimeFiles = DragonRuntimeBuilder.Build(projectRoot, assetsDir, buildDir);
            var efiRuntimeFiles = runtimeFiles.Where(file => file.IncludeInEfiPartition).ToList();

            var imageStartLbaPlaceholder = 0;
            var stage2 = BootCodeBuilder.BuildStage2(imageSectorCount, BuildSectorTable(imageStartLbaPlaceholder, imageSectorCount));
            var stage2SectorCount = DivideRoundUp(stage2.Length, SectorSize);

            if (stage2SectorCount > 17)
            {
                throw new InvalidOperationException($"Stage 2 is {stage2SectorCount} sectors. The stage 1 loader expects at most 17.");
            }

            var imageStartLba = 1 + stage2SectorCount;
            var imageSectorTable = BuildSectorTable(imageStartLba, imageSectorCount);
            stage2 = BootCodeBuilder.BuildStage2(imageSectorCount, imageSectorTable);
            stage2SectorCount = DivideRoundUp(stage2.Length, SectorSize);

            if (stage2SectorCount > 17)
            {
                throw new InvalidOperationException($"Stage 2 is {stage2SectorCount} sectors after patching. The stage 1 loader expects at most 17.");
            }

            var stage1 = BootCodeBuilder.BuildStage1(stage2SectorCount);
            var diskImage = BuildDiskImage(stage1, stage2, packedImage);
            var bootx64 = NativeUefiCompiler.Build(projectRoot, buildDir, splashSource, cursorSource, setupIconSource, runtimeFiles);
            var efiSystemPartition = FatImageBuilder.BuildEfiSystemPartition(bootx64, efiRuntimeFiles);
            var isoImage = ImapiIsoBuilder.Build(buildDir, efiSystemPartition, bootx64, runtimeFiles);

            var uefiIsoPath = Path.Combine(buildDir, "os-uefi.iso");
            var legacyIsoAliasPath = Path.Combine(buildDir, "os.iso");

            File.WriteAllBytes(Path.Combine(buildDir, "stage1.bin"), stage1);
            File.WriteAllBytes(Path.Combine(buildDir, "stage2.bin"), PadToSector(stage2));
            File.WriteAllBytes(Path.Combine(buildDir, "image.bin"), PadToSector(packedImage));
            File.WriteAllBytes(Path.Combine(buildDir, "BOOTX64.EFI"), bootx64);
            File.WriteAllBytes(Path.Combine(buildDir, "efiboot.img"), efiSystemPartition);
            File.WriteAllBytes(Path.Combine(buildDir, "os.img"), diskImage);
            File.WriteAllBytes(uefiIsoPath, isoImage);
            TryWriteAliasIso(legacyIsoAliasPath, isoImage);

            Console.WriteLine("Dragon OS image built successfully.");
            Console.WriteLine($"Dragon OS source : {bitmapPath}");
            Console.WriteLine($"Project root  : {projectRoot}");
            Console.WriteLine($"UEFI loader   : {Path.Combine(buildDir, "BOOTX64.EFI")}");
            Console.WriteLine($"EFI system img: {Path.Combine(buildDir, "efiboot.img")}");
            Console.WriteLine($"Bootable ISO  : {uefiIsoPath}");
            Console.WriteLine($"ISO alias     : {legacyIsoAliasPath}");
            Console.WriteLine($"Floppy image  : {Path.Combine(buildDir, "os.img")}");
            Console.WriteLine($"Runtime files : {runtimeFiles.Count}");
            Console.WriteLine($"Stage 2 size  : {stage2.Length} bytes ({stage2SectorCount} sectors)");
            Console.WriteLine($"Image payload : {packedImage.Length} bytes ({imageSectorCount} sectors)");
            Console.WriteLine($"Load buffer   : 0x{ImageBufferSegment:X4}:0000 ({ImageBufferSegment << 4:X5} physical)");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static string? ResolvePreferredAsset(string directory, string baseName, params string[] extensions)
    {
        foreach (var extension in extensions)
        {
            var candidate = Path.Combine(directory, baseName + extension);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static string FindProjectRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "DragonOSBuilder.csproj")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate DragonOSBuilder.csproj.");
    }

    private static byte[] BuildDiskImage(byte[] stage1, byte[] stage2, byte[] imageData)
    {
        if (stage1.Length != SectorSize)
        {
            throw new InvalidOperationException("Stage 1 must be exactly one sector.");
        }

        var paddedStage2 = PadToSector(stage2);
        var paddedImage = PadToSector(imageData);
        var totalUsed = stage1.Length + paddedStage2.Length + paddedImage.Length;

        if (totalUsed > FloppySize)
        {
            throw new InvalidOperationException("The generated image is larger than a 1.44MB floppy.");
        }

        var disk = new byte[FloppySize];
        Buffer.BlockCopy(stage1, 0, disk, 0, stage1.Length);
        Buffer.BlockCopy(paddedStage2, 0, disk, SectorSize, paddedStage2.Length);
        Buffer.BlockCopy(paddedImage, 0, disk, SectorSize + paddedStage2.Length, paddedImage.Length);
        return disk;
    }

    private static byte[] BuildSectorTable(int startLba, int sectorCount)
    {
        var table = new byte[sectorCount * 3];
        for (var i = 0; i < sectorCount; i++)
        {
            var (cylinder, head, sector) = LbaToChs(startLba + i);
            table[(i * 3) + 0] = cylinder;
            table[(i * 3) + 1] = head;
            table[(i * 3) + 2] = sector;
        }

        return table;
    }

    private static (byte Cylinder, byte Head, byte Sector) LbaToChs(int lba)
    {
        var sectorsPerCylinder = SectorsPerTrack * HeadsPerCylinder;
        var cylinder = lba / sectorsPerCylinder;
        var cylinderRemainder = lba % sectorsPerCylinder;
        var head = cylinderRemainder / SectorsPerTrack;
        var sector = (cylinderRemainder % SectorsPerTrack) + 1;

        if (cylinder > byte.MaxValue)
        {
            throw new InvalidOperationException("Cylinder overflow for floppy geometry.");
        }

        return ((byte)cylinder, (byte)head, (byte)sector);
    }

    private static byte[] PadToSector(byte[] data)
    {
        var paddedLength = DivideRoundUp(data.Length, SectorSize) * SectorSize;
        if (paddedLength == data.Length)
        {
            return data;
        }

        var padded = new byte[paddedLength];
        Buffer.BlockCopy(data, 0, padded, 0, data.Length);
        return padded;
    }

    private static int DivideRoundUp(int value, int divisor) => (value + divisor - 1) / divisor;

    private static void TryWriteAliasIso(string path, byte[] data)
    {
        try
        {
            File.WriteAllBytes(path, data);
        }
        catch (IOException)
        {
            Console.WriteLine($"Note         : Could not update {path} because it is currently in use.");
        }
    }
}

internal static class IsoBuilder
{
    private const int IsoSectorSize = 2048;
    private const int SystemAreaSectors = 16;
    private const int PrimaryVolumeDescriptorLba = 16;
    private const int BootRecordLba = 17;
    private const int VolumeDescriptorTerminatorLba = 18;
    private const int PathTableLeLba = 19;
    private const int PathTableBeLba = 20;
    private const int RootDirectoryLba = 21;
    private const int BootCatalogLba = 22;
    private const int BootImageLba = 23;
    private const string VolumeId = "DRAGON_OS";

    public static byte[] BuildBootableIso(byte[] floppyImage)
    {
        if (floppyImage.Length != 1_474_560)
        {
            throw new InvalidOperationException("The ISO builder expects a 1.44MB floppy image.");
        }

        var bootImageSectorCount = floppyImage.Length / IsoSectorSize;
        var totalSectors = BootImageLba + bootImageSectorCount;
        var iso = new byte[totalSectors * IsoSectorSize];
        var timestamp = DateTime.UtcNow;

        WritePrimaryVolumeDescriptor(iso, totalSectors, timestamp);
        WriteBootRecordVolumeDescriptor(iso);
        WriteVolumeDescriptorTerminator(iso);
        WritePathTables(iso);
        WriteRootDirectory(iso, floppyImage.Length, timestamp);
        WriteBootCatalog(iso);
        Buffer.BlockCopy(floppyImage, 0, iso, BootImageLba * IsoSectorSize, floppyImage.Length);

        return iso;
    }

    private static void WritePrimaryVolumeDescriptor(byte[] iso, int totalSectors, DateTime timestamp)
    {
        var sector = new Span<byte>(iso, PrimaryVolumeDescriptorLba * IsoSectorSize, IsoSectorSize);
        sector[0] = 0x01;
        WriteStandardIdentifier(sector);
        sector[6] = 0x01;
        WritePaddedAscii(sector, 8, 32, "DRAGON OS");
        WritePaddedAscii(sector, 40, 32, VolumeId);
        WriteBothEndianUInt32(sector, 80, checked((uint)totalSectors));
        WriteBothEndianUInt16(sector, 120, 1);
        WriteBothEndianUInt16(sector, 124, 1);
        WriteBothEndianUInt16(sector, 128, IsoSectorSize);
        WriteBothEndianUInt32(sector, 132, 10);
        BinaryPrimitives.WriteUInt32LittleEndian(sector.Slice(140, 4), PathTableLeLba);
        BinaryPrimitives.WriteUInt32BigEndian(sector.Slice(148, 4), PathTableBeLba);
        var rootRecord = BuildDirectoryRecord(
            extentLba: RootDirectoryLba,
            dataLength: IsoSectorSize,
            recordingTime: timestamp,
            flags: 0x02,
            fileIdentifier: [0x00]);
        rootRecord.CopyTo(sector.Slice(156, rootRecord.Length));
        WritePaddedAscii(sector, 318, 128, "Dragon OS Builder");
        WriteVolumeTimestamp(sector.Slice(813, 17), timestamp);
        WriteVolumeTimestamp(sector.Slice(830, 17), timestamp);
        WriteVolumeTimestamp(sector.Slice(847, 17), timestamp);
        WriteVolumeTimestamp(sector.Slice(864, 17), timestamp);
        sector[881] = 0x01;
    }

    private static void WriteBootRecordVolumeDescriptor(byte[] iso)
    {
        var sector = new Span<byte>(iso, BootRecordLba * IsoSectorSize, IsoSectorSize);
        sector[0] = 0x00;
        WriteStandardIdentifier(sector);
        sector[6] = 0x01;
        WritePaddedAscii(sector, 7, 32, "EL TORITO SPECIFICATION");
        WritePaddedAscii(sector, 39, 32, "DRAGON OS BOOT");
        BinaryPrimitives.WriteUInt32LittleEndian(sector.Slice(71, 4), BootCatalogLba);
    }

    private static void WriteVolumeDescriptorTerminator(byte[] iso)
    {
        var sector = new Span<byte>(iso, VolumeDescriptorTerminatorLba * IsoSectorSize, IsoSectorSize);
        sector[0] = 0xFF;
        WriteStandardIdentifier(sector);
        sector[6] = 0x01;
    }

    private static void WritePathTables(byte[] iso)
    {
        var little = new Span<byte>(iso, PathTableLeLba * IsoSectorSize, IsoSectorSize);
        little[0] = 1;
        little[1] = 0;
        BinaryPrimitives.WriteUInt32LittleEndian(little.Slice(2, 4), RootDirectoryLba);
        BinaryPrimitives.WriteUInt16LittleEndian(little.Slice(6, 2), 1);
        little[8] = 0;
        little[9] = 0;

        var big = new Span<byte>(iso, PathTableBeLba * IsoSectorSize, IsoSectorSize);
        big[0] = 1;
        big[1] = 0;
        BinaryPrimitives.WriteUInt32BigEndian(big.Slice(2, 4), RootDirectoryLba);
        BinaryPrimitives.WriteUInt16BigEndian(big.Slice(6, 2), 1);
        big[8] = 0;
        big[9] = 0;
    }

    private static void WriteRootDirectory(byte[] iso, int floppyImageLength, DateTime timestamp)
    {
        var sector = new Span<byte>(iso, RootDirectoryLba * IsoSectorSize, IsoSectorSize);
        var offset = 0;

        offset += CopyRecord(sector, offset, BuildDirectoryRecord(RootDirectoryLba, IsoSectorSize, timestamp, 0x02, [0x00]));
        offset += CopyRecord(sector, offset, BuildDirectoryRecord(RootDirectoryLba, IsoSectorSize, timestamp, 0x02, [0x01]));
        offset += CopyRecord(sector, offset, BuildDirectoryRecord(BootCatalogLba, IsoSectorSize, timestamp, 0x00, Encoding.ASCII.GetBytes("BOOT.CAT;1")));
        CopyRecord(sector, offset, BuildDirectoryRecord(BootImageLba, checked((uint)floppyImageLength), timestamp, 0x00, Encoding.ASCII.GetBytes("BOOTIMG.IMG;1")));
    }

    private static int CopyRecord(Span<byte> destination, int offset, byte[] record)
    {
        record.CopyTo(destination.Slice(offset, record.Length));
        return record.Length;
    }

    private static void WriteBootCatalog(byte[] iso)
    {
        var sector = new Span<byte>(iso, BootCatalogLba * IsoSectorSize, IsoSectorSize);
        sector[0] = 0x01;
        sector[1] = 0x00;
        WritePaddedAscii(sector, 4, 24, "DRAGON OS");
        sector[30] = 0x55;
        sector[31] = 0xAA;

        var checksum = ComputeElToritoChecksum(sector.Slice(0, 32));
        BinaryPrimitives.WriteUInt16LittleEndian(sector.Slice(28, 2), checksum);

        sector[32] = 0x88;
        sector[33] = 0x02;
        BinaryPrimitives.WriteUInt16LittleEndian(sector.Slice(34, 2), 0x0000);
        sector[36] = 0x00;
        sector[37] = 0x00;
        BinaryPrimitives.WriteUInt16LittleEndian(sector.Slice(38, 2), 0x0001);
        BinaryPrimitives.WriteUInt32LittleEndian(sector.Slice(40, 4), BootImageLba);
    }

    private static ushort ComputeElToritoChecksum(Span<byte> validationEntry)
    {
        uint sum = 0;
        for (var i = 0; i < 16; i++)
        {
            if (i == 14)
            {
                continue;
            }

            sum += BinaryPrimitives.ReadUInt16LittleEndian(validationEntry.Slice(i * 2, 2));
        }

        return unchecked((ushort)(0u - sum));
    }

    private static byte[] BuildDirectoryRecord(uint extentLba, uint dataLength, DateTime recordingTime, byte flags, ReadOnlySpan<byte> fileIdentifier)
    {
        var padding = (fileIdentifier.Length % 2 == 0) ? 1 : 0;
        var recordLength = 33 + fileIdentifier.Length + padding;
        var record = new byte[recordLength];
        var span = record.AsSpan();

        span[0] = checked((byte)recordLength);
        span[1] = 0x00;
        WriteBothEndianUInt32(span, 2, extentLba);
        WriteBothEndianUInt32(span, 10, dataLength);
        WriteDirectoryTimestamp(span.Slice(18, 7), recordingTime);
        span[25] = flags;
        span[26] = 0x00;
        span[27] = 0x00;
        WriteBothEndianUInt16(span, 28, 1);
        span[32] = checked((byte)fileIdentifier.Length);
        fileIdentifier.CopyTo(span.Slice(33, fileIdentifier.Length));
        return record;
    }

    private static void WriteStandardIdentifier(Span<byte> destination)
    {
        destination[1] = (byte)'C';
        destination[2] = (byte)'D';
        destination[3] = (byte)'0';
        destination[4] = (byte)'0';
        destination[5] = (byte)'1';
    }

    private static void WritePaddedAscii(Span<byte> destination, int offset, int length, string value)
    {
        destination.Slice(offset, length).Fill((byte)' ');
        var bytes = Encoding.ASCII.GetBytes(value);
        bytes.AsSpan(0, Math.Min(bytes.Length, length)).CopyTo(destination.Slice(offset, length));
    }

    private static void WriteBothEndianUInt16(Span<byte> destination, int offset, ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(destination.Slice(offset, 2), value);
        BinaryPrimitives.WriteUInt16BigEndian(destination.Slice(offset + 2, 2), value);
    }

    private static void WriteBothEndianUInt32(Span<byte> destination, int offset, uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(destination.Slice(offset, 4), value);
        BinaryPrimitives.WriteUInt32BigEndian(destination.Slice(offset + 4, 4), value);
    }

    private static void WriteDirectoryTimestamp(Span<byte> destination, DateTime timestamp)
    {
        destination[0] = checked((byte)(timestamp.Year - 1900));
        destination[1] = checked((byte)timestamp.Month);
        destination[2] = checked((byte)timestamp.Day);
        destination[3] = checked((byte)timestamp.Hour);
        destination[4] = checked((byte)timestamp.Minute);
        destination[5] = checked((byte)timestamp.Second);
        destination[6] = 0;
    }

    private static void WriteVolumeTimestamp(Span<byte> destination, DateTime timestamp)
    {
        var text = timestamp.ToString("yyyyMMddHHmmss00");
        Encoding.ASCII.GetBytes(text).CopyTo(destination);
        destination[16] = 0;
    }
}

internal static class BootCodeBuilder
{
    public static byte[] BuildStage1(int stage2SectorCount)
    {
        var asm = new Assembler16(0x7C00);

        asm.MarkLabel("start");
        asm.Cli();
        asm.XorRegWithSelf(Reg16.AX);
        asm.MovSegmentFromReg(SegmentReg.DS, Reg16.AX);
        asm.MovSegmentFromReg(SegmentReg.ES, Reg16.AX);
        asm.MovSegmentFromReg(SegmentReg.SS, Reg16.AX);
        asm.MovRegImm16(Reg16.SP, 0x7C00);
        asm.Cld();
        asm.Sti();
        asm.MovMem8AbsoluteFromReg8("boot_drive", Reg8.DL);
        asm.MovRegImm8(Reg8.AH, 0x02);
        asm.MovRegImm8(Reg8.AL, checked((byte)stage2SectorCount));
        asm.MovRegImm16(Reg16.BX, Stage2LoadOffset);
        asm.MovRegImm8(Reg8.CH, 0x00);
        asm.MovRegImm8(Reg8.CL, 0x02);
        asm.MovRegImm8(Reg8.DH, 0x00);
        asm.MovReg8FromMem8Absolute(Reg8.DL, "boot_drive");
        asm.Int(0x13);
        asm.JcShort("disk_error");
        asm.JmpFar(0x0000, Stage2LoadOffset);

        asm.MarkLabel("disk_error");
        asm.MovRegImm16Label(Reg16.SI, "disk_error_msg");
        asm.CallNear("print_string");
        asm.JmpShort("hang");

        asm.MarkLabel("print_string");
        asm.Lodsb();
        asm.TestReg8WithSelf(Reg8.AL);
        asm.JzShort("print_done");
        asm.MovRegImm8(Reg8.AH, 0x0E);
        asm.MovRegImm16(Reg16.BX, 0x0007);
        asm.Int(0x10);
        asm.JmpShort("print_string");

        asm.MarkLabel("print_done");
        asm.Ret();

        asm.MarkLabel("hang");
        asm.Hlt();
        asm.JmpShort("hang");

        asm.MarkLabel("boot_drive");
        asm.Db(0x00);

        asm.MarkLabel("disk_error_msg");
        asm.AsciiZ("Stage 2 load failed");

        asm.PadToSize(510);
        asm.Dw(0xAA55);

        return asm.ToArray(expectedLength: 512);
    }

    public static byte[] BuildStage2(int imageSectorCount, byte[] imageSectorTable)
    {
        var asm = new Assembler16(Stage2LoadOffset);

        asm.MarkLabel("start");
        asm.Cli();
        asm.XorRegWithSelf(Reg16.AX);
        asm.MovSegmentFromReg(SegmentReg.DS, Reg16.AX);
        asm.MovSegmentFromReg(SegmentReg.ES, Reg16.AX);
        asm.MovSegmentFromReg(SegmentReg.SS, Reg16.AX);
        asm.MovRegImm16(Reg16.SP, 0x7A00);
        asm.Cld();
        asm.Sti();
        asm.MovMem8AbsoluteFromReg8("boot_drive", Reg8.DL);
        asm.MovRegImm16(Reg16.AX, ImageBufferSegment);
        asm.MovSegmentFromReg(SegmentReg.ES, Reg16.AX);
        asm.XorRegWithSelf(Reg16.BX);
        asm.MovRegImm16Label(Reg16.SI, "sector_table");
        asm.MovRegImm16(Reg16.CX, checked((ushort)imageSectorCount));

        asm.MarkLabel("read_loop");
        asm.PushReg(Reg16.CX);
        asm.MovRegImm8(Reg8.AH, 0x02);
        asm.MovRegImm8(Reg8.AL, 0x01);
        asm.MovReg8FromSiOffset(Reg8.CH, 0);
        asm.MovReg8FromSiOffset(Reg8.DH, 1);
        asm.MovReg8FromSiOffset(Reg8.CL, 2);
        asm.MovReg8FromMem8Absolute(Reg8.DL, "boot_drive");
        asm.Int(0x13);
        asm.JcShort("disk_error");
        asm.AddRegImm16(Reg16.SI, 3);
        asm.AddRegImm16(Reg16.BX, 512);
        asm.PopReg(Reg16.CX);
        asm.LoopShort("read_loop");

        asm.MovRegImm16(Reg16.AX, 0x0013);
        asm.Int(0x10);

        asm.MovRegImm16(Reg16.AX, ImageBufferSegment);
        asm.MovSegmentFromReg(SegmentReg.DS, Reg16.AX);
        asm.XorRegWithSelf(Reg16.SI);
        asm.MovRegImm16(Reg16.DX, 0x03C8);
        asm.MovRegImm8(Reg8.AL, 0x00);
        asm.OutDxAl();
        asm.IncReg(Reg16.DX);
        asm.MovRegImm16(Reg16.CX, 768);

        asm.MarkLabel("palette_loop");
        asm.Lodsb();
        asm.OutDxAl();
        asm.LoopShort("palette_loop");

        asm.MovRegImm16(Reg16.SI, 0x0300);
        asm.MovRegImm16(Reg16.AX, 0xA000);
        asm.MovSegmentFromReg(SegmentReg.ES, Reg16.AX);
        asm.XorRegWithSelf(Reg16.DI);
        asm.MovRegImm16(Reg16.CX, 32_000);
        asm.RepMovsw();

        asm.MarkLabel("halt");
        asm.Hlt();
        asm.JmpShort("halt");

        asm.MarkLabel("disk_error");
        asm.XorRegWithSelf(Reg16.AX);
        asm.MovSegmentFromReg(SegmentReg.DS, Reg16.AX);
        asm.MovRegImm16Label(Reg16.SI, "disk_error_msg");
        asm.CallNear("print_string");
        asm.JmpShort("halt");

        asm.MarkLabel("print_string");
        asm.Lodsb();
        asm.TestReg8WithSelf(Reg8.AL);
        asm.JzShort("print_done");
        asm.MovRegImm8(Reg8.AH, 0x0E);
        asm.MovRegImm16(Reg16.BX, 0x0007);
        asm.Int(0x10);
        asm.JmpShort("print_string");

        asm.MarkLabel("print_done");
        asm.Ret();

        asm.MarkLabel("boot_drive");
        asm.Db(0x00);

        asm.MarkLabel("disk_error_msg");
        asm.AsciiZ("Image load failed");

        asm.MarkLabel("sector_table");
        asm.Db(imageSectorTable);

        return asm.ToArray();
    }

    private const ushort Stage2LoadOffset = 0x8000;
    private const ushort ImageBufferSegment = 0x2000;
}

internal static class ImageTools
{
    public static BmpImage ResizeNearest(BmpImage source, int targetWidth, int targetHeight)
    {
        if (source.Width == targetWidth && source.Height == targetHeight)
        {
            return source;
        }

        var pixels = new Rgb24[targetWidth * targetHeight];
        byte[]? alpha = source.Alpha is null ? null : new byte[targetWidth * targetHeight];
        for (var y = 0; y < targetHeight; y++)
        {
            var sourceY = y * source.Height / targetHeight;
            for (var x = 0; x < targetWidth; x++)
            {
                var sourceX = x * source.Width / targetWidth;
                var sourceIndex = (sourceY * source.Width) + sourceX;
                var targetIndex = (y * targetWidth) + x;
                pixels[targetIndex] = source.Pixels[sourceIndex];
                if (alpha is not null)
                {
                    alpha[targetIndex] = source.Alpha![sourceIndex];
                }
            }
        }

        return new BmpImage(targetWidth, targetHeight, pixels, alpha);
    }

    public static byte[] PackMode13Asset(BmpImage image)
    {
        if (image.Width != TargetWidth || image.Height != TargetHeight)
        {
            throw new InvalidOperationException($"Expected a {TargetWidth}x{TargetHeight} image after resizing.");
        }

        var palette = BuildVga332Palette();
        var output = new byte[(256 * 3) + (TargetWidth * TargetHeight)];
        Buffer.BlockCopy(palette, 0, output, 0, palette.Length);

        var pixelBase = palette.Length;
        for (var i = 0; i < image.Pixels.Length; i++)
        {
            var pixel = image.Pixels[i];
            output[pixelBase + i] = QuantizeTo332(pixel);
        }

        return output;
    }

    private static byte[] BuildVga332Palette()
    {
        var palette = new byte[256 * 3];
        for (var index = 0; index < 256; index++)
        {
            var red = (index >> 5) & 0x07;
            var green = (index >> 2) & 0x07;
            var blue = index & 0x03;

            palette[(index * 3) + 0] = ScaleToVga(red, 7);
            palette[(index * 3) + 1] = ScaleToVga(green, 7);
            palette[(index * 3) + 2] = ScaleToVga(blue, 3);
        }

        return palette;
    }

    private static byte QuantizeTo332(Rgb24 pixel)
    {
        var red = pixel.R >> 5;
        var green = pixel.G >> 5;
        var blue = pixel.B >> 6;
        return (byte)((red << 5) | (green << 2) | blue);
    }

    private static byte ScaleToVga(int value, int maxValue) =>
        (byte)((value * 63) / maxValue);

    private const int TargetWidth = 320;
    private const int TargetHeight = 200;
}

internal static class SampleBitmapWriter
{
    public static void Write(string path, int width, int height)
    {
        var pixels = new Rgb24[width * height];
        PaintBackground(pixels, width, height);
        PaintSun(pixels, width, height, 248, 44, 28);
        PaintMountains(pixels, width, height);
        PaintGround(pixels, width, height);
        PaintScanlines(pixels, width, height);
        PaintBanner(pixels, width, height);
        Write24BitBmp(path, width, height, pixels);
    }

    private static void PaintBackground(Rgb24[] pixels, int width, int height)
    {
        for (var y = 0; y < height; y++)
        {
            var t = y / (float)(height - 1);
            var top = new Rgb24(14, 30, 52);
            var bottom = new Rgb24(255, 140, 60);
            var color = Lerp(top, bottom, t);
            for (var x = 0; x < width; x++)
            {
                pixels[(y * width) + x] = color;
            }
        }
    }

    private static void PaintSun(Rgb24[] pixels, int width, int height, int centerX, int centerY, int radius)
    {
        for (var y = -radius; y <= radius; y++)
        {
            for (var x = -radius; x <= radius; x++)
            {
                if ((x * x) + (y * y) > radius * radius)
                {
                    continue;
                }

                var px = centerX + x;
                var py = centerY + y;
                if (px < 0 || py < 0 || px >= width || py >= height)
                {
                    continue;
                }

                var distance = MathF.Sqrt((x * x) + (y * y)) / radius;
                var color = Lerp(new Rgb24(255, 255, 180), new Rgb24(255, 90, 40), distance);
                pixels[(py * width) + px] = color;
            }
        }
    }

    private static void PaintMountains(Rgb24[] pixels, int width, int height)
    {
        DrawMountain(pixels, width, height, 50, 180, 200, 82, new Rgb24(30, 20, 40));
        DrawMountain(pixels, width, height, 190, 180, 210, 96, new Rgb24(52, 28, 58));
        DrawMountain(pixels, width, height, 275, 180, 140, 72, new Rgb24(38, 22, 48));
    }

    private static void PaintGround(Rgb24[] pixels, int width, int height)
    {
        for (var y = 150; y < height; y++)
        {
            var stripe = ((y - 150) % 8) < 2;
            var color = stripe ? new Rgb24(255, 110, 70) : new Rgb24(60, 18, 40);
            for (var x = 0; x < width; x++)
            {
                var influence = MathF.Abs((x - (width / 2f)) / (width / 2f));
                pixels[(y * width) + x] = Lerp(color, new Rgb24(16, 12, 32), influence * 0.6f);
            }
        }
    }

    private static void PaintScanlines(Rgb24[] pixels, int width, int height)
    {
        for (var y = 0; y < height; y += 4)
        {
            for (var x = 0; x < width; x++)
            {
                var index = (y * width) + x;
                var pixel = pixels[index];
                pixels[index] = new Rgb24(
                    (byte)(pixel.R * 0.82f),
                    (byte)(pixel.G * 0.82f),
                    (byte)(pixel.B * 0.82f));
            }
        }
    }

    private static void PaintBanner(Rgb24[] pixels, int width, int height)
    {
        FillRect(pixels, width, 16, 18, 164, 24, new Rgb24(18, 18, 32));
        DrawText(pixels, width, "BOOT IMAGE", 24, 26, new Rgb24(255, 220, 120));
        DrawText(pixels, width, "DRAGON OS", 92, 76, new Rgb24(240, 245, 255), scale: 2);
    }

    private static void DrawMountain(Rgb24[] pixels, int width, int height, int centerX, int baseY, int baseWidth, int peakHeight, Rgb24 color)
    {
        for (var y = 0; y < peakHeight; y++)
        {
            var rowWidth = (int)(baseWidth * (y / (float)peakHeight));
            var left = Math.Max(0, centerX - rowWidth);
            var right = Math.Min(width - 1, centerX + rowWidth);
            var py = baseY - peakHeight + y;
            if (py < 0 || py >= height)
            {
                continue;
            }

            for (var x = left; x <= right; x++)
            {
                var shade = 0.7f + ((x - left) / (float)Math.Max(1, right - left)) * 0.3f;
                pixels[(py * width) + x] = new Rgb24(
                    (byte)(color.R * shade),
                    (byte)(color.G * shade),
                    (byte)(color.B * shade));
            }
        }
    }

    private static void FillRect(Rgb24[] pixels, int width, int x, int y, int rectWidth, int rectHeight, Rgb24 color)
    {
        for (var py = y; py < y + rectHeight; py++)
        {
            for (var px = x; px < x + rectWidth; px++)
            {
                pixels[(py * width) + px] = color;
            }
        }
    }

    private static void DrawText(Rgb24[] pixels, int width, string text, int x, int y, Rgb24 color, int scale = 1)
    {
        var cursorX = x;
        foreach (var ch in text)
        {
            if (!Font5x7.TryGetValue(ch, out var rows))
            {
                cursorX += 6 * scale;
                continue;
            }

            for (var row = 0; row < rows.Length; row++)
            {
                for (var col = 0; col < 5; col++)
                {
                    if (((rows[row] >> (4 - col)) & 1) == 0)
                    {
                        continue;
                    }

                    for (var sy = 0; sy < scale; sy++)
                    {
                        for (var sx = 0; sx < scale; sx++)
                        {
                            var px = cursorX + (col * scale) + sx;
                            var py = y + (row * scale) + sy;
                            pixels[(py * width) + px] = color;
                        }
                    }
                }
            }

            cursorX += 6 * scale;
        }
    }

    private static void Write24BitBmp(string path, int width, int height, Rgb24[] pixels)
    {
        var rowStride = ((width * 3) + 3) & ~3;
        var pixelDataSize = rowStride * height;
        var fileSize = 14 + 40 + pixelDataSize;
        var bytes = new byte[fileSize];

        bytes[0] = (byte)'B';
        bytes[1] = (byte)'M';
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(2), fileSize);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(10), 54);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(14), 40);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(18), width);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(22), height);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(26), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(28), 24);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(34), pixelDataSize);

        var cursor = 54;
        for (var y = height - 1; y >= 0; y--)
        {
            for (var x = 0; x < width; x++)
            {
                var pixel = pixels[(y * width) + x];
                bytes[cursor++] = pixel.B;
                bytes[cursor++] = pixel.G;
                bytes[cursor++] = pixel.R;
            }

            while ((cursor - 54) % rowStride != 0)
            {
                bytes[cursor++] = 0;
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
    }

    private static Rgb24 Lerp(Rgb24 a, Rgb24 b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new Rgb24(
            (byte)(a.R + ((b.R - a.R) * t)),
            (byte)(a.G + ((b.G - a.G) * t)),
            (byte)(a.B + ((b.B - a.B) * t)));
    }

    private static readonly Dictionary<char, byte[]> Font5x7 = new()
    {
        ['A'] = [0b01110, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001, 0b10001],
        ['B'] = [0b11110, 0b10001, 0b10001, 0b11110, 0b10001, 0b10001, 0b11110],
        ['E'] = [0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b10000, 0b11111],
        ['G'] = [0b01110, 0b10001, 0b10000, 0b10111, 0b10001, 0b10001, 0b01110],
        ['I'] = [0b11111, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b11111],
        ['M'] = [0b10001, 0b11011, 0b10101, 0b10001, 0b10001, 0b10001, 0b10001],
        ['O'] = [0b01110, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110],
        ['P'] = [0b11110, 0b10001, 0b10001, 0b11110, 0b10000, 0b10000, 0b10000],
        ['S'] = [0b01111, 0b10000, 0b10000, 0b01110, 0b00001, 0b00001, 0b11110],
        ['T'] = [0b11111, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100],
        [' '] = [0, 0, 0, 0, 0, 0, 0]
    };
}

internal static class DefaultCursorBuilder
{
    public static BmpImage Create()
    {
        const int width = 21;
        const int height = 32;

        var transparent = new Rgb24(71, 112, 76);
        var outline = new Rgb24(0, 0, 0);
        var fill = new Rgb24(255, 255, 255);
        var pixels = Enumerable.Repeat(transparent, width * height).ToArray();
        var alpha = new byte[width * height];

        var shape = new[]
        {
            "X....................",
            "XX...................",
            "XOX..................",
            "XOOX.................",
            "XOOOX................",
            "XOOOOX...............",
            "XOOOOOX..............",
            "XOOOOOOX.............",
            "XOOOOOOOX............",
            "XOOOOOOOOX...........",
            "XOOOOOOOOOX..........",
            "XOOOOOOOOOOX.........",
            "XOOOOOOOOOOOX........",
            "XOOOOOOOOOOOOX.......",
            "XOOOOOOOOOOOOOX......",
            "XOOOOOOOOOOOOOOX.....",
            "XOOOOOOOOOOOOOOOX....",
            "XOOOOOOOOOOOOOOOOX...",
            "XOOOOOOOOOOOOOOOOOX..",
            "XOOOOOOOOOOOOOOOOOOX.",
            "XOOOOOOOOOOOOOOOOOOOX",
            "XOOOOX...............",
            "XOOXOX...............",
            "XOX.XOX..............",
            "XX..XOX..............",
            "X...XOOX.............",
            "....XOOX.............",
            "....XOOOX............",
            ".....XOOOX...........",
            ".....XOOOX...........",
            "......XOOX...........",
            "......XXXX..........."
        };

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var ch = shape[y][x];
                pixels[(y * width) + x] = ch switch
                {
                    'X' => outline,
                    'O' => fill,
                    _ => transparent
                };
                alpha[(y * width) + x] = ch == '.' ? (byte)0 : (byte)255;
            }
        }

        return new BmpImage(width, height, pixels, alpha);
    }
}

internal static class ImageAssetLoader
{
    public static BmpImage Load(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
            ? LoadPng(path)
            : BmpLoader.Load(path);
    }

    private static BmpImage LoadPng(string path)
    {
        using var bitmap = new Bitmap(path);
        var pixels = new Rgb24[bitmap.Width * bitmap.Height];
        var alpha = new byte[pixels.Length];

        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var index = (y * bitmap.Width) + x;
                var pixel = bitmap.GetPixel(x, y);
                pixels[index] = new Rgb24(pixel.R, pixel.G, pixel.B);
                alpha[index] = pixel.A;
            }
        }

        return new BmpImage(bitmap.Width, bitmap.Height, pixels, alpha);
    }
}

internal static class DragonRuntimeBuilder
{
    private static readonly string[] SupportedExtensions = [".dll", ".png", ".mp4"];

    public static IReadOnlyList<RuntimePackageFile> Build(string projectRoot, string assetsDir, string buildDir)
    {
        var packageFiles = new List<RuntimePackageFile>();
        var manifestEntries = new List<string>();
        var virtualPathRegistry = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var sourceDirectories = new[]
        {
            ("dll", projectRoot),
            ("dll", Path.Combine(projectRoot, "modules")),
            ("dll", Path.Combine(projectRoot, "plugins")),
            ("dll", Path.Combine(projectRoot, "dlls")),
            ("png", assetsDir),
            ("mp4", assetsDir)
        };

        foreach (var (kind, sourceDirectory) in sourceDirectories)
        {
            if (!Directory.Exists(sourceDirectory))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(sourceDirectory))
            {
                var extension = Path.GetExtension(file);
                if (!SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!extension.Equals("." + kind, StringComparison.OrdinalIgnoreCase) &&
                    !(kind == "png" && extension.Equals(".png", StringComparison.OrdinalIgnoreCase)) &&
                    !(kind == "mp4" && extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase)) &&
                    !(kind == "dll" && extension.Equals(".dll", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var sourceInfo = new FileInfo(file);
                var virtualPath = BuildRuntimeVirtualPath(kind.ToUpperInvariant(), file, sourceDirectory, virtualPathRegistry);
                var relativeSource = Path.GetRelativePath(projectRoot, file);
                var bytes = File.ReadAllBytes(file);
                var summaryText = extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase)
                    ? Mp4MetadataParser.TryParse(file, out var metadata)
                        ? $"metadata {metadata.Width}x{metadata.Height} {metadata.DurationSeconds:F2}s"
                        : "metadata unavailable"
                    : "";
                var includeInEfiPartition = false;
                packageFiles.Add(new RuntimePackageFile(virtualPath, bytes, relativeSource, kind.ToUpperInvariant(), includeInEfiPartition, summaryText));
                manifestEntries.Add($"{kind.ToUpperInvariant()}|{virtualPath}|{relativeSource}|{bytes.Length}|{sourceInfo.LastWriteTimeUtc:O}|{(includeInEfiPartition ? "EFI+ISO" : "ISO")}|{summaryText}");
            }
        }

        var dragonFilesDirectory = Path.Combine(projectRoot, "dragon_files");
        if (Directory.Exists(dragonFilesDirectory))
        {
            foreach (var file in Directory.EnumerateFiles(dragonFilesDirectory, "*", SearchOption.AllDirectories))
            {
                var sourceInfo = new FileInfo(file);
                var relativeDragonPath = Path.GetRelativePath(dragonFilesDirectory, file)
                    .Replace(Path.DirectorySeparatorChar, '\\')
                    .Replace(Path.AltDirectorySeparatorChar, '\\');
                var virtualPath = $@"DRAGON\FILES\{relativeDragonPath}";
                virtualPathRegistry.Add(virtualPath);
                var relativeSource = Path.GetRelativePath(projectRoot, file);
                var bytes = File.ReadAllBytes(file);
                packageFiles.Add(new RuntimePackageFile(virtualPath, bytes, relativeSource, "FILE", false, "dragon_files payload"));
                manifestEntries.Add($"FILE|{virtualPath}|{relativeSource}|{bytes.Length}|{sourceInfo.LastWriteTimeUtc:O}|ISO|dragon_files payload");
            }
        }

        var manifestLines = new List<string>
        {
            "DRAGON_RUNTIME_V1",
            $"GeneratedUtc|{DateTime.UtcNow:O}",
            $"FileCount|{packageFiles.Count}"
        };
        manifestLines.AddRange(manifestEntries.OrderBy(line => line, StringComparer.OrdinalIgnoreCase));

        var manifestBytes = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, manifestLines) + Environment.NewLine);
        var manifestPath = Path.Combine(buildDir, "DRAGON_MANIFEST.TXT");
        File.WriteAllBytes(manifestPath, manifestBytes);
        packageFiles.Add(new RuntimePackageFile(@"DRAGON\MANIFEST.TXT", manifestBytes, Path.GetRelativePath(projectRoot, manifestPath), "MANIFEST", true, "runtime manifest"));

        return packageFiles;
    }

    private static string BuildRuntimeVirtualPath(string kind, string file, string sourceDirectory, HashSet<string> virtualPathRegistry)
    {
        var originalName = Path.GetFileName(file);
        var candidate = $@"DRAGON\{kind}\{originalName}";
        if (virtualPathRegistry.Add(candidate))
        {
            return candidate;
        }

        var sourceFolderName = new DirectoryInfo(sourceDirectory).Name;
        if (!string.IsNullOrWhiteSpace(sourceFolderName))
        {
            candidate = $@"DRAGON\{kind}\{sourceFolderName}\{originalName}";
            if (virtualPathRegistry.Add(candidate))
            {
                return candidate;
            }
        }

        for (var suffix = 2; ; suffix++)
        {
            candidate = $@"DRAGON\{kind}\{Path.GetFileNameWithoutExtension(originalName)}_{suffix}{Path.GetExtension(originalName)}";
            if (virtualPathRegistry.Add(candidate))
            {
                return candidate;
            }
        }
    }
}

internal static class Mp4MetadataParser
{
    public static bool TryParse(string path, out Mp4Metadata metadata)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);

        var durationSeconds = 0d;
        var width = 0;
        var height = 0;
        ParseBoxRange(reader, stream.Length, ref durationSeconds, ref width, ref height, false);

        metadata = new Mp4Metadata(width, height, durationSeconds);
        return width > 0 && height > 0 && durationSeconds > 0;
    }

    private static void ParseBoxRange(BinaryReader reader, long endPosition, ref double durationSeconds, ref int width, ref int height, bool videoTrack)
    {
        while (reader.BaseStream.Position + 8 <= endPosition)
        {
            var boxStart = reader.BaseStream.Position;
            var size = ReadUInt32BigEndian(reader);
            var type = new string(reader.ReadChars(4));
            long payloadSize = size switch
            {
                0 => endPosition - boxStart,
                1 => checked((long)ReadUInt64BigEndian(reader)),
                _ => size
            };

            if (payloadSize < 8)
            {
                break;
            }

            var headerSize = reader.BaseStream.Position - boxStart;
            var boxEnd = boxStart + payloadSize;
            if (boxEnd > endPosition || boxEnd > reader.BaseStream.Length)
            {
                break;
            }

            switch (type)
            {
                case "moov":
                case "mdia":
                    ParseBoxRange(reader, boxEnd, ref durationSeconds, ref width, ref height, videoTrack);
                    break;
                case "trak":
                {
                    var localDuration = durationSeconds;
                    var localWidth = width;
                    var localHeight = height;
                    var isVideo = false;
                    ParseTrack(reader, boxEnd, ref localDuration, ref localWidth, ref localHeight, ref isVideo);
                    if (isVideo)
                    {
                        durationSeconds = localDuration;
                        width = localWidth;
                        height = localHeight;
                    }

                    break;
                }
                case "mvhd":
                    durationSeconds = ParseMovieHeader(reader);
                    break;
                default:
                    reader.BaseStream.Position = boxEnd;
                    break;
            }

            reader.BaseStream.Position = boxEnd;
        }
    }

    private static void ParseTrack(BinaryReader reader, long trackEnd, ref double durationSeconds, ref int width, ref int height, ref bool isVideo)
    {
        while (reader.BaseStream.Position + 8 <= trackEnd)
        {
            var boxStart = reader.BaseStream.Position;
            var size = ReadUInt32BigEndian(reader);
            var type = new string(reader.ReadChars(4));
            long payloadSize = size switch
            {
                0 => trackEnd - boxStart,
                1 => checked((long)ReadUInt64BigEndian(reader)),
                _ => size
            };

            if (payloadSize < 8)
            {
                break;
            }

            var boxEnd = boxStart + payloadSize;
            if (boxEnd > trackEnd || boxEnd > reader.BaseStream.Length)
            {
                break;
            }

            switch (type)
            {
                case "mdia":
                    ParseMedia(reader, boxEnd, ref isVideo);
                    break;
                case "tkhd":
                    ParseTrackHeader(reader, ref width, ref height);
                    break;
            }

            reader.BaseStream.Position = boxEnd;
        }
    }

    private static void ParseMedia(BinaryReader reader, long mediaEnd, ref bool isVideo)
    {
        while (reader.BaseStream.Position + 8 <= mediaEnd)
        {
            var boxStart = reader.BaseStream.Position;
            var size = ReadUInt32BigEndian(reader);
            var type = new string(reader.ReadChars(4));
            long payloadSize = size switch
            {
                0 => mediaEnd - boxStart,
                1 => checked((long)ReadUInt64BigEndian(reader)),
                _ => size
            };

            if (payloadSize < 8)
            {
                break;
            }

            var boxEnd = boxStart + payloadSize;
            if (boxEnd > mediaEnd || boxEnd > reader.BaseStream.Length)
            {
                break;
            }

            if (type == "hdlr")
            {
                reader.BaseStream.Position += 8;
                var handlerType = new string(reader.ReadChars(4));
                isVideo = handlerType == "vide";
            }

            reader.BaseStream.Position = boxEnd;
        }
    }

    private static double ParseMovieHeader(BinaryReader reader)
    {
        var version = reader.ReadByte();
        reader.ReadBytes(3);

        if (version == 1)
        {
            reader.ReadBytes(8 + 8);
            var timescale = ReadUInt32BigEndian(reader);
            var duration = ReadUInt64BigEndian(reader);
            return timescale == 0 ? 0d : duration / (double)timescale;
        }

        reader.ReadBytes(4 + 4);
        var timeScale32 = ReadUInt32BigEndian(reader);
        var duration32 = ReadUInt32BigEndian(reader);
        return timeScale32 == 0 ? 0d : duration32 / (double)timeScale32;
    }

    private static void ParseTrackHeader(BinaryReader reader, ref int width, ref int height)
    {
        var version = reader.ReadByte();
        reader.ReadBytes(3);

        if (version == 1)
        {
            reader.ReadBytes(8 + 8 + 4 + 4 + 8 + 8 + 2 + 2 + 2 + 2 + 36);
        }
        else
        {
            reader.ReadBytes(4 + 4 + 4 + 4 + 4 + 8 + 2 + 2 + 2 + 2 + 36);
        }

        width = (int)(ReadUInt32BigEndian(reader) >> 16);
        height = (int)(ReadUInt32BigEndian(reader) >> 16);
    }

    private static uint ReadUInt32BigEndian(BinaryReader reader)
    {
        Span<byte> bytes = stackalloc byte[4];
        reader.Read(bytes);
        return BinaryPrimitives.ReadUInt32BigEndian(bytes);
    }

    private static ulong ReadUInt64BigEndian(BinaryReader reader)
    {
        Span<byte> bytes = stackalloc byte[8];
        reader.Read(bytes);
        return BinaryPrimitives.ReadUInt64BigEndian(bytes);
    }
}

internal static class BmpLoader
{
    public static BmpImage Load(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < 54 || bytes[0] != 'B' || bytes[1] != 'M')
        {
            throw new InvalidOperationException("The source file is not a valid BMP.");
        }

        var dataOffset = ReadInt32(bytes, 10);
        var dibSize = ReadInt32(bytes, 14);
        if (dibSize < 40)
        {
            throw new InvalidOperationException("Only BITMAPINFOHEADER-style BMP files are supported.");
        }

        var width = ReadInt32(bytes, 18);
        var rawHeight = ReadInt32(bytes, 22);
        var planes = ReadUInt16(bytes, 26);
        var bitsPerPixel = ReadUInt16(bytes, 28);
        var compression = ReadInt32(bytes, 30);

        if (width <= 0 || rawHeight == 0)
        {
            throw new InvalidOperationException("Unsupported BMP dimensions.");
        }

        if (planes != 1)
        {
            throw new InvalidOperationException("Unsupported BMP plane count.");
        }

        if (compression != 0 && !(compression == 3 && bitsPerPixel == 32))
        {
            throw new InvalidOperationException("Only uncompressed BMP files and 32-bit BI_BITFIELDS BMP files are supported.");
        }

        var topDown = rawHeight < 0;
        var height = Math.Abs(rawHeight);
        var pixels = new Rgb24[width * height];
        byte[]? alpha = bitsPerPixel == 32 ? new byte[width * height] : null;
        var rowStride = ((width * bitsPerPixel) + 31) / 32 * 4;
        var redMask = 0u;
        var greenMask = 0u;
        var blueMask = 0u;
        var alphaMask = 0u;

        Rgb24[]? palette = null;
        if (bitsPerPixel == 8)
        {
            var paletteCount = ReadInt32(bytes, 46);
            if (paletteCount <= 0)
            {
                paletteCount = 256;
            }

            palette = new Rgb24[paletteCount];
            var paletteOffset = 14 + dibSize;
            for (var i = 0; i < paletteCount; i++)
            {
                var baseOffset = paletteOffset + (i * 4);
                palette[i] = new Rgb24(bytes[baseOffset + 2], bytes[baseOffset + 1], bytes[baseOffset + 0]);
            }
        }
        else if (bitsPerPixel == 32 && compression == 3)
        {
            redMask = ReadUInt32(bytes, 54);
            greenMask = ReadUInt32(bytes, 58);
            blueMask = ReadUInt32(bytes, 62);
            alphaMask = ReadUInt32(bytes, 66);
        }

        for (var y = 0; y < height; y++)
        {
            var sourceY = topDown ? y : (height - 1 - y);
            var rowStart = dataOffset + (sourceY * rowStride);

            for (var x = 0; x < width; x++)
            {
                var targetIndex = (y * width) + x;
                switch (bitsPerPixel)
                {
                    case 24:
                    {
                        var pixelOffset = rowStart + (x * 3);
                        pixels[targetIndex] = new Rgb24(bytes[pixelOffset + 2], bytes[pixelOffset + 1], bytes[pixelOffset + 0]);
                        break;
                    }
                    case 32:
                    {
                        var pixelOffset = rowStart + (x * 4);
                        if (compression == 3)
                        {
                            var packedPixel = ReadUInt32(bytes, pixelOffset);
                            pixels[targetIndex] = new Rgb24(
                                ExtractBitfieldChannel(packedPixel, redMask),
                                ExtractBitfieldChannel(packedPixel, greenMask),
                                ExtractBitfieldChannel(packedPixel, blueMask));
                            if (alpha is not null)
                            {
                                alpha[targetIndex] = alphaMask != 0u
                                    ? ExtractBitfieldChannel(packedPixel, alphaMask)
                                    : (byte)255;
                            }
                        }
                        else
                        {
                            pixels[targetIndex] = new Rgb24(bytes[pixelOffset + 2], bytes[pixelOffset + 1], bytes[pixelOffset + 0]);
                            if (alpha is not null)
                            {
                                alpha[targetIndex] = bytes[pixelOffset + 3];
                            }
                        }

                        break;
                    }
                    case 8:
                    {
                        var paletteIndex = bytes[rowStart + x];
                        if (palette is null || paletteIndex >= palette.Length)
                        {
                            throw new InvalidOperationException("The BMP palette is malformed.");
                        }

                        pixels[targetIndex] = palette[paletteIndex];
                        break;
                    }
                    default:
                        throw new InvalidOperationException($"Unsupported BMP depth: {bitsPerPixel} bits per pixel.");
                }
            }
        }

        return new BmpImage(width, height, pixels, alpha);
    }

    private static int ReadInt32(byte[] data, int offset) =>
        BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset, sizeof(int)));

    private static ushort ReadUInt16(byte[] data, int offset) =>
        BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset, sizeof(ushort)));

    private static uint ReadUInt32(byte[] data, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset, sizeof(uint)));

    private static byte ExtractBitfieldChannel(uint packedPixel, uint mask)
    {
        if (mask == 0u)
        {
            return 0;
        }

        var shift = 0;
        while (((mask >> shift) & 1u) == 0u)
        {
            shift++;
        }

        var value = (packedPixel & mask) >> shift;
        var normalizedMask = mask >> shift;
        var width = 0;
        while ((normalizedMask & 1u) != 0u)
        {
            width++;
            normalizedMask >>= 1;
        }

        var maxValue = (1u << width) - 1u;
        return (byte)((value * 255u + (maxValue / 2u)) / maxValue);
    }
}

internal sealed record RuntimePackageFile(string RelativePath, byte[] Data, string SourcePath, string Kind, bool IncludeInEfiPartition, string SummaryText);
internal readonly record struct Mp4Metadata(int Width, int Height, double DurationSeconds);

internal sealed class Assembler16
{
    private readonly ushort _origin;
    private readonly List<byte> _bytes = [];
    private readonly Dictionary<string, int> _labels = new(StringComparer.Ordinal);
    private readonly List<Fixup> _fixups = [];

    public Assembler16(ushort origin)
    {
        _origin = origin;
    }

    public void MarkLabel(string name) => _labels[name] = CurrentAddress;

    public void Db(byte value) => _bytes.Add(value);

    public void Db(ReadOnlySpan<byte> values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            _bytes.Add(values[i]);
        }
    }

    public void Dw(ushort value)
    {
        _bytes.Add((byte)(value & 0xFF));
        _bytes.Add((byte)(value >> 8));
    }

    public void AsciiZ(string text)
    {
        Db(Encoding.ASCII.GetBytes(text));
        Db(0x00);
    }

    public void PadToSize(int size)
    {
        while (_bytes.Count < size)
        {
            _bytes.Add(0x00);
        }
    }

    public byte[] ToArray(int? expectedLength = null)
    {
        foreach (var fixup in _fixups)
        {
            if (!_labels.TryGetValue(fixup.Label, out var target))
            {
                throw new InvalidOperationException($"Unknown label: {fixup.Label}");
            }

            switch (fixup.Kind)
            {
                case FixupKind.Absolute16:
                {
                    _bytes[fixup.Position + 0] = (byte)(target & 0xFF);
                    _bytes[fixup.Position + 1] = (byte)((target >> 8) & 0xFF);
                    break;
                }
                case FixupKind.Relative8:
                {
                    var displacement = target - (fixup.Position + 1 + _origin);
                    if (displacement < sbyte.MinValue || displacement > sbyte.MaxValue)
                    {
                        throw new InvalidOperationException($"Short jump out of range for label {fixup.Label}.");
                    }

                    _bytes[fixup.Position] = unchecked((byte)(sbyte)displacement);
                    break;
                }
                case FixupKind.Relative16:
                {
                    var displacement = target - (fixup.Position + 2 + _origin);
                    if (displacement < short.MinValue || displacement > short.MaxValue)
                    {
                        throw new InvalidOperationException($"Near call out of range for label {fixup.Label}.");
                    }

                    var value = unchecked((ushort)(short)displacement);
                    _bytes[fixup.Position + 0] = (byte)(value & 0xFF);
                    _bytes[fixup.Position + 1] = (byte)(value >> 8);
                    break;
                }
            }
        }

        if (expectedLength.HasValue && _bytes.Count != expectedLength.Value)
        {
            throw new InvalidOperationException($"Expected {expectedLength.Value} bytes, got {_bytes.Count}.");
        }

        return _bytes.ToArray();
    }

    public void Cli() => Db(0xFA);
    public void Sti() => Db(0xFB);
    public void Cld() => Db(0xFC);
    public void Hlt() => Db(0xF4);
    public void Ret() => Db(0xC3);
    public void Lodsb() => Db(0xAC);
    public void RepMovsw() => Db(stackalloc byte[] { 0xF3, 0xA5 });
    public void OutDxAl() => Db(0xEE);
    public void Int(byte interrupt) => Db(stackalloc byte[] { 0xCD, interrupt });

    public void PushReg(Reg16 reg) => Db((byte)(0x50 + (byte)reg));
    public void PopReg(Reg16 reg) => Db((byte)(0x58 + (byte)reg));
    public void IncReg(Reg16 reg) => Db((byte)(0x40 + (byte)reg));

    public void MovRegImm16(Reg16 reg, ushort value)
    {
        Db((byte)(0xB8 + (byte)reg));
        Dw(value);
    }

    public void MovRegImm16Label(Reg16 reg, string label)
    {
        Db((byte)(0xB8 + (byte)reg));
        AddFixup(FixupKind.Absolute16, label, width: 2);
    }

    public void MovRegImm8(Reg8 reg, byte value)
    {
        Db((byte)(0xB0 + (byte)reg));
        Db(value);
    }

    public void MovSegmentFromReg(SegmentReg segment, Reg16 reg)
    {
        Db(0x8E);
        Db((byte)(0xC0 | ((byte)segment << 3) | (byte)reg));
    }

    public void MovMem8AbsoluteFromReg8(string label, Reg8 reg)
    {
        Db(0x88);
        Db((byte)(((byte)reg << 3) | 0x06));
        AddFixup(FixupKind.Absolute16, label, width: 2);
    }

    public void MovReg8FromMem8Absolute(Reg8 reg, string label)
    {
        Db(0x8A);
        Db((byte)(((byte)reg << 3) | 0x06));
        AddFixup(FixupKind.Absolute16, label, width: 2);
    }

    public void MovReg8FromSiOffset(Reg8 reg, byte offset)
    {
        Db(0x8A);
        if (offset == 0)
        {
            Db((byte)(((byte)reg << 3) | 0x04));
        }
        else
        {
            Db((byte)(0x40 | ((byte)reg << 3) | 0x04));
            Db(offset);
        }
    }

    public void XorRegWithSelf(Reg16 reg)
    {
        Db(0x31);
        Db((byte)(0xC0 | ((byte)reg << 3) | (byte)reg));
    }

    public void TestReg8WithSelf(Reg8 reg)
    {
        Db(0x84);
        Db((byte)(0xC0 | ((byte)reg << 3) | (byte)reg));
    }

    public void AddRegImm16(Reg16 reg, ushort value)
    {
        Db(0x81);
        Db((byte)(0xC0 | (byte)reg));
        Dw(value);
    }

    public void JmpFar(ushort segment, ushort offset)
    {
        Db(0xEA);
        Dw(offset);
        Dw(segment);
    }

    public void CallNear(string label)
    {
        Db(0xE8);
        AddFixup(FixupKind.Relative16, label, width: 2);
    }

    public void JmpShort(string label)
    {
        Db(0xEB);
        AddFixup(FixupKind.Relative8, label, width: 1);
    }

    public void JzShort(string label)
    {
        Db(0x74);
        AddFixup(FixupKind.Relative8, label, width: 1);
    }

    public void JcShort(string label)
    {
        Db(0x72);
        AddFixup(FixupKind.Relative8, label, width: 1);
    }

    public void LoopShort(string label)
    {
        Db(0xE2);
        AddFixup(FixupKind.Relative8, label, width: 1);
    }

    private int CurrentAddress => _origin + _bytes.Count;

    private void AddFixup(FixupKind kind, string label, int width)
    {
        _fixups.Add(new Fixup(_bytes.Count, label, kind));
        for (var i = 0; i < width; i++)
        {
            _bytes.Add(0x00);
        }
    }

    private readonly record struct Fixup(int Position, string Label, FixupKind Kind);

    private enum FixupKind
    {
        Absolute16,
        Relative8,
        Relative16
    }
}

internal readonly record struct BmpImage(int Width, int Height, Rgb24[] Pixels, byte[]? Alpha = null);
internal readonly record struct Rgb24(byte R, byte G, byte B);

internal enum Reg16 : byte
{
    AX = 0,
    CX = 1,
    DX = 2,
    BX = 3,
    SP = 4,
    BP = 5,
    SI = 6,
    DI = 7
}

internal enum Reg8 : byte
{
    AL = 0,
    CL = 1,
    DL = 2,
    BL = 3,
    AH = 4,
    CH = 5,
    DH = 6,
    BH = 7
}

internal enum SegmentReg : byte
{
    ES = 0,
    CS = 1,
    SS = 2,
    DS = 3
}
