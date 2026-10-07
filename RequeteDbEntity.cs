using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace YourNamespace.Data
{
    // Interface pour découpler l'implémentation (facile à tester)
    public interface IQueryService
    {
        Task<T?> GetByIdAsync<T>(object id) where T : class;
        Task<List<T>> GetAllAsync<T>() where T : class;
        Task<List<T>> FindAsync<T>(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null) where T : class;
        IQueryable<T> Query<T>() where T : class;
        Task<int> CountAsync<T>(Expression<Func<T, bool>>? predicate = null) where T : class;
    }

    // Implémentation EF Core
    public class EfQueryService : IQueryService
    {
        private readonly DbContext _context;

        public EfQueryService(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<T?> GetByIdAsync<T>(object id) where T : class
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<List<T>> GetAllAsync<T>() where T : class
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<List<T>> FindAsync<T>(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null) where T : class
        {
            IQueryable<T> query = _context.Set<T>();

            if (predicate != null)
                query = query.Where(predicate);

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            if (orderBy != null)
                query = orderBy(query);

            if (skip.HasValue)
                query = query.Skip(skip.Value);

            if (take.HasValue)
                query = query.Take(take.Value);

            return await query.ToListAsync();
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return _context.Set<T>().AsQueryable();
        }

        public async Task<int> CountAsync<T>(Expression<Func<T, bool>>? predicate = null) where T : class
        {
            if (predicate == null)
                return await _context.Set<T>().CountAsync();
            return await _context.Set<T>().CountAsync(predicate);
        }
    }
}
