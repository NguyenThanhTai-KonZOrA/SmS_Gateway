namespace SmsGateway.Implement.ViewModels.Response
{
    public class SmsResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ProviderMessageId { get; set; }
        public string? ProviderName { get; set; }
        public string? OtpCode { get; set; }
    }
}