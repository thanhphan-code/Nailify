using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nailify.Api.Controllers;

/// <summary>
/// Temporary endpoints for verifying JWT role policies from Swagger.
/// Remove or replace these when the first protected business endpoints are added.
/// </summary>
[ApiController]
[Route("api/auth-test")]
public class AuthorizationTestController : ControllerBase
{
    [HttpGet("customer")]
    [Authorize(Policy = "CustomerOnly")]
    public IActionResult CustomerOnly() => Ok(new { message = "Customer policy passed." });

    [HttpGet("staff")]
    [Authorize(Policy = "StaffOnly")]
    public IActionResult StaffOnly() => Ok(new { message = "Staff policy passed." });

    [HttpGet("staff-or-admin")]
    [Authorize(Policy = "StaffOrAdmin")]
    public IActionResult StaffOrAdmin() => Ok(new { message = "Staff or Admin policy passed." });

    [HttpGet("admin")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult AdminOnly() => Ok(new { message = "Admin policy passed." });
}
