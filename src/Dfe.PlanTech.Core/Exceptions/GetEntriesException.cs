using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Dfe.PlanTech.Core.Exceptions;

[Serializable]
[ExcludeFromCodeCoverage]
public class GetEntriesException : Exception
{
    public GetEntriesException(string? message) : base(message)
    {
    }

    protected GetEntriesException(SerializationInfo info, StreamingContext context) : base()
    {
    }
}
