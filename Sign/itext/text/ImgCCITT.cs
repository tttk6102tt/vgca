using Sign.itext.error_messages;
using Sign.itext.text.pdf.codec;

namespace Sign.itext.text
{
    public class ImgCCITT : Image
    {
        public ImgCCITT(Image image)
            : base(image)
        {
        }

        public ImgCCITT(int width, int height, bool reverseBits, int typeCCITT, int parameters, byte[] data)
            : base((Uri)null)
        {
            if (typeCCITT != 256 && typeCCITT != 257 && typeCCITT != 258)
            {
                throw new BadElementException(MessageLocalization.GetComposedMessage("the.ccitt.compression.type.must.be.ccittg4.ccittg3.1d.or.ccittg3.2d"));
            }

            if (reverseBits)
            {
                TIFFFaxDecoder.ReverseBits(data);
            }

            type = 34;
            scaledHeight = height;
            Top = scaledHeight;
            scaledWidth = width;
            Right = scaledWidth;
            colorspace = parameters;
            bpc = typeCCITT;
            rawData = data;
            plainWidth = Width;
            plainHeight = Height;
        }
    }
}
