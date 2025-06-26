using Core.Users;
using DataAccess.Shared.Users;
using EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Users
{
    public class UserRepository : IUserRepository
    {
        #region Declarations
        private readonly AppDbContext _dbContext;
        #endregion

        #region Constructor
        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        #endregion

        #region Methods

        #region C
        public async Task<int> CreateUserAsync(User user)
        {
            _dbContext.Users.Add(user);

            await SaveChangesAsync();
            
            return user.Id;
        }
        #endregion

        #region R
        public async Task<List<User>> GetFilteredUsersAsync(string? name, bool? isActive)
        {
            var query = GetUsersQuery();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(u => u.Name.Contains(name));

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            return await query.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _dbContext.Users.FindAsync(id);
        }
        #endregion

        #region U
        public async Task UpdateUserAsync(User user)
        {
            _dbContext.Users.Update(user);

            await SaveChangesAsync();
        }
        #endregion

        #region D
        public async Task DeleteUserAsync(User user)
        {
            _dbContext.Users.Remove(user);

            await SaveChangesAsync();
        }
        #endregion

        #region Query
        private IQueryable<User> GetUsersQuery()
        {
            return _dbContext.Users.AsQueryable();
        }
        #endregion

        private async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}
