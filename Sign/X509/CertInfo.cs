using Sign.Org.BouncyCastle.X509;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Sign.X509
{
    public class CertInfo
    {
        private X509Certificate2 _certificate;

        public X509Certificate2 Certificate
        {
            get
            {
                return _certificate;
            }
        }
        public List<string> OUs { get; } = new List<string>();
        public string PEM
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("-----BEGIN CERTIFICATE-----");
                stringBuilder.Append(Convert.ToBase64String(_certificate.RawData));
                stringBuilder.Append("-----END CERTIFICATE-----");
                return stringBuilder.ToString();
            }
        }

        public byte[] RawData
        {
            get
            {
                return _certificate.RawData;
            }
        }

        public int Version
        {
            get
            {
                return _certificate.Version;
            }
        }

        public string SerialNumber
        {
            get
            {
                return _certificate.SerialNumber;
            }
        }

        public string SignatureAlgorithm
        {
            get
            {
                return _certificate.SignatureAlgorithm.FriendlyName;
            }
        }

        public string Issuer
        {
            get
            {
                return _certificate.Issuer;
            }
        }

        public string ValidFrom
        {
            get
            {
                return _certificate.NotBefore.ToString();
            }
        }

        public string ValidTo
        {
            get
            {
                return _certificate.NotAfter.ToString();
            }
        }

        public string Subject
        {
            get
            {
                return _certificate.Subject;
            }
        }

        public string PublicKeyString
        {
            get
            {
                return string.Format("RSA({0} bits), {1}", _certificate.PublicKey.Key.KeySize.ToString(), _certificate.PublicKey.EncodedKeyValue.Format(multiLine: false));
            }
        }
        public string Thumbprint
        {
            get
            {
                return _certificate.Thumbprint.ToString();
            }
        }

        public string BasicConstraints
        {
            get
            {
                string result = "";
                X509ExtensionEnumerator enumerator = _certificate.Extensions.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Extension current = enumerator.Current;
                    if (current.Oid.Value == "2.5.29.19")
                    {
                        X509BasicConstraintsExtension x509BasicConstraintsExtension = (X509BasicConstraintsExtension)current;
                        result = string.Format("Subject Type={0},Path Length Constraint={1}", x509BasicConstraintsExtension.CertificateAuthority ? "CA" : "End Entity", x509BasicConstraintsExtension.HasPathLengthConstraint ? x509BasicConstraintsExtension.PathLengthConstraint.ToString() : "None");
                        break;
                    }
                }
                return result;
            }
        }

        public string SubjectKeyIdentifier
        {
            get
            {
                string result = "";
                X509ExtensionEnumerator enumerator = _certificate.Extensions.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Extension current = enumerator.Current;
                    if (current.Oid.Value == "2.5.29.14")
                    {
                        result = ((X509SubjectKeyIdentifierExtension)current).SubjectKeyIdentifier;
                        break;
                    }
                }
                return result;
            }
        }

        public string AuthorityKeyIdentifier
        {
            get
            {
                string result = "";
                X509ExtensionEnumerator enumerator = _certificate.Extensions.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Extension current = enumerator.Current;
                    if (current.Oid.Value == "2.5.29.35")
                    {
                        result = current.Format(multiLine: true);
                        break;
                    }
                }
                return result;
            }
        }

        public string SubjectAlternativeName
        {
            get
            {
                string result = "";
                X509ExtensionEnumerator enumerator = _certificate.Extensions.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Extension current = enumerator.Current;
                    if (current.Oid.Value == "2.5.29.17")
                    {
                        result = current.Format(multiLine: true);
                        break;
                    }
                }
                return result;
            }
        }

        public string CRLDistributionPoints
        {
            get
            {
                string result = "";
                X509ExtensionEnumerator enumerator = _certificate.Extensions.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Extension current = enumerator.Current;
                    if (current.Oid.Value == "2.5.29.31")
                    {
                        result = current.Format(multiLine: true);
                        break;
                    }
                }
                return result;
            }
        }

        public string AuthorityInformationAccess
        {
            get
            {
                string result = "";
                X509ExtensionEnumerator enumerator = _certificate.Extensions.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Extension current = enumerator.Current;
                    if (current.Oid.Value == "1.3.6.1.5.5.7.1.1")
                    {
                        result = current.Format(multiLine: true);
                        break;
                    }
                }
                return result;
            }
        }

        public string IssuerName
        {
            get
            {
                return _certificate.GetNameInfo(X509NameType.SimpleName, forIssuer: true);
            }
        }

        public string O
        {
            get
            {
                string subject = _certificate.Subject;
                string[] array = subject.Split(new char[1]
                {
                    ','
                }, StringSplitOptions.RemoveEmptyEntries);
                string result = string.Empty;
                for (int i = 0; i < array.Length; i++)
                {
                    subject = array[i].Trim();
                    if (subject.IndexOf("O=") == 0)
                    {
                        result = subject.Substring(2);
                        break;
                    }
                }
                return result;
            }
        }

        public string OU
        {
            get
            {
                string subject = _certificate.Subject;
                string[] array = subject.Split(new char[1]
                {
                    ','
                }, StringSplitOptions.RemoveEmptyEntries);
                string text = string.Empty;
                for (int i = 0; i < array.Length; i++)
                {
                    subject = array[i].Trim();
                    if (subject.IndexOf("OU=") == 0)
                    {
                        text = ((!string.IsNullOrEmpty(text)) ? (subject.Substring(3) + ", " + text) : subject.Substring(3));
                    }
                }
                return text;
            }
        }

        public string Email
        {
            get
            {
                return _certificate.GetNameInfo(X509NameType.EmailName, forIssuer: false);
            }
        }

        //public string Email => x509Certificate2_0.GetNameInfo(X509NameType.EmailName, forIssuer: false);


        public string CommonName
        {
            get
            {
                return _certificate.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
            }
        }
        //public string CommonName => x509Certificate2_0.GetNameInfo(X509NameType.SimpleName, forIssuer: false);

        public string ShortName
        {
            get
            {
                return _certificate.GetNameInfo(X509NameType.SimpleName, forIssuer: false).Replace("(M)", "").Trim();
            }
        }

        //public string ShortName => x509Certificate2_0.GetNameInfo(X509NameType.SimpleName, forIssuer: false).Replace("(M)", "").Trim();


        public string Period
        {
            get
            {
                return string.Format("{0} đến {1}", _certificate.NotBefore.ToString("dd/MM/yyyy"), _certificate.NotAfter.ToString("dd/MM/yyyy"));
            }
        }

        //public string Period => string.Format("{0} đến {1}", x509Certificate2_0.NotBefore.ToString("dd/MM/yyyy"), x509Certificate2_0.NotAfter.ToString("dd/MM/yyyy"));

        public X509KeyUsageFlags KeyUsages
        {
            get
            {
                return ((X509KeyUsageExtension)_certificate.Extensions["2.5.29.15"]) == null ? X509KeyUsageFlags.None : ((X509KeyUsageExtension)_certificate.Extensions["2.5.29.15"]).KeyUsages;
            }
        }


        public bool SelfSigned
        {
            get
            {
                return IsSelfSigned(new X509CertificateParser().ReadCertificate(_certificate.RawData));
            }
        }

        public CertInfo(byte[] rawData)
        {
            _certificate = new X509Certificate2(rawData);
        }

        public CertInfo(X509Certificate2 cert)
        {
            _certificate = cert;
        }

        public CertInfo(string base64)
        {
            _certificate = new X509Certificate2(EncodedFormatCert(base64));
        }

        public string GetKeyUsagesString()
        {
            X509KeyUsageExtension x509KeyUsageExtension = (X509KeyUsageExtension)_certificate.Extensions["2.5.29.15"];
            if (x509KeyUsageExtension != null)
            {
                return x509KeyUsageExtension.Format(multiLine: true);
            }
            return X509KeyUsageFlags.None.ToString();
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Email))
            {
                return CommonName;
            }
            return string.Format("{0}<{1}>", CommonName, Email);
        }

        public static byte[] EncodedFormatCert(string base64Format)
        {
            try
            {
                string value = "-----BEGIN CERTIFICATE-----";
                string value2 = "-----END CERTIFICATE-----";
                int num = base64Format.IndexOf(value);
                int num2 = base64Format.IndexOf(value2);
                return Convert.FromBase64String(base64Format.Substring(num + 27, num2 - num - 27).Replace("\r", "").Replace("\n", "")
                    .Replace(Environment.NewLine, ""));
            }
            catch
            {
                return null;
            }
        }

        public static byte[] GetCertFromFile(string fileName)
        {
            try
            {
                return new X509Certificate2(fileName).RawData;
            }
            catch
            {
                return null;
            }
        }

        public static bool IsSelfSigned(Org.BouncyCastle.X509.X509Certificate cert)
        {
            try
            {
                if (!cert.SubjectDN.Equivalent(cert.IssuerDN))
                {
                    return false;
                }
                cert.Verify(cert.GetPublicKey());
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
