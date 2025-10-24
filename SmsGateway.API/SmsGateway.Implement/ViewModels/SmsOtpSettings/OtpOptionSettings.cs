namespace SmsGateway.Implement.ViewModels.SmsOtpSettings
{
    public sealed class OtpOptionSettings
    {
        public int CodeLength { get; set; } = 6;
        public int TtlMinutes { get; set; } = 5;
        public int MaxGenerationAttempts { get; set; } = 10;
    }
}