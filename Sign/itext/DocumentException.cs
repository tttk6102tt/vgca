using System.Runtime.Serialization;

namespace Sign.itext
{
    [Serializable]
    public class DocumentException : Exception
    {
        public DocumentException()
        {
        }

        public DocumentException(string message)
            : base(message)
        {
        }

        protected DocumentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
