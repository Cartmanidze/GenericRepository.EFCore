using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GenericRepository.EFCore.Models;
using Microsoft.EntityFrameworkCore;

namespace GenericRepository.EFCore.Repositories
{
    public abstract class GenericRepository<TContext, TModel> : IGenericRepository<TModel>
        where TContext : DbContext
        where TModel : BaseModel
    {
        private readonly TContext _context;

        private readonly DbSet<TModel> _dbSet;

        protected GenericRepository(TContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TModel>();
        }

        public virtual Task<TModel[]> GetAsync(
            Expression<Func<TModel, bool>> filter = null,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null,
            string includeProperties = "",
            bool isDeleted = false,
            CancellationToken token = default)
        {
            IQueryable<TModel> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            query = includeProperties
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            return orderBy != null ? orderBy(query).Where(e => e.IsDeleted == isDeleted).ToArrayAsync(token) : query.Where(e => e.IsDeleted == isDeleted).ToArrayAsync(token);
        }

        public virtual ValueTask<TModel> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            if (id == Guid.Empty) throw new ArgumentException(nameof(id) + " is empty");
            return _context.FindAsync<TModel>(new object[] { id }, token);
        }

        public virtual async Task CreateAsync(TModel entity, CancellationToken token = default)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));
            entity.CreatedDate = entity.LastModifiedDate = DateTime.UtcNow;
            await _dbSet.AddAsync(entity, token).ConfigureAwait(false);
        }

        public virtual async Task CreateManyAsync(IEnumerable<TModel> entities, CancellationToken token = default)
        {
            if (entities is null) throw new ArgumentNullException(nameof(entities));
            foreach (var entity in entities)
            {
                await CreateAsync(entity, token).ConfigureAwait(false);
            }
        }

        public virtual void Delete(TModel entityToDelete)
        {
            if (entityToDelete is null) throw new ArgumentNullException(nameof(entityToDelete));
            entityToDelete.IsDeleted = true;
            entityToDelete.LastModifiedDate = DateTime.UtcNow;
            _context.Entry(entityToDelete).Property(e => e.IsDeleted).IsModified = true;
            _context.Entry(entityToDelete).Property(e => e.LastModifiedDate).IsModified = true;
        }

        public virtual void DeleteMany(IEnumerable<TModel> entitiesToDelete)
        {
            if (entitiesToDelete is null) throw new ArgumentNullException(nameof(entitiesToDelete));
            foreach (var entityToDelete in entitiesToDelete)
            {
                Delete(entityToDelete);
            }
        }

        public virtual void Update(TModel entityToUpdate)
        {
            if (entityToUpdate is null) throw new ArgumentNullException(nameof(entityToUpdate));
            entityToUpdate.LastModifiedDate = DateTime.UtcNow;
            _context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual void UpdateMany(IEnumerable<TModel> entitiesToUpdate)
        {
            if (entitiesToUpdate is null) throw new ArgumentNullException(nameof(entitiesToUpdate));
            foreach (var entityToUpdate in entitiesToUpdate)
            {
                Update(entityToUpdate);
            }
        }

        public virtual void Restore(TModel entityToRestore)
        {
            if (entityToRestore is null) throw new ArgumentNullException(nameof(entityToRestore));
            if (!entityToRestore.IsDeleted) return;
            entityToRestore.IsDeleted = false;
            entityToRestore.LastModifiedDate = DateTime.UtcNow;
            _context.Entry(entityToRestore).Property(e => e.IsDeleted).IsModified = true;
            _context.Entry(entityToRestore).Property(e => e.LastModifiedDate).IsModified = true;
        }

        public virtual void RestoreMany(IEnumerable<TModel> entitiesToRestore)
        {
            if (entitiesToRestore is null) throw new ArgumentNullException(nameof(entitiesToRestore));
            foreach (var entityToRestore in entitiesToRestore)
            {
                Restore(entityToRestore);
            }
        }

        public virtual Task<int> SaveAsync(CancellationToken token)
        {
            return _context.SaveChangesAsync(token);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }
    }
}
