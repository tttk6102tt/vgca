using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfReaderInstance
    {
        internal static PdfLiteral IDENTITYMATRIX = new PdfLiteral("[1 0 0 1 0 0]");

        internal static PdfNumber ONE = new PdfNumber(1);

        internal int[] myXref;

        internal PdfReader reader;

        internal RandomAccessFileOrArray file;

        internal Dictionary<int, PdfImportedPage> importedPages = new Dictionary<int, PdfImportedPage>();

        internal PdfWriter writer;

        internal Dictionary<int, object> visited = new Dictionary<int, object>();

        internal List<int> nextRound = new List<int>();

        internal PdfReader Reader => reader;

        internal RandomAccessFileOrArray ReaderFile => file;

        internal PdfReaderInstance(PdfReader reader, PdfWriter writer)
        {
            this.reader = reader;
            this.writer = writer;
            file = reader.SafeFile;
            myXref = new int[reader.XrefSize];
        }

        internal PdfImportedPage GetImportedPage(int pageNumber)
        {
            if (!reader.IsOpenedWithFullPermissions)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("pdfreader.not.opened.with.owner.password"));
            }

            if (pageNumber < 1 || pageNumber > reader.NumberOfPages)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.page.number.1", pageNumber));
            }

            if (!importedPages.TryGetValue(pageNumber, out var value))
            {
                value = new PdfImportedPage(this, writer, pageNumber);
                importedPages[pageNumber] = value;
            }

            return value;
        }

        internal int GetNewObjectNumber(int number, int generation)
        {
            if (myXref[number] == 0)
            {
                myXref[number] = writer.IndirectReferenceNumber;
                nextRound.Add(number);
            }

            return myXref[number];
        }

        internal PdfObject GetResources(int pageNumber)
        {
            return PdfReader.GetPdfObjectRelease(reader.GetPageNRelease(pageNumber).Get(PdfName.RESOURCES));
        }

        internal PdfStream GetFormXObject(int pageNumber, int compressionLevel)
        {
            PdfDictionary pageNRelease = reader.GetPageNRelease(pageNumber);
            PdfObject pdfObjectRelease = PdfReader.GetPdfObjectRelease(pageNRelease.Get(PdfName.CONTENTS));
            PdfDictionary pdfDictionary = new PdfDictionary();
            byte[] array = null;
            if (pdfObjectRelease != null)
            {
                if (pdfObjectRelease.IsStream())
                {
                    pdfDictionary.Merge((PRStream)pdfObjectRelease);
                }
                else
                {
                    array = reader.GetPageContent(pageNumber, file);
                }
            }
            else
            {
                array = new byte[0];
            }

            pdfDictionary.Put(PdfName.RESOURCES, PdfReader.GetPdfObjectRelease(pageNRelease.Get(PdfName.RESOURCES)));
            pdfDictionary.Put(PdfName.TYPE, PdfName.XOBJECT);
            pdfDictionary.Put(PdfName.SUBTYPE, PdfName.FORM);
            PdfImportedPage pdfImportedPage = importedPages[pageNumber];
            pdfDictionary.Put(PdfName.BBOX, new PdfRectangle(pdfImportedPage.BoundingBox));
            PdfArray matrix = pdfImportedPage.Matrix;
            if (matrix == null)
            {
                pdfDictionary.Put(PdfName.MATRIX, IDENTITYMATRIX);
            }
            else
            {
                pdfDictionary.Put(PdfName.MATRIX, matrix);
            }

            pdfDictionary.Put(PdfName.FORMTYPE, ONE);
            PRStream pRStream;
            if (array == null)
            {
                pRStream = new PRStream((PRStream)pdfObjectRelease, pdfDictionary);
            }
            else
            {
                pRStream = new PRStream(reader, array);
                pRStream.Merge(pdfDictionary);
            }

            return pRStream;
        }

        internal void WriteAllVisited()
        {
            while (nextRound.Count > 0)
            {
                List<int> list = nextRound;
                nextRound = new List<int>();
                foreach (int item in list)
                {
                    if (!visited.ContainsKey(item))
                    {
                        visited[item] = null;
                        writer.AddToBody(reader.GetPdfObjectRelease(item), myXref[item]);
                    }
                }
            }
        }

        public virtual void WriteAllPages()
        {
            try
            {
                file.ReOpen();
                foreach (PdfImportedPage value in importedPages.Values)
                {
                    if (value.IsToCopy())
                    {
                        writer.AddToBody(value.GetFormXObject(writer.CompressionLevel), value.IndirectReference);
                        value.SetCopied();
                    }
                }

                WriteAllVisited();
            }
            finally
            {
                try
                {
                    file.Close();
                }
                catch
                {
                }
            }
        }
    }
}
