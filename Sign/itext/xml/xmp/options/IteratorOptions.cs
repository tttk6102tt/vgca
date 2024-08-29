namespace Sign.itext.xml.xmp.options
{
    public sealed class IteratorOptions : XmpOptions
    {
        public const uint JUST_CHILDREN = 256u;

        public const uint JUST_LEAFNODES = 512u;

        public const uint JUST_LEAFNAME = 1024u;

        public const uint OMIT_QUALIFIERS = 4096u;

        public bool JustChildren
        {
            get
            {
                return GetOption(256u);
            }
            set
            {
                SetOption(256u, value);
            }
        }

        public bool JustLeafname
        {
            get
            {
                return GetOption(1024u);
            }
            set
            {
                SetOption(1024u, value);
            }
        }

        public bool JustLeafnodes
        {
            get
            {
                return GetOption(512u);
            }
            set
            {
                SetOption(512u, value);
            }
        }

        public bool OmitQualifiers
        {
            get
            {
                return GetOption(4096u);
            }
            set
            {
                SetOption(4096u, value);
            }
        }

        protected internal override uint ValidOptions => 5888u;

        protected internal override string DefineOptionName(uint option)
        {
            return option switch
            {
                256u => "JUST_CHILDREN",
                512u => "JUST_LEAFNODES",
                1024u => "JUST_LEAFNAME",
                4096u => "OMIT_QUALIFIERS",
                _ => null,
            };
        }
    }
}
