namespace SmsGateway.Implement.SmsFactory.SmsModels.SmsRequest
{
    public class VnptSmsRequest
    {
        public RQST RQST { get; set; }
    }

    public class RQST
    {
        public string name { get; set; }
        public string REQID { get; set; }
        public string LABELID { get; set; }
        public string CONTRACTTYPEID { get; set; }
        public string CONTRACTID { get; set; }
        public string TEMPLATEID { get; set; }
        public List<PARAMS> PARAMS { get; set; } = [];
        public string SCHEDULETIME { get; set; }
        public string MOBILELIST { get; set; }
        public string ISTELCOSUB { get; set; }
        public string AGENTID { get; set; }
        public string APIUSER { get; set; }
        public string APIPASS { get; set; }
        public string USERNAME { get; set; }
        public string DATACODING { get; set; }
    }

    public class PARAMS
    {
        public string NUM { get; set; }
        public string CONTENT { get; set; }
    }
}
