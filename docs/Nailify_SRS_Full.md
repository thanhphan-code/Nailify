# NAILIFY
## Nail Salon Booking & Management Platform — Software Requirements Specification

**Version:** 2.0 (Full SRS, mở rộng từ Project Overview v1.0)
**Document Type:** Software Requirements Specification
**Project Status:** Planning / MVP
**Ngày cập nhật:** 18/08/2026

> **Ghi chú về phiên bản:** Tài liệu này mở rộng bản Project Overview v1.0, bổ sung các phần còn thiếu (Non-Functional Requirements, Actor "Guest", data dictionary chi tiết, API overview, assumptions) và **giải quyết các mâu thuẫn** đã phát hiện ở bản gốc (đặc biệt là vị trí của tính năng Favorite). Các điểm được sửa/làm rõ so với bản v1.0 sẽ được đánh dấu bằng ghi chú **[RESOLVED]**.

---

# 1. Project Overview

## 1.1. Project Name

**Nailify – Nail Salon Booking & Management Platform**

## 1.2. Project Description

Nailify là một nền tảng web hỗ trợ khách hàng tìm kiếm mẫu nail, xem dịch vụ, lựa chọn nhân viên và đặt lịch làm nail trực tuyến.

Đồng thời, hệ thống hỗ trợ nhân viên quản lý lịch hẹn và hỗ trợ chủ salon quản lý dịch vụ, mẫu nail, nhân viên, khách hàng, lịch hẹn và đánh giá.

Mục tiêu của dự án là thay thế quy trình đặt lịch thủ công thông qua điện thoại, Facebook hoặc tin nhắn bằng một hệ thống tập trung, dễ sử dụng và có khả năng mở rộng.

## 1.3. Định hướng sản phẩm

Nailify không nên được xây dựng đơn thuần như một website giới thiệu nail salon.

> **Nail Salon Booking & Management Platform** — trong đó **Appointment/Booking là nghiệp vụ trung tâm**, còn Nail Design, Service, Staff, Customer, Review và Dashboard xoay quanh appointment.

Nguyên tắc phát triển: **hoàn thành toàn bộ booking flow trước, sau đó mới mở rộng các chức năng nâng cao.**

---

# 2. Problem Statement

Các nail salon nhỏ và vừa thường gặp một số vấn đề:

- Khách hàng phải nhắn tin hoặc gọi điện để hỏi lịch trống.
- Thông tin dịch vụ và giá cả không được quản lý tập trung.
- Hình ảnh mẫu nail nằm rải rác trên mạng xã hội.
- Khó kiểm soát lịch làm việc của nhân viên.
- Có nguy cơ đặt trùng lịch.
- Khách hàng khó theo dõi lịch sử các lần sử dụng dịch vụ.
- Salon khó quản lý đánh giá và dữ liệu khách hàng.
- Việc thống kê số lượng booking và doanh thu còn thủ công.

Nailify giải quyết các vấn đề trên bằng một nền tảng quản lý và đặt lịch tập trung.

---

# 3. Project Objectives

## 3.1. Main Objectives (MVP)

1. Khách hàng xem và tìm kiếm mẫu nail.
2. Khách hàng xem danh sách dịch vụ và giá.
3. Khách hàng đặt lịch trực tuyến.
4. Hệ thống kiểm tra và ngăn ngừa booking bị trùng.
5. Nhân viên quản lý các appointment được giao.
6. Admin quản lý toàn bộ dữ liệu salon.
7. Khách hàng đánh giá sau khi hoàn thành dịch vụ.
8. Admin theo dõi các số liệu cơ bản của salon.

## 3.2. Long-term Objectives (Phase 2 / Phase 3)

- Thanh toán online và đặt cọc.
- Notification.
- Favorite / Yêu thích mẫu nail. *(chuyển từ MVP sang Phase 2 — xem mục 6.3)*
- Loyalty / Membership.
- Coupon / Promotion.
- Before & After Gallery.
- AI Nail Recommendation.
- Customer analytics.
- Personalized recommendations.

---

# 4. Target Users & Actors

Hệ thống có **4 nhóm actor**, bổ sung nhóm Guest so với bản gốc.

## 4.1. Guest (chưa đăng nhập) — **[RESOLVED: bổ sung mới]**

Guest là actor được phép truy cập một phần hệ thống mà không cần tài khoản:

- Xem gallery mẫu nail (không xem được số điện thoại/thông tin liên hệ khách khác).
- Xem danh sách dịch vụ và giá.
- Search / filter mẫu nail.
- **Không được** đặt lịch, đánh giá, hoặc favorite — các thao tác này bắt buộc đăng nhập (theo BR-001).
- Khi Guest bấm "Đặt lịch" hoặc "Yêu thích", hệ thống điều hướng sang màn hình Login/Register.

## 4.2. Customer

Khách hàng đã đăng nhập sử dụng hệ thống để:

- Xem mẫu nail, xem dịch vụ.
- Tìm kiếm và lọc mẫu.
- Đặt lịch.
- Theo dõi appointment.
- Hủy lịch (theo điều kiện BR-007).
- Đánh giá dịch vụ đã hoàn thành.
- *(Phase 2)* Yêu thích mẫu nail.

## 4.3. Staff

Nhân viên nail sử dụng hệ thống để:

- Xem lịch làm việc (ca làm) của bản thân.
- Xem appointment được phân công.
- Xác nhận appointment.
- Cập nhật trạng thái dịch vụ (Confirmed → In Progress → Completed).
- Xem thông tin khách hàng liên quan đến appointment của mình.
- Upload kết quả sau khi hoàn thành dịch vụ.

## 4.4. Admin

Admin/salon owner sử dụng hệ thống để:

- Quản lý người dùng (Customer, Staff).
- Quản lý dịch vụ, danh mục, mẫu nail.
- Quản lý appointment (toàn quyền, kể cả can thiệp khi cần).
- Quản lý review.
- Quản lý ca làm việc của staff (StaffSchedule).
- Theo dõi dashboard & báo cáo.

## 4.5. Ma trận quyền (Permission Matrix)

| Chức năng | Guest | Customer | Staff | Admin |
|---|:---:|:---:|:---:|:---:|
| Xem gallery / service | ✅ | ✅ | ✅ | ✅ |
| Đăng ký / Đăng nhập | ✅ | – | – | – |
| Đặt lịch (tạo appointment) | ❌ | ✅ | ❌ | ✅ (hỗ trợ nhập hộ) |
| Hủy appointment hợp lệ | ❌ | ✅ (của mình) | ✅ (được giao, có lý do) | ✅ (mọi appointment, có lý do) |
| Xác nhận / cập nhật trạng thái appointment | ❌ | ❌ | ✅ (được giao) | ✅ (toàn bộ) |
| Đánh giá (review) | ❌ | ✅ | ❌ | ❌ |
| CRUD Service / Category / NailDesign | ❌ | ❌ | ❌ | ✅ |
| CRUD Staff / StaffSchedule | ❌ | ❌ | ❌ | ✅ |
| Xem Dashboard | ❌ | ❌ | ❌ (chỉ lịch cá nhân) | ✅ |

---

# 5. System Scope

## 5.1. In Scope – MVP

### Authentication
- Register
- Login
- Logout
- JWT Authentication
- Role-based Authorization

### Nail Design
- View nail designs (Guest + Customer)
- Search
- Filter theo category
- View design detail
- Admin CRUD designs

### Service
- View services (Guest + Customer)
- View price
- View estimated duration
- Admin CRUD services

### Booking
- Select service
- Select nail design *(ràng buộc theo mục 9, BR-009)*
- Select staff (**optional** — xem BR-010)
- Select date
- Select available time (dựa trên StaffSchedule + Appointment hiện có)
- Create appointment
- View appointment
- Cancel appointment
- Staff confirms appointment
- Staff completes appointment

### Review
- Customer rating (1–5 sao)
- Customer comment
- View reviews
- Admin management (ẩn/xóa review vi phạm)

### Management
- Customer management
- Staff management
- **Staff schedule management** — **[RESOLVED: bổ sung mới]**
- Service management
- Design management
- Appointment management

### Dashboard
- Total customers
- Total appointments
- Completed appointments
- Basic revenue statistics

## 5.2. Out of Scope – MVP

- Online payment
- Deposit
- Real-time chat
- AI image analysis
- AI nail recommendation
- Loyalty points
- Membership
- Coupon
- Advanced analytics
- Mobile application

## 5.3. Favorite Feature — **[RESOLVED: quyết định cuối cùng]**

Bản v1.0 có mâu thuẫn: mục 4.1/5.1 liệt kê Favorite là chức năng Customer dùng ở MVP, nhưng mục 14 và 24 lại xếp Favorite vào "để sau". Tài liệu này **chốt Favorite thuộc Phase 2**, không thuộc MVP, vì lý do:

1. Favorite không nằm trên core booking flow (mục 1.3 — Appointment là trung tâm).
2. Bảng `Favorite` chỉ là quan hệ N:N đơn giản giữa User–NailDesign, có thể bổ sung nhanh ở Phase 2 mà không ảnh hưởng kiến trúc.
3. Giữ MVP gọn giúp hoàn thành đúng nguyên tắc "booking flow trước, mở rộng sau".

→ Tất cả các mục Target Users/Screens có nhắc "Favorite" trong tài liệu này đều được đánh dấu **(Phase 2)**.

Những chức năng ngoài MVP này có thể được phát triển trong Phase 2 hoặc Phase 3 (xem mục 19).

---

# 6. Core Business Flow

## 6.1. Customer Booking Flow

```text
(Guest) Browse Nail Designs / Services
  ↓
Login / Register (bắt buộc trước khi đặt lịch — BR-001)
  ↓
Choose Nail Design
  ↓
Choose Service
  ↓
Choose Staff (optional — BR-010)
  ↓
Choose Date
  ↓
Choose Available Time (hệ thống tính từ StaffSchedule + Appointment hiện có)
  ↓
Review Booking (hiển thị TotalPrice theo BR-008)
  ↓
Create Appointment
  ↓
Pending
  ↓
Staff Confirms
  ↓
Confirmed
  ↓
Service Starts
  ↓
In Progress
  ↓
Service Completed
  ↓
Completed
  ↓
Customer Reviews (chỉ khi Completed — BR-005)
```

## 6.2. Core User Journey (tổng quan)

```text
                 NAILIFY
                    │
                    ▼
              Guest Browse
                    │
                    ▼
              Customer Login
                    │
                    ▼
             Browse Nail Design
                    │
                    ▼
             Select Design
                    │
                    ▼
             Select Service
                    │
                    ▼
          Select Staff (optional)
                    │
                    ▼
          Select Date & Time
                    │
                    ▼
            Check Availability
                    │
              ┌─────┴─────┐
              │           │
           Available    Unavailable
              │           │
              ▼           ▼
          Booking       Choose
           Created      another slot
              │
              ▼
          Staff Confirm
              │
              ▼
         Service Started
              │
              ▼
          Service Done
              │
              ▼
          Customer Review
```

---

# 7. Appointment Status

```text
PENDING
    ↓
CONFIRMED
    ↓
IN_PROGRESS
    ↓
COMPLETED
```

Có thể hủy tại một số trạng thái:

```text
PENDING   ──────→ CANCELLED
CONFIRMED ──────→ CANCELLED
```

Không cho phép:

```text
COMPLETED → CANCELLED
CANCELLED → COMPLETED
IN_PROGRESS → CANCELLED   (khách không được hủy khi đang làm — xem BR-007)
```

**State transition table (bổ sung để rõ ràng khi code):**

| From \ To | PENDING | CONFIRMED | IN_PROGRESS | COMPLETED | CANCELLED |
|---|:---:|:---:|:---:|:---:|:---:|
| PENDING | – | ✅ (Staff) | ❌ | ❌ | ✅ (Customer/Staff được giao/Admin) |
| CONFIRMED | ❌ | – | ✅ (Staff) | ❌ | ✅ (Customer/Staff được giao/Admin) |
| IN_PROGRESS | ❌ | ❌ | – | ✅ (Staff) | ❌ |
| COMPLETED | ❌ | ❌ | ❌ | – | ❌ |
| CANCELLED | ❌ | ❌ | ❌ | ❌ | – |

---

# 8. Business Rules

## BR-001 – Authentication
User phải đăng nhập trước khi tạo appointment, review, hoặc favorite (Phase 2). Guest chỉ xem được nội dung public.

## BR-002 – Role Authorization
- Customer không được truy cập các chức năng quản trị.
- Staff chỉ được quản lý appointment được phân công hoặc thuộc phạm vi được cho phép.
- Admin có toàn quyền quản lý hệ thống.

## BR-003 – Appointment Conflict
Một staff không được có hai appointment trùng thời gian.

```text
10:00 - 11:00  (đã đặt)
10:30 - 11:30  (KHÔNG được tạo cho cùng staff)
```

Kiểm tra conflict áp dụng cho tất cả appointment có trạng thái `PENDING`, `CONFIRMED`, `IN_PROGRESS` (chưa `CANCELLED`).

## BR-004 – Service Duration
Thời gian kết thúc appointment được tính dựa trên duration của service.

```text
Start: 10:00
Duration: 90 minutes
End: 11:30
```

> **Ghi chú giả định [ASSUMPTION]:** MVP tính EndTime chỉ dựa trên `Service.Duration`, **không cộng thêm thời gian phát sinh từ độ phức tạp của NailDesign** (ví dụ Chrome/3D thường tốn thêm thời gian thực tế). Đây là đơn giản hóa có chủ đích cho MVP; nếu cần chính xác hơn, Phase 2 có thể bổ sung field `NailDesign.ExtraDuration`.

## BR-005 – Review
Customer chỉ được review appointment khi:

```text
Appointment.Status = COMPLETED
```

Mỗi appointment chỉ được review **1 lần** (quan hệ 1:0..1 giữa Appointment–Review).

## BR-006 – Appointment Ownership
- Customer chỉ được xem appointment của chính mình.
- Staff chỉ được cập nhật appointment thuộc phạm vi được phân công.

## BR-007 – Cancellation
- Customer không được hủy appointment đã `COMPLETED` hoặc đang `IN_PROGRESS`.
- Chỉ được hủy khi trạng thái là `PENDING` hoặc `CONFIRMED`.
- Staff được hủy appointment được giao cho mình tại hai trạng thái trên, nhưng bắt buộc ghi `CancellationReason` không rỗng.
- Admin có thể hủy mọi appointment tại hai trạng thái trên và phải ghi lý do khi can thiệp.

## BR-008 – Price
Total price được tính dựa trên:

```text
TotalPrice = ServicePrice + NailDesignAdditionalPrice
```

Ví dụ:

```text
Gel Manicure       250,000đ
Pink Chrome        +100,000đ
-----------------------------
Total               350,000đ
```

## BR-009 – Ràng buộc Service ↔ NailDesign — **[RESOLVED: bổ sung mới]**
Không phải mọi NailDesign đều áp dụng được cho mọi Service (ví dụ: design "Chrome 3D" không áp dụng được cho service "Basic Pedicure Trim"). Mỗi `NailDesign` có field `ApplicableServiceIds` hoặc bảng liên kết N:N `ServiceNailDesign` để giới hạn tổ hợp hợp lệ. Khi Customer chọn Service trước, hệ thống chỉ hiển thị các NailDesign tương thích.

> Với MVP đơn giản, có thể tạm thời cho phép mọi NailDesign áp dụng cho mọi Service (bỏ ràng buộc), nhưng cần ghi rõ đây là giả định tạm thời, không phải hành vi cuối cùng.

## BR-010 – Chọn Staff là Optional — **[RESOLVED: bổ sung mới]**
Customer có thể để trống Staff khi đặt lịch ("Không yêu cầu cụ thể"). Khi đó:
- Hệ thống tự động gán staff còn trống lịch vào khung giờ được chọn (thuật toán đơn giản: chọn staff đầu tiên rảnh).
- Nếu không có staff nào rảnh, hệ thống báo "Unavailable" giống như case đã chọn staff cụ thể nhưng bị trùng lịch.

## BR-011 – Ràng buộc StaffSchedule — **[RESOLVED: bổ sung mới]**
Một appointment chỉ được tạo trong khung giờ nằm trong `StaffSchedule` (ca làm việc) của staff được chọn/gán. Nếu StaffSchedule chưa được Admin thiết lập cho ngày đó, hệ thống mặc định dùng giờ hoạt động chung của salon (cấu hình toàn cục, ví dụ 08:00–20:00).

---

# 9. Main Features (chi tiết)

## 9.1. Nail Design Gallery

Customer/Guest có thể:

- Xem danh sách design.
- Search design theo tên.
- Filter theo category.
- Xem hình ảnh, giá phụ thu.
- Xem chi tiết design.
- *(Phase 2)* Favorite design.

Ví dụ category: French, Chrome, Cute, Luxury, Minimal, Korean.

## 9.2. Service Management

Admin quản lý CRUD Service, hiển thị công khai cho Guest/Customer.

## 9.3. Booking Engine

Là module lõi của hệ thống — bao gồm logic chọn design/service/staff/thời gian, kiểm tra availability (kết hợp BR-003, BR-009, BR-010, BR-011), tính TotalPrice (BR-008), và quản lý vòng đời appointment (mục 7).

## 9.4. Review System

Rating 1–5 sao + comment, chỉ cho phép sau khi Completed (BR-005), Admin có thể ẩn review vi phạm nội dung nhưng không sửa nội dung của khách.

## 9.5. Admin Dashboard

Thống kê: tổng khách hàng, tổng appointment, appointment hoàn thành, doanh thu cơ bản theo khoảng thời gian (ngày/tuần/tháng).

---

# 10. Main Screens

## 10.1. Customer

```text
Home
 ├── Nail Designs (Gallery)
 ├── Design Detail
 ├── Services
 ├── Booking (multi-step: Design → Service → Staff → Time → Confirm)
 ├── My Appointments
 ├── Favorites (Phase 2)
 ├── Reviews
 └── Profile
```

## 10.2. Staff

```text
Staff Dashboard
 ├── Today's Appointments
 ├── Schedule (ca làm việc cá nhân)
 ├── Appointment Detail
 ├── Customer Information
 └── Completed Services (upload kết quả)
```

## 10.3. Admin

```text
Admin Dashboard
 ├── Users
 ├── Staff
 │    └── Staff Schedule
 ├── Services
 ├── Categories
 ├── Nail Designs
 ├── Appointments
 ├── Reviews
 └── Reports
```

---

# 11. Data Dictionary (Entity chi tiết)

## 11.1. User

| Field | Type | Constraint |
|---|---|---|
| Id | Guid/int | PK |
| FullName | string(100) | Required |
| Email | string(150) | Required, Unique |
| PasswordHash | string | Required |
| Phone | string(15) | Required |
| Role | enum | `Customer` \| `Staff` \| `Admin` |
| Status | enum | `Active` \| `Inactive` |
| CreatedAt | datetime | Required |
| UpdatedAt | datetime | Required |

## 11.2. StaffProfile — **[RESOLVED: bổ sung mới, tách khỏi User]**

| Field | Type | Constraint |
|---|---|---|
| Id | Guid/int | PK |
| UserId | FK → User.Id | Required, Unique (1:1) |
| Specialty | string(200) | Optional (vd: "Chrome, 3D Art") |
| AvatarUrl | string | Optional |
| Bio | string(500) | Optional |

## 11.3. StaffSchedule — **[RESOLVED: bổ sung mới]**

| Field | Type | Constraint |
|---|---|---|
| Id | Guid/int | PK |
| StaffId | FK → User.Id | Required |
| WorkDate | date | Required |
| StartTime | time | Required |
| EndTime | time | Required |
| Status | enum | `Working` \| `Off` |

> Dùng để tính "Available Time" khi Customer đặt lịch (kết hợp trừ đi các khung giờ đã có Appointment).

## 11.4. Service

| Field | Type | Constraint |
|---|---|---|
| Id | Guid/int | PK |
| Name | string(150) | Required |
| Description | string(1000) | Optional |
| Price | decimal | Required, ≥ 0 |
| Duration | int (phút) | Required, > 0 |
| Status | enum | `Active` \| `Inactive` |
| CreatedAt | datetime | Required |
| UpdatedAt | datetime | Required |

## 11.5. Category

| Field | Type | Constraint |
|---|---|---|
| Id | Guid/int | PK |
| Name | string(100) | Required, Unique |
| Description | string(500) | Optional |

## 11.6. NailDesign

| Field | Type | Constraint |
|---|---|---|
| Id | Guid/int | PK |
| Name | string(150) | Required |
| ImageUrl | string | Required |
| CategoryId | FK → Category.Id | Required |
| ExtraPrice | decimal | Required, ≥ 0 |
| Status | enum | `Active` \| `Inactive` |

## 11.7. ServiceNailDesign (bảng liên kết N:N) — **[RESOLVED: bổ sung theo BR-009]**

| Field | Type | Constraint |
|---|---|---|
| ServiceId | FK → Service.Id | Required |
| NailDesignId | FK → NailDesign.Id | Required |

> Composite PK (ServiceId, NailDesignId). Nếu MVP tạm bỏ ràng buộc này (theo ghi chú ở BR-009), bảng này có thể để trống/không dùng nhưng schema vẫn nên có sẵn.

## 11.8. Appointment

| Field | Type | Constraint |
|---|---|---|
| Id | Guid/int | PK |
| CustomerId | FK → User.Id | Required |
| StaffId | FK → User.Id | Nullable (BR-010) |
| ServiceId | FK → Service.Id | Required |
| NailDesignId | FK → NailDesign.Id | Required |
| AppointmentDate | date | Required |
| StartTime | time | Required |
| EndTime | time | Required, tính theo BR-004 |
| TotalPrice | decimal | Required, tính theo BR-008 |
| Status | enum | `Pending`\|`Confirmed`\|`InProgress`\|`Completed`\|`Cancelled` |
| Note | string(500) | Optional |
| CreatedAt | datetime | Required |
| UpdatedAt | datetime | Required |

## 11.9. Review

| Field | Type | Constraint |
|---|---|---|
| Id | Guid/int | PK |
| AppointmentId | FK → Appointment.Id | Required, Unique (1:0..1) |
| CustomerId | FK → User.Id | Required |
| Rating | int | Required, 1–5 |
| Comment | string(1000) | Optional |
| CreatedAt | datetime | Required |

## 11.10. Favorite (Phase 2)

| Field | Type | Constraint |
|---|---|---|
| Id | Guid/int | PK |
| UserId | FK → User.Id | Required |
| NailDesignId | FK → NailDesign.Id | Required |
| CreatedAt | datetime | Required |

---

# 12. ERD — Full Design

```text
┌──────────────┐        ┌────────────────┐
│     User     │───1:1──│  StaffProfile  │
├──────────────┤        ├────────────────┤
│ Id           │        │ Id             │
│ FullName     │        │ UserId (FK)    │
│ Email        │        │ Specialty      │
│ PasswordHash │        │ AvatarUrl      │
│ Phone        │        │ Bio            │
│ Role         │        └────────────────┘
│ Status       │
└──────┬───────┘
       │ 1:N (as Staff)
       ▼
┌────────────────┐
│  StaffSchedule  │
├────────────────┤
│ Id             │
│ StaffId (FK)   │
│ WorkDate       │
│ StartTime      │
│ EndTime        │
│ Status         │
└────────────────┘

┌──────────────┐
│     User     │
└──────┬───────┘
       │ CustomerId / StaffId (nullable)
       ▼
┌───────────────────┐
│    Appointment    │
├───────────────────┤
│ Id                │
│ CustomerId        │
│ StaffId (nullable)│
│ ServiceId         │
│ NailDesignId      │
│ AppointmentDate   │
│ StartTime         │
│ EndTime           │
│ TotalPrice        │
│ Status            │
│ Note              │
└─────┬─────┬───────┘
      │     │
      │     └────────────────┐
      ▼                      ▼
┌─────────────┐        ┌──────────────────┐
│   Service   │───N:N──│ ServiceNailDesign │
├─────────────┤        └─────────┬────────┘
│ Id          │                  │
│ Name        │                  ▼
│ Price       │           ┌─────────────┐
│ Duration    │           │ NailDesign  │
│ Status      │           ├─────────────┤
└─────────────┘           │ Id          │
                           │ Name        │
                           │ ImageUrl    │
                           │ CategoryId  │
                           │ ExtraPrice  │
                           │ Status      │
                           └──────┬──────┘
                                  ▼
                           ┌─────────────┐
                           │  Category   │
                           ├─────────────┤
                           │ Id          │
                           │ Name        │
                           │ Description │
                           └─────────────┘

┌─────────────────┐
│     Review      │
├─────────────────┤
│ Id              │
│ AppointmentId   │  (1:0..1 với Appointment)
│ CustomerId      │
│ Rating          │
│ Comment         │
│ CreatedAt       │
└─────────────────┘

┌─────────────────┐   (Phase 2)
│    Favorite     │
├─────────────────┤
│ Id              │
│ UserId          │
│ NailDesignId    │
│ CreatedAt       │
└─────────────────┘
```

---

# 13. Entity Relationships

| Relationship | Type |
|---|---|
| User → Appointment as Customer | 1:N |
| User → Appointment as Staff | 1:N (nullable) |
| User → StaffProfile | 1:1 |
| User → StaffSchedule as Staff | 1:N |
| Service → Appointment | 1:N |
| NailDesign → Appointment | 1:N |
| Category → NailDesign | 1:N |
| Service ↔ NailDesign | N:N (qua ServiceNailDesign) |
| Appointment → Review | 1:0..1 |
| User → Review | 1:N |
| User → Favorite (Phase 2) | 1:N |
| NailDesign → Favorite (Phase 2) | 1:N |

---

# 14. Non-Functional Requirements — **[RESOLVED: bổ sung mới]**

## 14.1. Performance
- API response time trung bình < 500ms cho các thao tác đọc dữ liệu (view gallery, service list) với tải dưới 100 concurrent users.
- Truy vấn "Available Time" phải trả kết quả trong < 1s.

## 14.2. Security
- Mật khẩu lưu dạng hash (bcrypt hoặc tương đương), không lưu plaintext.
- JWT access token có thời gian sống ngắn (ví dụ 15–60 phút), kèm refresh token.
- Toàn bộ endpoint quản trị (Admin) phải kiểm tra role ở middleware, không chỉ ở UI.
- Validate input ở cả client và server (chống SQL Injection, XSS) — dùng FluentValidation ở backend.

## 14.3. Scalability
- Kiến trúc Clean Architecture cho phép tách Infrastructure (DB, file storage) độc lập, dễ chuyển đổi sang cloud storage (S3/Azure Blob) khi cần mở rộng upload ảnh.

## 14.4. Usability
- Booking flow tối đa 5–6 bước, có thể quay lại bước trước mà không mất dữ liệu đã chọn.
- Responsive tối thiểu cho desktop và tablet (MVP không bắt buộc mobile app riêng, nhưng web nên responsive).

## 14.5. Availability & Reliability
- Sao lưu (backup) database tối thiểu hàng ngày trong môi trường production.
- Xử lý lỗi có thông báo rõ ràng cho người dùng (không lộ stack trace).

## 14.6. Maintainability
- Tuân thủ Clean Architecture, có unit test tối thiểu cho các business rule quan trọng (BR-003, BR-004, BR-005, BR-007, BR-008).
- Swagger/OpenAPI đầy đủ cho mọi endpoint.

---

# 15. API Overview (gợi ý nhóm endpoint theo module)

| Module | Ví dụ endpoint | Role truy cập |
|---|---|---|
| Auth | `POST /api/auth/register`, `POST /api/auth/login` | Guest |
| NailDesign | `GET /api/nail-designs`, `GET /api/nail-designs/{id}` | Guest/Customer |
| NailDesign (Admin) | `POST/PUT/DELETE /api/admin/nail-designs` | Admin |
| Service | `GET /api/services` | Guest/Customer |
| Service (Admin) | `POST/PUT/DELETE /api/admin/services` | Admin |
| Booking | `GET /api/booking/available-slots`, `POST /api/appointments` | Customer |
| Appointment | `GET /api/appointments/me`, `PUT /api/appointments/{id}/cancel` | Customer |
| Appointment (Staff) | `PUT /api/staff/appointments/{id}/confirm`, `.../complete` | Staff |
| StaffSchedule (Admin) | `POST/PUT /api/admin/staff-schedules` | Admin |
| Review | `POST /api/reviews`, `GET /api/nail-designs/{id}/reviews` | Customer / Guest (read) |
| Dashboard | `GET /api/admin/dashboard/summary` | Admin |

> Đây là gợi ý mức module, chi tiết request/response DTO nên thiết kế riêng ở tài liệu API Spec khi bắt đầu Phase 1.

---

# 16. Suggested Technology Stack

## Backend
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- JWT Authentication
- FluentValidation
- PostgreSQL

## Frontend
- React + TypeScript
- Tailwind CSS
- Axios

## Development
- Git / GitHub
- Swagger / OpenAPI
- Postman

---

# 17. Suggested Architecture

```text
Nailify.sln
│
├── Nailify.Api            → Controllers, Middleware, Authentication, API config
├── Nailify.Application     → Use cases, Services, DTO, Validation, Business logic
├── Nailify.Domain          → Entities, Enums, Domain rules
├── Nailify.Infrastructure  → EF Core, PostgreSQL, Repository, Auth impl, File storage
└── Nailify.Contracts       → Request/Response DTO, API contracts
```

---

# 18. MVP Development Plan

## Phase 1 – Foundation
- Create solution, setup PostgreSQL, configure EF Core.
- Create entities (bao gồm StaffProfile, StaffSchedule, ServiceNailDesign).
- Create migrations.
- Setup JWT, Register/Login.

## Phase 2 – Catalog
- Service CRUD, Category CRUD, Nail Design CRUD.
- Gallery, Search & Filter.
- Thiết lập ServiceNailDesign (nếu áp dụng ràng buộc BR-009 ngay).

## Phase 3 – Booking
- Staff management, StaffSchedule management.
- Available time calculation (BR-011).
- Create appointment, appointment validation (BR-003, BR-004, BR-008, BR-010).
- Appointment status transitions (mục 7).

## Phase 4 – Review
- Customer review, rating, review management.

## Phase 5 – Dashboard
- Booking statistics, customer statistics, revenue statistics.
- Popular services, popular nail designs.

---

# 19. Future Development

## Phase 2 (sau MVP)
```text
Online Payment
Deposit
Notification
Favorite
Before / After Gallery
Promotion / Coupon
```

## Phase 3
```text
AI Nail Recommendation
Personalized Recommendation
Customer Analytics
Loyalty Program
Membership
```

---

# 20. MVP Success Criteria

MVP được xem là hoàn thành khi:

- Customer có thể đăng ký và đăng nhập.
- Guest có thể xem mẫu nail và dịch vụ mà không cần đăng nhập.
- Customer có thể xem mẫu nail, xem service.
- Customer có thể tạo appointment (kèm chọn/không chọn staff — BR-010).
- Hệ thống ngăn booking bị trùng (BR-003) và tôn trọng ca làm việc của staff (BR-011).
- Staff có thể xem và xử lý appointment, hoàn thành appointment.
- Customer có thể review appointment đã completed.
- Admin có thể CRUD các dữ liệu chính, bao gồm StaffSchedule.
- Admin có thể xem dashboard cơ bản.
- Authentication và authorization hoạt động đúng theo ma trận quyền (mục 4.5).

---

# 21. MVP Boundary

### BẮT BUỘC
```text
Authentication + Role Management
Nail Design + Service
Booking + Staff + StaffSchedule
Appointment (đầy đủ state machine)
Review
Admin Dashboard
```

### ĐỂ SAU
```text
Payment + Notification
Favorite
Promotion + AI Recommendation
Loyalty
```

---

# 22. Assumptions & Open Questions

Các giả định cần Product Owner / giảng viên xác nhận trước khi code:

1. **BR-009** (ràng buộc Service ↔ NailDesign): áp dụng ngay ở MVP hay để tự do chọn tổ hợp bất kỳ?
2. **BR-010** (staff optional): có cần thuật toán ưu tiên gán staff (ví dụ theo rating, theo số lượng appointment ít nhất) hay chỉ cần "staff đầu tiên rảnh" là đủ cho MVP?
3. Giờ hoạt động mặc định của salon khi chưa có StaffSchedule là bao nhiêu (ví dụ 08:00–20:00)?
4. Ảnh mẫu nail lưu trữ ở đâu cho MVP — local file server hay cloud storage ngay từ đầu?
5. Có giới hạn số lượng appointment PENDING tối đa mà 1 Customer được tạo cùng lúc không (tránh spam giữ chỗ)?

---

# 23. Glossary

| Thuật ngữ | Giải thích |
|---|---|
| MVP | Minimum Viable Product — phiên bản tối thiểu có thể vận hành |
| BR | Business Rule |
| ERD | Entity Relationship Diagram |
| JWT | JSON Web Token, dùng cho xác thực |
| Slot | Khung thời gian trống có thể đặt lịch |
