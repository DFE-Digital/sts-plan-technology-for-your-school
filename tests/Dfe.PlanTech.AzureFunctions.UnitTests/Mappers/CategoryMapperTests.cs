using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class CategoryMapperTests : BaseMapperTests
{
    private readonly CmsWebHookSystemDetailsInnerContainer[] Sections = new[]{
    new CmsWebHookSystemDetailsInnerContainer() {Sys = new() { Id = "Section One Id" } },
    new CmsWebHookSystemDetailsInnerContainer() {Sys = new() { Id = "Section Two Id" } },
    new CmsWebHookSystemDetailsInnerContainer() {Sys = new() { Id = "Section Three Id" } },
    };
    private readonly CmsWebHookSystemDetailsInnerContainer Header = new()
    {
        Sys = new()
        {
            Id = "Header Id"
        }
    };
    private const string CategoryId = "Category Id";

    private readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private readonly CategoryMapper _mapper;
    private readonly ILogger<CategoryMapper> _logger;

    private readonly DbSet<SectionDbEntity> _sectionsDbSet = Substitute.For<DbSet<SectionDbEntity>>();
    private readonly List<SectionDbEntity> _attachedSections = new(4);

    public CategoryMapperTests()
    {
        _logger = Substitute.For<ILogger<CategoryMapper>>();
        _mapper = new CategoryMapper(MapperHelpers.CreateMockEntityRetriever(), MapperHelpers.CreateMockEntityUpdater(), _db, _logger, JsonOptions);

        _db.Sections = _sectionsDbSet;

        _sectionsDbSet.WhenForAnyArgs(sectionDbSet => sectionDbSet.Attach(Arg.Any<SectionDbEntity>()))
                    .Do(callinfo =>
                    {
                        var section = callinfo.ArgAt<SectionDbEntity>(0);
                        _attachedSections.Add(section);
                    });
    }

    [Fact]
    public void Mapper_Should_Map_Category()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["header"] = WrapWithLocalisation(Header),
            ["sections"] = WrapWithLocalisation(Sections),
        };

        var payload = CreatePayload(fields, CategoryId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped;
        Assert.NotNull(concrete);

        Assert.Equal(CategoryId, concrete.Id);
        Assert.Equal(Header.Sys.Id, concrete.HeaderId);

        Assert.Equal(Sections.Length, _attachedSections.Count);

        foreach (var section in Sections)
        {
            var contains = _attachedSections.Any(attached => attached.Id == section.Sys.Id);
            Assert.True(contains);
        }
    }
}