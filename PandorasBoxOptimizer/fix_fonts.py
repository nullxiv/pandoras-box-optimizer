import os
from fontTools.ttLib import TTFont

# file -> (clean family, weightClass)
PLAN = {
    "Manrope-Regular.ttf":  ("Manrope", 400),
    "Manrope-Medium.ttf":   ("Manrope Medium", 500),
    "Manrope-SemiBold.ttf": ("Manrope SemiBold", 600),
    "Manrope-Bold.ttf":     ("Manrope Bold", 700),
    "Bricolage-Bold.ttf":   ("Bricolage Grotesque", 800),
}
AD = os.path.join(os.path.dirname(os.path.abspath(__file__)), "assets")

for fn, (fam, wght) in PLAN.items():
    p = os.path.join(AD, fn)
    t = TTFont(p)
    name = t["name"]
    for nid, val in [(1, fam), (2, "Regular"), (4, fam), (6, fam.replace(" ", "")),
                     (16, fam), (17, "Regular")]:
        name.setName(val, nid, 3, 1, 0x409)   # Windows Unicode
        name.setName(val, nid, 1, 0, 0)        # Mac Roman
    # Force "Regular" style so WPF never synth-bolds; weight carried by family name
    t["OS/2"].usWeightClass = 400
    t["OS/2"].fsSelection = (t["OS/2"].fsSelection & ~0b100001) | 0x40  # clear ITALIC/BOLD, set REGULAR
    t["head"].macStyle = 0
    t.save(p)
    print("fixed", fn, "->", fam)
print("DONE")
