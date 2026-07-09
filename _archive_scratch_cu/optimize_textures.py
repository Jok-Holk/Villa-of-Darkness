"""
optimize_textures.py
Chạy: python optimize_textures.py
Không cần mở Unity. Khi mở Unity lần sau nó tự reimport.

Áp dụng:
- COLOR textures  → maxSize 1024, crunch compression
- NORMAL maps     → maxSize 1024, crunch off (normal cần chính xác)
- ORM/AO/Rough    → maxSize 512,  crunch compression
- Tất cả          → streamingMipmaps on
- Kenney preview  → maxSize 128   (chỉ thumbnail, không cần render)
"""

import os, re, sys

ROOT = "Assets/_Project"
DRY_RUN = "--dry" in sys.argv  # chạy thử: python optimize_textures.py --dry

# ─── helpers ──────────────────────────────────────────────────────────────────

def is_normal(path):
    n = os.path.basename(path).lower()
    return any(x in n for x in ["normal", "nrm", "normalgl", "normalmap", "_n."])

def is_orm(path):
    n = os.path.basename(path).lower()
    return any(x in n for x in ["orm", "ao", "rough", "metal", "occlusion", "mask"])

def is_kenney_preview(path):
    return "kenney_furniture" in path.replace("\\", "/") and \
           any(x in path.lower() for x in ["isometric", "preview", "sample"])

def patch_field(text, field, new_value):
    """Replace 'field: <any>' with 'field: new_value'"""
    pattern = rf"(\b{re.escape(field)}: )\S+"
    replacement = rf"\g<1>{new_value}"
    return re.sub(pattern, replacement, text)

def process_meta(path, max_size, crunch, streaming=1):
    with open(path, "r", encoding="utf-8", errors="ignore") as f:
        original = f.read()

    if "TextureImporter" not in original:
        return False

    text = original

    # maxTextureSize — xuất hiện nhiều lần (per-platform), đổi tất cả
    text = re.sub(r"(maxTextureSize: )\d+", rf"\g<1>{max_size}", text)

    # streamingMipmaps
    text = patch_field(text, "streamingMipmaps", streaming)

    # crunchedCompression trong platformSettings blocks
    text = re.sub(r"(crunchedCompression: )\d", rf"\g<1>{1 if crunch else 0}", text)

    if text == original:
        return False  # nothing changed

    if not DRY_RUN:
        with open(path, "w", encoding="utf-8") as f:
            f.write(text)
    return True

# ─── main ─────────────────────────────────────────────────────────────────────

changed = {"color": 0, "normal": 0, "orm": 0, "preview": 0, "skip": 0}

for dirpath, _, files in os.walk(ROOT):
    for fname in files:
        if not fname.endswith(".meta"):
            continue
        full = os.path.join(dirpath, fname)

        if is_kenney_preview(full):
            ok = process_meta(full, max_size=128, crunch=True, streaming=0)
            if ok: changed["preview"] += 1
        elif is_normal(full):
            ok = process_meta(full, max_size=1024, crunch=False, streaming=1)
            if ok: changed["normal"] += 1
        elif is_orm(full):
            ok = process_meta(full, max_size=512, crunch=True, streaming=1)
            if ok: changed["orm"] += 1
        else:
            ok = process_meta(full, max_size=1024, crunch=True, streaming=1)
            if ok: changed["color"] += 1

total = sum(changed.values())
mode = "[DRY RUN] " if DRY_RUN else ""
print(f"{mode}Texture .meta files updated: {total}")
print(f"  Color/diffuse  → 1024px crunch : {changed['color']}")
print(f"  Normal maps    → 1024px no-crunch: {changed['normal']}")
print(f"  ORM/AO/Rough   → 512px crunch  : {changed['orm']}")
print(f"  Kenney previews→ 128px          : {changed['preview']}")
print()
if DRY_RUN:
    print("Dry run — không có file nào bị thay đổi. Bỏ --dry để apply thật.")
else:
    print("Done. Mở Unity → nó tự reimport tất cả. Lần mở đầu sẽ lâu hơn bình thường.")
    print("VRAM texture dự kiến giảm ~60-70% so với 2048px.")
