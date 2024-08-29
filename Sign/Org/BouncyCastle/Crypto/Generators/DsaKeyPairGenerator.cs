using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Security;
using Sign.Org.BouncyCastle.Utilities;

namespace Sign.Org.BouncyCastle.Crypto.Generators
{
    public class DsaKeyPairGenerator : IAsymmetricCipherKeyPairGenerator
    {
        private DsaKeyGenerationParameters param;

        public void Init(KeyGenerationParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            param = (DsaKeyGenerationParameters)parameters;
        }

        public AsymmetricCipherKeyPair GenerateKeyPair()
        {
            DsaParameters parameters = param.Parameters;
            BigInteger x = GeneratePrivateKey(parameters.Q, param.Random);
            return new AsymmetricCipherKeyPair(new DsaPublicKeyParameters(CalculatePublicKey(parameters.P, parameters.G, x), parameters), new DsaPrivateKeyParameters(x, parameters));
        }

        private static BigInteger GeneratePrivateKey(BigInteger q, SecureRandom random)
        {
            return BigIntegers.CreateRandomInRange(BigInteger.One, q.Subtract(BigInteger.One), random);
        }

        private static BigInteger CalculatePublicKey(BigInteger p, BigInteger g, BigInteger x)
        {
            return g.ModPow(x, p);
        }
    }
}
