namespace Sign.Org.BouncyCastle.Math.EC
{
    public abstract class ECFieldElement
    {
        public abstract string FieldName { get; }

        public abstract int FieldSize { get; }

        public abstract BigInteger ToBigInteger();

        public abstract ECFieldElement Add(ECFieldElement b);

        public abstract ECFieldElement Subtract(ECFieldElement b);

        public abstract ECFieldElement Multiply(ECFieldElement b);

        public abstract ECFieldElement Divide(ECFieldElement b);

        public abstract ECFieldElement Negate();

        public abstract ECFieldElement Square();

        public abstract ECFieldElement Invert();

        public abstract ECFieldElement Sqrt();

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            ECFieldElement eCFieldElement = obj as ECFieldElement;
            if (eCFieldElement == null)
            {
                return false;
            }

            return Equals(eCFieldElement);
        }

        protected bool Equals(ECFieldElement other)
        {
            return ToBigInteger().Equals(other.ToBigInteger());
        }

        public override int GetHashCode()
        {
            return ToBigInteger().GetHashCode();
        }

        public override string ToString()
        {
            return ToBigInteger().ToString(2);
        }
    }
}
