namespace Sign.itext.pdf.crypto
{
    public class ARCFOUREncryption
    {
        private byte[] state = new byte[256];

        private int x;

        private int y;

        public virtual void PrepareARCFOURKey(byte[] key)
        {
            PrepareARCFOURKey(key, 0, key.Length);
        }

        public virtual void PrepareARCFOURKey(byte[] key, int off, int len)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < 256; i++)
            {
                state[i] = (byte)i;
            }

            x = 0;
            y = 0;
            for (int j = 0; j < 256; j++)
            {
                num2 = (key[num + off] + state[j] + num2) & 0xFF;
                byte b = state[j];
                state[j] = state[num2];
                state[num2] = b;
                num = (num + 1) % len;
            }
        }

        public virtual void EncryptARCFOUR(byte[] dataIn, int off, int len, byte[] dataOut, int offOut)
        {
            int num = len + off;
            for (int i = off; i < num; i++)
            {
                x = (x + 1) & 0xFF;
                y = (state[x] + y) & 0xFF;
                byte b = state[x];
                state[x] = state[y];
                state[y] = b;
                dataOut[i - off + offOut] = (byte)(dataIn[i] ^ state[(state[x] + state[y]) & 0xFF]);
            }
        }

        public virtual void EncryptARCFOUR(byte[] data, int off, int len)
        {
            EncryptARCFOUR(data, off, len, data, off);
        }

        public virtual void EncryptARCFOUR(byte[] dataIn, byte[] dataOut)
        {
            EncryptARCFOUR(dataIn, 0, dataIn.Length, dataOut, 0);
        }

        public virtual void EncryptARCFOUR(byte[] data)
        {
            EncryptARCFOUR(data, 0, data.Length, data, 0);
        }
    }
}
