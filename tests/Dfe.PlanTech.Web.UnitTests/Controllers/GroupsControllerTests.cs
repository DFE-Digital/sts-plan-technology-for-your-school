﻿using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Groups.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Middleware;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.UnitTests.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class GroupsControllerTests
    {
        private readonly ILogger<GroupsController> _logger;
        private readonly IExceptionHandlerMiddleware _exceptionHandler;
        private readonly IUser _user;
        private readonly IGetPageQuery _getPageQuery;
        private readonly IComponentBuilder _componentBuilder;
        private readonly IGetEstablishmentIdQuery _getEstablishmentIdQuery;
        private readonly IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;
        private readonly IGetGroupSelectionQuery _getGroupSelectionQuery;

        public GroupsControllerTests()
        {
            _logger = Substitute.For<ILogger<GroupsController>>();
            _exceptionHandler = Substitute.For<IExceptionHandlerMiddleware>();
            _user = Substitute.For<IUser>();
            _getPageQuery = Substitute.For<IGetPageQuery>();
            _componentBuilder = Substitute.For<IComponentBuilder>();
            _getEstablishmentIdQuery = Substitute.For<IGetEstablishmentIdQuery>();
            _getSubmissionStatusesQuery = Substitute.For<IGetSubmissionStatusesQuery>();
            _getGroupSelectionQuery = Substitute.For<IGetGroupSelectionQuery>();
        }

        [Fact]
        public async Task GetSelectASchoolView_Returns_Correct_View()
        {
            var controller = new GroupsController(_logger, _exceptionHandler, _user, _getEstablishmentIdQuery, _getSubmissionStatusesQuery);

            var pageContent = _componentBuilder.BuildPage();

            _getPageQuery.GetPageBySlug("select-a-school", Arg.Any<CancellationToken>()).Returns(pageContent);
            _user.GetGroupEstablishments().Returns(new List<EstablishmentLink> { new EstablishmentLink { Id = 1, EstablishmentName = "School 1" } });
            _user.GetOrganisationData().Returns(new EstablishmentDto { OrgName = "Test Group" });

            var result = await controller.GetSelectASchoolView(_getPageQuery, CancellationToken.None);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GroupsSelectorViewModel>(viewResult.Model);
            Assert.Equal("Test Group", model.GroupName);
            Assert.Single(model.GroupEstablishments);
            Assert.Equal("School 1", model.GroupEstablishments.First().EstablishmentName);
        }


        [Fact]
        public async Task GetSchoolDashboardView_Returns_Correct_View()
        {
            var controller = new GroupsController(_logger, _exceptionHandler, _user, _getEstablishmentIdQuery, _getSubmissionStatusesQuery);

            _user.GetOrganisationData().Returns(new EstablishmentDto { OrgName = "Test Group" });
            _getEstablishmentIdQuery.GetEstablishmentId("1").Returns(1);

            var pageContent = _componentBuilder.BuildPage();
            _getPageQuery.GetPageBySlug(UrlConstants.GroupsDashboardSlug, Arg.Any<CancellationToken>()).Returns(pageContent);

            // TODO replace TempData with query

            var result = await controller.GetSchoolDashboardView(_getPageQuery, _getGroupSelectionQuery, CancellationToken.None);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GroupsSchoolDashboardViewModel>(viewResult.Model);
            Assert.Equal("School 1", model.SchoolName);
            Assert.Equal(1, model.SchoolId);
            Assert.Equal("Test Group", model.GroupName);
        }
    }
}
