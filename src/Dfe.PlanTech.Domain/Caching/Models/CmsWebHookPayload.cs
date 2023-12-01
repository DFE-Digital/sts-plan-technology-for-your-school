namespace Dfe.PlanTech.Domain.Caching.Models;

public class CmsWebHookPayload
{
    public CmsWebHookSystemDetails Sys { get; init; } = null!;
}

public class CmsWebHookSystemDetails
{
    public string Id { get; init; } = null!;
    public string Type { get; init; } = null!;

    public CmsWebHookSystemDetailsInnerContainer Environment { get; init; } = null!;
    public CmsWebHookSystemDetailsInnerContainer ContentType { get; init; } = null!;
    public CmsWebHookSystemDetailsInnerContainer CreatedBy { get; init; } = null!;
    public CmsWebHookSystemDetailsInnerContainer UpdatedBy { get; init; } = null!;

}

public class CmsWebHookSystemDetailsInnerContainer
{
    public CmsWebHookSystemDetailsInner Sys { get; init; } = null!;
}

public class CmsWebHookSystemDetailsInner
{
    public string Id { get; init; } = null!;
    public string LinkType { get; init; } = null!;
    public string Type { get; init; } = null!;
}