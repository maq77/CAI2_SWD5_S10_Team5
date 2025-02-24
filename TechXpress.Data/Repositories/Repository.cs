using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Repositories.Base;

namespace TechXpress.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        public Repository(AppDbContext dp)
        {
            _dp = dp;
            _Set = dp.Set<T>();
        }
        private readonly AppDbContext _dp;
        private readonly DbSet<T> _Set;
        public async Task<IEnumerable<T>> GetAll()
        {
            return await _Set.ToListAsync();
        }
        public async Task<T?> GetById(int id)
        {
            return await _Set.FindAsync(id);
        }
        public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate)
        {
            return await _Set.Where(predicate).ToListAsync();
        }
        public async Task Add(T entity, Action<string> logAction)
        {
            logAction?.Invoke($"Adding new {typeof(T).Name} entity to the Database !");
            await _Set.AddAsync(entity);
            await _dp.SaveChangesAsync();
        }
        public async Task AddRange(IEnumerable<T> entities, Action<string> logAction)
        {
            logAction?.Invoke($"Adding new entities to the Database !");
            await _Set.AddRangeAsync(entities);
            await _dp.SaveChangesAsync();
        }

        public async Task Update(T entity, Action<string> logAction)
        {
            logAction?.Invoke($"Updating {typeof(T).Name} entity.");
            _Set.Update(entity);
            await _dp.SaveChangesAsync();
        }


        public async Task Delete(int id, Action<string> logAction)
        {
            var entity = await _Set.FindAsync(id);
            if (entity != null)
            {
                logAction?.Invoke($"Deleting {typeof(T).Name} entity with ID: {id}.");
                _Set.Remove(entity);
                await _dp.SaveChangesAsync();
            }
        }
        public async Task<bool> SaveChanges()
        {
            return await _dp.SaveChangesAsync() > 0;
        }
    }
}
