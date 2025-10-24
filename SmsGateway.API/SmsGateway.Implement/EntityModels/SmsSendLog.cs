using SmsGateway.Common.BaseEntity;

namespace SmsGateway.Implement.EntityModels
{
    public class SmsSendLog : BaseEntity
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public string? PlayerId { get; set; }
        public string? FullName { get; set; }
        public string? Message { get; set; }
        public string Provider { get; set; } = default!;
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? CorrelationId { get; set; }
    }
}
