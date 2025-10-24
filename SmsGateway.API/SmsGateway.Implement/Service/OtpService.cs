using Microsoft.EntityFrameworkCore;
using SmsGateway.Implement.ApplicationDbContext;
using SmsGateway.Implement.EntityModels;
using SmsGateway.Implement.Repositories.Interface;
using SmsGateway.Implement.Services.Interfaces;
using SmsGateway.Implement.UnitOfWork;
using SmsGateway.Implement.ViewModels.Request;
using SmsGateway.Implement.ViewModels.Response;
using SmsGateway.Implement.ViewModels.SmsOtpSettings;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SmsGateway.Implement.Services
{
    public class OtpService : IOtpService
    {
        private readonly SmsGatewayDbContext _db;
        private readonly OtpOptionSettings _otpOptionSettings;
        private readonly IOtpCodeRepository _otpCodeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OtpService(SmsGatewayDbContext db, OtpOptionSettings otpOptionSettings,
            IOtpCodeRepository otpCodeRepository,
            IUnitOfWork unitOfWork
            )
        {
            _db = db;
            _otpOptionSettings = otpOptionSettings;
            _otpCodeRepository = otpCodeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GenerateAsync(string phoneNumber, CancellationToken ct = default)
        {
            // Invalidate any previous unused OTPs for this phone to keep uniqueness simple.
            var existingUnused = await _db.OtpCodes
                .Where(x => x.PhoneNumber == phoneNumber && !x.IsUsed)
                .ToListAsync(ct);

            if (existingUnused.Count > 0)
            {
                foreach (var otp in existingUnused)
                {
                    otp.IsUsed = true;
                    otp.UsedAtUtc = DateTime.UtcNow;
                }
                await _db.SaveChangesAsync(ct);
            }

            var attempts = 0;
            string code;
            do
            {
                code = GenerateNumericCode(_otpOptionSettings.CodeLength);
                attempts++;

                var collision = await _db.OtpCodes
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

            _db.OtpCodes.Add(entity);
            await _db.SaveChangesAsync(ct);

            return code;
        }

        public async Task<VerifySmsOtpResponse> VerifyAsync(VerifySmsOtpRequest smsOtpRequest, CancellationToken ct = default)
        {
            VerifySmsOtpResponse result = new();
            result.IsSuccess = false;
            var now = DateTime.UtcNow;
            var otp = await _db.OtpCodes
                .Where(x => x.PhoneNumber == smsOtpRequest.PhoneNumber && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (otp == null) return result;
            if (!string.Equals(otp.Code, smsOtpRequest.OtpCode, StringComparison.Ordinal)) return result;
            if (otp.ExpiresAtUtc <= now) return result;

            otp.IsUsed = true;
            otp.UsedAtUtc = now;

            await _db.SaveChangesAsync(ct);
            result.IsSuccess = true;
            return result;
        }

        private static string GenerateNumericCode(int length)
        {
            // Cryptographically strong numeric code
            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder(length);
            foreach (var b in bytes)
            {
                sb.Append((b % 10).ToString());
            }
            return sb.ToString();
        }
    }
}