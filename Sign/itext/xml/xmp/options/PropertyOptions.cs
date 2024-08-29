namespace Sign.itext.xml.xmp.options
{
    public sealed class PropertyOptions : XmpOptions
    {
        public const uint NO_OPTIONS = 0u;

        public const uint URI = 2u;

        public const uint HAS_QUALIFIERS = 16u;

        public const uint QUALIFIER = 32u;

        public const uint HAS_LANGUAGE = 64u;

        public const uint HAS_TYPE = 128u;

        public const uint STRUCT = 256u;

        public const uint ARRAY = 512u;

        public const uint ARRAY_ORDERED = 1024u;

        public const uint ARRAY_ALTERNATE = 2048u;

        public const uint ARRAY_ALT_TEXT = 4096u;

        public const uint SCHEMA_NODE = 2147483648u;

        public const uint DELETE_EXISTING = 536870912u;

        public const int SEPARATE_NODE = 1073741824;

        public bool Uri
        {
            get
            {
                return GetOption(2u);
            }
            set
            {
                SetOption(2u, value);
            }
        }

        public bool HasQualifiers
        {
            get
            {
                return GetOption(16u);
            }
            set
            {
                SetOption(16u, value);
            }
        }

        public bool Qualifier
        {
            get
            {
                return GetOption(32u);
            }
            set
            {
                SetOption(32u, value);
            }
        }

        public bool HasLanguage
        {
            get
            {
                return GetOption(64u);
            }
            set
            {
                SetOption(64u, value);
            }
        }

        public bool HasType
        {
            get
            {
                return GetOption(128u);
            }
            set
            {
                SetOption(128u, value);
            }
        }

        public bool Struct
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
                SetOption(1024u, value);
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
                SetOption(2048u, value);
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
                SetOption(4096u, value);
            }
        }

        public bool SchemaNode
        {
            get
            {
                return GetOption(2147483648u);
            }
            set
            {
                SetOption(2147483648u, value);
            }
        }

        public bool CompositeProperty => (Options & 0x300) != 0;

        public bool Simple => (Options & 0x300) == 0;

        public bool OnlyArrayOptions => (Options & 0xFFFFE1FFu) == 0;

        protected internal override uint ValidOptions => 3221233650u;

        public PropertyOptions()
        {
        }

        public PropertyOptions(uint options)
            : base(options)
        {
        }

        public bool EqualArrayTypes(PropertyOptions options)
        {
            if (Array == options.Array && ArrayOrdered == options.ArrayOrdered && ArrayAlternate == options.ArrayAlternate)
            {
                return ArrayAltText == options.ArrayAltText;
            }

            return false;
        }

        public void MergeWith(PropertyOptions options)
        {
            if (options != null)
            {
                Options |= options.Options;
            }
        }

        protected internal override string DefineOptionName(uint option)
        {
            return option switch
            {
                2u => "URI",
                16u => "HAS_QUALIFIER",
                32u => "QUALIFIER",
                64u => "HAS_LANGUAGE",
                128u => "HAS_TYPE",
                256u => "STRUCT",
                512u => "ARRAY",
                1024u => "ARRAY_ORDERED",
                2048u => "ARRAY_ALTERNATE",
                4096u => "ARRAY_ALT_TEXT",
                2147483648u => "SCHEMA_NODE",
                _ => null,
            };
        }

        protected internal override void AssertConsistency(uint options)
        {
            if ((options & 0x100u) != 0 && (options & 0x200u) != 0)
            {
                throw new XmpException("IsStruct and IsArray options are mutually exclusive", 103);
            }

            if ((options & 2u) != 0 && (options & 0x300u) != 0)
            {
                throw new XmpException("Structs and arrays can't have \"value\" options", 103);
            }
        }
    }
}
