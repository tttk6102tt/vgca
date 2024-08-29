namespace Sign.itext.text
{
    public sealed class Version
    {
        private static Version version = null;

        private static string iText = "iTextSharp™";

        private static string release = "5.5.5";

        private string iTextVersion = iText + " " + release + " ©2000-2014 iText Group NV";

        private string key;

        public string Product => iText;

        public string Release => release;

        public string GetVersion => iTextVersion;

        public string Key => key;

        public static Version GetInstance()
        {
            if (version == null)
            {
                version = new Version();
                /*
                lock (version)
                {
                    try
                    {
                        Type type = Type.GetType("iTextSharp.license.LicenseKey, itextsharp.LicenseKey");
                        string[] array = (string[])type.GetMethod("GetLicenseeInfo")!.Invoke(Activator.CreateInstance(type), null);
                        if (array[3] != null && array[3].Trim().Length > 0)
                        {
                            version.key = array[3];
                        }
                        else
                        {
                            version.key = "Trial version ";
                            if (array[5] == null)
                            {
                                version.key += "unauthorised";
                            }
                            else
                            {
                                version.key += array[5];
                            }
                        }

                        if (array[4] != null && array[4].Trim().Length > 0)
                        {
                            version.iTextVersion = array[4];
                        }
                        else if (array[2] != null && array[2].Trim().Length > 0)
                        {
                            Version obj = version;
                            obj.iTextVersion = obj.iTextVersion + " (" + array[2];
                            if (!version.key.ToLower().StartsWith("trial"))
                            {
                                version.iTextVersion += "; licensed version)";
                            }
                            else
                            {
                                Version obj2 = version;
                                obj2.iTextVersion = obj2.iTextVersion + "; " + version.key + ")";
                            }
                        }
                        else
                        {
                            if (array[0] == null || array[0].Trim().Length <= 0)
                            {
                                throw new Exception();
                            }

                            Version obj3 = version;
                            obj3.iTextVersion = obj3.iTextVersion + " (" + array[0];
                            if (!version.key.ToLower().StartsWith("trial"))
                            {
                                version.iTextVersion += "; licensed version)";
                            }
                            else
                            {
                                Version obj4 = version;
                                obj4.iTextVersion = obj4.iTextVersion + "; " + version.key + ")";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        version.iTextVersion += " (AGPL-version)";
                    }
                }
                */
            }

            return version;
        }
    }
}
