using DragonOSBuilder;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace BitmapOsBuilder;

internal static class ImapiIsoBuilder
{
    private const int FsiFileSystemIso9660 = 1;
    private const int FsiFileSystemJoliet = 2;
    private const int FsiFileSystemUdf = 4;
    private const int PlatformEfi = 0xEF;
    private const int EmulationNone = 0;
    private const int AdTypeBinary = 1;

    public static byte[] Build(string buildDir, byte[] bootImage, byte[] bootx64, IReadOnlyList<RuntimePackageFile> runtimeFiles)
    {
        var stagingRoot = Path.Combine(buildDir, "iso-root");
        var bootDir = Path.Combine(stagingRoot, "EFI", "BOOT");
        var bootImagePath = Path.Combine(buildDir, "efiboot.img");
        var bootx64Path = Path.Combine(bootDir, "BOOTX64.EFI");

        if (Directory.Exists(stagingRoot))
        {
            Directory.Delete(stagingRoot, recursive: true);
        }

        Directory.CreateDirectory(bootDir);
        File.WriteAllBytes(bootImagePath, bootImage);
        File.WriteAllBytes(bootx64Path, bootx64);

        foreach (var runtimeFile in runtimeFiles)
        {
            var destinationPath = Path.Combine(stagingRoot, runtimeFile.RelativePath);
            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.WriteAllBytes(destinationPath, runtimeFile.Data);
        }

        dynamic fileSystemImage = Activator.CreateInstance(Type.GetTypeFromProgID("IMAPI2FS.MsftFileSystemImage")!)
            ?? throw new InvalidOperationException("Could not create IMAPI2FS.MsftFileSystemImage.");
        dynamic bootOptions = Activator.CreateInstance(Type.GetTypeFromProgID("IMAPI2FS.BootOptions")!)
            ?? throw new InvalidOperationException("Could not create IMAPI2FS.BootOptions.");
        dynamic bootStream = Activator.CreateInstance(Type.GetTypeFromProgID("ADODB.Stream")!)
            ?? throw new InvalidOperationException("Could not create ADODB.Stream.");

        fileSystemImage.FileSystemsToCreate = FsiFileSystemIso9660 | FsiFileSystemJoliet | FsiFileSystemUdf;
        fileSystemImage.VolumeName = "DRAGON_OS";
        fileSystemImage.Root.AddTree(stagingRoot, false);

        bootStream.Type = AdTypeBinary;
        bootStream.Open();
        bootStream.LoadFromFile(bootImagePath);

        bootOptions.AssignBootImage(bootStream);
        bootOptions.PlatformId = PlatformEfi;
        bootOptions.Emulation = EmulationNone;
        bootOptions.Manufacturer = "Dragon OS";

        fileSystemImage.BootImageOptionsArray = new object[] { bootOptions };

        dynamic result = fileSystemImage.CreateResultImage();
        var imageStream = (IStream)result.ImageStream;
        return ReadAllBytes(imageStream);
    }

    private static byte[] ReadAllBytes(IStream stream)
    {
        stream.Stat(out var stat, 1);
        var totalLength = checked((long)stat.cbSize);
        var output = new byte[totalLength];
        var bytesReadPtr = Marshal.AllocHGlobal(sizeof(int));

        try
        {
            stream.Seek(0, 0, IntPtr.Zero);
            var offset = 0;
            while (offset < totalLength)
            {
                var chunkSize = (int)Math.Min(64 * 1024, totalLength - offset);
                var buffer = new byte[chunkSize];
                Marshal.WriteInt32(bytesReadPtr, 0);
                stream.Read(buffer, chunkSize, bytesReadPtr);
                var bytesRead = Marshal.ReadInt32(bytesReadPtr);
                if (bytesRead <= 0)
                {
                    break;
                }

                Buffer.BlockCopy(buffer, 0, output, offset, bytesRead);
                offset += bytesRead;
            }

            if (offset != totalLength)
            {
                Array.Resize(ref output, offset);
            }

            return output;
        }
        finally
        {
            Marshal.FreeHGlobal(bytesReadPtr);
        }
    }
}
