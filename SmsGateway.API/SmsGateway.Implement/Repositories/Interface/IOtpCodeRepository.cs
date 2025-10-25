using SmsGateway.Common.Repository;
using SmsGateway.Implement.EntityModels;

namespace SmsGateway.Implement.Repositories.Interface
{
    public interface IOtpCodeRepository : IGenericRepository<OtpCode>
    {
        Task<List<OtpCode>> GetListUnusedOtpsByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default);
        Task<OtpCode> GetUnusedOtpsByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default);
    }
}