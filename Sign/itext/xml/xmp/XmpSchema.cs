using System.Text;
namespace Sign.itext.xml.xmp
{
    [Obsolete]
    public abstract class XmpSchema : Sign.SystemItext.util.Properties
    {
        protected string xmlns;

        public virtual string Xmlns => xmlns;

        public override string this[string key]
        {
            set
            {
                base[key] = XMLUtil.EscapeXML(value, onlyASCII: false);
            }
        }

        public XmpSchema(string xmlns)
        {
            this.xmlns = xmlns;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string key in Keys)
            {
                Process(stringBuilder, key);
            }

            return stringBuilder.ToString();
        }

        protected virtual void Process(StringBuilder buf, object p)
        {
            buf.Append('<');
            buf.Append(p);
            buf.Append('>');
            buf.Append(this[p.ToString()]);
            buf.Append("</");
            buf.Append(p);
            buf.Append('>');
        }

        public virtual void AddProperty(string key, string value)
        {
            this[key] = value;
        }

        public virtual void SetProperty(string key, XmpArray value)
        {
            base[key] = value.ToString();
        }

        public virtual void SetProperty(string key, LangAlt value)
        {
            base[key] = value.ToString();
        }

        public static string Escape(string content)
        {
            return XMLUtil.EscapeXML(content, onlyASCII: false);
        }
    }
}
