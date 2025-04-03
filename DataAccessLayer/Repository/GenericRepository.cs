using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly EStoreDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;
        public GenericRepository(EStoreDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            // Detach any existing entity with the same key to avoid tracking conflicts
            var entry = _context.Entry(entity);
            var keyValues = entry.Metadata.FindPrimaryKey()?.Properties
                .Select(p => entry.Property(p.Name).CurrentValue)
                .ToArray();

            if (keyValues != null)
            {
                var existingEntity = await _dbSet.FindAsync(keyValues);
                if (existingEntity != null)
                {
                    _context.Entry(existingEntity).State = EntityState.Detached;
                }
            }

            // Now attach and update the entity
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
        {
            try
            {
                // Step 1: Get count in a completely separate query
                var countQuery = _dbSet.AsQueryable();
                if (predicate != null)
                {
                    countQuery = countQuery.Where(predicate);
                }
                var totalCount = await countQuery.CountAsync();

                // Step 2: Get items in a completely separate query
                var itemsQuery = _dbSet.AsQueryable().AsSplitQuery();
                if (predicate != null)
                {
                    itemsQuery = itemsQuery.Where(predicate);
                }

                if (orderBy != null)
                {
                    itemsQuery = orderBy(itemsQuery);
                }

                var items = await itemsQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in GetPagedAsync: {ex.Message}");
                // Return empty result
                return (new List<TEntity>(), 0);
            }
        }
    }
}
