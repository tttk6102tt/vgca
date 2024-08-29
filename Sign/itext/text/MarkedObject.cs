namespace Sign.itext.text
{
    public class MarkedObject : IElement
    {
        protected internal IElement element;

        protected internal Sign.SystemItext.util.Properties markupAttributes = new Sign.SystemItext.util.Properties();

        public virtual IList<Chunk> Chunks => element.Chunks;

        public virtual int Type => 50;

        public virtual Sign.SystemItext.util.Properties MarkupAttributes => markupAttributes;

        protected MarkedObject()
        {
            element = null;
        }

        public MarkedObject(IElement element)
        {
            this.element = element;
        }

        public virtual bool Process(IElementListener listener)
        {
            try
            {
                return listener.Add(element);
            }
            catch (DocumentException)
            {
                return false;
            }
        }

        public virtual bool IsContent()
        {
            return true;
        }

        public virtual bool IsNestable()
        {
            return true;
        }

        public virtual void SetMarkupAttribute(string key, string value)
        {
            markupAttributes.Add(key, value);
        }
    }
}
