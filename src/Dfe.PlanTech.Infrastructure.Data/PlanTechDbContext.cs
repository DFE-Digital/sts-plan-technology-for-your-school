using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.SignIn.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class PlanTechDbContext : DbContext, IPlanTechDbContext
{
    public DbSet<User> Users { get; set; } = null!;

    public DbSet<SignIn> SignIn { get; set; } = null!;

    public DbSet<Answer> Answers { get; set; } = null!;

    public PlanTechDbContext() { }

    public PlanTechDbContext(DbContextOptions<PlanTechDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Use Singular table names
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.DisplayName());
        }

        //Setup User Table
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(user => user.Id);
            builder.Property(user => user.Id).ValueGeneratedOnAdd();
            builder.Property(user => user.DateCreated).ValueGeneratedOnAdd();
            builder.Property(user => user.DfeSignInRef).HasMaxLength(30);
        });

        // Setup SignIn Table
        modelBuilder.Entity<SignIn>(builder =>
        {
            builder.HasKey(signinId => signinId.Id);
            builder.HasOne(signinId => signinId.User)
            .WithMany(signinId => signinId.SignIns)
            .HasForeignKey(signinId => signinId.UserId)
            .IsRequired();

            //When dealing with Establishment add mapping here and remove following lines
            builder.Property(signinId => signinId.EstablishmentId).HasDefaultValue(1);
            //////
        });

        // Setup Answer Table
        modelBuilder.Entity<Answer>(builder =>
        {
            builder.HasKey(answer => answer.Id);
            builder.Property(answer => answer.Id).ValueGeneratedOnAdd();
            builder.Property(answer => answer.AnswerText).HasMaxLength(4000); // NVARCHAR Max Length
            builder.Property(answer => answer.ContentfulRef).HasMaxLength(50);
            builder.Property(answer => answer.DateCreated).ValueGeneratedOnAdd();
        });
    }

    public IQueryable<User> GetUsers => Users;
    public IQueryable<SignIn> SignIns => SignIn;

    public void AddUser(User user) => Users.Add(user);
    public void AddSignIn(SignIn signIn) => SignIn.Add(signIn);

    public void AddAnswer(Answer answer) => Answers.Add(answer);

    public Task<int> SaveChangesAsync() => base.SaveChangesAsync();

    public Task<User?> GetUserBy(Expression<Func<User, bool>> predicate) => Users.FirstOrDefaultAsync(predicate);
}
