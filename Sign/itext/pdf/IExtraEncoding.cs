namespace Sign.itext.pdf
{
    public interface IExtraEncoding
    {
        byte[] CharToByte(string text, string encoding);

        byte[] CharToByte(char char1, string encoding);

        string ByteToChar(byte[] b, string encoding);
    }
}
