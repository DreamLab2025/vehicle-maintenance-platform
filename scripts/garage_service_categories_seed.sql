-- Seed service catalog categories for Garage service.
-- Safe to run multiple times (upsert by unique Slug).

BEGIN;

WITH category_data AS (
    SELECT
        v.name,
        v.slug,
        v.description,
        v.display_order,
        (
            substr(md5(v.slug), 1, 8) || '-' ||
            substr(md5(v.slug), 9, 4) || '-' ||
            substr(md5(v.slug), 13, 4) || '-' ||
            substr(md5(v.slug), 17, 4) || '-' ||
            substr(md5(v.slug), 21, 12)
        )::uuid AS generated_id
    FROM (
        VALUES
            ('Rửa xe', 'rua-xe', 'Dịch vụ rửa và làm sạch xe.', 1),
            ('Sửa xe', 'sua-xe', 'Dịch vụ sửa chữa các hạng mục kỹ thuật.', 2),
            ('Kiểm tra tổng quát', 'kiem-tra-tong-quat', 'Dịch vụ kiểm tra toàn diện tình trạng xe.', 3),
            ('Thay phụ tùng', 'thay-phu-tung', 'Dịch vụ thay thế phụ tùng chính hãng/tương thích.', 4),
            ('Tân trang xe', 'tan-trang-xe', 'Dịch vụ tân trang, làm mới ngoại/nội thất.', 5)
    ) AS v(name, slug, description, display_order)
)
INSERT INTO "ServiceCategories" (
    "Id",
    "Name",
    "Slug",
    "Description",
    "IconUrl",
    "DisplayOrder",
    "CreatedAt",
    "CreatedBy"
)
SELECT
    c.generated_id,
    c.name,
    c.slug,
    c.description,
    NULL,
    c.display_order,
    NOW(),
    '00000000-0000-0000-0000-000000000000'::uuid
FROM category_data c
ON CONFLICT ("Slug")
DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Description" = EXCLUDED."Description",
    "IconUrl" = EXCLUDED."IconUrl",
    "DisplayOrder" = EXCLUDED."DisplayOrder",
    "UpdatedAt" = NOW(),
    "UpdatedBy" = '00000000-0000-0000-0000-000000000000'::uuid;

COMMIT;

SELECT
    "Id",
    "Name",
    "Slug",
    "DisplayOrder",
    "CreatedAt",
    "UpdatedAt"
FROM "ServiceCategories"
WHERE "DeletedAt" IS NULL
ORDER BY "DisplayOrder", "Name";
