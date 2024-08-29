﻿using Sign.Org.BouncyCastle.Utilities;

namespace Sign.Org.BouncyCastle.Crypto.Parameters
{
    public class DHValidationParameters
    {
        private readonly byte[] seed;

        private readonly int counter;

        public int Counter => counter;

        public DHValidationParameters(byte[] seed, int counter)
        {
            if (seed == null)
            {
                throw new ArgumentNullException("seed");
            }

            this.seed = (byte[])seed.Clone();
            this.counter = counter;
        }

        public byte[] GetSeed()
        {
            return (byte[])seed.Clone();
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            DHValidationParameters dHValidationParameters = obj as DHValidationParameters;
            if (dHValidationParameters == null)
            {
                return false;
            }

            return Equals(dHValidationParameters);
        }

        protected bool Equals(DHValidationParameters other)
        {
            if (counter == other.counter)
            {
                return Arrays.AreEqual(seed, other.seed);
            }

            return false;
        }

        public override int GetHashCode()
        {
            int num = counter;
            return num.GetHashCode() ^ Arrays.GetHashCode(seed);
        }
    }
}
