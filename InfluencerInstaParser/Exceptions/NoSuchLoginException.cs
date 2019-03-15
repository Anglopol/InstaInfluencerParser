using System;

namespace InfluencerInstaParser.Exceptions
{
    public class NoSuchLoginException : Exception
    {
        public NoSuchLoginException() : base()
        {
        }

        public NoSuchLoginException(string message) : base(message)
        {
        }

        public NoSuchLoginException(string message, System.Exception inner) : base(message, inner)
        {
        }

        protected NoSuchLoginException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}