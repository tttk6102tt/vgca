using Sign.itext.error_messages;

namespace Sign.itext.text
{
    public sealed class FontFactory
    {
        public const string COURIER = "Courier";

        public const string COURIER_BOLD = "Courier-Bold";

        public const string COURIER_OBLIQUE = "Courier-Oblique";

        public const string COURIER_BOLDOBLIQUE = "Courier-BoldOblique";

        public const string HELVETICA = "Helvetica";

        public const string HELVETICA_BOLD = "Helvetica-Bold";

        public const string HELVETICA_OBLIQUE = "Helvetica-Oblique";

        public const string HELVETICA_BOLDOBLIQUE = "Helvetica-BoldOblique";

        public const string SYMBOL = "Symbol";

        public const string TIMES = "Times";

        public const string TIMES_ROMAN = "Times-Roman";

        public const string TIMES_BOLD = "Times-Bold";

        public const string TIMES_ITALIC = "Times-Italic";

        public const string TIMES_BOLDITALIC = "Times-BoldItalic";

        public const string ZAPFDINGBATS = "ZapfDingbats";

        private static FontFactoryImp fontImp = new FontFactoryImp();

        private const string defaultEncoding = "Cp1252";

        private const bool defaultEmbedding = false;

        public static ICollection<string> RegisteredFonts => fontImp.RegisteredFonts;

        public static ICollection<string> RegisteredFamilies => fontImp.RegisteredFamilies;

        public static string DefaultEncoding => "Cp1252";

        public static bool DefaultEmbedding => false;

        public static FontFactoryImp FontImp
        {
            get
            {
                return fontImp;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(MessageLocalization.GetComposedMessage("fontfactoryimp.cannot.be.null"));
                }

                fontImp = value;
            }
        }

        private FontFactory()
        {
        }

        public static Font GetFont(string fontname, string encoding, bool embedded, float size, int style, BaseColor color)
        {
            return fontImp.GetFont(fontname, encoding, embedded, size, style, color);
        }

        public static Font GetFont(string fontname, string encoding, bool embedded, float size, int style, BaseColor color, bool cached)
        {
            return fontImp.GetFont(fontname, encoding, embedded, size, style, color, cached);
        }

        public static Font GetFont(string fontname, string encoding, bool embedded, float size, int style)
        {
            return GetFont(fontname, encoding, embedded, size, style, null);
        }

        public static Font GetFont(string fontname, string encoding, bool embedded, float size)
        {
            return GetFont(fontname, encoding, embedded, size, -1, null);
        }

        public static Font GetFont(string fontname, string encoding, bool embedded)
        {
            return GetFont(fontname, encoding, embedded, -1f, -1, null);
        }

        public static Font GetFont(string fontname, string encoding, float size, int style, BaseColor color)
        {
            return GetFont(fontname, encoding, embedded: false, size, style, color);
        }

        public static Font GetFont(string fontname, string encoding, float size, int style)
        {
            return GetFont(fontname, encoding, embedded: false, size, style, null);
        }

        public static Font GetFont(string fontname, string encoding, float size)
        {
            return GetFont(fontname, encoding, embedded: false, size, -1, null);
        }

        public static Font GetFont(string fontname, string encoding)
        {
            return GetFont(fontname, encoding, embedded: false, -1f, -1, null);
        }

        public static Font GetFont(string fontname, float size, int style, BaseColor color)
        {
            return GetFont(fontname, "Cp1252", embedded: false, size, style, color);
        }

        public static Font GetFont(string fontname, float size, BaseColor color)
        {
            return GetFont(fontname, "Cp1252", embedded: false, size, -1, color);
        }

        public static Font GetFont(string fontname, float size, int style)
        {
            return GetFont(fontname, "Cp1252", embedded: false, size, style, null);
        }

        public static Font GetFont(string fontname, float size)
        {
            return GetFont(fontname, "Cp1252", embedded: false, size, -1, null);
        }

        public static Font GetFont(string fontname)
        {
            return GetFont(fontname, "Cp1252", embedded: false, -1f, -1, null);
        }

        public static void RegisterFamily(string familyName, string fullName, string path)
        {
            fontImp.RegisterFamily(familyName, fullName, path);
        }

        public static void Register(Sign.SystemItext.util.Properties attributes)
        {
            string text = null;
            string path = attributes.Remove("path");
            text = attributes.Remove("alias");
            fontImp.Register(path, text);
        }

        public static void Register(string path)
        {
            Register(path, null);
        }

        public static void Register(string path, string alias)
        {
            fontImp.Register(path, alias);
        }

        public static int RegisterDirectory(string dir)
        {
            return fontImp.RegisterDirectory(dir);
        }

        public static int RegisterDirectory(string dir, bool scanSubdirectories)
        {
            return fontImp.RegisterDirectory(dir, scanSubdirectories);
        }

        public static int RegisterDirectories()
        {
            return fontImp.RegisterDirectories();
        }

        public static bool Contains(string fontname)
        {
            return fontImp.IsRegistered(fontname);
        }

        public static bool IsRegistered(string fontname)
        {
            return fontImp.IsRegistered(fontname);
        }
    }
}
