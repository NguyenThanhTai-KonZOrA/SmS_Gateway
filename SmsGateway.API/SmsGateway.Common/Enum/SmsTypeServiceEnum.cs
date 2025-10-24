using System.ComponentModel;

namespace SmsGateway.Common.Enum
{
    public enum SmsTypeServiceEnum
    {
        [Description("VNPT")]
        VNPT,
        [Description("VIETTEL")]
        VIETTEL,
        [Description("MOBIFONE")]
        MOBIFONE,
        [Description("TWILIO")]
        TWILIO,
    }
}