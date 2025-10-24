namespace SmsGateway.Implement.ViewModels.SmsOtpSettings
{
    public sealed class SmsRateLimitOptionSettings
    {
        public int MaxPerDay { get; set; } = 5;
        public int LockDurationMinutes { get; set; } = 60;
    }
}