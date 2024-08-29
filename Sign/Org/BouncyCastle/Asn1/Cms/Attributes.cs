﻿namespace Sign.Org.BouncyCastle.Asn1.Cms
{
    public class Attributes : Asn1Encodable
    {
        private readonly Asn1Set attributes;

        private Attributes(Asn1Set attributes)
        {
            this.attributes = attributes;
        }

        public Attributes(Asn1EncodableVector v)
        {
            attributes = new BerSet(v);
        }

        public static Attributes GetInstance(object obj)
        {
            if (obj is Attributes)
            {
                return (Attributes)obj;
            }

            if (obj != null)
            {
                return new Attributes(Asn1Set.GetInstance(obj));
            }

            return null;
        }

        public virtual Sign.Org.BouncyCastle.Cms.Attribute[] GetAttributes()
        {
            Sign.Org.BouncyCastle.Cms.Attribute[] array = new Sign.Org.BouncyCastle.Cms.Attribute[attributes.Count];
            for (int i = 0; i != array.Length; i++)
            {
                array[i] = Sign.Org.BouncyCastle.Cms.Attribute.GetInstance(attributes[i]);
            }

            return array;
        }

        public override Asn1Object ToAsn1Object()
        {
            return attributes;
        }
    }
}
