using Sign.Org.BouncyCastle.Utilities;
using System.Collections;

namespace Sign.Org.BouncyCastle.Crypto.Modes.Gcm
{
    public class Tables1kGcmExponentiator : IGcmExponentiator
    {
        private IList lookupPowX2;

        public void Init(byte[] x)
        {
            if (lookupPowX2 == null || !Arrays.AreEqual(x, (byte[])lookupPowX2[0]))
            {
                lookupPowX2 = Platform.CreateArrayList(8);
                lookupPowX2.Add(Arrays.Clone(x));
            }
        }

        public void ExponentiateX(long pow, byte[] output)
        {
            byte[] array = GcmUtilities.OneAsBytes();
            int num = 0;
            while (pow > 0)
            {
                if ((pow & 1) != 0L)
                {
                    EnsureAvailable(num);
                    GcmUtilities.Multiply(array, (byte[])lookupPowX2[num]);
                }

                num++;
                pow >>= 1;
            }

            Array.Copy(array, 0, output, 0, 16);
        }

        private void EnsureAvailable(int bit)
        {
            int num = lookupPowX2.Count;
            if (num <= bit)
            {
                byte[] array = (byte[])lookupPowX2[num - 1];
                do
                {
                    array = Arrays.Clone(array);
                    GcmUtilities.Multiply(array, array);
                    lookupPowX2.Add(array);
                }
                while (++num <= bit);
            }
        }
    }
}
