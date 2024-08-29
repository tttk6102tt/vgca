using Sign.Org.BouncyCastle.Math;

namespace Sign.Org.BouncyCastle.Crypto
{
    public interface IBasicAgreement
    {
        void Init(ICipherParameters parameters);

        int GetFieldSize();

        BigInteger CalculateAgreement(ICipherParameters pubKey);
    }
}
