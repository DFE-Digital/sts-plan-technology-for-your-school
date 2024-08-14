using System.Runtime.Serialization;

namespace Dfe.PlanTech.Infrastructure.Contentful.Helpers;

[Serializable]
public class GetEntitiesException(string? message) : Exception(message)
{

    protected GetEntitiesException(SerializationInfo info, StreamingContext context) : this(null)
    {
    }
}
