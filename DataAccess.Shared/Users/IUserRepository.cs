using Core.Users;

namespace DataAccess.Shared.Users
{
    public interface IUserRepository
    {
        #region C
        Task<int> CreateUserAsync(User user);
        #endregion

        #region R
        Task<List<User>> GetFilteredUsersAsync(string? name, bool? isActive);
        Task<User?> GetUserByIdAsync(int id);
        #endregion

        #region U
        Task UpdateUserAsync(User user);
        #endregion

        #region D
        Task DeleteUserAsync(User user);
        #endregion
    }
}
