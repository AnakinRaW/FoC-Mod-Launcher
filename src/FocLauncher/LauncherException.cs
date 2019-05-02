using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace FocLauncher.Core
{
    [Serializable]
    public class LauncherException : Exception
    {
        internal const string LauncherDataModelKey = "LauncherDebugInformation";

        public string ResourceReferenceProperty { get; set; }
        public override string StackTrace => InnerException?.StackTrace;

        public LauncherException()
        {
        }

        public LauncherException(string message) : base(message)
        {
        }

        public LauncherException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LauncherException(SerializationInfo info, StreamingContext context)
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

        public override string ToString()
        {
            var sb = new StringBuilder();
            var debugInfo = Data[LauncherDataModelKey] as string;
            if (!string.IsNullOrEmpty(debugInfo))
            {
                sb.Append(debugInfo);
                sb.AppendLine();
                sb.AppendLine();
            }
            sb.Append(InnerException);
            return sb.ToString();
        }
    }
}
