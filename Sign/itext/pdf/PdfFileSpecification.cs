using Sign.itext.error_messages;
using Sign.itext.io;
using Sign.itext.text.pdf;
using Sign.itext.text.pdf.collection;
using System.Net;

namespace Sign.itext.pdf
{
    public class PdfFileSpecification : PdfDictionary
    {
        protected PdfWriter writer;

        protected PdfIndirectReference refi;

        public virtual PdfIndirectReference Reference
        {
            get
            {
                if (refi != null)
                {
                    return refi;
                }

                refi = writer.AddToBody(this).IndirectReference;
                return refi;
            }
        }

        public virtual byte[] MultiByteFileName
        {
            set
            {
                Put(PdfName.F, new PdfString(value).SetHexWriting(hexWriting: true));
            }
        }

        public virtual bool Volatile
        {
            set
            {
                Put(PdfName.V, new PdfBoolean(value));
            }
        }

        public PdfFileSpecification()
            : base(PdfName.FILESPEC)
        {
        }

        public static PdfFileSpecification Url(PdfWriter writer, string url)
        {
            PdfFileSpecification pdfFileSpecification = new PdfFileSpecification();
            pdfFileSpecification.writer = writer;
            pdfFileSpecification.Put(PdfName.FS, PdfName.URL);
            pdfFileSpecification.Put(PdfName.F, new PdfString(url));
            return pdfFileSpecification;
        }

        public static PdfFileSpecification FileEmbedded(PdfWriter writer, string filePath, string fileDisplay, byte[] fileStore)
        {
            return FileEmbedded(writer, filePath, fileDisplay, fileStore, 9);
        }

        public static PdfFileSpecification FileEmbedded(PdfWriter writer, string filePath, string fileDisplay, byte[] fileStore, int compressionLevel)
        {
            return FileEmbedded(writer, filePath, fileDisplay, fileStore, null, null, compressionLevel);
        }

        public static PdfFileSpecification FileEmbedded(PdfWriter writer, string filePath, string fileDisplay, byte[] fileStore, bool compress)
        {
            return FileEmbedded(writer, filePath, fileDisplay, fileStore, null, null, compress ? 9 : 0);
        }

        public static PdfFileSpecification FileEmbedded(PdfWriter writer, string filePath, string fileDisplay, byte[] fileStore, bool compress, string mimeType, PdfDictionary fileParameter)
        {
            return FileEmbedded(writer, filePath, fileDisplay, fileStore, mimeType, fileParameter, compress ? 9 : 0);
        }

        public static PdfFileSpecification FileEmbedded(PdfWriter writer, string filePath, string fileDisplay, byte[] fileStore, string mimeType, PdfDictionary fileParameter, int compressionLevel)
        {
            PdfFileSpecification pdfFileSpecification = new PdfFileSpecification();
            pdfFileSpecification.writer = writer;
            pdfFileSpecification.Put(PdfName.F, new PdfString(fileDisplay));
            pdfFileSpecification.SetUnicodeFileName(fileDisplay, unicode: false);
            Stream stream = null;
            PdfIndirectReference pdfIndirectReference = null;
            PdfIndirectReference indirectReference;
            try
            {
                PdfEFStream pdfEFStream;
                if (fileStore == null)
                {
                    pdfIndirectReference = writer.PdfIndirectReference;
                    if (File.Exists(filePath))
                    {
                        stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    }
                    else if (filePath.StartsWith("file:/") || filePath.StartsWith("http://") || filePath.StartsWith("https://"))
                    {
                        WebRequest webRequest = WebRequest.Create(filePath);
                        webRequest.Credentials = CredentialCache.DefaultCredentials;
                        stream = webRequest.GetResponse().GetResponseStream();
                    }
                    else
                    {
                        stream = StreamUtil.GetResourceStream(filePath);
                        if (stream == null)
                        {
                            throw new IOException(MessageLocalization.GetComposedMessage("1.not.found.as.file.or.resource", filePath));
                        }
                    }

                    pdfEFStream = new PdfEFStream(stream, writer);
                }
                else
                {
                    pdfEFStream = new PdfEFStream(fileStore);
                }

                pdfEFStream.Put(PdfName.TYPE, PdfName.EMBEDDEDFILE);
                pdfEFStream.FlateCompress(compressionLevel);
                PdfDictionary pdfDictionary = new PdfDictionary();
                if (fileParameter != null)
                {
                    pdfDictionary.Merge(fileParameter);
                }

                if (!pdfDictionary.Contains(PdfName.MODDATE))
                {
                    pdfDictionary.Put(PdfName.MODDATE, new PdfDate());
                }

                if (fileStore == null)
                {
                    pdfEFStream.Put(PdfName.PARAMS, pdfIndirectReference);
                }
                else
                {
                    pdfDictionary.Put(PdfName.SIZE, new PdfNumber(pdfEFStream.RawLength));
                    pdfEFStream.Put(PdfName.PARAMS, pdfDictionary);
                }

                if (mimeType != null)
                {
                    pdfEFStream.Put(PdfName.SUBTYPE, new PdfName(mimeType));
                }

                indirectReference = writer.AddToBody(pdfEFStream).IndirectReference;
                if (fileStore == null)
                {
                    pdfEFStream.WriteLength();
                    pdfDictionary.Put(PdfName.SIZE, new PdfNumber(pdfEFStream.RawLength));
                    writer.AddToBody(pdfDictionary, pdfIndirectReference);
                }
            }
            finally
            {
                if (stream != null)
                {
                    try
                    {
                        stream.Close();
                    }
                    catch
                    {
                    }
                }
            }

            PdfDictionary pdfDictionary2 = new PdfDictionary();
            pdfDictionary2.Put(PdfName.F, indirectReference);
            pdfDictionary2.Put(PdfName.UF, indirectReference);
            pdfFileSpecification.Put(PdfName.EF, pdfDictionary2);
            return pdfFileSpecification;
        }

        public static PdfFileSpecification FileExtern(PdfWriter writer, string filePath)
        {
            PdfFileSpecification pdfFileSpecification = new PdfFileSpecification();
            pdfFileSpecification.writer = writer;
            pdfFileSpecification.Put(PdfName.F, new PdfString(filePath));
            return pdfFileSpecification;
        }

        public virtual void SetUnicodeFileName(string filename, bool unicode)
        {
            Put(PdfName.UF, new PdfString(filename, unicode ? "UnicodeBig" : "PDF"));
        }

        public virtual void AddDescription(string description, bool unicode)
        {
            Put(PdfName.DESC, new PdfString(description, unicode ? "UnicodeBig" : "PDF"));
        }

        public virtual void AddCollectionItem(PdfCollectionItem ci)
        {
            Put(PdfName.CI, ci);
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            PdfWriter.CheckPdfIsoConformance(writer, 10, this);
            base.ToPdf(writer, os);
        }
    }
}
