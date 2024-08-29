using Sign.itext.text.pdf.security;
using Sign.Org.BouncyCastle.X509;

namespace Sign.itext.text.pdf
{
    public class XmlSignatureAppearance
    {
        private PdfStamperImp writer;

        private PdfStamper stamper;

        private X509Certificate signCertificate;

        private IXmlLocator xmlLocator;

        private IXpathConstructor xpathConstructor;

        private DateTime signDate = DateTime.MinValue;

        private string description;

        private string mimeType = "text/xml";

        internal XmlSignatureAppearance(PdfStamperImp writer)
        {
            this.writer = writer;
        }

        public virtual PdfStamperImp GetWriter()
        {
            return writer;
        }

        public virtual PdfStamper GetStamper()
        {
            return stamper;
        }

        public virtual void SetStamper(PdfStamper stamper)
        {
            this.stamper = stamper;
        }

        public virtual void SetCertificate(X509Certificate signCertificate)
        {
            this.signCertificate = signCertificate;
        }

        public virtual X509Certificate GetCertificate()
        {
            return signCertificate;
        }

        public virtual void SetDescription(string description)
        {
            this.description = description;
        }

        public virtual string GetDescription()
        {
            return description;
        }

        public virtual string GetMimeType()
        {
            return mimeType;
        }

        public virtual void SetMimeType(string mimeType)
        {
            this.mimeType = mimeType;
        }

        public virtual DateTime GetSignDate()
        {
            if (signDate == DateTime.MinValue)
            {
                signDate = DateTime.Now;
            }

            return signDate;
        }

        public virtual void SetSignDate(DateTime signDate)
        {
            this.signDate = signDate;
        }

        public virtual IXmlLocator GetXmlLocator()
        {
            return xmlLocator;
        }

        public virtual void SetXmlLocator(IXmlLocator xmlLocator)
        {
            this.xmlLocator = xmlLocator;
        }

        public virtual IXpathConstructor GetXpathConstructor()
        {
            return xpathConstructor;
        }

        public virtual void SetXpathConstructor(IXpathConstructor xpathConstructor)
        {
            this.xpathConstructor = xpathConstructor;
        }

        public virtual void Close()
        {
            writer.Close(stamper.MoreInfo);
        }
    }
}
