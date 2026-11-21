using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.PagesViewBuilderTests;

public abstract class PagesViewBuilderTestBase
{
    protected const string ContactLinkId = "contact-link-id";
    protected const string InternalErrorPageId = "internal-error-page-id";

    protected readonly FakeLogger<BaseViewBuilder> _logger = new();
    protected readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();
    protected readonly ICurrentUserProvider _currentUser = Substitute.For<ICurrentUserProvider>();
    protected readonly INotifyService _notifyService = Substitute.For<INotifyService>();
    protected readonly ISubmissionService _submissionService = Substitute.For<ISubmissionService>();
    protected readonly IRecommendationService _recommendationService =
        Substitute.For<IRecommendationService>();

    protected PagesViewBuilder CreateSystemUnderTest() =>
        new(
            _logger,
            Options.Create(new ContactOptionsConfiguration { LinkId = ContactLinkId }),
            Options.Create(
                new ErrorPagesConfiguration { InternalErrorPageId = InternalErrorPageId }
            ),
            _contentfulService,
            _currentUser,
            _notifyService,
            _submissionService,
            _recommendationService
        );
}
