namespace Dfe.PlanTech.Web.Exceptions;

using System;

public class ContentfulDataUnavailable : Exception
{
    public ContentfulDataUnavailable()
    {
    }

    public ContentfulDataUnavailable(string message)
        : base(message)
    {
    }

    public ContentfulDataUnavailable(string message, Exception inner)
        : base(message, inner)
    {
    }
}