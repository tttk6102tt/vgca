using Sign.itext.error_messages;
using Sign.itext.io;

namespace Sign.itext.pdf.fonts.cmaps
{
    public class CidResource : ICidLocation
    {
        public virtual PRTokeniser GetLocation(string location)
        {
            string text = "text.pdf.fonts.cmaps." + location;
            Stream resourceStream = StreamUtil.GetResourceStream(text);
            if (resourceStream == null)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("the.cmap.1.was.not.found", text));
            }

            return new PRTokeniser(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(resourceStream)));
        }
    }
}
