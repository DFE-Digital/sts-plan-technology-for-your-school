using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Dfe.PlanTech.Infrastructure.SignIns.UnitTests;

public class DummyAuthHandlerOptions : AuthenticationSchemeOptions
{

}
public class DummyAuthHandler : AuthenticationHandler<DummyAuthHandlerOptions>
{
    public DummyAuthHandler(IOptionsMonitor<DummyAuthHandlerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(AuthenticateResult.NoResult());
    }
}