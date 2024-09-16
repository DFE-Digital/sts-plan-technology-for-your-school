using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.SignIns.Models;
using Dfe.PlanTech.Infrastructure.SignIns.ConnectEvents;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Dfe.PlanTech.Infrastructure.SignIns;

[ExcludeFromCodeCoverage]
public static class DfeSignInSetup
{
    public static IServiceCollection AddDfeSignIn(this IServiceCollection services, IConfiguration configuration)
    {
        var config = GetDfeSignInConfig(configuration);

        services.AddAuthentication(ConfigureAuthentication)
                .AddOpenIdConnect(options => ConfigureOpenIdConnect(options, config))
                .AddCookie(options => ConfigureCookie(options, config));

        services.AddScoped((services) => config);
        services.AddScoped<IDfeSignInConfiguration>((_) => config);

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    private static void ConfigureAuthentication(AuthenticationOptions sharedOptions)
    {
        sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    }

    private static void ConfigureCookie(CookieAuthenticationOptions options, IDfeSignInConfiguration config)
    {
        options.Cookie.Name = config.CookieName;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(config.CookieExpireTimeSpanInMinutes);
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = config.SlidingExpiration;
        options.AccessDeniedPath = config.AccessDeniedPath;
    }

    private static void ConfigureOpenIdConnect(OpenIdConnectOptions options, IDfeSignInConfiguration config)
    {
        options.ClientId = config.ClientId;
        options.ClientSecret = config.ClientSecret;
        options.Authority = config.Authority;
        options.MetadataAddress = config.MetaDataUrl;

        options.CallbackPath = new PathString(config.CallbackUrl);
        options.SignedOutRedirectUri = new PathString(config.SignoutRedirectUrl);
        options.SignedOutCallbackPath = new PathString(config.SignoutCallbackUrl);
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SkipUnrecognizedRequests = config.SkipUnrecognizedRequests;

        options.Scope.Clear();
        foreach (string scope in config.Scopes)
        {
            options.Scope.Add(scope);
        }

        options.GetClaimsFromUserInfoEndpoint = config.GetClaimsFromUserInfoEndpoint;
        options.SaveTokens = config.SaveTokens;

        options.Events = new OpenIdConnectEvents()
        {
            OnUserInformationReceived = OnUserInformationReceivedEvent.RecordUserSignIn,
            OnRedirectToIdentityProvider = DfeOpenIdConnectEvents.OnRedirectToIdentityProvider,
            OnRedirectToIdentityProviderForSignOut = DfeOpenIdConnectEvents.OnRedirectToIdentityProviderForSignOut,
        };
    }

    private static DfeSignInConfiguration GetDfeSignInConfig(IConfiguration configuration)
    {
        var config = new DfeSignInConfiguration();
        configuration.GetSection("DfeSignIn").Bind(config);
        return config;
    }
}
