using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Engines;
using Sign.Org.BouncyCastle.Crypto.Modes;
using Sign.Org.BouncyCastle.Crypto.Paddings;
using Sign.Org.BouncyCastle.Crypto.Parameters;

namespace Sign.itext.text.pdf.crypto
{
    public class AESCipher
    {
        private PaddedBufferedBlockCipher bp;

        public AESCipher(bool forEncryption, byte[] key, byte[] iv)
        {
            IBlockCipher cipher = new CbcBlockCipher(new AesFastEngine());
            bp = new PaddedBufferedBlockCipher(cipher);
            ParametersWithIV parameters = new ParametersWithIV(new KeyParameter(key), iv);
            bp.Init(forEncryption, parameters);
        }

        public virtual byte[] Update(byte[] inp, int inpOff, int inpLen)
        {
            int updateOutputSize = bp.GetUpdateOutputSize(inpLen);
            byte[] array = null;
            if (updateOutputSize > 0)
            {
                array = new byte[updateOutputSize];
            }
            else
            {
                updateOutputSize = 0;
            }

            bp.ProcessBytes(inp, inpOff, inpLen, array, 0);
            return array;
        }

        public virtual byte[] DoFinal()
        {
            byte[] array = new byte[bp.GetOutputSize(0)];
            int num = 0;
            try
            {
                num = bp.DoFinal(array, 0);
            }
            catch
            {
                return array;
            }

            if (num != array.Length)
            {
                byte[] array2 = new byte[num];
                Array.Copy(array, 0, array2, 0, num);
                return array2;
            }

            return array;
        }
    }
}
