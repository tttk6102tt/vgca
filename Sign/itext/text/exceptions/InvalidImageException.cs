using System.Runtime.Serialization;

namespace Sign.itext.text.exceptions
{
    [Serializable]
    public class InvalidImageException : Exception
    {
        public InvalidImageException()
        {
        }

        public InvalidImageException(string message)
            : base(message)
        {
        }

        public InvalidImageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidImageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
