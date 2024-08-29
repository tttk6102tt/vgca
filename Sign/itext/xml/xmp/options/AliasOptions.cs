namespace Sign.itext.xml.xmp.options
{
    public sealed class AliasOptions : XmpOptions
    {
        public const uint PROP_DIRECT = 0u;

        public const uint PROP_ARRAY = 512u;

        public const uint PROP_ARRAY_ORDERED = 1024u;

        public const uint PROP_ARRAY_ALTERNATE = 2048u;

        public const uint PROP_ARRAY_ALT_TEXT = 4096u;

        public bool Simple => Options == 0;

        public bool Array
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

        public bool ArrayOrdered
        {
            get
            {
                return GetOption(1024u);
            }
            set
            {
                SetOption(1536u, value);
            }
        }

        public bool ArrayAlternate
        {
            get
            {
                return GetOption(2048u);
            }
            set
            {
                SetOption(3584u, value);
            }
        }

        public bool ArrayAltText
        {
            get
            {
                return GetOption(4096u);
            }
            set
            {
                SetOption(7680u, value);
            }
        }

        protected internal override uint ValidOptions => 7680u;

        public AliasOptions()
        {
        }

        public AliasOptions(uint options)
            : base(options)
        {
        }

        public PropertyOptions ToPropertyOptions()
        {
            return new PropertyOptions(Options);
        }

        protected internal override string DefineOptionName(uint option)
        {
            return option switch
            {
                0u => "PROP_DIRECT",
                512u => "ARRAY",
                1024u => "ARRAY_ORDERED",
                2048u => "ARRAY_ALTERNATE",
                4096u => "ARRAY_ALT_TEXT",
                _ => null,
            };
        }
    }
}
