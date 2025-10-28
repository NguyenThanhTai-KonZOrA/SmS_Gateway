using SmsGateway.Implement.SmsFactory.FactoryInterface;
using SmsGateway.Implement.ViewModels.Request;
using SmsGateway.Implement.ViewModels.Response;

namespace SmsGateway.Implement.SmsFactory.FactoryService
{
    public class MobifoneSmsProvider : ISmsProvider
    {
        public MobifoneSmsProvider() { }

        public Task<SmsResponse> SendOtpSmsAsync(SmsOtpRequest smsRequest)
        {
            throw new NotImplementedException();
        }

        public Task<SmsResponse> SendPointSmsAsync(SmsPointRequest smsRequest)
        {
            throw new NotImplementedException();
        }
    }
}
