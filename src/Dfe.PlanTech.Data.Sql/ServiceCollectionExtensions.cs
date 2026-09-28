using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Data.Sql;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        void databaseOptionsAction(DbContextOptionsBuilder options) =>
            options.UseSqlServer(
                configuration.GetConnectionString("Database"),
                opts =>
                {
                    var databaseRetryOptions = configuration
                        .GetRequiredSection(ConfigurationConstants.Database)
                        .Get<DatabaseConfiguration>();
                    opts.EnableRetryOnFailure(
                        databaseRetryOptions.MaxRetryCount,
                        TimeSpan.FromMilliseconds(databaseRetryOptions.MaxDelayInMilliseconds),
                        null
                    );
                }
            );

        services.AddDbContext<PlanTechDbContext>(databaseOptionsAction);

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<IAnswerRepository, AnswerRepository>()
            .AddScoped<IEstablishmentRepository, EstablishmentRepository>()
            .AddScoped<IEstablishmentLinkRepository, EstablishmentLinkRepository>()
            .AddScoped<
                IEstablishmentRecommendationHistoryRepository,
                EstablishmentRecommendationHistoryRepository
            >()
            .AddScoped<IGiasRepository, GiasRepository>()
            .AddScoped<IQuestionRepository, QuestionRepository>()
            .AddScoped<IRecommendationRepository, RecommendationRepository>()
            .AddScoped<IResponseRepository, ResponseRepository>()
            .AddScoped<ISignInRepository, SignInRepository>()
            .AddScoped<IStoredProcedureRepository, StoredProcedureRepository>()
            .AddScoped<ISubmissionRepository, SubmissionRepository>()
            .AddScoped<ITransactionManager, TransactionManager>()
            .AddScoped<IUserActionRepository, UserActionRepository>()
            .AddScoped<IUserContentViewRepository, UserContentViewRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IUserSettingsRepository, UserSettingsRepository>();
    }
}
