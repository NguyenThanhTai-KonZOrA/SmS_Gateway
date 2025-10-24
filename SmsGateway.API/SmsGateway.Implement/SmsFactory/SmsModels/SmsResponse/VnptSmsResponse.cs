namespace SmsGateway.Implement.SmsFactory.SmsModels.SmsResponse
{
    public class VnptSmsResponse
    {
        public RPLY RPLY { get; set; }
    }

    public class RPLY
    {
        public string ERROR_DESC { get; set; }
        public string name { get; set; }
        public string ERROR { get; set; }
    }
}
