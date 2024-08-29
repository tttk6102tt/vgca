using Sign.Org.BouncyCastle.Crypto.Parameters;

namespace Sign.Org.BouncyCastle.Crypto.Generators
{
    public class DesEdeKeyGenerator : DesKeyGenerator
    {
        public DesEdeKeyGenerator()
        {
        }

        internal DesEdeKeyGenerator(int defaultStrength)
            : base(defaultStrength)
        {
        }

        protected override void engineInit(KeyGenerationParameters parameters)
        {
            random = parameters.Random;
            strength = (parameters.Strength + 7) / 8;
            if (strength == 0 || strength == 21)
            {
                strength = 24;
            }
            else if (strength == 14)
            {
                strength = 16;
            }
            else if (strength != 24 && strength != 16)
            {
                throw new ArgumentException("DESede key must be " + 192 + " or " + 128 + " bits long.");
            }
        }

        protected override byte[] engineGenerateKey()
        {
            byte[] array;
            do
            {
                array = random.GenerateSeed(strength);
                DesParameters.SetOddParity(array);
            }
            while (DesEdeParameters.IsWeakKey(array, 0, array.Length));
            return array;
        }
    }
}
