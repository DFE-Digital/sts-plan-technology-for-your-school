using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Persistence;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Persistence
{
    public class CachedContentfulRepositoryTests
    {
        private readonly IContentRepository _contentRepository = Substitute.For<IContentRepository>();
        private readonly ICmsCache _cache = Substitute.For<ICmsCache>();
        private readonly IContentRepository _cachedContentRepository;

        public CachedContentfulRepositoryTests()
        {
            _cachedContentRepository = new CachedContentfulRepository(_contentRepository, _cache);
            _cache.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<Question?>>>())
                .Returns(callInfo =>
                {
                    var func = callInfo.ArgAt<Func<Task<Question?>>>(1);
                    return func();
                });
            _cache.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IEnumerable<Question>?>>>())
                .Returns(callInfo =>
                {
                    var func = callInfo.ArgAt<Func<Task<IEnumerable<Question>?>>>(1);
                    return func();
                });
            _contentRepository
                .GetEntityByIdOptions(Arg.Any<string>(), Arg.Any<int>())
                .Returns(callinfo =>
                {
                    var id = callinfo.ArgAt<string>(0);
                    var include = callinfo.ArgAt<int>(1);
                    return new GetEntriesOptions(include, [
                        new ContentQuerySingleValue()
                        {
                            Field = "sys.id",
                            Value = id
                        }
                    ]);
                });
        }

        [Fact]
        public async Task Should_Cache_GetEntities_Without_Options()
        {
            await _cachedContentRepository.GetEntities<Question>();
            await _cache.Received(1).GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IEnumerable<Question>>>>());
            await _contentRepository.Received(1).GetEntities<Question>();
        }

        [Fact]
        public async Task Should_Cache_GetEntities_With_Options()
        {
            var options = new GetEntriesOptions(include: 3);
            await _cachedContentRepository.GetEntities<Question>(options);
            await _cache.Received(1).GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IEnumerable<Question>>>>());
            await _contentRepository.Received(1).GetEntities<Question>(options);
        }

        [Fact]
        public async Task Should_Not_Cache_GetPaginatedEntities()
        {
            var options = new GetEntriesOptions(include: 3);
            await _cachedContentRepository.GetPaginatedEntities<Question>(options);
            await _cache.Received(0).GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IEnumerable<Question>>>>());
            await _contentRepository.Received(1).GetPaginatedEntities<Question>(options);
        }

        [Fact]
        public async Task Should_Not_Cache_GetEntitiesCount()
        {
            var options = new GetEntriesOptions(include: 3);
            await _cachedContentRepository.GetEntitiesCount<Question>();
            await _cache.Received(0).GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<int>>>());
            await _contentRepository.Received(1).GetEntitiesCount<Question>();
        }

        [Fact]
        public async Task Should_Cache_GetEntityById()
        {
            var id = "test-id";
            await _cachedContentRepository.GetEntityById<Question>(id);
            await _cache.Received(1).GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IEnumerable<Question>>>>());
            await _contentRepository.Received(1).GetEntities<Question>(
                Arg.Is<GetEntriesOptions>(arg => ValidateGetEntitiesOptions(arg, id)));
        }

        private static bool ValidateGetEntitiesOptions(GetEntriesOptions arg, string id)
        {
            var first = arg.Queries?.FirstOrDefault();
            return first is ContentQuerySingleValue queryEquals &&
                   queryEquals.Field == "sys.id" &&
                   queryEquals.Value == id;
        }
    }
}
