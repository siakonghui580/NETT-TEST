using Application.Shared.RabbitMq;
using Application.Shared.RedisCache;
using Application.Shared.Users;
using Application.Shared.Users.DTO;
using Core.Users;
using DataAccess.Shared.Users;

namespace Application.Users
{
    public class UserService : IUserService
    {
        #region Declarations
        private readonly IUserRepository _repository;
        private readonly IRabbitMqPublisherService _rabbitMqPublisherService;
        private readonly IRedisCacheService _redisCacheService;
        #endregion

        #region Constructor
        public UserService(
            IUserRepository repository,
            IRabbitMqPublisherService rabbitMqPublisherService,
            IRedisCacheService redisCacheService
            )
        {
            _repository = repository;
            _rabbitMqPublisherService = rabbitMqPublisherService;
            _redisCacheService = redisCacheService;
        }
        #endregion

        #region Methods

        #region C
        public async Task<int> CreateUserAsync(CreateUserInputDTO input)
        {
            #region Validation
            if (string.IsNullOrWhiteSpace(input.Name))
                throw new ArgumentException("Name cannot be null or empty.", nameof(input.Name));
            #endregion

            var user = new User
            {
                Name = input.Name,
                IsActive = input.IsActive,
            };

            var userId = await _repository.CreateUserAsync(user);

            _rabbitMqPublisherService.Publish($"New user created: Id = {user.Id}, Name = {user.Name}, Active = {user.IsActive}");

            return userId;
        }
        #endregion

        #region R
        public async Task<List<UserDTO>> GetUsersAsync(GetUsersInputDTO input)
        {
            var usersFromCache = await GetUsersFromCacheAsync($"name:{input.Name};isactive:{input.IsActive}");

            if (usersFromCache != null)
                return usersFromCache;

            var users = await _repository.GetFilteredUsersAsync(name: input.Name, isActive: input.IsActive);

            await _redisCacheService.SetAsync($"Users:name:{input.Name};isactive:{input.IsActive}", users, TimeSpan.FromMinutes(1));

            return users.Select(x => new UserDTO()
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive
            }).ToList();
        }
        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            #region Validation
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "User ID must be greater than zero.");
            #endregion

            var userFromCache = await GetUserFromCacheAsync(id);

            if (userFromCache != null)
                return userFromCache;

            var user = await _repository.GetUserByIdAsync(id);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found.");
            else
                await _redisCacheService.SetAsync($"User:{id}", user, TimeSpan.FromMinutes(10));

            return new UserDTO()
            {
                Id = user.Id,
                Name = user.Name,
                IsActive = user.IsActive
            };
        }

        private async Task<List<UserDTO>?> GetUsersFromCacheAsync(string filterCriteria)
        {
            return await _redisCacheService.GetAsync<List<UserDTO>>($"Users:{filterCriteria}");
        }

        private async Task<UserDTO?> GetUserFromCacheAsync(int id)
        {
            return await _redisCacheService.GetAsync<UserDTO>($"User:{id}");
        }
        #endregion

        #region U
        public async Task UpdateUserAsync(UpdateUserInputDTO input)
        {
            #region Validation
            if (input.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(input.Id), "User ID must be greater than zero.");

            if (string.IsNullOrWhiteSpace(input.Name))
                throw new ArgumentException("Name cannot be null or empty.", nameof(input.Name));

            var user = await _repository.GetUserByIdAsync(input.Id);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {input.Id} not found.");
            #endregion

            var rabbitMqPropoertyUpdatedMessages = new List<string>();

            if (user.Name != input.Name)
                rabbitMqPropoertyUpdatedMessages.Add($"Name updated from '{user.Name}' to '{input.Name}'");

            if (user.IsActive != input.IsActive)
                rabbitMqPropoertyUpdatedMessages.Add($"IsActive updated from '{user.IsActive}' to '{input.IsActive}'");

            user.Name = input.Name;
            user.IsActive = input.IsActive;

            await _repository.UpdateUserAsync(user);

            if (rabbitMqPropoertyUpdatedMessages.Count > 0)
            {
                await _redisCacheService.DeleteAsync($"User:{user.Id}");
                _rabbitMqPublisherService.Publish($"User updated: Id = {user.Id}, {string.Join(", ", rabbitMqPropoertyUpdatedMessages)}");
            }
        }
        #endregion

        #region D
        public async Task DeleteUserAsync(int id)
        {
            #region Validation
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "User ID must be greater than zero.");

            var user = await _repository.GetUserByIdAsync(id);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found.");
            #endregion

            await _repository.DeleteUserAsync(user);

            await _redisCacheService.DeleteAsync($"User:{user.Id}");
            _rabbitMqPublisherService.Publish($"User deleted: Id = {user.Id}");
        }
        #endregion

        #endregion
    }
}
