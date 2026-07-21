# Recolor Palette

Plugins Paint.NET pour recoloriser une texture en transférant la palette d'une image source vers une image cible.

## Fonctionnement

1. Ouvrir l'image **palette** → **Effets → Recolor → Prendre la palette**
2. Ouvrir l'image **base** → **Effets → Recolor → Appliquer la palette**
3. Le résultat s'ouvre dans un **nouvel onglet** Paint.NET (l'image base reste inchangée)

La palette est mémorisée entre les deux étapes dans :

`%LocalAppData%\RecolorPalette\palette.bin`

## Prérequis

- [Paint.NET](https://www.getpaint.net/) **5.2 ou plus récent**
- Windows

Pour compiler les plugins vous-même :

- [CodeLab](https://boltbait.com/pdn/codelab/) installé dans Paint.NET

## Installation

### Option A — Release (recommandé)

1. Télécharger la dernière [Release](https://github.com/ClementDaguenet/RecolorPalette/releases)
2. Extraire les fichiers
3. Lancer **`Install_All.bat`** (clic droit → Exécuter en tant qu'administrateur si besoin)
4. Redémarrer Paint.NET

Les effets apparaissent dans **Effets → Recolor**.

### Option B — Installation manuelle

Copier les deux DLL dans le dossier Effects de Paint.NET :

```
C:\Program Files\paint.net\Effects\
├── RecolorTakePalette.dll
└── RecolorApplyPalette.dll
```

Version Microsoft Store : copier plutôt dans :

```
Documents\paint.net App Files\Effects\
```

### Option C — Compiler depuis les sources

1. Ouvrir Paint.NET → **Effets → Advanced → CodeLab**
2. **File → Open** → ouvrir `RecolorTakePalette.cs`
3. **File → Build DLL** (Ctrl+B)
4. Répéter pour `RecolorApplyPalette.cs`
5. Redémarrer Paint.NET

Les DLL sont générées dans le dossier Effects de Paint.NET.

## Fichiers du dépôt

| Fichier | Description |
|---------|-------------|
| `RecolorTakePalette.cs` | Source CodeLab — enregistre la palette |
| `RecolorApplyPalette.cs` | Source CodeLab — applique la palette |
| `Install_All.bat` | Installe les deux plugins d'un coup |
| `Install_RecolorTakePalette.bat` | Installe uniquement « Prendre la palette » |
| `Install_RecolorApplyPalette.bat` | Installe uniquement « Appliquer la palette » |

## Algorithme

- Extraction des couleurs uniques non transparentes, triées par luminosité
- Correspondance palette → palette par index proportionnel
- Remplacement de chaque pixel par la couleur la plus proche dans la palette de base

## Publier une release sur GitHub

Après avoir compilé les DLL avec CodeLab, copiez-les à la racine du dépôt puis :

```bash
git add RecolorTakePalette.dll RecolorApplyPalette.dll
git commit -m "Add compiled plugins v1.0"
git tag v1.0
git push origin main --tags
```

Sur GitHub : **Releases → Draft a new release** → choisir le tag `v1.0` → joindre les DLL et les scripts `.bat`.

## Licence

MIT — voir [LICENSE](LICENSE).

## Auteur

WoXayZ / [ClementDaguenet](https://github.com/ClementDaguenet)
