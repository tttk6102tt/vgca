using Sign.Org.BouncyCastle.X509;

namespace Sign.itext.text.pdf.security
{
    public interface ICrlClient
    {
        ICollection<byte[]> GetEncoded(X509Certificate checkCert, string url);
    }
}
