using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class SectionMapperTests : BaseMapperTests
{
    private const string SectionName = "Section name";
    private readonly CmsWebHookSystemDetailsInnerContainer[] Questions = new[]{
    new CmsWebHookSystemDetailsInnerContainer() {Sys = new() { Id = "Question One Id" } },
    new CmsWebHookSystemDetailsInnerContainer() {Sys = new() { Id = "Question Two Id" } },
    new CmsWebHookSystemDetailsInnerContainer() {Sys = new() { Id = "Question Three Id" } },
    };
    private readonly CmsWebHookSystemDetailsInnerContainer InterstitialPage = new()
    {
        Sys = new()
        {
            Id = "Interstitial page id"
        }
    };

    private const string SectionId = "Question Id";

    private readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private readonly SectionMapper _mapper;
    private readonly ILogger<SectionMapper> _logger;

    private readonly DbSet<QuestionDbEntity> _questionsDbSet = Substitute.For<DbSet<QuestionDbEntity>>();
    private readonly List<QuestionDbEntity> _attachedQuestions = new(4);

    public SectionMapperTests()
    {
        PageDbEntity pageDbEntity = new PageDbEntity
        {
            Id = "Interstitial page id",
        };

        var list = new List<PageDbEntity>() { pageDbEntity };
        IQueryable<PageDbEntity> queryable = list.AsQueryable();

        var asyncProvider = new AsyncQueryProvider<PageDbEntity>(queryable.Provider);

        var mockPageDataSet = Substitute.For<DbSet<PageDbEntity>, IQueryable<PageDbEntity>>();
        ((IQueryable<PageDbEntity>)mockPageDataSet).Provider.Returns(asyncProvider);
        ((IQueryable<PageDbEntity>)mockPageDataSet).Expression.Returns(queryable.Expression);
        ((IQueryable<PageDbEntity>)mockPageDataSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<PageDbEntity>)mockPageDataSet).GetEnumerator().Returns(queryable.GetEnumerator());

        _logger = Substitute.For<ILogger<SectionMapper>>();
        _mapper = new SectionMapper(MapperHelpers.CreateMockEntityRetriever(), MapperHelpers.CreateMockEntityUpdater(), _db, _logger, JsonOptions);

        _db.Questions = _questionsDbSet;

        _db.Pages = mockPageDataSet;

        _questionsDbSet.WhenForAnyArgs(questionDbSet => questionDbSet.Attach(Arg.Any<QuestionDbEntity>()))
                    .Do(callinfo =>
                    {
                        var question = callinfo.ArgAt<QuestionDbEntity>(0);
                        _attachedQuestions.Add(question);
                    });
    }

    [Fact]
    public void Mapper_Should_Map_Relationship()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["name"] = WrapWithLocalisation(SectionName),
            ["interstitialPage"] = WrapWithLocalisation(InterstitialPage),
            ["questions"] = WrapWithLocalisation(Questions),
        };

        var payload = CreatePayload(fields, SectionId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        Assert.Equal(SectionId, mapped.Id);
        Assert.Equal(SectionName, mapped.Name);
        Assert.Equal(InterstitialPage.Sys.Id, mapped.InterstitialPageId);

        Assert.Equal(Questions.Length, _attachedQuestions.Count);

        foreach (var question in Questions)
        {
            var contains = _attachedQuestions.Any(attached => attached.Id == question.Sys.Id);
            Assert.True(contains);
        }
    }
}