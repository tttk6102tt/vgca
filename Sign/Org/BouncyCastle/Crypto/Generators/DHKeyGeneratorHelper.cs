using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Security;
using Sign.Org.BouncyCastle.Utilities;

namespace Sign.Org.BouncyCastle.Crypto.Generators
{
    internal class DHKeyGeneratorHelper
    {
        internal static readonly DHKeyGeneratorHelper Instance = new DHKeyGeneratorHelper();

        private DHKeyGeneratorHelper()
        {
        }

        internal BigInteger CalculatePrivate(DHParameters dhParams, SecureRandom random)
        {
            int l = dhParams.L;
            if (l != 0)
            {
                return new BigInteger(l, random).SetBit(l - 1);
            }

            BigInteger min = BigInteger.Two;
            int m = dhParams.M;
            if (m != 0)
            {
                min = BigInteger.One.ShiftLeft(m - 1);
            }

            BigInteger max = dhParams.P.Subtract(BigInteger.Two);
            BigInteger q = dhParams.Q;
            if (q != null)
            {
                max = q.Subtract(BigInteger.Two);
            }

            return BigIntegers.CreateRandomInRange(min, max, random);
        }

        internal BigInteger CalculatePublic(DHParameters dhParams, BigInteger x)
        {
            return dhParams.G.ModPow(x, dhParams.P);
        }
    }
}
