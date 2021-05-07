using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GenericRepository.EFCore.Models;

namespace GenericRepository.EFCore.Repositories
{
    public interface IGenericRepository<TModel> : IDisposable where TModel : BaseModel
    {
        Task<TModel[]> GetAsync(
            Expression<Func<TModel, bool>> filter = null,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null,
            string includeProperties = "",
            bool isDeleted = false,
            CancellationToken token = default);

        ValueTask<TModel> GetByIdAsync(Guid id, CancellationToken token = default);

        Task CreateAsync(TModel entity, CancellationToken token = default);

        Task CreateManyAsync(IEnumerable<TModel> entities, CancellationToken token = default);

        void Delete(TModel entityToDelete);

        void DeleteMany(IEnumerable<TModel> entitiesToDelete);

        void Update(TModel entityToUpdate);

        void UpdateMany(IEnumerable<TModel> entitiesToUpdate);

        void Restore(TModel entityToRestore);

        void RestoreMany(IEnumerable<TModel> entitiesToRestore);

        Task<int> SaveAsync(CancellationToken token = default);
    }
}
