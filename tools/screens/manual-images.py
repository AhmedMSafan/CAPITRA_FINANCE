# One-off: the manual's screenshots (PNG, EN-/AR- prefixed) as JPEGs in <manual>/img/en and img/ar, empty page bottom trimmed;
# the sign-in screens cropped to their card.
import glob, os, sys
from PIL import Image, ImageChops

src, dest = sys.argv[1], sys.argv[2]
for lang in ('en', 'ar'):
    os.makedirs(os.path.join(dest, 'img', lang), exist_ok=True)
for f in sorted(glob.glob(os.path.join(src, '*-m*.png'))):
    lang, name = os.path.basename(f)[:-4].split('-', 1)
    im = Image.open(f).convert('RGB')
    w, h = im.size
    if name[:3] in ('m13', 'm14', 'm15', 'm16'):
        # the white card on the dark background, with a margin of the background around it
        white = im.convert('L').point(lambda v: 255 if v > 235 else 0)
        x0, y0, x1, y1 = white.getbbox()
        im = im.crop((max(0, x0 - 60), max(0, y0 - 40), min(w, x1 + 60), min(h, y1 + 40)))
    else:
        bg = im.getpixel((w // 2, h - 3))
        diff = ImageChops.difference(im, Image.new('RGB', im.size, bg)).convert('L').point(lambda v: 255 if v > 12 else 0)
        box = diff.getbbox()
        im = im.crop((0, 0, w, max(300, min(h, (box[3] if box else h) + 28))))
    im.save(os.path.join(dest, 'img', lang.lower(), name + '.jpg'), quality=84, optimize=True, progressive=True)
    print(lang, name, im.size)
