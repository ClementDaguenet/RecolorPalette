// Name: RecolorApplyPalette
// Submenu: Recolor
// Author: WoXayZ
// Title: Appliquer la palette
// Version: 1.0
// Desc: Applique la palette enregistrée sur l'image ouverte dans un nouvel onglet Paint.NET.
// Force Single Render Call

#region UICode
#endregion

private static readonly string PaletteDirectory = Path.Combine(
    System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
    "RecolorPalette");

private static readonly string PaletteFilePath = Path.Combine(PaletteDirectory, "palette.bin");

private static float GetBrightness(ColorBgra32 color)
{
    return 0.2126f * color.R + 0.7152f * color.G + 0.0722f * color.B;
}

private static uint ColorKey(ColorBgra32 color)
{
    return (uint)((color.A << 24) | (color.B << 16) | (color.G << 8) | color.R);
}

private static double ColorDistanceSq(ColorBgra32 c1, ColorBgra32 c2)
{
    double dr = (double)c1.R - c2.R;
    double dg = (double)c1.G - c2.G;
    double db = (double)c1.B - c2.B;
    return dr * dr + dg * dg + db * db;
}

private static List<ColorBgra32> ExtractSortedPalette(RegionPtr<ColorBgra32> region, SizeInt32 size)
{
    HashSet<uint> seen = new HashSet<uint>();
    List<ColorBgra32> colors = new List<ColorBgra32>();

    for (int y = 0; y < size.Height; y++)
    {
        for (int x = 0; x < size.Width; x++)
        {
            ColorBgra32 pixel = region[x, y];
            if (pixel.A == 0)
            {
                continue;
            }

            uint key = ColorKey(pixel);
            if (seen.Add(key))
            {
                colors.Add(pixel);
            }
        }
    }

    colors.Sort((a, b) => GetBrightness(a).CompareTo(GetBrightness(b)));
    return colors;
}

private static List<ColorBgra32> LoadPalette()
{
    if (!File.Exists(PaletteFilePath))
    {
        return null;
    }

    List<ColorBgra32> palette = new List<ColorBgra32>();

    using (FileStream stream = new FileStream(PaletteFilePath, FileMode.Open, FileAccess.Read))
    using (BinaryReader reader = new BinaryReader(stream))
    {
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            byte b = reader.ReadByte();
            byte g = reader.ReadByte();
            byte r = reader.ReadByte();
            byte a = reader.ReadByte();
            palette.Add(new ColorBgra32(b, g, r, a));
        }
    }

    return palette;
}

private static Dictionary<uint, ColorBgra32> BuildMapping(List<ColorBgra32> srcPalette, List<ColorBgra32> tgtPalette)
{
    Dictionary<uint, ColorBgra32> mapping = new Dictionary<uint, ColorBgra32>();
    int nSrc = srcPalette.Count;
    int nTgt = tgtPalette.Count;

    for (int i = 0; i < nTgt; i++)
    {
        int idxSrc = nTgt > 1 ? (int)((long)i * (nSrc - 1) / (nTgt - 1)) : 0;
        mapping[ColorKey(tgtPalette[i])] = srcPalette[idxSrc];
    }

    return mapping;
}

private static ColorBgra32 FindNearestPaletteColor(ColorBgra32 pixel, List<ColorBgra32> palette)
{
    ColorBgra32 best = palette[0];
    double bestDistance = ColorDistanceSq(pixel, best);

    for (int i = 1; i < palette.Count; i++)
    {
        double distance = ColorDistanceSq(pixel, palette[i]);
        if (distance < bestDistance)
        {
            bestDistance = distance;
            best = palette[i];
        }
    }

    return best;
}

private static ColorBgra32 MapPixel(ColorBgra32 pixel, List<ColorBgra32> tgtPalette, Dictionary<uint, ColorBgra32> mapping)
{
    if (pixel.A == 0)
    {
        return pixel;
    }

    ColorBgra32 nearest = FindNearestPaletteColor(pixel, tgtPalette);
    return mapping[ColorKey(nearest)];
}

private static string FindPaintDotNetExe()
{
    string programFiles = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
    string[] candidates = new string[]
    {
        Path.Combine(programFiles, "paint.net", "PaintDotNet.exe"),
        Path.Combine(programFiles, "Paint.NET", "PaintDotNet.exe"),
    };

    for (int i = 0; i < candidates.Length; i++)
    {
        if (File.Exists(candidates[i]))
        {
            return candidates[i];
        }
    }

    return null;
}

[System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
private static extern System.IntPtr ShellExecute(
    System.IntPtr hwnd,
    string lpOperation,
    string lpFile,
    string lpParameters,
    string lpDirectory,
    int nShowCmd);

protected override void OnRender(IBitmapEffectOutput output)
{
    using IEffectInputBitmap<ColorBgra32> sourceBitmap = Environment.GetSourceBitmapBgra32();
    SizeInt32 imageSize = sourceBitmap.Size;

    using IBitmapLock<ColorBgra32> sourceLock = sourceBitmap.Lock(new RectInt32(0, 0, imageSize));
    RegionPtr<ColorBgra32> sourceRegion = sourceLock.AsRegionPtr();

    List<ColorBgra32> srcPalette = LoadPalette();
    if (srcPalette == null || srcPalette.Count == 0)
    {
        System.Windows.Forms.MessageBox.Show(
            "Aucune palette enregistrée.\n\nOuvrez d'abord l'image palette et utilisez « Prendre la palette ».",
            "Appliquer la palette",
            System.Windows.Forms.MessageBoxButtons.OK,
            System.Windows.Forms.MessageBoxIcon.Warning);
    }
    else
    {
        List<ColorBgra32> tgtPalette = ExtractSortedPalette(sourceRegion, imageSize);
        if (tgtPalette.Count == 0)
        {
            System.Windows.Forms.MessageBox.Show(
                "L'image ouverte ne contient aucune couleur visible.",
                "Appliquer la palette",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Warning);
        }
        else
        {
            Dictionary<uint, ColorBgra32> mapping = BuildMapping(srcPalette, tgtPalette);
            ColorBgra32[] recolored = new ColorBgra32[imageSize.Width * imageSize.Height];

            for (int y = 0; y < imageSize.Height; y++)
            {
                for (int x = 0; x < imageSize.Width; x++)
                {
                    recolored[y * imageSize.Width + x] = MapPixel(sourceRegion[x, y], tgtPalette, mapping);
                }
            }

            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(imageSize.Width, imageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                for (int y = 0; y < imageSize.Height; y++)
                {
                    for (int x = 0; x < imageSize.Width; x++)
                    {
                        ColorBgra32 pixel = recolored[y * imageSize.Width + x];
                        bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B));
                    }
                }

                string tempPath = Path.Combine(Path.GetTempPath(), "recolor_" + Guid.NewGuid().ToString("N") + ".png");
                bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

                string paintDotNetExe = FindPaintDotNetExe();
                if (paintDotNetExe == null)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Image recolorisée enregistrée ici :\n" + tempPath + "\n\nPaint.NET n'a pas été trouvé pour l'ouvrir automatiquement.",
                        "Appliquer la palette",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Information);
                }
                else
                {
                    string paintDotNetDir = Path.GetDirectoryName(paintDotNetExe);
                    ShellExecute(
                        System.IntPtr.Zero,
                        "open",
                        paintDotNetExe,
                        "\"" + tempPath + "\"",
                        paintDotNetDir,
                        1);
                }
            }
        }
    }

    RectInt32 outputBounds = output.Bounds;
    using IBitmapLock<ColorBgra32> outputLock = output.LockBgra32();
    RegionPtr<ColorBgra32> outputSubRegion = outputLock.AsRegionPtr();
    var outputRegion = outputSubRegion.OffsetView(-outputBounds.Location);

    for (int y = outputBounds.Top; y < outputBounds.Bottom; y++)
    {
        if (IsCancelRequested)
        {
            return;
        }

        for (int x = outputBounds.Left; x < outputBounds.Right; x++)
        {
            outputRegion[x, y] = sourceRegion[x, y];
        }
    }
}
