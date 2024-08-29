using Sign.Org.BouncyCastle.Crypto.Parameters;

namespace Sign.Org.BouncyCastle.Crypto.Generators
{
    public class DesKeyGenerator : CipherKeyGenerator
    {
        public DesKeyGenerator()
        {
        }

        internal DesKeyGenerator(int defaultStrength)
            : base(defaultStrength)
        {
        }

        protected override void engineInit(KeyGenerationParameters parameters)
        {
            base.engineInit(parameters);
            if (strength == 0 || strength == 7)
            {
                strength = 8;
            }
            else if (strength != 8)
            {
                throw new ArgumentException("DES key must be " + 64 + " bits long.");
            }
        }

        protected override byte[] engineGenerateKey()
        {
            byte[] array;
            do
            {
                array = random.GenerateSeed(8);
                DesParameters.SetOddParity(array);
            }
            while (DesParameters.IsWeakKey(array, 0));
            return array;
        }
    }
}
