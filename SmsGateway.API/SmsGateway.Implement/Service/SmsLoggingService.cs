using SmsGateway.Implement.EntityModels;
using SmsGateway.Implement.Repositories.Interface;
using SmsGateway.Implement.Services.Interfaces;
using SmsGateway.Implement.UnitOfWork;
using SmsGateway.Implement.ViewModels.Request;
using SmsGateway.Implement.ViewModels.Response;

namespace SmsGateway.Implement.Services
{
    public class SmsLoggingService : ISmsLoggingService
    {
        private readonly ISmsSendLogRepository _smsSendLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SmsLoggingService(
            ISmsSendLogRepository smsSendLogRepository,
            IUnitOfWork unitOfWork)
        {
            _smsSendLogRepository = smsSendLogRepository;
            _unitOfWork = unitOfWork;
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

            await _smsSendLogRepository.AddAsync(log);
            await _unitOfWork.CompleteAsync(ct);
        }
    }
}