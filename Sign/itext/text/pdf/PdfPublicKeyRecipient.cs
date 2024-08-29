using Sign.Org.BouncyCastle.X509;

namespace Sign.itext.text.pdf
{
    public class PdfPublicKeyRecipient
    {
        private X509Certificate certificate;

        private int permission;

        protected byte[] cms;

        public virtual X509Certificate Certificate => certificate;

        public virtual int Permission => permission;

        protected internal virtual byte[] Cms
        {
            get
            {
                return cms;
            }
            set
            {
                cms = value;
            }
        }

        public PdfPublicKeyRecipient(X509Certificate certificate, int permission)
        {
            this.certificate = certificate;
            this.permission = permission;
        }
    }
}
