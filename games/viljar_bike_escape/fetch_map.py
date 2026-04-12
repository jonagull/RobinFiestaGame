"""
Downloads and stitches OSM tiles for the Robin Bike Escape map.
Run once: python fetch_map.py
Output:   assets/map.png
"""

import requests, math, os
from PIL import Image
from io import BytesIO

def deg2tile(lat, lon, zoom):
    n = 2 ** zoom
    x = int((lon + 180) / 360 * n)
    y = int((1 - math.log(math.tan(math.radians(lat)) + 1 / math.cos(math.radians(lat))) / math.pi) / 2 * n)
    return x, y

ZOOM    = 15
LAT_MIN = 60.310   # ~1.7km south of Robin's home, plenty of room
LAT_MAX = 60.405   # ~3km north of old office
LON_MIN = 5.250    # ~3km west of old west edge, includes Robin's home (5.2749)
LON_MAX = 5.370    # slight east padding

x_min, y_min = deg2tile(LAT_MAX, LON_MIN, ZOOM)
x_max, y_max = deg2tile(LAT_MIN, LON_MAX, ZOOM)

cols = x_max - x_min + 1
rows = y_max - y_min + 1
total = cols * rows
print(f"Downloading {cols}x{rows} = {total} tiles...")

img = Image.new("RGB", (cols * 256, rows * 256))
headers = {"User-Agent": "RobinFiestaGame/1.0 (party game, non-commercial)"}

for i, x in enumerate(range(x_min, x_max + 1)):
    for j, y in enumerate(range(y_min, y_max + 1)):
        url = f"https://tile.openstreetmap.org/{ZOOM}/{x}/{y}.png"
        r = requests.get(url, headers=headers, timeout=10)
        r.raise_for_status()
        tile = Image.open(BytesIO(r.content))
        img.paste(tile, (i * 256, j * 256))
        print(f"  {i*rows+j+1}/{total} ({x},{y})")

os.makedirs("assets", exist_ok=True)
out = "assets/map.png"
img.save(out)
print(f"\nDone! Saved to {out}  ({img.width}x{img.height}px)")
