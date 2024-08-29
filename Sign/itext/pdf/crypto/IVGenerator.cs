namespace Sign.itext.pdf.crypto
{
    public sealed class IVGenerator
    {
        private static ARCFOUREncryption rc4;

        static IVGenerator()
        {
            rc4 = new ARCFOUREncryption();
            byte[] array = new byte[8];
            long num = DateTime.Now.Ticks;
            for (int i = 0; i != 8; i++)
            {
                array[i] = (byte)num;
                num = (long)((ulong)num >> 8);
            }

            rc4.PrepareARCFOURKey(array);
        }

        private IVGenerator()
        {
        }

        public static byte[] GetIV()
        {
            return GetIV(16);
        }

        public static byte[] GetIV(int len)
        {
            byte[] array = new byte[len];
            lock (rc4)
            {
                rc4.EncryptARCFOUR(array);
                return array;
            }
        }
    }
}
