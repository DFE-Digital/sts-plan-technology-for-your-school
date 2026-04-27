using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Interfaces;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dfe.PlanTech.Data.Sql;

public class PlanTechDbContext : DbContext
{
    public virtual DbSet<EstablishmentEntity> Establishments { get; set; } = null!;
    public virtual DbSet<EstablishmentGroupEntity> EstablishmentGroups { get; set; } = null!;
    public virtual DbSet<EstablishmentLinkEntity> EstablishmentLinks { get; set; } = null!;
    public virtual DbSet<ResponseEntity> Responses { get; set; } = null!;
    public virtual DbSet<AnswerEntity> Answers { get; set; } = null!;
    public virtual DbSet<QuestionEntity> Questions { get; set; } = null!;
    public virtual DbSet<SectionStatusEntity> SectionStatuses { get; set; } = null!;
    public virtual DbSet<SignInEntity> SignIns { get; set; } = null!;
    public virtual DbSet<SubmissionEntity> Submissions { get; set; } = null!;
    public virtual DbSet<UserEntity> Users { get; set; } = null!;
    public virtual DbSet<UserSettingsEntity> UserSettings { get; set; } = null!;
    public virtual DbSet<GroupReadActivityEntity> GroupReadActivities { get; set; } = null!;
    public virtual DbSet<RecommendationEntity> Recommendations { get; set; } = null!;
    public virtual DbSet<EstablishmentRecommendationHistoryEntity> EstablishmentRecommendationHistories { get; set; } =
        null!;

    public virtual DbSet<UserActionEntity> UserActions { get; set; } = null!;

    private readonly IUserActionIdAccessor? _userActionIdAccessor;

    public PlanTechDbContext() { }

    public PlanTechDbContext(
        DbContextOptions<PlanTechDbContext> options,
        IUserActionIdAccessor? correlationIdAccessor = null)
        : base(options)
    {
        _userActionIdAccessor = correlationIdAccessor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlanTechDbContext).Assembly);

        var submissionStatusConverter = new ValueConverter<SubmissionStatus, string>(
            v => v.ToString(),
            v => SubmissionHelper.ToSubmissionStatus(v)
        );

        modelBuilder
            .Entity<SubmissionEntity>()
            .Property(s => s.Status)
            .HasConversion(submissionStatusConverter);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_userActionIdAccessor is not null)
        {
            var userActionId = _userActionIdAccessor.GetUserActionId();

            foreach (var entry in ChangeTracker.Entries<IUserActionEntity>())
            {
                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    entry.Entity.UserActionId = userActionId;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
