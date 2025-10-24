using Microsoft.EntityFrameworkCore;
using SmsGateway.Common.PagedResult;
using SmsGateway.Common.Repository;
using SmsGateway.Implement.ApplicationDbContext;
using System.Linq.Expressions;

namespace Implement.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        #region Constructor
        protected readonly SmsGatewayDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(SmsGatewayDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        #endregion

        #region Main functions
        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task<IEnumerable<T>> GetAllNoTrackingAsync() => await _dbSet.AsNoTracking().ToListAsync();
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.Where(predicate).ToListAsync();
        public async Task<IEnumerable<T>> FindWithoutTrackingAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.Where(predicate).AsNoTracking().ToListAsync();
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).FirstOrDefaultAsync();
        }
        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
            _dbSet.AnyAsync(predicate);
        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);
        public void AddRange(IEnumerable<T> entities) => _dbSet.AddRange(entities);
        public void Update(T entity) => _dbSet.Update(entity);
        public void Remove(T entity) => _dbSet.Remove(entity);
        public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);
        public async Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet.AsQueryable();
            foreach (var include in includeProperties)
            {
                query = query.Include(include);
            }
            return await query.ToListAsync();
        }
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            foreach (var include in includeProperties)
            {
                query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            foreach (var include in includeProperties)
            {
                query = query.Include(include);
            }
            return await query.ToListAsync();
        }
        public async Task<PagedResult<T>> FindPagedAsync(
            Expression<Func<T, bool>> predicate,
            int page,
            int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            params Expression<Func<T, object>>[] includeProperties)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (orderBy is null) throw new ArgumentNullException(nameof(orderBy));

            IQueryable<T> query = _dbSet.AsNoTracking().Where(predicate);

            foreach (var include in includeProperties)
                query = query.Include(include);

            var total = await query.CountAsync();

            query = orderBy(query);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            int count = await _dbSet.CountAsync(predicate);
            return (count);
        }
    }
    #endregion
}