using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class CategoryMapperTests : BaseMapperTests<CategoryDbEntity, CategoryMapper>
{
    private const string ExistingCategoryId = "TestingCategory";
    private readonly string[] _sectionIds = ["section1", "section2", "section3", "section4", "section5"];

    private readonly List<SectionDbEntity> _sections = [];
    private readonly List<CategoryDbEntity> _categories = [];

    private readonly CategoryMapper _mapper;
    private readonly CategoryDbEntity _existingCategory = new()  {  Id = ExistingCategoryId };

    public CategoryMapperTests()
    {
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
        _mapper = new CategoryMapper(EntityUpdater, Logger, JsonOptions, DatabaseHelper);

        MockDatabaseCollection(_sections);
        MockDatabaseCollection(_categories);
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

        FindLogMessagesContainingStrings(EntityUpdaterLogger, notFoundSectionIds);
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
