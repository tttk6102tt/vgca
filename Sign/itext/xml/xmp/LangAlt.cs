using System.Text;

namespace Sign.itext.xml.xmp
{
    [Obsolete]
    public class LangAlt : Sign.SystemItext.util.Properties
    {
        public const string DEFAULT = "x-default";

        public LangAlt(string defaultValue)
        {
            AddLanguage("x-default", defaultValue);
        }

        public LangAlt()
        {
        }

        public virtual void AddLanguage(string language, string value)
        {
            this[language] = XMLUtil.EscapeXML(value, onlyASCII: false);
        }

        protected internal virtual void Process(StringBuilder buf, string lang)
        {
            buf.Append("<rdf:li xml:lang=\"");
            buf.Append(lang);
            buf.Append("\" >");
            buf.Append(this[lang]);
            buf.Append("</rdf:li>");
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<rdf:Alt>");
            foreach (string key in Keys)
            {
                Process(stringBuilder, key);
            }

            stringBuilder.Append("</rdf:Alt>");
            return stringBuilder.ToString();
        }
    }
}
