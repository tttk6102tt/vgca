namespace Sign.itext.xml.simpleparser.handler
{
    public class HTMLNewLineHandler : INewLineHandler
    {
        private readonly Dictionary<string, object> newLineTags = new Dictionary<string, object>();

        public HTMLNewLineHandler()
        {
            newLineTags["p"] = null;
            newLineTags["blockquote"] = null;
            newLineTags["br"] = null;
        }

        public virtual bool IsNewLineTag(string tag)
        {
            return newLineTags.ContainsKey(tag);
        }
    }
}
