using Microsoft.EntityFrameworkCore;
using Nailify.Domain.Entities;

namespace Nailify.Infrastructure.Persistence;

public class NailifyDbContext : DbContext
{
    public NailifyDbContext(DbContextOptions<NailifyDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<StaffProfile> StaffProfiles => Set<StaffProfile>();
    public DbSet<StaffSchedule> StaffSchedules => Set<StaffSchedule>();
    public DbSet<StaffService> StaffServices => Set<StaffService>();
    public DbSet<StaffTimeOff> StaffTimeOffs => Set<StaffTimeOff>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<NailDesign> NailDesigns => Set<NailDesign>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<AppointmentService> AppointmentServices => Set<AppointmentService>();
    public DbSet<ServiceNailDesign> ServiceNailDesigns => Set<ServiceNailDesign>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<CustomerFavoriteDesign> CustomerFavoriteDesigns => Set<CustomerFavoriteDesign>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<DepositPayment> DepositPayments => Set<DepositPayment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(NailifyDbContext).Assembly);

        // Appointment - Staff (nullable, BR-010) khong cascade delete de tranh mat lich su
        builder.Entity<Appointment>()
            .HasOne(a => a.Staff)
            .WithMany(u => u.AppointmentsAsStaff)
            .HasForeignKey(a => a.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.Customer)
            .WithMany(u => u.AppointmentsAsCustomer)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}
