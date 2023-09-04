namespace Dfe.PlanTech.Web.Exceptions;

using System;

public class ContentfulDataUnavailableException : Exception
{
    public ContentfulDataUnavailableException()
    {
    }

    public ContentfulDataUnavailableException(string message)
        : base(message)
    {
    }

    public ContentfulDataUnavailableException(string message, Exception inner)
        : base(message, inner)
    {
    }
}