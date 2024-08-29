namespace Sign.itext.xml.xmp
{
    public class PdfProperties
    {
        public static readonly string KEYWORDS = "Keywords";

        public static readonly string VERSION = "PDFVersion";

        public static readonly string PRODUCER = "Producer";

        public static readonly string PART = "part";

        public static void SetKeywords(IXmpMeta xmpMeta, string keywords)
        {
            xmpMeta.SetProperty("http://ns.adobe.com/pdf/1.3/", KEYWORDS, keywords);
        }

        public static void SetProducer(IXmpMeta xmpMeta, string producer)
        {
            xmpMeta.SetProperty("http://ns.adobe.com/pdf/1.3/", PRODUCER, producer);
        }

        public static void SetVersion(IXmpMeta xmpMeta, string version)
        {
            xmpMeta.SetProperty("http://ns.adobe.com/pdf/1.3/", VERSION, version);
        }
    }
}
