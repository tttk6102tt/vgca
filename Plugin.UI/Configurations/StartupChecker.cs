using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Plugin.UI.Configurations
{
    public class StartupChecker_NotUse
    {
        private readonly string CERT_DIR = "certs";
        private BackgroundWorker bg = new BackgroundWorker();

        public event EventHandler<ProgressChangedEventArgs> StartupChecking;

        public StartupChecker_NotUse()
        {
            this.bg.WorkerReportsProgress = true;
            this.bg.WorkerSupportsCancellation = true;
            this.bg.DoWork += new DoWorkEventHandler(this.bg_DoWork);
            this.bg.ProgressChanged += new ProgressChangedEventHandler(this.bg_ProgressChanged);
        }

        private void CheckingRootCACert()
        {
            this.bg.ReportProgress(0, (object)"Kiểm tra chứng thư số rootca...");
            StartupChecker_NotUse.CertInfo rootCaCert = StartupChecker_NotUse.CertInfo.GetRootCACert();
            bool flag = false;
            X509Store x509Store1 = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            x509Store1.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 certificate in x509Store1.Certificates)
            {
                if (new StartupChecker_NotUse.CertInfo(certificate).Thumbprint.Equals(rootCaCert.Thumbprint, StringComparison.InvariantCultureIgnoreCase))
                {
                    flag = true;
                    break;
                }
            }
            x509Store1.Close();
            if (!flag)
            {
                this.bg.ReportProgress(0, (object)"Cài đặt chứng thư số rootca...");
                X509Store x509Store2 = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                x509Store2.Open(OpenFlags.MaxAllowed);
                x509Store2.Add(rootCaCert.X509);
                x509Store2.Close();
            }
            this.bg.ReportProgress(0, (object)"Hoàn thành");
        }

        private void CheckingSubCACert()
        {
            this.bg.ReportProgress(0, (object)"Kiểm tra chứng thư số subca...");
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.CERT_DIR));
            if (!directoryInfo.Exists)
            {
                this.bg.ReportProgress(0, (object)"Không tìm thấy thư mục cài đặt chứng thư số!");
            }
            else
            {
                List<X509Certificate2> x509Certificate2List = new List<X509Certificate2>();
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    if (".cer.crt.der".Contains(file.Extension))
                    {
                        try
                        {
                            X509Certificate2 cert = new X509Certificate2(file.FullName);
                            StartupChecker_NotUse.CertInfo certInfo = new StartupChecker_NotUse.CertInfo(cert);
                            if ((certInfo.KeyUsages & (X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.KeyCertSign)) != X509KeyUsageFlags.None)
                            {
                                if (!certInfo.SelfSigned)
                                    x509Certificate2List.Add(cert);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                this.bg.ReportProgress(0, (object)"Cài đặt chứng thư số subca...");
                X509Store x509Store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser);
                x509Store.Open(OpenFlags.MaxAllowed);
                foreach (X509Certificate2 certificate in x509Certificate2List)
                    x509Store.Add(certificate);
                x509Store.Close();
                this.bg.ReportProgress(0, (object)"Hoàn thành");
            }
        }

        private void ApplyStartupChecked() => Configuration.SetConfigByKey(ConfigKey.StartupChecked, (object)true);

        public void Check()
        {
            object configByKey = Configuration.GetConfigByKey(ConfigKey.StartupChecked);
            if ((configByKey != null ? (Convert.ToBoolean(configByKey) ? 1 : 0) : 0) != 0)
                return;
            if (this.bg.IsBusy)
                this.bg.CancelAsync();
            while (this.bg.CancellationPending)
                Thread.Sleep(10);
            this.bg.RunWorkerAsync();
        }

        private void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            this.CheckingSubCACert();
            this.CheckingRootCACert();
            this.ApplyStartupChecked();
            this.bg.ReportProgress(100, (object)"Hoàn thành");
        }

        private void bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (this.StartupChecking == null)
                return;
            this.StartupChecking(sender, e);
        }

        private class CertInfo
        {
            private X509Certificate2 _cert;

            public X509Certificate2 X509 => this._cert;

            public string PEM
            {
                get
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("-----BEGIN CERTIFICATE-----");
                    stringBuilder.Append(Convert.ToBase64String(this._cert.RawData));
                    stringBuilder.Append("-----END CERTIFICATE-----");
                    return stringBuilder.ToString();
                }
            }

            public byte[] RawData => this._cert.RawData;

            public int Version => this._cert.Version;

            public string SerialNumber => this._cert.SerialNumber;

            public string SignatureAlgorithm => this._cert.SignatureAlgorithm.FriendlyName;

            public string Issuer => this._cert.Issuer;

            public string ValidFrom => this._cert.NotBefore.ToString();

            public string ValidTo => this._cert.NotAfter.ToString();

            public string Subject => this._cert.Subject;

            public string PublicKeyString => string.Format("RSA({0} bits), {1}", (object)this._cert.PublicKey.Key.KeySize.ToString(), (object)this._cert.PublicKey.EncodedKeyValue.Format(false));

            public string Thumbprint => this._cert.Thumbprint;

            public string BasicConstraints
            {
                get
                {
                    string basicConstraints = "";
                    foreach (X509Extension extension in this._cert.Extensions)
                    {
                        if (extension.Oid.Value == "2.5.29.19")
                        {
                            X509BasicConstraintsExtension constraintsExtension = (X509BasicConstraintsExtension)extension;
                            basicConstraints = string.Format("Subject Type={0},Path Length Constraint={1}", constraintsExtension.CertificateAuthority ? (object)"CA" : (object)"End Entity", constraintsExtension.HasPathLengthConstraint ? (object)constraintsExtension.PathLengthConstraint.ToString() : (object)"None");
                            break;
                        }
                    }
                    return basicConstraints;
                }
            }

            public string SubjectKeyIdentifier
            {
                get
                {
                    string subjectKeyIdentifier = "";
                    foreach (X509Extension extension in this._cert.Extensions)
                    {
                        if (extension.Oid.Value == "2.5.29.14")
                        {
                            subjectKeyIdentifier = ((X509SubjectKeyIdentifierExtension)extension).SubjectKeyIdentifier;
                            break;
                        }
                    }
                    return subjectKeyIdentifier;
                }
            }

            public string AuthorityKeyIdentifier
            {
                get
                {
                    string authorityKeyIdentifier = "";
                    foreach (X509Extension extension in this._cert.Extensions)
                    {
                        if (extension.Oid.Value == "2.5.29.35")
                        {
                            authorityKeyIdentifier = extension.Format(true);
                            break;
                        }
                    }
                    return authorityKeyIdentifier;
                }
            }

            public string SubjectAlternativeName
            {
                get
                {
                    string subjectAlternativeName = "";
                    foreach (X509Extension extension in this._cert.Extensions)
                    {
                        if (extension.Oid.Value == "2.5.29.17")
                        {
                            subjectAlternativeName = extension.Format(true);
                            break;
                        }
                    }
                    return subjectAlternativeName;
                }
            }

            public string CRLDistributionPoints
            {
                get
                {
                    string distributionPoints = "";
                    foreach (X509Extension extension in this._cert.Extensions)
                    {
                        if (extension.Oid.Value == "2.5.29.31")
                        {
                            distributionPoints = extension.Format(true);
                            break;
                        }
                    }
                    return distributionPoints;
                }
            }

            public string AuthorityInformationAccess
            {
                get
                {
                    string informationAccess = "";
                    foreach (X509Extension extension in this._cert.Extensions)
                    {
                        if (extension.Oid.Value == "1.3.6.1.5.5.7.1.1")
                        {
                            informationAccess = extension.Format(true);
                            break;
                        }
                    }
                    return informationAccess;
                }
            }

            public string IssuerName => this._cert.GetNameInfo(X509NameType.SimpleName, true);

            public string O
            {
                get
                {
                    string[] strArray = this._cert.Subject.Split(new char[1]
                    {
            ','
                    }, StringSplitOptions.RemoveEmptyEntries);
                    string o = string.Empty;
                    for (int index = 0; index < strArray.Length; ++index)
                    {
                        string str = strArray[index].Trim();
                        if (str.IndexOf("O=") == 0)
                        {
                            o = str.Substring(2);
                            break;
                        }
                    }
                    return o;
                }
            }

            public string OU
            {
                get
                {
                    string[] strArray = this._cert.Subject.Split(new char[1]
                    {
            ','
                    }, StringSplitOptions.RemoveEmptyEntries);
                    string ou = string.Empty;
                    for (int index = 0; index < strArray.Length; ++index)
                    {
                        string str = strArray[index].Trim();
                        if (str.IndexOf("OU=") == 0)
                            ou = !string.IsNullOrEmpty(ou) ? str.Substring(3) + ", " + ou : str.Substring(3);
                    }
                    return ou;
                }
            }

            public string Email => this._cert.GetNameInfo(X509NameType.EmailName, false);

            public string CommonName => this._cert.GetNameInfo(X509NameType.SimpleName, false);

            public string ShortName => this._cert.GetNameInfo(X509NameType.SimpleName, false).Replace("(M)", "").Trim();

            public string Period
            {
                get
                {
                    DateTime dateTime = this._cert.NotBefore;
                    string str1 = dateTime.ToString("dd/MM/yyyy");
                    dateTime = this._cert.NotAfter;
                    string str2 = dateTime.ToString("dd/MM/yyyy");
                    return string.Format("{0} đến {1}", (object)str1, (object)str2);
                }
            }

            public X509KeyUsageFlags KeyUsages
            {
                get
                {
                    X509KeyUsageExtension extension = (X509KeyUsageExtension)this._cert.Extensions["2.5.29.15"];
                    return extension != null ? extension.KeyUsages : X509KeyUsageFlags.None;
                }
            }

            public bool SelfSigned => this.Subject.Equals(this.Issuer, StringComparison.OrdinalIgnoreCase);

            public CertInfo(byte[] rawData) => this._cert = new X509Certificate2(rawData);

            public CertInfo(X509Certificate2 cert) => this._cert = cert;

            public CertInfo(string base64) => this._cert = new X509Certificate2(StartupChecker_NotUse.CertInfo.EncodedFormatCert(base64));

            public string GetKeyUsagesString()
            {
                X509KeyUsageExtension extension = (X509KeyUsageExtension)this._cert.Extensions["2.5.29.15"];
                return extension != null ? extension.Format(true) : X509KeyUsageFlags.None.ToString();
            }

            public override string ToString() => string.IsNullOrEmpty(this.Email) ? this.CommonName : string.Format("{0}<{1}>", (object)this.CommonName, (object)this.Email);

            public static byte[] EncodedFormatCert(string base64Format)
            {
                try
                {
                    string str1 = "-----BEGIN CERTIFICATE-----";
                    string str2 = "-----END CERTIFICATE-----";
                    int num1 = base64Format.IndexOf(str1);
                    int num2 = base64Format.IndexOf(str2);
                    return Convert.FromBase64String(base64Format.Substring(num1 + 27, num2 - num1 - 27).Replace("\r", "").Replace("\n", "").Replace(Environment.NewLine, ""));
                }
                catch
                {
                    return (byte[])null;
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
                    return (byte[])null;
                }
            }

            public static StartupChecker_NotUse.CertInfo GetRootCACert() => new StartupChecker_NotUse.CertInfo("-----BEGIN CERTIFICATE-----MIID+DCCAuCgAwIBAgIJAP8wOuTpCsHtMA0GCSqGSIb3DQEBBQUAMGsxCzAJBgNVBAYTAlZOMR0wGwYDVQQKDBRCYW4gQ28geWV1IENoaW5oIHBodTE9MDsGA1UEAww0Q28gcXVhbiBjaHVuZyB0aHVjIHNvIGNodXllbiBkdW5nIENoaW5oIHBodSAoUm9vdENBKTAeFw0xMDAzMTAwNTQ1NTdaFw0zMDAzMDUwNTQ1NTdaMGsxCzAJBgNVBAYTAlZOMR0wGwYDVQQKDBRCYW4gQ28geWV1IENoaW5oIHBodTE9MDsGA1UEAww0Q28gcXVhbiBjaHVuZyB0aHVjIHNvIGNodXllbiBkdW5nIENoaW5oIHBodSAoUm9vdENBKTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBANrzvexkvgul4dRUnV6GMcvLdenKrrzYnVpzIp78ijBMqWcG+cu+AJS2GbqYdbsO6JnaNLSxuxpM7Uejiwi2QBTe2NXIy4TtkadbIjPlQHUIetTYeLTESUw0vOEuwtAM2PVmoSpdEPFw4o06E3/MCtiM0fSRuyyXM8uu0EyYqUowFJbEDERqqlPeU0okutsgzUFtZkG/TM6WE97FMbA4KC5stxG8SHCe4YFNrQIaM8Ozemd11MIJaSHSvrv+EWR1TDeg02U18qB3aiaamSX2M7B3JMKedOoBo1UQkLc/ePqG2kKHVbc2p1mePX5n1etCpM6+RUjpzvdkcihxxAUjJAcCAwEAAaOBnjCBmzAPBgNVHRMBAf8EBTADAQH/MB0GA1UdDgQWBBTZFxtRoxe3nvwt22H6eQD/WHSdXDAfBgNVHSMEGDAWgBTZFxtRoxe3nvwt22H6eQD/WHSdXDAOBgNVHQ8BAf8EBAMCAQYwOAYDVR0fBDEwLzAtoCugKYYnaHR0cDovL2NhLmdvdi52bi9wa2kvcHViL2NybC9yb290Y2EuY3JsMA0GCSqGSIb3DQEBBQUAA4IBAQAbivpvhtC3w/9gWAh34UovGuSUwFDQOcmUTExhhJiADI18E49WBTeN1iC7oZhb1aFRQzW9e6NNgkSrCy5pik1gkdOtgB+qx2b3s9CCj8VNywlADH9ziMmXPgyJLv0n9TqBj7yTWT85Yc49er0nsDdvxSBqlJiiu/SGD6ZMda/mztJnkrteTAka2zw2i46rcwTSURjyYEJfpj/joxEcCqAubXwIdteNWjMhz07MrPXDa7OGdn7ppLpZEIHmSCZR+ULILtrd3cTDAzRlIP9bNzg1wc0bf4IY9ErVFZAPlnx6wxxIIOWp+JBRpf1TiKu73Q990Pmcpk92bAk68y20xRIl-----END CERTIFICATE-----");
        }
    }
}
