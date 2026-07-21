# Recolor Palette

Paint.NET plugins to recolor a texture by transferring the palette from a source image to a target image.

## How it works

1. Open the **palette** image → **Effects → Recolor → Take palette**
2. Open the **base** image → **Effects → Recolor → Apply palette**
3. The result opens in a **new Paint.NET tab** (the base image stays unchanged)

The palette is stored between the two steps at:

`%LocalAppData%\RecolorPalette\palette.bin`

## Requirements

- [Paint.NET](https://www.getpaint.net/) **5.2 or newer**
- Windows

To build the plugins yourself:

- [CodeLab](https://boltbait.com/pdn/codelab/) installed in Paint.NET

## Installation

### Option A - Release (recommended)

1. Download the latest [Release](https://github.com/ClementDaguenet/RecolorPalette/releases)
2. Extract the files
3. Run **`Install_All.bat`** (right-click → Run as administrator if needed)
4. Restart Paint.NET

The effects appear under **Effects → Recolor**.

### Option B - Manual installation

Copy both DLLs into the Paint.NET Effects folder:

```
C:\Program Files\paint.net\Effects\
├── RecolorTakePalette.dll
└── RecolorApplyPalette.dll
```

Microsoft Store version: copy to:

```
Documents\paint.net App Files\Effects\
```

### Option C - Build from source

1. Open Paint.NET → **Effects → Advanced → CodeLab**
2. **File → Open** → open `RecolorTakePalette.cs`
3. **File → Build DLL** (Ctrl+B)
4. Repeat for `RecolorApplyPalette.cs`
5. Restart Paint.NET

The DLLs are generated in Paint.NET’s Effects folder.

## Repository files

| File | Description |
|------|-------------|
| `RecolorTakePalette.cs` | CodeLab source - saves the palette |
| `RecolorApplyPalette.cs` | CodeLab source - applies the palette |
| `Install_All.bat` | Installs both plugins at once |
| `Install_RecolorTakePalette.bat` | Installs **Take palette** only |
| `Install_RecolorApplyPalette.bat` | Installs **Apply palette** only |

## Algorithm

- Extract unique non-transparent colors, sorted by brightness
- Map palette to palette by proportional index
- Replace each pixel with the nearest color in the base palette

## Publishing a GitHub release

After building the DLLs with CodeLab, copy them to the repository root, then:

```bash
git add RecolorTakePalette.dll RecolorApplyPalette.dll
git commit -m "Add compiled plugins v1.0"
git tag v1.0
git push origin main --tags
```

On GitHub: **Releases → Draft a new release** → select tag `v1.0` → attach the DLLs and `.bat` scripts.

## License

MIT - see [LICENSE](LICENSE).

## Author

WoXayZ / [ClementDaguenet](https://github.com/ClementDaguenet)
