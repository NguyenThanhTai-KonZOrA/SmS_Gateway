using SmsGateway.Implement.ApplicationDbContext;

namespace SmsGateway.Implement.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        #region Constructor
        private readonly SmsGatewayDbContext _context;
        public UnitOfWork(SmsGatewayDbContext context)
        {
            _context = context;
        }
        #endregion

        #region Main functions

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
        public void Update() => _context.Update(this);
        public void UpdateRange() => _context.UpdateRange(this);
        public void Dispose() => _context.Dispose();
        #endregion
    }
}