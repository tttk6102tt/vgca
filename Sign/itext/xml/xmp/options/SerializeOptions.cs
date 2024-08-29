namespace Sign.itext.xml.xmp.options
{
    public sealed class SerializeOptions : XmpOptions
    {
        public const uint OMIT_PACKET_WRAPPER = 16u;

        public const uint READONLY_PACKET = 32u;

        public const uint USE_COMPACT_FORMAT = 64u;

        public const uint USE_CANONICAL_FORMAT = 128u;

        public const uint INCLUDE_THUMBNAIL_PAD = 256u;

        public const uint EXACT_PACKET_LENGTH = 512u;

        public const uint OMIT_XMPMETA_ELEMENT = 4096u;

        public const uint SORT = 8192u;

        private const uint LITTLEENDIAN_BIT = 1u;

        private const uint UTF16_BIT = 2u;

        public const uint ENCODE_UTF8 = 0u;

        public const uint ENCODE_UTF16BE = 2u;

        public const uint ENCODE_UTF16LE = 3u;

        private const uint ENCODING_MASK = 3u;

        private int _baseIndent;

        private string _indent = "  ";

        private string _newline = "\n";

        private bool _omitVersionAttribute;

        private int _padding = 2048;

        public bool OmitPacketWrapper
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

        public bool OmitXmpMetaElement
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

        public bool ReadOnlyPacket
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

        public bool UseCompactFormat
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

        public bool UseCanonicalFormat
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

        public bool IncludeThumbnailPad
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

        public bool ExactPacketLength
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

        public bool Sort
        {
            get
            {
                return GetOption(8192u);
            }
            set
            {
                SetOption(8192u, value);
            }
        }

        public bool EncodeUtf16Be
        {
            get
            {
                return (Options & 3) == 2;
            }
            set
            {
                SetOption(3u, value: false);
                SetOption(2u, value);
            }
        }

        public bool EncodeUtf16Le
        {
            get
            {
                return (Options & 3) == 3;
            }
            set
            {
                SetOption(3u, value: false);
                SetOption(3u, value);
            }
        }

        public int BaseIndent
        {
            get
            {
                return _baseIndent;
            }
            set
            {
                _baseIndent = value;
            }
        }

        public string Indent
        {
            get
            {
                return _indent;
            }
            set
            {
                _indent = value;
            }
        }

        public string Newline
        {
            get
            {
                return _newline;
            }
            set
            {
                _newline = value;
            }
        }

        public int Padding
        {
            get
            {
                return _padding;
            }
            set
            {
                _padding = value;
            }
        }

        public bool OmitVersionAttribute => _omitVersionAttribute;

        public string Encoding
        {
            get
            {
                if (EncodeUtf16Be)
                {
                    return "UTF-16BE";
                }

                if (EncodeUtf16Le)
                {
                    return "UTF-16LE";
                }

                return "UTF-8";
            }
        }

        protected internal override uint ValidOptions => 13168u;

        public SerializeOptions()
        {
        }

        public SerializeOptions(uint options)
            : base(options)
        {
        }

        public object Clone()
        {
            try
            {
                return new SerializeOptions(Options)
                {
                    BaseIndent = _baseIndent,
                    Indent = _indent,
                    Newline = _newline,
                    Padding = _padding
                };
            }
            catch (XmpException)
            {
                return null;
            }
        }

        protected internal override string DefineOptionName(uint option)
        {
            return option switch
            {
                16u => "OMIT_PACKET_WRAPPER",
                32u => "READONLY_PACKET",
                64u => "USE_COMPACT_FORMAT",
                256u => "INCLUDE_THUMBNAIL_PAD",
                512u => "EXACT_PACKET_LENGTH",
                4096u => "OMIT_XMPMETA_ELEMENT",
                8192u => "NORMALIZED",
                _ => null,
            };
        }
    }
}
