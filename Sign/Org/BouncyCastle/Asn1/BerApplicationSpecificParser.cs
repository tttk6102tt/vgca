﻿namespace Sign.Org.BouncyCastle.Asn1
{
    public class BerApplicationSpecificParser : IAsn1ApplicationSpecificParser, IAsn1Convertible
    {
        private readonly int tag;

        private readonly Asn1StreamParser parser;

        internal BerApplicationSpecificParser(int tag, Asn1StreamParser parser)
        {
            this.tag = tag;
            this.parser = parser;
        }

        public IAsn1Convertible ReadObject()
        {
            return parser.ReadObject();
        }

        public Asn1Object ToAsn1Object()
        {
            return new BerApplicationSpecific(tag, parser.ReadVector());
        }
    }
}
