using System.Runtime.Serialization;

namespace Dfe.PlanTech.Data.Contentful.Helpers;

[Serializable]
public class GetEntriesException : Exception
{
    public GetEntriesException(string? message) : base(message)
    {
    }

    protected GetEntriesException(SerializationInfo info, StreamingContext context) : base()
    {
    }
}
