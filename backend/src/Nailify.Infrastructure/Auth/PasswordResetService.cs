using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Nailify.Application.Common.Interfaces;

namespace Nailify.Infrastructure.Auth;

public sealed class PasswordResetService : IPasswordResetService
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan RequestCooldown = TimeSpan.FromMinutes(1);
    private readonly IMemoryCache _cache;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;

    public PasswordResetService(IMemoryCache cache, IUserRepository users, IPasswordHasher passwordHasher, IEmailSender emailSender)
    {
        _cache = cache;
        _users = users;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
    }

    public async Task RequestCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var cooldownKey = $"password-reset:cooldown:{normalizedEmail}";
        if (_cache.TryGetValue(cooldownKey, out _)) return;

        _cache.Set(cooldownKey, true, RequestCooldown);
        var user = await _users.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null) return; // Avoid account enumeration.

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        _cache.Set($"password-reset:code:{normalizedEmail}", new ResetCode(_passwordHasher.Hash(code), 0), CodeLifetime);
        await _emailSender.SendPasswordResetCodeAsync(normalizedEmail, code, cancellationToken);
    }

    public Task<string> VerifyCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var key = $"password-reset:code:{normalizedEmail}";
        if (!_cache.TryGetValue(key, out ResetCode? resetCode) || resetCode is null || resetCode.Attempts >= 5 || !_passwordHasher.Verify(code, resetCode.Hash))
        {
            if (resetCode is not null) _cache.Set(key, resetCode with { Attempts = resetCode.Attempts + 1 }, CodeLifetime);
            throw new UnauthorizedAccessException("The verification code is invalid or expired.");
        }

        _cache.Remove(key);
        var resetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _cache.Set($"password-reset:token:{resetToken}", normalizedEmail, TimeSpan.FromMinutes(10));
        return Task.FromResult(resetToken);
    }

    public async Task ResetPasswordAsync(string resetToken, string newPassword, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue($"password-reset:token:{resetToken}", out string? email) || email is null)
            throw new UnauthorizedAccessException("The password reset session is invalid or expired.");

        var user = await _users.GetByEmailAsync(email, cancellationToken)
            ?? throw new UnauthorizedAccessException("The password reset session is invalid.");
        user.PasswordHash = _passwordHasher.Hash(newPassword);
        await _users.SaveChangesAsync(cancellationToken);
        _cache.Remove($"password-reset:token:{resetToken}");
    }

    private sealed record ResetCode(string Hash, int Attempts);
}
