using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfBorderDictionary : PdfDictionary
    {
        public const int STYLE_SOLID = 0;

        public const int STYLE_DASHED = 1;

        public const int STYLE_BEVELED = 2;

        public const int STYLE_INSET = 3;

        public const int STYLE_UNDERLINE = 4;

        public PdfBorderDictionary(float borderWidth, int borderStyle, PdfDashPattern dashes)
        {
            Put(PdfName.W, new PdfNumber(borderWidth));
            switch (borderStyle)
            {
                case 0:
                    Put(PdfName.S, PdfName.S);
                    break;
                case 1:
                    if (dashes != null)
                    {
                        Put(PdfName.D, dashes);
                    }

                    Put(PdfName.S, PdfName.D);
                    break;
                case 2:
                    Put(PdfName.S, PdfName.B);
                    break;
                case 3:
                    Put(PdfName.S, PdfName.I);
                    break;
                case 4:
                    Put(PdfName.S, PdfName.U);
                    break;
                default:
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.border.style"));
            }
        }

        public PdfBorderDictionary(float borderWidth, int borderStyle)
            : this(borderWidth, borderStyle, null)
        {
        }
    }
}
