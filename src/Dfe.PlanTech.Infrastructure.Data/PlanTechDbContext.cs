using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Questions.Models;
using Dfe.PlanTech.Domain.Responses.Models;
using Dfe.PlanTech.Domain.SignIn.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class PlanTechDbContext : DbContext, IPlanTechDbContext
{
    public DbSet<User> Users { get; set; } = null!;

    public DbSet<SignIn> SignIn { get; set; } = null!;

    public DbSet<Question> Questions { get; set; } = null!;

    public DbSet<Answer> Answers { get; set; } = null!;

    public DbSet<Submission> Submissions { get; set; } = null!;

    public DbSet<Response> Responses { get; set; } = null!;

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

        // Setup Question Table
        modelBuilder.Entity<Question>(builder =>
        {
            builder.HasKey(question => question.Id);
            builder.Property(question => question.Id).ValueGeneratedOnAdd();
            builder.Property(question => question.QuestionText).HasMaxLength(4000); // NVARCHAR Max Length
            builder.Property(question => question.ContentfulRef).HasMaxLength(50);
            builder.Property(question => question.DateCreated).ValueGeneratedOnAdd();
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

        modelBuilder.Entity<Submission>(builder =>
        {
            builder.HasKey(submission => submission.Id);
            builder.Property(submission => submission.DateCreated).HasColumnType("datetime").HasDefaultValue();
        });

        modelBuilder.Entity<Response>(builder =>
        {
            builder.HasKey(response => response.Id);
            builder.Property(response => response.DateCreated).HasColumnType("datetime").HasDefaultValue();
        });
    }

    public IQueryable<User> GetUsers => Users;
    public IQueryable<SignIn> SignIns => SignIn;

    public void AddUser(User user) => Users.Add(user);
    public void AddSignIn(SignIn signIn) => SignIn.Add(signIn);

    public void AddQuestion(Question question) => Questions.Add(question);
    public Task<Question?> GetQuestionBy(int questionId) => Questions.FirstOrDefaultAsync(question => question.Id == questionId);

    public void AddAnswer(Answer answer) => Answers.Add(answer);
    public Task<Answer?> GetAnswerBy(int answerId) => Answers.FirstOrDefaultAsync(answer => answer.Id == answerId);

    public void AddSubmission(Submission submission) => Submissions.Add(submission);

    public void AddResponse(Response response) => Responses.Add(response);
    public async Task<Response[]?> GetResponseListBy(int submissionId) => await Responses.Where(response => response.SubmissionId == submissionId).ToArrayAsync();

    public Task<int> SaveChangesAsync() => base.SaveChangesAsync();

    public Task<User?> GetUserBy(Expression<Func<User, bool>> predicate) => Users.FirstOrDefaultAsync(predicate);

    public Task<int> CallStoredProcedureWithReturnInt(string sprocName, List<SqlParameter> parms) => base.Database.ExecuteSqlRawAsync(sprocName, parms);
}
