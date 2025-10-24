using SmsGateway.Implement.SmsFactory.FactoryInterface;
using SmsGateway.Implement.ViewModels.Request;
using SmsGateway.Implement.ViewModels.Response;

namespace SmsGateway.Implement.SmsFactory.FactoryService
{
    public class TwilioSmsProvider : ISmsProvider
    {
        public TwilioSmsProvider() { }
        public Task<SmsResponse> SendSmsAsync(SmsRequest smsRequest)
        {
            throw new NotImplementedException();
        }
    }
}
