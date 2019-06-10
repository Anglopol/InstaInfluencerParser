using System;

namespace InfluencerInstaParser.Exceptions
{
    public class NoSuchUserInDatabaseException : Exception
    {
        public NoSuchUserInDatabaseException() : base()
        {
        }

        public NoSuchUserInDatabaseException(string message) : base(message)
        {
        }

        public NoSuchUserInDatabaseException(string message, System.Exception inner) : base(message, inner)
        {
        }

        protected NoSuchUserInDatabaseException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}