using System.Runtime.Serialization;

namespace Sign.itext.pdf
{
    [Serializable]
    public class PdfException : DocumentException
    {
        public PdfException()
        {
        }

        public PdfException(string message)
            : base(message)
        {
        }

        protected PdfException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
