using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Crypto.Engines
{
    internal class RsaCoreEngine
    {
        private RsaKeyParameters key;

        private bool forEncryption;

        private int bitSize;

        public void Init(bool forEncryption, ICipherParameters parameters)
        {
            if (parameters is ParametersWithRandom)
            {
                parameters = ((ParametersWithRandom)parameters).Parameters;
            }

            if (!(parameters is RsaKeyParameters))
            {
                throw new InvalidKeyException("Not an RSA key");
            }

            key = (RsaKeyParameters)parameters;
            this.forEncryption = forEncryption;
            bitSize = key.Modulus.BitLength;
        }

        public int GetInputBlockSize()
        {
            if (forEncryption)
            {
                return (bitSize - 1) / 8;
            }

            return (bitSize + 7) / 8;
        }

        public int GetOutputBlockSize()
        {
            if (forEncryption)
            {
                return (bitSize + 7) / 8;
            }

            return (bitSize - 1) / 8;
        }

        public BigInteger ConvertInput(byte[] inBuf, int inOff, int inLen)
        {
            int num = (bitSize + 7) / 8;
            if (inLen > num)
            {
                throw new DataLengthException("input too large for RSA cipher.");
            }

            BigInteger bigInteger = new BigInteger(1, inBuf, inOff, inLen);
            if (bigInteger.CompareTo(key.Modulus) >= 0)
            {
                throw new DataLengthException("input too large for RSA cipher.");
            }

            return bigInteger;
        }

        public byte[] ConvertOutput(BigInteger result)
        {
            byte[] array = result.ToByteArrayUnsigned();
            if (forEncryption)
            {
                int outputBlockSize = GetOutputBlockSize();
                if (array.Length < outputBlockSize)
                {
                    byte[] array2 = new byte[outputBlockSize];
                    array.CopyTo(array2, array2.Length - array.Length);
                    array = array2;
                }
            }

            return array;
        }

        public BigInteger ProcessBlock(BigInteger input)
        {
            if (key is RsaPrivateCrtKeyParameters)
            {
                RsaPrivateCrtKeyParameters obj = (RsaPrivateCrtKeyParameters)key;
                BigInteger p = obj.P;
                BigInteger q = obj.Q;
                BigInteger dP = obj.DP;
                BigInteger dQ = obj.DQ;
                BigInteger qInv = obj.QInv;
                BigInteger bigInteger = input.Remainder(p).ModPow(dP, p);
                BigInteger bigInteger2 = input.Remainder(q).ModPow(dQ, q);
                return bigInteger.Subtract(bigInteger2).Multiply(qInv).Mod(p)
                    .Multiply(q)
                    .Add(bigInteger2);
            }

            return input.ModPow(key.Exponent, key.Modulus);
        }
    }
}
