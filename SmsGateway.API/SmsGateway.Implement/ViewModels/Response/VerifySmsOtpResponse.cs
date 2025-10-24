namespace SmsGateway.Implement.ViewModels.Response
{
    public class VerifySmsOtpResponse
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public string Messages { get; set; }
    }
}
