using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Crypto.Paddings
{
    public interface IBlockCipherPadding
    {
        string PaddingName { get; }

        void Init(SecureRandom random);

        int AddPadding(byte[] input, int inOff);

        int PadCount(byte[] input);
    }
}
