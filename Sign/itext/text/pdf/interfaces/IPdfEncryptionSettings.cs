using Sign.Org.BouncyCastle.X509;

namespace Sign.itext.text.pdf.interfaces
{
    public interface IPdfEncryptionSettings
    {
        void SetEncryption(byte[] userPassword, byte[] ownerPassword, int permissions, int encryptionType);

        void SetEncryption(X509Certificate[] certs, int[] permissions, int encryptionType);
    }
}
