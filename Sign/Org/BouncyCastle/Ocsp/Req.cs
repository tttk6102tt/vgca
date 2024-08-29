using Sign.Org.BouncyCastle.Asn1.Ocsp;
using Sign.Org.BouncyCastle.Asn1.X509;

namespace Sign.Org.BouncyCastle.Ocsp
{
    public class Req : X509ExtensionBase
    {
        private Request req;

        public X509Extensions SingleRequestExtensions => req.SingleRequestExtensions;

        public Req(Request req)
        {
            this.req = req;
        }

        public CertificateID GetCertID()
        {
            return new CertificateID(req.ReqCert);
        }

        protected override X509Extensions GetX509Extensions()
        {
            return SingleRequestExtensions;
        }
    }
}
