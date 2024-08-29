using System.Runtime.Serialization;

namespace Sign.itext.text.exceptions
{
    [Serializable]
    public class IllegalPdfSyntaxException : ArgumentException
    {
        public IllegalPdfSyntaxException(string message)
            : base(message)
        {
        }

        protected IllegalPdfSyntaxException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
