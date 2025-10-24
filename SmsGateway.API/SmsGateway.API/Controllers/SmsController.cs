using Microsoft.AspNetCore.Mvc;
using SmsGateway.Common.Enum;
using SmsGateway.Implement.Services.Interfaces;
using SmsGateway.Implement.SmsFactory.FactoryInterface;
using SmsGateway.Implement.ViewModels.Request;

namespace SmsGateway.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private readonly ILogger<SmsController> _logger;
        private readonly ISmsFactory _smsFactory;
        private readonly ISmsRateLimiterService _rateLimiter;
        private readonly ISmsLoggingService _loggingService;
        private readonly IOtpService _otpService;

        public SmsController(
            ILogger<SmsController> logger,
            ISmsFactory smsFactory,
            ISmsRateLimiterService rateLimiter,
            ISmsLoggingService loggingService,
            IOtpService otpService)
        {
            _logger = logger;
            _smsFactory = smsFactory;
            _rateLimiter = rateLimiter;
            _loggingService = loggingService;
            _otpService = otpService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendSms([FromBody] SmsRequest request)
        {
            string? correlationId = HttpContext.TraceIdentifier;

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

                // Enforce rate limit before sending
                await _rateLimiter.EnsureAllowedAsync(request.PhoneNumber);

                var smsProvider = _smsFactory.CreateSmsProvider(request.Type ?? SmsTypeServiceEnum.VNPT);
#if DEBUG
                var otpCode = await _otpService.GenerateAsync(request.PhoneNumber);
                var response = new Implement.ViewModels.Response.SmsResponse
                {
                    IsSuccess = true,
                    ErrorCode = "0",
                    ProviderName = "VNPT",
                    ErrorMessage = "success",
                    OtpCode = otpCode,
                    ProviderMessageId = ""
                };
#elif RELEASE
                var response = await smsProvider.SendSmsAsync(request);

#endif

                await _loggingService.LogAsync(request, response, (request.Type ?? SmsTypeServiceEnum.VNPT).ToString(), response?.IsSuccess ?? false, correlationId);
                await _rateLimiter.RecordAsync(request.PhoneNumber, response?.IsSuccess ?? false);

                _logger.LogInformation("SMS send response: {@Response}", response);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending SMS by {phoneNumber}", request?.PhoneNumber);
                throw new BadHttpRequestException(ex.Message);
            }
        }

        [HttpPost("otp/generate")]
        public async Task<IActionResult> GenerateOtp([FromBody] string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return BadRequest(new { Message = "Phone number is required" });
            }

            var code = await _otpService.GenerateAsync(phoneNumber);
            // You can now send this code via your existing SendSms endpoint/provider.
            return Ok(new { PhoneNumber = phoneNumber, Code = code, Message = "OTP generated" });
        }

        [HttpPost("otp/verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifySmsOtpRequest smsOtpRequest )
        {
            if (string.IsNullOrWhiteSpace(smsOtpRequest.PhoneNumber) || string.IsNullOrWhiteSpace(smsOtpRequest.OtpCode))
            {
                return BadRequest(new { Message = "Phone number and code are required" });
            }

            var ok = await _otpService.VerifyAsync(smsOtpRequest);
            return Ok(new { PhoneNumber = smsOtpRequest.PhoneNumber, Verified = ok });
        }
    }
}