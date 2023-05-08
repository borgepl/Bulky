using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<IReadOnlyList<T>> GetAllAsync();

        Task<T> GetByIdAsync(int id );
       
        Task<T> GetAsync(Expression<Func<T, bool>> filter);

        void Add(T entity);

        void Remove(T entity);
        void RemoveRange(IReadOnlyList<T> entity);
    }
}