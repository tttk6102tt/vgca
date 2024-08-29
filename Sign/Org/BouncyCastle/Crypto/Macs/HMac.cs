using Sign.Org.BouncyCastle.Crypto.Parameters;

namespace Sign.Org.BouncyCastle.Crypto.Macs
{
    public class HMac : IMac
    {
        private const byte IPAD = 54;

        private const byte OPAD = 92;

        private readonly IDigest digest;

        private readonly int digestSize;

        private readonly int blockLength;

        private readonly byte[] inputPad;

        private readonly byte[] outputBuf;

        public virtual string AlgorithmName => digest.AlgorithmName + "/HMAC";

        public HMac(IDigest digest)
        {
            this.digest = digest;
            digestSize = digest.GetDigestSize();
            blockLength = digest.GetByteLength();
            inputPad = new byte[blockLength];
            outputBuf = new byte[blockLength + digestSize];
        }

        public virtual IDigest GetUnderlyingDigest()
        {
            return digest;
        }

        public virtual void Init(ICipherParameters parameters)
        {
            digest.Reset();
            byte[] key = ((KeyParameter)parameters).GetKey();
            int num = key.Length;
            if (num > blockLength)
            {
                digest.BlockUpdate(key, 0, num);
                digest.DoFinal(inputPad, 0);
                num = digestSize;
            }
            else
            {
                Array.Copy(key, 0, inputPad, 0, num);
            }

            Array.Clear(inputPad, num, blockLength - num);
            Array.Copy(inputPad, 0, outputBuf, 0, blockLength);
            XorPad(inputPad, blockLength, 54);
            XorPad(outputBuf, blockLength, 92);
            digest.BlockUpdate(inputPad, 0, inputPad.Length);
        }

        public virtual int GetMacSize()
        {
            return digestSize;
        }

        public virtual void Update(byte input)
        {
            digest.Update(input);
        }

        public virtual void BlockUpdate(byte[] input, int inOff, int len)
        {
            digest.BlockUpdate(input, inOff, len);
        }

        public virtual int DoFinal(byte[] output, int outOff)
        {
            digest.DoFinal(outputBuf, blockLength);
            digest.BlockUpdate(outputBuf, 0, outputBuf.Length);
            int result = digest.DoFinal(output, outOff);
            Array.Clear(outputBuf, blockLength, digestSize);
            digest.BlockUpdate(inputPad, 0, inputPad.Length);
            return result;
        }

        public virtual void Reset()
        {
            digest.Reset();
            digest.BlockUpdate(inputPad, 0, inputPad.Length);
        }

        private static void XorPad(byte[] pad, int len, byte n)
        {
            for (int i = 0; i < len; i++)
            {
                pad[i] ^= n;
            }
        }
    }
}
