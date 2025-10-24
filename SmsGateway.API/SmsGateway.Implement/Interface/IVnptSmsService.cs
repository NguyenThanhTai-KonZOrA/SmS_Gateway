using SmsGateway.Implement.SmsFactory.SmsModels.SmsResponse;
using SmsGateway.Implement.ViewModels.Request;

namespace SmsGateway.Implement.Interface
{
    public interface IVnptSmsService
    {
        Task<VnptSmsResponse> SendSmsAsnyc(SmsRequest smsRequest);
    }
}