using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public class PdfLiteral : PdfObject
    {
        private long position;

        public virtual long Position => position;

        public virtual int PosLength
        {
            get
            {
                if (bytes != null)
                {
                    return bytes.Length;
                }

                return 0;
            }
        }

        public PdfLiteral(string text)
            : base(0, text)
        {
        }

        public PdfLiteral(byte[] b)
            : base(0, b)
        {
        }

        public PdfLiteral(int type, string text)
            : base(type, text)
        {
        }

        public PdfLiteral(int type, byte[] b)
            : base(type, b)
        {
        }

        public PdfLiteral(int size)
            : base(0, (byte[])null)
        {
            bytes = new byte[size];
            for (int i = 0; i < size; i++)
            {
                bytes[i] = 32;
            }
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            if (os is OutputStreamCounter)
            {
                position = ((OutputStreamCounter)os).Counter;
            }

            base.ToPdf(writer, os);
        }
    }
}
