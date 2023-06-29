using System;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.data
{
    public class UserDBContext : DbContext, IUsersDbContext
    {
        public UserDBContext()
        {
        }

        public UserDBContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public async Task<bool> AddUserAsync(User user)
        {
            await Users.AddAsync(user);
            var count = await SaveChangesAsync();
            return count == 1;
        }
    }
}