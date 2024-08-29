using Sign.itext.io;

namespace Sign.itext.pdf.fonts.cmaps
{
    public class CidLocationFromByte : ICidLocation
    {
        private byte[] data;

        public CidLocationFromByte(byte[] data)
        {
            this.data = data;
        }

        public virtual PRTokeniser GetLocation(string location)
        {
            return new PRTokeniser(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(data)));
        }
    }
}
