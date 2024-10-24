namespace Dfe.PlanTech.Application.Exceptions;

using System;

public class ContentfulPageUnavailableException : Exception
{
    public ContentfulPageUnavailableException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public ContentfulPageUnavailableException(string message) : base(message)
    {
    }
}
