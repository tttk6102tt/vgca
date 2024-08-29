using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Math.EC;
using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Crypto.Signers
{
    public class ECDsaSigner : IDsa
    {
        private ECKeyParameters key;

        private SecureRandom random;

        public string AlgorithmName => "ECDSA";

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

                if (!(parameters is ECPrivateKeyParameters))
                {
                    throw new InvalidKeyException("EC private key required for signing");
                }

                key = (ECPrivateKeyParameters)parameters;
            }
            else
            {
                if (!(parameters is ECPublicKeyParameters))
                {
                    throw new InvalidKeyException("EC public key required for verification");
                }

                key = (ECPublicKeyParameters)parameters;
            }
        }

        public BigInteger[] GenerateSignature(byte[] message)
        {
            BigInteger n = key.Parameters.N;
            BigInteger bigInteger = calculateE(n, message);
            BigInteger bigInteger2 = null;
            BigInteger bigInteger3 = null;
            do
            {
                BigInteger bigInteger4 = null;
                while (true)
                {
                    bigInteger4 = new BigInteger(n.BitLength, random);
                    if (bigInteger4.SignValue != 0 && bigInteger4.CompareTo(n) < 0)
                    {
                        bigInteger2 = key.Parameters.G.Multiply(bigInteger4).X.ToBigInteger().Mod(n);
                        if (bigInteger2.SignValue != 0)
                        {
                            break;
                        }
                    }
                }

                BigInteger d = ((ECPrivateKeyParameters)key).D;
                bigInteger3 = bigInteger4.ModInverse(n).Multiply(bigInteger.Add(d.Multiply(bigInteger2).Mod(n))).Mod(n);
            }
            while (bigInteger3.SignValue == 0);
            return new BigInteger[2] { bigInteger2, bigInteger3 };
        }

        public bool VerifySignature(byte[] message, BigInteger r, BigInteger s)
        {
            BigInteger n = key.Parameters.N;
            if (r.SignValue < 1 || s.SignValue < 1 || r.CompareTo(n) >= 0 || s.CompareTo(n) >= 0)
            {
                return false;
            }

            BigInteger bigInteger = calculateE(n, message);
            BigInteger val = s.ModInverse(n);
            BigInteger a = bigInteger.Multiply(val).Mod(n);
            BigInteger b = r.Multiply(val).Mod(n);
            ECPoint g = key.Parameters.G;
            ECPoint q = ((ECPublicKeyParameters)key).Q;
            ECPoint eCPoint = ECAlgorithms.SumOfTwoMultiplies(g, a, q, b);
            if (eCPoint.IsInfinity)
            {
                return false;
            }

            return eCPoint.X.ToBigInteger().Mod(n).Equals(r);
        }

        private BigInteger calculateE(BigInteger n, byte[] message)
        {
            int num = message.Length * 8;
            BigInteger bigInteger = new BigInteger(1, message);
            if (n.BitLength < num)
            {
                bigInteger = bigInteger.ShiftRight(num - n.BitLength);
            }

            return bigInteger;
        }
    }
}
