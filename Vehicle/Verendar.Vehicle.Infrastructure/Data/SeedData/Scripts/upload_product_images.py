#!/usr/bin/env python3
"""
Upload anh part products len S3 va cap nhat cot ImageUrl trong PartProducts.csv
thanh URL CloudFront.

Cau hinh (trung Media API):
- Bucket: verendar, Region: ap-southeast-1
- CloudFront: d3iova6424vljy.cloudfront.net
- S3 prefix: master/part-products

Cach dung:
  pip install boto3
  python upload_product_images.py [--images-dir PATH] [--csv PATH] [--dry-run]

Thu muc anh: mac dinh la thu muc chua script (Products). Trong CSV, ImageUrl la path
relative (vd: image/Oil/Manual/Castrol/xxx.avif) hoac da la URL CloudFront.
Script se tim file local theo path hoac theo ten file (voi cac duoi .png, .jpg, .avif, .webp).
"""

import argparse
import csv
import mimetypes
import os
import sys
from pathlib import Path
from urllib.parse import quote, unquote, urlparse

try:
    import boto3
    from botocore.exceptions import ClientError
except ImportError:
    print("Can cai boto3: pip install boto3", file=sys.stderr)
    sys.exit(1)

BUCKET_NAME = "verendar"
REGION = os.environ.get("AWS_DEFAULT_REGION", "ap-southeast-1")
CLOUDFRONT_DOMAIN = "d3iova6424vljy.cloudfront.net"
S3_KEY_PREFIX = "master/part-products"
DEFAULT_EXTENSIONS = (".png", ".webp", ".jpg", ".jpeg", ".avif", ".svg")


def get_content_type(file_path: Path) -> str:
    ext = file_path.suffix.lower()
    mime = mimetypes.types_map.get(ext)
    if mime:
        return mime
    if ext in (".webp",):
        return "image/webp"
    if ext in (".avif",):
        return "image/avif"
    if ext in (".svg",):
        return "image/svg+xml"
    return "application/octet-stream"


def build_cloudfront_url(s3_key: str) -> str:
    return f"https://{CLOUDFRONT_DOMAIN}/{quote(s3_key, safe='/')}"


def extract_s3_key_from_url(url: str) -> str | None:
    if not url or not url.startswith(("http://", "https://")):
        return None
    prefix = f"https://{CLOUDFRONT_DOMAIN}/"
    if url.startswith(prefix):
        return unquote(url[len(prefix) :]) or None
    try:
        p = urlparse(url)
        if p.netloc == CLOUDFRONT_DOMAIN and p.path:
            return unquote(p.path.lstrip("/"))
    except Exception:
        pass
    return None


def upload_file_to_s3(s3_client, local_path: Path, s3_key: str, content_type: str) -> bool:
    try:
        with open(local_path, "rb") as f:
            s3_client.put_object(Bucket=BUCKET_NAME, Key=s3_key, Body=f, ContentType=content_type)
        return True
    except ClientError as e:
        print(f"  Loi S3: {e}", file=sys.stderr)
        return False


def find_local_file(images_base: Path, path_or_none: str | None, product_id: str) -> Path | None:
    """Tim file anh local: theo path trong CSV, hoac theo ten file (voi nhieu duoi)."""
    path_or_none = (path_or_none or "").strip()
    normalized = path_or_none.replace("\\", "/") if path_or_none else ""
    # Path relative (khong phai URL)
    if path_or_none and not path_or_none.startswith("http"):
        # Thu full path trong thu muc Products hoac Products/image/...
        for base in (images_base, images_base / "image"):
            p = base / normalized
            if p.is_file():
                return p
        # Thu chi ten file (bo thu muc) ngay trong Products
        name_only = normalized.split("/")[-1] if "/" in normalized else normalized
        base_name = Path(name_only).stem
        ext_from_path = Path(name_only).suffix.lower()
        if ext_from_path:
            f = images_base / name_only
            if f.is_file():
                return f
            for ext in DEFAULT_EXTENSIONS:
                if ext != ext_from_path:
                    f = images_base / (base_name + ext)
                    if f.is_file():
                        return f
        else:
            for ext in DEFAULT_EXTENSIONS:
                f = images_base / (name_only + ext)
                if f.is_file():
                    return f
                f = images_base / (base_name + ext)
                if f.is_file():
                    return f
    # Fallback: tim theo base name trong thu muc
    base_name = Path(normalized.split("/")[-1] if normalized and "/" in normalized else normalized or "").stem
    if not base_name:
        return None
    for ext in DEFAULT_EXTENSIONS:
        for candidate in (base_name + ext, base_name.replace(" ", "") + ext):
            f = images_base / candidate
            if f.is_file():
                return f
    for f in images_base.iterdir():
        if f.is_file() and f.suffix.lower() in DEFAULT_EXTENSIONS and f.stem == base_name:
            return f
    return None


def main():
    parser = argparse.ArgumentParser(
        description="Upload anh part products len S3, cap nhat PartProducts.csv (ImageUrl)."
    )
    script_dir = Path(__file__).resolve().parent
    seed_data = script_dir.parent.parent  # SeedData
    parser.add_argument("--images-dir", type=Path, default=script_dir, help="Thu muc chua anh (mac dinh: Products)")
    parser.add_argument("--csv", type=Path, default=seed_data / "PartProducts.csv", help="Duong dan PartProducts.csv")
    parser.add_argument("--dry-run", action="store_true", help="Khong upload, khong ghi CSV")
    parser.add_argument("--skip-existing", action="store_true", help="Bo qua dong da co ImageUrl la URL CloudFront")
    parser.add_argument("--url-only", action="store_true", help="Chi cap nhat path -> URL CloudFront, khong upload")
    args = parser.parse_args()

    images_base = args.images_dir.resolve()
    csv_path = args.csv.resolve()
    if not csv_path.is_file():
        print(f"Khong tim thay CSV: {csv_path}", file=sys.stderr)
        sys.exit(1)

    with open(csv_path, "r", encoding="utf-8-sig", newline="") as f:
        reader = csv.DictReader(f)
        fieldnames = list(reader.fieldnames)
        if "ImageUrl" not in fieldnames or "Id" not in fieldnames:
            print("CSV can cot ImageUrl va Id.", file=sys.stderr)
            sys.exit(1)
        rows = list(reader)

    s3_client = boto3.client("s3", region_name=REGION) if not (args.dry_run or args.url_only) else None
    updated = skipped = failed = 0

    for row in rows:
        image_url = (row.get("ImageUrl") or "").strip()
        product_id = (row.get("Id") or "").strip()
        if not product_id:
            skipped += 1
            continue

        if image_url.startswith(("http://", "https://")):
            if CLOUDFRONT_DOMAIN in image_url:
                s3_key = extract_s3_key_from_url(image_url)
                if not s3_key:
                    skipped += 1
                    continue
                if args.url_only or args.skip_existing:
                    skipped += 1
                    continue
                local_path = images_base / s3_key.replace("\\", "/")
                if not local_path.is_file():
                    local_path = images_base / (Path(s3_key).name if "/" in s3_key else s3_key)
                if not local_path.is_file():
                    local_path = find_local_file(images_base, image_url, product_id)
                    if local_path:
                        s3_key = f"{S3_KEY_PREFIX}/{product_id}{local_path.suffix}"
                if not local_path.is_file():
                    if args.skip_existing:
                        skipped += 1
                        continue
                    print(f"  Khong tim thay file: {product_id}")
                    failed += 1
                    continue
            else:
                skipped += 1
                continue
        else:
            if args.url_only:
                s3_key = f"{S3_KEY_PREFIX}/{product_id}.png"
                if image_url:
                    name = Path(image_url.replace("\\", "/")).name
                    if "." in name:
                        s3_key = f"{S3_KEY_PREFIX}/{product_id}{Path(name).suffix}"
                row["ImageUrl"] = build_cloudfront_url(s3_key)
                updated += 1
                continue
            local_path = find_local_file(images_base, image_url, product_id)
            if not local_path:
                print(f"  Khong tim thay file: {product_id} (path: {image_url or 'infer'})")
                failed += 1
                continue
            s3_key = f"{S3_KEY_PREFIX}/{product_id}{local_path.suffix}"

        cloudfront_url = build_cloudfront_url(s3_key)
        if args.dry_run:
            print(f"  [dry-run] {local_path} -> s3://{BUCKET_NAME}/{s3_key}")
            row["ImageUrl"] = cloudfront_url
            updated += 1
            continue
        if upload_file_to_s3(s3_client, local_path, s3_key, get_content_type(local_path)):
            row["ImageUrl"] = cloudfront_url
            updated += 1
            print(f"  OK: {s3_key}")
        else:
            failed += 1

    if not args.dry_run and updated > 0:
        with open(csv_path, "w", encoding="utf-8", newline="") as f:
            w = csv.DictWriter(f, fieldnames=fieldnames)
            w.writeheader()
            w.writerows(rows)
        print(f"\nDa ghi {csv_path} voi {updated} dong.")
    print(f"\nKet qua: {updated} cap nhat, {skipped} bo qua, {failed} loi.")
    if failed:
        sys.exit(1)


if __name__ == "__main__":
    main()
