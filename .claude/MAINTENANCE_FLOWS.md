# Các Luồng Chính – Bảo Trì Xe & Reminder

Tài liệu mô tả các luồng chính: theo dõi phụ tùng, cập nhật số km và reminder. Tham khảo `MAINTENANCE_FLOW_EXAMPLE.md` cho ví dụ chi tiết (AI, API request/response).

---

## 2. Odo: Ghi Liên Tục, Tính Chỉ 3 Tháng Gần Nhất

**Nguyên tắc:**

- **Ghi:** Mỗi lần user cập nhật odo (PATCH odometer), hệ thống ghi **OdometerHistory** và cập nhật **UserVehicle.CurrentOdometer** — tức tracking odo **liên tục**, không giới hạn thời gian.
- **Tính:** Khi cần tính **km/ngày**, **km/tháng** (để gợi ý PredictedNext\*, personalization, hoặc Level/PercentageRemaining của Reminder), **chỉ lấy OdometerHistory trong 3 tháng gần nhất** (RecordedDate >= today - 3 tháng).
- **Lý do:** Tránh tình trạng người dùng thay đổi đột ngột (vd trước chạy nhiều, sau ít chạy hoặc ngược lại) làm số liệu cũ kéo lệch dự đoán.

**Chưa có nhập odo (hoặc không có odo trong 3 tháng):**

- **Số km hiện tại:** Dùng **UserVehicle.CurrentOdometer** (số km khi thêm xe hoặc lần cập nhật gần nhất).
- **Km/ngày để ước tính "còn X ngày":** Ưu tiên **OdometerHistory 3 tháng gần nhất**; nếu không có (ít hơn 2 lần ghi) thì fallback **UserVehicle.AverageKmPerDay** (km/ngày từ ngày mua xe đến hiện tại). Nếu cả hai đều null → chỉ dùng % theo km và **PredictedNextDate** (nếu user đã set); không ước tính "còn X ngày" từ km.
- **Reminder:** Vẫn tạo theo PredictedNext\* và CurrentOdometer; nhắc theo ngày nếu có PredictedNextDate hoặc có km/ngày (ước tính TargetDate = today + km còn lại / km/ngày).

---

## 3. Luồng Chính

### 3.1 Apply Tracking → Bắt đầu track phụ tùng

```
[User] → Nhập LastReplacement*, PredictedNext* (vd từ AI hoặc tay)
   ↓
[Client] → POST apply-tracking (partCategoryCode, lastReplacementOdometer/Date, predictedNextOdometer/Date, instanceIdentifier?, currentPartProductId?)
   ↓
[System] → Tìm/tạo VehiclePartTracking (UserVehicle + PartCategory, instanceIdentifier nếu part AllowsMultipleInstances) → cập nhật Last*, PredictedNext*, IsDeclared = true, CurrentPartProductId (user chọn product từ hệ thống)
   ↓
Phụ tùng bắt đầu được track; PredictedNext* dùng cho reminder sau này.
```

### 3.2 Cập nhật odo

```
[User] → PATCH odometer { currentOdometer }
   ↓
[System] → Insert OdometerHistory, Update UserVehicle.CurrentOdometer, LastOdometerUpdate
   ↓
[System] → (Đồng bộ) So sánh CurrentOdometer với PredictedNext* → tạo/cập nhật MaintenanceReminder theo level (nhiều bản ghi per part; mặc định lấy level cao nhất). Ngưỡng Level: Critical <5%, High 5–15%, Medium 15–25%, Low 25–40%.
```

### 3.3 Odo → Reminder (cần bổ sung)

**Chạy đồng bộ** trong API PATCH odometer (sau SaveChanges): lấy **UserVehicle.CurrentOdometer**; lấy VehiclePartTracking của xe (IsDeclared = true, có PredictedNext\*); so sánh current vs target → tính PercentageRemaining, Level → **tạo/cập nhật nhiều MaintenanceReminder** (mỗi level một bản ghi). Mặc định hiển thị/notification lấy **reminder có level cao nhất**. GetPendingReminders + notification.

**Ngưỡng ReminderLevel** (% còn lại): **Normal** >40%, **Low** 25–40%, **Medium** 15–25%, **High** 5–15%, **Critical** <5%. Flow: Normal → Low → Medium → High → Critical → [Thay phụ tùng] → Normal.

Khi tính km/ngày hoặc km/tháng cho Level/%: chỉ dùng **OdometerHistory 3 tháng gần nhất** (mục 2).

### 3.4 Chu kỳ đầy đủ 1 vòng

```
Thêm xe (POST user-vehicles, số km ban đầu)
   → UserVehicle + OdometerHistory + VehiclePartTracking (mọi part, IsDeclared = false)
   → Apply tracking (thay nhớt 1: Last*, PredictedNext*, IsDeclared = true)
   → User cập nhật odo nhiều lần (OdometerHistory ghi liên tục)
   → Odo → MaintenanceReminder (đồng bộ trong PATCH odometer; 3 tháng gần nhất khi cần tính km/tháng)
   → GetPendingReminders + notification → User thay nhớt 2
   → Apply tracking lại (Last*, PredictedNext* mới) → lặp lại.
```

| Bước                                   | Trạng thái                                |
| -------------------------------------- | ----------------------------------------- |
| Thêm xe, Apply tracking, Cập nhật odo  | ✅ Đã có                                  |
| Odo → tạo/cập nhật MaintenanceReminder | ✅ Đã có (đồng bộ trong PATCH odometer)   |
| GetPendingReminders, notification      | ✅ API/repo có; notification tùy hệ thống |

---

## 4. Tracking cần field nào cho Reminder

| Field                                                | Vai trò                                                                                                                   |
| ---------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| **PredictedNextOdometer**, **PredictedNextDate**     | Mốc nhắc thay → TargetOdometer, TargetDate. Bắt buộc (theo km hoặc theo ngày).                                            |
| **LastReplacementOdometer**, **LastReplacementDate** | Hiển thị và tính % còn lại.                                                                                               |
| **CustomKmInterval**, **CustomMonthsInterval**       | Không bắt buộc cho "khi nào nhắc" nếu đã có PredictedNext*; dùng khi backend tự tính PredictedNext* cho chu kỳ tiếp theo. |
| **InstanceIdentifier**                               | Phân biệt instance khi part AllowsMultipleInstances (vd "Lốp trước"/"Lốp sau"). Ưu tiên có; không có thì không bắt buộc.  |
| **CurrentPartProductId**                             | Sản phẩm đang dùng (user chọn từ danh sách PartProduct của hệ thống). Có trong Apply tracking.                            |

Số km hiện tại: **UserVehicle.CurrentOdometer** (hoặc OdometerHistory mới nhất), không lưu trên VehiclePartTracking.

---

## 5. Tóm Tắt Key Flows

1. **Apply tracking:** User nhập Last*/PredictedNext* → VehiclePartTracking tạo/cập nhật, IsDeclared = true → phụ tùng được track.
2. **Cập nhật odo:** PATCH odometer → OdometerHistory (ghi liên tục) + UserVehicle.CurrentOdometer. Chưa có: so sánh với PredictedNext\* → Reminder.
3. **Odo → Reminder:** CurrentOdometer vs PredictedNext\* → MaintenanceReminder; khi tính km/tháng/%, **chỉ dùng odo 3 tháng gần nhất** để tránh thay đổi đột ngột.
4. **Chu kỳ:** Thêm xe → Apply tracking → Cập nhật odo (liên tục) → Reminder (3 tháng gần nhất khi tính) → Nhắc → Thay → Apply tracking lại → lặp.
5. **Chưa có odo:** Dùng UserVehicle.CurrentOdometer; không personalization cho đến khi có OdometerHistory trong 3 tháng gần nhất.

---

## 6. Đã làm rõ (theo MAINTENANCE_FLOWS_QUESTIONS.md)

| Chủ đề                                  | Quyết định                                                                                      |
| --------------------------------------- | ----------------------------------------------------------------------------------------------- |
| Đánh số mục (thiếu ## 1)                | Không quan trọng, có thể bỏ.                                                                    |
| Odo → Reminder chạy ở đâu?              | **Đồng bộ** trong API PATCH odometer (sau khi ghi OdometerHistory + cập nhật UserVehicle).      |
| Reminder: một hay nhiều bản ghi?        | **Nhiều** reminder theo level (mỗi level một bản); mặc định lấy reminder có **level cao nhất**. |
| Ngưỡng ReminderLevel                    | **Normal** >40%, **Low** 25–40%, **Medium** 15–25%, **High** 5–15%, **Critical** <5%.           |
| InstanceIdentifier khi Apply tracking   | Ưu tiên làm; nếu không có thì không cần (optional).                                             |
| CurrentPartProductId khi Apply tracking | **Có**: user chọn product từ danh sách PartProduct của hệ thống khi record phụ tùng.            |

---

## Tham Chiếu

- **MAINTENANCE_FLOW_EXAMPLE.md**: Ví dụ chi tiết đăng ký xe với AI, apply tracking, odo, bảo dưỡng, notification, dashboard.
- **MAINTENANCE_FLOWS_QUESTIONS.md**: Câu hỏi đã trả lời (FAQ).
