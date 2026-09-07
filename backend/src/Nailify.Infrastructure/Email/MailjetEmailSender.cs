using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Nailify.Application.Common.Interfaces;

namespace Nailify.Infrastructure.Email;

public sealed class MailjetEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly MailjetOptions _options;

    public MailjetEmailSender(HttpClient httpClient, IOptions<MailjetOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task SendPasswordResetCodeAsync(string recipientEmail, string code, CancellationToken cancellationToken = default)
    {
        await SendAsync(recipientEmail, "Mã đặt lại mật khẩu Nailify",
            $"Mã đặt lại mật khẩu Nailify của bạn là {code}. Mã hết hạn sau 10 phút.",
            $"<p>Mã đặt lại mật khẩu Nailify của bạn:</p><p style=\"font-size:28px;font-weight:700;letter-spacing:6px\">{code}</p><p>Mã hết hạn sau 10 phút.</p>", cancellationToken);
    }

    public async Task SendAsync(string recipientEmail, string subject, string textContent, string htmlContent, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
            throw new InvalidOperationException("Mailjet is not configured. Set Mailjet API credentials and a verified sender address.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mailjet.com/v3.1/send");
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ApiKey}:{_options.SecretKey}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = JsonContent.Create(new
        {
            Messages = new[]
            {
                new
                {
                    From = new { Email = _options.FromEmail, Name = _options.FromName },
                    To = new[] { new { Email = recipientEmail } },
                    Subject = subject,
                    TextPart = textContent,
                    HTMLPart = htmlContent
                }
            }
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Mailjet không thể gửi email.");
    }
}
