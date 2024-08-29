namespace Sign.itext.xml.xmp.impl
{
    public class QName
    {
        private readonly string _localName;

        private readonly string _prefix;

        public virtual string LocalName => _localName;

        public virtual string Prefix => _prefix;

        public QName(string qname)
        {
            int num = qname.IndexOf(':');
            if (num >= 0)
            {
                _prefix = qname.Substring(0, num);
                _localName = qname.Substring(num + 1);
            }
            else
            {
                _prefix = "";
                _localName = qname;
            }
        }

        public QName(string prefix, string localName)
        {
            _prefix = prefix;
            _localName = localName;
        }

        public virtual bool HasPrefix()
        {
            return !string.IsNullOrEmpty(_prefix);
        }
    }
}
