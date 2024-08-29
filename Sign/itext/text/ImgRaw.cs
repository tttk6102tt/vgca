using Sign.itext.error_messages;

namespace Sign.itext.text
{
    public class ImgRaw : Image
    {
        public ImgRaw(Image image)
            : base(image)
        {
        }

        public ImgRaw(int width, int height, int components, int bpc, byte[] data)
            : base((Uri)null)
        {
            type = 34;
            scaledHeight = height;
            Top = scaledHeight;
            scaledWidth = width;
            Right = scaledWidth;
            if (components != 1 && components != 3 && components != 4)
            {
                throw new BadElementException(MessageLocalization.GetComposedMessage("components.must.be.1.3.or.4"));
            }

            if (bpc != 1 && bpc != 2 && bpc != 4 && bpc != 8)
            {
                throw new BadElementException(MessageLocalization.GetComposedMessage("bits.per.component.must.be.1.2.4.or.8"));
            }

            colorspace = components;
            base.bpc = bpc;
            rawData = data;
            plainWidth = Width;
            plainHeight = Height;
        }
    }
}
