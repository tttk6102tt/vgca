using Sign.itext.xml.simpleparser;
using System.Text;

namespace Sign.itext.xml
{
    public class XmlToTxt : ISimpleXMLDocHandler
    {
        protected internal StringBuilder buf;

        public static string Parse(Stream isp)
        {
            XmlToTxt xmlToTxt = new XmlToTxt();
            SimpleXMLParser.Parse(xmlToTxt, null, new StreamReader(isp), html: true);
            return xmlToTxt.ToString();
        }

        protected XmlToTxt()
        {
            buf = new StringBuilder();
        }

        public override string ToString()
        {
            return buf.ToString();
        }

        public virtual void StartElement(string tag, IDictionary<string, string> h)
        {
        }

        public virtual void EndElement(string tag)
        {
        }

        public virtual void StartDocument()
        {
        }

        public virtual void EndDocument()
        {
        }

        public virtual void Text(string str)
        {
            buf.Append(str);
        }
    }
}
