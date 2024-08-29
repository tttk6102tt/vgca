namespace Sign.itext.xml.xmp.options
{
    public sealed class ParseOptions : XmpOptions
    {
        public const uint REQUIRE_XMP_META = 1u;

        public const uint STRICT_ALIASING = 4u;

        public const uint FIX_CONTROL_CHARS = 8u;

        public const uint ACCEPT_LATIN_1 = 16u;

        public const uint OMIT_NORMALIZATION = 32u;

        public bool RequireXmpMeta
        {
            get
            {
                return GetOption(1u);
            }
            set
            {
                SetOption(1u, value);
            }
        }

        public bool StrictAliasing
        {
            get
            {
                return GetOption(4u);
            }
            set
            {
                SetOption(4u, value);
            }
        }

        public bool FixControlChars
        {
            get
            {
                return GetOption(8u);
            }
            set
            {
                SetOption(8u, value);
            }
        }

        public bool AcceptLatin1
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

        public bool OmitNormalization
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

        protected internal override uint ValidOptions => 61u;

        public ParseOptions()
        {
            SetOption(24u, value: true);
        }

        protected internal override string DefineOptionName(uint option)
        {
            return option switch
            {
                1u => "REQUIRE_XMP_META",
                4u => "STRICT_ALIASING",
                8u => "FIX_CONTROL_CHARS",
                16u => "ACCEPT_LATIN_1",
                32u => "OMIT_NORMALIZATION",
                _ => null,
            };
        }
    }
}
