using System.Text;

namespace Sign.itext.xml.xmp
{
    [Obsolete]
    public class XmpArray : List<string>
    {
        public const string UNORDERED = "rdf:Bag";

        public const string ORDERED = "rdf:Seq";

        public const string ALTERNATIVE = "rdf:Alt";

        protected string type;

        public XmpArray(string type)
        {
            this.type = type;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("<");
            stringBuilder.Append(type);
            stringBuilder.Append('>');
            using (Enumerator enumerator = GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string current = enumerator.Current;
                    stringBuilder.Append("<rdf:li>");
                    stringBuilder.Append(XMLUtil.EscapeXML(current, onlyASCII: false));
                    stringBuilder.Append("</rdf:li>");
                }
            }

            stringBuilder.Append("</");
            stringBuilder.Append(type);
            stringBuilder.Append('>');
            return stringBuilder.ToString();
        }
    }
}
