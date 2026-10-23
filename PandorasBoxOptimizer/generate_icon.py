import os
from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
AD = os.path.join(HERE, "assets")

# symbol.png is the brand emblem (512, with alpha). Recolor it WHITE via its alpha.
sym = Image.open(os.path.join(AD, "symbol.png")).convert("RGBA")
a = sym.split()[3]
size = sym.size[0]
w = Image.new("L", sym.size, 255)
white = Image.merge("RGBA", (w, w, w, a))

# Dark rounded tile so the white emblem is visible on any background (taskbar, light explorer).
tile = Image.new("RGBA", (size, size), (0, 0, 0, 0))
d = ImageDraw.Draw(tile)
rad = int(size * 0.20)
d.rounded_rectangle([0, 0, size - 1, size - 1], radius=rad, fill=(14, 16, 21, 255))       # #0E1015
d.rounded_rectangle([0, 0, size - 1, size - 1], radius=rad, outline=(42, 47, 58, 255), width=max(2, size // 150))
tile.alpha_composite(white, (0, 0))

tile.save(os.path.join(AD, "app.ico"), sizes=[(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)])
tile.convert("RGB").save(os.path.join(HERE, "..", "icon_preview.png"))
print("app.ico regenerated (white emblem on dark tile)")
