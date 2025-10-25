using Newtonsoft.Json;
using SmsGateway.Common.ApiClient;
using SmsGateway.Common.Constants;
using SmsGateway.Implement.Interface;
using SmsGateway.Implement.SmsFactory.SmsConfigs.VnptSettings;
using SmsGateway.Implement.SmsFactory.SmsModels.SmsRequest;
using SmsGateway.Implement.SmsFactory.SmsModels.SmsResponse;
using SmsGateway.Implement.ViewModels.Request;

namespace SmsGateway.Implement.Service
{
    public class VnptSmsService : IVnptSmsService
    {
        private readonly IApiClient _apiClient;
        private readonly VnptSmsSettings _vnptSmsSettings;
        public VnptSmsService(
               IApiClient apiClient,
               VnptSmsSettings vnptSmsSettings
            )
        {
            _apiClient = apiClient;
            _vnptSmsSettings = vnptSmsSettings;
        }

        public async Task<VnptSmsResponse> SendSmsAsnyc(SmsPointRequest smsRequest)
        {
            //var response = JsonConvert.DeserializeObject<VnptSmsResponse>("{\"RPLY\":{\"ERROR_DESC\":\"success\",\"name\":\"send_sms_list\",\"ERROR\":\"31\"}}");
            //return response;
            var stringContentSms = new VnptSmsRequest
            {
                RQST = new RQST
                {
                    name = _vnptSmsSettings.Name,
                    REQID = "1396000023",
                    LABELID = _vnptSmsSettings.LabelID,
                    CONTRACTTYPEID = _vnptSmsSettings.ContractTypeId,
                    CONTRACTID = _vnptSmsSettings.ContractId,
                    TEMPLATEID = _vnptSmsSettings.TemplateId,
                    PARAMS =
                    [
                        new() { NUM = "1", CONTENT = $"{smsRequest.PlayerPoint} in Slot Free Play / MPV"},
                        new() { NUM = "2", CONTENT = smsRequest.FullName},
                        new() { NUM = "3", CONTENT = smsRequest.PlayerId},
                        new() { NUM = "4", CONTENT = $"{smsRequest.PlayerPoint} worth of slot free play credits and MPV"},
                        new() { NUM = "5", CONTENT = DateTime.Now.ToString(CommonConstants.DateFormat)},
                        new() { NUM = "6", CONTENT = smsRequest.ContractName},
                        new() { NUM = "7", CONTENT = smsRequest.ContractPhoneNumber}
                    ],
                    SCHEDULETIME = "",
                    MOBILELIST = smsRequest.PhoneNumber,
                    ISTELCOSUB = _vnptSmsSettings.IsTelcosub,
                    AGENTID = _vnptSmsSettings.AgentId,
                    APIUSER = _vnptSmsSettings.ApiUser,
                    APIPASS = _vnptSmsSettings.ApiPass,
                    USERNAME = _vnptSmsSettings.UserName,
                    DATACODING = _vnptSmsSettings.DataCoding
                }
            };

            var response = await _apiClient.PostAsync<VnptSmsRequest, VnptSmsResponse>(_vnptSmsSettings.BaseUrl, stringContentSms);

            return response ?? new VnptSmsResponse();
        }
    }
}
