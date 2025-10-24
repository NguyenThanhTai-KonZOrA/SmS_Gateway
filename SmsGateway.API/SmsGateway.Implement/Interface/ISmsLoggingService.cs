using SmsGateway.Implement.ViewModels.Request;
using SmsGateway.Implement.ViewModels.Response;

namespace SmsGateway.Implement.Services.Interfaces
{
    public interface ISmsLoggingService
    {
        Task LogAsync(SmsRequest request, SmsResponse response, string provider, bool success, string? correlationId, CancellationToken ct = default);
    }
}