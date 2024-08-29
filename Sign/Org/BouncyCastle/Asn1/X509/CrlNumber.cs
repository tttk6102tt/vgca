using Sign.Org.BouncyCastle.Math;

namespace Sign.Org.BouncyCastle.Asn1.X509
{
    public class CrlNumber : DerInteger
    {
        public BigInteger Number => base.PositiveValue;

        public CrlNumber(BigInteger number)
            : base(number)
        {
        }

        public override string ToString()
        {
            return "CRLNumber: " + Number;
        }
    }
}
