﻿using Sign.Org.BouncyCastle.Utilities;

namespace Sign.Org.BouncyCastle.Asn1
{
    public abstract class Asn1TaggedObject : Asn1Object, Asn1TaggedObjectParser, IAsn1Convertible
    {
        internal int tagNo;

        internal bool explicitly = true;

        internal Asn1Encodable obj;

        public int TagNo => tagNo;

        public static Asn1TaggedObject GetInstance(Asn1TaggedObject obj, bool explicitly)
        {
            if (explicitly)
            {
                return (Asn1TaggedObject)obj.GetObject();
            }

            throw new ArgumentException("implicitly tagged tagged object");
        }

        public static Asn1TaggedObject GetInstance(object obj)
        {
            if (obj == null || obj is Asn1TaggedObject)
            {
                return (Asn1TaggedObject)obj;
            }

            throw new ArgumentException("Unknown object in GetInstance: " + obj.GetType().FullName, "obj");
        }

        protected Asn1TaggedObject(int tagNo, Asn1Encodable obj)
        {
            explicitly = true;
            this.tagNo = tagNo;
            this.obj = obj;
        }

        protected Asn1TaggedObject(bool explicitly, int tagNo, Asn1Encodable obj)
        {
            this.explicitly = explicitly || obj is IAsn1Choice;
            this.tagNo = tagNo;
            this.obj = obj;
        }

        protected override bool Asn1Equals(Asn1Object asn1Object)
        {
            Asn1TaggedObject asn1TaggedObject = asn1Object as Asn1TaggedObject;
            if (asn1TaggedObject == null)
            {
                return false;
            }

            if (tagNo == asn1TaggedObject.tagNo && explicitly == asn1TaggedObject.explicitly)
            {
                return object.Equals(GetObject(), asn1TaggedObject.GetObject());
            }

            return false;
        }

        protected override int Asn1GetHashCode()
        {
            int num = tagNo.GetHashCode();
            if (obj != null)
            {
                num ^= obj.GetHashCode();
            }

            return num;
        }

        public bool IsExplicit()
        {
            return explicitly;
        }

        public bool IsEmpty()
        {
            return false;
        }

        public Asn1Object GetObject()
        {
            if (obj != null)
            {
                return obj.ToAsn1Object();
            }

            return null;
        }

        public IAsn1Convertible GetObjectParser(int tag, bool isExplicit)
        {
            switch (tag)
            {
                case 17:
                    return Asn1Set.GetInstance(this, isExplicit).Parser;
                case 16:
                    return Asn1Sequence.GetInstance(this, isExplicit).Parser;
                case 4:
                    return Asn1OctetString.GetInstance(this, isExplicit).Parser;
                default:
                    if (isExplicit)
                    {
                        return GetObject();
                    }

                    throw Platform.CreateNotImplementedException("implicit tagging for tag: " + tag);
            }
        }

        public override string ToString()
        {
            return "[" + tagNo + "]" + obj;
        }
    }
}
