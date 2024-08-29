using Sign.Configurations;
using Sign.Enums;
using Sign.PDF;
using System.Net;

namespace Sign.ConfigConfigurations
{
    public class ConfigConfiguration2
    {
        public static readonly string WORKING_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HiPT\\Pdf");
        private const string REGKEY = "SOFTWARE\\HiPT";
        private const string CONNECTIONKEY = "SOFTWARE\\HIPT\\Connection";
        private const string SIGNKEY = "SOFTWARE\\HIPT\\Sign";
        private const string CERTCHECKERKEY = "SOFTWARE\\HIPT\\CertChecker";
        private const string PDFKEY = "SOFTWARE\\HIPT\\Pdf";
        private const string CERTIFICATEKEY = "SOFTWARE\\HIPT\\Certificates";

        public bool StartupChecked
        {
            get
            {
                object configByKey = ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.StartupChecked);
                bool result = false;
                return bool.TryParse((string)configByKey, out result) && result;
            }
        }

        public bool UsedProxy
        {
            get
            {
                object configByKey = ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.UseProxy);
                bool result = false;
                return bool.TryParse((string)configByKey, out result) && result;
            }
        }

        public bool AutoDetectProxy
        {
            get
            {
                object configByKey = ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.AutoDetectProxy);
                bool result = false;
                return bool.TryParse((string)configByKey, out result) && result;
            }
        }

        public bool UsePxAuth
        {
            get
            {
                object configByKey = ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.UsePxAuth);
                bool result = false;
                return bool.TryParse((string)configByKey, out result) && result;
            }
        }

        public bool BypassPxForLocal
        {
            get
            {
                object configByKey = ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.BypassPxForLocal);
                bool result = false;
                return bool.TryParse((string)configByKey, out result) && result;
            }
        }

        public bool UsedTsa
        {
            get
            {
                object configByKey = ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.UseTsa);
                bool result = false;
                return bool.TryParse((string)configByKey, out result) && result;
            }
        }

        public bool AllowedOnlineCheckingCert
        {
            get
            {
                object configByKey = ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.AllowedOnlineCheckingCert);
                if (configByKey == null)
                    return true;
                bool result = false;
                return !bool.TryParse((string)configByKey, out result) || result;
            }
        }

        public bool AllowedOCSPForCheckingSigningCert
        {
            get
            {
                object configByKey = ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.AllowedOCSPForCheckingSigningCert);
                if (configByKey == null)
                    return true;
                bool result = false;
                return !bool.TryParse((string)configByKey, out result) || result;
            }
        }

        public string[] AdditionalCrls
        {
            get
            {
                return (string[])null;
                try
                {
                    return ((string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.AdditionalCrls)).Split(new char[1]
                    {
            '|'
                    }, StringSplitOptions.RemoveEmptyEntries);
                }
                catch
                {
                    return (string[])null;
                }
            }
        }

        public int RevocationMode
        {
            get
            {
                string configByKey = (string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.RevocationMode);
                int num = 0;
                ref int local = ref num;
                return int.TryParse(configByKey, out local) ? num : 0;
            }
        }

        public string TsaAddress => (string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.TsaAddress);

        public string PxAddress => (string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.PxAddress);

        public int PxPort
        {
            get
            {
                string configByKey = (string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.PxPort);
                int num = 0;
                ref int local = ref num;
                return int.TryParse(configByKey, out local) ? num : 0;
            }
        }

        public string PxUsername => (string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.PxUsername);

        public string PxPassword => (string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.PxPassword);

        public WebProxy Proxy
        {
            get
            {
                string configByKey1 = (string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.PxAddress);
                if (string.IsNullOrEmpty(configByKey1))
                    return (WebProxy)null;
                string configByKey2 = (string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.PxPort);
                if (string.IsNullOrEmpty(configByKey2))
                    return (WebProxy)null;
                int result = 0;
                if (!int.TryParse(configByKey2, out result))
                    return (WebProxy)null;
                WebProxy proxy = new WebProxy(configByKey1, result);
                proxy.BypassProxyOnLocal = this.BypassPxForLocal;
                if (this.UsePxAuth)
                {
                    string configByKey3 = (string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.PxUsername);
                    string configByKey4 = (string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.PxPassword);
                    if (!string.IsNullOrEmpty(configByKey3) && !string.IsNullOrEmpty(configByKey4))
                        proxy.Credentials = (ICredentials)new NetworkCredential(configByKey3, configByKey4);
                }
                return proxy;
            }
        }

        public bool UpdateOnStart
        {
            get
            {
                string configByKey = (string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.UpdateOnStart);
                bool flag = false;
                ref bool local = ref flag;
                return bool.TryParse(configByKey, out local) && flag;
            }
        }

        public DateTime LastCheckingUpdate
        {
            get
            {
                DateTime result;
                DateTime.TryParse((string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.LastCheckingUpdate), out result);
                return result;
            }
        }

        public Version CurrentVersion
        {
            get
            {
                string configByKey = (string)ConfigConfiguration2.GetConfigByKey(ENUM_CONFIGKEY.CurrentVersion);
                try
                {
                    return new Version(configByKey);
                }
                catch
                {
                    return new Version("1.0.0.0");
                }
            }
        }

        public SignerProfileStore2 PdfSignerProfiles => new SignerProfileStore2(Path.Combine(ConfigConfiguration2.WORKING_PATH, "signerprofile.xml"));

        public SignerProfile PdfSignerDefault
        {
            get
            {
                try
                {
                    SignerProfileStore2 signerProfileStore = new SignerProfileStore2(Path.Combine(ConfigConfiguration2.WORKING_PATH, "signerprofile.xml"));
                    return signerProfileStore.DefaultSigner < 0 ? (SignerProfile)null : signerProfileStore[signerProfileStore.DefaultSigner];
                }
                catch
                {
                    return (SignerProfile)null;
                }
            }
        }

        public static object GetConfigByKey(ENUM_CONFIGKEY key)
        {
            XRegistry xregistry = (XRegistry)null;
            object configByKey = (object)null;
            try
            {
                switch (key)
                {
                    case ENUM_CONFIGKEY.UseProxy:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("UseProxyServer");
                        break;
                    case ENUM_CONFIGKEY.AutoDetectProxy:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("AutoDetectProxy");
                        break;
                    case ENUM_CONFIGKEY.UsePxAuth:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("UsePxAuthentication");
                        break;
                    case ENUM_CONFIGKEY.BypassPxForLocal:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("BypassPxForLocal");
                        break;
                    case ENUM_CONFIGKEY.PxAddress:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("PxAddress");
                        break;
                    case ENUM_CONFIGKEY.PxPort:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("PxPort");
                        break;
                    case ENUM_CONFIGKEY.PxUsername:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("PxUsername");
                        break;
                    case ENUM_CONFIGKEY.PxPassword:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("PxPassword");
                        break;
                    case ENUM_CONFIGKEY.UseTsa:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Sign");
                        configByKey = xregistry.Read("UseTSA");
                        break;
                    case ENUM_CONFIGKEY.TsaAddress:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Sign");
                        configByKey = xregistry.Read("TsaAddress");
                        break;
                    case ENUM_CONFIGKEY.AllowedOnlineCheckingCert:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        configByKey = xregistry.Read("AllowedOnlineCheckingCert");
                        break;
                    case ENUM_CONFIGKEY.AllowedOCSPForCheckingSigningCert:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        configByKey = xregistry.Read("AllowedOCSPForCheckingSigningCert");
                        break;
                    case ENUM_CONFIGKEY.RevocationMode:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        configByKey = xregistry.Read("RevocationMode");
                        break;
                    case ENUM_CONFIGKEY.AdditionalCrls:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        configByKey = xregistry.Read("AdditionalCrls");
                        break;
                    case ENUM_CONFIGKEY.UpdateOnStart:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        configByKey = xregistry.Read("UpdateOnStart");
                        break;
                    case ENUM_CONFIGKEY.LastCheckingUpdate:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        configByKey = xregistry.Read("LastCheckingUpdate");
                        break;
                    case ENUM_CONFIGKEY.CurrentVersion:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        configByKey = xregistry.Read("Version");
                        break;
                    case ENUM_CONFIGKEY.StartupChecked:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        configByKey = xregistry.Read("StartupChecked");
                        break;
                }
            }
            finally
            {
                xregistry?.Dispose();
            }
            return configByKey;
        }

        public static void SetConfigByKey(ENUM_CONFIGKEY key, object value)
        {
            XRegistry xregistry = (XRegistry)null;
            try
            {
                switch (key)
                {
                    case ENUM_CONFIGKEY.UseProxy:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("UseProxyServer", value);
                        break;
                    case ENUM_CONFIGKEY.AutoDetectProxy:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("AutoDetectProxy", value);
                        break;
                    case ENUM_CONFIGKEY.UsePxAuth:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("UsePxAuthentication", value);
                        break;
                    case ENUM_CONFIGKEY.BypassPxForLocal:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("BypassPxForLocal", value);
                        break;
                    case ENUM_CONFIGKEY.PxAddress:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("PxAddress", value);
                        break;
                    case ENUM_CONFIGKEY.PxPort:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("PxPort", value);
                        break;
                    case ENUM_CONFIGKEY.PxUsername:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("PxUsername", value);
                        break;
                    case ENUM_CONFIGKEY.PxPassword:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("PxPassword", value);
                        break;
                    case ENUM_CONFIGKEY.UseTsa:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Sign");
                        xregistry.Write("UseTSA", value);
                        break;
                    case ENUM_CONFIGKEY.TsaAddress:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Sign");
                        xregistry.Write("TsaAddress", value);
                        break;
                    case ENUM_CONFIGKEY.AllowedOnlineCheckingCert:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        xregistry.Write("AllowedOnlineCheckingCert", value);
                        break;
                    case ENUM_CONFIGKEY.AllowedOCSPForCheckingSigningCert:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        xregistry.Write("AllowedOCSPForCheckingSigningCert", value);
                        break;
                    case ENUM_CONFIGKEY.RevocationMode:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        xregistry.Write("RevocationMode", value);
                        break;
                    case ENUM_CONFIGKEY.AdditionalCrls:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        xregistry.Write("AdditionalCrls", value);
                        break;
                    case ENUM_CONFIGKEY.UpdateOnStart:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        xregistry.Write("UpdateOnStart", value);
                        break;
                    case ENUM_CONFIGKEY.LastCheckingUpdate:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        xregistry.Write("LastCheckingUpdate", value);
                        break;
                    case ENUM_CONFIGKEY.CurrentVersion:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        xregistry.Write("Version", value);
                        break;
                    case ENUM_CONFIGKEY.StartupChecked:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        xregistry.Write("StartupChecked", value);
                        break;
                }
            }
            finally
            {
                xregistry?.Dispose();
            }
        }
    }
}
