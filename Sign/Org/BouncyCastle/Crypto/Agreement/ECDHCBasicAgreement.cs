using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;

namespace Sign.Org.BouncyCastle.Crypto.Agreement
{
    public class ECDHCBasicAgreement : IBasicAgreement
    {
        private ECPrivateKeyParameters key;

        public virtual void Init(ICipherParameters parameters)
        {
            if (parameters is ParametersWithRandom)
            {
                parameters = ((ParametersWithRandom)parameters).Parameters;
            }

            key = (ECPrivateKeyParameters)parameters;
        }

        public virtual int GetFieldSize()
        {
            return (key.Parameters.Curve.FieldSize + 7) / 8;
        }

        public virtual BigInteger CalculateAgreement(ICipherParameters pubKey)
        {
            ECPublicKeyParameters obj = (ECPublicKeyParameters)pubKey;
            ECDomainParameters parameters = obj.Parameters;
            return obj.Q.Multiply(parameters.H.Multiply(key.D)).X.ToBigInteger();
        }
    }
}
