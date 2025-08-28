using Dfe.PlanTech.Core.Caching.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Options;
using Dfe.PlanTech.Core.Contentful.Queries;
using Dfe.PlanTech.Data.Contentful.Interfaces;
using Dfe.PlanTech.Data.Contentful.Persistence;
using NSubstitute;

namespace Dfe.PlanTech.Data.Contentful.UnitTests.Persistence
{
    public class CachedContentfulRepositoryTests
    {
        private readonly IContentfulRepository _contentRepository = Substitute.For<IContentfulRepository>();
        private readonly ICmsCache _cache = Substitute.For<ICmsCache>();
        private readonly IContentfulRepository _cachedContentRepository;

        public CachedContentfulRepositoryTests()
        {
            _cachedContentRepository = new CachedContentfulRepository(_contentRepository, _cache);
            _cache.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<QuestionnaireQuestionEntry?>>>())
                .Returns(callInfo =>
                {
                    var func = callInfo.ArgAt<Func<Task<QuestionnaireQuestionEntry?>>>(1);
                    return func();
                });
            _cache.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IEnumerable<QuestionnaireQuestionEntry>?>>>())
                .Returns(callInfo =>
                {
                    var func = callInfo.ArgAt<Func<Task<IEnumerable<QuestionnaireQuestionEntry>?>>>(1);
                    return func();
                });
            _contentRepository
                .GetEntryByIdOptions(Arg.Any<string>(), Arg.Any<int>())
                .Returns(callinfo =>
                {
                    var id = callinfo.ArgAt<string>(0);
                    var include = callinfo.ArgAt<int>(1);
                    return new GetEntriesOptions(include, [
                        new ContentfulQuerySingleValue()
                         {
                             Field = "sys.id",
                             Value = id
                         }
                    ]);
                });
        }

        [Fact]
        public async Task Should_Cache_GetEntriesAsync_Without_Options()
        {
            await _cachedContentRepository.GetEntriesAsync<QuestionnaireQuestionEntry>();
            await _cache.Received(1).GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IEnumerable<QuestionnaireQuestionEntry>>>>());
            await _contentRepository.Received(1).GetEntriesAsync<QuestionnaireQuestionEntry>();
        }

        [Fact]
        public async Task Should_Cache_GetEntriesAsync_With_Options()
        {
            var options = new GetEntriesOptions(include: 3);
            await _cachedContentRepository.GetEntriesAsync<QuestionnaireQuestionEntry>(options);
            await _cache.Received(1).GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IEnumerable<QuestionnaireQuestionEntry>>>>());
            await _contentRepository.Received(1).GetEntriesAsync<QuestionnaireQuestionEntry>(options);
        }

        [Fact]
        public async Task Should_Not_Cache_GetPaginatedEntities()
        {
            var options = new GetEntriesOptions(include: 3);
            await _cachedContentRepository.GetPaginatedEntriesAsync<QuestionnaireQuestionEntry>(options);
            await _cache.Received(0).GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IEnumerable<QuestionnaireQuestionEntry>>>>());
            await _contentRepository.Received(1).GetPaginatedEntriesAsync<QuestionnaireQuestionEntry>(options);
        }

        [Fact]
        public async Task Should_Not_Cache_GetEntriesAsyncCount()
        {
            var options = new GetEntriesOptions(include: 3);
            await _cachedContentRepository.GetEntriesCountAsync<QuestionnaireQuestionEntry>();
            await _cache.Received(0).GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<int>>>());
            await _contentRepository.Received(1).GetEntriesCountAsync<QuestionnaireQuestionEntry>();
        }

        [Fact]
        public async Task Should_Cache_GetEntityById()
        {
            var id = "test-id";
            await _cachedContentRepository.GetEntryByIdAsync<QuestionnaireQuestionEntry>(id);
            await _cache.Received(1).GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IEnumerable<QuestionnaireQuestionEntry>>>>());
            await _contentRepository.Received(1).GetEntriesAsync<QuestionnaireQuestionEntry>(
                Arg.Is<GetEntriesOptions>(arg => ValidateGetEntriesAsyncOptions(arg, id)));
        }

        private static bool ValidateGetEntriesAsyncOptions(GetEntriesOptions arg, string id)
        {
            var first = arg.Queries?.FirstOrDefault();
            return first is ContentfulQuerySingleValue queryEquals &&
                   queryEquals.Field == "sys.id" &&
                   queryEquals.Value == id;
        }
    }
}
