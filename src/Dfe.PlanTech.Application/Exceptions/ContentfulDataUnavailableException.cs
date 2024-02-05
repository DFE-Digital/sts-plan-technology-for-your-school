namespace Dfe.PlanTech.Application.Exceptions;
public class ContentfulDataUnavailableException : Exception
{
    public ContentfulDataUnavailableException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public ContentfulDataUnavailableException(string message) : base(message)
    {
    }
}