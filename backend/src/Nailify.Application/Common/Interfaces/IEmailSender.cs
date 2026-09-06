namespace Nailify.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string recipientEmail, string subject, string textContent, string htmlContent, CancellationToken cancellationToken = default);
    Task SendPasswordResetCodeAsync(string recipientEmail, string code, CancellationToken cancellationToken = default);
}
