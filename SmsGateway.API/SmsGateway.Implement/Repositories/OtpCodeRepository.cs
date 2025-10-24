using Implement.Repositories;
using SmsGateway.Implement.ApplicationDbContext;
using SmsGateway.Implement.EntityModels;
using SmsGateway.Implement.Repositories.Interface;

namespace SmsGateway.Implement.Repositories
{
    public class OtpCodeRepository : GenericRepository<OtpCode>, IOtpCodeRepository
    {
        public OtpCodeRepository(SmsGatewayDbContext context) : base(context)
        {
        }
    }
}