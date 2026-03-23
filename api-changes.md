# API — Vehicle Service (`Verendar.Vehicle`)

---

## Quy ước

| Mục                | Chi tiết                                                               |
| ------------------ | ---------------------------------------------------------------------- |
| Auth               | `Authorization: Bearer …`. **Admin** = role `Admin`.                   |
| Rate limiting      | Nhóm route dùng policy `Fixed`.                                        |
| Phân trang (query) | `pageNumber`, `pageSize` (max 100), `isDescending` (tuỳ DTO).          |
| Slug               | Tạo mới catalog: slug do **backend** sinh; client chỉ đọc từ response. |
| Part category      | Body/URL: **`partCategorySlug`**.                                      |

---

## Envelope `ApiResponse<T>`

**Thành công (ví dụ 200):**

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "string",
  "data": {},
  "metadata": null
}
```

**Danh sách có phân trang** (`SuccessPagedResponse`): `data` là mảng; `metadata`:

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalItems": 42,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

**Lỗi (4xx/5xx):** `isSuccess: false`, `statusCode`, `message`; `data` thường `null`. Chi tiết validation có thể thêm qua middleware RFC 7807 (tuỳ cấu hình host).

---

## Enum (tham khảo)

| Kiểu                      | Giá trị                                    |
| ------------------------- | ------------------------------------------ |
| `VehicleFuelType`         | `1` = Gasoline, `2` = Diesel               |
| `VehicleTransmissionType` | `1` = Manual, `2` = Automatic, `3` = Sport |

---

## Kiểu dùng lại (tóm tắt JSON)

### `typeSummary` / `typeResponse`

```json
// TypeSummary
{ "id": "uuid", "name": "string", "slug": "string", "imageUrl": "string | null" }

// TypeResponse
{ "id": "uuid", "name": "string", "slug": "string", "imageUrl": "string | null", "description": "string | null", "createdAt": "datetime", "updatedAt": "datetime | null" }
```

### `brandSummary` / `brandResponse`

```json
// BrandSummary
{ "id": "uuid", "vehicleTypeId": "uuid", "vehicleTypeName": "string", "name": "string", "slug": "string", "logoUrl": "string | null", "logoMediaFileId": "uuid | null" }

// BrandResponse
{ "id": "uuid", "vehicleTypeId": "uuid", "vehicleTypeName": "string", "name": "string", "slug": "string", "logoUrl": "string | null", "logoMediaFileId": "uuid | null", "website": "string | null", "supportPhone": "string | null", "createdAt": "datetime", "updatedAt": "datetime | null" }
```

### `modelSummary` / `modelResponse` / `modelResponseWithVariants`

```json
// ModelSummary
{ "id": "uuid", "name": "string", "slug": "string", "brandId": "uuid", "brandName": "string", "typeId": "uuid", "typeName": "string", "manufactureYear": 2024, "fuelTypeName": "string", "transmissionTypeName": "string", "description": "string | null" }

// ModelResponse (+ enum fuelType/transmissionType, engine…)
{ "id": "uuid", "name": "string", "slug": "string", "brandId": "uuid", "brandName": "string", "typeId": "uuid", "typeName": "string", "manufactureYear": 2024, "fuelType": 1, "fuelTypeName": "string", "transmissionType": 2, "transmissionTypeName": "string", "engineDisplacementDisplay": "150 cc | null", "engineCapacity": 0.15, "description": "string | null", "createdAt": "datetime", "updatedAt": "datetime | null" }

// ModelResponseWithVariants = ModelResponse + "variants": [ variantResponse ]
```

### `variantResponse`

```json
{
  "id": "uuid",
  "vehicleModelId": "uuid",
  "color": "string",
  "hexCode": "#RRGGBB",
  "imageUrl": "string",
  "imageMediaFileId": "uuid | null",
  "createdAt": "datetime",
  "updatedAt": "datetime | null"
}
```

### `userVariantResponse` (dùng trong `userVehicleResponse`)

`variantResponse` + nested:

```json
{
  "model": {
    /* modelResponse đầy đủ */
  }
}
```

### `vehicleModelRefSummaryDto` (nested trong summary xe)

```json
{
  "id": "uuid",
  "name": "string",
  "brand": {
    "id": "uuid",
    "name": "string",
    "type": { "id": "uuid", "name": "string" }
  }
}
```

### `userVehicleSummaryDto`

```json
{
  "id": "uuid",
  "userId": "uuid",
  "licensePlate": "string | null",
  "vin": "string | null",
  "purchaseDate": "yyyy-MM-dd | null",
  "currentOdometer": 0,
  "lastOdometerUpdate": "yyyy-MM-dd | null",
  "averageKmPerDay": 0,
  "createdAt": "datetime",
  "updatedAt": "datetime | null",
  "variant": {
    "id": "uuid",
    "color": "string",
    "hexCode": "#RRGGBB",
    "imageUrl": "string",
    "imageMediaFileId": "uuid | null",
    "model": {
      /* vehicleModelRefSummaryDto */
    }
  }
}
```

### `userVehicleResponse` / `userVehicleDetailResponse`

`userVehicleResponse`:

```json
{
  "id": "uuid",
  "userId": "uuid",
  "licensePlate": "string | null",
  "vin": "string | null",
  "purchaseDate": "yyyy-MM-dd | null",
  "currentOdometer": 0,
  "lastOdometerUpdate": "yyyy-MM-dd | null",
  "averageKmPerDay": 0,
  "createdAt": "datetime",
  "updatedAt": "datetime | null",
  "userVehicleVariant": {
    /* userVariantResponse */
  }
}
```

`userVehicleDetailResponse` = trên +

```json
{
  "totalMaintenanceActivities": 0,
  "lastMaintenanceDate": "datetime | null",
  "daysSincePurchase": 0,
  "totalKmDriven": 0
}
```

### `reminderSummary`

```json
{
  "id": "uuid",
  "level": "string",
  "status": "string",
  "currentOdometer": 0,
  "targetOdometer": 0,
  "remainingKm": 0,
  "targetDate": "date | null",
  "percentageRemaining": 0,
  "isNotified": false,
  "notifiedDate": "date | null",
  "isDismissed": false,
  "dismissedDate": "date | null"
}
```

### `trackingCycleSummary`

```json
{
  "id": "uuid",
  "status": "string",
  "startOdometer": 0,
  "startDate": "date",
  "targetOdometer": 0,
  "targetDate": "date | null",
  "reminders": [
    /* reminderSummary */
  ]
}
```

### `partTrackingSummary`

```json
{
  "id": "uuid",
  "partCategoryId": "uuid",
  "partCategoryName": "string",
  "partCategorySlug": "string",
  "instanceIdentifier": "string | null",
  "currentPartProductId": "uuid | null",
  "currentPartProductName": "string | null",
  "lastReplacementOdometer": 0,
  "lastReplacementDate": "date | null",
  "customKmInterval": 0,
  "customMonthsInterval": 0,
  "predictedNextOdometer": 0,
  "predictedNextDate": "date | null",
  "isDeclared": false,
  "activeCycle": {
    /* trackingCycleSummary | null */
  }
}
```

### `categoryInfoDto`

```json
{
  "id": "uuid",
  "name": "string",
  "slug": "string",
  "description": "string | null",
  "iconUrl": "string | null",
  "iconMediaFileId": "uuid | null",
  "identificationSigns": "string | null",
  "consequencesIfNotHandled": "string | null"
}
```

### `reminderDetailDto`

Như các field reminder + `"partCategory": { /* categoryInfoDto */ }`.

### `partCategoryResponse` / `partCategorySummary`

```json
// PartCategorySummary
{ "id": "uuid", "name": "string", "slug": "string", "iconUrl": "string | null", "iconMediaFileId": "uuid | null", "displayOrder": 0 }

// PartCategoryResponse
{ "id": "uuid", "name": "string", "slug": "string", "description": "string | null", "iconUrl": "string | null", "iconMediaFileId": "uuid | null", "displayOrder": 0, "requiresOdometerTracking": true, "requiresTimeTracking": true, "allowsMultipleInstances": false, "identificationSigns": "string | null", "consequencesIfNotHandled": "string | null", "createdAt": "datetime", "updatedAt": "datetime | null" }
```

### `partProductSummary` / `partProductResponse`

```json
// PartProductSummary
{
  "id": "uuid",
  "partCategoryId": "uuid",
  "partCategoryName": "string",
  "name": "string",
  "brand": "string | null",
  "imageUrl": "string | null",
  "referencePrice": 0
}

// PartProductResponse
{
  "id": "uuid",
  "partCategoryId": "uuid",
  "partCategoryName": "string",
  "name": "string",
  "brand": "string | null",
  "description": "string | null",
  "imageUrl": "string | null",
  "referencePrice": 0,
  "recommendedKmInterval": 0,
  "recommendedMonthsInterval": 0,
  "createdAt": "datetime",
  "updatedAt": "datetime | null"
}
```

### `defaultScheduleResponse` (internal schedule)

```json
{
  "initialKm": 0,
  "kmInterval": 0,
  "monthsInterval": 0,
  "requiresOdometerTracking": true,
  "requiresTimeTracking": true
}
```

---

## 1. `/api/v1/types`

| Method | Path                 | Auth  |
| ------ | -------------------- | ----- |
| GET    | `/api/v1/types`      | User  |
| GET    | `/api/v1/types/{id}` | User  |
| POST   | `/api/v1/types`      | Admin |
| PUT    | `/api/v1/types/{id}` | Admin |
| DELETE | `/api/v1/types/{id}` | Admin |

### `GET /api/v1/types`

- **Query:** `pageNumber`, `pageSize`, `isDescending`
- **Response `data`:** `typeSummary[]` + **`metadata`** phân trang

### `GET /api/v1/types/{id}`

- **Response `data`:** `typeResponse`

### `POST /api/v1/types`

**Request body:**

```json
{
  "name": "string",
  "imageUrl": "string | null",
  "description": "string | null"
}
```

- `imageUrl` / `description` optional (khớp entity nullable).
- **Response `data`:** `typeResponse` (có `slug` do server tạo). **201**

### `PUT /api/v1/types/{id}`

- **Request body:** giống POST
- **Response `data`:** `typeResponse` (**slug không đổi**)

### `DELETE /api/v1/types/{id}`

- **Response `data`:** chuỗi (ví dụ thông báo xóa)

---

## 2. `/api/v1/brands`

| Method | Path                  | Auth  |
| ------ | --------------------- | ----- |
| GET    | `/api/v1/brands`      | User  |
| GET    | `/api/v1/brands/{id}` | User  |
| POST   | `/api/v1/brands`      | Admin |
| PUT    | `/api/v1/brands/{id}` | Admin |
| DELETE | `/api/v1/brands/{id}` | Admin |

### `GET /api/v1/brands`

- **Query:** `typeId` (optional), `pageNumber`, `pageSize`, `isDescending`
  - Có `typeId`: trả về list brand của loại đó (**không** dùng metadata kiểu paged trong service — trả list trong `data`).
  - Không `typeId`: paged toàn bộ → `data`: `brandSummary[]`, **`metadata`** phân trang.

### `GET /api/v1/brands/{id}`

- **Response `data`:** `brandResponse`

### `POST /api/v1/brands`

**Request body:**

```json
{
  "vehicleTypeId": "uuid",
  "name": "string",
  "logoUrl": "string | null",
  "logoMediaFileId": "uuid | null",
  "website": "string | null",
  "supportPhone": "string | null"
}
```

- **Response `data`:** `brandResponse` (**slug** server). **201**

### `PUT /api/v1/brands/{id}`

- **Request body:** giống POST (thường gửi `vehicleTypeId` + các field cập nhật)
- **Response `data`:** `brandResponse`

### `DELETE /api/v1/brands/{id}`

- **Response `data`:** string

---

## 3. `/api/v1/models`

| Method | Path                           | Auth  |
| ------ | ------------------------------ | ----- |
| GET    | `/api/v1/models`               | User  |
| GET    | `/api/v1/models/{id}`          | User  |
| GET    | `/api/v1/models/{id}/variants` | User  |
| POST   | `/api/v1/models`               | Admin |
| PUT    | `/api/v1/models/{id}`          | Admin |
| DELETE | `/api/v1/models/{id}`          | Admin |

### `GET /api/v1/models`

- **Query:** `typeId`, `brandId`, `modelName`, `transmissionType`, `engineDisplacement`, `manufactureYear`, `pageNumber`, `pageSize`, `isDescending`
- **Response `data`:** `modelSummary[]` + **`metadata`** phân trang

### `GET /api/v1/models/{id}`

- **Response `data`:** `modelResponseWithVariants`

### `GET /api/v1/models/{id}/variants`

- **Response `data`:** `variantResponse[]`

### `POST /api/v1/models`

**Request body:**

```json
{
  "name": "string",
  "brandId": "uuid",
  "typeId": "uuid",
  "manufactureYear": 2024,
  "fuelType": 1,
  "transmissionType": 2,
  "images": [{ "color": "string", "hexCode": "#000000", "imageUrl": "string" }],
  "engineDisplacement": 150,
  "engineCapacity": 0.15,
  "description": "string | null"
}
```

- Không gửi `slug` — server sinh khi tạo model.
- **Response `data`:** `modelResponseWithVariants` (**slug** server). **201**

### `PUT /api/v1/models/{id}`

- **Request body:** giống POST (ảnh/variant quản lý qua API variants riêng nếu cần)
- **Response `data`:** `modelResponse` (**slug** không đổi)

### `DELETE /api/v1/models/{id}`

- **Response `data`:** string

---

## 4. `/api/v1/variants`

| Method | Path                    | Auth  |
| ------ | ----------------------- | ----- |
| POST   | `/api/v1/variants`      | Admin |
| PUT    | `/api/v1/variants/{id}` | Admin |
| DELETE | `/api/v1/variants/{id}` | Admin |

### `POST /api/v1/variants`

**Request body:**

```json
{
  "vehicleModelId": "uuid",
  "color": "string",
  "hexCode": "#RRGGBB",
  "imageUrl": "string",
  "imageMediaFileId": "uuid | null"
}
```

- **Response `data`:** `variantResponse`. **201**

### `PUT /api/v1/variants/{id}`

**Request body:**

```json
{
  "color": "string",
  "hexCode": "#RRGGBB",
  "imageUrl": "string",
  "imageMediaFileId": "uuid | null"
}
```

- **Response `data`:** `variantResponse`

### `DELETE /api/v1/variants/{id}`

- **Response `data`:** string

---

## 5. `/api/v1/vehicle-models`

### `GET /api/v1/vehicle-models/{vehicleModelId}/part-categories`

- **Response `data`:** `partCategoryResponse[]`

---

## 6. `/api/v1/user-vehicles`

| Method | Path                                                                  | Auth |
| ------ | --------------------------------------------------------------------- | ---- |
| GET    | `/api/v1/user-vehicles`                                               | User |
| GET    | `/api/v1/user-vehicles/{userVehicleId}`                               | User |
| GET    | `/api/v1/user-vehicles/{userVehicleId}/parts`                         | User |
| GET    | `/api/v1/user-vehicles/is-allowed-create`                             | User |
| POST   | `/api/v1/user-vehicles`                                               | User |
| PUT    | `/api/v1/user-vehicles/{userVehicleId}`                               | User |
| DELETE | `/api/v1/user-vehicles/{userVehicleId}`                               | User |
| GET    | `/api/v1/user-vehicles/{userVehicleId}/streak`                        | User |
| POST   | `/api/v1/user-vehicles/{userVehicleId}/apply-tracking`                | User |
| GET    | `/api/v1/user-vehicles/{userVehicleId}/parts/{partTrackingId}/cycles` | User |
| GET    | `/api/v1/user-vehicles/{userVehicleId}/reminders`                     | User |

### `GET /api/v1/user-vehicles`

- **Response `data`:** `userVehicleSummaryDto[]`

### `GET /api/v1/user-vehicles/{userVehicleId}`

- **Response `data`:** `userVehicleDetailResponse`

### `GET /api/v1/user-vehicles/{userVehicleId}/parts`

- **Response `data`:** `partSummary[]`

### `GET /api/v1/user-vehicles/is-allowed-create`

- **Response `data`:**

```json
{ "isAllowed": true, "message": "string | null" }
```

### `POST` / `PUT /api/v1/user-vehicles` …

**Request body:**

```json
{
  "vehicleVariantId": "uuid",
  "licensePlate": "string | null",
  "vin": "string | null",
  "purchaseDate": "yyyy-MM-dd | null",
  "currentOdometer": 0
}
```

- `licensePlate` optional (có thể bỏ hoặc null).
- **POST Response `data`:** `userVehicleResponse`. **201**
- **PUT Response `data`:** `userVehicleResponse`

### `DELETE /api/v1/user-vehicles/{userVehicleId}`

- **Response `data`:** string

### `GET /api/v1/user-vehicles/{userVehicleId}/streak`

- **Response `data`:**

```json
{
  "vehicleId": "uuid",
  "currentStreak": 0,
  "isStreakActive": false,
  "daysToNextUnlock": 0
}
```

### `POST /api/v1/user-vehicles/{userVehicleId}/apply-tracking`

**Request body:**

```json
{
  "partCategorySlug": "string",
  "lastReplacementOdometer": 0,
  "lastReplacementDate": "yyyy-MM-dd",
  "predictedNextOdometer": 0,
  "predictedNextDate": "yyyy-MM-dd",
  "aiReasoning": "string | null",
  "confidenceScore": 0.85
}
```

- **Response `data`:** `partTrackingSummary`

### `GET .../parts/{partTrackingId}/cycles`

- **Response `data`:** `trackingCycleSummary[]`

### `GET .../reminders`

- **Response `data`:** `reminderDetailDto[]`

---

## 7. `/api/v1/odometer-history`

### `PATCH /api/v1/odometer-history/{userVehicleId}`

**Request body:**

```json
{ "currentOdometer": 12500 }
```

- **Response `data`:**

```json
{
  "userVehicleId": "uuid",
  "currentOdometer": 12500,
  "lastOdometerUpdate": "yyyy-MM-dd | null"
}
```

### `GET /api/v1/odometer-history`

- **Query:** `userVehicleId` (bắt buộc), `pageNumber`, `pageSize`, `isDescending`, `fromDate`, `toDate` (`date`)
- **Response `data`:**

```json
[
  {
    "id": "uuid",
    "userVehicleId": "uuid",
    "odometerValue": 0,
    "recordedDate": "yyyy-MM-dd",
    "kmOnRecordedDate": 0,
    "source": "string"
  }
]
```

- **`metadata`** phân trang

---

## 8. `/api/v1/maintenance-records`

### `GET /api/v1/maintenance-records`

- **Query:** `userVehicleId` (bắt buộc)
- **Response `data`:**

```json
[
  {
    "id": "uuid",
    "userVehicleId": "uuid",
    "serviceDate": "yyyy-MM-dd",
    "odometerAtService": 0,
    "garageName": "string | null",
    "totalCost": 0,
    "notes": "string | null",
    "invoiceImageUrl": "string | null",
    "itemCount": 0
  }
]
```

### `GET /api/v1/maintenance-records/{maintenanceRecordId}`

- **Response `data`:**

```json
{
  "id": "uuid",
  "userVehicleId": "uuid",
  "serviceDate": "yyyy-MM-dd",
  "odometerAtService": 0,
  "garageName": "string | null",
  "totalCost": 0,
  "notes": "string | null",
  "invoiceImageUrl": "string | null",
  "items": [
    {
      "id": "uuid",
      "partCategoryId": "uuid",
      "partCategorySlug": "string",
      "partProductId": "uuid | null",
      "partProductName": "string | null",
      "customPartName": "string | null",
      "instanceIdentifier": "string | null",
      "price": 0,
      "notes": "string | null",
      "updatesTracking": true
    }
  ]
}
```

### `POST /api/v1/maintenance-records`

**Request body:**

```json
{
  "userVehicleId": "uuid",
  "serviceDate": "yyyy-MM-dd",
  "odometerAtService": 12000,
  "garageName": "string | null",
  "totalCost": 0,
  "notes": "string | null",
  "invoiceImageUrl": "string | null",
  "items": [
    {
      "partCategorySlug": "string",
      "partProductId": "uuid | null",
      "customPartName": "string | null",
      "customKmInterval": 0,
      "customMonthsInterval": 0,
      "instanceIdentifier": "string | null",
      "price": 0,
      "itemNotes": "string | null",
      "updatesTracking": true
    }
  ]
}
```

- **Response `data`:**

```json
{
  "maintenanceRecordId": "uuid",
  "items": [
    {
      "maintenanceRecordItemId": "uuid",
      "partCategorySlug": "string",
      "tracking": {
        /* partTrackingSummary */
      }
    }
  ]
}
```

**201**

---

## 9. `/api/v1/part-categories`

### `GET /api/v1/part-categories`

- **Query:** `userVehicleId` (optional); nếu không có: thêm `pageNumber`, `pageSize`, `isDescending`
- **Response `data`:** `partCategorySummary[]` (có `userVehicleId` thì không phân trang theo `PaginationRequest`; không có thì có **`metadata`**)

### `GET /api/v1/part-categories/{id}`

- **Response `data`:** `partCategoryResponse`

### `GET /api/v1/part-categories/{partCategorySlug}/reminders`

- **Query:** `userVehicleId` (bắt buộc)
- **Response `data`:** `reminderDetailDto[]`

### `POST /api/v1/part-categories`

**Request body:**

```json
{
  "name": "string",
  "description": "string | null",
  "iconUrl": "string | null",
  "iconMediaFileId": "uuid | null",
  "displayOrder": 0,
  "requiresOdometerTracking": true,
  "requiresTimeTracking": true,
  "allowsMultipleInstances": false,
  "identificationSigns": "string | null",
  "consequencesIfNotHandled": "string | null"
}
```

- **Response `data`:** `partCategoryResponse` (**slug** server). **201**

### `PUT /api/v1/part-categories/{id}`

- **Request body:** giống POST
- **Response `data`:** `partCategoryResponse`

### `DELETE /api/v1/part-categories/{id}`

- **Response `data`:** string

---

## 10. `/api/v1/part-products`

### `GET /api/v1/part-products/category/{categoryId}`

- **Query:** `pageNumber`, `pageSize`, `isDescending`
- **Response `data`:** `partProductSummary[]` + **`metadata`**

### `GET /api/v1/part-products/{id}`

- **Response `data`:** `partProductResponse`

### `POST /api/v1/part-products`

**Request body:**

```json
{
  "partCategoryId": "uuid",
  "name": "string",
  "brand": "string | null",
  "description": "string | null",
  "imageUrl": "string | null",
  "referencePrice": 0,
  "recommendedKmInterval": 0,
  "recommendedMonthsInterval": 0
}
```

- **Response `data`:** `partProductResponse`. **201**

### `PUT /api/v1/part-products/{id}`

- **Request body:** giống POST
- **Response `data`:** `partProductResponse`

### `DELETE /api/v1/part-products/{id}`

- **Response `data`:** string

---
