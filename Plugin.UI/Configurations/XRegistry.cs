using Microsoft.Win32;

namespace Plugin.UI.Configurations
{
    public class XRegistry : IDisposable
    {
        private string subKey;
        private RegistryKey baseRegistryKey;

        public RegistryKey BaseRegistryKey
        {
            get => this.baseRegistryKey;
            set => this.baseRegistryKey = value;
        }

        public XRegistry(string SubKey, RegistryKey baseKey)
        {
            this.subKey = SubKey;
            this.baseRegistryKey = baseKey;
        }

        public XRegistry(string SubKey)
        {
            this.subKey = SubKey;
            this.baseRegistryKey = Registry.CurrentUser;
        }

        public object Read(string KeyName)
        {
            using (RegistryKey baseRegistryKey = this.baseRegistryKey)
            {
                using (RegistryKey registryKey = baseRegistryKey.OpenSubKey(this.subKey))
                {
                    if (registryKey == null)
                        return (object)null;
                    try
                    {
                        return registryKey.GetValue(KeyName.ToUpper());
                    }
                    catch
                    {
                        return (object)null;
                    }
                }
            }
        }

        public bool Write(string KeyName, object Value)
        {
            try
            {
                using (RegistryKey baseRegistryKey = this.baseRegistryKey)
                {
                    using (RegistryKey subKey = baseRegistryKey.CreateSubKey(this.subKey))
                    {
                        subKey.SetValue(KeyName.ToUpper(), Value);
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteKey(string KeyName)
        {
            try
            {
                using (RegistryKey baseRegistryKey = this.baseRegistryKey)
                {
                    using (RegistryKey subKey = baseRegistryKey.CreateSubKey(this.subKey))
                    {
                        if (subKey == null)
                            return true;
                        subKey.DeleteValue(KeyName);
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteSubKeyTree()
        {
            try
            {
                using (RegistryKey baseRegistryKey = this.baseRegistryKey)
                {
                    using (RegistryKey registryKey = baseRegistryKey.OpenSubKey(this.subKey))
                    {
                        if (registryKey != null)
                            baseRegistryKey.DeleteSubKeyTree(this.subKey);
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public int SubKeyCount()
        {
            try
            {
                using (RegistryKey baseRegistryKey = this.baseRegistryKey)
                {
                    using (RegistryKey registryKey = baseRegistryKey.OpenSubKey(this.subKey))
                        return registryKey != null ? registryKey.SubKeyCount : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public int ValueCount()
        {
            try
            {
                using (RegistryKey baseRegistryKey = this.baseRegistryKey)
                {
                    using (RegistryKey registryKey = baseRegistryKey.OpenSubKey(this.subKey))
                        return registryKey != null ? registryKey.ValueCount : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public void Dispose()
        {
            try
            {
                if (this.baseRegistryKey == null)
                    return;
                this.baseRegistryKey.Close();
            }
            catch
            {
            }
        }
    }
}
