﻿using Sign.Org.BouncyCastle.Utilities;

namespace Sign.Org.BouncyCastle.Asn1
{
    public class DerT61String : DerStringBase
    {
        private readonly string str;

        public static DerT61String GetInstance(object obj)
        {
            if (obj == null || obj is DerT61String)
            {
                return (DerT61String)obj;
            }

            throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
        }

        public static DerT61String GetInstance(Asn1TaggedObject obj, bool isExplicit)
        {
            Asn1Object @object = obj.GetObject();
            if (isExplicit || @object is DerT61String)
            {
                return GetInstance(@object);
            }

            return new DerT61String(Asn1OctetString.GetInstance(@object).GetOctets());
        }

        public DerT61String(byte[] str)
            : this(Strings.FromByteArray(str))
        {
        }

        public DerT61String(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            this.str = str;
        }

        public override string GetString()
        {
            return str;
        }

        internal override void Encode(DerOutputStream derOut)
        {
            derOut.WriteEncoded(20, GetOctets());
        }

        public byte[] GetOctets()
        {
            return Strings.ToByteArray(str);
        }

        protected override bool Asn1Equals(Asn1Object asn1Object)
        {
            DerT61String derT61String = asn1Object as DerT61String;
            if (derT61String == null)
            {
                return false;
            }

            return str.Equals(derT61String.str);
        }
    }
}
