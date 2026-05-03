using DragonOSBuilder;
using System.Buffers.Binary;
using System.Text;

namespace BitmapOsBuilder;

internal static class UefiImageTools
{
    public static (byte[] Rgbx, byte[] Bgrx) PackFramebufferVariants(BmpImage image)
    {
        var rgbx = new byte[image.Pixels.Length * 4];
        var bgrx = new byte[image.Pixels.Length * 4];

        for (var i = 0; i < image.Pixels.Length; i++)
        {
            var pixel = image.Pixels[i];
            var baseOffset = i * 4;

            rgbx[baseOffset + 0] = pixel.R;
            rgbx[baseOffset + 1] = pixel.G;
            rgbx[baseOffset + 2] = pixel.B;
            rgbx[baseOffset + 3] = 0;

            bgrx[baseOffset + 0] = pixel.B;
            bgrx[baseOffset + 1] = pixel.G;
            bgrx[baseOffset + 2] = pixel.R;
            bgrx[baseOffset + 3] = 0;
        }

        return (rgbx, bgrx);
    }
}

internal static class UefiAppBuilder
{
    private const int TargetWidth = 320;
    private const int TargetHeight = 200;
    private const int BytesPerPixel = 4;
    private const int RowBytes = TargetWidth * BytesPerPixel;

    public static byte[] Build(byte[] rgbxPixels, byte[] bgrxPixels)
    {
        if (rgbxPixels.Length != TargetWidth * TargetHeight * BytesPerPixel ||
            bgrxPixels.Length != TargetWidth * TargetHeight * BytesPerPixel)
        {
            throw new InvalidOperationException("Unexpected UEFI framebuffer payload size.");
        }

        var code = BuildCode(rgbxPixels, bgrxPixels);
        return PortableExecutableBuilder.BuildEfiApplication(code, entryPointOffset: 0);
    }

    private static byte[] BuildCode(byte[] rgbxPixels, byte[] bgrxPixels)
    {
        var asm = new X64Assembler();

        asm.Push(Reg64.RDI);
        asm.Push(Reg64.RSI);
        asm.Push(Reg64.R12);
        asm.Push(Reg64.R13);
        asm.Push(Reg64.R14);
        asm.SubRsp(0x30);

        asm.MovR64FromMem(Reg64.RAX, Reg64.RDX, 0x60);
        asm.MovR64FromMem(Reg64.RAX, Reg64.RAX, 0x140);
        asm.LeaRipRelative(Reg64.RCX, "gop_guid");
        asm.XorR32(Reg64.RDX);
        asm.LeaRegFromMem(Reg64.R8, Reg64.RSP, 0x20);
        asm.CallReg(Reg64.RAX);
        asm.TestR64(Reg64.RAX);
        asm.Jnz("exit_status");

        asm.MovR64FromMem(Reg64.R9, Reg64.RSP, 0x20);
        asm.MovR64FromMem(Reg64.R10, Reg64.R9, 0x18);
        asm.MovR64FromMem(Reg64.RCX, Reg64.R10, 0x08);
        asm.MovR32FromMem(Reg64.RDX, Reg64.RCX, 0x04);
        asm.CmpR32Imm32(Reg64.RDX, TargetWidth);
        asm.Jb("unsupported");
        asm.MovR32FromMem(Reg64.RDX, Reg64.RCX, 0x08);
        asm.CmpR32Imm32(Reg64.RDX, TargetHeight);
        asm.Jb("unsupported");
        asm.MovR32FromMem(Reg64.RAX, Reg64.RCX, 0x0C);
        asm.CmpR32Imm32(Reg64.RAX, 3);
        asm.Je("unsupported");
        asm.LeaRipRelative(Reg64.RSI, "rgb_pixels");
        asm.TestR32(Reg64.RAX);
        asm.Je("source_ready");
        asm.LeaRipRelative(Reg64.RSI, "bgr_pixels");

        asm.MarkLabel("source_ready");
        asm.MovR64FromMem(Reg64.RDI, Reg64.R10, 0x18);
        asm.MovR32FromMem(Reg64.R12, Reg64.RCX, 0x20);
        asm.ShlR64(Reg64.R12, 2);
        asm.MovR64FromReg(Reg64.R14, Reg64.R12);
        asm.SubR64Imm32(Reg64.R14, RowBytes);
        asm.MovR32Imm32(Reg64.R13, TargetHeight);

        asm.MarkLabel("row_loop");
        asm.MovR32Imm32(Reg64.RCX, RowBytes / 8);
        asm.RepMovsq();
        asm.AddR64Reg(Reg64.RDI, Reg64.R14);
        asm.DecR64(Reg64.R13);
        asm.Jnz("row_loop");

        asm.XorR32(Reg64.RAX);
        asm.Jmp("epilogue");

        asm.MarkLabel("unsupported");
        asm.MovR32Imm32(Reg64.RAX, 3);
        asm.Jmp("epilogue");

        asm.MarkLabel("exit_status");
        asm.MarkLabel("epilogue");
        asm.AddRsp(0x30);
        asm.Pop(Reg64.R14);
        asm.Pop(Reg64.R13);
        asm.Pop(Reg64.R12);
        asm.Pop(Reg64.RSI);
        asm.Pop(Reg64.RDI);
        asm.Ret();

        asm.Align(8);
        asm.MarkLabel("gop_guid");
        asm.Emit(new byte[]
        {
            0xDE, 0xA9, 0x42, 0x90,
            0xDC, 0x23,
            0x38, 0x4A,
            0x96, 0xFB, 0x7A, 0xDE, 0xD0, 0x80, 0x51, 0x6A
        });

        asm.Align(16);
        asm.MarkLabel("rgb_pixels");
        asm.Emit(rgbxPixels);

        asm.Align(16);
        asm.MarkLabel("bgr_pixels");
        asm.Emit(bgrxPixels);

        return asm.ToArray();
    }
}

internal static class PortableExecutableBuilder
{
    public static byte[] BuildEfiApplication(byte[] sectionBytes, int entryPointOffset)
    {
        const int fileAlignment = 0x200;
        const int sectionAlignment = 0x1000;
        const int sectionRva = 0x1000;
        const int headersSize = 0x200;

        var rawSectionSize = AlignUp(sectionBytes.Length, fileAlignment);
        var imageSize = AlignUp(sectionRva + sectionBytes.Length, sectionAlignment);
        var fileSize = headersSize + rawSectionSize;
        var image = new byte[fileSize];

        image[0] = (byte)'M';
        image[1] = (byte)'Z';
        BinaryPrimitives.WriteInt32LittleEndian(image.AsSpan(0x3C, 4), 0x80);

        var peOffset = 0x80;
        image[peOffset + 0] = (byte)'P';
        image[peOffset + 1] = (byte)'E';
        image[peOffset + 2] = 0;
        image[peOffset + 3] = 0;

        var fileHeader = peOffset + 4;
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(fileHeader + 0, 2), 0x8664);
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(fileHeader + 2, 2), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(fileHeader + 16, 2), 0x00F0);
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(fileHeader + 18, 2), 0x0022);

        var optionalHeader = fileHeader + 20;
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(optionalHeader + 0, 2), 0x20B);
        image[optionalHeader + 2] = 1;
        image[optionalHeader + 3] = 0;
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(optionalHeader + 4, 4), checked((uint)rawSectionSize));
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(optionalHeader + 16, 4), checked((uint)(sectionRva + entryPointOffset)));
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(optionalHeader + 20, 4), sectionRva);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(optionalHeader + 24, 8), 0x0000000000100000);
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(optionalHeader + 32, 4), sectionAlignment);
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(optionalHeader + 36, 4), fileAlignment);
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(optionalHeader + 40, 2), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(optionalHeader + 48, 2), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(optionalHeader + 56, 4), checked((uint)imageSize));
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(optionalHeader + 60, 4), headersSize);
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(optionalHeader + 68, 2), 10);
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(optionalHeader + 70, 2), 0);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(optionalHeader + 72, 8), 0x00100000);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(optionalHeader + 80, 8), 0x00001000);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(optionalHeader + 88, 8), 0x00100000);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(optionalHeader + 96, 8), 0x00001000);
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(optionalHeader + 108, 4), 16);

        var sectionHeader = optionalHeader + 0xF0;
        WriteAscii(image.AsSpan(sectionHeader + 0, 8), ".text");
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(sectionHeader + 8, 4), checked((uint)sectionBytes.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(sectionHeader + 12, 4), sectionRva);
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(sectionHeader + 16, 4), checked((uint)rawSectionSize));
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(sectionHeader + 20, 4), headersSize);
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(sectionHeader + 36, 4), 0x60000020);

        Buffer.BlockCopy(sectionBytes, 0, image, headersSize, sectionBytes.Length);
        return image;
    }

    private static void WriteAscii(Span<byte> destination, string value)
    {
        destination.Clear();
        Encoding.ASCII.GetBytes(value).CopyTo(destination);
    }

    private static int AlignUp(int value, int alignment) => (value + alignment - 1) / alignment * alignment;
}

internal static class FatImageBuilder
{
    private const int BytesPerSector = 512;
    private const int TotalSectors = 32768;
    private const int ReservedSectors = 1;
    private const int FatCount = 2;
    private const int RootEntryCount = 224;
    private const int SectorsPerFat = 16;
    private const int SectorsPerCluster = 16;
    private const int ClusterSizeBytes = BytesPerSector * SectorsPerCluster;
    private const int RootDirectorySectors = 14;
    private const int FirstDataSector = ReservedSectors + (FatCount * SectorsPerFat) + RootDirectorySectors;

    public static byte[] BuildEfiSystemPartition(byte[] bootx64, IReadOnlyList<RuntimePackageFile> runtimeFiles)
    {
        var disk = new byte[TotalSectors * BytesPerSector];
        var root = new FatDirectoryNode("", null);
        var efiDir = root.GetOrAddDirectory("EFI");
        var bootDir = efiDir.GetOrAddDirectory("BOOT");
        bootDir.AddFile("BOOTX64.EFI", bootx64);

        foreach (var runtimeFile in runtimeFiles)
        {
            AddRuntimeFile(root, runtimeFile);
        }

        var allDirectories = new List<FatDirectoryNode>();
        CollectDirectories(root, allDirectories);
        var allFiles = new List<FatFileNode>();
        CollectFiles(root, allFiles);

        var nextCluster = 2;
        foreach (var directory in allDirectories.Where(d => d.Parent is not null))
        {
            var entryCount = directory.Children.Count + 2;
            directory.StartCluster = nextCluster;
            directory.ClusterCount = Math.Max(1, DivideRoundUp(entryCount * 32, ClusterSizeBytes));
            nextCluster += directory.ClusterCount;
        }

        foreach (var file in allFiles)
        {
            file.StartCluster = nextCluster;
            file.ClusterCount = Math.Max(1, DivideRoundUp(file.Data.Length, ClusterSizeBytes));
            nextCluster += file.ClusterCount;
        }

        if (FirstDataSector + ((nextCluster - 2) * SectorsPerCluster) > TotalSectors)
        {
            throw new InvalidOperationException("The EFI system partition ran out of space.");
        }

        var fatEntries = new ushort[nextCluster + 1];
        fatEntries[0] = 0xFF0;
        fatEntries[1] = 0xFFF;

        foreach (var directory in allDirectories.Where(d => d.Parent is not null))
        {
            LinkClusterChain(fatEntries, directory.StartCluster, directory.ClusterCount);
        }

        foreach (var file in allFiles)
        {
            LinkClusterChain(fatEntries, file.StartCluster, file.ClusterCount);
        }

        WriteBootSector(disk);
        WriteFatCopies(disk, fatEntries);
        WriteRootDirectory(disk, root);

        foreach (var directory in allDirectories.Where(d => d.Parent is not null))
        {
            WriteDirectoryClusters(disk, directory);
        }

        foreach (var file in allFiles)
        {
            WriteFileToClusterChain(disk, file.Data, file.StartCluster, file.ClusterCount);
        }

        return disk;
    }

    private static void WriteBootSector(byte[] disk)
    {
        var sector = disk.AsSpan(0, BytesPerSector);
        sector[0] = 0xEB;
        sector[1] = 0x3C;
        sector[2] = 0x90;
        WriteAsciiPadded(sector.Slice(3, 8), "MSWIN4.1");
        BinaryPrimitives.WriteUInt16LittleEndian(sector.Slice(11, 2), BytesPerSector);
        sector[13] = SectorsPerCluster;
        BinaryPrimitives.WriteUInt16LittleEndian(sector.Slice(14, 2), ReservedSectors);
        sector[16] = FatCount;
        BinaryPrimitives.WriteUInt16LittleEndian(sector.Slice(17, 2), RootEntryCount);
        BinaryPrimitives.WriteUInt16LittleEndian(sector.Slice(19, 2), TotalSectors);
        sector[21] = 0xF0;
        BinaryPrimitives.WriteUInt16LittleEndian(sector.Slice(22, 2), SectorsPerFat);
        BinaryPrimitives.WriteUInt16LittleEndian(sector.Slice(24, 2), 18);
        BinaryPrimitives.WriteUInt16LittleEndian(sector.Slice(26, 2), 2);
        sector[36] = 0;
        sector[38] = 0x29;
        BinaryPrimitives.WriteUInt32LittleEndian(sector.Slice(39, 4), 0x12345678);
        WriteAsciiPadded(sector.Slice(43, 11), "DRAGON_OS");
        WriteAsciiPadded(sector.Slice(54, 8), "FAT12");
        sector[510] = 0x55;
        sector[511] = 0xAA;
    }

    private static void WriteFatCopies(byte[] disk, ushort[] entries)
    {
        var fatBytes = new byte[SectorsPerFat * BytesPerSector];
        WriteFat12Entries(fatBytes, entries);

        var fat1Offset = ReservedSectors * BytesPerSector;
        var fat2Offset = fat1Offset + fatBytes.Length;
        Buffer.BlockCopy(fatBytes, 0, disk, fat1Offset, fatBytes.Length);
        Buffer.BlockCopy(fatBytes, 0, disk, fat2Offset, fatBytes.Length);
    }

    private static void WriteRootDirectory(byte[] disk, FatDirectoryNode rootDirectory)
    {
        var rootOffset = (ReservedSectors + (FatCount * SectorsPerFat)) * BytesPerSector;
        var rootSpan = disk.AsSpan(rootOffset, RootDirectorySectors * BytesPerSector);
        rootSpan.Clear();

        for (var i = 0; i < rootDirectory.Children.Count; i++)
        {
            WriteNodeEntry(rootSpan.Slice(i * 32, 32), rootDirectory.Children[i]);
        }
    }

    private static void WriteDirectoryClusters(byte[] disk, FatDirectoryNode directory)
    {
        var bytes = new byte[directory.ClusterCount * ClusterSizeBytes];
        var span = bytes.AsSpan();
        WriteSpecialDirectoryEntry(span.Slice(0, 32), ".", checked((ushort)directory.StartCluster));
        WriteSpecialDirectoryEntry(
            span.Slice(32, 32),
            "..",
            directory.Parent?.Parent is null ? (ushort)0 : checked((ushort)directory.Parent!.StartCluster));

        for (var i = 0; i < directory.Children.Count; i++)
        {
            WriteNodeEntry(span.Slice((i + 2) * 32, 32), directory.Children[i]);
        }

        WriteFileToClusterChain(disk, bytes, directory.StartCluster, directory.ClusterCount);
    }

    private static void WriteNodeEntry(Span<byte> entry, FatNode node)
    {
        if (node is FatDirectoryNode directory)
        {
            WriteDirectoryEntry(entry, directory.ShortName, directory.ShortExtension, 0x10, checked((ushort)directory.StartCluster), 0);
            return;
        }

        var file = (FatFileNode)node;
        WriteDirectoryEntry(entry, file.ShortName, file.ShortExtension, 0x20, checked((ushort)file.StartCluster), file.Data.Length);
    }

    private static void WriteFileToClusterChain(byte[] disk, byte[] data, int startCluster, int clusterCount)
    {
        for (var i = 0; i < clusterCount; i++)
        {
            var clusterSpan = GetClusterSpan(disk, startCluster + i);
            clusterSpan.Clear();
            var sourceOffset = i * ClusterSizeBytes;
            if (sourceOffset >= data.Length)
            {
                continue;
            }

            var chunkLength = Math.Min(ClusterSizeBytes, data.Length - sourceOffset);
            data.AsSpan(sourceOffset, chunkLength).CopyTo(clusterSpan);
        }
    }

    private static Span<byte> GetClusterSpan(byte[] disk, int cluster)
    {
        var sector = FirstDataSector + ((cluster - 2) * SectorsPerCluster);
        return disk.AsSpan(sector * BytesPerSector, ClusterSizeBytes);
    }

    private static void WriteFat12Entries(byte[] fatBytes, ushort[] entries)
    {
        fatBytes.AsSpan().Clear();
        SetFat12Entry(fatBytes, 0, entries[0]);
        SetFat12Entry(fatBytes, 1, entries[1]);

        for (var i = 2; i < entries.Length; i++)
        {
            if (entries[i] != 0)
            {
                SetFat12Entry(fatBytes, i, entries[i]);
            }
        }
    }

    private static void SetFat12Entry(byte[] fat, int index, ushort value)
    {
        value &= 0x0FFF;
        var offset = index + (index / 2);
        if ((index & 1) == 0)
        {
            fat[offset] = (byte)(value & 0xFF);
            fat[offset + 1] = (byte)((fat[offset + 1] & 0xF0) | ((value >> 8) & 0x0F));
        }
        else
        {
            fat[offset] = (byte)((fat[offset] & 0x0F) | ((value << 4) & 0xF0));
            fat[offset + 1] = (byte)((value >> 4) & 0xFF);
        }
    }

    private static void WriteSpecialDirectoryEntry(Span<byte> entry, string name, ushort cluster)
    {
        entry.Clear();
        if (name == ".")
        {
            entry[0] = (byte)'.';
            entry[1] = (byte)' ';
        }
        else
        {
            entry[0] = (byte)'.';
            entry[1] = (byte)'.';
        }

        entry[11] = 0x10;
        BinaryPrimitives.WriteUInt16LittleEndian(entry.Slice(26, 2), cluster);
    }

    private static void WriteDirectoryEntry(Span<byte> entry, string name, string extension, byte attributes, ushort firstCluster, int size)
    {
        entry.Clear();
        WriteShortName(entry.Slice(0, 8), name);
        WriteShortName(entry.Slice(8, 3), extension);
        entry[11] = attributes;
        BinaryPrimitives.WriteUInt16LittleEndian(entry.Slice(26, 2), firstCluster);
        BinaryPrimitives.WriteUInt32LittleEndian(entry.Slice(28, 4), checked((uint)size));
    }

    private static void WriteShortName(Span<byte> destination, string value)
    {
        destination.Fill((byte)' ');
        var upper = value.ToUpperInvariant();
        Encoding.ASCII.GetBytes(upper[..Math.Min(upper.Length, destination.Length)]).CopyTo(destination);
    }

    private static void WriteAsciiPadded(Span<byte> destination, string value)
    {
        destination.Fill((byte)' ');
        Encoding.ASCII.GetBytes(value[..Math.Min(value.Length, destination.Length)]).CopyTo(destination);
    }

    private static void AddRuntimeFile(FatDirectoryNode root, RuntimePackageFile runtimeFile)
    {
        var segments = runtimeFile.RelativePath
            .Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            return;
        }

        var directory = root;
        for (var i = 0; i < segments.Length - 1; i++)
        {
            directory = directory.GetOrAddDirectory(segments[i]);
        }

        directory.AddFile(segments[^1], runtimeFile.Data);
    }

    private static void CollectDirectories(FatDirectoryNode directory, List<FatDirectoryNode> output)
    {
        output.Add(directory);
        foreach (var child in directory.Children.OfType<FatDirectoryNode>())
        {
            CollectDirectories(child, output);
        }
    }

    private static void CollectFiles(FatDirectoryNode directory, List<FatFileNode> output)
    {
        foreach (var child in directory.Children)
        {
            if (child is FatDirectoryNode subdirectory)
            {
                CollectFiles(subdirectory, output);
            }
            else
            {
                output.Add((FatFileNode)child);
            }
        }
    }

    private static void LinkClusterChain(ushort[] fatEntries, int startCluster, int clusterCount)
    {
        for (var i = 0; i < clusterCount; i++)
        {
            var cluster = startCluster + i;
            fatEntries[cluster] = i == clusterCount - 1
                ? (ushort)0xFFF
                : checked((ushort)(cluster + 1));
        }
    }

    private static int DivideRoundUp(int value, int divisor) => (value + divisor - 1) / divisor;

    private abstract class FatNode(string shortName, string shortExtension)
    {
        public string ShortName { get; } = shortName;

        public string ShortExtension { get; } = shortExtension;
    }

    private sealed class FatDirectoryNode : FatNode
    {
        public FatDirectoryNode(string shortName, FatDirectoryNode? parent)
            : base(ShortNameParts.NormalizeName(shortName).Name, "")
        {
            Parent = parent;
        }

        public FatDirectoryNode? Parent { get; }

        public List<FatNode> Children { get; } = [];

        public int StartCluster { get; set; }

        public int ClusterCount { get; set; }

        public FatDirectoryNode GetOrAddDirectory(string rawName)
        {
            var normalized = ShortNameParts.NormalizeName(rawName);
            var existing = Children
                .OfType<FatDirectoryNode>()
                .FirstOrDefault(child => child.ShortName.Equals(normalized.Name, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                return existing;
            }

            var directory = new FatDirectoryNode(normalized.Name, this);
            Children.Add(directory);
            return directory;
        }

        public void AddFile(string rawName, byte[] data)
        {
            var normalized = ShortNameParts.NormalizeFile(rawName);
            Children.RemoveAll(child => child is FatFileNode file &&
                file.ShortName.Equals(normalized.Name, StringComparison.OrdinalIgnoreCase) &&
                file.ShortExtension.Equals(normalized.Extension, StringComparison.OrdinalIgnoreCase));
            Children.Add(new FatFileNode(normalized.Name, normalized.Extension, data));
        }
    }

    private sealed class FatFileNode(string shortName, string shortExtension, byte[] data) : FatNode(shortName, shortExtension)
    {
        public byte[] Data { get; } = data;

        public int StartCluster { get; set; }

        public int ClusterCount { get; set; }
    }

    private readonly record struct ShortNameParts(string Name, string Extension)
    {
        public static ShortNameParts NormalizeName(string rawName)
        {
            var name = Sanitize(rawName, 8);
            return new ShortNameParts(name, "");
        }

        public static ShortNameParts NormalizeFile(string rawName)
        {
            var extension = Path.GetExtension(rawName);
            var fileName = Path.GetFileNameWithoutExtension(rawName);
            return new ShortNameParts(Sanitize(fileName, 8), Sanitize(extension.TrimStart('.'), 3));
        }

        private static string Sanitize(string rawName, int maxLength)
        {
            var builder = new StringBuilder(maxLength);
            foreach (var ch in rawName.ToUpperInvariant())
            {
                if ((ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
                {
                    builder.Append(ch);
                }

                if (builder.Length == maxLength)
                {
                    break;
                }
            }

            if (builder.Length == 0)
            {
                builder.Append('X');
            }

            return builder.ToString();
        }
    }
}

internal static class UefiIsoBuilder
{
    private const int IsoSectorSize = 2048;
    private const int PrimaryVolumeDescriptorLba = 16;
    private const int BootRecordLba = 17;
    private const int VolumeDescriptorTerminatorLba = 18;
    private const int PathTableLeLba = 19;
    private const int PathTableBeLba = 20;
    private const int RootDirectoryLba = 21;
    private const int EfiDirectoryLba = 22;
    private const int BootDirectoryLba = 23;
    private const int BootCatalogLba = 24;
    private const int BootImageLba = 25;

    public static byte[] BuildBootableIso(byte[] efiSystemPartition, byte[] bootx64)
    {
        var espSectorCount = DivideRoundUp(efiSystemPartition.Length, IsoSectorSize);
        var bootx64Lba = BootImageLba + espSectorCount;
        var bootx64SectorCount = DivideRoundUp(bootx64.Length, IsoSectorSize);
        var totalSectors = bootx64Lba + bootx64SectorCount;
        var iso = new byte[totalSectors * IsoSectorSize];
        var timestamp = DateTime.UtcNow;

        WritePrimaryVolumeDescriptor(iso, totalSectors, timestamp);
        WriteBootRecordVolumeDescriptor(iso);
        WriteVolumeDescriptorTerminator(iso);
        WritePathTables(iso);
        WriteRootDirectory(iso, timestamp);
        WriteEfiDirectory(iso, timestamp);
        WriteBootDirectory(iso, bootx64Lba, bootx64.Length, timestamp);
        WriteBootCatalog(iso);
        Buffer.BlockCopy(efiSystemPartition, 0, iso, BootImageLba * IsoSectorSize, efiSystemPartition.Length);
        Buffer.BlockCopy(bootx64, 0, iso, bootx64Lba * IsoSectorSize, bootx64.Length);

        return iso;
    }

    private static void WritePrimaryVolumeDescriptor(byte[] iso, int totalSectors, DateTime timestamp)
    {
        var sector = new Span<byte>(iso, PrimaryVolumeDescriptorLba * IsoSectorSize, IsoSectorSize);
        sector[0] = 0x01;
        WriteStandardIdentifier(sector);
        sector[6] = 0x01;
        WritePaddedAscii(sector, 8, 32, "BITMAP OS");
        WritePaddedAscii(sector, 40, 32, "BITMAP_OS");
        WriteBothEndianUInt32(sector, 80, checked((uint)totalSectors));
        WriteBothEndianUInt16(sector, 120, 1);
        WriteBothEndianUInt16(sector, 124, 1);
        WriteBothEndianUInt16(sector, 128, IsoSectorSize);
        WriteBothEndianUInt32(sector, 132, 34);
        BinaryPrimitives.WriteUInt32LittleEndian(sector.Slice(140, 4), PathTableLeLba);
        BinaryPrimitives.WriteUInt32BigEndian(sector.Slice(148, 4), PathTableBeLba);
        var rootRecord = BuildDirectoryRecord(RootDirectoryLba, IsoSectorSize, timestamp, 0x02, [0x00]);
        rootRecord.CopyTo(sector.Slice(156, rootRecord.Length));
        WritePaddedAscii(sector, 318, 128, "Bitmap OS Builder");
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
        var littleOffset = 0;
        littleOffset += WritePathTableEntryLittleEndian(little.Slice(littleOffset), RootDirectoryLba, 1, [0x00]);
        littleOffset += WritePathTableEntryLittleEndian(little.Slice(littleOffset), EfiDirectoryLba, 1, Encoding.ASCII.GetBytes("EFI"));
        WritePathTableEntryLittleEndian(little.Slice(littleOffset), BootDirectoryLba, 2, Encoding.ASCII.GetBytes("BOOT"));

        var big = new Span<byte>(iso, PathTableBeLba * IsoSectorSize, IsoSectorSize);
        var bigOffset = 0;
        bigOffset += WritePathTableEntryBigEndian(big.Slice(bigOffset), RootDirectoryLba, 1, [0x00]);
        bigOffset += WritePathTableEntryBigEndian(big.Slice(bigOffset), EfiDirectoryLba, 1, Encoding.ASCII.GetBytes("EFI"));
        WritePathTableEntryBigEndian(big.Slice(bigOffset), BootDirectoryLba, 2, Encoding.ASCII.GetBytes("BOOT"));
    }

    private static void WriteRootDirectory(byte[] iso, DateTime timestamp)
    {
        var sector = new Span<byte>(iso, RootDirectoryLba * IsoSectorSize, IsoSectorSize);
        var offset = 0;
        offset += CopyRecord(sector, offset, BuildDirectoryRecord(RootDirectoryLba, IsoSectorSize, timestamp, 0x02, [0x00]));
        offset += CopyRecord(sector, offset, BuildDirectoryRecord(RootDirectoryLba, IsoSectorSize, timestamp, 0x02, [0x01]));
        offset += CopyRecord(sector, offset, BuildDirectoryRecord(EfiDirectoryLba, IsoSectorSize, timestamp, 0x02, Encoding.ASCII.GetBytes("EFI")));
        CopyRecord(sector, offset, BuildDirectoryRecord(BootCatalogLba, IsoSectorSize, timestamp, 0x00, Encoding.ASCII.GetBytes("BOOT.CAT;1")));
    }

    private static void WriteEfiDirectory(byte[] iso, DateTime timestamp)
    {
        var sector = new Span<byte>(iso, EfiDirectoryLba * IsoSectorSize, IsoSectorSize);
        var offset = 0;
        offset += CopyRecord(sector, offset, BuildDirectoryRecord(EfiDirectoryLba, IsoSectorSize, timestamp, 0x02, [0x00]));
        offset += CopyRecord(sector, offset, BuildDirectoryRecord(RootDirectoryLba, IsoSectorSize, timestamp, 0x02, [0x01]));
        CopyRecord(sector, offset, BuildDirectoryRecord(BootDirectoryLba, IsoSectorSize, timestamp, 0x02, Encoding.ASCII.GetBytes("BOOT")));
    }

    private static void WriteBootDirectory(byte[] iso, int bootx64Lba, int bootx64Length, DateTime timestamp)
    {
        var sector = new Span<byte>(iso, BootDirectoryLba * IsoSectorSize, IsoSectorSize);
        var offset = 0;
        offset += CopyRecord(sector, offset, BuildDirectoryRecord(BootDirectoryLba, IsoSectorSize, timestamp, 0x02, [0x00]));
        offset += CopyRecord(sector, offset, BuildDirectoryRecord(EfiDirectoryLba, IsoSectorSize, timestamp, 0x02, [0x01]));
        CopyRecord(sector, offset, BuildDirectoryRecord(checked((uint)bootx64Lba), checked((uint)bootx64Length), timestamp, 0x00, Encoding.ASCII.GetBytes("BOOTX64.EFI;1")));
    }

    private static void WriteBootCatalog(byte[] iso)
    {
        var sector = new Span<byte>(iso, BootCatalogLba * IsoSectorSize, IsoSectorSize);
        sector[0] = 0x01;
        sector[1] = 0xEF;
        WritePaddedAscii(sector, 4, 24, "BITMAP OS UEFI");
        sector[30] = 0x55;
        sector[31] = 0xAA;
        var checksum = ComputeElToritoChecksum(sector.Slice(0, 32));
        BinaryPrimitives.WriteUInt16LittleEndian(sector.Slice(28, 2), checksum);

        sector[32] = 0x88;
        sector[33] = 0x00;
        BinaryPrimitives.WriteUInt16LittleEndian(sector.Slice(34, 2), 0);
        sector[36] = 0x00;
        sector[37] = 0x00;
        BinaryPrimitives.WriteUInt16LittleEndian(sector.Slice(38, 2), 1);
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
        WriteBothEndianUInt32(span, 2, extentLba);
        WriteBothEndianUInt32(span, 10, dataLength);
        WriteDirectoryTimestamp(span.Slice(18, 7), recordingTime);
        span[25] = flags;
        WriteBothEndianUInt16(span, 28, 1);
        span[32] = checked((byte)fileIdentifier.Length);
        fileIdentifier.CopyTo(span.Slice(33, fileIdentifier.Length));
        return record;
    }

    private static int WritePathTableEntryLittleEndian(Span<byte> destination, int extentLba, ushort parentDirectoryNumber, ReadOnlySpan<byte> identifier)
    {
        destination[0] = checked((byte)identifier.Length);
        destination[1] = 0;
        BinaryPrimitives.WriteUInt32LittleEndian(destination.Slice(2, 4), checked((uint)extentLba));
        BinaryPrimitives.WriteUInt16LittleEndian(destination.Slice(6, 2), parentDirectoryNumber);
        identifier.CopyTo(destination.Slice(8, identifier.Length));
        var length = 8 + identifier.Length;
        if ((identifier.Length & 1) != 0)
        {
            destination[8 + identifier.Length] = 0;
            length++;
        }

        return length;
    }

    private static int WritePathTableEntryBigEndian(Span<byte> destination, int extentLba, ushort parentDirectoryNumber, ReadOnlySpan<byte> identifier)
    {
        destination[0] = checked((byte)identifier.Length);
        destination[1] = 0;
        BinaryPrimitives.WriteUInt32BigEndian(destination.Slice(2, 4), checked((uint)extentLba));
        BinaryPrimitives.WriteUInt16BigEndian(destination.Slice(6, 2), parentDirectoryNumber);
        identifier.CopyTo(destination.Slice(8, identifier.Length));
        var length = 8 + identifier.Length;
        if ((identifier.Length & 1) != 0)
        {
            destination[8 + identifier.Length] = 0;
            length++;
        }

        return length;
    }

    private static int CopyRecord(Span<byte> destination, int offset, byte[] record)
    {
        record.CopyTo(destination.Slice(offset, record.Length));
        return record.Length;
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
        Encoding.ASCII.GetBytes(value[..Math.Min(value.Length, length)]).CopyTo(destination.Slice(offset, length));
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

    private static int DivideRoundUp(int value, int divisor) => (value + divisor - 1) / divisor;
}

internal sealed class X64Assembler
{
    private readonly List<byte> _bytes = [];
    private readonly Dictionary<string, int> _labels = new(StringComparer.Ordinal);
    private readonly List<Fixup> _fixups = [];

    public void MarkLabel(string name) => _labels[name] = _bytes.Count;

    public void Align(int alignment)
    {
        while ((_bytes.Count % alignment) != 0)
        {
            _bytes.Add(0x90);
        }
    }

    public void Emit(byte value) => _bytes.Add(value);

    public void Emit(ReadOnlySpan<byte> values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            _bytes.Add(values[i]);
        }
    }

    public byte[] ToArray()
    {
        var output = _bytes.ToArray();

        foreach (var fixup in _fixups)
        {
            if (!_labels.TryGetValue(fixup.Label, out var target))
            {
                throw new InvalidOperationException($"Unknown x64 label: {fixup.Label}");
            }

            switch (fixup.Kind)
            {
                case FixupKind.Relative32:
                {
                    var displacement = target - (fixup.Position + 4);
                    BinaryPrimitives.WriteInt32LittleEndian(output.AsSpan(fixup.Position, 4), displacement);
                    break;
                }
                case FixupKind.RipRelative32:
                {
                    var displacement = target - (fixup.Position + 4);
                    BinaryPrimitives.WriteInt32LittleEndian(output.AsSpan(fixup.Position, 4), displacement);
                    break;
                }
            }
        }

        return output;
    }

    public void Push(Reg64 reg) => EmitWithOptionalRex(0x50, reg, w: false);
    public void Pop(Reg64 reg) => EmitWithOptionalRex(0x58, reg, w: false);
    public void Ret() => Emit(0xC3);
    public void RepMovsq() => Emit(stackalloc byte[] { 0xF3, 0x48, 0xA5 });

    public void SubRsp(int immediate)
    {
        Emit(0x48);
        Emit(0x83);
        Emit(0xEC);
        Emit(checked((byte)immediate));
    }

    public void AddRsp(int immediate)
    {
        Emit(0x48);
        Emit(0x83);
        Emit(0xC4);
        Emit(checked((byte)immediate));
    }

    public void MovR64FromMem(Reg64 destination, Reg64 baseRegister, int displacement) =>
        EmitMovFromMemory(destination, baseRegister, displacement, is64Bit: true);

    public void MovR32FromMem(Reg64 destination, Reg64 baseRegister, int displacement) =>
        EmitMovFromMemory(destination, baseRegister, displacement, is64Bit: false);

    public void MovR64FromReg(Reg64 destination, Reg64 source)
    {
        EmitRex(w: true, r: (int)source, b: (int)destination);
        Emit(0x89);
        Emit(ModRm(0b11, (int)source, (int)destination));
    }

    public void MovR32Imm32(Reg64 destination, int value)
    {
        EmitRex(w: false, b: (int)destination);
        Emit((byte)(0xB8 + ((int)destination & 7)));
        Emit(BitConverter.GetBytes(value));
    }

    public void LeaRipRelative(Reg64 destination, string label)
    {
        EmitRex(w: true, r: (int)destination);
        Emit(0x8D);
        Emit(ModRm(0b00, (int)destination, 0b101));
        AddFixup(FixupKind.RipRelative32, label);
    }

    public void LeaRegFromMem(Reg64 destination, Reg64 baseRegister, int displacement)
    {
        EmitRex(w: true, r: (int)destination, b: (int)baseRegister);
        Emit(0x8D);
        EmitMemoryOperand((int)destination, (int)baseRegister, displacement);
    }

    public void CallReg(Reg64 reg)
    {
        EmitRex(w: true, b: (int)reg);
        Emit(0xFF);
        Emit(ModRm(0b11, 0b010, (int)reg));
    }

    public void TestR64(Reg64 reg)
    {
        EmitRex(w: true, r: (int)reg, b: (int)reg);
        Emit(0x85);
        Emit(ModRm(0b11, (int)reg, (int)reg));
    }

    public void TestR32(Reg64 reg)
    {
        EmitRex(w: false, r: (int)reg, b: (int)reg);
        Emit(0x85);
        Emit(ModRm(0b11, (int)reg, (int)reg));
    }

    public void XorR32(Reg64 reg)
    {
        EmitRex(w: false, r: (int)reg, b: (int)reg);
        Emit(0x31);
        Emit(ModRm(0b11, (int)reg, (int)reg));
    }

    public void CmpR32Imm32(Reg64 reg, int value)
    {
        EmitRex(w: false, b: (int)reg);
        Emit(0x81);
        Emit(ModRm(0b11, 0b111, (int)reg));
        Emit(BitConverter.GetBytes(value));
    }

    public void ShlR64(Reg64 reg, byte count)
    {
        EmitRex(w: true, b: (int)reg);
        Emit(0xC1);
        Emit(ModRm(0b11, 0b100, (int)reg));
        Emit(count);
    }

    public void SubR64Imm32(Reg64 reg, int value)
    {
        EmitRex(w: true, b: (int)reg);
        Emit(0x81);
        Emit(ModRm(0b11, 0b101, (int)reg));
        Emit(BitConverter.GetBytes(value));
    }

    public void AddR64Reg(Reg64 destination, Reg64 source)
    {
        EmitRex(w: true, r: (int)source, b: (int)destination);
        Emit(0x01);
        Emit(ModRm(0b11, (int)source, (int)destination));
    }

    public void DecR64(Reg64 reg)
    {
        EmitRex(w: true, b: (int)reg);
        Emit(0xFF);
        Emit(ModRm(0b11, 0b001, (int)reg));
    }

    public void Jnz(string label)
    {
        Emit(stackalloc byte[] { 0x0F, 0x85 });
        AddFixup(FixupKind.Relative32, label);
    }

    public void Je(string label)
    {
        Emit(stackalloc byte[] { 0x0F, 0x84 });
        AddFixup(FixupKind.Relative32, label);
    }

    public void Jb(string label)
    {
        Emit(stackalloc byte[] { 0x0F, 0x82 });
        AddFixup(FixupKind.Relative32, label);
    }

    public void Jmp(string label)
    {
        Emit(0xE9);
        AddFixup(FixupKind.Relative32, label);
    }

    private void EmitMovFromMemory(Reg64 destination, Reg64 baseRegister, int displacement, bool is64Bit)
    {
        EmitRex(w: is64Bit, r: (int)destination, b: (int)baseRegister);
        Emit(0x8B);
        EmitMemoryOperand((int)destination, (int)baseRegister, displacement);
    }

    private void EmitMemoryOperand(int regField, int baseRegister, int displacement)
    {
        if (displacement >= sbyte.MinValue && displacement <= sbyte.MaxValue)
        {
            Emit(ModRm(0b01, regField, baseRegister));
            if ((baseRegister & 7) == 4)
            {
                Emit(0x24);
            }

            Emit(unchecked((byte)(sbyte)displacement));
            return;
        }

        Emit(ModRm(0b10, regField, baseRegister));
        if ((baseRegister & 7) == 4)
        {
            Emit(0x24);
        }

        Emit(BitConverter.GetBytes(displacement));
    }

    private void EmitWithOptionalRex(byte baseOpcode, Reg64 reg, bool w)
    {
        if ((int)reg >= 8)
        {
            EmitRex(w, b: (int)reg);
        }

        Emit((byte)(baseOpcode + ((int)reg & 7)));
    }

    private void EmitRex(bool w, int r = 0, int x = 0, int b = 0)
    {
        var rex = 0x40;
        if (w)
        {
            rex |= 0x08;
        }

        if ((r & 8) != 0)
        {
            rex |= 0x04;
        }

        if ((x & 8) != 0)
        {
            rex |= 0x02;
        }

        if ((b & 8) != 0)
        {
            rex |= 0x01;
        }

        if (rex != 0x40)
        {
            Emit((byte)rex);
        }
    }

    private static byte ModRm(int mod, int reg, int rm) =>
        (byte)(((mod & 0x3) << 6) | ((reg & 0x7) << 3) | (rm & 0x7));

    private void AddFixup(FixupKind kind, string label)
    {
        _fixups.Add(new Fixup(_bytes.Count, label, kind));
        Emit(new byte[4]);
    }

    private readonly record struct Fixup(int Position, string Label, FixupKind Kind);

    private enum FixupKind
    {
        Relative32,
        RipRelative32
    }
}

internal enum Reg64
{
    RAX = 0,
    RCX = 1,
    RDX = 2,
    RBX = 3,
    RSP = 4,
    RBP = 5,
    RSI = 6,
    RDI = 7,
    R8 = 8,
    R9 = 9,
    R10 = 10,
    R11 = 11,
    R12 = 12,
    R13 = 13,
    R14 = 14,
    R15 = 15
}
