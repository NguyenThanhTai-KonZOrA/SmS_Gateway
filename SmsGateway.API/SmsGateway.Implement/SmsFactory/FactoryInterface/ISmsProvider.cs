using SmsGateway.Implement.ViewModels.Request;
using SmsGateway.Implement.ViewModels.Response;

namespace SmsGateway.Implement.SmsFactory.FactoryInterface
{
    public interface ISmsProvider
    {
        Task<SmsResponse> SendSmsAsync(SmsPointRequest smsRequest);
    }
}
