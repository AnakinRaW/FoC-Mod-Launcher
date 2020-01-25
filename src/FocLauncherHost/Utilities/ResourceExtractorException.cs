using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FocLauncherHost.Utilities
{
    [Serializable]
    public class ResourceExtractorException : Exception
    {
        public string ResourceReferenceProperty { get; set; }

        public ResourceExtractorException()
        {
        }

        public ResourceExtractorException(string message) : base(message)
        {
        }

        public ResourceExtractorException(string message, Exception inner) : base(message, inner)
        {

        }

        protected ResourceExtractorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ResourceReferenceProperty = info.GetString("ResourceReferenceProperty");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            info.AddValue("ResourceReferenceProperty", ResourceReferenceProperty);
            base.GetObjectData(info, context);
        }
    }
}