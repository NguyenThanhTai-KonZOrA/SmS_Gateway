using Microsoft.AspNetCore.Mvc;
using SmsGateway.Common.Enum;
using SmsGateway.Common.Helper;
using SmsGateway.Implement.Services.Interfaces;
using SmsGateway.Implement.SmsFactory.FactoryInterface;
using SmsGateway.Implement.ViewModels.Request;
using SmsGateway.Implement.ViewModels.Response;

namespace SmsGateway.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private readonly ILogger<SmsController> _logger;
        private readonly ISmsFactory _smsFactory;
        private readonly ISmsRateLimiterService _rateLimiterService;
        private readonly ISmsLoggingService _loggingService;
        private readonly IOtpService _otpService;

        public SmsController(
            ILogger<SmsController> logger,
            ISmsFactory smsFactory,
            ISmsRateLimiterService rateLimiterService,
            ISmsLoggingService loggingService,
            IOtpService otpService)
        {
            _logger = logger;
            _smsFactory = smsFactory;
            _rateLimiterService = rateLimiterService;
            _loggingService = loggingService;
            _otpService = otpService;
        }

        [HttpPost("send-point")]
        public async Task<IActionResult> SendPointSmsÁync([FromBody] SmsPointRequest request)
        {
            string? correlationId = StringHelper.GenerateTimeBasedCorrelationId();

            try
            {
                _logger.LogInformation("Received SMS send request: {@Request}", request);
                if (request == null)
                {
                    _logger.LogWarning("SMS send request is null");
                    return BadRequest(new { Message = "Invalid SMS request" });
                }

                if (!request.Type.HasValue)
                {
                    _logger.LogWarning("SMS provider type is not specified");
                }

                // Normalize phone number to "84..."
                request.PhoneNumber = PhoneNumberHelper.NormalizeTo84(request.PhoneNumber);

                // Enforce rate limit before sending
                await _rateLimiterService.EnsureAllowedAsync(request.PhoneNumber);

                var smsProvider = _smsFactory.CreateSmsProvider(request.Type ?? SmsTypeServiceEnum.VNPT);
                var response = await smsProvider.SendPointSmsAsync(request);
                await _loggingService.LogAsync(new SmsLogRequest
                {
                    ContractName = request.ContractName,
                    ContractPhoneNumber = request.ContractPhoneNumber,
                    FullName = request.FullName,
                    Message = request.Message,
                    PhoneNumber = request.PhoneNumber,
                    PlayerId = request.PlayerId,
                    PlayerPoint = request.PlayerPoint,
                    ScheduleTime = request.ScheduleTime,
                    Type = request.Type
                }, response, (request.Type ?? SmsTypeServiceEnum.VNPT).ToString(), response?.IsSuccess ?? false, correlationId);
                await _rateLimiterService.RecordAsync(request.PhoneNumber, response?.IsSuccess ?? false);

                _logger.LogInformation("SMS send response: {@Response}", response);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending SMS by {phoneNumber}", request?.PhoneNumber);
                throw new BadHttpRequestException(ex.Message);
            }
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendSmsOtpAsync([FromBody] SmsOtpRequest request)
        {
            string? correlationId = StringHelper.GenerateTimeBasedCorrelationId();

            try
            {
                _logger.LogInformation("Received SMS send request: {@Request}", request);
                if (request == null)
                {
                    _logger.LogWarning("SMS send request is null");
                    return BadRequest(new { Message = "Invalid SMS request" });
                }

                if (!request.Type.HasValue)
                {
                    _logger.LogWarning("SMS provider type is not specified");
                }

                request.PhoneNumber = PhoneNumberHelper.NormalizeTo84(request.PhoneNumber);

                // Enforce rate limit before sending
                await _rateLimiterService.EnsureAllowedAsync(request.PhoneNumber);
                var smsProvider = _smsFactory.CreateSmsProvider(request.Type ?? SmsTypeServiceEnum.VNPT);
                var otpCode = await _otpService.GenerateAsync(request.PhoneNumber);

                var response = await smsProvider.SendOtpSmsAsync(new SmsOtpRequest
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Message = request.Message,
                    OtpCode = otpCode,
                    Type = request.Type,
                    CorrelationId = correlationId
                });

                await _loggingService.LogAsync(new SmsLogRequest
                {
                    ContractName = "",
                    ContractPhoneNumber = "",
                    FullName = request.FullName,
                    Message = request.Message,
                    PhoneNumber = request.PhoneNumber,
                    PlayerId = "",
                    PlayerPoint = "",
                    ScheduleTime = DateTime.UtcNow,
                    Type = request.Type
                }, response, (request.Type ?? SmsTypeServiceEnum.VNPT).ToString(), response?.IsSuccess ?? false, correlationId);
                await _rateLimiterService.RecordAsync(request.PhoneNumber, response?.IsSuccess ?? false);

                _logger.LogInformation("SMS send response: {@Response}", response);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending SMS by {phoneNumber}", request?.PhoneNumber);
                var error = new SmsResponse
                {
                    IsSuccess = false,
                    ErrorCode = "1",
                    ErrorMessage = ex.Message,
                    ProviderName = (request?.Type ?? SmsTypeServiceEnum.VNPT).ToString()
                };
                return BadRequest(error);
            }
        }

        [HttpPost("otp-generate")]
        public async Task<IActionResult> GenerateOtp([FromBody] string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return BadRequest(new { Message = "Phone number is required" });
            }

            var normalized = PhoneNumberHelper.NormalizeTo84(phoneNumber);
            var code = await _otpService.GenerateAsync(normalized);
            return Ok(new { PhoneNumber = normalized, Code = code, Message = "OTP generated" });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifySmsOtpRequest smsOtpRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(smsOtpRequest.PhoneNumber) || string.IsNullOrWhiteSpace(smsOtpRequest.OtpCode))
                {
                    return BadRequest(new { Message = "Phone number and code are required" });
                }

                smsOtpRequest.PhoneNumber = PhoneNumberHelper.NormalizeTo84(smsOtpRequest.PhoneNumber);

                var result = await _otpService.VerifyAsync(smsOtpRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verify SMS by {phoneNumber}", smsOtpRequest?.PhoneNumber);
                throw new BadHttpRequestException(ex.Message);
            }
        }
    }
}