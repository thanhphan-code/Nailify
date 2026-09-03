using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using Nailify.Application.Auth;
using Nailify.Application.Common.Interfaces;
using Nailify.Application.Auth.Commands.Login;
using Nailify.Application.Auth.Commands.Logout;
using Nailify.Application.Auth.Commands.RefreshToken;
using Nailify.Application.Auth.Commands.Register;
using Nailify.Application.Auth.Queries.GetCurrentUser;
using Nailify.Contracts.Auth;
using Nailify.Domain.Entities;

namespace Nailify.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenIssuer _tokenIssuer;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;

    public AuthController(IMediator mediator, IUserRepository users, IPasswordHasher passwordHasher, IAuthTokenIssuer tokenIssuer, IPasswordResetService passwordResetService, IMemoryCache cache, IConfiguration configuration)
    {
        _mediator = mediator;
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenIssuer = tokenIssuer;
        _passwordResetService = passwordResetService;
        _cache = cache;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegisterCommand(request), cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LoginCommand(request), cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request), cancellationToken);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new GetCurrentUserQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        return NoContent();
    }

    [HttpGet("google")]
    [AllowAnonymous]
    public IActionResult GoogleLogin()
    {
        if (!IsGoogleConfigured())
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Google sign-in is not configured.");

        return Challenge(new AuthenticationProperties { RedirectUri = Url.ActionLink(nameof(GoogleCallback)) }, "Google");
    }

    [HttpGet("google/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback(CancellationToken cancellationToken)
    {
        var result = await HttpContext.AuthenticateAsync("GoogleExternal");
        if (!result.Succeeded || result.Principal is null)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Google sign-in failed.");

        var email = result.Principal.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return Problem(statusCode: StatusCodes.Status422UnprocessableEntity, title: "Google did not return an email address.");

        var user = await _users.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                FullName = result.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0],
                Email = email,
                PhoneNumber = string.Empty,
                PasswordHash = _passwordHasher.Hash(Convert.ToHexString(RandomNumberGenerator.GetBytes(32)))
            };
            await _users.AddAsync(user, cancellationToken);
            await _users.SaveChangesAsync(cancellationToken);
        }

        var response = await _tokenIssuer.IssueTokensAsync(user, cancellationToken);
        var ticket = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _cache.Set($"google-auth-ticket:{ticket}", response, TimeSpan.FromMinutes(1));
        await HttpContext.SignOutAsync("GoogleExternal");

        var frontendUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5173";
        return Redirect($"{frontendUrl}/auth/google/callback?ticket={Uri.EscapeDataString(ticket)}");
    }

    [HttpPost("google/exchange")]
    [AllowAnonymous]
    public ActionResult<AuthResponse> ExchangeGoogleTicket([FromBody] GoogleTicketRequest request)
    {
        var key = $"google-auth-ticket:{request.Ticket}";
        if (!_cache.TryGetValue(key, out AuthResponse? response) || response is null)
            return Unauthorized(new { title = "Google sign-in session is invalid or expired." });

        _cache.Remove(key);
        return Ok(response);
    }

    [HttpPost("password-reset/request")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            return BadRequest(new { title = "A valid email is required." });

        await _passwordResetService.RequestCodeAsync(request.Email, cancellationToken);
        return NoContent();
    }

    [HttpPost("password-reset/verify")]
    [AllowAnonymous]
    public async Task<ActionResult<PasswordResetVerificationResponse>> VerifyPasswordResetCode([FromBody] PasswordResetVerifyRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code) || request.Code.Length != 6)
            return BadRequest(new { title = "Email and a 6-digit code are required." });

        var resetToken = await _passwordResetService.VerifyCodeAsync(request.Email, request.Code, cancellationToken);
        return Ok(new PasswordResetVerificationResponse(resetToken));
    }

    [HttpPost("password-reset/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] PasswordResetConfirmRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ResetToken) || !IsValidPassword(request.NewPassword))
            return BadRequest(new { title = "Password must contain 8+ characters, uppercase, lowercase, number and special character." });

        await _passwordResetService.ResetPasswordAsync(request.ResetToken, request.NewPassword, cancellationToken);
        return NoContent();
    }

    private bool IsGoogleConfigured() => !string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientId"])
        && !string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientSecret"]);

    private static bool IsValidPassword(string password) => password.Length >= 8
        && password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit) && password.Any(character => !char.IsLetterOrDigit(character));

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (sub is null || !Guid.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("Invalid token.");

        return userId;
    }
}

public sealed record GoogleTicketRequest(string Ticket);
public sealed record PasswordResetRequest(string Email);
public sealed record PasswordResetVerifyRequest(string Email, string Code);
public sealed record PasswordResetConfirmRequest(string ResetToken, string NewPassword);
public sealed record PasswordResetVerificationResponse(string ResetToken);
