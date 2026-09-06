using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nailify.Contracts.Bookings;
using Nailify.Domain.Entities;
using Nailify.Domain.Enums;
using Nailify.Infrastructure.Persistence;

namespace Nailify.Api.Controllers;

[ApiController]
[Authorize(Policy = "CustomerOnly")]
[Route("api/appointments/{appointmentId:guid}/services/{serviceId:guid}/review")]
public class ReviewsController(NailifyDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create(Guid appointmentId, Guid serviceId, CreateReviewRequest request, CancellationToken ct)
    {
        if (request.Rating is < 1 or > 5 || request.Comment?.Length > 1_000)
            return BadRequest(new { code = "INVALID_REVIEW", message = "Rating must be from 1 to 5 and comment at most 1000 characters." });

        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var appointment = await db.Appointments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == appointmentId && x.CustomerId == customerId, ct);
        if (appointment is null) return NotFound();
        if (appointment.Status != AppointmentStatus.Completed)
            return Conflict(new { code = "APPOINTMENT_NOT_COMPLETED", message = "Only completed services can be reviewed." });
        if (!await db.AppointmentServices.AnyAsync(x => x.AppointmentId == appointmentId && x.ServiceId == serviceId, ct))
            return BadRequest(new { code = "SERVICE_NOT_IN_APPOINTMENT" });
        if (await db.Reviews.AnyAsync(x => x.AppointmentId == appointmentId && x.ServiceId == serviceId, ct))
            return Conflict(new { code = "REVIEW_ALREADY_EXISTS", message = "This service has already been reviewed." });

        var review = new Review
        {
            AppointmentId = appointmentId,
            ServiceId = serviceId,
            CustomerId = customerId,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            IsApproved = false,
            IsVisible = true
        };
        db.Reviews.Add(review);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException)
        {
            return Conflict(new { code = "REVIEW_ALREADY_EXISTS" });
        }
        return Created(string.Empty, new ReviewDto(review.Id, review.Rating, review.Comment, review.IsApproved, review.CreatedAt));
    }
}
