using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf.codec
{
    public class JBIG2Image
    {
        public static byte[] GetGlobalSegment(RandomAccessFileOrArray ra)
        {
            try
            {
                JBIG2SegmentReader jBIG2SegmentReader = new JBIG2SegmentReader(ra);
                jBIG2SegmentReader.Read();
                return jBIG2SegmentReader.GetGlobal(for_embedding: true);
            }
            catch
            {
                return null;
            }
        }

        public static Image GetJbig2Image(RandomAccessFileOrArray ra, int page)
        {
            if (page < 1)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.page.number.must.be.gt.eq.1"));
            }

            JBIG2SegmentReader jBIG2SegmentReader = new JBIG2SegmentReader(ra);
            jBIG2SegmentReader.Read();
            JBIG2SegmentReader.JBIG2Page page2 = jBIG2SegmentReader.GetPage(page);
            return new ImgJBIG2(page2.pageBitmapWidth, page2.pageBitmapHeight, page2.GetData(for_embedding: true), jBIG2SegmentReader.GetGlobal(for_embedding: true));
        }

        public static int GetNumberOfPages(RandomAccessFileOrArray ra)
        {
            JBIG2SegmentReader jBIG2SegmentReader = new JBIG2SegmentReader(ra);
            jBIG2SegmentReader.Read();
            return jBIG2SegmentReader.NumberOfPages();
        }
    }
}
