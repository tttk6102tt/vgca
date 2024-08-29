using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Math.EC;
using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Crypto.Signers
{
    public class ECGost3410Signer : IDsa
    {
        private ECKeyParameters key;

        private SecureRandom random;

        public string AlgorithmName => "ECGOST3410";

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
            byte[] array = new byte[message.Length];
            for (int i = 0; i != array.Length; i++)
            {
                array[i] = message[array.Length - 1 - i];
            }

            BigInteger val = new BigInteger(1, array);
            BigInteger n = key.Parameters.N;
            BigInteger bigInteger = null;
            BigInteger bigInteger2 = null;
            do
            {
                BigInteger bigInteger3 = null;
                while (true)
                {
                    bigInteger3 = new BigInteger(n.BitLength, random);
                    if (bigInteger3.SignValue != 0)
                    {
                        bigInteger = key.Parameters.G.Multiply(bigInteger3).X.ToBigInteger().Mod(n);
                        if (bigInteger.SignValue != 0)
                        {
                            break;
                        }
                    }
                }

                BigInteger d = ((ECPrivateKeyParameters)key).D;
                bigInteger2 = bigInteger3.Multiply(val).Add(d.Multiply(bigInteger)).Mod(n);
            }
            while (bigInteger2.SignValue == 0);
            return new BigInteger[2] { bigInteger, bigInteger2 };
        }

        public bool VerifySignature(byte[] message, BigInteger r, BigInteger s)
        {
            byte[] array = new byte[message.Length];
            for (int i = 0; i != array.Length; i++)
            {
                array[i] = message[array.Length - 1 - i];
            }

            BigInteger bigInteger = new BigInteger(1, array);
            BigInteger n = key.Parameters.N;
            if (r.CompareTo(BigInteger.One) < 0 || r.CompareTo(n) >= 0)
            {
                return false;
            }

            if (s.CompareTo(BigInteger.One) < 0 || s.CompareTo(n) >= 0)
            {
                return false;
            }

            BigInteger val = bigInteger.ModInverse(n);
            BigInteger a = s.Multiply(val).Mod(n);
            BigInteger b = n.Subtract(r).Multiply(val).Mod(n);
            ECPoint g = key.Parameters.G;
            ECPoint q = ((ECPublicKeyParameters)key).Q;
            ECPoint eCPoint = ECAlgorithms.SumOfTwoMultiplies(g, a, q, b);
            if (eCPoint.IsInfinity)
            {
                return false;
            }

            return eCPoint.X.ToBigInteger().Mod(n).Equals(r);
        }
    }
}
