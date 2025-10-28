using SmsGateway.Implement.ViewModels.Request;
using SmsGateway.Implement.ViewModels.Response;

namespace SmsGateway.Implement.SmsFactory.FactoryInterface
{
    public interface ISmsProvider
    {
        Task<SmsResponse> SendPointSmsAsync(SmsPointRequest smsRequest);
        Task<SmsResponse> SendOtpSmsAsync(SmsOtpRequest smsRequest);
    }
}
