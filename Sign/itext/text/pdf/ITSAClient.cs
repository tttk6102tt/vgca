using Sign.Org.BouncyCastle.Crypto;

namespace Sign.itext.text.pdf
{
    public interface ITSAClient
    {
        int GetTokenSizeEstimate();

        IDigest GetMessageDigest();

        byte[] GetTimeStampToken(byte[] imprint);
    }
}
