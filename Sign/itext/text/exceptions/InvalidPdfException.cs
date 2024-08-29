using System.Runtime.Serialization;

namespace Sign.itext.text.exceptions
{
    [Serializable]
    public class InvalidPdfException : IOException
    {
        private readonly Exception cause;

        public InvalidPdfException(string message)
            : base(message)
        {
        }

        public InvalidPdfException(string message, Exception cause)
            : base(message, cause)
        {
            this.cause = cause;
        }

        protected InvalidPdfException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
