namespace Nailify.Application.Common.Interfaces;

public interface IPasswordResetService
{
    Task RequestCodeAsync(string email, CancellationToken cancellationToken = default);
    Task<string> VerifyCodeAsync(string email, string code, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(string resetToken, string newPassword, CancellationToken cancellationToken = default);
}
