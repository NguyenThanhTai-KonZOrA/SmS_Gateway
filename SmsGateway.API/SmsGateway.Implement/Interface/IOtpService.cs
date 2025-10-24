using SmsGateway.Implement.ViewModels.Request;
using SmsGateway.Implement.ViewModels.Response;

namespace SmsGateway.Implement.Services.Interfaces
{
    public interface IOtpService
    {
        Task<string> GenerateAsync(string phoneNumber, CancellationToken ct = default);
        Task<VerifySmsOtpResponse> VerifyAsync(VerifySmsOtpRequest smsOtpRequest, CancellationToken ct = default);
    }
}