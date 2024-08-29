using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Math.EC;

namespace Sign.Org.BouncyCastle.Crypto.Agreement
{
    public class ECMqvBasicAgreement : IBasicAgreement
    {
        protected internal MqvPrivateParameters privParams;

        public virtual void Init(ICipherParameters parameters)
        {
            if (parameters is ParametersWithRandom)
            {
                parameters = ((ParametersWithRandom)parameters).Parameters;
            }

            privParams = (MqvPrivateParameters)parameters;
        }

        public virtual int GetFieldSize()
        {
            return (privParams.StaticPrivateKey.Parameters.Curve.FieldSize + 7) / 8;
        }

        public virtual BigInteger CalculateAgreement(ICipherParameters pubKey)
        {
            MqvPublicParameters mqvPublicParameters = (MqvPublicParameters)pubKey;
            ECPrivateKeyParameters staticPrivateKey = privParams.StaticPrivateKey;
            return calculateMqvAgreement(staticPrivateKey.Parameters, staticPrivateKey, privParams.EphemeralPrivateKey, privParams.EphemeralPublicKey, mqvPublicParameters.StaticPublicKey, mqvPublicParameters.EphemeralPublicKey).X.ToBigInteger();
        }

        private static ECPoint calculateMqvAgreement(ECDomainParameters parameters, ECPrivateKeyParameters d1U, ECPrivateKeyParameters d2U, ECPublicKeyParameters Q2U, ECPublicKeyParameters Q1V, ECPublicKeyParameters Q2V)
        {
            BigInteger n = parameters.N;
            int num = (n.BitLength + 1) / 2;
            BigInteger m = BigInteger.One.ShiftLeft(num);
            ECPoint eCPoint = ((Q2U != null) ? Q2U.Q : parameters.G.Multiply(d2U.D));
            BigInteger val = eCPoint.X.ToBigInteger().Mod(m).SetBit(num);
            BigInteger val2 = d1U.D.Multiply(val).Mod(n).Add(d2U.D)
                .Mod(n);
            BigInteger bigInteger = Q2V.Q.X.ToBigInteger().Mod(m).SetBit(num);
            BigInteger bigInteger2 = parameters.H.Multiply(val2).Mod(n);
            ECPoint eCPoint2 = ECAlgorithms.SumOfTwoMultiplies(Q1V.Q, bigInteger.Multiply(bigInteger2).Mod(n), Q2V.Q, bigInteger2);
            if (eCPoint2.IsInfinity)
            {
                throw new InvalidOperationException("Infinity is not a valid agreement value for MQV");
            }

            return eCPoint2;
        }
    }
}
