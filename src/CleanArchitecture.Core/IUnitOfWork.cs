namespace CleanArchitecture.Core
{
    public interface IUnitOfWork
    {
        void Add<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity;

        void Add<TEntity>(params TEntity[] entities) where TEntity : class, IEntity;

        Task<TEntity?> FindAsync<TEntity>(long id) where TEntity : class, IEntity;

        IQueryable<TEntity> Query<TEntity>() where TEntity : class, IEntity;

        void Remove<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity;

        void Remove<TEntity>(params TEntity[] entities) where TEntity : class, IEntity;

        Task CompleteAsync();

        void Update<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity;

        void Update<TEntity>(params TEntity[] entities) where TEntity : class, IEntity;
    }
}