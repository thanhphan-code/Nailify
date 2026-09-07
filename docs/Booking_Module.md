# Tài liệu module Booking — Nailify

## 1. Mục đích

Module Booking cho phép khách hàng:

- Chọn một dịch vụ chính.
- Chọn mẫu nail tương thích với dịch vụ.
- Chọn nhân viên cụ thể hoặc để hệ thống tự phân công.
- Xem các khung giờ thực sự còn trống.
- Tạo lịch hẹn và giữ chỗ trong 10 phút.
- Thanh toán tiền cọc 30% qua payOS.
- Được hệ thống tự động xác nhận lịch sau khi payOS báo thanh toán thành công.
- Xem chi tiết, đổi lịch, hủy lịch và đánh giá từng dịch vụ đã hoàn thành.

Nguyên tắc nghiệp vụ quan trọng:

> Chưa thanh toán cọc: giữ slot tối đa 10 phút. Thanh toán thành công: tự động chuyển lịch sang `Confirmed`, không cần nhân viên xác nhận thủ công.

## 2. Phạm vi và thành phần

### Frontend

| Thành phần | Trách nhiệm |
|---|---|
| `ServicesPage.tsx` | Hiển thị và chọn dịch vụ chính |
| `BookingPage.tsx` | Chọn mẫu nail, nhân viên, ngày, giờ và tạo lịch |
| `DepositPage.tsx` | Hiển thị tiền cọc, mở payOS, đếm ngược và polling trạng thái |
| `AppointmentsPage.tsx` | Xem lịch, hủy, đổi lịch và đánh giá dịch vụ |
| `features/bookings/api.ts` | Các hàm gọi Booking API |
| `serviceSelectionStore.ts` | Lưu tạm dịch vụ được chọn khi chuyển trang |

### Backend

| Thành phần | Trách nhiệm |
|---|---|
| `BookingsController` | Giờ trống, tạo lịch, danh sách lịch, hủy và đổi lịch |
| `DepositsController` | Tạo link payOS, đọc và hủy phiên đặt cọc |
| `PayOsWebhooksController` | Xác minh webhook và tự xác nhận lịch |
| `StaffAppointmentsController` | Nhân viên quản lý vòng đời thực hiện dịch vụ |
| `DepositExpiryService` | Tự hủy lịch chưa đặt cọc quá hạn |
| `PayOsService` | Ký request, gọi API payOS và xác minh chữ ký webhook |
| `BookingRules` | Quy tắc dùng chung về thời gian, giá và trạng thái |

## 3. Luồng đặt lịch chuẩn

### Bước 1 — Chọn dịch vụ

Khách chọn đúng một dịch vụ chính tại `/services`.

Dịch vụ hợp lệ phải thỏa mãn:

- `IsActive = true`.
- `IsBookable = true`.
- Giá và thời lượng lấy từ database, không tin dữ liệu giá do frontend gửi lên.

Nếu khách chưa đăng nhập, hệ thống chuyển đến `/login` và giữ `returnUrl` để quay lại luồng đặt lịch.

### Bước 2 — Chọn mẫu nail tương thích

Frontend gọi:

```http
GET /api/services/{serviceId}/nail-designs
```

Backend chỉ trả về các mẫu có quan hệ trong `ServiceNailDesigns`, đang hoạt động và có thể đặt.

Tổng tiền và thời lượng dự kiến:

```text
Tổng tiền = Giá dịch vụ + Phụ phí mẫu nail
Tổng thời lượng = Thời lượng dịch vụ + Thời lượng thêm của mẫu nail
```

Giá được lưu và hiển thị bằng VND.

### Bước 3 — Chọn nhân viên

Frontend gọi:

```http
GET /api/services/{serviceId}/staff
```

Nhân viên hợp lệ phải:

- Có vai trò `Staff`.
- Có trạng thái `Active`.
- Được gán thực hiện dịch vụ thông qua `StaffServices`.

Khách có thể:

- Chọn một nhân viên cụ thể.
- Không chọn nhân viên; backend tự lấy nhân viên đủ điều kiện và còn trống đầu tiên.

### Bước 4 — Lấy khung giờ còn trống

```http
GET /api/bookings/available-slots
    ?serviceId={serviceId}
    &nailDesignId={nailDesignId}
    &staffId={staffId-tùy-chọn}
    &date={yyyy-MM-dd}
```

Quy tắc tạo slot:

- Khoảng cách giữa các giờ bắt đầu là 30 phút.
- Chỉ cho đặt từ hôm nay đến tối đa 30 ngày tiếp theo.
- Không cho chọn giờ đã qua trong ngày hiện tại.
- Toàn bộ thời lượng dịch vụ phải nằm trong ca làm việc.
- Loại bỏ thời gian nghỉ của nhân viên.
- Loại bỏ lịch trùng của nhân viên.
- Loại bỏ lịch trùng của chính khách hàng đang đăng nhập.
- Nếu chưa có lịch làm việc riêng, dùng giờ mở cửa cấu hình của salon.

Các trạng thái đang chiếm slot:

```text
AwaitingDeposit
Pending
Confirmed
InProgress
```

Các trạng thái giải phóng slot:

```text
Completed
Cancelled
```

Hai khoảng thời gian chỉ được xem là trùng khi:

```text
startA < endB và endA > startB
```

Vì vậy lịch kết thúc lúc `10:00` không xung đột với lịch bắt đầu đúng `10:00`.

### Bước 5 — Tạo lịch và giữ slot

```http
POST /api/appointments
Authorization: Bearer {customerAccessToken}
Content-Type: application/json

{
  "serviceId": "guid",
  "nailDesignId": "guid",
  "staffId": "guid hoặc null",
  "appointmentDate": "2026-09-10",
  "startTime": "10:30:00",
  "note": "Ghi chú tối đa 500 ký tự"
}
```

Backend thực hiện trong transaction mức `Serializable`:

1. Kiểm tra lại dịch vụ và mẫu nail.
2. Kiểm tra quan hệ tương thích.
3. Tính lại tổng thời lượng và tổng giá ở server.
4. Kiểm tra lịch trùng của khách.
5. Chọn lại nhân viên thực sự còn trống.
6. Tạo `Appointment` với trạng thái `AwaitingDeposit`.
7. Tạo `DepositPayment` bằng 30% tổng tiền.
8. Đặt `ExpiresAt = UtcNow + 10 phút`.
9. Tạo thông báo cho khách, nhân viên và quản trị viên.

Transaction và ràng buộc database giúp hạn chế hai khách đặt cùng nhân viên, cùng khung giờ trong tình huống race condition.

### Bước 6 — Tạo phiên thanh toán payOS

Sau khi tạo lịch, frontend chuyển đến:

```text
/deposit/{appointmentId}
```

Frontend gọi:

```http
GET /api/deposits/{appointmentId}
Authorization: Bearer {customerAccessToken}
```

Backend:

- Chỉ trả phiên đặt cọc thuộc chính khách đang đăng nhập.
- Sinh `PayOsOrderCode` duy nhất.
- Gửi request tạo payment link đến payOS.
- Lưu `PaymentLinkId` và `CheckoutUrl` để không tạo link lặp lại.
- Trả `503` nếu payOS tạm thời không khả dụng.

URL trả về được tạo theo lịch hẹn:

```text
Thành công: {FrontendBaseUrl}/deposit/{appointmentId}?payment=success
Hủy:       {FrontendBaseUrl}/deposit/{appointmentId}?payment=cancelled
```

Frontend polling trạng thái mỗi 2,5 giây trong khi phiên còn hiệu lực. Khách không cần tải biên lai và nhân viên không cần xác nhận tiền thủ công.

### Bước 7 — payOS gửi webhook

Endpoint:

```http
POST /api/payments/payos/webhook
```

Endpoint cho phép request không có JWT vì request đến từ máy chủ payOS. Bảo mật được thực hiện bằng chữ ký:

- Sắp xếp các trường trong `data` theo alphabet.
- Ghép thành chuỗi `key=value&key=value`.
- Ký bằng `HMAC-SHA256` với `ChecksumKey`.
- So sánh chữ ký theo cách hạn chế timing attack.
- Kiểm tra `orderCode`, số tiền và `paymentLinkId` với dữ liệu database.
- Webhook sai chữ ký hoặc sai số tiền trả `400`.
- Webhook lặp lại được xử lý idempotent và không xác nhận hai lần.

Khi webhook hợp lệ:

```text
DepositPayment.Status: AwaitingReceipt -> Approved
Appointment.Status:    AwaitingDeposit -> Confirmed
```

Sau đó hệ thống tạo thông báo `DepositApproved` cho khách hàng và gửi email xác nhận/biên nhận tiền cọc qua Mailjet nếu Mailjet đã được cấu hình.

### Bước 8 — Hết hạn hoặc hủy thanh toán

`DepositExpiryService` chạy mỗi 30 giây.

Nếu `ExpiresAt <= UtcNow` và lịch vẫn là `AwaitingDeposit`, worker thực hiện theo thứ tự an toàn:

1. Đọc trạng thái mới nhất từ API payOS để tránh hủy nhầm giao dịch đã thanh toán nhưng webhook đến chậm.
2. Nếu payOS trả `PAID` và đủ tiền cọc, chuyển lịch sang `Confirmed`.
3. Nếu vẫn chưa thanh toán, hủy payment link trên payOS.
4. Chỉ sau khi hủy link thành công mới giải phóng slot.

```text
DepositPayment.Status -> Expired
Appointment.Status    -> Cancelled
CancellationReason    -> Quá hạn thanh toán tiền cọc
```

Vì worker quét mỗi 30 giây, thời điểm giải phóng thực tế thường nằm trong khoảng từ 10 đến gần 10 phút 30 giây sau khi tạo lịch. Nếu payOS tạm thời không phản hồi, worker giữ slot và thử lại để tránh hủy nhầm một giao dịch đã thanh toán.

Nếu khách bấm hủy trên payOS, frontend gọi:

```http
POST /api/deposits/{appointmentId}/cancel
Authorization: Bearer {customerAccessToken}
```

Backend chỉ cho hủy khi lịch còn `AwaitingDeposit`. Trước khi hủy, backend đối soát trạng thái và hủy payment link trực tiếp trên payOS. Một lịch đã thanh toán hoặc đã `Confirmed` không thể bị hủy bằng endpoint thanh toán này.

## 4. Sơ đồ trạng thái

```text
Tạo lịch
   |
   v
AwaitingDeposit -- thanh toán hợp lệ --> Confirmed --> InProgress --> Completed
   |                                      |
   |                                      +--> Cancelled
   |
   +-- quá 10 phút --> Cancelled
   |
   +-- khách hủy payOS --> Cancelled

Confirmed -- khách đổi lịch --> Pending -- nhân viên xác nhận lại --> Confirmed
```

Ý nghĩa trạng thái:

| Trạng thái | Ý nghĩa |
|---|---|
| `AwaitingDeposit` | Đang giữ slot và chờ khách đặt cọc |
| `Pending` | Lịch đã đổi giờ và đang chờ nhân viên xác nhận lại |
| `Confirmed` | Đã đặt cọc và được xác nhận |
| `InProgress` | Nhân viên đang thực hiện dịch vụ |
| `Completed` | Dịch vụ đã hoàn thành |
| `Cancelled` | Lịch đã bị hủy hoặc hết hạn |

## 5. Quy tắc xác nhận của nhân viên

### Lịch mới

Nhân viên **không cần bấm xác nhận** sau khi khách đặt cọc. Webhook payOS tự chuyển lịch từ `AwaitingDeposit` sang `Confirmed`.

### Lịch được đổi ngày/giờ

Theo logic hiện tại, khách chỉ có thể đổi lịch đang `Pending` hoặc `Confirmed`. Sau khi đổi thành công, trạng thái về `Pending`. Nhân viên được phân công phải xác nhận lại:

```http
PATCH /api/staff/appointments/{id}/confirm
```

Lý do: nhân viên cần chấp nhận lịch mới sau khi khách đổi thời gian. Không yêu cầu khách đặt cọc lại trong luồng đổi lịch hiện tại.

## 6. Hủy và đổi lịch

### Khách hủy lịch

```http
PATCH /api/appointments/{id}/cancel
Authorization: Bearer {customerAccessToken}

{
  "reason": "Lý do tối đa 500 ký tự"
}
```

Cho phép khi:

- Lịch chưa bắt đầu.
- Trạng thái là `AwaitingDeposit`, `Pending` hoặc `Confirmed`.

### Khách đổi lịch

```http
PATCH /api/appointments/{id}/reschedule
Authorization: Bearer {customerAccessToken}

{
  "staffId": "guid hoặc null",
  "appointmentDate": "2026-09-12",
  "startTime": "14:00:00"
}
```

Backend kiểm tra lại toàn bộ tương thích, nhân viên, lịch trùng và thời gian trước khi cập nhật.

## 7. Luồng của nhân viên

Nhân viên hoặc quản trị viên xem lịch tại:

```http
GET /api/staff/appointments
```

Các chuyển trạng thái hợp lệ:

```text
Pending   -> Confirmed
Confirmed -> InProgress
InProgress -> Completed
Pending hoặc Confirmed -> Cancelled
```

Nhân viên không xác nhận lịch đang `AwaitingDeposit`; trạng thái này chỉ được chuyển bởi webhook payOS hoặc worker hết hạn.

## 8. Thông báo hệ thống

| Loại | Người nhận | Thời điểm |
|---|---|---|
| `DepositRequired` | Khách hàng | Vừa tạo lịch, cần đặt cọc trong 10 phút |
| `BookingAwaitingDeposit` | Nhân viên | Có slot đang được giữ chờ cọc |
| `BookingNeedsConfirmation` | Quản trị viên | Có lịch mới được tạo |
| `DepositApproved` | Khách hàng | payOS xác nhận thanh toán thành công |
| `DepositExpired` | Khách hàng | Quá hạn 10 phút, lịch tự hủy |
| `BookingRescheduled` | Nhân viên | Khách đổi ngày/giờ |
| `BookingCancelled` | Bên liên quan | Lịch bị hủy |
| `BookingInProgress` | Khách hàng | Dịch vụ bắt đầu |
| `BookingCompleted` | Khách hàng | Dịch vụ hoàn thành |

## 9. Đánh giá sau dịch vụ

Khách chỉ được đánh giá khi lịch đã `Completed`. Mỗi dịch vụ trong một lịch chỉ được đánh giá một lần.

```http
POST /api/appointments/{appointmentId}/services/{serviceId}/review
Authorization: Bearer {customerAccessToken}

{
  "rating": 5,
  "comment": "Dịch vụ rất tốt"
}
```

Quy tắc:

- `rating` từ 1 đến 5.
- Bình luận tối đa 1.000 ký tự.
- Đánh giá chỉ xuất hiện trên Trang chủ sau khi quản trị viên duyệt.

## 10. Mã lỗi chính

| HTTP | Code | Ý nghĩa |
|---|---|---|
| `400` | `INVALID_BOOKING_DATE` | Ngày đặt không hợp lệ |
| `400` | `INVALID_BOOKING_REQUEST` | Request hoặc ghi chú không hợp lệ |
| `400` | `SERVICE_OR_DESIGN_UNAVAILABLE` | Dịch vụ hoặc mẫu không còn khả dụng |
| `400` | `INCOMPATIBLE_SERVICE_DESIGN` | Mẫu không tương thích với dịch vụ |
| `409` | `CUSTOMER_SLOT_CONFLICT` | Khách đã có lịch trùng thời gian |
| `409` | `SLOT_UNAVAILABLE` | Nhân viên/slot vừa được người khác đặt |
| `409` | `APPOINTMENT_CANNOT_BE_CANCELLED` | Lịch không thể hủy ở trạng thái hiện tại |
| `409` | `APPOINTMENT_CANNOT_BE_RESCHEDULED` | Lịch không thể đổi ở trạng thái hiện tại |
| `409` | `PAYMENT_ALREADY_APPROVED` | Tiền cọc đã được xác nhận |
| `503` | — | Không thể kết nối payOS |

## 11. Cấu hình

Các khóa cấu hình bắt buộc:

```json
{
  "Frontend": {
    "BaseUrl": "https://ten-mien-frontend"
  },
  "Salon": {
    "Timezone": "Asia/Ho_Chi_Minh",
    "BookingHours": {
      "Start": "09:00",
      "End": "18:00"
    }
  },
  "Payment": {
    "DepositPercent": 0.30
  },
  "PayOS": {
    "ClientId": "",
    "ApiKey": "",
    "ChecksumKey": ""
  }
}
```

Không commit khóa thật vào Git. Khi phát triển cục bộ, dùng .NET User Secrets:

```powershell
# Chạy trong thư mục backend bằng PowerShell
dotnet user-secrets --project src/Nailify.Api set "PayOS:ClientId" "GIA_TRI_CUA_BAN"
dotnet user-secrets --project src/Nailify.Api set "PayOS:ApiKey" "GIA_TRI_CUA_BAN"
dotnet user-secrets --project src/Nailify.Api set "PayOS:ChecksumKey" "GIA_TRI_CUA_BAN"
```

Webhook production phải là URL HTTPS công khai:

```text
https://api.ten-mien-cua-ban/api/payments/payos/webhook
```

`localhost` không nhận được webhook từ máy chủ payOS. Khi phát triển có thể dùng Cloudflare Tunnel hoặc ngrok.

## 12. Kiểm thử

### Kiểm thử tự động hiện có

Chạy tại thư mục `backend`:

```powershell
dotnet test Nailify.sln
```

Các trường hợp đã có test:

- Trạng thái nào chiếm/giải phóng slot.
- Quy tắc giao nhau của khoảng thời gian.
- Tổng thời lượng gồm thời gian thêm của mẫu.
- Tổng giá gồm phụ phí mẫu.
- Quy tắc khách hủy lịch.
- Tính tiền cọc 30% và làm tròn VND.
- Từ chối tỷ lệ đặt cọc hoặc tổng tiền không hợp lệ.
- Request tạo link payOS có header, URL và chữ ký đúng.
- API lấy trạng thái link payOS đọc đúng số tiền đã thanh toán.
- API hủy link payOS gửi đúng mã đơn và lý do hủy.
- Webhook đúng chữ ký được chấp nhận.
- Webhook bị sửa số tiền bị từ chối.

### Checklist kiểm thử tích hợp

1. Đăng nhập bằng tài khoản khách hàng.
2. Chọn dịch vụ, mẫu nail và nhân viên.
3. Kiểm tra slot trùng không xuất hiện.
4. Tạo lịch và xác nhận trạng thái ban đầu là `AwaitingDeposit`.
5. Kiểm tra tiền cọc bằng 30% tổng dự kiến.
6. Mở checkout URL payOS.
7. Thanh toán một giao dịch test/thật có kiểm soát.
8. Xác nhận webhook trả HTTP 2xx.
9. Xác nhận `DepositPayment = Approved`.
10. Xác nhận `Appointment = Confirmed` mà không cần nhân viên bấm.
11. Kiểm tra thông báo thanh toán thành công.
12. Tạo lịch khác nhưng không thanh toán; chờ tối đa gần 10 phút 30 giây.
13. Xác nhận lịch tự chuyển `Cancelled` và slot xuất hiện lại.
14. Bấm hủy tại payOS; xác nhận slot được giải phóng.
15. Gửi webhook sai chữ ký; xác nhận HTTP `400` và database không thay đổi.
16. Gửi lại cùng webhook hợp lệ; xác nhận không tạo cập nhật trùng.

### Build frontend

Chạy tại thư mục `frontend`:

```powershell
npm run build
```

## 13. Lưu ý trước khi production

- Các secret local đã được chuyển khỏi `appsettings.json` sang .NET User Secrets; production vẫn phải dùng biến môi trường hoặc secret manager.
- Đổi lại các secret từng xuất hiện trong repository.
- Cấu hình webhook HTTPS công khai trong kênh thanh toán payOS.
- Chỉ tin trạng thái từ webhook đã xác minh chữ ký, không tin query string trả về frontend.
- Bật logging có correlation ID nhưng không ghi token, API key hoặc toàn bộ payload nhạy cảm.
- Giám sát lỗi webhook, lỗi tạo link và số lượng lịch hết hạn.
- Kiểm tra timezone production đúng `Asia/Ho_Chi_Minh`.
- Cân nhắc chính sách hoàn cọc khi khách hủy lịch `Confirmed`; phiên bản hiện tại chưa tự động hoàn tiền payOS.
- Cân nhắc xác nhận tự động sau khi đổi lịch nếu nghiệp vụ không muốn nhân viên xác nhận lại.

## 14. Trạng thái kiểm thử hiện tại

Tại thời điểm cập nhật tài liệu:

- Backend build thành công.
- Frontend build production thành công.
- 28/28 unit test thành công.
- API public hoạt động.
- Endpoint đặt cọc yêu cầu xác thực.
- Webhook cho phép payOS gọi không cần JWT nhưng từ chối chữ ký sai.
- Có cơ chế đối soát dự phòng khi webhook đến chậm hoặc bị lỡ.
- Khi hết hạn/hủy, backend hủy payment link payOS trước khi giải phóng slot.
- Giao diện khách và nhân viên dùng chung nhãn trạng thái tiếng Việt; nhân viên không duyệt cọc thủ công.
- Mailjet gửi email xác nhận tiền cọc sau khi lịch chuyển sang `Confirmed`.
- Chưa thể xác nhận end-to-end giao dịch ngân hàng thật nếu chưa có backend HTTPS công khai và webhook đã đăng ký trên payOS.
