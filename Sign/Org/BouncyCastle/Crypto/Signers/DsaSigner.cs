using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Crypto.Signers
{
    public class DsaSigner : IDsa
    {
        private DsaKeyParameters key;

        private SecureRandom random;

        public string AlgorithmName => "DSA";

        public void Init(bool forSigning, ICipherParameters parameters)
        {
            if (forSigning)
            {
                if (parameters is ParametersWithRandom)
                {
                    ParametersWithRandom parametersWithRandom = (ParametersWithRandom)parameters;
                    random = parametersWithRandom.Random;
                    parameters = parametersWithRandom.Parameters;
                }
                else
                {
                    random = new SecureRandom();
                }

                if (!(parameters is DsaPrivateKeyParameters))
                {
                    throw new InvalidKeyException("DSA private key required for signing");
                }

                key = (DsaPrivateKeyParameters)parameters;
            }
            else
            {
                if (!(parameters is DsaPublicKeyParameters))
                {
                    throw new InvalidKeyException("DSA public key required for verification");
                }

                key = (DsaPublicKeyParameters)parameters;
            }
        }

        public BigInteger[] GenerateSignature(byte[] message)
        {
            DsaParameters parameters = key.Parameters;
            BigInteger q = parameters.Q;
            BigInteger bigInteger = calculateE(q, message);
            BigInteger bigInteger2;
            do
            {
                bigInteger2 = new BigInteger(q.BitLength, random);
            }
            while (bigInteger2.CompareTo(q) >= 0);
            BigInteger bigInteger3 = parameters.G.ModPow(bigInteger2, parameters.P).Mod(q);
            bigInteger2 = bigInteger2.ModInverse(q).Multiply(bigInteger.Add(((DsaPrivateKeyParameters)key).X.Multiply(bigInteger3)));
            BigInteger bigInteger4 = bigInteger2.Mod(q);
            return new BigInteger[2] { bigInteger3, bigInteger4 };
        }

        public bool VerifySignature(byte[] message, BigInteger r, BigInteger s)
        {
            DsaParameters parameters = key.Parameters;
            BigInteger q = parameters.Q;
            BigInteger bigInteger = calculateE(q, message);
            if (r.SignValue <= 0 || q.CompareTo(r) <= 0)
            {
                return false;
            }

            if (s.SignValue <= 0 || q.CompareTo(s) <= 0)
            {
                return false;
            }

            BigInteger val = s.ModInverse(q);
            BigInteger e = bigInteger.Multiply(val).Mod(q);
            BigInteger e2 = r.Multiply(val).Mod(q);
            BigInteger p = parameters.P;
            e = parameters.G.ModPow(e, p);
            e2 = ((DsaPublicKeyParameters)key).Y.ModPow(e2, p);
            return e.Multiply(e2).Mod(p).Mod(q)
                .Equals(r);
        }

        private BigInteger calculateE(BigInteger n, byte[] message)
        {
            int length = System.Math.Min(message.Length, n.BitLength / 8);
            return new BigInteger(1, message, 0, length);
        }
    }
}
