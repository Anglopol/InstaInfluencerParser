using System;

namespace InfluencerInstaParser.Exceptions
{
    public class ProxyNotInitializeException : Exception
    {
        public ProxyNotInitializeException() : base()
        {
        }

        public ProxyNotInitializeException(string message) : base(message)
        {
        }

        public ProxyNotInitializeException(string message, System.Exception inner) : base(message, inner)
        {
        }

        protected ProxyNotInitializeException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}