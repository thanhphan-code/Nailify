using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Nailify.Api.Services;
using Xunit;

namespace Nailify.Application.Tests;

public sealed class PayOsServiceTests
{
    private const string ChecksumKey = "test-checksum-key";

    [Fact]
    public async Task Create_payment_link_sends_signed_request_and_existing_frontend_routes()
    {
        var handler = new CapturingHandler("""
            {"code":"00","data":{"paymentLinkId":"link-123","checkoutUrl":"https://pay.payos.vn/web/link-123"}}
            """);
        var service = CreateService(handler);
        var appointmentId = Guid.Parse("21e419f9-a7d1-4d15-a86a-a07ff0ae5058");

        var result = await service.CreatePaymentLink(12345678, appointmentId, 99_000m, DateTime.UtcNow.AddMinutes(10), CancellationToken.None);

        result.PaymentLinkId.Should().Be("link-123");
        handler.RequestUri.Should().Be("https://api-merchant.payos.vn/v2/payment-requests");
        handler.ClientId.Should().Be("test-client-id");
        handler.ApiKey.Should().Be("test-api-key");

        using var body = JsonDocument.Parse(handler.Body!);
        var root = body.RootElement;
        var returnUrl = $"http://localhost:5173/deposit/{appointmentId}?payment=success";
        var cancelUrl = $"http://localhost:5173/deposit/{appointmentId}?payment=cancelled";
        root.GetProperty("returnUrl").GetString().Should().Be(returnUrl);
        root.GetProperty("cancelUrl").GetString().Should().Be(cancelUrl);
        root.GetProperty("amount").GetInt32().Should().Be(99_000);
        var signedData = $"amount=99000&cancelUrl={cancelUrl}&description=N12345678&orderCode=12345678&returnUrl={returnUrl}";
        root.GetProperty("signature").GetString().Should().Be(Sign(signedData));
    }

    [Fact]
    public void Webhook_signature_accepts_original_data_and_rejects_tampering()
    {
        var service = CreateService(new CapturingHandler("{}"));
        using var original = JsonDocument.Parse("""{"orderCode":123,"amount":99000,"code":"00","paymentLinkId":"link-123","reference":"TX-1"}""");
        var signature = Sign("amount=99000&code=00&orderCode=123&paymentLinkId=link-123&reference=TX-1");

        service.IsValidWebhook(original.RootElement, signature).Should().BeTrue();

        using var tampered = JsonDocument.Parse("""{"orderCode":123,"amount":99001,"code":"00","paymentLinkId":"link-123","reference":"TX-1"}""");
        service.IsValidWebhook(tampered.RootElement, signature).Should().BeFalse();
    }

    [Fact]
    public async Task Get_payment_status_reads_paid_amount_from_payos()
    {
        var handler = new CapturingHandler("""
            {"code":"00","data":{"id":"link-123","orderCode":12345678,"amount":99000,"amountPaid":99000,"status":"PAID"}}
            """);
        var service = CreateService(handler);

        var result = await service.GetPaymentStatus(12345678, CancellationToken.None);

        handler.Method.Should().Be(HttpMethod.Get);
        handler.RequestUri.Should().Be("https://api-merchant.payos.vn/v2/payment-requests/12345678");
        result.IsPaid.Should().BeTrue();
        result.AmountPaid.Should().Be(99_000m);
    }

    [Fact]
    public async Task Cancel_payment_link_sends_reason_and_reads_cancelled_status()
    {
        var handler = new CapturingHandler("""
            {"code":"00","data":{"id":"link-123","orderCode":12345678,"amount":99000,"amountPaid":0,"status":"CANCELLED"}}
            """);
        var service = CreateService(handler);

        var result = await service.CancelPaymentLink(12345678, "Hết hạn", CancellationToken.None);

        handler.Method.Should().Be(HttpMethod.Post);
        handler.RequestUri.Should().Be("https://api-merchant.payos.vn/v2/payment-requests/12345678/cancel");
        using var body = JsonDocument.Parse(handler.Body!);
        body.RootElement.GetProperty("cancellationReason").GetString().Should().Be("Hết hạn");
        result.IsCancelled.Should().BeTrue();
    }

    private static PayOsService CreateService(HttpMessageHandler handler)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["PayOS:ClientId"] = "test-client-id",
            ["PayOS:ApiKey"] = "test-api-key",
            ["PayOS:ChecksumKey"] = ChecksumKey,
            ["Frontend:BaseUrl"] = "http://localhost:5173"
        }).Build();
        return new PayOsService(new HttpClient(handler), configuration);
    }

    private static string Sign(string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(ChecksumKey));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }

    private sealed class CapturingHandler(string responseBody) : HttpMessageHandler
    {
        public string? RequestUri { get; private set; }
        public string? ClientId { get; private set; }
        public string? ApiKey { get; private set; }
        public string? Body { get; private set; }
        public HttpMethod? Method { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri?.ToString();
            Method = request.Method;
            ClientId = request.Headers.GetValues("x-client-id").Single();
            ApiKey = request.Headers.GetValues("x-api-key").Single();
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(responseBody, Encoding.UTF8, "application/json") };
        }
    }
}
