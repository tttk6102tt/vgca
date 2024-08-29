namespace Sign.Org.BouncyCastle.Asn1.Utilities.Encoders
{
    public interface IEncoder
    {
        int Encode(byte[] data, int off, int length, Stream outStream);

        int Decode(byte[] data, int off, int length, Stream outStream);

        int DecodeString(string data, Stream outStream);
    }
}
