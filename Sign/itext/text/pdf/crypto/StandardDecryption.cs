using Sign.itext.pdf.crypto;

namespace Sign.itext.text.pdf.crypto
{
    public class StandardDecryption
    {
        protected ARCFOUREncryption arcfour;

        protected AESCipher cipher;

        private byte[] key;

        private const int AES_128 = 4;

        private const int AES_256 = 5;

        private bool aes;

        private bool initiated;

        private byte[] iv = new byte[16];

        private int ivptr;

        public StandardDecryption(byte[] key, int off, int len, int revision)
        {
            aes = revision == 4 || revision == 5;
            if (aes)
            {
                this.key = new byte[len];
                Array.Copy(key, off, this.key, 0, len);
            }
            else
            {
                arcfour = new ARCFOUREncryption();
                arcfour.PrepareARCFOURKey(key, off, len);
            }
        }

        public virtual byte[] Update(byte[] b, int off, int len)
        {
            if (aes)
            {
                if (initiated)
                {
                    return cipher.Update(b, off, len);
                }

                int num = Math.Min(iv.Length - ivptr, len);
                Array.Copy(b, off, iv, ivptr, num);
                off += num;
                len -= num;
                ivptr += num;
                if (ivptr == iv.Length)
                {
                    cipher = new AESCipher(forEncryption: false, key, iv);
                    initiated = true;
                    if (len > 0)
                    {
                        return cipher.Update(b, off, len);
                    }
                }

                return null;
            }

            byte[] array = new byte[len];
            arcfour.EncryptARCFOUR(b, off, len, array, 0);
            return array;
        }

        public virtual byte[] Finish()
        {
            if (aes)
            {
                return cipher.DoFinal();
            }

            return null;
        }
    }
}
