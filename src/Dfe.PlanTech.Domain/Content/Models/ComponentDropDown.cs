using System;
namespace Dfe.PlanTech.Domain.Content.Models
{
	public class ComponentDropDown : ContentComponent
	{
        public string Title { get; init; } = null!;
        public RichTextContent Content { get; init; } = null!;
    }
}

