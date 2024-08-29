using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfMediaClipData : PdfDictionary
    {
        internal PdfMediaClipData(string file, PdfFileSpecification fs, string mimeType)
        {
            Put(PdfName.TYPE, new PdfName("MediaClip"));
            Put(PdfName.S, new PdfName("MCD"));
            Put(PdfName.N, new PdfString("Media clip for " + file));
            Put(new PdfName("CT"), new PdfString(mimeType));
            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(new PdfName("TF"), new PdfString("TEMPACCESS"));
            Put(new PdfName("P"), pdfDictionary);
            Put(PdfName.D, fs.Reference);
        }
    }
}
