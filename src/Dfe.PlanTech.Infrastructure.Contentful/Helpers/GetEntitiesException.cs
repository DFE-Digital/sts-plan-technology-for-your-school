using System.Runtime.Serialization;

namespace Dfe.PlanTech.Infrastructure.Contentful.Helpers
{
    [Serializable]
    public class GetEntitiesException : Exception, ISerializable
    {
        public GetEntitiesException(string? message) : base(message)
        {
        }

        public GetEntitiesException()
        {
        }

        public static GetEntitiesException Create(string? message)
        {
            return new GetEntitiesException(message);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        protected GetEntitiesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}