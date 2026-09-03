# Nailify — Nail Salon Booking & Management Platform

Thư mục mẫu để bắt đầu code Backend (.NET, Clean Architecture) và Frontend (React + TypeScript).
Đi kèm tài liệu `docs/Nailify_SRS_Full.md` — tham chiếu entity, business rule (BR-001 → BR-011), API khi code.

## Cấu trúc tổng quan

```text
Nailify/
├── backend/            .NET solution (Clean Architecture)
│   ├── src/
│   │   ├── Nailify.Api             API layer: Controllers, Middleware, DI
│   │   ├── Nailify.Application     Use cases, business logic, validation
│   │   ├── Nailify.Domain          Entities, Enums, domain rules (không phụ thuộc layer khác)
│   │   ├── Nailify.Infrastructure  EF Core, PostgreSQL, Repository, Auth
│   │   └── Nailify.Contracts       Request/Response DTO dùng chung Api ↔ Application
│   └── tests/
│       └── Nailify.Application.Tests
│
├── frontend/            React + TypeScript + Vite + Tailwind
│   └── src/
│       ├── pages/        Trang theo role: customer/, staff/, admin/
│       ├── features/     Logic + API call theo domain: auth/, booking/, nail-designs/...
│       ├── components/   ui/ (component tái sử dụng), layout/ (header, sidebar...)
│       ├── services/     apiClient.ts (axios instance dùng chung)
│       ├── hooks/        custom hooks (useAuth, useBooking...)
│       ├── types/        TypeScript interface khớp với entity backend
│       ├── store/        state management (zustand)
│       └── routes/       cấu hình route theo role
│
└── docs/
    └── Nailify_SRS_Full.md
```

## Backend — Bắt đầu

Yêu cầu: .NET 8 SDK, PostgreSQL đã cài (hoặc chạy qua Docker).

```bash
cd backend

# Restore packages cho từng project (do project reference đã có sẵn trong .csproj)
dotnet restore

# Cập nhật connection string trong src/Nailify.Api/appsettings.json trước khi chạy

# Tạo migration đầu tiên sau khi review entity trong Nailify.Domain/Entities
dotnet ef migrations add InitialCreate \
  --project src/Nailify.Infrastructure \
  --startup-project src/Nailify.Api

dotnet ef database update \
  --project src/Nailify.Infrastructure \
  --startup-project src/Nailify.Api

# Chạy API (mặc định https://localhost:5001, xem launchSettings nếu cần)
dotnet run --project src/Nailify.Api
```

Kiểm tra nhanh: mở `https://localhost:5001/api/health` hoặc `/swagger` (môi trường Development).

**Đã có sẵn:** entity (`User`, `StaffProfile`, `StaffSchedule`, `Category`, `NailDesign`, `Service`, `Appointment`, `Review`), enum, `NailifyDbContext` với các quan hệ chính (N:N Service↔NailDesign theo BR-009, 1:0..1 Appointment↔Review), 1 controller mẫu (`HealthController`).

**Cần code tiếp (theo Phase 1 trong SRS mục 18):**
- JWT Authentication middleware + `AuthController` (Register/Login).
- Repository pattern hoặc dùng thẳng DbContext qua MediatR handler trong `Nailify.Application/Features/*`.
- FluentValidation cho từng Request DTO trong `Nailify.Contracts/Requests`.
- Logic tính available time (BR-011), check conflict (BR-003) trong `Features/Appointments`.

## Frontend — Bắt đầu

Yêu cầu: Node.js 18+.

```bash
cd frontend
npm install
cp .env.example .env    # chỉnh VITE_API_BASE_URL trỏ đúng backend

npm run dev              # http://localhost:5173, đã proxy /api sang backend
```

**Đã có sẵn:** Vite + React 18 + TypeScript + Tailwind config, React Router, TanStack Query, Zustand, React Hook Form + Zod (đã khai báo trong `package.json`, chạy `npm install` để tải về), `apiClient.ts` (axios instance tự đính JWT vào header), 1 trang mẫu `HomePage`, type mẫu khớp entity backend (`NailDesign`, `Appointment`).

**Cần code tiếp:**
- Trang Login/Register (`features/auth`).
- Booking flow nhiều bước (`features/booking`) theo mục 6.1 SRS.
- Layout theo role (Customer/Staff/Admin) trong `components/layout`.
- Kết nối route thật trong `App.tsx` (hiện đang để comment placeholder).

## Gợi ý thứ tự code (bám theo SRS mục 18 — MVP Development Plan)

1. **Phase 1 – Foundation:** Auth (Register/Login/JWT) cả 2 phía, chạy được migration đầu tiên.
2. **Phase 2 – Catalog:** CRUD Service/Category/NailDesign, trang Gallery có search/filter.
3. **Phase 3 – Booking:** StaffSchedule, tính available time, tạo/hủy appointment, state machine.
4. **Phase 4 – Review:** rating + comment sau khi Completed.
5. **Phase 5 – Dashboard:** thống kê cơ bản cho Admin.

Toàn bộ business rule chi tiết (BR-001 → BR-011) và data dictionary nằm trong `docs/Nailify_SRS_Full.md` — nên đọc lại trước khi code từng module để tránh sai logic (đặc biệt BR-003 chống trùng lịch và BR-004/BR-008 tính giá/thời gian).
