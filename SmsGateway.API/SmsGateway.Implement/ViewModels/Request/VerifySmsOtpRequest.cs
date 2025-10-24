namespace SmsGateway.Implement.ViewModels.Request
{
    public class VerifySmsOtpRequest
    {
        public string PhoneNumber { get; set; }
        public string OtpCode { get; set; }
    }
}