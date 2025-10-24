namespace SmsGateway.Implement.Services.Interfaces
{
    public interface ISmsRateLimiterService
    {
        Task EnsureAllowedAsync(string phoneNumber, CancellationToken ct = default);
        Task RecordAsync(string phoneNumber, bool success, CancellationToken ct = default);
    }
}