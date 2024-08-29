using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfICCBased : PdfStream
    {
        public PdfICCBased(ICC_Profile profile)
            : this(profile, -1)
        {
        }

        public PdfICCBased(ICC_Profile profile, int compressionLevel)
        {
            int numComponents = profile.NumComponents;
            switch (numComponents)
            {
                case 1:
                    Put(PdfName.ALTERNATE, PdfName.DEVICEGRAY);
                    break;
                case 3:
                    Put(PdfName.ALTERNATE, PdfName.DEVICERGB);
                    break;
                case 4:
                    Put(PdfName.ALTERNATE, PdfName.DEVICECMYK);
                    break;
                default:
                    throw new PdfException(MessageLocalization.GetComposedMessage("1.component.s.is.not.supported", numComponents));
            }

            Put(PdfName.N, new PdfNumber(numComponents));
            bytes = profile.Data;
            Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
            FlateCompress(compressionLevel);
        }
    }
}
