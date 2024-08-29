using Sign.itext.error_messages;
using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.X509;
using System.Collections;
using System.Globalization;
using System.Text;

namespace Sign.itext.text.pdf.security
{
    public static class CertificateInfo
    {
        public class X509Name
        {
            public static DerObjectIdentifier C;

            public static DerObjectIdentifier O;

            public static DerObjectIdentifier OU;

            public static DerObjectIdentifier T;

            public static DerObjectIdentifier CN;

            public static DerObjectIdentifier SN;

            public static DerObjectIdentifier L;

            public static DerObjectIdentifier ST;

            public static DerObjectIdentifier SURNAME;

            public static DerObjectIdentifier GIVENNAME;

            public static DerObjectIdentifier INITIALS;

            public static DerObjectIdentifier GENERATION;

            public static DerObjectIdentifier UNIQUE_IDENTIFIER;

            public static DerObjectIdentifier EmailAddress;

            public static DerObjectIdentifier E;

            public static DerObjectIdentifier DC;

            public static DerObjectIdentifier UID;

            public static readonly Dictionary<DerObjectIdentifier, string> DefaultSymbols;

            public Dictionary<string, List<string>> values = new Dictionary<string, List<string>>();

            static X509Name()
            {
                C = new DerObjectIdentifier("2.5.4.6");
                O = new DerObjectIdentifier("2.5.4.10");
                OU = new DerObjectIdentifier("2.5.4.11");
                T = new DerObjectIdentifier("2.5.4.12");
                CN = new DerObjectIdentifier("2.5.4.3");
                SN = new DerObjectIdentifier("2.5.4.5");
                L = new DerObjectIdentifier("2.5.4.7");
                ST = new DerObjectIdentifier("2.5.4.8");
                SURNAME = new DerObjectIdentifier("2.5.4.4");
                GIVENNAME = new DerObjectIdentifier("2.5.4.42");
                INITIALS = new DerObjectIdentifier("2.5.4.43");
                GENERATION = new DerObjectIdentifier("2.5.4.44");
                UNIQUE_IDENTIFIER = new DerObjectIdentifier("2.5.4.45");
                EmailAddress = new DerObjectIdentifier("1.2.840.113549.1.9.1");
                E = EmailAddress;
                DC = new DerObjectIdentifier("0.9.2342.19200300.100.1.25");
                UID = new DerObjectIdentifier("0.9.2342.19200300.100.1.1");
                DefaultSymbols = new Dictionary<DerObjectIdentifier, string>();
                DefaultSymbols[C] = "C";
                DefaultSymbols[O] = "O";
                DefaultSymbols[T] = "T";
                DefaultSymbols[OU] = "OU";
                DefaultSymbols[CN] = "CN";
                DefaultSymbols[L] = "L";
                DefaultSymbols[ST] = "ST";
                DefaultSymbols[SN] = "SN";
                DefaultSymbols[EmailAddress] = "E";
                DefaultSymbols[DC] = "DC";
                DefaultSymbols[UID] = "UID";
                DefaultSymbols[SURNAME] = "SURNAME";
                DefaultSymbols[GIVENNAME] = "GIVENNAME";
                DefaultSymbols[INITIALS] = "INITIALS";
                DefaultSymbols[GENERATION] = "GENERATION";
            }

            public X509Name(Asn1Sequence seq)
            {
                IEnumerator enumerator = seq.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Asn1Set asn1Set = (Asn1Set)enumerator.Current;
                    for (int i = 0; i < asn1Set.Count; i++)
                    {
                        Asn1Sequence asn1Sequence = (Asn1Sequence)asn1Set[i];
                        if (asn1Sequence[0] is DerObjectIdentifier && DefaultSymbols.TryGetValue((DerObjectIdentifier)asn1Sequence[0], out var value))
                        {
                            if (!values.TryGetValue(value, out var value2))
                            {
                                value2 = new List<string>();
                                values[value] = value2;
                            }

                            value2.Add(((DerStringBase)asn1Sequence[1]).GetString());
                        }
                    }
                }
            }

            public X509Name(string dirName)
            {
                X509NameTokenizer x509NameTokenizer = new X509NameTokenizer(dirName);
                while (x509NameTokenizer.HasMoreTokens())
                {
                    string text = x509NameTokenizer.NextToken();
                    int num = text.IndexOf('=');
                    if (num == -1)
                    {
                        throw new ArgumentException(MessageLocalization.GetComposedMessage("badly.formated.directory.string"));
                    }

                    string key = text.Substring(0, num).ToUpper(CultureInfo.InvariantCulture);
                    string item = text.Substring(num + 1);
                    if (!values.TryGetValue(key, out var value))
                    {
                        value = new List<string>();
                        values[key] = value;
                    }

                    value.Add(item);
                }
            }

            public virtual string GetField(string name)
            {
                if (values.TryGetValue(name, out var value))
                {
                    if (value.Count != 0)
                    {
                        return value[0];
                    }

                    return null;
                }

                return null;
            }

            public virtual List<string> GetFieldArray(string name)
            {
                if (values.TryGetValue(name, out var value))
                {
                    return value;
                }

                return null;
            }

            public virtual Dictionary<string, List<string>> GetFields()
            {
                return values;
            }

            public override string ToString()
            {
                return values.ToString();
            }
        }

        public class X509NameTokenizer
        {
            private string oid;

            private int index;

            private StringBuilder buf = new StringBuilder();

            public X509NameTokenizer(string oid)
            {
                this.oid = oid;
                index = -1;
            }

            public virtual bool HasMoreTokens()
            {
                return index != oid.Length;
            }

            public virtual string NextToken()
            {
                if (index == oid.Length)
                {
                    return null;
                }

                int i = index + 1;
                bool flag = false;
                bool flag2 = false;
                buf.Length = 0;
                for (; i != oid.Length; i++)
                {
                    char c = oid[i];
                    if (c == '"')
                    {
                        if (!flag2)
                        {
                            flag = !flag;
                        }
                        else
                        {
                            buf.Append(c);
                        }

                        flag2 = false;
                        continue;
                    }

                    if (flag2 || flag)
                    {
                        buf.Append(c);
                        flag2 = false;
                        continue;
                    }

                    switch (c)
                    {
                        case '\\':
                            flag2 = true;
                            continue;
                        default:
                            buf.Append(c);
                            continue;
                        case ',':
                            break;
                    }

                    break;
                }

                index = i;
                return buf.ToString().Trim();
            }
        }

        public static X509Name GetIssuerFields(X509Certificate cert)
        {
            return new X509Name((Asn1Sequence)GetIssuer(cert.GetTbsCertificate()));
        }

        public static Asn1Object GetIssuer(byte[] enc)
        {
            Asn1Sequence obj = (Asn1Sequence)new Asn1InputStream(new MemoryStream(enc)).ReadObject();
            return (Asn1Object)obj[(obj[0] is Asn1TaggedObject) ? 3 : 2];
        }

        public static X509Name GetSubjectFields(X509Certificate cert)
        {
            if (cert != null)
            {
                return new X509Name((Asn1Sequence)GetSubject(cert.GetTbsCertificate()));
            }

            return null;
        }

        private static Asn1Object GetSubject(byte[] enc)
        {
            Asn1Sequence obj = (Asn1Sequence)new Asn1InputStream(new MemoryStream(enc)).ReadObject();
            return (Asn1Object)obj[(obj[0] is Asn1TaggedObject) ? 5 : 4];
        }
    }
}
