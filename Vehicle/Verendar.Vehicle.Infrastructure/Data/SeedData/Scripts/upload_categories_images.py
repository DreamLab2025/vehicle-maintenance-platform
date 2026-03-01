#!/usr/bin/env python3
"""
Upload icon part categories len S3 va cap nhat cot IconUrl trong PartCategories.csv.
S3 prefix: master/part-categories (trung Media API init-upload/part-categories).
Ho tro path local, URL CloudFront, hoac tai tu URL ngoai.
"""

import argparse
import csv
import mimetypes
import os
import sys
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
S3_KEY_PREFIX = "master/part-categories"
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
    return f"https://{CLOUDFRONT_DOMAIN}/{quote(s3_key, safe='/')}"


def extract_s3_key_from_url(url: str) -> str | None:
    if not url or not url.startswith(("http://", "https://")):
        return None
    prefix = f"https://{CLOUDFRONT_DOMAIN}/"
    if url.startswith(prefix):
        return unquote(url[len(prefix):]) or None
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


def upload_bytes_to_s3(s3_client, data: bytes, s3_key: str, content_type: str) -> bool:
    try:
        s3_client.put_object(Bucket=BUCKET_NAME, Key=s3_key, Body=data, ContentType=content_type)
        return True
    except ClientError as e:
        print(f"  Loi S3: {e}", file=sys.stderr)
        return False


def download_image(url: str) -> tuple[bytes, str] | None:
    try:
        req = Request(url, headers={"User-Agent": "Verendar-Seed/1.0"})
        with urlopen(req, timeout=15) as resp:
            data = resp.read()
            ct = resp.headers.get("Content-Type", "").split(";")[0].strip()
            if not ct or ct == "application/octet-stream":
                p = urlparse(url)
                path = p.path or ""
                if ".webp" in path.lower():
                    ct = "image/webp"
                elif ".png" in path.lower():
                    ct = "image/png"
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


def find_local_file(images_base: Path, path_or_none: str | None, code: str) -> Path | None:
    if path_or_none and not path_or_none.startswith("http"):
        p = images_base / path_or_none.replace("\\", "/")
        if p.is_file():
            return p
        p2 = images_base / S3_KEY_PREFIX / (path_or_none.split("/")[-1] if "/" in path_or_none else path_or_none)
        if p2.is_file():
            return p2
    for base in (images_base / "Categories", images_base / "Image" / "Categories", images_base / S3_KEY_PREFIX):
        for ext in DEFAULT_EXTENSIONS:
            for name in (f"{code}{ext}", f"{code.upper()}{ext}", f"{code.lower()}{ext}"):
                f = base / name
                if f.is_file():
                    return f
    return None


def main():
    parser = argparse.ArgumentParser(description="Upload icon part categories len S3, cap nhat PartCategories.csv (IconUrl).")
    script_dir = Path(__file__).resolve().parent
    parser.add_argument("--images-dir", type=Path, default=script_dir)
    parser.add_argument("--csv", type=Path, default=script_dir / "PartCategories.csv")
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--skip-existing", action="store_true")
    parser.add_argument("--url-only", action="store_true")
    args = parser.parse_args()

    images_base = args.images_dir.resolve()
    csv_path = args.csv.resolve()
    if not csv_path.is_file():
        print(f"Khong tim thay CSV: {csv_path}", file=sys.stderr)
        sys.exit(1)

    with open(csv_path, "r", encoding="utf-8-sig") as f:
        r = csv.DictReader(f)
        fieldnames = list(r.fieldnames)
        if "IconUrl" not in fieldnames or "Code" not in fieldnames:
            print("CSV can cot IconUrl va Code.", file=sys.stderr)
            sys.exit(1)
        rows = list(r)

    s3_client = boto3.client("s3", region_name=REGION) if not (args.dry_run or args.url_only) else None
    updated = skipped = failed = 0

    for row in rows:
        icon_url = (row.get("IconUrl") or "").strip()
        code = (row.get("Code") or "").strip()
        if not code:
            skipped += 1
            continue

        if icon_url.startswith(("http://", "https://")):
            if CLOUDFRONT_DOMAIN in icon_url:
                s3_key = extract_s3_key_from_url(icon_url)
                if not s3_key:
                    skipped += 1
                    continue
                if args.url_only or args.skip_existing:
                    skipped += 1
                    continue
                local_path = images_base / s3_key.replace("\\", "/")
                if not local_path.is_file():
                    local_path = images_base / "Image" / "Categories" / (Path(s3_key).name if "/" in s3_key else s3_key)
                if not local_path.is_file():
                    local_path = find_local_file(images_base, None, code)
                    if local_path:
                        s3_key = f"{S3_KEY_PREFIX}/{code}{local_path.suffix}"
                if not local_path.is_file():
                    if args.skip_existing:
                        skipped += 1
                        continue
                    print(f"  Khong tim thay file: {code}")
                    failed += 1
                    continue
            else:
                local_path = find_local_file(images_base, None, code)
                if local_path:
                    s3_key = f"{S3_KEY_PREFIX}/{code}{local_path.suffix}"
                else:
                    result = download_image(icon_url)
                    if not result:
                        print(f"  Khong tai duoc icon: {code}")
                        failed += 1
                        continue
                    data, content_type = result
                    path_from_url = urlparse(icon_url).path or ""
                    ext = Path(path_from_url).suffix.lower() or ".png"
                    if ext not in DEFAULT_EXTENSIONS:
                        ext = ".png"
                    s3_key = f"{S3_KEY_PREFIX}/{code}{ext}"
                    cloudfront_url = build_cloudfront_url(s3_key)
                    if args.dry_run:
                        print(f"  [dry-run] Tai tu URL -> {s3_key}")
                        row["IconUrl"] = cloudfront_url
                        updated += 1
                        continue
                    if upload_bytes_to_s3(s3_client, data, s3_key, content_type):
                        row["IconUrl"] = cloudfront_url
                        updated += 1
                        print(f"  OK: {s3_key} (tai tu URL)")
                    else:
                        failed += 1
                    continue
        else:
            if args.url_only:
                s3_key = f"{S3_KEY_PREFIX}/{code}.png" if not icon_url or icon_url == code else f"{S3_KEY_PREFIX}/{Path(icon_url).name}"
                row["IconUrl"] = build_cloudfront_url(s3_key)
                updated += 1
                continue
            local_path = find_local_file(images_base, icon_url, code)
            if not local_path:
                print(f"  Khong tim thay file: {code} (path: {icon_url or 'infer'})")
                failed += 1
                continue
            s3_key = f"{S3_KEY_PREFIX}/{code}{local_path.suffix}"

        cloudfront_url = build_cloudfront_url(s3_key)
        if args.dry_run:
            print(f"  [dry-run] {local_path} -> s3://{BUCKET_NAME}/{s3_key}")
            row["IconUrl"] = cloudfront_url
            updated += 1
            continue
        if upload_file_to_s3(s3_client, local_path, s3_key, get_content_type(local_path)):
            row["IconUrl"] = cloudfront_url
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
