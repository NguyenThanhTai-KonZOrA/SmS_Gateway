using SmsGateway.Common.Enum;

namespace SmsGateway.Implement.ViewModels.Request
{
    public class SmsOtpRequest
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public string OtpCode { get; set; }
        public SmsTypeServiceEnum? Type { get; set; }
        public string CorrelationId { get; set; }
    }
}
