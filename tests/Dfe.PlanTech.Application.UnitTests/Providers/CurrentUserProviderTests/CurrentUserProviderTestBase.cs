using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Application.Providers;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Providers.CurrentUserProviderTests;

public abstract class CurrentUserProviderTestBase
{
    protected readonly FakeLogger<CurrentUserProvider> _logger = new();
    protected readonly HttpContextAccessor _contextAccessor = new();
    protected readonly IEstablishmentService _establishmentService =
        Substitute.For<IEstablishmentService>();
    protected readonly IGiasService _giasService = Substitute.For<IGiasService>();

    protected CurrentUserProvider CreateSystemUnderTest(
        EstablishmentModel? organisation = null,
        FakeSession? session = null,
        IEnumerable<Claim>? claims = null,
        bool isAuthenticated = true
    )
    {
        var contextBuilder = new HttpContextBuilder().WithSession(session ?? new FakeSession());

        if (!isAuthenticated)
        {
            contextBuilder.Unauthenticated();
        }

        if (claims is not null)
        {
            contextBuilder.WithClaims([.. claims]);
        }

        if (organisation is not null)
        {
            contextBuilder.WithClaim(
                ClaimConstants.Organisation,
                JsonSerializer.Serialize(organisation)
            );
        }

        _contextAccessor.HttpContext = contextBuilder.Build();

        return new CurrentUserProvider(
            _logger,
            _contextAccessor,
            _establishmentService,
            _giasService
        );
    }
}
