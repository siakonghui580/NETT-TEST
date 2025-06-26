using Application.Shared.Users;
using Application.Shared.Users.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Web.Host.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController
    {
        #region Declarations
        private readonly IUserService _service;
        #endregion

        #region Constructor
        public UserController(IUserService service)
        {
            _service = service;
        }
        #endregion

        #region Methods

        #region C
        [HttpPost("create")]
        public async Task<int> CreateUserAsync(CreateUserInputDTO input)
        {
            return await _service.CreateUserAsync(input);
        }
        #endregion

        #region R
        [HttpPost("user-listing")]
        public async Task<List<UserDTO>> GetUsersAsync(GetUsersInputDTO input)
        {
            return await _service.GetUsersAsync(input);
        }

        [HttpGet("{id}")]
        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            return await _service.GetUserByIdAsync(id);
        }
        #endregion

        #region U
        [HttpPut("update")]
        public async Task UpdateUserAsync([FromBody] UpdateUserInputDTO input)
        {
            await _service.UpdateUserAsync(input);
        }
        #endregion

        #region D
        [HttpDelete("{id}")]
        public async Task DeleteUserAsync(int id)
        {
            await _service.DeleteUserAsync(id);
        }
        #endregion

        #endregion
    }
}
