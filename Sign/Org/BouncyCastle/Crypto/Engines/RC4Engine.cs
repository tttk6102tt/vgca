using Sign.Org.BouncyCastle.Crypto.Parameters;

namespace Sign.Org.BouncyCastle.Crypto.Engines
{
    public class RC4Engine : IStreamCipher
    {
        private static readonly int STATE_LENGTH = 256;

        private byte[] engineState;

        private int x;

        private int y;

        private byte[] workingKey;

        public string AlgorithmName => "RC4";

        public void Init(bool forEncryption, ICipherParameters parameters)
        {
            if (parameters is KeyParameter)
            {
                workingKey = ((KeyParameter)parameters).GetKey();
                SetKey(workingKey);
                return;
            }

            throw new ArgumentException("invalid parameter passed to RC4 init - " + parameters.GetType().ToString());
        }

        public byte ReturnByte(byte input)
        {
            x = (x + 1) & 0xFF;
            y = (engineState[x] + y) & 0xFF;
            byte b = engineState[x];
            engineState[x] = engineState[y];
            engineState[y] = b;
            return (byte)(input ^ engineState[(engineState[x] + engineState[y]) & 0xFF]);
        }

        public void ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
        {
            if (inOff + length > input.Length)
            {
                throw new DataLengthException("input buffer too short");
            }

            if (outOff + length > output.Length)
            {
                throw new DataLengthException("output buffer too short");
            }

            for (int i = 0; i < length; i++)
            {
                x = (x + 1) & 0xFF;
                y = (engineState[x] + y) & 0xFF;
                byte b = engineState[x];
                engineState[x] = engineState[y];
                engineState[y] = b;
                output[i + outOff] = (byte)(input[i + inOff] ^ engineState[(engineState[x] + engineState[y]) & 0xFF]);
            }
        }

        public void Reset()
        {
            SetKey(workingKey);
        }

        private void SetKey(byte[] keyBytes)
        {
            workingKey = keyBytes;
            x = 0;
            y = 0;
            if (engineState == null)
            {
                engineState = new byte[STATE_LENGTH];
            }

            for (int i = 0; i < STATE_LENGTH; i++)
            {
                engineState[i] = (byte)i;
            }

            int num = 0;
            int num2 = 0;
            for (int j = 0; j < STATE_LENGTH; j++)
            {
                num2 = ((keyBytes[num] & 0xFF) + engineState[j] + num2) & 0xFF;
                byte b = engineState[j];
                engineState[j] = engineState[num2];
                engineState[num2] = b;
                num = (num + 1) % keyBytes.Length;
            }
        }
    }
}
