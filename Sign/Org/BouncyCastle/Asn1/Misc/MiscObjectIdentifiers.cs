namespace Sign.Org.BouncyCastle.Asn1.Misc
{
    public abstract class MiscObjectIdentifiers
    {
        public static readonly DerObjectIdentifier Netscape = new DerObjectIdentifier("2.16.840.1.113730.1");

        public static readonly DerObjectIdentifier NetscapeCertType = new DerObjectIdentifier(string.Concat(Netscape, ".1"));

        public static readonly DerObjectIdentifier NetscapeBaseUrl = new DerObjectIdentifier(string.Concat(Netscape, ".2"));

        public static readonly DerObjectIdentifier NetscapeRevocationUrl = new DerObjectIdentifier(string.Concat(Netscape, ".3"));

        public static readonly DerObjectIdentifier NetscapeCARevocationUrl = new DerObjectIdentifier(string.Concat(Netscape, ".4"));

        public static readonly DerObjectIdentifier NetscapeRenewalUrl = new DerObjectIdentifier(string.Concat(Netscape, ".7"));

        public static readonly DerObjectIdentifier NetscapeCAPolicyUrl = new DerObjectIdentifier(string.Concat(Netscape, ".8"));

        public static readonly DerObjectIdentifier NetscapeSslServerName = new DerObjectIdentifier(string.Concat(Netscape, ".12"));

        public static readonly DerObjectIdentifier NetscapeCertComment = new DerObjectIdentifier(string.Concat(Netscape, ".13"));

        internal const string Verisign = "2.16.840.1.113733.1";

        public static readonly DerObjectIdentifier VerisignCzagExtension = new DerObjectIdentifier("2.16.840.1.113733.1.6.3");

        public static readonly DerObjectIdentifier VerisignDnbDunsNumber = new DerObjectIdentifier("2.16.840.1.113733.1.6.15");

        public static readonly string Novell = "2.16.840.1.113719";

        public static readonly DerObjectIdentifier NovellSecurityAttribs = new DerObjectIdentifier(Novell + ".1.9.4.1");

        public static readonly string Entrust = "1.2.840.113533.7";

        public static readonly DerObjectIdentifier EntrustVersionExtension = new DerObjectIdentifier(Entrust + ".65.0");
    }
}
