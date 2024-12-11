using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Redis.UnitTests;

public class ContentComponentJsonExtensionsTests
{
    [Theory]
    [InlineData(typeof(ContentComponent))]
    [InlineData(typeof(IContentComponent))]
    public void AddContentComponentPolymorphicInfo_ShouldSetPolymorphismOptions(Type type)
    {
        var options = new JsonSerializerOptions()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver().WithAddedModifier(ContentComponentJsonExtensions.AddContentComponentPolymorphicInfo<ContentComponent>)
                                                                .WithAddedModifier(ContentComponentJsonExtensions.AddContentComponentPolymorphicInfo<IContentComponent>)
        };

        var typeInfo = options.GetTypeInfo(type);
        Assert.NotNull(typeInfo);
        Assert.NotNull(typeInfo.PolymorphismOptions);
        Assert.Equal("$" + type.Name.ToLower(), typeInfo.PolymorphismOptions.TypeDiscriminatorPropertyName);
        Assert.True(typeInfo.PolymorphismOptions.IgnoreUnrecognizedTypeDiscriminators);
        Assert.Equal(JsonUnknownDerivedTypeHandling.FailSerialization, typeInfo.PolymorphismOptions.UnknownDerivedTypeHandling);
    }
}
