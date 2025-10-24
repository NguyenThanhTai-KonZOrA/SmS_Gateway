using SmsGateway.Common.BaseEntity;

namespace SmsGateway.Implement.EntityModels
{
    public class SmsDailyStatistic : BaseEntity
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public DateOnly Day { get; set; }
        public int AttemptCount { get; set; }
        public int SuccessCount { get; set; }
        public DateTime? LockedUntilUtc { get; set; }

        // For optimistic concurrency on updates under load
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
