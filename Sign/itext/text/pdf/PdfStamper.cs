using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.text.pdf.collection;
using Sign.itext.text.pdf.interfaces;
using Sign.itext.text.pdf.security;
using Sign.itext.xml.xmp;
using Sign.Org.BouncyCastle.X509;

namespace Sign.itext.text.pdf
{
    public class PdfStamper : IPdfViewerPreferences, IPdfEncryptionSettings, IDisposable
    {
        protected PdfStamperImp stamper;

        private IDictionary<string, string> moreInfo;

        protected internal bool hasSignature;

        protected PdfSignatureAppearance sigApp;

        protected XmlSignatureAppearance sigXmlApp;

        private LtvVerification verification;

        public virtual IDictionary<string, string> MoreInfo
        {
            get
            {
                return moreInfo;
            }
            set
            {
                moreInfo = value;
            }
        }

        public virtual PdfSignatureAppearance SignatureAppearance => sigApp;

        public virtual XmlSignatureAppearance XmlSignatureAppearance => sigXmlApp;

        public virtual bool RotateContents
        {
            get
            {
                return stamper.RotateContents;
            }
            set
            {
                stamper.RotateContents = value;
            }
        }

        public virtual PdfWriter Writer => stamper;

        public virtual PdfReader Reader => stamper.reader;

        public virtual AcroFields AcroFields => stamper.GetAcroFields();

        public virtual bool FormFlattening
        {
            set
            {
                stamper.FormFlattening = value;
            }
        }

        public virtual bool FreeTextFlattening
        {
            set
            {
                stamper.FreeTextFlattening = value;
            }
        }

        public virtual bool AnnotationFlattening
        {
            set
            {
                stamper.FlatAnnotations = value;
            }
        }

        public virtual IList<Dictionary<string, object>> Outlines
        {
            set
            {
                stamper.Outlines = value;
            }
        }

        public virtual string JavaScript
        {
            set
            {
                stamper.AddJavaScript(value, !PdfEncodings.IsPdfDocEncoding(value));
            }
        }

        public virtual int ViewerPreferences
        {
            set
            {
                stamper.ViewerPreferences = value;
            }
        }

        public virtual byte[] XmpMetadata
        {
            set
            {
                stamper.XmpMetadata = value;
            }
        }

        public virtual XmpWriter XmpWriter => stamper.XmpWriter;

        public virtual bool FullCompression => stamper.FullCompression;

        public virtual LtvVerification LtvVerification
        {
            get
            {
                if (verification == null)
                {
                    verification = new LtvVerification(this);
                }

                return verification;
            }
        }

        public PdfStamper(PdfReader reader, Stream os)
        {
            stamper = new PdfStamperImp(reader, os, '\0', append: false);
        }

        public PdfStamper(PdfReader reader, Stream os, char pdfVersion)
        {
            stamper = new PdfStamperImp(reader, os, pdfVersion, append: false);
        }

        public PdfStamper(PdfReader reader, Stream os, char pdfVersion, bool append)
        {
            stamper = new PdfStamperImp(reader, os, pdfVersion, append);
        }

        protected PdfStamper()
        {
        }

        public virtual void ReplacePage(PdfReader r, int pageImported, int pageReplaced)
        {
            stamper.ReplacePage(r, pageImported, pageReplaced);
        }

        public virtual void InsertPage(int pageNumber, Rectangle mediabox)
        {
            stamper.InsertPage(pageNumber, mediabox);
        }

        public virtual void Close()
        {
            if (!stamper.closed)
            {
                if (hasSignature)
                {
                    throw new DocumentException("Signature defined. Must be closed in PdfSignatureAppearance.");
                }

                MergeVerification();
                stamper.Close(moreInfo);
            }
        }

        public virtual PdfContentByte GetUnderContent(int pageNum)
        {
            return stamper.GetUnderContent(pageNum);
        }

        public virtual PdfContentByte GetOverContent(int pageNum)
        {
            return stamper.GetOverContent(pageNum);
        }

        public virtual void SetEncryption(byte[] userPassword, byte[] ownerPassword, int permissions, bool strength128Bits)
        {
            if (stamper.append)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("append.mode.does.not.support.changing.the.encryption.status"));
            }

            if (stamper.ContentWritten)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("content.was.already.written.to.the.output"));
            }

            stamper.SetEncryption(userPassword, ownerPassword, permissions, strength128Bits ? 1 : 0);
        }

        public virtual void SetEncryption(byte[] userPassword, byte[] ownerPassword, int permissions, int encryptionType)
        {
            if (stamper.IsAppend())
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("append.mode.does.not.support.changing.the.encryption.status"));
            }

            if (stamper.ContentWritten)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("content.was.already.written.to.the.output"));
            }

            stamper.SetEncryption(userPassword, ownerPassword, permissions, encryptionType);
        }

        public virtual void SetEncryption(bool strength, string userPassword, string ownerPassword, int permissions)
        {
            SetEncryption(DocWriter.GetISOBytes(userPassword), DocWriter.GetISOBytes(ownerPassword), permissions, strength);
        }

        public virtual void SetEncryption(int encryptionType, string userPassword, string ownerPassword, int permissions)
        {
            SetEncryption(DocWriter.GetISOBytes(userPassword), DocWriter.GetISOBytes(ownerPassword), permissions, encryptionType);
        }

        public virtual void SetEncryption(X509Certificate[] certs, int[] permissions, int encryptionType)
        {
            if (stamper.IsAppend())
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("append.mode.does.not.support.changing.the.encryption.status"));
            }

            if (stamper.ContentWritten)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("content.was.already.written.to.the.output"));
            }

            stamper.SetEncryption(certs, permissions, encryptionType);
        }

        public virtual PdfImportedPage GetImportedPage(PdfReader reader, int pageNumber)
        {
            return stamper.GetImportedPage(reader, pageNumber);
        }

        public virtual void AddAnnotation(PdfAnnotation annot, int page)
        {
            stamper.AddAnnotation(annot, page);
        }

        public virtual PdfFormField AddSignature(string name, int page, float llx, float lly, float urx, float ury)
        {
            PdfAcroForm acroForm = stamper.AcroForm;
            PdfFormField pdfFormField = PdfFormField.CreateSignature(stamper);
            acroForm.SetSignatureParams(pdfFormField, name, llx, lly, urx, ury);
            acroForm.DrawSignatureAppearences(pdfFormField, llx, lly, urx, ury);
            AddAnnotation(pdfFormField, page);
            return pdfFormField;
        }

        public virtual void AddComments(FdfReader fdf)
        {
            stamper.AddComments(fdf);
        }

        public virtual void SetThumbnail(Image image, int page)
        {
            stamper.SetThumbnail(image, page);
        }

        public virtual bool PartialFormFlattening(string name)
        {
            return stamper.PartialFormFlattening(name);
        }

        public virtual void AddJavaScript(string name, string js)
        {
            stamper.AddJavaScript(name, PdfAction.JavaScript(js, stamper, !PdfEncodings.IsPdfDocEncoding(js)));
        }

        public virtual void AddFileAttachment(string description, byte[] fileStore, string file, string fileDisplay)
        {
            AddFileAttachment(description, PdfFileSpecification.FileEmbedded(stamper, file, fileDisplay, fileStore));
        }

        public virtual void AddFileAttachment(string description, PdfFileSpecification fs)
        {
            stamper.AddFileAttachment(description, fs);
        }

        public virtual void MakePackage(PdfName initialView)
        {
            PdfCollection pdfCollection = new PdfCollection(0);
            pdfCollection.Put(PdfName.VIEW, initialView);
            stamper.MakePackage(pdfCollection);
        }

        public virtual void MakePackage(PdfCollection collection)
        {
            stamper.MakePackage(collection);
        }

        public virtual void AddViewerPreference(PdfName key, PdfObject value)
        {
            stamper.AddViewerPreference(key, value);
        }

        public virtual void CreateXmpMetadata()
        {
            stamper.CreateXmpMetadata();
        }

        public virtual void SetFullCompression()
        {
            if (!stamper.append)
            {
                stamper.fullCompression = true;
                stamper.SetAtLeastPdfVersion('5');
            }
        }

        public virtual void SetPageAction(PdfName actionType, PdfAction action, int page)
        {
            stamper.SetPageAction(actionType, action, page);
        }

        public virtual void SetDuration(int seconds, int page)
        {
            stamper.SetDuration(seconds, page);
        }

        public virtual void SetTransition(PdfTransition transition, int page)
        {
            stamper.SetTransition(transition, page);
        }

        public static PdfStamper CreateSignature(PdfReader reader, Stream os, char pdfVersion, string tempFile, bool append)
        {
            PdfStamper pdfStamper;
            if (tempFile == null)
            {
                ByteBuffer byteBuffer = new ByteBuffer();
                pdfStamper = new PdfStamper(reader, byteBuffer, pdfVersion, append);
                pdfStamper.sigApp = new PdfSignatureAppearance(pdfStamper.stamper);
                pdfStamper.sigApp.Sigout = byteBuffer;
            }
            else
            {
                if (Directory.Exists(tempFile))
                {
                    tempFile = Path.GetTempFileName();
                }

                FileStream os2 = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
                pdfStamper = new PdfStamper(reader, os2, pdfVersion, append);
                pdfStamper.sigApp = new PdfSignatureAppearance(pdfStamper.stamper);
                pdfStamper.sigApp.SetTempFile(tempFile);
            }

            pdfStamper.sigApp.Originalout = os;
            pdfStamper.sigApp.SetStamper(pdfStamper);
            pdfStamper.hasSignature = true;
            PdfDictionary catalog = reader.Catalog;
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObject(catalog.Get(PdfName.ACROFORM), catalog);
            if (pdfDictionary != null)
            {
                pdfDictionary.Remove(PdfName.NEEDAPPEARANCES);
                pdfStamper.stamper.MarkUsed(pdfDictionary);
            }

            return pdfStamper;
        }

        public static PdfStamper CreateSignature(PdfReader reader, Stream os, char pdfVersion)
        {
            return CreateSignature(reader, os, pdfVersion, null, append: false);
        }

        public static PdfStamper CreateSignature(PdfReader reader, Stream os, char pdfVersion, string tempFile)
        {
            return CreateSignature(reader, os, pdfVersion, tempFile, append: false);
        }

        public static PdfStamper createXmlSignature(PdfReader reader, Stream os)
        {
            PdfStamper pdfStamper = new PdfStamper(reader, os);
            pdfStamper.sigXmlApp = new XmlSignatureAppearance(pdfStamper.stamper);
            pdfStamper.sigXmlApp.SetStamper(pdfStamper);
            return pdfStamper;
        }

        public virtual Dictionary<string, PdfLayer> GetPdfLayers()
        {
            return stamper.GetPdfLayers();
        }

        public virtual void Dispose()
        {
            Close();
        }

        public virtual void MarkUsed(PdfObject obj)
        {
            stamper.MarkUsed(obj);
        }

        internal void MergeVerification()
        {
            if (verification != null)
            {
                verification.Merge();
            }
        }
    }
}
