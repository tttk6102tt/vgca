using Sign.Org.BouncyCastle.X509;

namespace Sign.itext.text.pdf.security
{
    public interface IOcspClient
    {
        byte[] GetEncoded(X509Certificate checkCert, X509Certificate rootCert, string url);
    }
}
