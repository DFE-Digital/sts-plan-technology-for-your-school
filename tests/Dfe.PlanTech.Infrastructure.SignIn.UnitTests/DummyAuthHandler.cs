using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Infrastructure.SignIn.UnitTests;

public class DummyAuthHandlerOptions : AuthenticationSchemeOptions
{

}

public class DummyAuthHandler : AuthenticationHandler<DummyAuthHandlerOptions>
{
    public DummyAuthHandler(IOptionsMonitor<DummyAuthHandlerOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(AuthenticateResult.NoResult());
    }
}
