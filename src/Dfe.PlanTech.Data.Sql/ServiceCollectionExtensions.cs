using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Options;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Data.Sql;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        void databaseOptionsAction(DbContextOptionsBuilder options) => options.UseSqlServer(configuration.GetConnectionString("Database"),
        opts =>
            {
                var databaseRetryOptions = configuration.GetRequiredSection(ConfigurationConstants.Database).Get<DatabaseOptions>();
                opts.EnableRetryOnFailure(databaseRetryOptions.MaxRetryCount, TimeSpan.FromMilliseconds(databaseRetryOptions.MaxDelayInMilliseconds), null);
            });

        services.AddDbContext<PlanTechDbContext>(databaseOptionsAction);

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<IEstablishmentLinkRepository, EstablishmentLinkRepository>()
            .AddScoped<IEstablishmentRecommendationHistoryRepository, EstablishmentRecommendationHistoryRepository>()
            .AddScoped<IEstablishmentRepository, EstablishmentRepository>()
            .AddScoped<ISignInRepository, SignInRepository>()
            .AddScoped<IRecommendationRepository, RecommendationRepository>()
            .AddScoped<IStoredProcedureRepository, StoredProcedureRepository>()
            .AddScoped<ISubmissionRepository, SubmissionRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IUserSettingsRepository, UserSettingsRepository>()
            ;
    }
}
