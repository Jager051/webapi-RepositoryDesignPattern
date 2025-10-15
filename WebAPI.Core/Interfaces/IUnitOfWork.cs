namespace WebAPI.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Generic repository for entities without custom repository
        IRepository<T> Repository<T>() where T : class;
        
        // Specific repositories with custom queries
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        IUserRepository Users { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}

