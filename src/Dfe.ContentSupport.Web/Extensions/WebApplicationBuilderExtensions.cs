using Contentful.Core;
using Contentful.Core.Configuration;
using Dfe.ContentSupport.Web.Configuration;
using Dfe.ContentSupport.Web.Models.Mapped;
using Dfe.ContentSupport.Web.Services;
using Microsoft.Extensions.Options;

namespace Dfe.ContentSupport.Web.Extensions;

public static class WebApplicationBuilderExtensions
{
    public const string ContentAndSupportServiceKey = "content-and-support";

    public static void InitCsDependencyInjection(this WebApplicationBuilder app)
    {
        app.Services.Configure<TrackingOptions>(app.Configuration.GetSection("tracking"))
            .AddSingleton(sp => sp.GetRequiredService<IOptions<TrackingOptions>>().Value);

        app.Services
            .Configure<SupportedAssetTypes>(app.Configuration.GetSection("cs:supportedAssetTypes"))
            .AddSingleton(sp => sp.GetRequiredService<IOptions<SupportedAssetTypes>>().Value);

        app.Services.SetupContentfulClient(app);

        app.Services.AddKeyedTransient<ICacheService<List<CsPage>>, CsPagesCacheService>(
            ContentAndSupportServiceKey);
        app.Services.AddKeyedTransient<IModelMapper, ModelMapper>(ContentAndSupportServiceKey);
        app.Services
            .AddKeyedTransient<IContentService, ContentService>(ContentAndSupportServiceKey);
        app.Services.AddKeyedTransient<ILayoutService, LayoutService>(ContentAndSupportServiceKey);

        app.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
            options.ConsentCookieValue = "false";
        });
    }

    public static void SetupContentfulClient(this IServiceCollection services,
        WebApplicationBuilder app)
    {
        app.Services.Configure<ContentfulOptions>(app.Configuration.GetSection("cs:contentful"))
            .AddKeyedSingleton(ContentAndSupportServiceKey, (IServiceProvider sp) =>
                sp.GetRequiredService<IOptions<ContentfulOptions>>().Value);

        services.AddKeyedScoped<IContentfulClient, ContentfulClient>(ContentAndSupportServiceKey,
            (sp, _) =>
            {
                var contentfulOptions =
                    sp.GetRequiredKeyedService<Func<IServiceProvider, ContentfulOptions>>(
                        ContentAndSupportServiceKey)(sp);
                var httpClient = sp.GetRequiredService<HttpClient>();
                return new ContentfulClient(httpClient, contentfulOptions);
            });

        if (app.Environment.EnvironmentName.Equals("e2e"))
            services.AddKeyedScoped<IContentfulService, StubContentfulService>(
                ContentAndSupportServiceKey);
        else
            services.AddKeyedScoped<IContentfulService, ContentfulService>(
                ContentAndSupportServiceKey);

        CsHttpClientPolicyExtensions.AddRetryPolicy(services.AddHttpClient<ContentfulClient>());
    }
}
