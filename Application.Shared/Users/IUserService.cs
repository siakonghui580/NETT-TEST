using Application.Shared.Users.DTO;

namespace Application.Shared.Users
{
    public interface IUserService
    {
        #region C
        Task<int> CreateUserAsync(CreateUserInputDTO input);
        #endregion

        #region R
        Task<List<UserDTO>> GetUsersAsync(GetUsersInputDTO input);
        Task<UserDTO?> GetUserByIdAsync(int id);
        #endregion

        #region U
        Task UpdateUserAsync(UpdateUserInputDTO input);
        #endregion

        #region D
        Task DeleteUserAsync(int id);
        #endregion
    }
}
