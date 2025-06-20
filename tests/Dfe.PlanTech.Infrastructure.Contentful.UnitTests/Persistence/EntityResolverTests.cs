using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Persistence
{
    public class EntityResolverTests
    {
        [Fact]
        public void Should_Find_Types()
        {
            var entityResolver = new EntityResolver(new NullLogger<EntityResolver>());

            Assert.NotEmpty(entityResolver.Types);
        }

        [Fact]
        public void Should_Find_Type_When_Given_Correct_Id()
        {
            var type = typeof(Category);

            var entityResolver = new EntityResolver(new NullLogger<EntityResolver>());

            var contentTypeId = type.Name.ToLower();

            var foundType = entityResolver.Resolve(contentTypeId);

            Assert.Equal(type, foundType);
        }

        [Fact]
        public void Should_Return_MissingComponent_When_NotFound()
        {
            var expectedType = typeof(MissingComponent);

            var entityResolver = new EntityResolver(new NullLogger<EntityResolver>());

            var foundType = entityResolver.Resolve("not a real type");

            Assert.Equal(expectedType, foundType);
        }
    }
}
