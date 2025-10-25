using Microsoft.Extensions.Logging;
using SmsGateway.Implement.ApplicationDbContext;
using SmsGateway.Implement.EntityModels;
using SmsGateway.Implement.Services.Interfaces;
using SmsGateway.Implement.ViewModels.Request;
using SmsGateway.Implement.ViewModels.Response;

namespace SmsGateway.Implement.Services
{
    public class SmsLoggingService : ISmsLoggingService
    {
        private readonly SmsGatewayDbContext _db;
        private readonly ILogger<SmsLoggingService> _logger;

        public SmsLoggingService(SmsGatewayDbContext db, ILogger<SmsLoggingService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task LogAsync(SmsLogRequest request, SmsResponse response, string provider, bool success, string? correlationId, CancellationToken ct = default)
        {
            var log = new SmsSendLog
            {
                Id = Guid.NewGuid(),
                PhoneNumber = request.PhoneNumber,
                PlayerId = request.PlayerId,
                FullName = request.FullName,
                Message = request.Message,
                Provider = provider,
                Success = success,
                ErrorCode = response?.ErrorCode,
                ErrorMessage = response?.ErrorMessage,
                CreatedAtUtc = DateTime.UtcNow,
                CorrelationId = correlationId
            };

            _db.SmsSendLogs.Add(log);
            await _db.SaveChangesAsync(ct);
        }
    }
}