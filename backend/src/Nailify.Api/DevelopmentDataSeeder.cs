using Microsoft.EntityFrameworkCore;
using Nailify.Domain.Entities;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Api;

internal static class DevelopmentDataSeeder
{
    public static async Task SeedServicesAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NailifyDbContext>();

        if (!await db.Services.AnyAsync(cancellationToken))
        {
            db.Services.AddRange(
                new Service { Name = "Classic Gel Manicure", Category = ServiceCategory.Manicure, Description = "Nail shaping, cuticle care and long-lasting gel color.", Price = 35, DurationMinutes = 45, IsFeatured = true, DisplayOrder = 1 },
                new Service { Name = "Spa Pedicure", Category = ServiceCategory.Pedicure, Description = "Relaxing foot care with exfoliation, massage and polish.", Price = 48, DurationMinutes = 60, IsFeatured = true, DisplayOrder = 2 },
                new Service { Name = "French Tips", Category = ServiceCategory.NailArt, Description = "A clean French finish added to your chosen manicure.", Price = 12, DurationMinutes = 20, DisplayOrder = 3 },
                new Service { Name = "Simple Nail Art", Category = ServiceCategory.NailArt, Description = "Minimal accents and hand-painted details for up to two nails.", Price = 15, DurationMinutes = 25, DisplayOrder = 4 },
                new Service { Name = "Gel Removal", Category = ServiceCategory.AddOn, Description = "Gentle professional gel removal before your next service.", Price = 10, DurationMinutes = 15, DisplayOrder = 5 },
                new Service { Name = "Nail Repair", Category = ServiceCategory.Other, Description = "Repair and reinforce one damaged nail.", Price = 6, DurationMinutes = 10, DisplayOrder = 6 }
            );
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.Categories.AnyAsync(cancellationToken))
        {
            db.Categories.AddRange(
                new Category { Name = "French", Description = "Classic and modern French styles." },
                new Category { Name = "Chrome", Description = "Reflective chrome nail finishes." },
                new Category { Name = "Minimal", Description = "Clean, understated nail art." },
                new Category { Name = "Korean", Description = "Soft Korean-inspired nail looks." }
            );
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.NailDesigns.AnyAsync(cancellationToken))
        {
            var categories = await db.Categories.ToDictionaryAsync(x => x.Name, cancellationToken);
            db.NailDesigns.AddRange(
                new NailDesign { Name = "Soft French", CategoryId = categories["French"].Id, ImageUrl = "https://images.unsplash.com/photo-1604654894610-df63bc536371?auto=format&fit=crop&w=900&q=85", ExtraPrice = 8, IsFeatured = true, DisplayOrder = 1 },
                new NailDesign { Name = "Ocean Chrome", CategoryId = categories["Chrome"].Id, ImageUrl = "https://images.unsplash.com/photo-1604902396830-aca29e19d4c9?auto=format&fit=crop&w=900&q=85", ExtraPrice = 12, IsFeatured = true, DisplayOrder = 2 },
                new NailDesign { Name = "Clean Minimal", CategoryId = categories["Minimal"].Id, ImageUrl = "https://images.unsplash.com/photo-1610992015732-2449b76344bc?auto=format&fit=crop&w=900&q=85", ExtraPrice = 6, IsFeatured = true, DisplayOrder = 3 },
                new NailDesign { Name = "Korean Glow", CategoryId = categories["Korean"].Id, ImageUrl = "https://images.unsplash.com/photo-1522335789203-aabd1fc54bc9?auto=format&fit=crop&w=900&q=85", ExtraPrice = 10, DisplayOrder = 4 }
            );
            await db.SaveChangesAsync(cancellationToken);
        }

        await SeedVietnameseCatalogAsync(db, cancellationToken);
        await NormalizeLegacyDemoPricesAsync(db, cancellationToken);

        var designsWithoutDuration = await db.NailDesigns
            .Where(x => x.AdditionalDurationMinutes == 0)
            .ToListAsync(cancellationToken);
        if (designsWithoutDuration.Count > 0)
        {
            foreach (var design in designsWithoutDuration) design.AdditionalDurationMinutes = 15;
            await db.SaveChangesAsync(cancellationToken);
        }

        var compatibleServices = await db.Services
            .Where(x => x.IsActive && x.IsBookable)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        var bookableDesigns = await db.NailDesigns
            .Where(x => x.Status == UserStatus.Active && x.IsBookable)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        var existingLinkKeys = (await db.ServiceNailDesigns
                .Select(x => new { x.ServiceId, x.NailDesignId })
                .ToListAsync(cancellationToken))
            .Select(x => (x.ServiceId, x.NailDesignId))
            .ToHashSet();
        var missingLinks = compatibleServices
            .SelectMany(serviceId => bookableDesigns.Select(nailDesignId => (serviceId, nailDesignId)))
            .Where(link => !existingLinkKeys.Contains(link))
            .Select(link => new ServiceNailDesign { ServiceId = link.serviceId, NailDesignId = link.nailDesignId })
            .ToList();

        if (missingLinks.Count > 0)
        {
            db.ServiceNailDesigns.AddRange(missingLinks);
            await db.SaveChangesAsync(cancellationToken);
        }

        // Development data makes the public slot API usable immediately. These accounts are staff-only
        // and intentionally have no login password; customers should register through the normal flow.
        if (!await db.Users.AnyAsync(x => x.Role == UserRole.Staff, cancellationToken))
        {
            var anna = new User { FullName = "Anna Nguyen", Email = "anna.staff@nailify.local", PhoneNumber = "0900000001", PasswordHash = "development-staff-account", Role = UserRole.Staff };
            var linh = new User { FullName = "Linh Tran", Email = "linh.staff@nailify.local", PhoneNumber = "0900000002", PasswordHash = "development-staff-account", Role = UserRole.Staff };
            db.Users.AddRange(anna, linh);
            db.StaffProfiles.AddRange(
                new StaffProfile { User = anna, Specialty = "Gel manicure and minimal designs" },
                new StaffProfile { User = linh, Specialty = "Pedicure and French designs" });
            await db.SaveChangesAsync(cancellationToken);
        }

        var staffMembers = await db.Users.Where(x => x.Role == UserRole.Staff && x.Status == UserStatus.Active).OrderBy(x => x.Email).ToListAsync(cancellationToken);
        foreach (var staff in staffMembers.Where(x => x.PasswordHash == "development-staff-account"))
            staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123");
        if (!await db.Users.AnyAsync(x => x.Role == UserRole.Admin, cancellationToken))
            db.Users.Add(new User { FullName = "Nailify Manager", Email = "admin@nailify.local", PhoneNumber = "0900000099", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), Role = UserRole.Admin });
        await db.SaveChangesAsync(cancellationToken);
        var bookableServices = await db.Services.Where(x => x.IsActive && x.IsBookable).ToListAsync(cancellationToken);
        if (staffMembers.Count > 0)
        {
            var existingStaffServiceKeys = (await db.StaffServices
                    .Select(x => new { x.StaffId, x.ServiceId })
                    .ToListAsync(cancellationToken))
                .Select(x => (x.StaffId, x.ServiceId))
                .ToHashSet();
            var missingStaffServices = staffMembers
                .SelectMany(staff => bookableServices.Select(service => (StaffId: staff.Id, ServiceId: service.Id)))
                .Where(link => !existingStaffServiceKeys.Contains(link))
                .Select(link => new StaffService { StaffId = link.StaffId, ServiceId = link.ServiceId })
                .ToList();
            if (missingStaffServices.Count > 0)
            {
                db.StaffServices.AddRange(missingStaffServices);
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        if (!await db.StaffSchedules.AnyAsync(cancellationToken) && staffMembers.Count > 0)
        {
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            var schedules = new List<StaffSchedule>();
            for (var dayOffset = 0; dayOffset <= 30; dayOffset++)
            {
                var workDate = startDate.AddDays(dayOffset);
                if (workDate.DayOfWeek is DayOfWeek.Sunday) continue;
                schedules.AddRange(staffMembers.Select(staff => new StaffSchedule
                {
                    StaffId = staff.Id,
                    WorkDate = workDate,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(17, 0),
                    Status = ScheduleStatus.Working
                }));
            }
            db.StaffSchedules.AddRange(schedules);
            await db.SaveChangesAsync(cancellationToken);
        }

        await SeedCompletedAppointmentsAndReviews(db, staffMembers, bookableServices, cancellationToken);
    }

    private static async Task SeedVietnameseCatalogAsync(NailifyDbContext db, CancellationToken cancellationToken)
    {
        var serviceSeeds = new[]
        {
            ("Sơn gel cơ bản", ServiceCategory.Manicure, "Cắt sửa móng, chăm sóc da quanh móng và sơn gel bền đẹp.", 250000m, 45, true, 1),
            ("Chăm sóc chân spa", ServiceCategory.Pedicure, "Ngâm chân, tẩy da chết, massage thư giãn và sơn màu.", 450000m, 60, true, 2),
            ("Sơn gel cao cấp", ServiceCategory.Manicure, "Làm sạch da chết kỹ lưỡng, tạo form và sơn gel cao cấp.", 350000m, 60, true, 3),
            ("Đắp bột ombre", ServiceCategory.NailArt, "Đắp bột nhẹ, tạo hiệu ứng ombre mềm mại và bền chắc.", 750000m, 90, true, 4),
            ("Tháo gel an toàn", ServiceCategory.AddOn, "Tháo gel chuyên nghiệp, hạn chế tổn thương bề mặt móng.", 80000m, 15, false, 5),
            ("Sửa móng gãy", ServiceCategory.Other, "Gia cố và phục hồi một móng bị gãy hoặc nứt.", 50000m, 10, false, 6),
            ("Sơn French", ServiceCategory.NailArt, "Đầu móng French thanh lịch, phù hợp mọi dịp.", 180000m, 25, true, 7),
            ("Chăm sóc móng tay spa", ServiceCategory.Manicure, "Tẩy da chết, đắp mặt nạ tay và massage dưỡng ẩm.", 300000m, 55, false, 8),
            ("Đắp gel builder", ServiceCategory.Other, "Gia cố móng thật bằng gel builder, giảm gãy yếu.", 650000m, 75, true, 9),
            ("Vẽ charm nổi", ServiceCategory.NailArt, "Điểm xuyết charm, đá và họa tiết nổi theo yêu cầu.", 250000m, 30, false, 10)
        };

        foreach (var seed in serviceSeeds)
        {
            var service = await db.Services.FirstOrDefaultAsync(x => x.DisplayOrder == seed.Item7, cancellationToken);
            if (service is null)
            {
                db.Services.Add(new Service { Name = seed.Item1, Category = seed.Item2, Description = seed.Item3, Price = seed.Item4, DurationMinutes = seed.Item5, IsFeatured = seed.Item6, DisplayOrder = seed.Item7 });
            }
            else
            {
                service.Name = seed.Item1; service.Category = seed.Item2; service.Description = seed.Item3; service.Price = seed.Item4; service.DurationMinutes = seed.Item5; service.IsFeatured = seed.Item6;
            }
        }

        var categorySeeds = new[] { ("French hiện đại", "Kiểu French cổ điển và biến tấu hiện đại."), ("Hiệu ứng Chrome", "Bề mặt ánh kim và tráng gương."), ("Tối giản", "Thiết kế thanh lịch, nhẹ nhàng."), ("Phong cách Hàn", "Màu trong trẻo và điểm nhấn dễ thương."), ("Đính đá", "Mẫu nổi bật với đá, charm và phụ kiện.") };
        foreach (var seed in categorySeeds)
        {
            if (!await db.Categories.AnyAsync(x => x.Name == seed.Item1, cancellationToken)) db.Categories.Add(new Category { Name = seed.Item1, Description = seed.Item2 });
        }
        await db.SaveChangesAsync(cancellationToken);

        var categories = await db.Categories.ToDictionaryAsync(x => x.Name, cancellationToken);
        var designSeeds = new[]
        {
            ("French sữa", "French hiện đại", 80000m, 15, true, 1, "https://images.fresha.com/locations/location-profile-images/500430/5594557/68a7e831-2253-4cbd-a2d8-3c10b834927e-LaurasNailBoutique-GB-England-SilverEnd-Fresha.jpg"),
            ("Chrome đại dương", "Hiệu ứng Chrome", 120000m, 20, true, 2, "https://images.unsplash.com/photo-1607779097040-26e80aa78e66?auto=format&fit=crop&w=900&q=85"),
            ("Tối giản thanh lịch", "Tối giản", 60000m, 15, true, 3, "https://i.pinimg.com/originals/94/e8/60/94e86009ff6714bb303859b5f8338807.jpg"),
            ("Ánh hồng Hàn Quốc", "Phong cách Hàn", 100000m, 20, true, 4, "https://images.unsplash.com/photo-1631729371254-42c2892f0e6e?auto=format&fit=crop&w=900&q=85"),
            ("French đỏ rượu", "French hiện đại", 90000m, 15, false, 5, "https://images.fresha.com/locations/location-profile-images/1244916/5085793/1a3cbcb6-28a4-403d-9141-40a6598424ee-ChicNailStudio-US-Virginia-Arlington-Barcroft-Fresha.jpg?class=venue-gallery-small&f_width=1200"),
            ("Chrome ngọc trai", "Hiệu ứng Chrome", 120000m, 20, true, 6, "https://static.wixstatic.com/media/14f7dd_341c966a1b0d4e1dbb6562c932dcb0fd~mv2.jpg/v1/fit/w_1200,h_900,al_c/14f7dd_341c966a1b0d4e1dbb6562c932dcb0fd~mv2.jpg"),
            ("Hoa cúc nhỏ", "Tối giản", 80000m, 20, false, 7, "https://images.unsplash.com/photo-1610992015732-2449b76344bc?auto=format&fit=crop&w=900&q=85"),
            ("Mắt mèo tím khói", "Phong cách Hàn", 140000m, 25, true, 8, "https://images.unsplash.com/photo-1604654894610-df63bc536371?auto=format&fit=crop&w=900&q=85"),
            ("Nơ đá sang trọng", "Đính đá", 180000m, 30, true, 9, "https://images.unsplash.com/photo-1632345031435-8727f6897d53?auto=format&fit=crop&w=900&q=85"),
            ("Loang màu mùa hè", "Tối giản", 110000m, 25, false, 10, "https://images.unsplash.com/photo-1571290274554-6a2eaa771e5f?auto=format&fit=crop&w=900&q=85"),
            ("Tráng gương bạc", "Hiệu ứng Chrome", 130000m, 20, false, 11, "https://images.unsplash.com/photo-1519014816548-bf5fe059798b?auto=format&fit=crop&w=900&q=85"),
            ("Đính đá ánh sao", "Đính đá", 200000m, 30, true, 12, "https://images.unsplash.com/photo-1522337660859-02fbefca4702?auto=format&fit=crop&w=900&q=85")
        };
        foreach (var seed in designSeeds)
        {
            var design = await db.NailDesigns.FirstOrDefaultAsync(x => x.DisplayOrder == seed.Item6, cancellationToken);
            if (design is null)
                db.NailDesigns.Add(new NailDesign { Name = seed.Item1, CategoryId = categories[seed.Item2].Id, ExtraPrice = seed.Item3, AdditionalDurationMinutes = seed.Item4, IsFeatured = seed.Item5, DisplayOrder = seed.Item6, ImageUrl = seed.Item7 });
            else
            {
                design.Name = seed.Item1; design.CategoryId = categories[seed.Item2].Id; design.ExtraPrice = seed.Item3; design.AdditionalDurationMinutes = seed.Item4; design.IsFeatured = seed.Item5; design.ImageUrl = seed.Item7;
            }
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task NormalizeLegacyDemoPricesAsync(NailifyDbContext db, CancellationToken cancellationToken)
    {
        var appointments = await db.Appointments.Where(x => x.TotalPrice > 0 && x.TotalPrice < 1_000m)
            .Include(x => x.AppointmentServices).ToListAsync(cancellationToken);
        foreach (var appointment in appointments)
        {
            appointment.TotalPrice *= 25_000m;
            appointment.DesignExtraPriceAtBooking *= 25_000m;
            foreach (var service in appointment.AppointmentServices.Where(x => x.PriceAtBooking > 0 && x.PriceAtBooking < 1_000m)) service.PriceAtBooking *= 25_000m;
        }
        if (appointments.Count > 0) await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCompletedAppointmentsAndReviews(
        NailifyDbContext db,
        IReadOnlyList<User> staffMembers,
        IReadOnlyList<Service> services,
        CancellationToken cancellationToken)
    {
        if (staffMembers.Count == 0 || services.Count == 0) return;

        var customerSeeds = new[]
        {
            (Id: Guid.Parse("7d8fdce1-b5d2-4d2b-b78f-4bd85e9a1001"), Name: "Mai Nguyễn", Email: "mai.review@nailify.local", Phone: "0901000001"),
            (Id: Guid.Parse("7d8fdce1-b5d2-4d2b-b78f-4bd85e9a1002"), Name: "Thảo Lê", Email: "thao.review@nailify.local", Phone: "0901000002"),
            (Id: Guid.Parse("7d8fdce1-b5d2-4d2b-b78f-4bd85e9a1003"), Name: "Hương Phạm", Email: "huong.review@nailify.local", Phone: "0901000003")
        };
        var existingEmails = (await db.Users.Where(x => customerSeeds.Select(seed => seed.Email).Contains(x.Email)).Select(x => x.Email).ToListAsync(cancellationToken)).ToHashSet();
        foreach (var seed in customerSeeds.Where(seed => !existingEmails.Contains(seed.Email)))
            db.Users.Add(new User { Id = seed.Id, FullName = seed.Name, Email = seed.Email, PhoneNumber = seed.Phone, PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), Role = UserRole.Customer });
        await db.SaveChangesAsync(cancellationToken);

        var customers = await db.Users.Where(x => customerSeeds.Select(seed => seed.Email).Contains(x.Email)).OrderBy(x => x.Email).ToListAsync(cancellationToken);
        var designs = await db.NailDesigns.Where(x => x.Status == UserStatus.Active).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
        var comments = new[]
        {
            "Màu gel lên đều, form móng gọn và giữ bóng rất lâu. Nhân viên tư vấn nhẹ nhàng, đúng giờ.",
            "Không gian sạch, thao tác kỹ và phần chăm sóc chân rất thư giãn. Mình sẽ quay lại.",
            "Mẫu nail hoàn thiện giống ảnh tham khảo, chi tiết tinh tế và mức giá được báo rõ từ đầu."
        };

        for (var index = 0; index < customers.Count; index++)
        {
            var appointmentId = Guid.Parse($"8e9fdce1-b5d2-4d2b-b78f-4bd85e9a20{index + 1:00}");
            var service = services[index % services.Count];
            if (!await db.Appointments.AnyAsync(x => x.Id == appointmentId, cancellationToken))
            {
                var design = designs.Count == 0 ? null : designs[index % designs.Count];
                var date = DateOnly.FromDateTime(DateTime.Today.AddDays(-14 - index * 9));
                var start = new TimeOnly(10 + index, 0);
                db.Appointments.Add(new Appointment
                {
                    Id = appointmentId,
                    CustomerId = customers[index].Id,
                    StaffId = staffMembers[index % staffMembers.Count].Id,
                    NailDesignId = design?.Id,
                    AppointmentDate = date,
                    StartTime = start,
                    EndTime = start.AddMinutes(service.DurationMinutes + (design?.AdditionalDurationMinutes ?? 0)),
                    TotalPrice = service.Price + (design?.ExtraPrice ?? 0),
                    DesignExtraPriceAtBooking = design?.ExtraPrice ?? 0,
                    DesignAdditionalDurationAtBooking = design?.AdditionalDurationMinutes ?? 0,
                    Status = AppointmentStatus.Completed,
                    AppointmentServices = [new AppointmentService { ServiceId = service.Id, PriceAtBooking = service.Price, DurationAtBooking = service.DurationMinutes }]
                });
                await db.SaveChangesAsync(cancellationToken);
            }

            if (!await db.Reviews.AnyAsync(x => x.AppointmentId == appointmentId && x.ServiceId == service.Id, cancellationToken))
                db.Reviews.Add(new Review { AppointmentId = appointmentId, ServiceId = service.Id, CustomerId = customers[index].Id, Rating = index == 1 ? 4 : 5, Comment = comments[index], IsApproved = true, IsVisible = true, IsFeatured = true, CreatedAt = DateTime.UtcNow.AddDays(-4 - index * 6) });
        }
        await db.SaveChangesAsync(cancellationToken);

        // Give the first real development customer one completed, unreviewed service so the
        // end-to-end review form can be exercised immediately from My Appointments.
        var realCustomer = await db.Users.FirstOrDefaultAsync(x => x.Role == UserRole.Customer && !x.Email.EndsWith("@nailify.local"), cancellationToken);
        var reviewableAppointmentId = Guid.Parse("8e9fdce1-b5d2-4d2b-b78f-4bd85e9a2100");
        if (realCustomer is not null && !await db.Appointments.AnyAsync(x => x.Id == reviewableAppointmentId, cancellationToken))
        {
            var service = services[0];
            var start = new TimeOnly(14, 0);
            db.Appointments.Add(new Appointment
            {
                Id = reviewableAppointmentId,
                CustomerId = realCustomer.Id,
                StaffId = staffMembers[0].Id,
                AppointmentDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
                StartTime = start,
                EndTime = start.AddMinutes(service.DurationMinutes),
                TotalPrice = service.Price,
                Status = AppointmentStatus.Completed,
                Note = "Dữ liệu demo để khách hàng thử luồng đánh giá.",
                AppointmentServices = [new AppointmentService { ServiceId = service.Id, PriceAtBooking = service.Price, DurationAtBooking = service.DurationMinutes }]
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
