using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;

namespace Sign.Org.BouncyCastle.Crypto.Agreement
{
    public class ECDHBasicAgreement : IBasicAgreement
    {
        protected internal ECPrivateKeyParameters privKey;

        public virtual void Init(ICipherParameters parameters)
        {
            if (parameters is ParametersWithRandom)
            {
                parameters = ((ParametersWithRandom)parameters).Parameters;
            }

            privKey = (ECPrivateKeyParameters)parameters;
        }

        public virtual int GetFieldSize()
        {
            return (privKey.Parameters.Curve.FieldSize + 7) / 8;
        }

        public virtual BigInteger CalculateAgreement(ICipherParameters pubKey)
        {
            return ((ECPublicKeyParameters)pubKey).Q.Multiply(privKey.D).X.ToBigInteger();
        }
    }
}
