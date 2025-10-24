using SmsGateway.Common.Constants;
using SmsGateway.Common.Enum;
using SmsGateway.Common.Helper;
using SmsGateway.Implement.Interface;
using SmsGateway.Implement.SmsFactory.FactoryInterface;
using SmsGateway.Implement.ViewModels.Request;
using SmsGateway.Implement.ViewModels.Response;

namespace SmsGateway.Implement.SmsFactory.FactoryService
{
    public class VnptSmsProvider : ISmsProvider
    {
        private readonly IVnptSmsService _vnptSmsService;
        public VnptSmsProvider(
           IVnptSmsService vnptSmsService
            )
        {
            _vnptSmsService = vnptSmsService;
        }

        public async Task<SmsResponse> SendSmsAsync(SmsRequest smsRequest)
        {
            var response = await _vnptSmsService.SendSmsAsnyc(smsRequest);

            if (response?.RPLY != null && response.RPLY.ERROR_DESC == CommonConstants.Success && response.RPLY.ERROR == CommonConstants.CodeSuccess)
            {
                return new SmsResponse
                {
                    IsSuccess = true,
                    ErrorCode = response.RPLY.ERROR_DESC,
                    ProviderName = SmsTypeServiceEnum.VNPT.ToString(),
                    ErrorMessage = ErrorrMessageMapper.GetMessage(VnptErrorMessages(), response?.RPLY.ERROR ?? CommonConstants.UnknownErrorCode),
                };
            }
            else
            {
                return new SmsResponse
                {
                    IsSuccess = false,
                    ErrorCode = response?.RPLY.ERROR_DESC ?? CommonConstants.UnknownError,
                    ProviderName = SmsTypeServiceEnum.VNPT.ToString(),
                    ErrorMessage = ErrorrMessageMapper.GetMessage(VnptErrorMessages(), response?.RPLY.ERROR ?? CommonConstants.UnknownErrorCode)
                };
            }
        }

        public async Task<VerifySmsOtpResponse> VerifyOtpAsync(string phoneNumber, string otp)
        {
            throw new NotImplementedException();
        }

        private Dictionary<string, string> VnptErrorMessages() => new()
        {
                { "-1", "Exception: The request contains XML special characters, exceeds allowed length, or an internal error occurred." },
                { "0", "Success" },
                { "1", "Invalid username, password, IP, or API status. Verify API credentials and IP restrictions." },
                { "2", "Invalid schedule time format." },
                { "3", "Invalid method ID." },
                { "7", "Invalid or non-existent template for the label/agent. Check template ID on the portal." },
                { "8", "Invalid sending time for marketing messages. Allowed time slots: 08:00–11:30, 13:00–18:30, 20:00–21:00." },
                { "9", "Invalid contract_type_id. Valid values: 1 = Customer Care (CSKH), 2 = Marketing (QC)." },
                { "10", "Invalid user_name. The Agent’s login account on the portal is incorrect." },
                { "11", "Invalid message length." },
                { "12", "Invalid sending time according to Vinaphone policy." },
                { "13", "Invalid contract." },
                { "14", "Invalid label." },
                { "15", "Invalid agent." },
                { "16", "Exceeded allowed sending rate." },
                { "17", "Invalid character format in message." },
                { "20", "Contract message quota exhausted. Agents may extend if quota remains." },
                { "21", "Customer message quota exhausted. Agents may extend if quota remains." },
                { "22", "Agent message quota exhausted. Contact VNPT admin for more quota." },
                { "23", "Invalid phone number or multiple networks in a single request. Only one network allowed per request." },
                { "24", "Scheduled time is earlier than current system time." },
                { "25", "Invalid telco or label (network mismatch or invalid label)." },
                { "26", "Subscriber has already received 3 marketing SMS today." },
                { "27", "Invalid parameter value at the specified position." },
                { "28", "Parameter length at specified position exceeds declared limit." },
                { "29", "Brandname expired or no longer valid." },
                { "30", "Message contains illegal or restricted keyword." },
                { "31", "Invalid advertiser status (adser status not valid)." },
                { "33", "Duplicate request ID." }
            };
    }
}
