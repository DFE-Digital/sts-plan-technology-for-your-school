using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.SignIns.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class PlanTechDbContext : DbContext, IPlanTechDbContext
{
    public DbSet<Establishment> Establishments { get; set; } = null!;
    public DbSet<Response> Responses { get; set; } = null!;
    public DbSet<ResponseAnswer> Answers { get; set; } = null!;
    public DbSet<ResponseQuestion> Questions { get; set; } = null!;
    public DbSet<SectionStatusDto> SectionStatusesSp { get; set; } = null!;
    public DbSet<SignIn> SignIn { get; set; } = null!;
    public DbSet<Submission> Submissions { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

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
            builder.ToTable(tb => tb.HasTrigger("tr_user"));
            builder.Property(user => user.Id).ValueGeneratedOnAdd();
            builder.Property(user => user.DateCreated).ValueGeneratedOnAdd();
            builder.Property(user => user.DfeSignInRef).HasMaxLength(30);
            builder.Property(user => user.DateLastUpdated).HasColumnType("datetime").HasDefaultValue();
        });

        //Setup Establishment Table
        modelBuilder.Entity<Establishment>(builder =>
        {
            builder.HasKey(establishment => establishment.Id);
            builder.ToTable(tb => tb.HasTrigger("tr_establishment"));
            builder.Property(establishment => establishment.DateLastUpdated).HasColumnType("datetime").HasDefaultValue();
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

        // Setup Question Table
        modelBuilder.Entity<ResponseQuestion>(builder =>
        {
            builder.ToTable("question");
            builder.HasKey(question => question.Id);
            builder.Property(question => question.Id).ValueGeneratedOnAdd();
            builder.Property(question => question.QuestionText).HasMaxLength(4000); // NVARCHAR Max Length
            builder.Property(question => question.ContentfulRef).HasMaxLength(50);
            builder.Property(question => question.DateCreated).ValueGeneratedOnAdd();
        });

        // Setup Answer Table
        modelBuilder.Entity<ResponseAnswer>(builder =>
        {
            builder.ToTable("answer");
            builder.HasKey(answer => answer.Id);
            builder.Property(answer => answer.Id).ValueGeneratedOnAdd();
            builder.Property(answer => answer.AnswerText).HasMaxLength(4000); // NVARCHAR Max Length
            builder.Property(answer => answer.ContentfulRef).HasMaxLength(50);
            builder.Property(answer => answer.DateCreated).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Submission>(builder =>
        {
            builder.HasKey(submission => submission.Id);
            builder.ToTable(tb => tb.HasTrigger("tr_submission"));
            builder.Property(submission => submission.DateCreated).HasColumnType("datetime").HasDefaultValue();
            builder.Property(submission => submission.DateLastUpdated).HasColumnType("datetime").HasDefaultValue();
        });

        modelBuilder.Entity<Response>(builder =>
        {
            builder.HasKey(response => response.Id);
            builder.ToTable(tb => tb.HasTrigger("tr_response"));
            builder.Property(response => response.DateCreated).HasColumnType("datetime").ValueGeneratedOnAdd();
            builder.Property(submission => submission.DateLastUpdated).HasColumnType("datetime").HasDefaultValue();
        });

        modelBuilder.Entity<SectionStatusDto>(builder =>
        {
            builder.HasNoKey();
        });
    }

    public IQueryable<SectionStatusDto> GetSectionStatuses(string sectionIds, int establishmentId) => SectionStatusesSp.FromSqlInterpolated($"{DatabaseConstants.GetSectionStatuses} {sectionIds} , {establishmentId}");

    public void AddUser(User user) => Users.Add(user);

    public void AddEstablishment(Establishment establishment) => Establishments.Add(establishment);

    public void AddSignIn(SignIn signIn) => SignIn.Add(signIn);


    public IQueryable<Submission> GetSubmissions => Submissions;

    public IQueryable<ResponseAnswer> GetAnswers => Answers;

    public Task<int> SaveChangesAsync() => base.SaveChangesAsync();

    public Task<User?> GetUserBy(Expression<Func<User, bool>> predicate) => Users.FirstOrDefaultAsync(predicate);

    public Task<Establishment?> GetEstablishmentBy(Expression<Func<Establishment, bool>> predicate) => Establishments.FirstOrDefaultAsync(predicate);

    public Task<int> CallStoredProcedureWithReturnInt(string sprocName, IEnumerable<object> parameters, CancellationToken cancellationToken = default)
     => Database.ExecuteSqlRawAsync(sprocName, parameters, cancellationToken: cancellationToken);

    public Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken = default)
    => Database.ExecuteSqlAsync(sql, cancellationToken);

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default) => queryable.FirstOrDefaultAsync(cancellationToken);

    public Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default) => queryable.ToListAsync(cancellationToken);
}
