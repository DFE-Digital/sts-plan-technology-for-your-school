using System.Linq.Expressions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data;

public class UsersDbContext : DbContext, IUsersDbContext
{
    public DbSet<User> Users { get; set; } = null!;

    public UsersDbContext()
    {
    }

    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Use Singular table names
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.DisplayName());
        }

        //Setup user table
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(user => user.Id);
            builder.Property(user => user.Id).ValueGeneratedOnAdd();
            builder.Property(user => user.DateCreated).ValueGeneratedOnAdd();
        });
    }

    public IQueryable<User> GetUsers => Users;

    public void AddUser(User user) => Users.Add(user);

    public Task<int> SaveChangesAsync() => base.SaveChangesAsync();

    public Task<User?> GetUserBy(Expression<Func<User, bool>> predicate) => Users.FirstOrDefaultAsync(predicate);
}
