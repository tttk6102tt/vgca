using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Crypto.Paddings
{
    public class Pkcs7Padding : IBlockCipherPadding
    {
        public string PaddingName => "PKCS7";

        public void Init(SecureRandom random)
        {
        }

        public int AddPadding(byte[] input, int inOff)
        {
            byte b = (byte)(input.Length - inOff);
            while (inOff < input.Length)
            {
                input[inOff] = b;
                inOff++;
            }

            return b;
        }

        public int PadCount(byte[] input)
        {
            int num = input[^1];
            if (num < 1 || num > input.Length)
            {
                throw new InvalidCipherTextException("pad block corrupted");
            }

            for (int i = 1; i <= num; i++)
            {
                if (input[^i] != num)
                {
                    throw new InvalidCipherTextException("pad block corrupted");
                }
            }

            return num;
        }
    }
}
