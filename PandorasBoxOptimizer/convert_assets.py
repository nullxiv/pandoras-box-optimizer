import glob, os, sys
from fontTools.ttLib import TTFont
from PIL import Image

SITE = r"C:/My Web Sites/aa/Pandora-Claude-Project/website/public"
OUT  = os.path.join(os.path.dirname(os.path.abspath(__file__)), "assets")
os.makedirs(OUT, exist_ok=True)

def woff2ttf(pattern, outname):
    files = glob.glob(os.path.join(SITE, "fonts", pattern))
    if not files:
        print("MISSING font:", pattern); return
    f = TTFont(files[0]); f.flavor = None
    f.save(os.path.join(OUT, outname)); print("font ->", outname)

woff2ttf("manrope-latin-400-normal.*.woff2", "Manrope-Regular.ttf")
woff2ttf("manrope-latin-500-normal.*.woff2", "Manrope-Medium.ttf")
woff2ttf("manrope-latin-600-normal.*.woff2", "Manrope-SemiBold.ttf")
woff2ttf("manrope-latin-700-normal.*.woff2", "Manrope-Bold.ttf")
woff2ttf("bricolage-grotesque-latin-700-normal.*.woff2", "Bricolage-Bold.ttf")

logo = Image.open(os.path.join(SITE, "assets/brand/logo-on-dark.webp")).convert("RGBA")
logo.save(os.path.join(OUT, "logo.png")); print("logo ->", logo.size)

sym = Image.open(os.path.join(SITE, "assets/brand/pandora-flat-512.png")).convert("RGBA")
sym.save(os.path.join(OUT, "symbol.png"))
sym.save(os.path.join(OUT, "app.ico"), sizes=[(16,16),(32,32),(48,48),(64,64),(128,128),(256,256)])
print("icon -> app.ico"); print("DONE")
