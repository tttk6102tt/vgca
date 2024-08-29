

using System.Runtime.Serialization;

namespace Sign.itext.text
{
    [Serializable]
    public class BadElementException : DocumentException
    {
        public BadElementException()
        {
        }

        public BadElementException(string message)
            : base(message)
        {
        }

        protected BadElementException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
