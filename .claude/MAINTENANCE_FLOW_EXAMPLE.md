# Ví dụ Luồng Tạo User Vehicle và Bảo Dưỡng

## 1. Khi User Đăng Ký Xe Mới (với AI Analysis - Per Part)

### Flow Đăng Ký Xe Mới:

```
[User] → Chọn xe (Brand/Model/Variant)
   ↓
[User] → Nhập thông tin cơ bản (biển số, số km hiện tại)
   ↓
[System] → Tạo UserVehicle với NeedsOnboarding = true
   ↓
[System] → KHÔNG tạo tracking - đợi user chọn part
   ↓
[Frontend] → Hiển thị danh sách parts có thể track
   ↓
[User] → Chọn part để phân tích (e.g., engine_oil)
   ↓
[System] → Lấy DefaultSchedule cho part đó
   ↓
[Frontend] → Hiển thị câu hỏi cụ thể cho part
   ↓
[User] → Trả lời câu hỏi
   ↓
[AI] → Phân tích và đề xuất (1 part/request)
   ↓
[User] → Review và xác nhận
   ↓
[System] → TẠO MỚI VehiclePartTracking cho part đó (nếu chưa có)
   ↓
[User] → Có thể quay lại chọn part khác để track sau
   ↓
[System] → Set NeedsOnboarding = false khi user hoàn tất (optional)
```

### Bước 1: Tạo UserVehicle Với Default Tracking

```json
POST /api/v1/user-vehicles
{
  "vehicleVariantId": "guid-honda-wave-alpha-red",
  "licensePlate": "59H1-12345",
  "vinNumber": "MLHJF1234567890",
  "purchaseDate": "2024-01-15",
  "currentOdometer": 8500
}

Response:
{
  "isSuccess": true,
  "data": {
    "id": "user-vehicle-guid",
    "userId": "user-guid",
    "licensePlate": "59H1-12345",
    "vinNumber": "MLHJF1234567890",
    "purchaseDate": "2024-01-15T00:00:00Z",
    "currentOdometer": 8500,
    "needsOnboarding": true,
    "userVehicleVariant": {
      "variantName": "Đỏ",
      "modelName": "Honda Wave Alpha",
      "brandName": "Honda"
    }
  },
  "message": "Thêm xe thành công"
}

// System tự động tạo:
// 1. OdometerHistory: initial 8500km
// 2. KHÔNG tạo VehiclePartTracking - sẽ tạo khi user chọn phân tích part
```

### Bước 2: Frontend Lấy Default Schedule Cho Linh Kiện

**Ví dụ: User chọn phân tích engine_oil**

```json
GET /api/v1/vehicle-models/{vehicleModelId}/part-categories/engine_oil/default-schedule

Response:
{
  "isSuccess": true,
  "data": {
    "id": "schedule-guid",
    "partCategoryId": "engine-oil-guid",
    "partCategoryCode": "engine_oil",
    "partCategoryName": "Dầu động cơ",
    "partCategoryDescription": "Dầu bôi trơn động cơ",
    "iconUrl": "https://...",
    "initialKm": 1000,
    "kmInterval": 2000,
    "monthsInterval": 6,
    "requiresOdometerTracking": true,
    "requiresTimeTracking": true,
    "displayOrder": 1
  },
  "message": "Lấy lịch bảo dưỡng thành công"
}
```

### Bước 3: User Trả Lời Câu Hỏi Cho Linh Kiện

**Frontend hiển thị form cho engine_oil:**

```
┌─────────────────────────────────────────────────────────┐
│ 🛢️ Dầu động cơ (engine_oil)                             │
│                                                          │
│ Lịch chuẩn: Lần đầu 1000km, sau đó 2000km/6 tháng       │
│                                                          │
│ 🤖 Trả lời câu hỏi để AI ước tính chính xác:            │
│ ┌────────────────────────────────────────────────────┐ │
│ │ Khi nào thay dầu gần nhất?                        │ │
│ │ [Cách đây 1 tháng]                                │ │
│ │                                                    │ │
│ │ Xe chạy bao nhiêu km khi thay?                    │ │
│ │ [7200km]                                          │ │
│ │                                                    │ │
│ │ Loại dầu đã sử dụng?                              │ │
│ │ [Castrol 10W-40]                                  │ │
│ └────────────────────────────────────────────────────┘ │
│                                                          │
│ [Bỏ qua]  [Phân tích với AI] →                          │
└─────────────────────────────────────────────────────────┘
```

### Bước 4: Gửi Lên AI Service (1 Linh Kiện)

```json
POST /api/v1/ai/vehicle-questionnaire/analyze
{
  "userId": "user-guid", // Tự động từ JWT
  "userVehicleId": "user-vehicle-guid",
  "vehicleInfo": {
    "brand": "Honda",
    "model": "Wave Alpha",
    "variant": "Đỏ",
    "isUsedVehicle": true,
    "currentOdometer": 8500,
    "purchaseDate": "2024-01-15"
  },
  "defaultSchedules": [
    {
      "partCategoryCode": "engine_oil",
      "partCategoryName": "Dầu động cơ",
      "initialKm": 1000,
      "kmInterval": 2000,
      "monthsInterval": 6
    }
  ],
  "answers": [
    {
      "question": "Khi nào thay dầu gần nhất?",
      "value": "Cách đây 1 tháng"
    },
    {
      "question": "Xe chạy bao nhiêu km khi thay?",
      "value": "7200km"
    },
    {
      "question": "Loại dầu đã sử dụng?",
      "value": "Castrol 10W-40"
    }
  ]
}
```

**Validation:**

- Chỉ chấp nhận 1 linh kiện trong `defaultSchedules`
- Nếu >1: "Chỉ hỗ trợ phân tích 1 linh kiện trong mỗi request"

### Bước 5: AI Phân Tích (Backend - Gemini 2.0-flash)

**Prompt gửi cho Gemini (tiếng Việt để tiết kiệm token):**

```
Hôm nay: 2025-01-25

XE:
Honda Wave Alpha Đỏ - Xe cũ - 8,500km - Mua: 2024-01-15

LỊCH CHUẨN:
- engine_oil: Lần đầu 1000km, sau đó 2000km/6 tháng

THÔNG TIN TỪ NGƯỜI DÙNG:
- Khi nào thay dầu gần nhất?: Cách đây 1 tháng
- Xe chạy bao nhiêu km khi thay?: 7200km
- Loại dầu đã sử dụng?: Castrol 10W-40

YÊU CẦU:
Phân tích để ước tính lần thay cuối và lần thay tiếp theo cho linh kiện.
- Ưu tiên: Câu trả lời người dùng > Tính theo km > Lịch chuẩn
- Nếu "thay gần đây", tính thời gian tương đối từ hôm nay
- Nếu "không nhớ", ước tính dựa trên tổng km và chu kỳ

Trả về JSON:
{
  "recommendations": [
    {
      "partCategoryCode": "mã_linh_kiện",
      "lastServiceOdometer": số_km,
      "lastServiceDate": "yyyy-MM-dd",
      "predictedNextOdometer": số_km,
      "predictedNextDate": "yyyy-MM-dd",
      "confidenceScore": 0.0-1.0,
      "reasoning": "lý_do_ngắn_gọn",
      "needsImmediateAttention": true/false
    }
  ],
  "warnings": []
}
```

**AI Response (Gemini):**

```json
{
  "isSuccess": true,
  "data": {
    "recommendations": [
      {
        "partCategoryCode": "engine_oil",
        "partCategoryName": "Dầu động cơ",
        "lastReplacementOdometer": 7200,
        "lastReplacementDate": "2024-12-25",
        "predictedNextOdometer": 9200,
        "predictedNextDate": "2025-06-25",
        "confidenceScore": 0.85,
        "reasoning": "Dầu thay cách đây 1 tháng lúc 7200km, đã chạy 1300km. Dự kiến thay tiếp ở 9200km (7200+2000) hoặc 6 tháng sau (2025-06-25).",
        "needsImmediateAttention": false
      }
    ],
    "warnings": [],
    "metadata": {
      "model": "gemini-2.0-flash",
      "totalTokens": 450,
      "totalCost": 0.000225,
      "responseTimeMs": 850
    }
  },
  "message": "Phân tích thành công"
}
```

**Validation:**

- AI phải trả về đúng 1 recommendation
- Nếu AI trả về nhiều hơn: "AI trả về 2 khuyến nghị thay vì 1. Vui lòng thử lại."

### Bước 6: Frontend Hiển Thị Kết Quả AI (1 Linh Kiện)

```
┌─────────────────────────────────────────────────────────┐
│ 🤖 AI đã phân tích xong - Dầu động cơ                   │
│                                                          │
│ 🛢️ Kết quả phân tích:                                   │
│ ┌─────────────────────────────────────────────────┐    │
│ │ ✅ Dầu động cơ - Confidence: 85%                │    │
│ │                                                  │    │
│ │ Thay lần cuối:                                   │    │
│ │ • 7200 km (25/12/2024)                           │    │
│ │                                                  │    │
│ │ Dự kiến thay tiếp:                               │    │
│ │ • 9200 km hoặc 25/06/2025                        │    │
│ │                                                  │    │
│ │ 📝 Lý do:                                        │    │
│ │ "Dầu thay cách đây 1 tháng lúc 7200km, đã      │    │
│ │  chạy 1300km. Dự kiến thay tiếp ở 9200km       │    │
│ │  (7200+2000) hoặc 6 tháng sau (2025-06-25)."   │    │
│ │                                                  │    │
│ │ ⚠️ Cần chú ý ngay: Không                         │    │
│ └─────────────────────────────────────────────────┘    │
│                                                          │
│ [Sửa lại]  [Xác nhận] →                                 │
└─────────────────────────────────────────────────────────┘
```

### Bước 7: User Xác Nhận và Apply Tracking Config

```json
POST /api/v1/user-vehicles/{userVehicleId}/apply-tracking
{
  "partCategoryCode": "engine_oil",
  "lastReplacementOdometer": 7200,
  "lastReplacementDate": "2024-12-25",
  "predictedNextOdometer": 9200,
  "predictedNextDate": "2025-06-25",
  "aiReasoning": "Dầu thay cách đây 1 tháng lúc 7200km...",
  "confidenceScore": 0.85
}

Response:
{
  "isSuccess": true,
  "data": {
    "id": "tracking-guid",
    "partCategoryId": "engine-oil-guid",
    "partCategoryName": "Dầu động cơ",
    "partCategoryCode": "engine_oil",
    "lastReplacementOdometer": 7200,
    "lastReplacementDate": "2024-12-25",
    "customKmInterval": 2000,
    "customMonthsInterval": 6,
    "predictedNextOdometer": 9200,
    "predictedNextDate": "2025-06-25",
    "aiAnalysisResult": "{\"reasoning\":\"...\",\"confidenceScore\":0.85,\"analyzedAt\":\"2025-01-25T10:30:00Z\"}"
  },
  "message": "Áp dụng cấu hình tracking thành công"
}
```

**System update:**

```
CREATED NEW VehiclePartTracking (engine_oil):
├─ CustomKmInterval: 2000 (từ default schedule)
├─ CustomMonthsInterval: 6 (từ default schedule)
├─ LastReplacementOdometer: 7200 (từ AI)
├─ LastReplacementDate: 2024-12-25 (từ AI)
├─ PredictedNextOdometer: 9200 (từ AI)
├─ PredictedNextDate: 2025-06-25 (từ AI)
└─ AiAnalysisResult: JSON với reasoning và confidence
```

### Bước 8: Lặp Lại Cho Part Khác (Optional)

```
[User] → Chọn part tiếp theo (e.g., brake_pad)
   ↓
Lặp lại Bước 2-7 cho brake_pad
   ↓
[User] → Không chọn part nào khác
   ↓
[System] → Chỉ có parts đã chọn mới có tracking
```

**Kết quả cuối:**

```
VehiclePartTracking:
├─ engine_oil: ✅ Tracked (có LastReplacement từ AI)
├─ brake_pad: ✅ Tracked (có LastReplacement từ AI)
├─ oil_filter: ❌ KHÔNG CÓ TRACKING (user chưa chọn)
└─ air_filter: ❌ KHÔNG CÓ TRACKING (user chưa chọn)
```

**Lưu ý quan trọng:**

- Tracking chỉ được tạo khi user chọn phân tích part
- User có thể quay lại sau để thêm tracking cho part khác
- Không có tracking = không có reminder cho part đó

### Bước 9: Hoàn Thành Onboarding (Optional)

**Khi user hoàn thành hoặc muốn skip onboarding:**

```json
PATCH /api/v1/user-vehicles/{userVehicleId}/complete-onboarding

Response:
{
  "isSuccess": true,
  "data": {
    "id": "user-vehicle-guid",
    "userId": "user-guid",
    "licensePlate": "59H1-12345",
    "currentOdometer": 8500,
    "needsOnboarding": false,  // Changed to false
    "userVehicleVariant": {
      "variantName": "Đỏ",
      "modelName": "Honda Wave Alpha",
      "brandName": "Honda"
    }
  },
  "message": "Hoàn thành onboarding thành công"
}
```

**Note:**

- Endpoint này đặt `NeedsOnboarding = false`
- Frontend có thể dùng để ẩn onboarding wizard sau khi user hoàn thành
- User có thể hoàn thành onboarding dù đã phân tích 0, 1 hoặc nhiều linh kiện

---

## 2. Alternative Flow: User Chọn "Bỏ Qua AI"

```
[User] → Chọn "Bỏ qua - Dùng mặc định"
   ↓
[System] → Hiển thị form manual
   ↓
[User] → Nhập odometer hiện tại
   ↓
[System] → Tạo xe với DefaultSchedule
   ↓
All parts: LastReplacement = 0 / PurchaseDate
```

---

## 3. User Cập Nhật Odometer Sau Đó

**30 ngày sau, user update:**

```json
PUT /api/user-vehicles/{id}/odometer
{
  "currentOdometer": 9000
}
```

**System tự động:**

```
1. Update UserVehicle.CurrentOdometer = 9000
2. Tạo OdometerHistory mới
3. Background job tính lại reminders:

   Dầu máy:
   - Km used: 9000 - 7200 = 1800
   - Km interval: 2000
   - Remaining: 200km (10%)
   - Level: HIGH → Gửi notification

   Má phanh:
   - Km used: 9000 - 0 = 9000
   - Km interval: 10000
   - Remaining: 1000km (10%)
   - Level: HIGH → Gửi notification
```

---

## 4. User Đi Bảo Dưỡng

**Flow bảo dưỡng:**

```
[User] → Click reminder "Má phanh cần thay"
   ↓
[App] → Mở form tạo maintenance record
   ↓
[User] → Nhập thông tin bảo dưỡng
   ↓
[System] → Tạo MaintenanceRecord + Items
   ↓
[System] → Update VehiclePartTracking
   ├─ LastReplacement = new values
   ├─ PredictedNext = recalculate
   └─ CurrentProduct = new product
   ↓
[System] → Dismiss old reminders
   ↓
[System] → Create OdometerHistory from service
```

**Form bảo dưỡng:**

```json
POST /api/maintenance-records
{
  "userVehicleId": "guid",
  "serviceDate": "2025-01-25",
  "odometerAtService": 9100,
  "serviceProvider": "Đại lý Honda",
  "items": [
    {
      "partCategoryId": "brake-pad-guid",
      "partProductId": "honda-brake-pad-guid",
      "quantity": 1,
      "unitPrice": 120000,
      "customKmInterval": null, // dùng default
      "customMonthsInterval": null
    }
  ]
}
```

**System update tracking:**

```
VehiclePartTracking (Má phanh):
├─ LastReplacementOdometer: 0 → 9100
├─ LastReplacementDate: 2024-01-15 → 2025-01-25
├─ CurrentPartProductId: null → honda-brake-pad-guid
└─ PredictedNext: 10000 → 19100 (9100 + 10000)

MaintenanceReminder (Má phanh):
└─ IsDismissed: false → true
```

---

## 5. Flow với Generative AI cho Bảo Dưỡng

**User có thể dùng AI để phân tích sau bảo dưỡng:**

```
[User] → "Tôi vừa đi bảo dưỡng xe"
   ↓
[System] → Hiển thị form AI analysis
   ↓
[User] → Paste hóa đơn / Mô tả text
   ↓
[AI] → Parse và extract thông tin
   ↓
[System] → Pre-fill form maintenance record
   ↓
[User] → Xác nhận và save
```

**Ví dụ user paste hóa đơn:**

```
"HÓA ĐƠN BẢO DƯỠNG
Đại lý Honda Hà Nội
Ngày: 25/01/2025
Biển số: 59H1-12345
Số km: 9100

1. Thay má phanh trước Honda: 120,000đ
2. Kiểm tra phanh sau: Miễn phí
3. Công thay: 50,000đ

Tổng: 170,000đ"
```

**AI phân tích:**

```json
{
  "serviceDate": "2025-01-25",
  "odometerAtService": 9100,
  "serviceProvider": "Đại lý Honda Hà Nội",
  "totalCost": 170000,
  "items": [
    {
      "partCategoryId": "brake-pad-guid",
      "partName": "Má phanh",
      "partProductName": "Honda chính hãng",
      "quantity": 1,
      "unitPrice": 120000
    }
  ],
  "notes": "Bao gồm công thay 50,000đ. Má phanh sau còn tốt.",
  "confidence": 0.95
}
```

---

## 6. Notification Flow

**Background Service chạy hàng ngày:**

```
Mỗi ngày 08:00 AM:
   ↓
Query all active reminders
   ↓
For each reminder with level >= HIGH và chưa notify:
   ├─ Tính km/days remaining
   ├─ Generate notification message
   └─ Send via Notification Service
      ├─ Push notification
      ├─ In-app notification
      └─ Email (nếu Critical)
```

**Notification messages:**

```
Level HIGH (10-25% remaining):
"🟡 Honda Wave Alpha (59H1-12345)
Dầu máy còn 200km nữa cần thay.
Dự kiến: ~7 ngày nữa"

Level Critical (<10% remaining):
"🔴 KHẨN CẤP!
Honda Wave Alpha (59H1-12345)
Dầu máy đã vượt 100km so với khuyến nghị!
Nên đi bảo dưỡng ngay."
```

---

## 7. Dashboard Query Flow

```
GET /api/user-vehicles/{id}/dashboard

Response:
{
  "vehicle": {
    "model": "Honda Wave Alpha",
    "licensePlate": "59H1-12345",
    "currentOdometer": 9000,
    "averageKmPerDay": 16,
    "nextOdometerMilestone": 10000
  },
  "upcomingMaintenance": [
    {
      "partName": "Dầu máy",
      "status": "HIGH",
      "urgencyLevel": 85,
      "kmRemaining": 200,
      "daysRemaining": 12,
      "predictedDate": "2025-02-06",
      "lastService": {
        "odometer": 7200,
        "date": "2024-12-23",
        "product": "Castrol 10W-40"
      }
    }
  ],
  "recentMaintenance": [
    {
      "date": "2025-01-25",
      "odometer": 9100,
      "items": ["Má phanh"],
      "cost": 170000,
      "provider": "Đại lý Honda"
    }
  ],
  "healthScore": 85,
  "recommendations": [
    "Dầu máy cần thay trong 200km tới",
    "Tình trạng xe tốt, không có vấn đề đáng lo"
  ]
}
```

---

## Tóm Tắt Key Flows

### 1️⃣ Đăng ký xe với AI

```
Input text → AI analyze → Confirm → Create with accurate tracking
```

### 2️⃣ Cập nhật km

```
Manual input → Auto calculate reminders → Send notifications
```

### 3️⃣ Bảo dưỡng với AI

```
Paste hóa đơn → AI parse → Pre-fill form → Confirm → Update tracking
```

### 4️⃣ Nhắc nhở tự động

```
Background job → Calculate urgency → Send notifications → Track dismissed
```

### 5️⃣ Dashboard

```
Query → Aggregate data → Calculate health score → Show recommendations
```

---

## Ưu Điểm Thiết Kế

✅ **Accurate từ đầu**: AI giúp xác định tình trạng xe chính xác ngay khi đăng ký

✅ **Flexible**: User có thể bypass AI nếu muốn nhập manual

✅ **Smart parsing**: AI đọc hóa đơn tự động khi bảo dưỡng

✅ **Proactive**: System chủ động tính toán và nhắc nhở

✅ **Traceable**: Lưu lại history đầy đủ cho audit

✅ **User-friendly**: Giảm thiểu input manual, tăng UX
