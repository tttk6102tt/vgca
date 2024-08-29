using System.Runtime.Serialization;

namespace Sign.itext.text.pdf
{
    [Serializable]
    public class PdfIsoConformanceException : Exception
    {
        private const long serialVersionUID = -8972376258066225871L;

        public PdfIsoConformanceException()
        {
        }

        public PdfIsoConformanceException(string s)
            : base(s)
        {
        }

        protected PdfIsoConformanceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
