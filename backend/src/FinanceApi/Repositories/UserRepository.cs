using FinanceApi.Data;
using FinanceApi.Entities;
using FinanceApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Repositories;

public class UserRepository(FinanceDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetByIdAsync(int id) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task AddAsync(User user) => await dbContext.Users.AddAsync(user);

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();
}
