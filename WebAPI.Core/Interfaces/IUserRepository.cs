using WebAPI.Core.Entities;

namespace WebAPI.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        // Custom queries specific to User
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null);
        Task<IEnumerable<User>> SearchUsersByNameAsync(string searchTerm);
    }
}

