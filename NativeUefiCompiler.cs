using DragonOSBuilder;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Text;

namespace BitmapOsBuilder;

internal static class NativeUefiCompiler
{
    public static byte[] Build(string projectRoot, string buildDir, BmpImage splashImage, BmpImage cursorImage, BmpImage setupIconImage, IReadOnlyList<RuntimePackageFile> runtimeFiles)
    {
        var sourcePath = Path.Combine(projectRoot, "BootApp.c");
        var generatedHeaderPath = Path.Combine(buildDir, "generated_bitmap.h");
        var objPath = Path.Combine(buildDir, "BootApp.obj");
        var efiPath = Path.Combine(buildDir, "BOOTX64.EFI");

        WriteGeneratedHeader(projectRoot, generatedHeaderPath, splashImage, cursorImage, setupIconImage, runtimeFiles);

        var tools = FindToolchain();
        RunProcess(
            tools.ClPath,
            $"/nologo /c /TC /GS- /Gs9999999 /W4 /WX /Zl /Fo\"{objPath}\" /I\"{buildDir}\" \"{sourcePath}\"",
            projectRoot);

        RunProcess(
            tools.LinkPath,
            $"/nologo /subsystem:efi_application /nodefaultlib /entry:EfiMain /machine:x64 /out:\"{efiPath}\" \"{objPath}\"",
            projectRoot);

        return File.ReadAllBytes(efiPath);
    }

    private static void WriteGeneratedHeader(string projectRoot, string path, BmpImage splashImage, BmpImage cursorImage, BmpImage setupIconImage, IReadOnlyList<RuntimePackageFile> runtimeFiles)
    {
        var builder = new StringBuilder();
        var fontAtlas = FontAtlasBuilder.Build(projectRoot, "Ubuntu", 14f, 24, 32, FontStyle.Regular);
        var iconFontAtlas = FontAtlasBuilder.Build(projectRoot, "Ubuntu Mono", 5f, 8, 8, FontStyle.Regular);
        var setupTitleFontAtlas = FontAtlasBuilder.Build(projectRoot, "Ubuntu", 44f, 72, 96, FontStyle.Bold);
        var setupUiFontAtlas = FontAtlasBuilder.Build(projectRoot, "Ubuntu", 24f, 40, 56, FontStyle.Regular);
        var tweenTables = TweenTableBuilder.Build();
        var dllCount = runtimeFiles.Count(file => file.Kind.Equals("DLL", StringComparison.OrdinalIgnoreCase));
        var pngCount = runtimeFiles.Count(file => file.Kind.Equals("PNG", StringComparison.OrdinalIgnoreCase));
        var mp4Count = runtimeFiles.Count(file => file.Kind.Equals("MP4", StringComparison.OrdinalIgnoreCase));
        var runtimeSummaryText = $"DLL:{dllCount} PNG:{pngCount} MP4:{mp4Count}";
        var consoleIconText = LoadConsoleIconText(projectRoot);
        var runtimeRegistryText = BuildRuntimeRegistryText(runtimeFiles);
        var videoSummaryText = runtimeFiles
            .FirstOrDefault(file => file.Kind.Equals("MP4", StringComparison.OrdinalIgnoreCase)) is { } videoFile
            ? $"MP4 metadata only: {videoFile.SummaryText}"
            : "MP4 metadata only: no video loaded";

        builder.AppendLine($"#define SPLASH_WIDTH {splashImage.Width}u");
        builder.AppendLine($"#define SPLASH_HEIGHT {splashImage.Height}u");
        builder.AppendLine($"#define CURSOR_WIDTH {cursorImage.Width}u");
        builder.AppendLine($"#define CURSOR_HEIGHT {cursorImage.Height}u");
        builder.AppendLine($"#define SETUP_ICON_WIDTH {setupIconImage.Width}u");
        builder.AppendLine($"#define SETUP_ICON_HEIGHT {setupIconImage.Height}u");
        builder.AppendLine($"#define FONT_FIRST_CHAR {fontAtlas.FirstChar}u");
        builder.AppendLine($"#define FONT_GLYPH_COUNT {fontAtlas.GlyphCount}u");
        builder.AppendLine($"#define FONT_GLYPH_WIDTH {fontAtlas.GlyphWidth}u");
        builder.AppendLine($"#define FONT_GLYPH_HEIGHT {fontAtlas.GlyphHeight}u");
        builder.AppendLine($"#define ICON_FONT_FIRST_CHAR {iconFontAtlas.FirstChar}u");
        builder.AppendLine($"#define ICON_FONT_GLYPH_COUNT {iconFontAtlas.GlyphCount}u");
        builder.AppendLine($"#define ICON_FONT_GLYPH_WIDTH {iconFontAtlas.GlyphWidth}u");
        builder.AppendLine($"#define ICON_FONT_GLYPH_HEIGHT {iconFontAtlas.GlyphHeight}u");
        builder.AppendLine($"#define SETUP_TITLE_FONT_FIRST_CHAR {setupTitleFontAtlas.FirstChar}u");
        builder.AppendLine($"#define SETUP_TITLE_FONT_GLYPH_COUNT {setupTitleFontAtlas.GlyphCount}u");
        builder.AppendLine($"#define SETUP_TITLE_FONT_GLYPH_WIDTH {setupTitleFontAtlas.GlyphWidth}u");
        builder.AppendLine($"#define SETUP_TITLE_FONT_GLYPH_HEIGHT {setupTitleFontAtlas.GlyphHeight}u");
        builder.AppendLine($"#define SETUP_UI_FONT_FIRST_CHAR {setupUiFontAtlas.FirstChar}u");
        builder.AppendLine($"#define SETUP_UI_FONT_GLYPH_COUNT {setupUiFontAtlas.GlyphCount}u");
        builder.AppendLine($"#define SETUP_UI_FONT_GLYPH_WIDTH {setupUiFontAtlas.GlyphWidth}u");
        builder.AppendLine($"#define SETUP_UI_FONT_GLYPH_HEIGHT {setupUiFontAtlas.GlyphHeight}u");
        builder.AppendLine();

        AppendRgbArray(builder, "gSplashPixels", "SPLASH_WIDTH * SPLASH_HEIGHT * 3u", splashImage.Pixels);
        builder.AppendLine();
        AppendRgbArray(builder, "gCursorPixels", "CURSOR_WIDTH * CURSOR_HEIGHT * 3u", cursorImage.Pixels);
        builder.AppendLine();
        AppendByteArray(builder, "gCursorAlpha", "CURSOR_WIDTH * CURSOR_HEIGHT", cursorImage.Alpha ?? Enumerable.Repeat((byte)255, cursorImage.Pixels.Length).ToArray());
        builder.AppendLine();
        AppendRgbArray(builder, "gSetupIconPixels", "SETUP_ICON_WIDTH * SETUP_ICON_HEIGHT * 3u", setupIconImage.Pixels);
        builder.AppendLine();
        AppendByteArray(builder, "gSetupIconAlpha", "SETUP_ICON_WIDTH * SETUP_ICON_HEIGHT", setupIconImage.Alpha ?? Enumerable.Repeat((byte)255, setupIconImage.Pixels.Length).ToArray());
        builder.AppendLine();
        AppendByteArray(builder, "gFontGlyphAlpha", "FONT_GLYPH_COUNT * FONT_GLYPH_WIDTH * FONT_GLYPH_HEIGHT", fontAtlas.Alpha);
        builder.AppendLine();
        AppendByteArray(builder, "gFontGlyphAdvance", "FONT_GLYPH_COUNT", fontAtlas.Advances);
        builder.AppendLine();
        AppendByteArray(builder, "gIconFontGlyphAlpha", "ICON_FONT_GLYPH_COUNT * ICON_FONT_GLYPH_WIDTH * ICON_FONT_GLYPH_HEIGHT", iconFontAtlas.Alpha);
        builder.AppendLine();
        AppendByteArray(builder, "gIconFontGlyphAdvance", "ICON_FONT_GLYPH_COUNT", iconFontAtlas.Advances);
        builder.AppendLine();
        AppendByteArray(builder, "gSetupTitleFontGlyphAlpha", "SETUP_TITLE_FONT_GLYPH_COUNT * SETUP_TITLE_FONT_GLYPH_WIDTH * SETUP_TITLE_FONT_GLYPH_HEIGHT", setupTitleFontAtlas.Alpha);
        builder.AppendLine();
        AppendByteArray(builder, "gSetupTitleFontGlyphAdvance", "SETUP_TITLE_FONT_GLYPH_COUNT", setupTitleFontAtlas.Advances);
        builder.AppendLine();
        AppendByteArray(builder, "gSetupUiFontGlyphAlpha", "SETUP_UI_FONT_GLYPH_COUNT * SETUP_UI_FONT_GLYPH_WIDTH * SETUP_UI_FONT_GLYPH_HEIGHT", setupUiFontAtlas.Alpha);
        builder.AppendLine();
        AppendByteArray(builder, "gSetupUiFontGlyphAdvance", "SETUP_UI_FONT_GLYPH_COUNT", setupUiFontAtlas.Advances);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenLinearIn", "1025", tweenTables.LinearIn);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenLinearOut", "1025", tweenTables.LinearOut);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenLinearInOut", "1025", tweenTables.LinearInOut);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenLinearOutIn", "1025", tweenTables.LinearOutIn);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenSineIn", "1025", tweenTables.SineIn);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenSineOut", "1025", tweenTables.SineOut);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenSineInOut", "1025", tweenTables.SineInOut);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenSineOutIn", "1025", tweenTables.SineOutIn);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenCubicIn", "1025", tweenTables.CubicIn);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenCubicOut", "1025", tweenTables.CubicOut);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenCubicInOut", "1025", tweenTables.CubicInOut);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenCubicOutIn", "1025", tweenTables.CubicOutIn);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenExponentialIn", "1025", tweenTables.ExponentialIn);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenExponentialOut", "1025", tweenTables.ExponentialOut);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenExponentialInOut", "1025", tweenTables.ExponentialInOut);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenExponentialOutIn", "1025", tweenTables.ExponentialOutIn);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenBackIn", "1025", tweenTables.BackIn);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenBackOut", "1025", tweenTables.BackOut);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenBackInOut", "1025", tweenTables.BackInOut);
        builder.AppendLine();
        AppendUShortArray(builder, "gTweenBackOutIn", "1025", tweenTables.BackOutIn);
        builder.AppendLine();
        AppendAsciiString(builder, "gDefaultFontName", fontAtlas.FamilyName);
        builder.AppendLine();
        AppendAsciiString(builder, "gRuntimeSummaryText", runtimeSummaryText);
        builder.AppendLine();
        AppendAsciiString(builder, "gRuntimeVideoText", videoSummaryText);
        builder.AppendLine();
        AppendAsciiString(builder, "gConsoleIconText", consoleIconText);
        builder.AppendLine();
        AppendAsciiString(builder, "gRuntimeRegistryText", runtimeRegistryText);

        File.WriteAllText(path, builder.ToString(), Encoding.ASCII);
    }

    private static string LoadConsoleIconText(string projectRoot)
    {
        var path = Path.Combine(projectRoot, "assets", "console_icon.txt");
        if (!File.Exists(path))
        {
            return "Dragon OS";
        }

        var text = File.ReadAllText(path)
            .Replace("\r\n", "\n")
            .Replace('\r', '\n');

        return text.TrimEnd('\n');
    }

    private static string BuildRuntimeRegistryText(IReadOnlyList<RuntimePackageFile> runtimeFiles)
    {
        var lines = new List<string>();
        foreach (var file in runtimeFiles
            .Where(file => !file.Kind.Equals("MANIFEST", StringComparison.OrdinalIgnoreCase))
            .OrderBy(file => file.Kind, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            var leafName = Path.GetFileName(file.RelativePath.Replace('\\', Path.DirectorySeparatorChar));
            var location = file.IncludeInEfiPartition ? "EFI+ISO" : "ISO";
            var detail = string.IsNullOrWhiteSpace(file.SummaryText) ? "" : $" {file.SummaryText}";
            lines.Add($"{file.Kind,-3} {leafName} [{location}]{detail}");
        }

        return lines.Count == 0 ? "No runtime assets packaged." : string.Join("\n", lines);
    }

    private static void AppendRgbArray(StringBuilder builder, string name, string lengthExpression, Rgb24[] pixels)
    {
        builder.Append("static const UINT8 ");
        builder.Append(name);
        builder.Append('[');
        builder.Append(lengthExpression);
        builder.AppendLine("] = {");

        for (var i = 0; i < pixels.Length; i++)
        {
            var pixel = pixels[i];
            builder.Append("    ");
            builder.Append(pixel.R);
            builder.Append(", ");
            builder.Append(pixel.G);
            builder.Append(", ");
            builder.Append(pixel.B);

            if (i != pixels.Length - 1)
            {
                builder.Append(',');
            }

            builder.AppendLine();
        }

        builder.AppendLine("};");
    }

    private static void AppendByteArray(StringBuilder builder, string name, string lengthExpression, byte[] values)
    {
        builder.Append("static const UINT8 ");
        builder.Append(name);
        builder.Append('[');
        builder.Append(lengthExpression);
        builder.AppendLine("] = {");

        for (var i = 0; i < values.Length; i++)
        {
            builder.Append("    ");
            builder.Append(values[i]);

            if (i != values.Length - 1)
            {
                builder.Append(',');
            }

            builder.AppendLine();
        }

        builder.AppendLine("};");
    }

    private static void AppendUShortArray(StringBuilder builder, string name, string lengthExpression, ushort[] values)
    {
        builder.Append("static const UINT16 ");
        builder.Append(name);
        builder.Append('[');
        builder.Append(lengthExpression);
        builder.AppendLine("] = {");

        for (var i = 0; i < values.Length; i++)
        {
            builder.Append("    ");
            builder.Append(values[i]);

            if (i != values.Length - 1)
            {
                builder.Append(',');
            }

            builder.AppendLine();
        }

        builder.AppendLine("};");
    }

    private static void AppendAsciiString(StringBuilder builder, string name, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        builder.Append("static const char ");
        builder.Append(name);
        builder.Append('[');
        builder.Append(bytes.Length + 1);
        builder.AppendLine("] = {");

        for (var i = 0; i < bytes.Length; i++)
        {
            builder.Append("    ");
            builder.Append(bytes[i]);
            builder.AppendLine(",");
        }

        builder.AppendLine("    0");
        builder.AppendLine("};");
    }

    private static ToolchainPaths FindToolchain()
    {
        var vsRoots = new[]
        {
            @"C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC",
            @"C:\Program Files\Microsoft Visual Studio\2022\BuildTools\VC\Tools\MSVC",
            @"C:\Program Files\Microsoft Visual Studio\18\Insiders\VC\Tools\MSVC"
        };

        foreach (var root in vsRoots)
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            var versionDir = new DirectoryInfo(root)
                .GetDirectories()
                .OrderByDescending(d => d.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (versionDir is null)
            {
                continue;
            }

            var binDir = Path.Combine(versionDir.FullName, "bin", "Hostx64", "x64");
            var clPath = Path.Combine(binDir, "cl.exe");
            var linkPath = Path.Combine(binDir, "link.exe");

            if (File.Exists(clPath) && File.Exists(linkPath))
            {
                return new ToolchainPaths(clPath, linkPath);
            }
        }

        throw new InvalidOperationException("Could not find a usable Visual Studio C toolchain.");
    }

    private static void RunProcess(string fileName, string arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start {fileName}.");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"{Path.GetFileName(fileName)} failed with exit code {process.ExitCode}.{Environment.NewLine}{stdout}{stderr}");
        }
    }

    private sealed record ToolchainPaths(string ClPath, string LinkPath);
    private sealed record TweenTables(
        ushort[] LinearIn,
        ushort[] LinearOut,
        ushort[] LinearInOut,
        ushort[] LinearOutIn,
        ushort[] SineIn,
        ushort[] SineOut,
        ushort[] SineInOut,
        ushort[] SineOutIn,
        ushort[] CubicIn,
        ushort[] CubicOut,
        ushort[] CubicInOut,
        ushort[] CubicOutIn,
        ushort[] ExponentialIn,
        ushort[] ExponentialOut,
        ushort[] ExponentialInOut,
        ushort[] ExponentialOutIn,
        ushort[] BackIn,
        ushort[] BackOut,
        ushort[] BackInOut,
        ushort[] BackOutIn);

    private sealed record FontAtlas(string FamilyName, int FirstChar, int GlyphCount, int GlyphWidth, int GlyphHeight, byte[] Alpha, byte[] Advances);

    private static class FontAtlasBuilder
    {
        private const int FirstChar = 32;
        private const int LastChar = 126;

        public static FontAtlas Build(string projectRoot, string preferredFamilyName, float fontSize, int glyphWidth, int glyphHeight, FontStyle fontStyle)
        {
            using var bitmap = new Bitmap(glyphWidth, glyphHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            graphics.PageUnit = GraphicsUnit.Pixel;

            using var privateFonts = TryLoadPrivateFonts(projectRoot, preferredFamilyName, fontStyle);
            var installedFamilyName = ResolveFamilyName(preferredFamilyName);
            using var font = privateFonts is not null && privateFonts.Families.Length > 0
                ? new Font(privateFonts.Families[0], fontSize, fontStyle, GraphicsUnit.Pixel)
                : new Font(installedFamilyName, fontSize, fontStyle, GraphicsUnit.Pixel);
            using var brush = new SolidBrush(Color.White);
            using var format = new StringFormat(StringFormat.GenericTypographic)
            {
                FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.MeasureTrailingSpaces
            };

            var glyphCount = LastChar - FirstChar + 1;
            var alpha = new byte[glyphCount * glyphWidth * glyphHeight];
            var advances = new byte[glyphCount];

            for (var codePoint = FirstChar; codePoint <= LastChar; codePoint++)
            {
                var glyphIndex = codePoint - FirstChar;
                var glyph = ((char)codePoint).ToString();
                graphics.Clear(Color.Transparent);
                graphics.DrawString(glyph, font, brush, new PointF(0f, 0f), format);

                var minAdvance = glyphWidth <= 8 ? 1 : 4;
                var advance = Math.Clamp((int)Math.Ceiling(graphics.MeasureString(glyph, font, glyphWidth, format).Width), minAdvance, glyphWidth);
                advances[glyphIndex] = (byte)advance;

                for (var y = 0; y < glyphHeight; y++)
                {
                    for (var x = 0; x < glyphWidth; x++)
                    {
                        var color = bitmap.GetPixel(x, y);
                        alpha[(glyphIndex * glyphWidth * glyphHeight) + (y * glyphWidth) + x] = color.A;
                    }
                }
            }

            return new FontAtlas(font.FontFamily.Name, FirstChar, glyphCount, glyphWidth, glyphHeight, alpha, advances);
        }

        private static string ResolveFamilyName(string preferredFamilyName)
        {
            using var fonts = new InstalledFontCollection();
            if (fonts.Families.Any(f => f.Name.Equals(preferredFamilyName, StringComparison.OrdinalIgnoreCase)))
            {
                return preferredFamilyName;
            }

            if (fonts.Families.Any(f => f.Name.Equals("Ubuntu Mono", StringComparison.OrdinalIgnoreCase)))
            {
                return "Ubuntu Mono";
            }

            return preferredFamilyName;
        }

        private static PrivateFontCollection? TryLoadPrivateFonts(string projectRoot, string preferredFamilyName, FontStyle fontStyle)
        {
            var fontsRoot = Path.Combine(projectRoot, "assets", "fonts");
            string? fileName;

            if (!Directory.Exists(fontsRoot))
            {
                return null;
            }

            fileName = preferredFamilyName.Equals("Ubuntu Mono", StringComparison.OrdinalIgnoreCase)
                ? "UbuntuMono-R.ttf"
                : preferredFamilyName.Equals("Ubuntu", StringComparison.OrdinalIgnoreCase)
                    ? (fontStyle & FontStyle.Bold) != 0
                        ? "Ubuntu-B.ttf"
                        : "Ubuntu-R.ttf"
                    : null;
            if (fileName is null)
            {
                return null;
            }

            var fontPath = Directory.EnumerateFiles(fontsRoot, fileName, SearchOption.AllDirectories)
                .FirstOrDefault(path => !Path.GetFileName(path).StartsWith("._", StringComparison.Ordinal));
            if (fontPath is null)
            {
                return null;
            }

            var privateFonts = new PrivateFontCollection();
            privateFonts.AddFontFile(fontPath);
            return privateFonts;
        }
    }

    private static class TweenTableBuilder
    {
        public static TweenTables Build()
        {
            return new TweenTables(
                BuildTable(t => t),
                BuildTable(t => t),
                BuildTable(t => t),
                BuildTable(t => t),
                BuildTable(SineIn),
                BuildTable(SineOut),
                BuildTable(SineInOut),
                BuildTable(t => Piecewise(SineOut, SineIn, t)),
                BuildTable(CubicIn),
                BuildTable(CubicOut),
                BuildTable(CubicInOut),
                BuildTable(t => Piecewise(CubicOut, CubicIn, t)),
                BuildTable(ExponentialIn),
                BuildTable(ExponentialOut),
                BuildTable(ExponentialInOut),
                BuildTable(t => Piecewise(ExponentialOut, ExponentialIn, t)),
                BuildTable(BackIn),
                BuildTable(BackOut),
                BuildTable(BackInOut),
                BuildTable(t => Piecewise(BackOut, BackIn, t)));
        }

        private static ushort[] BuildTable(Func<double, double> easing)
        {
            var values = new ushort[1025];
            for (var i = 0; i <= 1024; i++)
            {
                var t = i / 1024d;
                var value = Math.Clamp(easing(t), 0d, 1d);
                values[i] = (ushort)Math.Round(value * 1024d);
            }

            return values;
        }

        private static double Piecewise(Func<double, double> firstHalf, Func<double, double> secondHalf, double t) =>
            t < 0.5d
                ? firstHalf(t * 2d) * 0.5d
                : 0.5d + (secondHalf((t - 0.5d) * 2d) * 0.5d);

        private static double SineIn(double t) => 1d - Math.Cos((t * Math.PI) / 2d);

        private static double SineOut(double t) => Math.Sin((t * Math.PI) / 2d);

        private static double SineInOut(double t) => -(Math.Cos(Math.PI * t) - 1d) / 2d;

        private static double CubicIn(double t) => t * t * t;

        private static double CubicOut(double t) => 1d - Math.Pow(1d - t, 3d);

        private static double CubicInOut(double t) =>
            t < 0.5d
                ? 4d * t * t * t
                : 1d - Math.Pow(-2d * t + 2d, 3d) / 2d;

        private static double ExponentialIn(double t) => t == 0d ? 0d : Math.Pow(2d, 10d * t - 10d);

        private static double ExponentialOut(double t) => t == 1d ? 1d : 1d - Math.Pow(2d, -10d * t);

        private static double ExponentialInOut(double t)
        {
            if (t == 0d)
            {
                return 0d;
            }

            if (t == 1d)
            {
                return 1d;
            }

            return t < 0.5d
                ? Math.Pow(2d, 20d * t - 10d) / 2d
                : (2d - Math.Pow(2d, -20d * t + 10d)) / 2d;
        }

        private static double BackIn(double t)
        {
            const double c1 = 1.70158d;
            const double c3 = c1 + 1d;
            return c3 * t * t * t - c1 * t * t;
        }

        private static double BackOut(double t)
        {
            const double c1 = 1.70158d;
            const double c3 = c1 + 1d;
            return 1d + c3 * Math.Pow(t - 1d, 3d) + c1 * Math.Pow(t - 1d, 2d);
        }

        private static double BackInOut(double t)
        {
            const double c1 = 1.70158d;
            const double c2 = c1 * 1.525d;
            return t < 0.5d
                ? (Math.Pow(2d * t, 2d) * (((c2 + 1d) * 2d * t) - c2)) / 2d
                : (Math.Pow(2d * t - 2d, 2d) * (((c2 + 1d) * (t * 2d - 2d)) + c2) + 2d) / 2d;
        }
    }
}
