using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web
{
    public static class WebApplicationBuilderExtensions
    {
        public static IServiceCollection AddSystemConfiguration(this WebApplicationBuilder builder)
        {
            return builder
                .Services.Configure<AutomatedTestingConfiguration>(
                    builder.Configuration.GetRequiredSection(
                        ConfigurationConstants.AutomatedTesting
                    )
                )
                .Configure<ContactOptionsConfiguration>(
                    builder.Configuration.GetRequiredSection(ConfigurationConstants.ContactUs)
                )
                .Configure<ErrorMessagesConfiguration>(
                    builder.Configuration.GetRequiredSection(ConfigurationConstants.ErrorMessages)
                )
                .Configure<ErrorPagesConfiguration>(
                    builder.Configuration.GetRequiredSection(ConfigurationConstants.ErrorPages)
                )
                .Configure<RobotsConfiguration>(
                    builder.Configuration.GetRequiredSection(ConfigurationConstants.Robots)
                );
        }

        public static void AddContentAndSupportConfiguration(this WebApplicationBuilder app)
        {
            app.Services.Configure<ContentSecurityPolicyConfiguration>(
                    app.Configuration.GetRequiredSection(
                        ConfigurationConstants.ContentSecurityPolicy
                    )
                )
                .AddSingleton(sp =>
                    sp.GetRequiredService<IOptions<ContentSecurityPolicyConfiguration>>().Value
                );

            app.Services.Configure<SupportedAssetTypesConfiguration>(
                    app.Configuration.GetRequiredSection(
                        ConfigurationConstants.CAndSSupportedAssetTypes
                    )
                )
                .AddSingleton(sp =>
                    sp.GetRequiredService<IOptions<SupportedAssetTypesConfiguration>>().Value
                );

            app.Services.Configure<TrackingOptionsConfiguration>(
                    app.Configuration.GetSection("tracking")
                )
                .AddSingleton(sp =>
                    sp.GetRequiredService<IOptions<TrackingOptionsConfiguration>>().Value
                );

            app.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.ConsentCookieValue = "false";
            });
        }
    }
}
