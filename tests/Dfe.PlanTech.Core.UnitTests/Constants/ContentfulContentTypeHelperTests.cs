using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Core.UnitTests.Constants;

public class ContentfulContentTypeHelperTests
{
    [Fact]
    public void GetContentTypeName_ReturnsExpectedContentfulContentTypeId_ForValidEntryClass()
    {
        // Arrange
        var expectedContentfulContentTypeId = ContentfulContentTypeConstants.ComponentAccordionContentfulContentTypeId;

        // Act
        var actualContentfulContentTypeId = ContentfulContentTypeHelper.GetContentTypeName<ComponentAccordionEntry>();

        // Assert
        Assert.Equal(expectedContentfulContentTypeId, actualContentfulContentTypeId);
    }

    [Fact]
    public void GetContentTypeName_ThrowsInvalidOperationException_ForInvalidEntryClass()
    {
        // Arrange
        var invalidEntryClassType = typeof(InvalidEntryClass);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => ContentfulContentTypeHelper.GetContentTypeName<InvalidEntryClass>());
        Assert.Equal($"Could not find content type ID for class type {invalidEntryClassType.Name}", exception.Message);
    }

    private class InvalidEntryClass { }
}
