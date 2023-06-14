using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Domain.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Infrastructure.Contentful.Helpers;
using Dfe.PlanTech.Web.Helpers;

namespace Dfe.PlanTech.Web;

public static class ProgramExtensions
{
    public static IServiceCollection AddContentfulServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.SetupContentfulClient(configuration, "Contentful", HttpClientPolicyExtensions.AddRetryPolicy);

        services.AddScoped((_) => new TextRendererOptions(new List<MarkOption>() {
            new MarkOption(){
                Mark = "bold",
                HtmlTag = "span",
                Classes = "govuk-body govuk-!-font-weight-bold",
            }
        }));

        services.AddScoped((_) => new ParagraphRendererOptions()
        {
            Classes = "govuk-body",
        });

        services.AddScoped((_) => new HyperlinkRendererOptions()
        {
            Classes = "govuk-link",
        });

        return services;
    }

    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = ".Dfe.PlanTech";
        });

        services.AddSingleton<ICacheOptions>((services) => new CacheOptions());
        services.AddTransient<ICacher, Cacher>();
        services.AddTransient<IUrlHistory, UrlHistory>();

        return services;
    }
}
