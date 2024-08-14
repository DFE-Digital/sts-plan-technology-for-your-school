using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class CategoryMapperTests : BaseMapperTests
{
    private const string ExistingCategoryId = "TestingCategory";
    private readonly string[] _sectionIds = ["section1", "section2", "section3", "section4", "section5"];

    private readonly List<SectionDbEntity> _sections = [];
    private readonly List<CategoryDbEntity> _categories = [];

    private readonly DbSet<SectionDbEntity> _sectionsDbSet;
    private readonly DbSet<CategoryDbEntity> _categoriesDbSet;

    private static readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private static readonly ILogger<CategoryMapper> _categoryMapper = Substitute.For<ILogger<CategoryMapper>>();
    private readonly ILogger<EntityUpdater> _entityUpdaterLogger;
    private static EntityUpdater CreateMockCategoryUpdater(ILogger<EntityUpdater> logger) => new(logger, _db);
    private readonly CategoryMapper _mapper;

    private readonly CategoryDbEntity _existingCategory = new()
    {
        Id = ExistingCategoryId,
    };

    public CategoryMapperTests()
    {
        _entityUpdaterLogger = Substitute.For<ILogger<EntityUpdater>>();
        _categories.Add(_existingCategory);

        for (var x = 0; x < _sectionIds.Length; x++)
        {
            var sectionId = _sectionIds[x];
            var section = new SectionDbEntity()
            {
                Id = sectionId,
                Order = x
            };

            _sections.Add(section);

            if (x < 3)
            {
                section.CategoryId = ExistingCategoryId;
            }
        }

        _categoriesDbSet = _categories.BuildMockDbSet();
        _db.Categories = _categoriesDbSet;

        for (int x = 0; x < _sectionIds.Length; x++)
        {
            string id = _sectionIds[x];
            var section = new SectionDbEntity()
            {
                Id = id,
                Order = x
            };

            _sections.Add(section);
        }

        _sections.Add(new SectionDbEntity() { Id = "section4" });

        _sectionsDbSet = _sections.BuildMockDbSet();
        _db.Sections = _sectionsDbSet;

        _categories.Add(_existingCategory);
        _categoriesDbSet = _categories.BuildMockDbSet();
        _db.Categories = _categoriesDbSet;

        _db.Set<SectionDbEntity>().Returns(_sectionsDbSet);
        _db.Set<CategoryDbEntity>().Returns(_categoriesDbSet);

        MockEntityEntry(_db, typeof(CategoryDbEntity), typeof(SectionDbEntity));
        _mapper = new CategoryMapper(MapperHelpers.CreateMockEntityRetriever(_db), CreateMockCategoryUpdater(_entityUpdaterLogger), _db, _categoryMapper, JsonOptions);
    }

    [Fact]
    public async Task Should_Create_New_Category_With_Existing_Data()
    {
        var categoryId = "new-category-id";
        var headerId = "new-header-id";
        var internalName = "New category goes here";
        var payload = CreateCategoryPayload(categoryId, headerId, internalName, _sectionIds);

        var category = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.NotNull(category);

        var (incoming, existing) = category.GetTypedEntities<CategoryDbEntity>();

        Assert.NotNull(incoming);
        Assert.Null(existing);

        ValidateCategoryValues(incoming, categoryId, headerId, internalName, _sectionIds);
    }

    [Fact]
    public async Task Should_Update_Existing_Category()
    {
        var headerId = "new-header-id";
        var internalName = "New category internal namegoes here";

        string[] sectionIds = _sectionIds[2..];
        var payload = CreateCategoryPayload(_existingCategory.Id, headerId, internalName, sectionIds);

        var Category = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.NotNull(Category);

        var (incoming, existing) = Category.GetTypedEntities<CategoryDbEntity>();

        Assert.NotNull(incoming);
        Assert.NotNull(existing);

        ValidateCategoryValues(existing, existing.Id, headerId, internalName, sectionIds);
    }

    [Theory]
    [InlineData("Not-An-Existing-RecSec")]
    [InlineData(ExistingCategoryId)]
    public async Task Should_LogWarning_On_Missing_References(string categoryId)
    {
        string[] notFoundSectionIds = ["not-existing-section", "also-not-existing"];
        var expectedSectionIds = _sectionIds[3..];

        var headerId = "new-header-id";
        var internalName = "new internal name";

        var payload = CreateCategoryPayload(categoryId, headerId, internalName, [.. expectedSectionIds, .. notFoundSectionIds]);

        var category = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        var (incoming, existing) = category.GetTypedEntities<CategoryDbEntity>();

        Assert.NotNull(incoming);

        var categoryToCheck = (categoryId == ExistingCategoryId ? existing : incoming)!;

        if (categoryId == ExistingCategoryId)
        {
            Assert.NotNull(existing);
        }
        else
        {
            Assert.Null(existing);
        }

        ValidateCategoryValues(categoryToCheck, categoryId, headerId, internalName, expectedSectionIds);

        FindLogMessagesContainingStrings(_entityUpdaterLogger, notFoundSectionIds);
    }

    private static void ValidateCategoryValues(CategoryDbEntity entity, string categoryId, string headerId, string internalName, string[] sectionIds)
    {
        Assert.Equal(categoryId, entity.Id);
        Assert.Equal(headerId, entity.HeaderId);
        Assert.Equal(internalName, entity.InternalName);

        Assert.Equal(sectionIds.Length, entity.Sections.Count);

        foreach (var sectionId in sectionIds)
        {
            var matching = entity.Sections.FirstOrDefault(section => section.Id == sectionId);
            Assert.NotNull(matching);
        }
    }

    private CmsWebHookPayload CreateCategoryPayload(string categoryId, string headerId, string internalName, string[] sectionIds)
    {
        CmsWebHookSystemDetailsInnerContainer[] sections = sectionIds.Select(CreateReferenceInnerForId).ToArray();

        var fields = new Dictionary<string, object?>()
        {
            ["sections"] = WrapWithLocalisation(sections),
            ["header"] = WrapWithLocalisation(CreateReferenceInnerForId(headerId)),
            ["internalName"] = WrapWithLocalisation(internalName)
        };

        var payload = CreatePayload(fields, categoryId);
        return payload;
    }
}
