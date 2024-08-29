using Sign.itext.pdf;
using Sign.itext.text.log;
using System.Globalization;

namespace Sign.itext.text
{
    public class FontFactoryImp : IFontProvider
    {
        private static readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(FontFactoryImp));

        private Dictionary<string, string> trueTypeFonts = new Dictionary<string, string>();

        private static string[] TTFamilyOrder = new string[12]
        {
            "3", "1", "1033", "3", "0", "1033", "1", "0", "0", "0",
            "3", "0"
        };

        private Dictionary<string, List<string>> fontFamilies = new Dictionary<string, List<string>>();

        private string defaultEncoding = "Cp1252";

        private bool defaultEmbedding;

        public virtual ICollection<string> RegisteredFonts => trueTypeFonts.Keys;

        public virtual ICollection<string> RegisteredFamilies => fontFamilies.Keys;

        public virtual string DefaultEncoding
        {
            get
            {
                return defaultEncoding;
            }
            set
            {
                defaultEncoding = value;
            }
        }

        public virtual bool DefaultEmbedding
        {
            get
            {
                return defaultEmbedding;
            }
            set
            {
                defaultEmbedding = value;
            }
        }

        public FontFactoryImp()
        {
            trueTypeFonts.Add("Courier".ToLower(CultureInfo.InvariantCulture), "Courier");
            trueTypeFonts.Add("Courier-Bold".ToLower(CultureInfo.InvariantCulture), "Courier-Bold");
            trueTypeFonts.Add("Courier-Oblique".ToLower(CultureInfo.InvariantCulture), "Courier-Oblique");
            trueTypeFonts.Add("Courier-BoldOblique".ToLower(CultureInfo.InvariantCulture), "Courier-BoldOblique");
            trueTypeFonts.Add("Helvetica".ToLower(CultureInfo.InvariantCulture), "Helvetica");
            trueTypeFonts.Add("Helvetica-Bold".ToLower(CultureInfo.InvariantCulture), "Helvetica-Bold");
            trueTypeFonts.Add("Helvetica-Oblique".ToLower(CultureInfo.InvariantCulture), "Helvetica-Oblique");
            trueTypeFonts.Add("Helvetica-BoldOblique".ToLower(CultureInfo.InvariantCulture), "Helvetica-BoldOblique");
            trueTypeFonts.Add("Symbol".ToLower(CultureInfo.InvariantCulture), "Symbol");
            trueTypeFonts.Add("Times-Roman".ToLower(CultureInfo.InvariantCulture), "Times-Roman");
            trueTypeFonts.Add("Times-Bold".ToLower(CultureInfo.InvariantCulture), "Times-Bold");
            trueTypeFonts.Add("Times-Italic".ToLower(CultureInfo.InvariantCulture), "Times-Italic");
            trueTypeFonts.Add("Times-BoldItalic".ToLower(CultureInfo.InvariantCulture), "Times-BoldItalic");
            trueTypeFonts.Add("ZapfDingbats".ToLower(CultureInfo.InvariantCulture), "ZapfDingbats");
            List<string> value = new List<string> { "Courier", "Courier-Bold", "Courier-Oblique", "Courier-BoldOblique" };
            fontFamilies["Courier".ToLower(CultureInfo.InvariantCulture)] = value;
            value = new List<string> { "Helvetica", "Helvetica-Bold", "Helvetica-Oblique", "Helvetica-BoldOblique" };
            fontFamilies["Helvetica".ToLower(CultureInfo.InvariantCulture)] = value;
            value = new List<string> { "Symbol" };
            fontFamilies["Symbol".ToLower(CultureInfo.InvariantCulture)] = value;
            value = new List<string> { "Times-Roman", "Times-Bold", "Times-Italic", "Times-BoldItalic" };
            fontFamilies["Times".ToLower(CultureInfo.InvariantCulture)] = value;
            fontFamilies["Times-Roman".ToLower(CultureInfo.InvariantCulture)] = value;
            value = new List<string> { "ZapfDingbats" };
            fontFamilies["ZapfDingbats".ToLower(CultureInfo.InvariantCulture)] = value;
        }

        public virtual Font GetFont(string fontname, string encoding, bool embedded, float size, int style, BaseColor color)
        {
            return GetFont(fontname, encoding, embedded, size, style, color, cached: true);
        }

        public virtual Font GetFont(string fontname, string encoding, bool embedded, float size, int style, BaseColor color, bool cached)
        {
            if (fontname == null)
            {
                return new Font(Font.FontFamily.UNDEFINED, size, style, color);
            }

            string key = fontname.ToLower(CultureInfo.InvariantCulture);
            fontFamilies.TryGetValue(key, out var value);
            if (value != null)
            {
                lock (value)
                {
                    int num = ((style != -1) ? style : 0);
                    int num2 = 0;
                    bool flag = false;
                    foreach (string item in value)
                    {
                        string text = item.ToLower(CultureInfo.InvariantCulture);
                        num2 = 0;
                        if (text.IndexOf("bold") != -1)
                        {
                            num2 |= 1;
                        }

                        if (text.IndexOf("italic") != -1 || text.IndexOf("oblique") != -1)
                        {
                            num2 |= 2;
                        }

                        if ((num & 3) == num2)
                        {
                            fontname = item;
                            flag = true;
                            break;
                        }
                    }

                    if (style != -1 && flag)
                    {
                        style &= ~num2;
                    }
                }
            }

            BaseFont baseFont = null;
            try
            {
                baseFont = GetBaseFont(fontname, encoding, embedded, cached);
                if (baseFont == null)
                {
                    return new Font(Font.FontFamily.UNDEFINED, size, style, color);
                }
            }
            catch (DocumentException ex)
            {
                throw ex;
            }
            catch (IOException)
            {
                return new Font(Font.FontFamily.UNDEFINED, size, style, color);
            }
            catch
            {
                return new Font(Font.FontFamily.UNDEFINED, size, style, color);
            }

            return new Font(baseFont, size, style, color);
        }

        protected virtual BaseFont GetBaseFont(string fontname, string encoding, bool embedded, bool cached)
        {
            BaseFont baseFont = null;
            try
            {
                baseFont = BaseFont.CreateFont(fontname, encoding, embedded, cached, null, null, noThrow: true);
            }
            catch (DocumentException)
            {
            }

            if (baseFont == null)
            {
                trueTypeFonts.TryGetValue(fontname.ToLowerInvariant(), out fontname);
                if (fontname != null)
                {
                    baseFont = BaseFont.CreateFont(fontname, encoding, embedded, cached, null, null);
                }
            }

            return baseFont;
        }

        public virtual Font GetFont(string fontname, string encoding, bool embedded, float size, int style)
        {
            return GetFont(fontname, encoding, embedded, size, style, null);
        }

        public virtual Font GetFont(string fontname, string encoding, bool embedded, float size)
        {
            return GetFont(fontname, encoding, embedded, size, -1, null);
        }

        public virtual Font GetFont(string fontname, string encoding, bool embedded)
        {
            return GetFont(fontname, encoding, embedded, -1f, -1, null);
        }

        public virtual Font GetFont(string fontname, string encoding, float size, int style, BaseColor color)
        {
            return GetFont(fontname, encoding, defaultEmbedding, size, style, color);
        }

        public virtual Font GetFont(string fontname, string encoding, float size, int style)
        {
            return GetFont(fontname, encoding, defaultEmbedding, size, style, null);
        }

        public virtual Font GetFont(string fontname, string encoding, float size)
        {
            return GetFont(fontname, encoding, defaultEmbedding, size, -1, null);
        }

        public virtual Font GetFont(string fontname, string encoding)
        {
            return GetFont(fontname, encoding, defaultEmbedding, -1f, -1, null);
        }

        public virtual Font GetFont(string fontname, float size, int style, BaseColor color)
        {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, size, style, color);
        }

        public virtual Font GetFont(string fontname, float size, BaseColor color)
        {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, size, -1, color);
        }

        public virtual Font GetFont(string fontname, float size, int style)
        {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, size, style, null);
        }

        public virtual Font GetFont(string fontname, float size)
        {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, size, -1, null);
        }

        public virtual Font GetFont(string fontname)
        {
            return GetFont(fontname, defaultEncoding, defaultEmbedding, -1f, -1, null);
        }

        public virtual void RegisterFamily(string familyName, string fullName, string path)
        {
            if (path != null)
            {
                trueTypeFonts[fullName] = path;
            }

            List<string> value;
            lock (fontFamilies)
            {
                fontFamilies.TryGetValue(familyName, out value);
                if (value == null)
                {
                    value = new List<string>();
                    value.Add(fullName);
                    fontFamilies[familyName] = value;
                }
            }

            lock (value)
            {
                if (value.Contains(fullName))
                {
                    return;
                }

                int length = fullName.Length;
                bool flag = false;
                for (int i = 0; i < value.Count; i++)
                {
                    if (value[i].Length >= length)
                    {
                        value.Insert(i, fullName);
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    value.Add(fullName);
                }
            }
        }

        public virtual void Register(string path)
        {
            Register(path, null);
        }

        public virtual void Register(string path, string alias)
        {
            try
            {
                if (path.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttf") || path.ToLower(CultureInfo.InvariantCulture).EndsWith(".otf") || path.ToLower(CultureInfo.InvariantCulture).IndexOf(".ttc,") > 0)
                {
                    object[] allFontNames = BaseFont.GetAllFontNames(path, "Cp1252", null);
                    trueTypeFonts[((string)allFontNames[0]).ToLower(CultureInfo.InvariantCulture)] = path;
                    if (alias != null)
                    {
                        trueTypeFonts[alias.ToLower(CultureInfo.InvariantCulture)] = path;
                    }

                    string[][] array = (string[][])allFontNames[2];
                    for (int i = 0; i < array.Length; i++)
                    {
                        trueTypeFonts[array[i][3].ToLower(CultureInfo.InvariantCulture)] = path;
                    }

                    string text = null;
                    string text2 = null;
                    array = (string[][])allFontNames[1];
                    for (int j = 0; j < TTFamilyOrder.Length; j += 3)
                    {
                        string[][] array2 = array;
                        foreach (string[] array3 in array2)
                        {
                            if (TTFamilyOrder[j].Equals(array3[0]) && TTFamilyOrder[j + 1].Equals(array3[1]) && TTFamilyOrder[j + 2].Equals(array3[2]))
                            {
                                text2 = array3[3].ToLower(CultureInfo.InvariantCulture);
                                j = TTFamilyOrder.Length;
                                break;
                            }
                        }
                    }

                    if (text2 != null)
                    {
                        string value = "";
                        array = (string[][])allFontNames[2];
                        string[][] array2 = array;
                        foreach (string[] array4 in array2)
                        {
                            for (int l = 0; l < TTFamilyOrder.Length; l += 3)
                            {
                                if (TTFamilyOrder[l].Equals(array4[0]) && TTFamilyOrder[l + 1].Equals(array4[1]) && TTFamilyOrder[l + 2].Equals(array4[2]))
                                {
                                    text = array4[3];
                                    if (!text.Equals(value))
                                    {
                                        value = text;
                                        RegisterFamily(text2, text, null);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (path.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttc"))
                {
                    if (alias != null)
                    {
                        LOGGER.Error("You can't define an alias for a true type collection.");
                    }

                    string[] array5 = BaseFont.EnumerateTTCNames(path);
                    for (int m = 0; m < array5.Length; m++)
                    {
                        Register(path + "," + m);
                    }
                }
                else if (path.ToLower(CultureInfo.InvariantCulture).EndsWith(".afm") || path.ToLower(CultureInfo.InvariantCulture).EndsWith(".pfm"))
                {
                    BaseFont baseFont = BaseFont.CreateFont(path, "Cp1252", embedded: false);
                    string text3 = baseFont.FullFontName[0][3].ToLower(CultureInfo.InvariantCulture);
                    string familyName = baseFont.FamilyFontName[0][3].ToLower(CultureInfo.InvariantCulture);
                    string key = baseFont.PostscriptFontName.ToLower(CultureInfo.InvariantCulture);
                    RegisterFamily(familyName, text3, null);
                    trueTypeFonts[key] = path;
                    trueTypeFonts[text3] = path;
                }

                if (LOGGER.IsLogging(Level.TRACE))
                {
                    LOGGER.Trace($"Registered {path}");
                }
            }
            catch (DocumentException ex)
            {
                throw ex;
            }
            catch (IOException ex2)
            {
                throw ex2;
            }
        }

        public virtual int RegisterDirectory(string dir)
        {
            return RegisterDirectory(dir, scanSubdirectories: false);
        }

        public virtual int RegisterDirectory(string dir, bool scanSubdirectories)
        {
            if (LOGGER.IsLogging(Level.DEBUG))
            {
                LOGGER.Debug($"Registering directory {dir}, looking for fonts");
            }

            int num = 0;
            try
            {
                if (!Directory.Exists(dir))
                {
                    return 0;
                }

                string[] files = Directory.GetFiles(dir);
                if (files == null)
                {
                    return 0;
                }

                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        if (Directory.Exists(files[i]))
                        {
                            if (scanSubdirectories)
                            {
                                num += RegisterDirectory(Path.GetFullPath(files[i]), scanSubdirectories: true);
                            }

                            continue;
                        }

                        string fullPath = Path.GetFullPath(files[i]);
                        string value = ((fullPath.Length < 4) ? null : fullPath.Substring(fullPath.Length - 4).ToLower(CultureInfo.InvariantCulture));
                        if (".afm".Equals(value) || ".pfm".Equals(value))
                        {
                            if (File.Exists(fullPath.Substring(0, fullPath.Length - 4) + ".pfb"))
                            {
                                Register(fullPath, null);
                                num++;
                            }
                        }
                        else if (".ttf".Equals(value) || ".otf".Equals(value) || ".ttc".Equals(value))
                        {
                            Register(fullPath, null);
                            num++;
                        }
                    }
                    catch
                    {
                    }
                }

                return num;
            }
            catch
            {
                return num;
            }
        }

        public virtual int RegisterDirectories()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                return 0 + RegisterDirectory("/usr/share/X11/fonts", scanSubdirectories: true) + RegisterDirectory("/usr/X/lib/X11/fonts", scanSubdirectories: true) + RegisterDirectory("/usr/openwin/lib/X11/fonts", scanSubdirectories: true) + RegisterDirectory("/usr/share/fonts", scanSubdirectories: true) + RegisterDirectory("/usr/X11R6/lib/X11/fonts", scanSubdirectories: true) + RegisterDirectory("/Library/Fonts") + RegisterDirectory("/System/Library/Fonts");
            }

            string dir = Path.Combine(Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.System)), "Fonts");
            return RegisterDirectory(dir);
        }

        public virtual bool IsRegistered(string fontname)
        {
            return trueTypeFonts.ContainsKey(fontname.ToLower(CultureInfo.InvariantCulture));
        }
    }
}
