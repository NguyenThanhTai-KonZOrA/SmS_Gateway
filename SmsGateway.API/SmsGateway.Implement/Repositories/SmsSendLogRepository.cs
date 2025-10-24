using Implement.Repositories;
using SmsGateway.Implement.ApplicationDbContext;
using SmsGateway.Implement.EntityModels;
using SmsGateway.Implement.Repositories.Interface;

namespace SmsGateway.Implement.Repositories
{
    public class SmsSendLogRepository : GenericRepository<SmsSendLog>, ISmsSendLogRepository
    {
        public SmsSendLogRepository(SmsGatewayDbContext context) : base(context)
        {
        }
    }
}