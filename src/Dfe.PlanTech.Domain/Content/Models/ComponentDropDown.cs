using System;

namespace Dfe.PlanTech.Domain.Content.Models
{
    /// <summary>
    /// Model for DropDown type.
    /// </summary>
	public class ComponentDropDown : ContentComponent
	{
        /// <summary>
        /// The title to display.
        /// </summary>
        public string Title { get; init; } = null!;

        /// <summary>
        /// The Content to display.
        /// </summary>
        public RichTextContent Content { get; init; } = null!;
    }
}

