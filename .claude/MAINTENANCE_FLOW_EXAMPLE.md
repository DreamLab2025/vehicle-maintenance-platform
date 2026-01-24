# Ví dụ Luồng Tạo User Vehicle và Bảo Dưỡng

## 1. Khi User Đăng Ký Xe Mới (với AI Analysis)

### Flow Đăng Ký Xe:

```
[User] → Chọn xe (Brand/Model/Variant)
   ↓
[User] → Nhập thông tin cơ bản
   ↓
[System] → Hiển thị form câu hỏi về tình trạng xe
   ↓
[User] → Trả lời câu hỏi (text tự do)
   ↓
[AI] → Phân tích và đề xuất
   ↓
[User] → Xác nhận hoặc chỉnh sửa
   ↓
[System] → Tạo xe + Tracking + Reminders
```

### Bước 1: Form Thông Tin Cơ Bản

```json
POST /api/user-vehicles/initialize
{
  "vehicleVariantId": "guid-honda-wave-alpha-red",
  "licensePlate": "59H1-12345",
  "vinNumber": "MLHJF1234567890",
  "purchaseDate": "2024-01-15"
}

Response:
{
  "sessionId": "temp-session-guid",
  "vehicle": {
    "model": "Honda Wave Alpha",
    "variant": "Đỏ",
    "year": 2024
  },
  "defaultParts": [
    {
      "partCategoryId": "oil-guid",
      "name": "Dầu máy",
      "defaultInterval": "2000 km hoặc 6 tháng"
    },
    {
      "partCategoryId": "tire-guid",
      "name": "Lốp xe",
      "defaultInterval": "15000 km hoặc 2 năm"
    },
    {
      "partCategoryId": "brake-pad-guid",
      "name": "Má phanh",
      "defaultInterval": "10000 km hoặc 1 năm"
    }
  ],
  "aiQuestions": "Hãy mô tả tình trạng xe của bạn hiện tại..."
}
```

### Bước 2: User Trả Lời Câu Hỏi AI

**Frontend hiển thị form:**
```
┌─────────────────────────────────────────────────────────┐
│ 🤖 AI sẽ giúp bạn đánh giá tình trạng xe                │
│                                                          │
│ Vui lòng mô tả tình trạng xe của bạn:                   │
│ ┌────────────────────────────────────────────────────┐ │
│ │ - Xe đã chạy bao nhiêu km?                         │ │
│ │ - Lần cuối thay dầu, lốp, phanh là khi nào?       │ │
│ │ - Có phần nào vừa mới thay không?                 │ │
│ │ - Xe có vấn đề gì cần lưu ý không?                │ │
│ │                                                    │ │
│ │ [User nhập text tự do...]                         │ │
│ └────────────────────────────────────────────────────┘ │
│                                                          │
│ [Bỏ qua - Dùng mặc định]  [Phân tích với AI] →          │
└─────────────────────────────────────────────────────────┘
```

**Ví dụ user nhập:**
```
"Xe mình mua xe đã qua sử dụng, hiện tại đồng hồ đang 8500km.
Lần cuối thay dầu là cách đây 1 tháng lúc 7200km, dùng dầu Castrol.
Lốp thì còn mới, chủ cũ thay lúc 6000km.
Má phanh chưa thay bao giờ, phanh hơi kêu kẹt kẹt."
```

### Bước 3: Gửi Lên AI Service

```json
POST /api/user-vehicles/analyze-condition
{
  "sessionId": "temp-session-guid",
  "vehicleVariantId": "guid-honda-wave-alpha-red",
  "userDescription": "Xe mình mua xe đã qua sử dụng...",
  "defaultParts": [
    {
      "partCategoryId": "oil-guid",
      "name": "Dầu máy",
      "defaultKmInterval": 2000,
      "defaultMonthsInterval": 6
    },
    {
      "partCategoryId": "tire-guid",
      "name": "Lốp xe",
      "defaultKmInterval": 15000,
      "defaultMonthsInterval": 24
    },
    {
      "partCategoryId": "brake-pad-guid",
      "name": "Má phanh",
      "defaultKmInterval": 10000,
      "defaultMonthsInterval": 12
    }
  ]
}
```

### Bước 4: AI Phân Tích (Backend)

**Prompt gửi cho AI (Claude/GPT):**
```
Bạn là chuyên gia bảo dưỡng xe máy. Hãy phân tích mô tả sau của người dùng về xe Honda Wave Alpha:

User description: "Xe mình mua xe đã qua sử dụng, hiện tại đồng hồ đang 8500km.
Lần cuối thay dầu là cách đây 1 tháng lúc 7200km, dùng dầu Castrol.
Lốp thì còn mới, chủ cũ thay lúc 6000km.
Má phanh chưa thay bao giờ, phanh hơi kêu kẹt kẹt."

Default maintenance schedule:
- Dầu máy: 2000km hoặc 6 tháng
- Lốp xe: 15000km hoặc 24 tháng
- Má phanh: 10000km hoặc 12 tháng

Hãy trả về JSON với format:
{
  "currentOdometer": <số km hiện tại>,
  "analysisNotes": "<ghi chú tổng quan>",
  "parts": [
    {
      "partCategoryId": "oil-guid",
      "partName": "Dầu máy",
      "status": "GOOD|WARNING|URGENT|OVERDUE",
      "lastReplacementOdometer": <số km lần thay cuối>,
      "lastReplacementDate": "YYYY-MM-DD",
      "partProductName": "<tên sản phẩm nếu có>",
      "kmUntilNext": <km còn lại>,
      "predictedNextOdometer": <km dự kiến thay tiếp>,
      "predictedNextDate": "YYYY-MM-DD",
      "reasoningNote": "<lý do phân tích>",
      "urgencyScore": 0-100
    }
  ],
  "recommendations": [
    "<khuyến nghị 1>",
    "<khuyến nghị 2>"
  ]
}
```

**AI Response:**
```json
{
  "currentOdometer": 8500,
  "analysisNotes": "Xe đã qua sử dụng với số km vừa phải. Có một số phụ tùng cần chú ý kiểm tra.",
  "parts": [
    {
      "partCategoryId": "oil-guid",
      "partName": "Dầu máy",
      "status": "GOOD",
      "lastReplacementOdometer": 7200,
      "lastReplacementDate": "2024-12-23",
      "partProductName": "Castrol 10W-40",
      "kmUntilNext": 800,
      "predictedNextOdometer": 9200,
      "predictedNextDate": "2025-06-23",
      "reasoningNote": "Dầu vừa thay cách đây 1 tháng và 1300km. Còn 800km nữa đến lần thay tiếp theo theo chu kỳ 2000km.",
      "urgencyScore": 35
    },
    {
      "partCategoryId": "tire-guid",
      "partName": "Lốp xe",
      "status": "GOOD",
      "lastReplacementOdometer": 6000,
      "lastReplacementDate": "2024-06-15",
      "partProductName": null,
      "kmUntilNext": 12500,
      "predictedNextOdometer": 21000,
      "predictedNextDate": "2026-06-15",
      "reasoningNote": "Lốp thay lúc 6000km, hiện tại 8500km mới chạy 2500km. Còn rất mới và đủ tuổi thọ.",
      "urgencyScore": 15
    },
    {
      "partCategoryId": "brake-pad-guid",
      "partName": "Má phanh",
      "status": "WARNING",
      "lastReplacementOdometer": 0,
      "lastReplacementDate": "2024-01-15",
      "partProductName": null,
      "kmUntilNext": 1500,
      "predictedNextOdometer": 10000,
      "predictedNextDate": "2025-01-15",
      "reasoningNote": "Má phanh chưa từng thay và đã 8500km. Có dấu hiệu kêu kẹt, nên kiểm tra và thay trong 1500km tới.",
      "urgencyScore": 65
    }
  ],
  "recommendations": [
    "🔧 Má phanh cần kiểm tra gấp do có tiếng kêu bất thường",
    "✅ Dầu máy tình trạng tốt, còn 800km nữa mới cần thay",
    "✅ Lốp xe còn mới, không cần lo lắng trong thời gian tới",
    "📅 Nên đặt lịch kiểm tra má phanh trong tuần tới"
  ]
}
```

### Bước 5: Frontend Hiển Thị Kết Quả AI

```
┌─────────────────────────────────────────────────────────┐
│ 🤖 AI đã phân tích xong!                                 │
│                                                          │
│ 📊 Tình trạng xe: Honda Wave Alpha - 8500 km            │
│ "Xe đã qua sử dụng với số km vừa phải..."              │
│                                                          │
│ ┌─────────────────────────────────────────────────┐    │
│ │ ✅ Dầu máy - TÌNH TRẠNG TỐT (35%)               │    │
│ │ • Thay lần cuối: 7200km (23/12/2024)            │    │
│ │ • Sản phẩm: Castrol 10W-40                      │    │
│ │ • Còn lại: 800km (~2 tháng)                     │    │
│ │ • Thay tiếp: ~9200km (23/06/2025)               │    │
│ │ [────────────────████████████] 35%              │    │
│ │ ✏️ Sửa                                           │    │
│ └─────────────────────────────────────────────────┘    │
│                                                          │
│ ┌─────────────────────────────────────────────────┐    │
│ │ ⚠️ Má phanh - CẦN CHÚ Ý (65%)                   │    │
│ │ • Chưa thay lần nào                              │    │
│ │ • Đã chạy: 8500km                                │    │
│ │ • Còn lại: 1500km (~3 tháng)                    │    │
│ │ • Có tiếng kêu bất thường                        │    │
│ │ [──────────██████████████████] 65%              │    │
│ │ ✏️ Sửa                                           │    │
│ └─────────────────────────────────────────────────┘    │
│                                                          │
│ ┌─────────────────────────────────────────────────┐    │
│ │ ✅ Lốp xe - TÌNH TRẠNG TỐT (15%)                │    │
│ │ • Thay lần cuối: 6000km (15/06/2024)            │    │
│ │ • Còn lại: 12500km (~2 năm)                     │    │
│ │ [───████████████████████████] 15%               │    │
│ │ ✏️ Sửa                                           │    │
│ └─────────────────────────────────────────────────┘    │
│                                                          │
│ 💡 Khuyến nghị:                                          │
│ • 🔧 Má phanh cần kiểm tra gấp do có tiếng kêu          │
│ • 📅 Nên đặt lịch kiểm tra má phanh trong tuần tới     │
│                                                          │
│ [Sửa lại]  [Xác nhận và tạo xe] →                      │
└─────────────────────────────────────────────────────────┘
```

### Bước 6: User Xác Nhận và Tạo Xe

```json
POST /api/user-vehicles/confirm
{
  "sessionId": "temp-session-guid",
  "confirmed": true,
  "adjustments": [] // hoặc user có thể sửa
}
```

### Bước 7: Backend Tạo Xe + Tracking với AI Data

**Flow tạo data:**
```
1. Tạo UserVehicle
   ├─ CurrentOdometer = 8500 (từ AI)
   ├─ PurchaseDate = user input
   └─ AverageKmPerDay = calculated

2. Tạo OdometerHistory
   └─ Initial record: 8500km

3. Tạo VehiclePartTracking (từ AI analysis)
   ├─ Dầu máy:
   │  ├─ LastReplacementOdometer = 7200
   │  ├─ LastReplacementDate = 2024-12-23
   │  ├─ CurrentPartProductId = Castrol-guid
   │  └─ PredictedNext = 9200km / 2025-06-23
   │
   ├─ Má phanh:
   │  ├─ LastReplacementOdometer = 0
   │  ├─ LastReplacementDate = 2024-01-15
   │  └─ PredictedNext = 10000km / 2025-01-15
   │
   └─ Lốp xe:
      ├─ LastReplacementOdometer = 6000
      ├─ LastReplacementDate = 2024-06-15
      └─ PredictedNext = 21000km / 2026-06-15

4. Tạo MaintenanceReminder (từ AI urgency)
   ├─ Má phanh: WARNING (65% urgency)
   └─ Dầu máy: LOW (35% urgency)
```

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
      └─ Email (nếu urgent)
```

**Notification messages:**
```
Level HIGH (10-25% remaining):
"🟡 Honda Wave Alpha (59H1-12345)
Dầu máy còn 200km nữa cần thay.
Dự kiến: ~7 ngày nữa"

Level URGENT (<10% remaining):
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
