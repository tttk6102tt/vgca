using Sign.itext.pdf.security;

namespace Sign.itext.text
{
    public class ImgJBIG2 : Image
    {
        private byte[] global;

        private byte[] globalHash;

        public virtual byte[] GlobalBytes => global;

        public virtual byte[] GlobalHash => globalHash;

        private ImgJBIG2(Image image)
            : base(image)
        {
        }

        public ImgJBIG2()
            : base((Image)null)
        {
        }

        public ImgJBIG2(int width, int height, byte[] data, byte[] globals)
            : base((Uri)null)
        {
            type = 36;
            originalType = 9;
            scaledHeight = height;
            Top = scaledHeight;
            scaledWidth = width;
            Right = scaledWidth;
            bpc = 1;
            colorspace = 1;
            rawData = data;
            plainWidth = Width;
            plainHeight = Height;
            if (globals != null)
            {
                global = globals;
                try
                {
                    globalHash = DigestAlgorithms.Digest("MD5", global);
                }
                catch
                {
                }
            }
        }
    }
}
