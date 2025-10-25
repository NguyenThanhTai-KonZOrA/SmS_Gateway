using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmsGateway.Implement.EntityModels;
using SmsGateway.Implement.Repositories.Interface;
using SmsGateway.Implement.Services.Interfaces;
using SmsGateway.Implement.UnitOfWork;
using SmsGateway.Implement.ViewModels.SmsOtpSettings;

namespace SmsGateway.Implement.Services
{
    public class SmsRateLimiterService : ISmsRateLimiterService
    {
        private readonly SmsRateLimitOptionSettings _options;
        private readonly ILogger<SmsRateLimiterService> _logger;
        private readonly ISmsDailyStatisticRepository _smsDailyStatisticRepository;
        private readonly IUnitOfWork _unitOfWork;
        public SmsRateLimiterService(
            SmsRateLimitOptionSettings options,
            ISmsDailyStatisticRepository smsDailyStatisticRepository,
            ILogger<SmsRateLimiterService> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _options = options;
            _smsDailyStatisticRepository = smsDailyStatisticRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task EnsureAllowedAsync(string phoneNumber, CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var statistic = await _smsDailyStatisticRepository.SingleOrDefaultAsync(x => x.PhoneNumber == phoneNumber && x.Day == today, ct);

            if (statistic == null)
            {
                statistic = new SmsDailyStatistic
                {
                    Id = Guid.NewGuid(),
                    PhoneNumber = phoneNumber,
                    Day = today,
                    AttemptCount = 0,
                    SuccessCount = 0,
                    LockedUntilUtc = null
                };
                await _smsDailyStatisticRepository.AddAsync(statistic);
                await _unitOfWork.CompleteAsync(ct);
            }

            if (statistic.LockedUntilUtc.HasValue && statistic.LockedUntilUtc.Value > DateTime.UtcNow)
            {
                throw new Exception($"SMS locked until {statistic.LockedUntilUtc.Value:O}");
            }

            if (statistic.AttemptCount >= _options.MaxPerDay)
            {
                statistic.LockedUntilUtc = DateTime.UtcNow.AddMinutes(_options.LockDurationMinutes);
                await _unitOfWork.CompleteAsync(ct);
                throw new Exception($"SMS limit exceeded. Locked for {_options.LockDurationMinutes} minutes.");
            }
        }

        public async Task RecordAsync(string phoneNumber, bool success, CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            for (var retry = 0; retry < 3; retry++)
            {
                var stat = await _smsDailyStatisticRepository.SingleOrDefaultAsync(x => x.PhoneNumber == phoneNumber && x.Day == today, ct);

                if (stat == null)
                {
                    stat = new SmsDailyStatistic
                    {
                        Id = Guid.NewGuid(),
                        PhoneNumber = phoneNumber,
                        Day = today,
                        AttemptCount = 0,
                        SuccessCount = 0
                    };
                    await _smsDailyStatisticRepository.AddAsync(stat);
                }

                stat.AttemptCount += 1;
                if (success) stat.SuccessCount += 1;

                try
                {
                    await _unitOfWork.CompleteAsync(ct);
                    return;
                }
                catch (DbUpdateConcurrencyException)
                {
                    _unitOfWork.ClearChangeTracker();
                    // retry on concurrency conflict
                }
            }

            _logger.LogWarning("Failed to record SMS stats for {phoneNumber} due to concurrency.", phoneNumber);
        }
    }
}