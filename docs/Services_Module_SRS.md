# Module Services — SRS bổ sung

## Mục đích

Module Services cho phép mọi người xem và lọc dịch vụ nail. Customer có thể chọn nhiều dịch vụ để đưa vào flow đặt lịch; Admin quản trị danh mục và xem báo cáo top service.

## Dữ liệu và quy tắc

`Service` gồm Id, Name, Category, DurationMinutes, Price, Description, ImageUrl, IsActive, CreatedAt và UpdatedAt. Name là duy nhất trong cùng Category; Price không âm và DurationMinutes lớn hơn 0. Service đã có booking chỉ được ngừng hoạt động (`IsActive = false`).

`AppointmentService` là bảng nối Appointment–Service, khóa chính `(AppointmentId, ServiceId)`, lưu `PriceAtBooking` và `DurationAtBooking`. Appointment có thể không có NailDesign. Tổng tiền/thời lượng phải được tính từ snapshot, không dùng giá hiện hành của Service.

## Phân quyền

Guest/Customer/Staff/Admin đều xem được danh sách và chi tiết service. Chỉ Customer đã xác thực được chọn service. Chỉ Admin tạo, sửa, bật/tắt service và xem báo cáo. Guest không được hiển thị giỏ service hoặc dữ liệu appointment cá nhân.

## API

- `GET /api/services`: public; filter `category`, `search`, min/max price, min/max duration và `page`, `pageSize`.
- `GET /api/services/{id}`: public.
- `POST /api/services`, `PUT /api/services/{id}`, `PATCH /api/services/{id}/toggle-active`: Admin.
- `GET /api/services/reports/top?from=&to=`: Admin; trả top 10 theo doanh thu snapshot.

## Tiêu chí chấp nhận

- Guest thấy Services và filter nhưng CTA booking disabled, nhãn “Sign in to book”.
- Customer có thể thêm/bớt nhiều service, thấy tổng tiền/thời lượng realtime và CTA tiếp tục đặt lịch.
- Sửa giá/thời lượng service không làm thay đổi dữ liệu snapshot trong AppointmentService.
- Migration chuyển mỗi `Appointments.ServiceId` cũ thành AppointmentService với giá/thời lượng tại lúc migration.
