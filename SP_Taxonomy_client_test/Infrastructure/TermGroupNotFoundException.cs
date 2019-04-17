using System;
using System.Runtime.Serialization;

namespace SP_Taxonomy_client_test.Infrastructure
{
    [Serializable]
    internal class TermGroupNotFoundException : Exception
    {
        public TermGroupNotFoundException()
        {
        }

        public TermGroupNotFoundException(string message) : base(message)
        {
        }

        public TermGroupNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TermGroupNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}