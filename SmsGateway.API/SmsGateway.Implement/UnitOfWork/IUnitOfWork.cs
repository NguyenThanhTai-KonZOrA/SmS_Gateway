namespace SmsGateway.Implement.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> CompleteAsync();
        void Update();
        void UpdateRange();
    }
}