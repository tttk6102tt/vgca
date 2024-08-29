using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfRendition : PdfDictionary
    {
        public PdfRendition(string file, PdfFileSpecification fs, string mimeType)
        {
            Put(PdfName.S, new PdfName("MR"));
            Put(PdfName.N, new PdfString("Rendition for " + file));
            Put(PdfName.C, new PdfMediaClipData(file, fs, mimeType));
        }
    }
}
