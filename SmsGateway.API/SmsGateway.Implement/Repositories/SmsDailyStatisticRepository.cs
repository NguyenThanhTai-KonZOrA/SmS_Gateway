using Implement.Repositories;
using SmsGateway.Implement.ApplicationDbContext;
using SmsGateway.Implement.EntityModels;
using SmsGateway.Implement.Repositories.Interface;

namespace SmsGateway.Implement.Repositories
{
    public class SmsDailyStatisticRepository : GenericRepository<SmsDailyStatistic>, ISmsDailyStatisticRepository
    {
        public SmsDailyStatisticRepository(SmsGatewayDbContext context) : base(context)
        {
        }
    }
}