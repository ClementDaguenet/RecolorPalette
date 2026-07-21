from PIL import Image
import numpy as np
import argparse

def get_sorted_palette(img):
    """
    Prend une image PIL, retourne une liste de couleurs uniques triees par luminosite.
    """
    arr = np.array(img.convert("RGBA"))
    unique = set()
    h, w, _ = arr.shape
    for y in range(h):
        for x in range(w):
            rgba = tuple(arr[y, x])
            if rgba[3] == 0:
                continue
            unique.add(rgba)
    def brightness(rgba):
        r, g, b, a = rgba
        return 0.2126 * r + 0.7152 * g + 0.0722 * b
    sorted_pal = sorted(unique, key=brightness)
    return sorted_pal

def build_palette_image(sorted_pal):
    """
    Prend une palette (liste de tuples RGBA), cree une image d'une ligne.
    """
    width = len(sorted_pal)
    img = Image.new("RGBA", (width, 1))
    img.putdata(sorted_pal)
    return img

def map_colors(target_img, src_palette, tgt_palette):
    """
    Remplace chaque pixel de target_img (PIL) : 
    - trouve la couleur dans tgt_palette qui correspond le mieux
    - remplace par la couleur correspondante dans src_palette
    Retourne une nouvelle image recolorisee.
    """
    tgt_arr = np.array(target_img.convert("RGBA"))
    h, w, _ = tgt_arr.shape

    n_src = len(src_palette)
    n_tgt = len(tgt_palette)
    mapping = {}
    for i, c in enumerate(tgt_palette):
        idx_src = int(i * (n_src - 1) / (n_tgt - 1)) if n_tgt > 1 else 0
        mapping[c] = src_palette[idx_src]

    def dist2(c1, c2):
        return sum((float(c1[i]) - float(c2[i]))**2 for i in range(3))

    out_arr = np.zeros_like(tgt_arr)
    for y in range(h):
        for x in range(w):
            rgba = tuple(tgt_arr[y, x])
            if rgba[3] == 0:
                out_arr[y, x] = rgba
            else:
                best = None
                best_d = None
                for c in tgt_palette:
                    d = dist2(rgba, c)
                    if best is None or d < best_d:
                        best = c
                        best_d = d
                out_arr[y, x] = mapping[best]
    out_img = Image.fromarray(out_arr, mode="RGBA")
    return out_img

def recolor(input_path, target_path, output_path):
    img_in = Image.open(input_path)
    img_tgt = Image.open(target_path)

    pal_in = get_sorted_palette(img_in)
    pal_tgt = get_sorted_palette(img_tgt)

    img_out = map_colors(img_tgt, pal_in, pal_tgt)
    img_out.save(output_path)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Recolor a target texture using palette from input texture.")
    parser.add_argument("input_path", help="Chemin vers l'image d'entrée (source de palette)")
    parser.add_argument("target_path", help="Chemin vers l'image cible à recoloriser")
    parser.add_argument("output_path", help="Chemin pour enregistrer l'image recolorisée")
    args = parser.parse_args()

    recolor(args.input_path, args.target_path, args.output_path)