using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Core.UnitTests.Constants;

public class ContentTypeHelperTests
{
    [Fact]
    public void GetContentTypeName_ReturnsCorrectContentTypeId_ForValidEntryClass()
    {
        // Arrange
        var expectedContentTypeId = ContentTypeConstants.ComponentAccordionContentTypeId;

        // Act
        var actualContentTypeId = ContentTypeHelper.GetContentTypeName<ComponentAccordionEntry>();

        // Assert
        Assert.Equal(expectedContentTypeId, actualContentTypeId);
    }

    [Fact]
    public void GetContentTypeName_ThrowsInvalidOperationException_ForInvalidEntryClass()
    {
        // Arrange
        var invalidEntryClassType = typeof(InvalidEntryClass);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => ContentTypeHelper.GetContentTypeName<InvalidEntryClass>());
        Assert.Equal($"Could not find content type ID for class type {invalidEntryClassType.Name}", exception.Message);
    }

    private class InvalidEntryClass { }
}
