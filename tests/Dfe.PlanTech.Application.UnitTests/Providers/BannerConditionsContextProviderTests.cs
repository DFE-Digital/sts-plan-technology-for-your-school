using Dfe.PlanTech.Application.Providers;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests;

public class BannerConditionsContextProviderTests
{
    private readonly ILogger<BannerConditionsContextProvider> _logger = Substitute.For<
        ILogger<BannerConditionsContextProvider>
    >();
    private readonly ICurrentUserProvider _currentUser = Substitute.For<ICurrentUserProvider>();
    private readonly ISubmissionService _submissionService = Substitute.For<ISubmissionService>();
    private readonly IRecommendationService _recommendationService =
        Substitute.For<IRecommendationService>();
    private readonly IUserContentViewService _userContentViewService =
        Substitute.For<IUserContentViewService>();

    private BannerConditionsContextProvider CreateServiceUnderTest()
    {
        _userContentViewService
            .RecordContentViewAsync(Arg.Any<string>())
            .Returns(Task.FromResult(true));

        return new(
            _logger,
            _currentUser,
            _submissionService,
            _recommendationService,
            _userContentViewService
        );
    }

    [Theory]
    [InlineData(null, new[] { 2, 0 }, true)]
    [InlineData(null, new[] { 0, 0 }, false)]
    [InlineData(null, new[] { -2, 0 }, false)]
    [InlineData(new[] { -2, 0 }, null, true)]
    [InlineData(new[] { 0, 0 }, null, true)]
    [InlineData(new[] { 2, 0 }, null, false)]
    [InlineData(new[] { 2, 0 }, new[] { 4, 0 }, false)]
    [InlineData(new[] { 0, 0 }, new[] { 2, 0 }, true)]
    [InlineData(new[] { -2, 0 }, new[] { 0, 0 }, false)]
    [InlineData(new[] { -2, 0 }, new[] { -4, 0 }, false)]
    public async Task RecordViewActionAndGetBannerVisibility_ReturnsCorrectVisibility_When_DateTimeInRange(
        int[]? displayFromMinutesSecondsDeltas,
        int[]? displayToMinutesSecondsDeltas,
        bool expectedVisibility
    )
    {
        var displayFrom = ProduceDateTime(displayFromMinutesSecondsDeltas);
        var displayTo = ProduceDateTime(displayToMinutesSecondsDeltas);

        var banner = new ComponentNotificationBannerEntry
        {
            Sys = new SystemDetails("Banner"),
            DisplayFrom = displayFrom,
            DisplayTo = displayTo,
            NumberOfTimesToShow = null,
            ShowToSchoolUsers = null,
            ShowToGroupUsers = null,
            Conditions = [],
        };

        var sut = CreateServiceUnderTest();

        var bannerVisible = await sut.RecordViewActionAndGetBannerVisibility(banner);

        Assert.Equal(expectedVisibility, bannerVisible);
    }

    [Theory]
    [InlineData(null, 0, true)]
    [InlineData(null, 100, true)]
    [InlineData(5, 0, true)]
    [InlineData(5, 5, false)]
    [InlineData(5, 10, false)]
    public async Task RecordViewActionAndGetBannerVisibility_ReturnsCorrectVisibility_For_NumberOfTimesToShow(
        int? numberOfTimesToShow,
        int numberOfTimesViewed,
        bool expectedVisibility
    )
    {
        _userContentViewService
            .GetNumberOfTimesContentViewedByUserAsync(Arg.Any<string>())
            .Returns(numberOfTimesViewed);

        var banner = new ComponentNotificationBannerEntry
        {
            Sys = new SystemDetails("Banner"),
            DisplayFrom = null,
            DisplayTo = null,
            NumberOfTimesToShow = numberOfTimesToShow,
            ShowToSchoolUsers = null,
            ShowToGroupUsers = null,
            Conditions = [],
        };

        var sut = CreateServiceUnderTest();

        var bannerVisible = await sut.RecordViewActionAndGetBannerVisibility(banner);

        Assert.Equal(expectedVisibility, bannerVisible);
    }

    [Theory]
    [InlineData(true, false, null, null, true)]
    [InlineData(true, false, null, true, true)]
    [InlineData(true, false, null, false, true)]
    [InlineData(true, false, true, null, true)]
    [InlineData(true, false, true, true, true)]
    [InlineData(true, false, true, false, true)]
    [InlineData(true, false, false, null, false)]
    [InlineData(true, false, false, true, false)]
    [InlineData(true, false, false, false, false)]
    [InlineData(false, true, null, null, true)]
    [InlineData(false, true, null, true, true)]
    [InlineData(false, true, null, false, false)]
    [InlineData(false, true, true, null, true)]
    [InlineData(false, true, true, true, true)]
    [InlineData(false, true, true, false, false)]
    [InlineData(false, true, false, null, true)]
    [InlineData(false, true, false, true, true)]
    [InlineData(false, true, false, false, false)]
    public async Task RecordViewActionAndGetBannerVisibility_ReturnsCorrectVisibility_For_UserType(
        bool isSchool,
        bool isGroup,
        bool? showToSchoolUsers,
        bool? showToGroupUsers,
        bool expectedVisibility
    )
    {
        _currentUser
            .OrganisationCategoryIdMatchesAny(
                Arg.Is<IEnumerable<string>>(x =>
                    x.SequenceEqual(DsiConstants.OrganisationEstablishmentCategoryIds)
                )
            )
            .Returns(isSchool);

        _currentUser
            .OrganisationCategoryIdMatchesAny(
                Arg.Is<IEnumerable<string>>(x =>
                    x.SequenceEqual(DsiConstants.OrganisationGroupCategoryIds)
                )
            )
            .Returns(isGroup);

        var banner = new ComponentNotificationBannerEntry
        {
            Sys = new SystemDetails("Banner"),
            DisplayFrom = null,
            DisplayTo = null,
            NumberOfTimesToShow = null,
            ShowToSchoolUsers = showToSchoolUsers,
            ShowToGroupUsers = showToGroupUsers,
            Conditions = [],
        };

        var sut = CreateServiceUnderTest();

        var bannerVisible = await sut.RecordViewActionAndGetBannerVisibility(banner);

        Assert.Equal(expectedVisibility, bannerVisible);
    }

    private DateTime? ProduceDateTime(int[]? minutesSecondsDeltas) =>
        minutesSecondsDeltas is null
            ? default(DateTime?)
            : DateTime
                .UtcNow.AddMinutes(minutesSecondsDeltas[0])
                .AddSeconds(minutesSecondsDeltas[1])
                .AddMilliseconds(-DateTime.UtcNow.Millisecond)
                .AddMicroseconds(-DateTime.UtcNow.Microsecond);
}
