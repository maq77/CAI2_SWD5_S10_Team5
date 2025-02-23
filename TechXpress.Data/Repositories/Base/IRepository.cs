using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Data.Repositories.Base
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T?> GetById(int id);
        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate);
        Task Add(T entity, Action<string> logAction);
        Task Update(T entity, Action<string> logAction);
        Task Delete(int id, Action<string> logAction);
        Task<bool> SaveChanges();
    }
}
