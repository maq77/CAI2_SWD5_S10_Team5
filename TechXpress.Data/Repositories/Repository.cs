using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Repositories.Base;

namespace TechXpress.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _dp;
        private readonly DbSet<T> _Set;
        private readonly ILogger<Repository<T>> _logger; // Logger added

        public Repository(AppDbContext dp, ILogger<Repository<T>> logger)
        {
            _dp = dp;
            _Set = dp.Set<T>();
            _logger = logger;
        }

        public async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IQueryable<T>>? include = null) 
        {
            IQueryable<T> query = _Set;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
            {
                query = include(query); // Apply includes dynamically
            }

            _logger.LogInformation("Fetching all {EntityName} records from the database.", typeof(T).Name);
            return await query.ToListAsync();
        }


        public async Task<T?> GetById(int id)
        {
            _logger.LogInformation("Fetching {EntityName} with ID {Id}", typeof(T).Name, id);
            return await _Set.FindAsync(id);
        }

        public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate)
        {
            _logger.LogInformation("Finding {EntityName} records with specified condition.", typeof(T).Name);
            return await _Set.Where(predicate).ToListAsync();
        }
        public async Task<T?> Find_First(Expression<Func<T, bool>> predicate)
        {
            _logger.LogInformation("Finding {EntityName} records with specified condition.", typeof(T).Name);
            return await _Set.FirstOrDefaultAsync(predicate);
        }

        public async Task Add(T entity, Action<string>? logAction)
        {
            logAction?.Invoke($"Adding new {typeof(T).Name} entity to the Database!");
            _logger.LogInformation("Adding new {EntityName} entity to the Database.", typeof(T).Name);

            await _Set.AddAsync(entity);
            await _dp.SaveChangesAsync();
        }

        public async Task AddRange(IEnumerable<T> entities, Action<string>? logAction)
        {
            logAction?.Invoke($"Adding new entities to the Database !");
            _logger.LogInformation("Adding new {Entities} to the Database !", typeof(T).Name);
            await _Set.AddRangeAsync(entities);
            await _dp.SaveChangesAsync();
        }
        public async Task Update(T entity, Action<string>? logAction)
        {
            logAction?.Invoke($"Updating {typeof(T).Name} entity.");
            _logger.LogInformation("Updating {EntityName} entity.", typeof(T).Name);

            _Set.Update(entity);
            await _dp.SaveChangesAsync();
        }

        public async Task Delete(int id, Action<string>? logAction)
        {
            var entity = await _Set.FindAsync(id);
            if (entity != null)
            {
                logAction?.Invoke($"Deleting {typeof(T).Name} entity with ID: {id}.");
                _logger.LogInformation("Deleting {EntityName} entity with ID {Id}.", typeof(T).Name, id);

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
