using Microsoft.Extensions.Logging;
using SmsGateway.Common.ApiClient;
using SmsGateway.Common.Constants;
using SmsGateway.Implement.Interface;
using SmsGateway.Implement.Services.Interfaces;
using SmsGateway.Implement.SmsFactory.SmsConfigs.VnptSettings;
using SmsGateway.Implement.SmsFactory.SmsModels.SmsRequest;
using SmsGateway.Implement.SmsFactory.SmsModels.SmsResponse;
using SmsGateway.Implement.ViewModels.Request;

namespace SmsGateway.Implement.Service
{
    public class VnptSmsService : IVnptSmsService
    {
        private readonly ILogger<VnptSmsService> _logger;
        private readonly IApiClient _apiClient;
        private readonly VnptSmsSettings _vnptSmsSettings;
        public VnptSmsService(
            ILogger<VnptSmsService> logger,
               IApiClient apiClient,
               VnptSmsSettings vnptSmsSettings
            )
        {
            _logger = logger;
            _apiClient = apiClient;
            _vnptSmsSettings = vnptSmsSettings;
        }

        public async Task<VnptSmsResponse> SendPointSmsAsnyc(SmsPointRequest smsRequest)
        {
            //var response = JsonConvert.DeserializeObject<VnptSmsResponse>("{\"RPLY\":{\"ERROR_DESC\":\"success\",\"name\":\"send_sms_list\",\"ERROR\":\"31\"}}");
            //return response;
            var stringContentSms = new VnptSmsRequest
            {
                RQST = new RQST
                {
                    name = _vnptSmsSettings.Name,
                    REQID = "1396000023",
                    LABELID = _vnptSmsSettings.LabelId,
                    CONTRACTTYPEID = _vnptSmsSettings.ContractTypeId,
                    CONTRACTID = _vnptSmsSettings.ContractId,
                    TEMPLATEID = _vnptSmsSettings.TemplateSmsPointId,
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

        public async Task<VnptSmsResponse> SendSmsOtpAsnyc(SmsOtpRequest smsOtpRequest)
        {
            //var response = JsonConvert.DeserializeObject<VnptSmsResponse>("{\"RPLY\":{\"ERROR_DESC\":\"success\",\"name\":\"send_sms_list\",\"ERROR\":\"31\"}}");
            //return response;
            if (string.IsNullOrEmpty(smsOtpRequest.OtpCode) && string.IsNullOrEmpty(smsOtpRequest.PhoneNumber))
            {
                return new VnptSmsResponse
                {
                    RPLY = new RPLY
                    {
                        ERROR_DESC = "Invalid input: OtpCode and PhoneNumber are required.",
                        name = "send_sms_list",
                        ERROR = "-1",
                    }
                };
            }

            var stringContentSms = new VnptSmsRequest
            {
                RQST = new RQST
                {
                    name = _vnptSmsSettings.Name,
                    REQID = smsOtpRequest.CorrelationId,
                    LABELID = _vnptSmsSettings.LabelId,
                    CONTRACTTYPEID = _vnptSmsSettings.ContractTypeId,
                    CONTRACTID = _vnptSmsSettings.ContractId,
                    TEMPLATEID = _vnptSmsSettings.TemplateSmsOtpId,
                    PARAMS =
                    [
                        new() { NUM = "1", CONTENT = smsOtpRequest.OtpCode },
                    ],
                    SCHEDULETIME = "",
                    MOBILELIST = smsOtpRequest.PhoneNumber,
                    ISTELCOSUB = _vnptSmsSettings.IsTelcosub,
                    AGENTID = _vnptSmsSettings.AgentId,
                    APIUSER = _vnptSmsSettings.ApiUser,
                    APIPASS = _vnptSmsSettings.ApiPass,
                    USERNAME = _vnptSmsSettings.UserName,
                    DATACODING = "0"
                }
            };
            _logger.LogInformation("VNPT SMS OTP Request: {@Request}", stringContentSms);

            var response = await _apiClient.PostAsync<VnptSmsRequest, VnptSmsResponse>(_vnptSmsSettings.BaseUrl, stringContentSms);
            _logger.LogInformation("End SMS OTP Request");
            if (response != null)
            {
                _logger.LogInformation("Send SMS success Phone: {PhoneNumber}, Code: {OtpCode}", smsOtpRequest.PhoneNumber, smsOtpRequest.OtpCode);
                return response;
            }
            else
            {
                _logger.LogError("Send SMS Faild Phone: {PhoneNumber}, Code: {OtpCode}", smsOtpRequest.PhoneNumber, smsOtpRequest.OtpCode);
                _logger.LogInformation("Reesponse SMS OTP : {@response}", response);
                return new VnptSmsResponse
                {
                    RPLY = new RPLY
                    {
                        ERROR_DESC = CommonConstants.UnknownError,
                        name = "send_sms_list",
                        ERROR = CommonConstants.UnknownErrorCode,
                    }
                };
            }
        }
    }
}
