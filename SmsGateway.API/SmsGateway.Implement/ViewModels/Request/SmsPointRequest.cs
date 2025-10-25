using SmsGateway.Common.Enum;

namespace SmsGateway.Implement.ViewModels.Request
{
    public class SmsPointRequest
    {
        public string PlayerId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string PlayerPoint { get; set; }
        public string ContractName { get; set; }
        public string ContractPhoneNumber { get; set; }
        public string Message { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public SmsTypeServiceEnum? Type { get; set; }
    }
}