namespace Dfe.PlanTech.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ContentfulTypeAttribute : Attribute
{
    public string Id { get; }

    public ContentfulTypeAttribute(string id) => Id = id;
}
