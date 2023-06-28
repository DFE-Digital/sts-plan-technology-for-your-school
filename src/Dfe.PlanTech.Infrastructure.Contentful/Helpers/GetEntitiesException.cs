using System.Runtime.Serialization;

namespace Dfe.PlanTech.Infrastructure.Contentful.Helpers
{
    [Serializable]
    public class GetEntitiesException : Exception, ISerializable
    {
        public GetEntitiesException()
        {
            
        }

        public GetEntitiesException(string? message) : base(message)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}