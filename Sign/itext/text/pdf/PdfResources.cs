using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    internal class PdfResources : PdfDictionary
    {
        internal PdfResources()
        {
        }

        internal void Add(PdfName key, PdfDictionary resource)
        {
            if (resource.Size != 0)
            {
                PdfDictionary asDict = GetAsDict(key);
                if (asDict == null)
                {
                    Put(key, resource);
                }
                else
                {
                    asDict.Merge(resource);
                }
            }
        }
    }
}
