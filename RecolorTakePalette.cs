// Name: RecolorTakePalette
// Submenu: Recolor
// Author: WoXayZ
// Title: Prendre la palette
// Version: 1.0
// Desc: Enregistre la palette de l'image ouverte pour l'appliquer ensuite sur une autre texture.
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

private static void SavePalette(List<ColorBgra32> palette)
{
    Directory.CreateDirectory(PaletteDirectory);

    using (FileStream stream = new FileStream(PaletteFilePath, FileMode.Create, FileAccess.Write))
    using (BinaryWriter writer = new BinaryWriter(stream))
    {
        writer.Write(palette.Count);
        for (int i = 0; i < palette.Count; i++)
        {
            ColorBgra32 color = palette[i];
            writer.Write(color.B);
            writer.Write(color.G);
            writer.Write(color.R);
            writer.Write(color.A);
        }
    }
}

protected override void OnRender(IBitmapEffectOutput output)
{
    using IEffectInputBitmap<ColorBgra32> sourceBitmap = Environment.GetSourceBitmapBgra32();
    SizeInt32 imageSize = sourceBitmap.Size;

    using IBitmapLock<ColorBgra32> sourceLock = sourceBitmap.Lock(new RectInt32(0, 0, imageSize));
    RegionPtr<ColorBgra32> sourceRegion = sourceLock.AsRegionPtr();

    List<ColorBgra32> palette = ExtractSortedPalette(sourceRegion, imageSize);
    SavePalette(palette);

    System.Windows.Forms.MessageBox.Show(
        string.Format("Palette enregistrée ({0} couleur{1}).\n\nOuvrez maintenant l'image de base et utilisez « Appliquer la palette ».", palette.Count, palette.Count > 1 ? "s" : ""),
        "Prendre la palette",
        System.Windows.Forms.MessageBoxButtons.OK,
        System.Windows.Forms.MessageBoxIcon.Information);

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
