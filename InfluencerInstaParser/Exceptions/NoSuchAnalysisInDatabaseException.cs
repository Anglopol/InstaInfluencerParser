using System;

namespace InfluencerInstaParser.Exceptions
{
    public class NoSuchAnalysisInDatabaseException : Exception
    {
        public NoSuchAnalysisInDatabaseException() : base()
        {
        }

        public NoSuchAnalysisInDatabaseException(string message) : base(message)
        {
        }

        public NoSuchAnalysisInDatabaseException(string message, System.Exception inner) : base(message, inner)
        {
        }

        protected NoSuchAnalysisInDatabaseException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}