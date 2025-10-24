using SmsGateway.Common.BaseEntity;

namespace SmsGateway.Implement.EntityModels
{
    public class OtpCode : BaseEntity
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public string Code { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAtUtc { get; set; }
    }
}
