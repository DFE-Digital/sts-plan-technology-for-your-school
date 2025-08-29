using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Extensions;

namespace Dfe.PlanTech.Infrastructure.Redis.UnitTests;

public class ContentComponentJsonExtensionsTests
{
    [Theory]
    [InlineData(typeof(ContentfulEntry))]
    [InlineData(typeof(IContentfulEntry))]
    public void AddContentComponentPolymorphicInfo_ShouldSetPolymorphismOptions(Type type)
    {
        var options = new JsonSerializerOptions()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver().WithAddedModifier(ContentComponentJsonExtensions.AddContentComponentPolymorphicInfo<ContentfulEntry>)
                                                                .WithAddedModifier(ContentComponentJsonExtensions.AddContentComponentPolymorphicInfo<IContentfulEntry>)
        };

        var typeInfo = options.GetTypeInfo(type);
        Assert.NotNull(typeInfo);
        Assert.NotNull(typeInfo.PolymorphismOptions);
        Assert.Equal("$" + type.Name.ToLower(), typeInfo.PolymorphismOptions.TypeDiscriminatorPropertyName);
        Assert.True(typeInfo.PolymorphismOptions.IgnoreUnrecognizedTypeDiscriminators);
        Assert.Equal(JsonUnknownDerivedTypeHandling.FailSerialization, typeInfo.PolymorphismOptions.UnknownDerivedTypeHandling);
    }
}
