using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Nailify.Api.Services;

public sealed class PayOsService(HttpClient httpClient, IConfiguration configuration)
{
    private const string ApiBaseUrl = "https://api-merchant.payos.vn";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(configuration["PayOS:ClientId"])
        && !string.IsNullOrWhiteSpace(configuration["PayOS:ApiKey"])
        && !string.IsNullOrWhiteSpace(configuration["PayOS:ChecksumKey"]);

    public async Task<PayOsPaymentLink> CreatePaymentLink(long orderCode, Guid appointmentId, decimal amount, DateTime expiresAt, CancellationToken ct)
    {
        EnsureConfigured();
        var amountVnd = decimal.ToInt32(decimal.Round(amount, 0, MidpointRounding.AwayFromZero));
        var frontendUrl = configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? throw new InvalidOperationException("Frontend URL is not configured.");
        var returnUrl = $"{frontendUrl}/deposit/{appointmentId}?payment=success";
        var cancelUrl = $"{frontendUrl}/deposit/{appointmentId}?payment=cancelled";
        var description = $"N{orderCode}";
        var signedData = $"amount={amountVnd}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
        var request = new PayOsCreateRequest(orderCode, amountVnd, description, cancelUrl, returnUrl,
            (int)new DateTimeOffset(expiresAt).ToUnixTimeSeconds(), Sign(signedData));
        using var message = new HttpRequestMessage(HttpMethod.Post, $"{ApiBaseUrl}/v2/payment-requests")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("x-client-id", configuration["PayOS:ClientId"]);
        message.Headers.Add("x-api-key", configuration["PayOS:ApiKey"]);
        using var response = await httpClient.SendAsync(message, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("payOS could not create the payment link.");
        var result = JsonSerializer.Deserialize<PayOsResponse>(body, JsonOptions) ?? throw new InvalidOperationException("payOS returned an invalid response.");
        if (result.Code != "00" || result.Data is null || string.IsNullOrWhiteSpace(result.Data.CheckoutUrl)) throw new InvalidOperationException("payOS rejected the payment request.");
        return new PayOsPaymentLink(result.Data.PaymentLinkId, result.Data.CheckoutUrl);
    }

    public async Task<PayOsPaymentStatus> GetPaymentStatus(long orderCode, CancellationToken ct)
    {
        EnsureConfigured();
        using var response = await SendAuthorizedAsync(HttpMethod.Get, $"{ApiBaseUrl}/v2/payment-requests/{orderCode}", null, ct);
        var result = await ReadStatusResponse(response, ct);
        return new PayOsPaymentStatus(result.OrderCode, result.Amount, result.AmountPaid, result.Status, result.Id);
    }

    public async Task<PayOsPaymentStatus> CancelPaymentLink(long orderCode, string reason, CancellationToken ct)
    {
        EnsureConfigured();
        using var response = await SendAuthorizedAsync(HttpMethod.Post, $"{ApiBaseUrl}/v2/payment-requests/{orderCode}/cancel",
            JsonContent.Create(new { cancellationReason = reason }), ct);
        var result = await ReadStatusResponse(response, ct);
        return new PayOsPaymentStatus(result.OrderCode, result.Amount, result.AmountPaid, result.Status, result.Id);
    }

    public bool IsValidWebhook(JsonElement data, string signature)
    {
        if (!IsConfigured || data.ValueKind != JsonValueKind.Object || string.IsNullOrWhiteSpace(signature)) return false;
        var canonical = string.Join("&", data.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal)
            .Select(x => $"{x.Name}={JsonValue(x.Value)}"));
        var expected = Sign(canonical);
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
    }

    private string Sign(string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(configuration["PayOS:ChecksumKey"]!));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured) throw new InvalidOperationException("PayOS is not configured. Add PayOS:ClientId, PayOS:ApiKey and PayOS:ChecksumKey on the server.");
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(HttpMethod method, string url, HttpContent? content, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, url) { Content = content };
        request.Headers.Add("x-client-id", configuration["PayOS:ClientId"]);
        request.Headers.Add("x-api-key", configuration["PayOS:ApiKey"]);
        return await httpClient.SendAsync(request, ct);
    }

    private static async Task<PayOsStatusData> ReadStatusResponse(HttpResponseMessage response, CancellationToken ct)
    {
        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException("payOS không thể xử lý yêu cầu thanh toán.");
            var result = JsonSerializer.Deserialize<PayOsStatusResponse>(body, JsonOptions)
                ?? throw new InvalidOperationException("payOS trả về dữ liệu không hợp lệ.");
            if (result.Code != "00" || result.Data is null)
                throw new InvalidOperationException("payOS từ chối yêu cầu thanh toán.");
            return result.Data;
        }
    }

    private static string JsonValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null => "",
        JsonValueKind.String => value.GetString() ?? "",
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        _ => value.GetRawText()
    };

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private sealed record PayOsCreateRequest(long OrderCode, int Amount, string Description, string CancelUrl, string ReturnUrl, int ExpiredAt, string Signature);
    private sealed record PayOsResponse(string Code, PayOsResponseData? Data);
    private sealed record PayOsResponseData(string PaymentLinkId, string CheckoutUrl);
    private sealed record PayOsStatusResponse(string Code, PayOsStatusData? Data);
    private sealed record PayOsStatusData(string Id, long OrderCode, decimal Amount, decimal AmountPaid, string Status);
}

public sealed record PayOsPaymentLink(string PaymentLinkId, string CheckoutUrl);
public sealed record PayOsPaymentStatus(long OrderCode, decimal Amount, decimal AmountPaid, string Status, string PaymentLinkId)
{
    public bool IsPaid => string.Equals(Status, "PAID", StringComparison.OrdinalIgnoreCase);
    public bool IsCancelled => string.Equals(Status, "CANCELLED", StringComparison.OrdinalIgnoreCase);
}
