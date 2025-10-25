using Implement.Repositories;
using Microsoft.EntityFrameworkCore;
using SmsGateway.Implement.ApplicationDbContext;
using SmsGateway.Implement.EntityModels;
using SmsGateway.Implement.Repositories.Interface;

namespace SmsGateway.Implement.Repositories
{
    public class OtpCodeRepository : GenericRepository<OtpCode>, IOtpCodeRepository
    {
        private readonly SmsGatewayDbContext _context;
        public OtpCodeRepository(SmsGatewayDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<OtpCode>> GetListUnusedOtpsByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default)
        {
            var existingOtpUnused = await _context.OtpCodes.Where(x => x.PhoneNumber == phoneNumber && !x.IsUsed).ToListAsync(ct);
            return existingOtpUnused;
        }

        public async Task<OtpCode> GetUnusedOtpsByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default)
        {
            return await _context.OtpCodes.Where(x => x.PhoneNumber == phoneNumber && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }
    }
}