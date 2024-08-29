using Sign.itext.io;
using System.Text;

namespace Sign.itext.error_messages
{
    public class MessageLocalization
    {
        private static Dictionary<string, string> defaultLanguage;

        private static Dictionary<string, string> currentLanguage;

        private const string BASE_PATH = "text.error_messages.";

        private MessageLocalization()
        {
        }

        static MessageLocalization()
        {
            defaultLanguage = new Dictionary<string, string>();
            try
            {
                defaultLanguage = GetLanguageMessages("vi", null);
            }
            catch
            {
            }

            if (defaultLanguage == null)
            {
                defaultLanguage = new Dictionary<string, string>();
            }
        }

        public static string GetMessage(string key)
        {
            return GetMessage(key, useDefaultLanguageIfMessageNotFound: true);
        }

        public static string GetMessage(string key, bool useDefaultLanguageIfMessageNotFound)
        {
            Dictionary<string, string> dictionary = currentLanguage;
            string value;
            if (dictionary != null)
            {
                dictionary.TryGetValue(key, out value);
                if (value != null)
                {
                    return value;
                }
            }

            if (useDefaultLanguageIfMessageNotFound)
            {
                dictionary = defaultLanguage;
                dictionary.TryGetValue(key, out value);
                if (value != null)
                {
                    return value;
                }
            }

            return "No message found for " + key;
        }

        public static string GetComposedMessage(string key, params object[] p)
        {
            string text = GetMessage(key);
            for (int i = 0; i < p.Length; i++)
            {
                text = text.Replace("{" + (i + 1) + "}", p[i].ToString());
            }

            return text;
        }

        public static bool SetLanguage(string language, string country)
        {
            Dictionary<string, string> languageMessages = GetLanguageMessages(language, country);
            if (languageMessages == null)
            {
                return false;
            }

            currentLanguage = languageMessages;
            return true;
        }

        public static void SetMessages(TextReader r)
        {
            currentLanguage = ReadLanguageStream(r);
        }

        private static Dictionary<string, string> GetLanguageMessages(string language, string country)
        {
            if (language == null)
            {
                throw new ArgumentException("The language cannot be null.");
            }

            Stream stream = null;
            try
            {

                //"text.error_messages.en"

                //stream = StreamUtil.GetResourceStream("Sign.Properties.Resources.iTextSharp.text.error_messages.en");
                //string text = ((country == null) ? (language + ".lng") : (language + "_" + country + ".lng"));

                string text = ((country == null) ? (language) : (language + "_" + country));
                stream = StreamUtil.GetResourceStream("text.error_messages." + text);
                if (stream != null)
                {
                    return ReadLanguageStream(stream);
                }

                if (country == null)
                {
                    return null;
                }

                text = language + ".lng";
                stream = StreamUtil.GetResourceStream("text.error_messages." + text);
                if (stream != null)
                {
                    return ReadLanguageStream(stream);
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                try
                {
                    stream.Close();
                }
                catch
                {
                }
            }
        }

        private static Dictionary<string, string> ReadLanguageStream(Stream isp)
        {
            return ReadLanguageStream(new StreamReader(isp, Encoding.UTF8));
        }

        private static Dictionary<string, string> ReadLanguageStream(TextReader br)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string text;
            while ((text = br.ReadLine()) != null)
            {
                int num = text.IndexOf('=');
                if (num >= 0)
                {
                    string text2 = text.Substring(0, num).Trim();
                    if (!text2.StartsWith("#"))
                    {
                        dictionary[text2] = text.Substring(num + 1);
                    }
                }
            }

            return dictionary;
        }
    }
}
