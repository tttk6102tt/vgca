using System.Runtime.Serialization;

namespace Sign.itext.text.exceptions
{
    [Serializable]
    public class UnsupportedPdfException : InvalidPdfException
    {
        public UnsupportedPdfException(string message)
            : base(message)
        {
        }

        protected UnsupportedPdfException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
