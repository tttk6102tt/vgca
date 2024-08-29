using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Engines;
using Sign.Org.BouncyCastle.Crypto.Modes;
using Sign.Org.BouncyCastle.Crypto.Parameters;

namespace Sign.itext.pdf.crypto
{
    public class AESCipherCBCnoPad
    {
        private IBlockCipher cbc;

        public AESCipherCBCnoPad(bool forEncryption, byte[] key)
        {
            IBlockCipher cipher = new AesFastEngine();
            cbc = new CbcBlockCipher(cipher);
            KeyParameter parameters = new KeyParameter(key);
            cbc.Init(forEncryption, parameters);
        }

        public virtual byte[] ProcessBlock(byte[] inp, int inpOff, int inpLen)
        {
            if (inpLen % cbc.GetBlockSize() != 0)
            {
                throw new ArgumentException("Not multiple of block: " + inpLen);
            }

            byte[] array = new byte[inpLen];
            int num = 0;
            while (inpLen > 0)
            {
                cbc.ProcessBlock(inp, inpOff, array, num);
                inpLen -= cbc.GetBlockSize();
                num += cbc.GetBlockSize();
                inpOff += cbc.GetBlockSize();
            }

            return array;
        }
    }
}
