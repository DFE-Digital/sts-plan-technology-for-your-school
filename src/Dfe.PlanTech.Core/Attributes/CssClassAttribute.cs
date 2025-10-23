namespace Dfe.PlanTech.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CssClassAttribute : Attribute
    {
        public string ClassName { get; set; }
    }
}
