using System.Globalization;
using System.Reflection;
using System.Text;

namespace Sign.itext.error_messages
{
    public class LocalizedResource
    {
        private static readonly char[] splt = new char[1] { '_' };
        private Sign.SystemItext.util.Properties msgs = new Sign.SystemItext.util.Properties();

        public LocalizedResource(string resourceRoot, CultureInfo culture, Assembly assembly)
        {
            Stream inStream = (Stream)null;
            string str = culture.Name.Replace('-', '_');
            if (str != "")
            {
                try
                {
                    inStream = assembly.GetManifestResourceStream(resourceRoot + "_" + str + ".properties");
                }
                catch
                {
                }
                if (inStream == null)
                {
                    string[] strArray = str.Split(LocalizedResource.splt, 2);
                    if (strArray.Length == 2)
                    {
                        try
                        {
                            inStream = assembly.GetManifestResourceStream(resourceRoot + "_" + strArray[0] + ".properties");
                        }
                        catch
                        {
                        }
                    }
                }
            }
            if (inStream == null)
            {
                try
                {
                    inStream = assembly.GetManifestResourceStream(resourceRoot + ".properties");
                }
                catch
                {
                }
            }
            if (inStream != null)
                this.msgs.Load(inStream);
            foreach (string key in new List<string>((IEnumerable<string>)this.msgs.Keys))
            {
                string msg = this.msgs[key];
                StringBuilder stringBuilder = new StringBuilder();
                int num1 = 0;
                int num2 = 0;
                foreach (char ch in msg)
                {
                    switch (num2)
                    {
                        case 0:
                            switch (ch)
                            {
                                case '%':
                                    num2 = 1;
                                    continue;
                                case '{':
                                case '}':
                                    stringBuilder.Append(ch);
                                    break;
                            }
                            stringBuilder.Append(ch);
                            break;
                        case 1:
                            if (ch == '%')
                            {
                                stringBuilder.Append(ch);
                                num2 = 0;
                            }
                            else if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z')
                                num2 = 0;
                            stringBuilder.Append('{').Append(num1++).Append('}');
                            break;
                    }
                }
                this.msgs[key] = stringBuilder.ToString();
            }
        }

        public virtual string GetMessage(string key) => this.msgs[key] ?? key;
    }
}
