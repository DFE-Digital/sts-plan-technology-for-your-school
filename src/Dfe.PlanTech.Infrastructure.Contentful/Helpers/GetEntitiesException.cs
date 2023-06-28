using System.Runtime.Serialization;

namespace Dfe.PlanTech.Infrastructure.Contentful.Helpers
{
    [Serializable]
    public class GetEntitiesException : Exception
    {
        protected GetEntitiesException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            if (serializationInfo == null) throw new ArgumentNullException();
            
        }
        public GetEntitiesException(string? message) : base(message)
        {
        }
    }
}