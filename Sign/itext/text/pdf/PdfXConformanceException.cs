using System.Runtime.Serialization;

namespace Sign.itext.text.pdf
{
    [Serializable]
    public class PdfXConformanceException : PdfIsoConformanceException
    {
        public PdfXConformanceException()
        {
        }

        public PdfXConformanceException(string s)
            : base(s)
        {
        }

        protected PdfXConformanceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
