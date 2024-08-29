using Sign.PDF;
using System.Collections;
using System.Text;
using System.Xml;

namespace Plugin.UI.Configurations
{
    public class SignerProfileStore : IEnumerable<SignerProfile>, IEnumerable
    {
        private const string NODE_SIGNER_PROFILE = "SignerProfile";
        private const string ATT_DEFAULT_SIGNER = "DefaultSigner";
        private const string NODE_SIGNER = "Signer";
        private const string ATT_NAME = "Name";
        private const string ATT_APPEARANCE_MODE = "AppearanceMode";
        private const string ATT_ORG_PROFILE = "IsOrgProfile";
        private const string ATT_SHOW_LABEL = "ShowLabel";
        private const string ATT_SHOW_EMAIL = "ShowEmail";
        private const string ATT_SHOW_CQ1 = "ShowCQ1";
        private const string ATT_SHOW_CQ2 = "ShowCQ2";
        private const string ATT_SHOW_DATE = "ShowDate";
        private const string ATT_SHOW_CQ3 = "ShowCQ3";
        private const string ATT_ORG = "Org";
        private const string ATT_JOB = "Job";
        private const string NODE_IMAGE = "Image";
        private string path;
        private List<SignerProfile> lst = new List<SignerProfile>();
        private int defaultSigner = -1;

        public int DefaultSigner
        {
            get => this.defaultSigner;
            set
            {
                this.defaultSigner = value;
                XmlDocument xmlDocument = new XmlDocument();
                using (FileStream inStream = new FileStream(this.path, FileMode.Open))
                {
                    xmlDocument.Load((Stream)inStream);
                    ((XmlElement)xmlDocument.GetElementsByTagName("SignerProfile")[0]).SetAttribute(nameof(DefaultSigner), value.ToString());
                }
                xmlDocument.Save(this.path);
            }
        }

        public int Count => this.lst.Count;

        public SignerProfileStore(string path)
        {
            this.path = path;
            this.OpenStore();
        }

        public virtual SignerProfile this[int index]
        {
            get => this.lst[index];
            set
            {
                XmlDocument xmlDocument = new XmlDocument();
                using (FileStream inStream = new FileStream(this.path, FileMode.Open))
                {
                    xmlDocument.Load((Stream)inStream);
                    XmlElement xmlElement = (XmlElement)xmlDocument.GetElementsByTagName("Signer")[index];
                    xmlElement.SetAttribute("Name", value.Name);
                    xmlElement.SetAttribute("AppearanceMode", value.AppearanceMode.ToString());
                    xmlElement.SetAttribute("IsOrgProfile", value.IsOrgProfile.ToString());
                    xmlElement.SetAttribute("ShowLabel", value.ShowLabel.ToString());
                    xmlElement.SetAttribute("ShowEmail", value.ShowEmail.ToString());
                    xmlElement.SetAttribute("ShowCQ1", value.ShowCQ1.ToString());
                    xmlElement.SetAttribute("ShowCQ2", value.ShowCQ2.ToString());
                    xmlElement.SetAttribute("ShowCQ3", value.ShowCQ3.ToString());
                    xmlElement.SetAttribute("ShowDate", value.ShowDate.ToString());
                    xmlDocument.GetElementsByTagName("Image")[index].InnerText = value.ImageBase64;
                }
                xmlDocument.Save(this.path);
                this.lst[index] = value;
            }
        }

        private void CreateStore()
        {
            XmlTextWriter xmlTextWriter = new XmlTextWriter(this.path, Encoding.UTF8);
            xmlTextWriter.WriteStartDocument();
            xmlTextWriter.WriteStartElement("SignerProfile");
            xmlTextWriter.WriteEndElement();
            xmlTextWriter.Close();
        }

        private bool OpenStore()
        {
            bool flag = this.LoadProfiles();
            if (flag)
                return flag;
            this.CreateStore();
            return this.LoadProfiles();
        }

        private bool LoadProfiles()
        {
            try
            {
                this.lst = new List<SignerProfile>();
                using (FileStream inStream = new FileStream(this.path, FileMode.Open))
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load((Stream)inStream);
                    XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("Signer");
                    for (int i = 0; i < elementsByTagName.Count; ++i)
                    {
                        XmlElement xmlElement1 = (XmlElement)xmlDocument.GetElementsByTagName("Signer")[i];
                        XmlElement xmlElement2 = (XmlElement)xmlDocument.GetElementsByTagName("Image")[i];
                        this.lst.Add(new SignerProfile(xmlElement1.GetAttribute("Name"), xmlElement2.InnerText)
                        {
                            AppearanceMode = int.Parse(xmlElement1.GetAttribute("AppearanceMode")),
                            IsOrgProfile = bool.Parse(xmlElement1.GetAttribute("IsOrgProfile")),
                            ShowDate = bool.Parse(xmlElement1.GetAttribute("ShowDate")),
                            ShowEmail = bool.Parse(xmlElement1.GetAttribute("ShowEmail")),
                            ShowCQ1 = bool.Parse(xmlElement1.GetAttribute("ShowCQ1")),
                            ShowCQ2 = bool.Parse(xmlElement1.GetAttribute("ShowCQ2")),
                            ShowCQ3 = bool.Parse(xmlElement1.GetAttribute("ShowCQ3")),
                            ShowLabel = bool.Parse(xmlElement1.GetAttribute("ShowLabel"))
                        });
                    }
                    string attribute = ((XmlElement)xmlDocument.GetElementsByTagName("SignerProfile")[0]).GetAttribute("DefaultSigner");
                    int num = -1;
                    ref int local = ref num;
                    if (int.TryParse(attribute, out local))
                        this.defaultSigner = num;
                }
                return true;
            }
            catch (DirectoryNotFoundException ex)
            {
                try
                {
                    Directory.CreateDirectory(string.Format("{0}", (object)Path.GetDirectoryName(this.path)));
                }
                catch
                {
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void Add(SignerProfile sp)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (FileStream inStream = new FileStream(this.path, FileMode.Open))
            {
                xmlDocument.Load((Stream)inStream);
                XmlElement element1 = xmlDocument.CreateElement("Signer");
                element1.SetAttribute("Name", sp.Name);
                element1.SetAttribute("AppearanceMode", sp.AppearanceMode.ToString());
                element1.SetAttribute("IsOrgProfile", sp.IsOrgProfile.ToString());
                element1.SetAttribute("ShowLabel", sp.ShowLabel.ToString());
                element1.SetAttribute("ShowEmail", sp.ShowEmail.ToString());
                element1.SetAttribute("ShowCQ1", sp.ShowCQ1.ToString());
                element1.SetAttribute("ShowCQ2", sp.ShowCQ2.ToString());
                element1.SetAttribute("ShowCQ3", sp.ShowCQ3.ToString());
                element1.SetAttribute("ShowDate", sp.ShowDate.ToString());
                XmlElement element2 = xmlDocument.CreateElement("Image");
                XmlText textNode = xmlDocument.CreateTextNode(sp.ImageBase64);
                element2.AppendChild((XmlNode)textNode);
                element1.AppendChild((XmlNode)element2);
                xmlDocument.DocumentElement.AppendChild((XmlNode)element1);
            }
            xmlDocument.Save(this.path);
            this.lst.Add(sp);
        }

        public void Add(SignerProfile sp, bool isDefault)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (FileStream inStream = new FileStream(this.path, FileMode.Open))
            {
                xmlDocument.Load((Stream)inStream);
                if (isDefault)
                {
                    ((XmlElement)xmlDocument.GetElementsByTagName("SignerProfile")[0]).SetAttribute("DefaultSigner", this.Count.ToString());
                    this.defaultSigner = this.Count;
                }
                XmlElement element1 = xmlDocument.CreateElement("Signer");
                element1.SetAttribute("Name", sp.Name);
                element1.SetAttribute("AppearanceMode", sp.AppearanceMode.ToString());
                element1.SetAttribute("IsOrgProfile", sp.IsOrgProfile.ToString());
                element1.SetAttribute("ShowLabel", sp.ShowLabel.ToString());
                XmlElement xmlElement1 = element1;
                bool flag = sp.ShowEmail;
                string str1 = flag.ToString();
                xmlElement1.SetAttribute("ShowEmail", str1);
                XmlElement xmlElement2 = element1;
                flag = sp.ShowCQ1;
                string str2 = flag.ToString();
                xmlElement2.SetAttribute("ShowCQ1", str2);
                XmlElement xmlElement3 = element1;
                flag = sp.ShowCQ2;
                string str3 = flag.ToString();
                xmlElement3.SetAttribute("ShowCQ2", str3);
                XmlElement xmlElement4 = element1;
                flag = sp.ShowCQ3;
                string str4 = flag.ToString();
                xmlElement4.SetAttribute("ShowCQ3", str4);
                XmlElement xmlElement5 = element1;
                flag = sp.ShowDate;
                string str5 = flag.ToString();
                xmlElement5.SetAttribute("ShowDate", str5);
                XmlElement element2 = xmlDocument.CreateElement("Image");
                XmlText textNode = xmlDocument.CreateTextNode(sp.ImageBase64);
                element2.AppendChild((XmlNode)textNode);
                element1.AppendChild((XmlNode)element2);
                xmlDocument.DocumentElement.AppendChild((XmlNode)element1);
            }
            xmlDocument.Save(this.path);
            this.lst.Add(sp);
        }

        public void Remove(int index)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (FileStream inStream = new FileStream(this.path, FileMode.Open))
            {
                xmlDocument.Load((Stream)inStream);
                XmlElement xmlElement = (XmlElement)xmlDocument.GetElementsByTagName("SignerProfile")[0];
                xmlElement.GetAttribute("DefaultSigner");
                if (index == this.defaultSigner)
                {
                    xmlElement.SetAttribute("DefaultSigner", "-1");
                    this.defaultSigner = -1;
                }
                XmlElement oldChild = (XmlElement)xmlDocument.GetElementsByTagName("Signer")[index];
                xmlDocument.DocumentElement.RemoveChild((XmlNode)oldChild);
            }
            xmlDocument.Save(this.path);
            this.lst.RemoveAt(index);
        }

        public IEnumerator<SignerProfile> GetEnumerator() => (IEnumerator<SignerProfile>)this.lst.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();
    }
}
