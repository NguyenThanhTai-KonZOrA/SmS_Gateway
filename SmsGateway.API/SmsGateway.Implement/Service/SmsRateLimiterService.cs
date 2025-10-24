using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmsGateway.Implement.ApplicationDbContext;
using SmsGateway.Implement.EntityModels;
using SmsGateway.Implement.Services.Interfaces;
using SmsGateway.Implement.ViewModels.SmsOtpSettings;

namespace SmsGateway.Implement.Services
{
    public class SmsRateLimiterService : ISmsRateLimiterService
    {
        private readonly SmsGatewayDbContext _db;
        private readonly SmsRateLimitOptionSettings _options;
        private readonly ILogger<SmsRateLimiterService> _logger;

        public SmsRateLimiterService(
            SmsGatewayDbContext db,
            IOptions<SmsRateLimitOptionSettings> options,
            ILogger<SmsRateLimiterService> logger)
        {
            _db = db;
            _logger = logger;
            _options = options.Value;
        }

        public async Task EnsureAllowedAsync(string phoneNumber, CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var stat = await _db.SmsDailyStatistics
                .SingleOrDefaultAsync(x => x.PhoneNumber == phoneNumber && x.Day == today, ct);

            if (stat == null)
            {
                stat = new SmsDailyStatistic
                {
                    Id = Guid.NewGuid(),
                    PhoneNumber = phoneNumber,
                    Day = today,
                    AttemptCount = 0,
                    SuccessCount = 0,
                    LockedUntilUtc = null
                };
                _db.SmsDailyStatistics.Add(stat);
                await _db.SaveChangesAsync(ct);
            }

            if (stat.LockedUntilUtc.HasValue && stat.LockedUntilUtc.Value > DateTime.UtcNow)
            {
                throw new InvalidOperationException($"SMS locked until {stat.LockedUntilUtc.Value:O}");
            }

            if (stat.AttemptCount >= _options.MaxPerDay)
            {
                stat.LockedUntilUtc = DateTime.UtcNow.AddMinutes(_options.LockDurationMinutes);
                await _db.SaveChangesAsync(ct);
                throw new InvalidOperationException($"SMS limit exceeded. Locked for {_options.LockDurationMinutes} minutes.");
            }
        }

        public async Task RecordAsync(string phoneNumber, bool success, CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            for (var retry = 0; retry < 3; retry++)
            {
                var stat = await _db.SmsDailyStatistics
                    .SingleOrDefaultAsync(x => x.PhoneNumber == phoneNumber && x.Day == today, ct);

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
                    _db.SmsDailyStatistics.Add(stat);
                }

                stat.AttemptCount += 1;
                if (success) stat.SuccessCount += 1;

                try
                {
                    await _db.SaveChangesAsync(ct);
                    return;
                }
                catch (DbUpdateConcurrencyException)
                {
                    _db.ChangeTracker.Clear();
                    // retry on concurrency conflict
                }
            }

            _logger.LogWarning("Failed to record SMS stats for {phoneNumber} due to concurrency.", phoneNumber);
        }
    }
}