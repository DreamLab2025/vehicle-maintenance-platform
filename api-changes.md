# API Changes — Vehicle Service

> Generated: 2026-03-21
> All JSON field names are **unchanged**. Only route shapes, a few query/body params, and list response shapes changed.

---

## 1. Odometer History

### `PATCH /api/v1/odometer-history/{userVehicleId}` ← route changed

|       | Before                                                        | After                                            |
| ----- | ------------------------------------------------------------- | ------------------------------------------------ |
| Route | `PATCH /api/v1/odometer-history/user-vehicle/{userVehicleId}` | `PATCH /api/v1/odometer-history/{userVehicleId}` |

**Request body — unchanged:**

```json
{
  "currentOdometer": 12500
}
```

**Response `200` — unchanged:**

```json
{
  "id": "guid",
  "userId": "guid",
  "licensePlate": "string",
  "vinNumber": "string",
  "purchaseDate": "datetime",
  "currentOdometer": 12500,
  "lastOdometerUpdateAt": "datetime",
  "averageKmPerDay": 30,
  "needsOnboarding": false,
  "createdAt": "datetime",
  "updatedAt": "datetime",
  "userVehicleVariant": { ... }
}
```

---

### `GET /api/v1/odometer-history` ← route changed

|       | Before                                                      | After                                             |
| ----- | ----------------------------------------------------------- | ------------------------------------------------- |
| Route | `GET /api/v1/odometer-history/user-vehicle/{userVehicleId}` | `GET /api/v1/odometer-history?userVehicleId={id}` |

**Query params:**

| Param           | Type       | Required | Notes                             |
| --------------- | ---------- | -------- | --------------------------------- |
| `userVehicleId` | `guid`     | **Yes**  | was path param                    |
| `page`          | `int`      | No       | default 1                         |
| `pageSize`      | `int`      | No       | default 20, max 100               |
| `fromDate`      | `DateOnly` | No       | `yyyy-MM-dd`                      |
| `toDate`        | `DateOnly` | No       | `yyyy-MM-dd`, must be >= fromDate |

**Response `200` — unchanged:**

```json
{
  "data": [
    {
      "id": "guid",
      "userVehicleId": "guid",
      "odometerValue": 12500,
      "recordedDate": "2025-03-21",
      "kmOnRecordedDate": 30,
      "source": "Manual"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 5
}
```

---

## 2. Brands

### `GET /api/v1/brands` ← filter by type merged into same route

|                | Before                              | After                                |
| -------------- | ----------------------------------- | ------------------------------------ |
| All brands     | `GET /api/v1/brands`                | `GET /api/v1/brands`                 |
| Filter by type | `GET /api/v1/brands/types/{typeId}` | `GET /api/v1/brands?typeId={typeId}` |

**Query params:**

| Param      | Type   | Required | Notes                  |
| ---------- | ------ | -------- | ---------------------- |
| `typeId`   | `guid` | No       | omit to get all brands |
| `page`     | `int`  | No       | default 1              |
| `pageSize` | `int`  | No       | default 20             |

**Response `200` — unchanged:**

```json
{
  "data": [
    {
      "id": "guid",
      "vehicleTypeId": "guid",
      "vehicleTypeName": "string",
      "name": "string",
      "logoUrl": "string",
      "website": "string",
      "supportPhone": "string",
      "createdAt": "datetime",
      "updatedAt": "datetime"
    }
  ]
}
```

---

## 3. Maintenance Records

### `GET /api/v1/maintenance-records` ← route changed

|       | Before                                                     | After                                                |
| ----- | ---------------------------------------------------------- | ---------------------------------------------------- |
| Route | `GET /api/v1/maintenance-records/vehicles/{userVehicleId}` | `GET /api/v1/maintenance-records?userVehicleId={id}` |

**Query params:**

| Param           | Type   | Required |
| --------------- | ------ | -------- |
| `userVehicleId` | `guid` | **Yes**  |

**Response `200` — unchanged:**

```json
{
  "data": [
    {
      "id": "guid",
      "userVehicleId": "guid",
      "serviceDate": "2025-03-21",
      "odometerAtService": 12000,
      "garageName": "string",
      "totalCost": 500000,
      "notes": "string",
      "invoiceImageUrl": "string",
      "itemCount": 2
    }
  ]
}
```

---

### `POST /api/v1/maintenance-records` ← route changed + body changed

|       | Before                                                      | After                              |
| ----- | ----------------------------------------------------------- | ---------------------------------- |
| Route | `POST /api/v1/maintenance-records/vehicles/{userVehicleId}` | `POST /api/v1/maintenance-records` |

**Request body — `userVehicleId` moved from path into body:**

```json
{
  "userVehicleId": "guid",            ← ADDED (was path param)
  "serviceDate": "2025-03-21",
  "odometerAtService": 12000,
  "garageName": "Garage ABC",
  "totalCost": 500000,
  "notes": "string",
  "invoiceImageUrl": "string",
  "items": [
    {
      "partCategoryCode": "OIL_FILTER",
      "partProductId": "guid | null",
      "customPartName": "string",       ← required if partProductId is null
      "customKmInterval": 3000,         ← required if no partProductId + updatesTracking=true
      "customMonthsInterval": 3,        ← required if no partProductId + updatesTracking=true
      "instanceIdentifier": "string",
      "price": 120000,
      "itemNotes": "string",
      "updatesTracking": true
    }
  ]
}
```

**Validation rules for items:**

- `partCategoryCode` — required, max 50 chars
- `customPartName` — required if `partProductId` is null
- `customKmInterval` or `customMonthsInterval` — at least one required if `partProductId` is null and `updatesTracking` is true
- `price` — must be >= 0 if provided
- `instanceIdentifier` — max 50 chars
- `itemNotes` — max 500 chars
- `totalCost` — must be >= 0 if provided
- `garageName` — max 200 chars
- `notes` — max 2000 chars
- `invoiceImageUrl` — max 500 chars

**Response `201` — unchanged:**

```json
{
  "maintenanceRecordId": "guid",
  "items": [
    {
      "maintenanceRecordItemId": "guid",
      "partCategoryCode": "OIL_FILTER",
      "tracking": {
        "id": "guid",
        "partCategoryId": "guid",
        "partCategoryName": "string",
        "partCategoryCode": "string",
        "instanceIdentifier": "string",
        "currentPartProductId": "guid",
        "currentPartProductName": "string",
        "lastReplacementOdometer": 12000,
        "lastReplacementDate": "2025-03-21",
        "customKmInterval": 3000,
        "customMonthsInterval": 3,
        "predictedNextOdometer": 15000,
        "predictedNextDate": "2025-06-21",
        "isDeclared": true,
        "reminders": [ ... ]
      }
    }
  ]
}
```

---

## 4. Part Categories

### `GET /api/v1/part-categories` ← filter by vehicle merged into same route

|                 | Before                                                 | After                                                   |
| --------------- | ------------------------------------------------------ | ------------------------------------------------------- |
| All categories  | `GET /api/v1/part-categories`                          | `GET /api/v1/part-categories`                           |
| By user vehicle | `GET /api/v1/part-categories/user-vehicle/{vehicleId}` | `GET /api/v1/part-categories?userVehicleId={vehicleId}` |

**Query params:**

| Param               | Type   | Required | Notes                                        |
| ------------------- | ------ | -------- | -------------------------------------------- |
| `userVehicleId`     | `guid` | No       | omit to get all categories                   |
| `page` / `pageSize` | `int`  | No       | only applied when `userVehicleId` is omitted |

**Response `200` — unchanged:**

```json
{
  "data": [
    {
      "id": "guid",
      "name": "string",
      "code": "OIL_FILTER",
      "description": "string",
      "iconUrl": "string",
      "displayOrder": 1,
      "status": "Active",
      "requiresOdometerTracking": true,
      "requiresTimeTracking": true,
      "allowsMultipleInstances": false,
      "identificationSigns": "string",
      "consequencesIfNotHandled": "string",
      "createdAt": "datetime",
      "updatedAt": "datetime"
    }
  ]
}
```

---

### `GET /api/v1/part-categories/{code}/reminders` ← route changed

|       | Before                                                                      | After                                                             |
| ----- | --------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| Route | `GET /api/v1/part-categories/{code}/reminders/user-vehicle/{userVehicleId}` | `GET /api/v1/part-categories/{code}/reminders?userVehicleId={id}` |

**Query params:**

| Param           | Type   | Required |
| --------------- | ------ | -------- |
| `userVehicleId` | `guid` | **Yes**  |

**Response `200` — unchanged:**

```json
{
  "data": [
    {
      "id": "guid",
      "vehiclePartTrackingId": "guid",
      "level": "Warning",
      "currentOdometer": 14500,
      "targetOdometer": 15000,
      "remainingKm": 500,
      "targetDate": "2025-06-21",
      "percentageRemaining": 16.67,
      "isNotified": false,
      "notifiedDate": null,
      "isDismissed": false,
      "dismissedDate": null,
      "isCurrent": true,
      "partCategory": {
        "id": "guid",
        "name": "string",
        "code": "OIL_FILTER",
        "description": "string",
        "iconUrl": "string",
        "identificationSigns": "string",
        "consequencesIfNotHandled": "string"
      }
    }
  ]
}
```

---

## 5. Parts — Split into Two Resources

`/api/v1/parts/**` no longer exists. Two new base paths:

### Part Products — `/api/v1/part-products`

```
GET    /api/v1/part-products/category/{categoryId}   # list by category
GET    /api/v1/part-products/{id}                    # by ID
POST   /api/v1/part-products                         # create (Admin)
PUT    /api/v1/part-products/{id}                    # update (Admin)
DELETE /api/v1/part-products/{id}                    # delete (Admin)
```

**`POST` / `PUT` request body:**

```json
{
  "partCategoryId": "guid",
  "name": "string",
  "brand": "string",
  "description": "string",
  "imageUrl": "string",
  "referencePrice": 120000,
  "recommendedKmInterval": 3000,
  "recommendedMonthsInterval": 3
}
```

**Response:**

```json
{
  "id": "guid",
  "partCategoryId": "guid",
  "partCategoryName": "string",
  "name": "string",
  "brand": "string",
  "description": "string",
  "imageUrl": "string",
  "referencePrice": 120000,
  "recommendedKmInterval": 3000,
  "recommendedMonthsInterval": 3,
  "status": "Active",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

---

## 6. Vehicle Models — Variants

### `GET /api/v1/models/{id}/variants` ← moved

|                       | Before                                                        | After                              |
| --------------------- | ------------------------------------------------------------- | ---------------------------------- |
| Get variants by model | `GET /api/v1/vehicle-models/{modelId}` _(old variants route)_ | `GET /api/v1/models/{id}/variants` |

**Response `200` — unchanged:**

```json
{
  "data": [
    {
      "id": "guid",
      "vehicleModelId": "guid",
      "color": "Đen",
      "hexCode": "#000000",
      "imageUrl": "string",
      "createdAt": "datetime",
      "updatedAt": "datetime"
    }
  ]
}
```

### Variant Admin CRUD — `/api/v1/variants`

```
POST   /api/v1/variants        # create
PUT    /api/v1/variants/{id}   # update
DELETE /api/v1/variants/{id}   # delete
```

**`POST` request body:**

```json
{
  "vehicleModelId": "guid",
  "color": "Đen",
  "hexCode": "#000000",
  "imageUrl": "string"
}
```

**`PUT` request body:**

```json
{
  "color": "Đen",
  "hexCode": "#000000",
  "imageUrl": "string"
}
```

---

## 7. Default Maintenance Schedule

|                              | Before                                                                    | After                                             |
| ---------------------------- | ------------------------------------------------------------------------- | ------------------------------------------------- |
| Schedule by model + category | `GET /api/v1/vehicle-models/{id}/part-categories/{code}/default-schedule` | **Removed**                                       |
| Active categories for model  | _(new)_                                                                   | `GET /api/v1/vehicle-models/{id}/part-categories` |

**`GET /api/v1/vehicle-models/{vehicleModelId}/part-categories` response:**

```json
{
  "data": [
    {
      "id": "guid",
      "name": "string",
      "code": "OIL_FILTER",
      "description": "string",
      "iconUrl": "string",
      "displayOrder": 1,
      "status": "Active",
      "requiresOdometerTracking": true,
      "requiresTimeTracking": true,
      "allowsMultipleInstances": false,
      "identificationSigns": "string",
      "consequencesIfNotHandled": "string",
      "createdAt": "datetime",
      "updatedAt": "datetime"
    }
  ]
}
```

---

## 8. Summary vs Detail Response Shapes

All list endpoints now return lean summary DTOs. Use `GET /{id}` to get the full detail.

### Brands

| Endpoint                            | Response               |
| ----------------------------------- | ---------------------- |
| `GET /api/v1/brands`                | `BrandSummary[]`       |
| `GET /api/v1/brands/{id}` ← **NEW** | `BrandResponse` (full) |

**`BrandSummary`:**

```json
{
  "id": "guid",
  "vehicleTypeId": "guid",
  "vehicleTypeName": "string",
  "name": "string",
  "logoUrl": "string"
}
```

**`BrandResponse` (detail — unchanged):** adds `website`, `supportPhone`, `createdAt`, `updatedAt`

---

### Vehicle Types

| Endpoint                           | Response              |
| ---------------------------------- | --------------------- |
| `GET /api/v1/types`                | `TypeSummary[]`       |
| `GET /api/v1/types/{id}` ← **NEW** | `TypeResponse` (full) |

**`TypeSummary`:**

```json
{ "id": "guid", "name": "string", "imageUrl": "string" }
```

**`TypeResponse` (detail — unchanged):** adds `description`, `createdAt`, `updatedAt`

---

### Models

| Endpoint                  | Response                                      |
| ------------------------- | --------------------------------------------- |
| `GET /api/v1/models`      | `ModelSummary[]`                              |
| `GET /api/v1/models/{id}` | `ModelResponseWithVariants` (full + variants) |

**`ModelSummary`:**

```json
{
  "id": "guid",
  "name": "string",
  "brandId": "guid",
  "brandName": "string",
  "typeId": "guid",
  "typeName": "string",
  "releaseYear": 2022,
  "fuelTypeName": "Xăng",
  "transmissionTypeName": "Tay ga"
}
```

**`ModelResponseWithVariants` (detail):** adds `fuelType`, `transmissionType`, `engineDisplacementDisplay`, `engineCapacity`, `createdAt`, `updatedAt`, `variants[]`

---

### Part Categories

| Endpoint                                         | Response                      |
| ------------------------------------------------ | ----------------------------- |
| `GET /api/v1/part-categories`                    | `PartCategorySummary[]`       |
| `GET /api/v1/part-categories?userVehicleId={id}` | `PartCategorySummary[]`       |
| `GET /api/v1/part-categories/{id}`               | `PartCategoryResponse` (full) |

**`PartCategorySummary`:**

```json
{
  "id": "guid",
  "name": "string",
  "code": "OIL_FILTER",
  "iconUrl": "string",
  "displayOrder": 1,
  "status": "Active"
}
```

**`PartCategoryResponse` (detail):** adds `description`, `requiresOdometerTracking`, `requiresTimeTracking`, `allowsMultipleInstances`, `identificationSigns`, `consequencesIfNotHandled`, `createdAt`, `updatedAt`

---

### Part Products

| Endpoint                                          | Response                     |
| ------------------------------------------------- | ---------------------------- |
| `GET /api/v1/part-products/category/{categoryId}` | `PartProductSummary[]`       |
| `GET /api/v1/part-products/{id}`                  | `PartProductResponse` (full) |

**`PartProductSummary`:**

```json
{
  "id": "guid",
  "partCategoryId": "guid",
  "partCategoryName": "string",
  "name": "string",
  "brand": "string",
  "imageUrl": "string",
  "referencePrice": 120000,
  "status": "Active"
}
```

**`PartProductResponse` (detail):** adds `description`, `recommendedKmInterval`, `recommendedMonthsInterval`, `createdAt`, `updatedAt`

---

## 9. Unchanged APIs

Routes, request bodies, and responses are **100% identical**:

| Endpoint                                               | Notes                               |
| ------------------------------------------------------ | ----------------------------------- |
| `GET /api/v1/user-vehicles`                            | returns `UserVehicleResponse[]`     |
| `GET /api/v1/user-vehicles/{id}`                       | returns `UserVehicleDetailResponse` |
| `POST /api/v1/user-vehicles`                           | create                              |
| `PUT /api/v1/user-vehicles/{id}`                       | update                              |
| `DELETE /api/v1/user-vehicles/{id}`                    | soft delete                         |
| `GET /api/v1/user-vehicles/{id}/parts`                 | part summaries                      |
| `GET /api/v1/user-vehicles/{id}/reminders`             | all reminders                       |
| `GET /api/v1/user-vehicles/{id}/streak`                | streak                              |
| `POST /api/v1/user-vehicles/{id}/apply-tracking`       | AI tracking                         |
| `PATCH /api/v1/user-vehicles/{id}/complete-onboarding` | finish onboarding                   |
| `GET /api/v1/user-vehicles/is-allowed-create`          | permission check                    |
| `POST /api/v1/models`                                  | create (Admin)                      |
| `PUT /api/v1/models/{id}`                              | update (Admin)                      |
| `DELETE /api/v1/models/{id}`                           | delete (Admin)                      |
| `GET /api/v1/part-categories/{id}`                     | by ID → full detail                 |
| `POST /api/v1/part-categories`                         | create (Admin)                      |
| `PUT /api/v1/part-categories/{id}`                     | update (Admin)                      |
| `DELETE /api/v1/part-categories/{id}`                  | delete (Admin)                      |
| `POST /api/v1/types`                                   | create (Admin)                      |
| `PUT /api/v1/types/{id}`                               | update (Admin)                      |
| `DELETE /api/v1/types/{id}`                            | delete (Admin)                      |
| `POST /api/v1/brands`                                  | create (Admin)                      |
| `PUT /api/v1/brands/{id}`                              | update (Admin)                      |
| `DELETE /api/v1/brands/{id}`                           | delete (Admin)                      |
| `GET /api/v1/maintenance-records/{id}`                 | record detail                       |
| `GET /api/internal/vehicles/user-vehicles/{id}`        | internal                            |
