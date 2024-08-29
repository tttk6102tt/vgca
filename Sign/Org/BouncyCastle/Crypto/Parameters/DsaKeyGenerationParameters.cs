using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Crypto.Parameters
{
    public class DsaKeyGenerationParameters : KeyGenerationParameters
    {
        private readonly DsaParameters parameters;

        public DsaParameters Parameters => parameters;

        public DsaKeyGenerationParameters(SecureRandom random, DsaParameters parameters)
            : base(random, parameters.P.BitLength - 1)
        {
            this.parameters = parameters;
        }
    }
}
