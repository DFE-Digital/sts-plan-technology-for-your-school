using System.Diagnostics.CodeAnalysis;

namespace Dfe.ContentSupport.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}