﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Data.Repositories.Base
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IQueryable<T>>? include = null);
        Task<IEnumerable<T>> GetAll_includes(Expression<Func<T, bool>>? filter = null, string[]? includes = null);
        Task<T?> GetById(int id);
        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate);
        Task<T?> Find_First(Expression<Func<T, bool>> predicate);
        Task Add(T entity, Action<string>? logAction);
        Task AddRange(IEnumerable<T> entities, Action<string>? logAction);
        Task Update(T entity, Action<string>? logAction);
        Task Delete(int id, Action<string>? logAction);
        Task<bool> SaveChanges();
    }
}
