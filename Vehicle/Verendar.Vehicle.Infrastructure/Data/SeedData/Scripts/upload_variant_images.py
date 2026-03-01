#!/usr/bin/env python3
"""
Script upload ảnh vehicle variants lên S3 và cập nhật cột ImageUrl trong VehicleVariants.csv
thành URL CloudFront.

Cấu hình (từ Media API):
- Bucket: verendar
- Region: ap-southeast-1
- CloudFront: d3iova6424vljy.cloudfront.net

Cách dùng:
  pip install boto3
  set AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY (và tùy chọn AWS_DEFAULT_REGION=ap-southeast-1)
  python upload_variant_images.py [--images-dir PATH] [--csv PATH] [--dry-run]

Thư mục ảnh: mặc định là thư mục chứa script (SeedData), trong đó có thư mục Image/
theo đúng cấu trúc trong CSV (vd: Image/Honda/Manual/Wave Alpha/Black.png).
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
    print("Cần cài boto3: pip install boto3", file=sys.stderr)
    sys.exit(1)


# Cấu hình S3/CloudFront (khớp với appsettings.json và Media API)
BUCKET_NAME = "verendar"
REGION = os.environ.get("AWS_DEFAULT_REGION", "ap-southeast-1")
CLOUDFRONT_DOMAIN = "d3iova6424vljy.cloudfront.net"
# Prefix S3 cho ảnh variant (trùng với Media: master/variants)
S3_KEY_PREFIX = "master/variants"
# Path cũ: Image/... (map sang S3 là master/variants/...)
LEGACY_PATH_PREFIX = "Image"


def get_content_type(file_path: Path) -> str:
    ext = file_path.suffix.lower()
    mime = mimetypes.types_map.get(ext)
    if mime:
        return mime
    if ext in (".webp",):
        return "image/webp"
    if ext in (".avif",):
        return "image/avif"
    return "application/octet-stream"


def build_cloudfront_url(s3_key: str) -> str:
    # Key có thể chứa khoảng trắng -> encode cho URL
    encoded = quote(s3_key, safe="/")
    return f"https://{CLOUDFRONT_DOMAIN}/{encoded}"


def normalize_to_s3_key(path_or_key: str) -> str:
    """Chuyen path/URL path sang S3 key chuan (master/variants/...)."""
    path_or_key = (path_or_key or "").strip()
    if path_or_key.startswith(S3_KEY_PREFIX + "/"):
        return path_or_key
    if path_or_key.startswith(LEGACY_PATH_PREFIX + "/"):
        return S3_KEY_PREFIX + "/" + path_or_key[len(LEGACY_PATH_PREFIX) + 1:]
    if path_or_key.startswith(LEGACY_PATH_PREFIX):
        return S3_KEY_PREFIX + "/" + path_or_key[len(LEGACY_PATH_PREFIX):].lstrip("/")
    return path_or_key


def extract_s3_key_from_url(url: str) -> str | None:
    """Lay S3 key tu CloudFront URL. Path Image/... duoc chuyen thanh master/variants/..."""
    if not url or not (url.startswith("http://") or url.startswith("https://")):
        return None
    prefix = f"https://{CLOUDFRONT_DOMAIN}/"
    if not url.startswith(prefix):
        try:
            p = urlparse(url)
            if p.netloc == CLOUDFRONT_DOMAIN and p.path:
                raw = unquote(p.path.lstrip("/"))
                return normalize_to_s3_key(raw)
        except Exception:
            pass
        return None
    path = unquote(url[len(prefix):])
    return normalize_to_s3_key(path) if path else None


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
        print(f"  Lỗi S3: {e}", file=sys.stderr)
        return False


def main():
    parser = argparse.ArgumentParser(
        description="Upload ảnh variants lên S3 và cập nhật VehicleVariants.csv với URL CloudFront."
    )
    script_dir = Path(__file__).resolve().parent
    parser.add_argument(
        "--images-dir",
        type=Path,
        default=script_dir,
        help=f"Thư mục gốc chứa thư mục Image/ (mặc định: {script_dir})",
    )
    parser.add_argument(
        "--csv",
        type=Path,
        default=script_dir / "VehicleVariants.csv",
        help="Đường dẫn file CSV (mặc định: VehicleVariants.csv trong thư mục script)",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Chỉ in ra thao tác, không upload và không ghi CSV",
    )
    parser.add_argument(
        "--skip-existing",
        action="store_true",
        help="Bỏ qua dòng đã có ImageUrl là URL CloudFront (đã upload).",
    )
    parser.add_argument(
        "--url-only",
        action="store_true",
        help="Chỉ cập nhật CSV: đổi path trong ImageUrl thành URL CloudFront, không upload (không cần AWS, không cần file ảnh).",
    )
    args = parser.parse_args()

    images_base = args.images_dir.resolve()
    csv_path = args.csv.resolve()

    if not csv_path.is_file():
        print(f"Không tìm thấy file CSV: {csv_path}", file=sys.stderr)
        sys.exit(1)

    # boto3 tu dong dung credential tu aws configure (~/.aws/credentials) hoac env
    s3_client = None
    if not args.dry_run and not args.url_only:
        s3_client = boto3.client("s3", region_name=REGION)

    # Đọc CSV (UTF-8)
    rows = []
    with open(csv_path, "r", encoding="utf-8-sig", newline="") as f:
        reader = csv.DictReader(f)
        fieldnames = reader.fieldnames
        if "ImageUrl" not in (fieldnames or []):
            print("CSV không có cột ImageUrl.", file=sys.stderr)
            sys.exit(1)
        rows = list(reader)

    updated = 0
    skipped = 0
    failed = 0

    for i, row in enumerate(rows):
        image_url_cell = (row.get("ImageUrl") or "").strip()
        if not image_url_cell:
            skipped += 1
            continue

        # Xac dinh S3 key: neu la URL CloudFront thi giai ma path, con lai la path local
        if image_url_cell.startswith("http://") or image_url_cell.startswith("https://"):
            s3_key = extract_s3_key_from_url(image_url_cell)
            if not s3_key:
                skipped += 1
                continue
            # Che do url-only: da la URL roi thi bo qua
            if args.url_only:
                skipped += 1
                continue
        else:
            s3_key = normalize_to_s3_key(image_url_cell)
            # Che do url-only: doi path -> URL
            if args.url_only:
                row["ImageUrl"] = build_cloudfront_url(s3_key)
                updated += 1
                continue

        cloudfront_url = build_cloudfront_url(s3_key)

        # Da la URL CloudFront thi co the bo qua neu --skip-existing
        if args.skip_existing and CLOUDFRONT_DOMAIN in image_url_cell and image_url_cell.startswith("http"):
            skipped += 1
            continue

        # Tim file local: thu images_base / s3_key (SeedData/master/variants/...), neu khong co thi thu Image/ (cu)
        local_path = images_base / s3_key.replace("\\", "/")
        if not local_path.is_file() and s3_key.startswith(S3_KEY_PREFIX + "/"):
            legacy_rel = s3_key[len(S3_KEY_PREFIX) + 1:]
            local_path = images_base / LEGACY_PATH_PREFIX / legacy_rel.replace("\\", "/")

        if not local_path.is_file():
            print(f"  Khong tim thay file: {local_path}")
            failed += 1
            continue

        content_type = get_content_type(local_path)

        if args.dry_run:
            print(f"  [dry-run] Upload {local_path} -> s3://{BUCKET_NAME}/{s3_key} -> {cloudfront_url}")
            row["ImageUrl"] = cloudfront_url
            updated += 1
            continue

        if upload_file_to_s3(s3_client, local_path, s3_key, content_type):
            row["ImageUrl"] = cloudfront_url
            updated += 1
            print(f"  OK: {s3_key}")
        else:
            failed += 1

    # Ghi lại CSV
    if (not args.dry_run) and updated > 0:
        with open(csv_path, "w", encoding="utf-8", newline="") as f:
            writer = csv.DictWriter(f, fieldnames=fieldnames)
            writer.writeheader()
            writer.writerows(rows)
        print(f"\nDa ghi {csv_path} voi {updated} dong cap nhat ImageUrl.")

    print(f"\nKet qua: {updated} upload/cap nhat, {skipped} bo qua, {failed} loi.")
    if failed:
        sys.exit(1)


if __name__ == "__main__":
    main()
