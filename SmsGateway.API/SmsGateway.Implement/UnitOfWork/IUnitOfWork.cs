namespace SmsGateway.Implement.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> CompleteAsync(CancellationToken cancellationToken);
        Task<int> CompleteAsync();
        void Update();
        void UpdateRange();
        void ClearChangeTracker();
    }
}