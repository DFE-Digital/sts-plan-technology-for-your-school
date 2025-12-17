using Microsoft.AspNetCore.Mvc.Testing;

namespace Dfe.PlanTech.Web.SiteOfflineMicrosite.UnitTests;

public class SiteOfflineMicrositeWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
    }
}

