namespace Dfe.PlanTech.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CssClassAttribute : Attribute
    {
        public required string ClassName { get; set; }
    }
}
