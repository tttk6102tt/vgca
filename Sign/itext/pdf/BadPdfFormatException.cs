using System.Runtime.Serialization;

namespace Sign.itext.pdf
{
    [Serializable]
    public class BadPdfFormatException : Exception
    {
        public BadPdfFormatException()
        {
        }

        public BadPdfFormatException(string message)
            : base(message)
        {
        }

        protected BadPdfFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
