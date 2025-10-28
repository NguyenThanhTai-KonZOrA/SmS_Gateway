using SmsGateway.Implement.SmsFactory.SmsModels.SmsResponse;
using SmsGateway.Implement.ViewModels.Request;

namespace SmsGateway.Implement.Interface
{
    public interface IVnptSmsService
    {
        Task<VnptSmsResponse> SendPointSmsAsnyc(SmsPointRequest smsRequest);
        Task<VnptSmsResponse> SendSmsOtpAsnyc(SmsOtpRequest smsOtpRequest);
    }
}