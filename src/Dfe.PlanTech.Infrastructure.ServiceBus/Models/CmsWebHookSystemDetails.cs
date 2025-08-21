﻿namespace Dfe.PlanTech.Domain.Caching.Models;

public class CmsWebHookSystemDetails
{
    public string Id { get; init; } = null!;
    public string Type { get; init; } = null!;

    public CmsWebHookSystemDetailsInnerContainer Environment { get; init; } = null!;
    public CmsWebHookSystemDetailsInnerContainer ContentType { get; init; } = null!;
    public CmsWebHookSystemDetailsInnerContainer CreatedBy { get; init; } = null!;
    public CmsWebHookSystemDetailsInnerContainer UpdatedBy { get; init; } = null!;
}
