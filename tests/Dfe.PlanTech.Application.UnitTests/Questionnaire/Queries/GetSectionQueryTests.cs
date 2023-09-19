
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Exceptions;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Questionnaire.Queries;

    public class GetSectionQueryTests
    {
        [Fact]
        public async Task GetSectionBySlug_ThrowsExceptionOnRepositoryError()
        {
            var sectionSlug = "section-slug";
            var cancellationToken = CancellationToken.None;

            var repository = Substitute.For<IContentRepository>();
            repository
                .When(repo => repo.GetEntities<Section>(Arg.Any<GetEntitiesOptions>(), cancellationToken))
                .Throw(new Exception("Dummy Exception"));

            var getSectionQuery = new GetSectionQuery(repository);

            await Assert.ThrowsAsync<ContentfulDataUnavailableException>(
                async () => await getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken)
            );
        }
    }

