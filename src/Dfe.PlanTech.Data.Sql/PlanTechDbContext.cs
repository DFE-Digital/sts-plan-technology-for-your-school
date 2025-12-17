using Dfe.PlanTech.Data.Sql.Entities;
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
    public virtual DbSet<EstablishmentRecommendationHistoryEntity> EstablishmentRecommendationHistories { get; set; } = null!;

    public PlanTechDbContext() { }

    public PlanTechDbContext(DbContextOptions<PlanTechDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlanTechDbContext).Assembly);
    }
}
