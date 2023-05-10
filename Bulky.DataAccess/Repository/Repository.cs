using System.Linq.Expressions;
using Bulky.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext dbContext;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> critera, string includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            query = query.Where(critera);
             if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries ))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<T>> GetAllAsync( string includeProperties = null )
        {
            IQueryable<T> query = dbSet;
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries ))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IReadOnlyList<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}