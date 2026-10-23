import os
from fontTools.ttLib import TTFont

p = os.path.join(os.path.dirname(os.path.abspath(__file__)), "assets", "Bricolage-Bold.ttf")
t = TTFont(p)
# Drop variable/optical-size metadata so WPF sees a plain static family name.
for tag in ("fvar", "STAT", "avar", "gvar"):
    if tag in t:
        del t[tag]
name = t["name"]
fam = "Bricolage Grotesque"
# Wipe every existing name record, then set a clean minimal set on both platforms.
name.names = []
for nid, val in [(1, fam), (2, "Regular"), (3, fam), (4, fam), (6, "BricolageGrotesque"), (16, fam), (17, "Regular")]:
    name.setName(val, nid, 3, 1, 0x409)
    name.setName(val, nid, 1, 0, 0)
t["OS/2"].usWeightClass = 400
t["OS/2"].fsSelection = (t["OS/2"].fsSelection & ~0b100001) | 0x40
t["head"].macStyle = 0
t.save(p)
print("rewrote Bricolage name tables ->", fam)
