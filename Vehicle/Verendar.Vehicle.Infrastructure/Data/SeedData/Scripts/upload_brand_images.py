#!/usr/bin/env python3
"""
Script upload logo vehicle brands len S3 va cap nhat cot LogoUrl trong VehicleBrands.csv
thanh URL CloudFront.

Cau hinh (trung Media API):
- Bucket: verendar, Region: ap-southeast-1
- CloudFront: d3iova6424vljy.cloudfront.net
- S3 prefix: master/brands

Cach dung:
  pip install boto3
  python upload_brand_images.py [--images-dir PATH] [--csv PATH] [--dry-run]

Thu muc anh: mac dinh la thu muc chua script (SeedData). Dat file logo vao:
  SeedData/brands/HONDA.png   (theo Code trong CSV)
  hoac  SeedData/master/brands/HONDA.png
Hoac trong CSV de LogoUrl = path relative, vd: brands/HONDA.png
"""

import argparse
import csv
import mimetypes
import os
import sys
import tempfile
from pathlib import Path
from urllib.parse import quote, unquote, urlparse
from urllib.request import urlopen, Request

try:
    import boto3
    from botocore.exceptions import ClientError
except ImportError:
    print("Can cai boto3: pip install boto3", file=sys.stderr)
    sys.exit(1)


BUCKET_NAME = "verendar"
REGION = os.environ.get("AWS_DEFAULT_REGION", "ap-southeast-1")
CLOUDFRONT_DOMAIN = "d3iova6424vljy.cloudfront.net"
S3_KEY_PREFIX = "master/brands"

# Extensions thu tim neu khong co path trong CSV
DEFAULT_EXTENSIONS = (".png", ".webp", ".jpg", ".jpeg", ".svg")


def get_content_type(file_path: Path) -> str:
    ext = file_path.suffix.lower()
    mime = mimetypes.types_map.get(ext)
    if mime:
        return mime
    if ext in (".webp",):
        return "image/webp"
    if ext in (".svg",):
        return "image/svg+xml"
    return "application/octet-stream"


def build_cloudfront_url(s3_key: str) -> str:
    encoded = quote(s3_key, safe="/")
    return f"https://{CLOUDFRONT_DOMAIN}/{encoded}"


def normalize_to_s3_key(path_or_key: str) -> str:
    path_or_key = (path_or_key or "").strip()
    if path_or_key.startswith(S3_KEY_PREFIX + "/"):
        return path_or_key
    if path_or_key.startswith("brands/"):
        return S3_KEY_PREFIX + "/" + path_or_key[len("brands/"):].lstrip("/")
    if "/" not in path_or_key and path_or_key:
        return f"{S3_KEY_PREFIX}/{path_or_key}"
    return f"{S3_KEY_PREFIX}/{path_or_key.lstrip('/')}"


def extract_s3_key_from_url(url: str) -> str | None:
    if not url or not (url.startswith("http://") or url.startswith("https://")):
        return None
    prefix = f"https://{CLOUDFRONT_DOMAIN}/"
    if not url.startswith(prefix):
        try:
            p = urlparse(url)
            if p.netloc == CLOUDFRONT_DOMAIN and p.path:
                return unquote(p.path.lstrip("/"))
        except Exception:
            pass
        return None
    path = unquote(url[len(prefix):])
    return path if path else None


def upload_file_to_s3(s3_client, local_path: Path, s3_key: str, content_type: str) -> bool:
    try:
        with open(local_path, "rb") as f:
            s3_client.put_object(
                Bucket=BUCKET_NAME,
                Key=s3_key,
                Body=f,
                ContentType=content_type,
            )
        return True
    except ClientError as e:
        print(f"  Loi S3: {e}", file=sys.stderr)
        return False


def upload_bytes_to_s3(s3_client, data: bytes, s3_key: str, content_type: str) -> bool:
    try:
        s3_client.put_object(
            Bucket=BUCKET_NAME,
            Key=s3_key,
            Body=data,
            ContentType=content_type,
        )
        return True
    except ClientError as e:
        print(f"  Loi S3: {e}", file=sys.stderr)
        return False


def download_image(url: str) -> tuple[bytes, str] | None:
    """Tai anh tu URL, tra ve (bytes, content_type) hoac None."""
    try:
        req = Request(url, headers={"User-Agent": "Verendar-Seed/1.0"})
        with urlopen(req, timeout=15) as resp:
            data = resp.read()
            ct = resp.headers.get("Content-Type", "").split(";")[0].strip()
            if not ct or ct == "application/octet-stream":
                # Doan tu duong dan
                p = urlparse(url)
                path = p.path or ""
                if ".png" in path.lower():
                    ct = "image/png"
                elif ".webp" in path.lower():
                    ct = "image/webp"
                elif ".jpg" in path.lower() or ".jpeg" in path.lower():
                    ct = "image/jpeg"
                elif ".svg" in path.lower():
                    ct = "image/svg+xml"
                else:
                    ct = "image/png"
            return (data, ct)
    except Exception as e:
        print(f"  Loi tai URL: {e}", file=sys.stderr)
        return None


def find_local_file(images_base: Path, path_or_code: str, code: str) -> Path | None:
    """Tim file local: path co san, hoac brands/{Code}.ext."""
    path_or_code = (path_or_code or "").strip()
    # Neu la path relative (khong phai URL)
    if path_or_code and not path_or_code.startswith("http"):
        p = images_base / path_or_code.replace("\\", "/")
        if p.is_file():
            return p
        # Thu them voi prefix master/brands
        if not path_or_code.startswith("master/"):
            p2 = images_base / S3_KEY_PREFIX / path_or_code.lstrip("brands/").lstrip("/")
            if p2.is_file():
                return p2
    # Infer tu Code: brands/HONDA.png hoac master/brands/HONDA.png
    for base in (images_base / "brands", images_base / S3_KEY_PREFIX):
        for ext in DEFAULT_EXTENSIONS:
            f = base / f"{code}{ext}"
            if f.is_file():
                return f
    return None


def main():
    parser = argparse.ArgumentParser(
        description="Upload logo brands len S3 va cap nhat VehicleBrands.csv (LogoUrl)."
    )
    script_dir = Path(__file__).resolve().parent
    parser.add_argument("--images-dir", type=Path, default=script_dir,
                        help="Thu muc goc chua brands/ (mac dinh: thu muc script)")
    parser.add_argument("--csv", type=Path, default=script_dir / "VehicleBrands.csv",
                        help="Duong dan file CSV")
    parser.add_argument("--dry-run", action="store_true", help="Khong upload, khong ghi CSV")
    parser.add_argument("--skip-existing", action="store_true",
                        help="Bo qua dong da co LogoUrl la URL CloudFront")
    parser.add_argument("--url-only", action="store_true",
                        help="Chi cap nhat CSV path -> URL CloudFront, khong upload")
    args = parser.parse_args()

    images_base = args.images_dir.resolve()
    csv_path = args.csv.resolve()

    if not csv_path.is_file():
        print(f"Khong tim thay CSV: {csv_path}", file=sys.stderr)
        sys.exit(1)

    if "LogoUrl" not in (next(csv.DictReader(open(csv_path, "r", encoding="utf-8-sig"))).keys()):
        print("CSV khong co cot LogoUrl.", file=sys.stderr)
        sys.exit(1)

    s3_client = None
    if not args.dry_run and not args.url_only:
        s3_client = boto3.client("s3", region_name=REGION)

    rows = []
    with open(csv_path, "r", encoding="utf-8-sig", newline="") as f:
        reader = csv.DictReader(f)
        fieldnames = list(reader.fieldnames)
        rows = list(reader)

    updated = 0
    skipped = 0
    failed = 0

    for row in rows:
        logo_url_cell = (row.get("LogoUrl") or "").strip()
        code = (row.get("Code") or "").strip()
        if not code:
            skipped += 1
            continue

        # Da la URL CloudFront
        if logo_url_cell.startswith("http://") or logo_url_cell.startswith("https://"):
            if CLOUDFRONT_DOMAIN in logo_url_cell:
                s3_key = extract_s3_key_from_url(logo_url_cell)
                if not s3_key:
                    skipped += 1
                    continue
                if args.url_only:
                    skipped += 1
                    continue
                if args.skip_existing:
                    skipped += 1
                    continue
                local_path = images_base / s3_key.replace("\\", "/")
                if not local_path.is_file():
                    legacy = images_base / "brands" / (Path(s3_key).name if "/" in s3_key else s3_key)
                    if legacy.is_file():
                        local_path = legacy
                    else:
                        print(f"  Khong tim thay file cho {code}: {s3_key}")
                        failed += 1
                        continue
            else:
                # URL ngoai: thu tai ve roi upload, hoac tim file local
                local_path = find_local_file(images_base, None, code)
                if local_path:
                    s3_key = f"{S3_KEY_PREFIX}/{code}{local_path.suffix}"
                else:
                    # Tai tu URL
                    result = download_image(logo_url_cell)
                    if not result:
                        print(f"  Khong tai duoc logo cho {code}")
                        failed += 1
                        continue
                    data, content_type = result
                    # Duoi file tu URL hoac mac dinh .png
                    path_from_url = urlparse(logo_url_cell).path or ""
                    ext = Path(path_from_url).suffix.lower() or ".png"
                    if ext not in DEFAULT_EXTENSIONS:
                        ext = ".png"
                    s3_key = f"{S3_KEY_PREFIX}/{code}{ext}"
                    cloudfront_url = build_cloudfront_url(s3_key)
                    if args.dry_run:
                        print(f"  [dry-run] Tai tu URL -> s3://{BUCKET_NAME}/{s3_key}")
                        row["LogoUrl"] = cloudfront_url
                        updated += 1
                        continue
                    if upload_bytes_to_s3(s3_client, data, s3_key, content_type):
                        row["LogoUrl"] = cloudfront_url
                        updated += 1
                        print(f"  OK: {s3_key} (tai tu URL)")
                    else:
                        failed += 1
                    continue
        else:
            # Path hoac trong
            if args.url_only:
                s3_key = normalize_to_s3_key(logo_url_cell or f"{code}.png")
                row["LogoUrl"] = build_cloudfront_url(s3_key)
                updated += 1
                continue
            local_path = find_local_file(images_base, logo_url_cell, code)
            if not local_path:
                print(f"  Khong tim thay file: {code} (path: {logo_url_cell or 'infer'})")
                failed += 1
                continue
            s3_key = f"{S3_KEY_PREFIX}/{code}{local_path.suffix}"

        cloudfront_url = build_cloudfront_url(s3_key)

        if args.dry_run:
            print(f"  [dry-run] {local_path} -> s3://{BUCKET_NAME}/{s3_key}")
            row["LogoUrl"] = cloudfront_url
            updated += 1
            continue

        content_type = get_content_type(local_path)
        if upload_file_to_s3(s3_client, local_path, s3_key, content_type):
            row["LogoUrl"] = cloudfront_url
            updated += 1
            print(f"  OK: {s3_key}")
        else:
            failed += 1

    if (not args.dry_run) and updated > 0:
        with open(csv_path, "w", encoding="utf-8", newline="") as f:
            writer = csv.DictWriter(f, fieldnames=fieldnames)
            writer.writeheader()
            writer.writerows(rows)
        print(f"\nDa ghi {csv_path} voi {updated} dong cap nhat LogoUrl.")

    print(f"\nKet qua: {updated} upload/cap nhat, {skipped} bo qua, {failed} loi.")
    if failed:
        sys.exit(1)


if __name__ == "__main__":
    main()
