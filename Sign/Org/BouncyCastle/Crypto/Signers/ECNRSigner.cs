using Sign.Org.BouncyCastle.Crypto.Generators;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Math.EC;
using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Crypto.Signers
{
    public class ECNRSigner : IDsa
    {
        private bool forSigning;

        private ECKeyParameters key;

        private SecureRandom random;

        public string AlgorithmName => "ECNR";

        public void Init(bool forSigning, ICipherParameters parameters)
        {
            this.forSigning = forSigning;
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
            if (!forSigning)
            {
                throw new InvalidOperationException("not initialised for signing");
            }

            BigInteger n = ((ECPrivateKeyParameters)key).Parameters.N;
            int bitLength = n.BitLength;
            BigInteger bigInteger = new BigInteger(1, message);
            int bitLength2 = bigInteger.BitLength;
            ECPrivateKeyParameters eCPrivateKeyParameters = (ECPrivateKeyParameters)key;
            if (bitLength2 > bitLength)
            {
                throw new DataLengthException("input too large for ECNR key.");
            }

            BigInteger bigInteger2 = null;
            BigInteger bigInteger3 = null;
            AsymmetricCipherKeyPair asymmetricCipherKeyPair;
            do
            {
                ECKeyPairGenerator eCKeyPairGenerator = new ECKeyPairGenerator();
                eCKeyPairGenerator.Init(new ECKeyGenerationParameters(eCPrivateKeyParameters.Parameters, random));
                asymmetricCipherKeyPair = eCKeyPairGenerator.GenerateKeyPair();
                bigInteger2 = ((ECPublicKeyParameters)asymmetricCipherKeyPair.Public).Q.X.ToBigInteger().Add(bigInteger).Mod(n);
            }
            while (bigInteger2.SignValue == 0);
            BigInteger d = eCPrivateKeyParameters.D;
            bigInteger3 = ((ECPrivateKeyParameters)asymmetricCipherKeyPair.Private).D.Subtract(bigInteger2.Multiply(d)).Mod(n);
            return new BigInteger[2] { bigInteger2, bigInteger3 };
        }

        public bool VerifySignature(byte[] message, BigInteger r, BigInteger s)
        {
            if (forSigning)
            {
                throw new InvalidOperationException("not initialised for verifying");
            }

            ECPublicKeyParameters eCPublicKeyParameters = (ECPublicKeyParameters)key;
            BigInteger n = eCPublicKeyParameters.Parameters.N;
            int bitLength = n.BitLength;
            BigInteger bigInteger = new BigInteger(1, message);
            if (bigInteger.BitLength > bitLength)
            {
                throw new DataLengthException("input too large for ECNR key.");
            }

            if (r.CompareTo(BigInteger.One) < 0 || r.CompareTo(n) >= 0)
            {
                return false;
            }

            if (s.CompareTo(BigInteger.Zero) < 0 || s.CompareTo(n) >= 0)
            {
                return false;
            }

            ECPoint g = eCPublicKeyParameters.Parameters.G;
            ECPoint q = eCPublicKeyParameters.Q;
            ECPoint eCPoint = ECAlgorithms.SumOfTwoMultiplies(g, s, q, r);
            if (eCPoint.IsInfinity)
            {
                return false;
            }

            BigInteger n2 = eCPoint.X.ToBigInteger();
            return r.Subtract(n2).Mod(n).Equals(bigInteger);
        }
    }
}
