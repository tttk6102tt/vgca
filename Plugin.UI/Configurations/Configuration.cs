using Sign.PDF;
using System.Net;

namespace Plugin.UI.Configurations
{
    public class Configuration
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
                object configByKey = Configuration.GetConfigByKey(ConfigKey.StartupChecked);
                bool result = false;
                return bool.TryParse((string)configByKey, out result) && result;
            }
        }

        public bool UsedProxy
        {
            get
            {
                object configByKey = Configuration.GetConfigByKey(ConfigKey.UseProxy);
                bool result = false;
                return bool.TryParse((string)configByKey, out result) && result;
            }
        }

        public bool AutoDetectProxy
        {
            get
            {
                object configByKey = Configuration.GetConfigByKey(ConfigKey.AutoDetectProxy);
                bool result = false;
                return bool.TryParse((string)configByKey, out result) && result;
            }
        }

        public bool UsePxAuth
        {
            get
            {
                object configByKey = Configuration.GetConfigByKey(ConfigKey.UsePxAuth);
                bool result = false;
                return bool.TryParse((string)configByKey, out result) && result;
            }
        }

        public bool BypassPxForLocal
        {
            get
            {
                object configByKey = Configuration.GetConfigByKey(ConfigKey.BypassPxForLocal);
                bool result = false;
                return bool.TryParse((string)configByKey, out result) && result;
            }
        }

        public bool UsedTsa
        {
            get
            {
                object configByKey = Configuration.GetConfigByKey(ConfigKey.UseTsa);
                bool result = false;
                return bool.TryParse((string)configByKey, out result) && result;
            }
        }

        public bool AllowedOnlineCheckingCert
        {
            get
            {
                object configByKey = Configuration.GetConfigByKey(ConfigKey.AllowedOnlineCheckingCert);
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
                object configByKey = Configuration.GetConfigByKey(ConfigKey.AllowedOCSPForCheckingSigningCert);
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
                    return ((string)Configuration.GetConfigByKey(ConfigKey.AdditionalCrls)).Split(new char[1]
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
                string configByKey = (string)Configuration.GetConfigByKey(ConfigKey.RevocationMode);
                int num = 0;
                ref int local = ref num;
                return int.TryParse(configByKey, out local) ? num : 0;
            }
        }

        public string TsaAddress => (string)Configuration.GetConfigByKey(ConfigKey.TsaAddress);

        public string PxAddress => (string)Configuration.GetConfigByKey(ConfigKey.PxAddress);

        public int PxPort
        {
            get
            {
                string configByKey = (string)Configuration.GetConfigByKey(ConfigKey.PxPort);
                int num = 0;
                ref int local = ref num;
                return int.TryParse(configByKey, out local) ? num : 0;
            }
        }

        public string PxUsername => (string)Configuration.GetConfigByKey(ConfigKey.PxUsername);

        public string PxPassword => (string)Configuration.GetConfigByKey(ConfigKey.PxPassword);

        public WebProxy Proxy
        {
            get
            {
                string configByKey1 = (string)Configuration.GetConfigByKey(ConfigKey.PxAddress);
                if (string.IsNullOrEmpty(configByKey1))
                    return (WebProxy)null;
                string configByKey2 = (string)Configuration.GetConfigByKey(ConfigKey.PxPort);
                if (string.IsNullOrEmpty(configByKey2))
                    return (WebProxy)null;
                int result = 0;
                if (!int.TryParse(configByKey2, out result))
                    return (WebProxy)null;
                WebProxy proxy = new WebProxy(configByKey1, result);
                proxy.BypassProxyOnLocal = this.BypassPxForLocal;
                if (this.UsePxAuth)
                {
                    string configByKey3 = (string)Configuration.GetConfigByKey(ConfigKey.PxUsername);
                    string configByKey4 = (string)Configuration.GetConfigByKey(ConfigKey.PxPassword);
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
                string configByKey = (string)Configuration.GetConfigByKey(ConfigKey.UpdateOnStart);
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
                DateTime.TryParse((string)Configuration.GetConfigByKey(ConfigKey.LastCheckingUpdate), out result);
                return result;
            }
        }

        public Version CurrentVersion
        {
            get
            {
                string configByKey = (string)Configuration.GetConfigByKey(ConfigKey.CurrentVersion);
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

        public SignerProfileStore PdfSignerProfiles => new SignerProfileStore(Path.Combine(Configuration.WORKING_PATH, "signerprofile.xml"));

        public SignerProfile PdfSignerDefault
        {
            get
            {
                try
                {
                    SignerProfileStore signerProfileStore = new SignerProfileStore(Path.Combine(Configuration.WORKING_PATH, "signerprofile.xml"));
                    return signerProfileStore.DefaultSigner < 0 ? (SignerProfile)null : signerProfileStore[signerProfileStore.DefaultSigner];
                }
                catch
                {
                    return (SignerProfile)null;
                }
            }
        }

        public static object GetConfigByKey(ConfigKey key)
        {
            XRegistry xregistry = (XRegistry)null;
            object configByKey = (object)null;
            try
            {
                switch (key)
                {
                    case ConfigKey.UseProxy:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("UseProxyServer");
                        break;
                    case ConfigKey.AutoDetectProxy:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("AutoDetectProxy");
                        break;
                    case ConfigKey.UsePxAuth:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("UsePxAuthentication");
                        break;
                    case ConfigKey.BypassPxForLocal:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("BypassPxForLocal");
                        break;
                    case ConfigKey.PxAddress:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("PxAddress");
                        break;
                    case ConfigKey.PxPort:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("PxPort");
                        break;
                    case ConfigKey.PxUsername:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("PxUsername");
                        break;
                    case ConfigKey.PxPassword:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        configByKey = xregistry.Read("PxPassword");
                        break;
                    case ConfigKey.UseTsa:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Sign");
                        configByKey = xregistry.Read("UseTSA");
                        break;
                    case ConfigKey.TsaAddress:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Sign");
                        configByKey = xregistry.Read("TsaAddress");
                        break;
                    case ConfigKey.AllowedOnlineCheckingCert:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        configByKey = xregistry.Read("AllowedOnlineCheckingCert");
                        break;
                    case ConfigKey.AllowedOCSPForCheckingSigningCert:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        configByKey = xregistry.Read("AllowedOCSPForCheckingSigningCert");
                        break;
                    case ConfigKey.RevocationMode:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        configByKey = xregistry.Read("RevocationMode");
                        break;
                    case ConfigKey.AdditionalCrls:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        configByKey = xregistry.Read("AdditionalCrls");
                        break;
                    case ConfigKey.UpdateOnStart:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        configByKey = xregistry.Read("UpdateOnStart");
                        break;
                    case ConfigKey.LastCheckingUpdate:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        configByKey = xregistry.Read("LastCheckingUpdate");
                        break;
                    case ConfigKey.CurrentVersion:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        configByKey = xregistry.Read("Version");
                        break;
                    case ConfigKey.StartupChecked:
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

        public static void SetConfigByKey(ConfigKey key, object value)
        {
            XRegistry xregistry = (XRegistry)null;
            try
            {
                switch (key)
                {
                    case ConfigKey.UseProxy:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("UseProxyServer", value);
                        break;
                    case ConfigKey.AutoDetectProxy:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("AutoDetectProxy", value);
                        break;
                    case ConfigKey.UsePxAuth:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("UsePxAuthentication", value);
                        break;
                    case ConfigKey.BypassPxForLocal:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("BypassPxForLocal", value);
                        break;
                    case ConfigKey.PxAddress:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("PxAddress", value);
                        break;
                    case ConfigKey.PxPort:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("PxPort", value);
                        break;
                    case ConfigKey.PxUsername:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("PxUsername", value);
                        break;
                    case ConfigKey.PxPassword:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Connection");
                        xregistry.Write("PxPassword", value);
                        break;
                    case ConfigKey.UseTsa:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Sign");
                        xregistry.Write("UseTSA", value);
                        break;
                    case ConfigKey.TsaAddress:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Sign");
                        xregistry.Write("TsaAddress", value);
                        break;
                    case ConfigKey.AllowedOnlineCheckingCert:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        xregistry.Write("AllowedOnlineCheckingCert", value);
                        break;
                    case ConfigKey.AllowedOCSPForCheckingSigningCert:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        xregistry.Write("AllowedOCSPForCheckingSigningCert", value);
                        break;
                    case ConfigKey.RevocationMode:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        xregistry.Write("RevocationMode", value);
                        break;
                    case ConfigKey.AdditionalCrls:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\CertChecker");
                        xregistry.Write("AdditionalCrls", value);
                        break;
                    case ConfigKey.UpdateOnStart:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        xregistry.Write("UpdateOnStart", value);
                        break;
                    case ConfigKey.LastCheckingUpdate:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        xregistry.Write("LastCheckingUpdate", value);
                        break;
                    case ConfigKey.CurrentVersion:
                        xregistry = new XRegistry("SOFTWARE\\HIPT\\Pdf");
                        xregistry.Write("Version", value);
                        break;
                    case ConfigKey.StartupChecked:
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
