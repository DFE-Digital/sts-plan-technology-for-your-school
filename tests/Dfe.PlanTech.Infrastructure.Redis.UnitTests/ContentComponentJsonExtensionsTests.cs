using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Redis.UnitTests;

public class ContentComponentJsonExtensionsTests
{
    [Fact]
    public void AddContentComponentPolymorphicInfo_ShouldSetPolymorphismOptions_WhenTypeIsContentComponent()
    {
        var options = new JsonSerializerOptions()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver().WithAddedModifier(ContentComponentJsonExtensions.AddContentComponentPolymorphicInfo)
        };

        var typeInfo = options.GetTypeInfo(typeof(ContentComponent));
        Assert.NotNull(typeInfo);
        Assert.NotNull(typeInfo.PolymorphismOptions);
        Assert.Equal("$contentcomponenttype", typeInfo.PolymorphismOptions.TypeDiscriminatorPropertyName);
        Assert.True(typeInfo.PolymorphismOptions.IgnoreUnrecognizedTypeDiscriminators);
        Assert.Equal(JsonUnknownDerivedTypeHandling.FailSerialization, typeInfo.PolymorphismOptions.UnknownDerivedTypeHandling);
    }
}
