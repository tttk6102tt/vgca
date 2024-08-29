using Sign.Classes;
using Sign.Enums;
using Sign.itext.pdf.security;
using Sign.Org.BouncyCastle.X509;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Sign.X509
{
    public enum CertCheckResult
    {
        Ok = 0,
        CertExpired = 1,
        CertNotYetValid = 2,
        InvalidCertChain = 3,
        UntrustedRoot = 4,
        InvalidCrlDistPoints = 5,
        CouldnotDownloadCrl = 6,
        OnlineCheckingCertDisabled = 7,
        CertIsRevoked = 8,
        CaCertIsRevoked = 9,
        InvalidCrl = 10,
        OcspRespUnknown = 11
    }
    public class CertChecker
    {
        private X509Certificate2 _certificate;

        public WebProxy Proxy { get; set; }

        private Org.BouncyCastle.X509.X509Certificate[] x509Certificate_0;

        private DateTime _checkingDateTime;

        private bool _onlineCheckingAllowed = true;

        private bool _checkingViaOcsp;

        private string[] _additionalCRLs;

        public DateTime CheckingDateTime
        {
            get
            {
                return _checkingDateTime;
            }
            set
            {
                _checkingDateTime = value;
            }
        }

        public bool OnlineCheckingAllowed
        {
            get
            {
                return _onlineCheckingAllowed;
            }
            set
            {
                _onlineCheckingAllowed = value;
            }
        }

        public bool CheckingViaOcsp
        {
            get
            {
                return _checkingViaOcsp;
            }
            set
            {
                _checkingViaOcsp = value;
            }
        }

        public string[] AdditionalCRLs
        {
            get
            {
                return _additionalCRLs;
            }
            set
            {
                _additionalCRLs = value;
            }
        }

        public CertChecker(X509Certificate2 certificate, DateTime checkingDatetime, bool onlineCheckingAllowed = true, bool checkingViaOcsp = false, string[] additionalCRLs = null)
        {
            _certificate = certificate;
            _checkingDateTime = checkingDatetime;
            _onlineCheckingAllowed = onlineCheckingAllowed;
            _checkingViaOcsp = checkingViaOcsp;
            _additionalCRLs = additionalCRLs;
        }

        private int method_0()
        {
            return 0;
        }

        private static string smethod_0()
        {
            return "90273e913b6f7012863306ff4d97e0e03e441242";
        }



        private static List<CerProvide> getCerProvide()
        {
            return new List<CerProvide>()
            {
                new CerProvide()
                {
                    CompanyName = "Co quan chung thuc so chuyen dung Chinh phu (RootCA)",
                    KeyCer = "6a51d34ae7141f45e8160dcf0051b4fe387ef62b"
                },
                new CerProvide()
                {
                    CompanyName = "BKAVCA_BIN",
                    KeyCer = "96c501f3550ee2de58bb47d2993e857161b019cc"
                },
                new CerProvide()
                {
                    CompanyName = "BNG_BIN",
                    KeyCer = "78edee138cc4bed91d56fa9e6d7afce4ee81b9ba"
                },
                new CerProvide()
                {
                    CompanyName = "BTC_BIN",
                    KeyCer = "785aa4c078e31d9cf67eaa42f6940b1ee1bd2c36"
                },
                new CerProvide()
                {
                    CompanyName = "CA2_BIN",
                    KeyCer = "fb3588819753b2fb8a8318d43b31ba5e555babfb"
                },
                new CerProvide()
                {
                    CompanyName = "CP_BIN",
                    KeyCer = "/*0729c929d018b97f1e1b9f23be603a4593061926*/"
                },
                new CerProvide()
                {
                    CompanyName = "DCS_BIN",
                    KeyCer = "0bc1ddd3777e8eb9e49c3484e6473817ae814ce6"
                },
                    new CerProvide()
                {
                    CompanyName = "FPTCA_BIN",
                    KeyCer = "2d1b8e8738c2bcf62685e18ea55d8ebd63c6165a"
                },
                        new CerProvide()
                {
                    CompanyName = "MIC_BIN",
                    KeyCer = "42843bc401476cda242034b945bbf409a6bdd5c7"
                },
                new CerProvide()
                {
                    CompanyName = "NEWTELCA_BIN",
                    KeyCer = "3f34ee1709df90e787342b810a8c610b9cd14807"
                },
                new CerProvide()
                {
                    CompanyName = "SafeCA_BIN",
                    KeyCer = "6b0ffe76a18af51686c04cdaedc8bb6c5f24b4b3"
                },
                new CerProvide()
                {
                    CompanyName = "SMARTSIGN_BIN",
                    KeyCer = "5a8c1f27bfd9accbbc3c9455a63620d3bfa6bb5d"
                },
                new CerProvide()
                {
                    CompanyName = "VGCA_ROOTCA_BIN",
                    KeyCer = "90273e913b6f7012863306ff4d97e0e03e441242"
                },
                new CerProvide()
                {
                    CompanyName = "VGCALicenseKeyGenerator",
                    KeyCer = "ee84a5991a97883485ae6d5631b89699a1d559f0"
                },
                new CerProvide()
                {
                    CompanyName = "VIETTELCA_BIN",
                    KeyCer = "587a00b53a56d9a72fe4d5a3d5e21c7fb225f2d8"
                },
                new CerProvide()
                {
                    CompanyName = "VNPTCA_BIN",
                    KeyCer = "42a9b879177da96ab659a503dc6f2b4dc8d59622"
                },
                new CerProvide()
                {
                    CompanyName = "4Bit",
                    KeyCer = "761A8D4043C70E020B8578B78F89BC16A637918B"
                },
                new CerProvide
                {
                    CompanyName = "sbv",
                    KeyCer = "e21f8d56c2761df8a8e974c1f521fd6e7a8fbf1b"
                }
            };
        }
        private string GetCrlDownloadPath()
        {
            string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HiPT", "crl");
            if (!Directory.Exists(text))
            {
                try
                {
                    Directory.CreateDirectory(text);
                    return text;
                }
                catch
                {
                    return Path.GetTempPath();
                }
            }
            return text;
        }

        private int CheckValidityPeriod()
        {
            if (_checkingDateTime.CompareTo(_certificate.NotAfter) > 0)
            {
                return (int)CertCheckResult.CertExpired;
            }
            if (_checkingDateTime.CompareTo(_certificate.NotBefore) < 0)
            {
                return (int)CertCheckResult.CertNotYetValid;
            }
            return (int)CertCheckResult.Ok;
        }

        private int CheckCertificateChain()
        {
            X509ChainPolicy x509ChainPolicy = new X509ChainPolicy();
            x509ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            x509ChainPolicy.VerificationTime = _checkingDateTime;
            x509ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
            X509Chain x509Chain = new X509Chain();
            x509Chain.ChainPolicy = x509ChainPolicy;
            if (!x509Chain.Build(_certificate))
            {
                return (int)CertCheckResult.InvalidCertChain;
            }
            x509Certificate_0 = new Org.BouncyCastle.X509.X509Certificate[x509Chain.ChainElements.Count];
            X509CertificateParser x509CertificateParser = new X509CertificateParser();
            string value = null;
            for (int i = 0; i < x509Chain.ChainElements.Count; i++)
            {
                Org.BouncyCastle.X509.X509Certificate x509Certificate = x509CertificateParser.ReadCertificate(x509Chain.ChainElements[i].Certificate.RawData);
                x509Certificate_0[i] = x509Certificate;

                value = x509Chain.ChainElements[i].Certificate.Thumbprint;
                if (CertInfo.IsSelfSigned(x509Certificate))
                {
                    value = x509Chain.ChainElements[i].Certificate.Thumbprint;
                }
            }
            if (!getCerProvide().ConvertAll(s => s.KeyCer.ToLower()).Contains(value.ToLower()))
            {
                return (int)CertCheckResult.UntrustedRoot;
            }
            return (int)CertCheckResult.Ok;
        }

        private static bool IsCrlValid(X509Crl crl, Org.BouncyCastle.X509.X509Certificate issuerCertificate, DateTime checkingDateTime)
        {
            try
            {
                crl.Verify(issuerCertificate.GetPublicKey());
                return crl.NextUpdate.Value >= checkingDateTime;
            }
            catch
            {
                return false;
            }
        }

        private X509Crl LoadCrlFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                X509CrlParser x509CrlParser = new X509CrlParser();
                FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                try
                {
                    return x509CrlParser.ReadCrl(fileStream);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi đọc CRL từ file: {ex.Message}");
                    return null;
                }
                finally
                {
                    fileStream.Close();
                }
            }

            return null;
        }
        /// <summary>
        /// Tải nội dung từ một URL và lưu vào file.
        /// </summary>
        /// <param name="url">URL cần tải nội dung.</param>
        /// <param name="filePath">Đường dẫn đến file để lưu nội dung.</param>
        /// <returns>True nếu tải thành công, False nếu có lỗi xảy ra.</returns>
        private bool DownloadFileCrl(string url, string filePath)
        {
            bool result = false;
            Stream downloadStream = null;
            Stream fileStream = null;
            WebResponse webResponse = null;
            try
            {
                WebRequest webRequest = WebRequest.Create(url);
                if (webRequest != null)
                {
                    webResponse = webRequest.GetResponse();
                    if (webResponse != null)
                    {
                        downloadStream = webResponse.GetResponseStream();
                        fileStream = File.Create(filePath);
                        byte[] array = new byte[1024];
                        int num;
                        do
                        {
                            num = downloadStream.Read(array, 0, array.Length);
                            fileStream.Write(array, 0, num);
                        }
                        while (num > 0);
                        result = true;
                        return result;
                    }
                    return result;
                }
                return result;
            }
            catch (Exception exception_)
            {
                ExceptionUtils.LogException("Lỗi tải CRL", exception_);
                return result;
            }
            finally
            {
                webResponse?.Close();
                downloadStream?.Close();
                fileStream?.Close();
            }
        }

        private int CheckCertificateRevocationStatus(Org.BouncyCastle.X509.X509Certificate certificate, Org.BouncyCastle.X509.X509Certificate issuerCertificate)
        {
            string cRLURL = CertificateUtil.GetCRLURL(certificate);
            if (string.IsNullOrEmpty(cRLURL))
            {
                return (int)CertCheckResult.InvalidCrlDistPoints;
            }
            string crlFilePath = Path.Combine(GetCrlDownloadPath(), Path.GetFileName(cRLURL));
            bool crlDownloaded = false;
            X509Crl x509Crl = null;
            do
            {
                x509Crl = LoadCrlFromFile(crlFilePath);
                if (IsCrlValid(x509Crl, issuerCertificate, _checkingDateTime))
                {
                    break;
                }
                if (!crlDownloaded)
                {
                    List<string> crlUrlsToTry = new List<string>();
                    crlUrlsToTry.Add(cRLURL);
                    if (_additionalCRLs != null && _additionalCRLs.Length != 0)
                    {
                        string fileName = Path.GetFileName(cRLURL);
                        string[] array = _additionalCRLs;
                        foreach (string text2 in array)
                        {
                            if (Path.GetFileName(text2) == fileName)
                            {
                                crlUrlsToTry.Add(text2);
                            }
                        }
                    }
                    bool flag2 = false;
                    foreach (string urlToTry in crlUrlsToTry)
                    {
                        if (!DownloadFileCrl(urlToTry, crlFilePath))
                        {
                            Thread.Sleep(10);
                            continue;
                        }
                        flag2 = true;
                        break;
                    }
                    if (flag2)
                    {
                        crlDownloaded = true;
                        continue;
                    }
                    return (int)CertCheckResult.CouldnotDownloadCrl;
                }
                return (int)CertCheckResult.InvalidCrl;
            }
            while (x509Crl == null || !crlDownloaded);
            if (x509Crl.IsRevoked(certificate) && x509Crl.GetRevokedCertificate(certificate.SerialNumber).RevocationDate.ToLocalTime() < _checkingDateTime)
            {
                return (int)CertCheckResult.CertIsRevoked;
            }
            return (int)CertCheckResult.Ok;
        }

        private int CheckCertificateRevocationStatus(Org.BouncyCastle.X509.X509Certificate[] certificateChain)
        {
            if (_checkingViaOcsp)
            {
                OCSPClient @class = new OCSPClient();
                Org.BouncyCastle.X509.X509Certificate certificate = new X509CertificateParser().ReadCertificate(_certificate.RawData);
                Org.BouncyCastle.X509.X509Certificate issuerCertificate = null;
                foreach (Org.BouncyCastle.X509.X509Certificate chainCertificate in certificateChain)
                {
                    try
                    {
                        certificate.Verify(chainCertificate.GetPublicKey());
                        issuerCertificate = chainCertificate;
                    }
                    catch
                    {
                        continue;
                    }
                    break;
                }
                switch (@class.CheckCertificateRevocation(certificate, issuerCertificate))
                {
                    case ENUM_CERTIFICATESTATUS.Revoked:
                        return (int)CertCheckResult.CertIsRevoked;
                    case ENUM_CERTIFICATESTATUS.Unknown:
                        return (int)CertCheckResult.OcspRespUnknown;
                }
            }
            else
            {
                for (int i = 0; i < certificateChain.Length; i++)
                {
                    Org.BouncyCastle.X509.X509Certificate currentCertificate = certificateChain[i];
                    Org.BouncyCastle.X509.X509Certificate issuerCertificate = (i < certificateChain.Length - 1) ? certificateChain[i + 1] : currentCertificate;

                    int revocationStatus = CheckCertificateRevocationStatus(currentCertificate, issuerCertificate);
                    if (revocationStatus == (int)CertCheckResult.CertIsRevoked && i != 0)
                    {
                        // Chứng chỉ trung gian đã bị thu hồi.
                        return (int)CertCheckResult.CaCertIsRevoked;
                    }

                    // Trả về trạng thái thu hồi của chứng chỉ hiện tại.
                    return revocationStatus;
                }
            }
            return (int)CertCheckResult.Ok;
        }

        public int Check()
        {
            int resultCode = CheckValidityPeriod();
            if (resultCode != (int)CertCheckResult.Ok)
            {
                return resultCode;
            }
            resultCode = CheckCertificateChain();
            if (resultCode != (int)CertCheckResult.Ok)
            {
                return resultCode;
            }
            if (_onlineCheckingAllowed)
            {
                return CheckCertificateRevocationStatus(x509Certificate_0);
            }
            return (int)CertCheckResult.OnlineCheckingCertDisabled;
        }

        public static bool IsValidCert(X509Certificate2 cert)
        {
            X509ChainPolicy chainPolicy = new X509ChainPolicy();
            chainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chainPolicy.VerificationTime = DateTime.Now;
            chainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
            X509Chain chain = new X509Chain();
            chain.ChainPolicy = chainPolicy;
            if (!chain.Build(cert))
            {
                return false;
            }
            //string text = smethod_0();
            List<CerProvide> trustedCertificates = getCerProvide();


            for (int i = chain.ChainElements.Count - 1; i >= 0; i--)
            {
                if (trustedCertificates.ConvertAll(c => c.KeyCer.ToLower()).Contains(chain.ChainElements[i].Certificate.Thumbprint.ToLower()))
                {
                    // Tìm thấy chứng chỉ tin cậy trong chuỗi chứng chỉ.
                    return true;
                }
            }

            return true;
        }
    }
}
