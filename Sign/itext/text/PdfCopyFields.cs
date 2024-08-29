using Sign.itext.pdf;
using Sign.itext.text.pdf;
using Sign.itext.text.pdf.interfaces;
using Sign.Org.BouncyCastle.X509;

namespace Sign.itext.text
{
    [Obsolete]
    public class PdfCopyFields : IPdfViewerPreferences, IPdfEncryptionSettings
    {
        private PdfCopyFieldsImp fc;

        public virtual IList<Dictionary<string, object>> Outlines
        {
            set
            {
                fc.Outlines = value;
            }
        }

        public virtual PdfWriter Writer => fc;

        public virtual bool FullCompression => fc.FullCompression;

        public virtual int ViewerPreferences
        {
            set
            {
                fc.ViewerPreferences = value;
            }
        }

        public PdfCopyFields(Stream os)
        {
            fc = new PdfCopyFieldsImp(os);
        }

        public PdfCopyFields(Stream os, char pdfVersion)
        {
            fc = new PdfCopyFieldsImp(os, pdfVersion);
        }

        public virtual void AddDocument(PdfReader reader)
        {
            fc.AddDocument(reader);
        }

        public virtual void AddDocument(PdfReader reader, IList<int> pagesToKeep)
        {
            fc.AddDocument(reader, pagesToKeep);
        }

        public virtual void AddDocument(PdfReader reader, string ranges)
        {
            fc.AddDocument(reader, SequenceList.Expand(ranges, reader.NumberOfPages));
        }

        public virtual void SetEncryption(byte[] userPassword, byte[] ownerPassword, int permissions, bool strength128Bits)
        {
            fc.SetEncryption(userPassword, ownerPassword, permissions, strength128Bits ? 1 : 0);
        }

        public virtual void SetEncryption(bool strength, string userPassword, string ownerPassword, int permissions)
        {
            SetEncryption(DocWriter.GetISOBytes(userPassword), DocWriter.GetISOBytes(ownerPassword), permissions, strength);
        }

        public virtual void Close()
        {
            fc.Close();
        }

        public virtual void Open()
        {
            fc.OpenDoc();
        }

        public virtual void AddJavaScript(string js)
        {
            fc.AddJavaScript(js, !PdfEncodings.IsPdfDocEncoding(js));
        }

        public virtual void SetFullCompression()
        {
            fc.SetFullCompression();
        }

        public virtual void SetEncryption(byte[] userPassword, byte[] ownerPassword, int permissions, int encryptionType)
        {
            fc.SetEncryption(userPassword, ownerPassword, permissions, encryptionType);
        }

        public virtual void AddViewerPreference(PdfName key, PdfObject value)
        {
            fc.AddViewerPreference(key, value);
        }

        public virtual void SetEncryption(X509Certificate[] certs, int[] permissions, int encryptionType)
        {
            fc.SetEncryption(certs, permissions, encryptionType);
        }
    }
}
