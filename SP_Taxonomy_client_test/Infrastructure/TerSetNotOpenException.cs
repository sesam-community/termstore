using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SP_Taxonomy_client_test.Infrastructure
{
    [Serializable]
    internal class TerSetNotOpenException: Exception
    {
        public TerSetNotOpenException()
        {
        }

        public TerSetNotOpenException(string message) : base(message)
        {
        }

        public TerSetNotOpenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TerSetNotOpenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
