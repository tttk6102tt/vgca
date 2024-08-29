namespace Sign.itext.text.pdf.security
{
    public interface IExternalSignature
    {
        string GetHashAlgorithm();

        string GetEncryptionAlgorithm();

        byte[] Sign(byte[] message);
    }
}
