namespace Sign.itext.xml.xmp.impl.xpath
{
    public class XmpPathSegment
    {
        private bool _alias;

        private uint _aliasForm;

        private uint _kind;

        private string _name;

        public virtual uint Kind
        {
            get
            {
                return _kind;
            }
            set
            {
                _kind = value;
            }
        }

        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public virtual bool Alias
        {
            get
            {
                return _alias;
            }
            set
            {
                _alias = value;
            }
        }

        public virtual uint AliasForm
        {
            get
            {
                return _aliasForm;
            }
            set
            {
                _aliasForm = value;
            }
        }

        public XmpPathSegment(string name)
        {
            _name = name;
        }

        public XmpPathSegment(string name, uint kind)
        {
            _name = name;
            _kind = kind;
        }

        public override string ToString()
        {
            switch (_kind)
            {
                case 1u:
                case 2u:
                case 3u:
                case 4u:
                    return _name;
                case 5u:
                case 6u:
                    return _name;
                default:
                    return _name;
            }
        }
    }
}
