using SmsGateway.Common.Helper;
using SmsGateway.Implement.EntityModels;
using SmsGateway.Implement.Repositories.Interface;
using SmsGateway.Implement.Services.Interfaces;
using SmsGateway.Implement.UnitOfWork;
using SmsGateway.Implement.ViewModels.Request;
using SmsGateway.Implement.ViewModels.Response;
using SmsGateway.Implement.ViewModels.SmsOtpSettings;

namespace SmsGateway.Implement.Services
{
    public class OtpService : IOtpService
    {
        private readonly OtpOptionSettings _otpOptionSettings;
        private readonly IOtpCodeRepository _otpCodeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OtpService(OtpOptionSettings otpOptionSettings,
            IOtpCodeRepository otpCodeRepository,
            IUnitOfWork unitOfWork
            )
        {
            _otpOptionSettings = otpOptionSettings;
            _otpCodeRepository = otpCodeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GenerateAsync(string phoneNumber, CancellationToken ct = default)
        {
            // Invalidate any previous unused OTPs for this phone to keep uniqueness simple.
            var existingUnused = await _otpCodeRepository.GetListUnusedOtpsByPhoneNumberAsync(phoneNumber, ct);

            if (existingUnused.Any())
            {
                foreach (var otp in existingUnused)
                {
                    otp.IsUsed = true;
                    otp.UsedAtUtc = DateTime.UtcNow;
                }
                _otpCodeRepository.UpdateRange(existingUnused);
                await _unitOfWork.CompleteAsync(ct);
            }

            var attempts = 0;
            string code;
            do
            {
                code = StringHelper.GenerateNumericCode(_otpOptionSettings.CodeLength);
                attempts++;

                var collision = await _otpCodeRepository
                    .AnyAsync(x => x.PhoneNumber == phoneNumber && !x.IsUsed && x.Code == code, ct);

                if (!collision) break;
            }
            while (attempts < _otpOptionSettings.MaxGenerationAttempts);

            if (attempts >= _otpOptionSettings.MaxGenerationAttempts)
            {
                throw new InvalidOperationException("Failed to generate a unique OTP. Please try again.");
            }

            var now = DateTime.UtcNow;
            var entity = new OtpCode
            {
                Id = Guid.NewGuid(),
                PhoneNumber = phoneNumber,
                Code = code,
                ExpiresAtUtc = now.AddMinutes(_otpOptionSettings.TtlMinutes),
                IsUsed = false
            };

            await _otpCodeRepository.AddAsync(entity);
            await _unitOfWork.CompleteAsync(ct);

            return code;
        }

        public async Task<VerifySmsOtpResponse> VerifyAsync(VerifySmsOtpRequest smsOtpRequest, CancellationToken ct = default)
        {
            VerifySmsOtpResponse result = new()
            {
                IsSuccess = false
            };

            var now = DateTime.UtcNow;
            var otp = await _otpCodeRepository.GetUnusedOtpsByPhoneNumberAsync(smsOtpRequest.PhoneNumber, ct);

            if (otp == null) return result;

            // Important !!! Ignore For test
            if (!string.Equals(otp.Code, smsOtpRequest.OtpCode, StringComparison.Ordinal)) return result;

            if (otp.ExpiresAtUtc <= now) return result;

            otp.IsUsed = true;
            otp.UsedAtUtc = now;

            _otpCodeRepository.Update(otp);
            await _unitOfWork.CompleteAsync(ct);
            result.IsSuccess = true;
            return result;
        }
    }
}