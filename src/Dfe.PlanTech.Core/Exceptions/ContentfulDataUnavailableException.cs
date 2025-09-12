namespace Dfe.PlanTech.Core.Exceptions;

using System;

public class ContentfulDataUnavailableException : Exception
{
    public ContentfulDataUnavailableException(string message)
        : base(message)
    {
    }

    public ContentfulDataUnavailableException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
