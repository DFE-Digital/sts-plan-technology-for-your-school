using Dfe.PlanTech.Core.Providers.Interfaces;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

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
    public virtual DbSet<UserContentViewEntity> UserContentViews { get; set; } = null!;

    private readonly IUserActionIdProvider? _userActionIdProvider;

    public PlanTechDbContext() { }

    public PlanTechDbContext(
        DbContextOptions<PlanTechDbContext> options,
        IUserActionIdProvider? userActionIdProvider = null
    )
        : base(options)
    {
        _userActionIdProvider = userActionIdProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlanTechDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_userActionIdProvider is not null)
        {
            Guid? userActionId = null;

            try
            {
                userActionId = _userActionIdProvider.GetUserActionId();
            }
            catch (InvalidOperationException)
            {
                userActionId = null;
            }

            if (userActionId is not null)
            {
                foreach (
                    var entry in ChangeTracker
                        .Entries<IUserActionEntity>()
                        .Where(e => e.State is EntityState.Added or EntityState.Modified)
                )
                {
                    entry.Entity.UserActionId = userActionId;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
