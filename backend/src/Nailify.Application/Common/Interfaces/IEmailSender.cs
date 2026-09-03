namespace Nailify.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendPasswordResetCodeAsync(string recipientEmail, string code, CancellationToken cancellationToken = default);
}
