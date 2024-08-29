using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.pdf.draw;
using Sign.itext.pdf.interfaces;
using Sign.itext.text;
using Sign.itext.text.pdf;
using System.Text;

namespace Sign.itext
{
    public class Chunk : IElement, IAccessibleElement
    {
        public const string OBJECT_REPLACEMENT_CHARACTER = "￼";

        public static readonly Chunk NEWLINE;

        public static readonly Chunk NEXTPAGE;

        public static readonly Chunk TABBING;

        public static readonly Chunk SPACETABBING;

        protected StringBuilder content;

        protected Font font;

        protected Dictionary<string, object> attributes;

        protected internal PdfName role;

        protected internal Dictionary<PdfName, PdfObject> accessibleAttributes;

        protected AccessibleElementId id;

        public const string SEPARATOR = "SEPARATOR";

        public const string TAB = "TAB";

        public const string TABSETTINGS = "TABSETTINGS";

        private string contentWithNoTabs;

        public const string HSCALE = "HSCALE";

        public const string UNDERLINE = "UNDERLINE";

        public const string SUBSUPSCRIPT = "SUBSUPSCRIPT";

        public const string SKEW = "SKEW";

        public const string BACKGROUND = "BACKGROUND";

        public const string TEXTRENDERMODE = "TEXTRENDERMODE";

        public const string SPLITCHARACTER = "SPLITCHARACTER";

        public const string HYPHENATION = "HYPHENATION";

        public const string REMOTEGOTO = "REMOTEGOTO";

        public const string LOCALGOTO = "LOCALGOTO";

        public const string LOCALDESTINATION = "LOCALDESTINATION";

        public const string GENERICTAG = "GENERICTAG";

        public const string LINEHEIGHT = "LINEHEIGHT";

        public const string IMAGE = "IMAGE";

        public const string ACTION = "ACTION";

        public const string NEWPAGE = "NEWPAGE";

        public const string PDFANNOTATION = "PDFANNOTATION";

        public const string COLOR = "COLOR";

        public const string ENCODING = "ENCODING";

        public const string CHAR_SPACING = "CHAR_SPACING";

        public const string WORD_SPACING = "WORD_SPACING";

        public const string WHITESPACE = "WHITESPACE";

        public virtual int Type => 10;

        public virtual IList<Chunk> Chunks => new List<Chunk> { this };

        public virtual Font Font
        {
            get
            {
                return font;
            }
            set
            {
                font = value;
            }
        }

        public virtual string Content
        {
            get
            {
                if (contentWithNoTabs == null)
                {
                    contentWithNoTabs = content.ToString().Replace("\t", "");
                }

                return contentWithNoTabs;
            }
        }

        public virtual Dictionary<string, object> Attributes
        {
            get
            {
                return attributes;
            }
            set
            {
                attributes = value;
            }
        }

        public virtual float HorizontalScaling
        {
            get
            {
                if (attributes != null && attributes.ContainsKey("HSCALE"))
                {
                    return (float)attributes["HSCALE"];
                }

                return 1f;
            }
        }

        public virtual PdfName Role
        {
            get
            {
                if (GetImage() != null)
                {
                    return GetImage().Role;
                }

                return role;
            }
            set
            {
                if (GetImage() != null)
                {
                    GetImage().Role = value;
                }
                else
                {
                    role = value;
                }
            }
        }

        public virtual AccessibleElementId ID
        {
            get
            {
                if (id == null)
                {
                    id = new AccessibleElementId();
                }

                return id;
            }
            set
            {
                id = value;
            }
        }

        public virtual bool IsInline => true;

        static Chunk()
        {
            NEWLINE = new Chunk("\n");
            NEXTPAGE = new Chunk("");
            TABBING = new Chunk(float.NaN, isWhitespace: false);
            SPACETABBING = new Chunk(float.NaN, isWhitespace: true);
            NEXTPAGE.SetNewPage();
            NEWLINE.Role = PdfName.P;
        }

        public Chunk()
        {
            content = new StringBuilder();
            font = new Font();
            role = PdfName.SPAN;
        }

        public Chunk(Chunk ck)
        {
            if (ck.content != null)
            {
                content = new StringBuilder(ck.content.ToString());
            }

            if (ck.font != null)
            {
                font = new Font(ck.font);
            }

            if (ck.attributes != null)
            {
                attributes = new Dictionary<string, object>(ck.attributes);
            }

            role = ck.role;
            if (ck.accessibleAttributes != null)
            {
                accessibleAttributes = new Dictionary<PdfName, PdfObject>(ck.accessibleAttributes);
            }

            id = ck.ID;
        }

        public Chunk(string content, Font font)
        {
            this.content = new StringBuilder(content);
            this.font = font;
            role = PdfName.SPAN;
        }

        public Chunk(string content)
            : this(content, new Font())
        {
        }

        public Chunk(char c, Font font)
        {
            content = new StringBuilder();
            content.Append(c);
            this.font = font;
            role = PdfName.SPAN;
        }

        public Chunk(char c)
            : this(c, new Font())
        {
        }

        public Chunk(Image image, float offsetX, float offsetY)
            : this("￼", new Font())
        {
            Image instance = Image.GetInstance(image);
            instance.SetAbsolutePosition(float.NaN, float.NaN);
            SetAttribute("IMAGE", new object[4] { instance, offsetX, offsetY, false });
            role = null;
        }

        public Chunk(IDrawInterface separator)
            : this(separator, vertical: false)
        {
        }

        public Chunk(IDrawInterface separator, bool vertical)
            : this("￼", new Font())
        {
            SetAttribute("SEPARATOR", new object[2] { separator, vertical });
            role = null;
        }

        [Obsolete]
        public Chunk(IDrawInterface separator, float tabPosition)
            : this(separator, tabPosition, newline: false)
        {
        }

        [Obsolete]
        public Chunk(IDrawInterface separator, float tabPosition, bool newline)
            : this("￼", new Font())
        {
            if (tabPosition < 0f)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("a.tab.position.may.not.be.lower.than.0.yours.is.1", tabPosition));
            }

            SetAttribute("TAB", new object[4] { separator, tabPosition, newline, 0 });
            role = PdfName.ARTIFACT;
        }

        private Chunk(float tabInterval, bool isWhitespace)
            : this("￼", new Font())
        {
            if (tabInterval < 0f)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("a.tab.position.may.not.be.lower.than.0.yours.is.1", tabInterval));
            }

            SetAttribute("TAB", new object[2] { tabInterval, isWhitespace });
            SetAttribute("SPLITCHARACTER", TabSplitCharacter.TAB);
            SetAttribute("TABSETTINGS", null);
            role = PdfName.ARTIFACT;
        }

        public Chunk(Image image, float offsetX, float offsetY, bool changeLeading)
            : this("￼", new Font())
        {
            SetAttribute("IMAGE", new object[4] { image, offsetX, offsetY, changeLeading });
            role = PdfName.ARTIFACT;
        }

        public virtual bool Process(IElementListener listener)
        {
            try
            {
                return listener.Add(this);
            }
            catch (DocumentException)
            {
                return false;
            }
        }

        public virtual StringBuilder Append(string str)
        {
            contentWithNoTabs = null;
            return content.Append(str);
        }

        public override string ToString()
        {
            return Content;
        }

        public virtual bool IsEmpty()
        {
            if (content.ToString().Trim().Length == 0 && content.ToString().IndexOf("\n") == -1)
            {
                return attributes == null;
            }

            return false;
        }

        public virtual float GetWidthPoint()
        {
            if (GetImage() != null)
            {
                return GetImage().ScaledWidth;
            }

            return font.GetCalculatedBaseFont(specialEncoding: true).GetWidthPoint(Content, font.CalculatedSize) * HorizontalScaling;
        }

        public virtual bool HasAttributes()
        {
            return attributes != null;
        }

        private Chunk SetAttribute(string name, object obj)
        {
            if (attributes == null)
            {
                attributes = new Dictionary<string, object>();
            }

            attributes[name] = obj;
            return this;
        }

        public virtual Chunk SetHorizontalScaling(float scale)
        {
            return SetAttribute("HSCALE", scale);
        }

        public virtual Chunk SetUnderline(float thickness, float yPosition)
        {
            return SetUnderline(null, thickness, 0f, yPosition, 0f, 0);
        }

        public virtual Chunk SetUnderline(BaseColor color, float thickness, float thicknessMul, float yPosition, float yPositionMul, int cap)
        {
            if (attributes == null)
            {
                attributes = new Dictionary<string, object>();
            }

            object[] item = new object[2]
            {
                color,
                new float[5] { thickness, thicknessMul, yPosition, yPositionMul, cap }
            };
            object[][] original = null;
            if (attributes.ContainsKey("UNDERLINE"))
            {
                original = (object[][])attributes["UNDERLINE"];
            }

            object[][] obj = Utilities.AddToArray(original, item);
            return SetAttribute("UNDERLINE", obj);
        }

        public virtual Chunk SetTextRise(float rise)
        {
            return SetAttribute("SUBSUPSCRIPT", rise);
        }

        public virtual float GetTextRise()
        {
            if (attributes != null && attributes.ContainsKey("SUBSUPSCRIPT"))
            {
                return (float)attributes["SUBSUPSCRIPT"];
            }

            return 0f;
        }

        public virtual Chunk SetSkew(float alpha, float beta)
        {
            alpha = (float)Math.Tan((double)alpha * Math.PI / 180.0);
            beta = (float)Math.Tan((double)beta * Math.PI / 180.0);
            return SetAttribute("SKEW", new float[2] { alpha, beta });
        }

        public virtual Chunk SetBackground(BaseColor color)
        {
            return SetBackground(color, 0f, 0f, 0f, 0f);
        }

        public virtual Chunk SetBackground(BaseColor color, float extraLeft, float extraBottom, float extraRight, float extraTop)
        {
            return SetAttribute("BACKGROUND", new object[2]
            {
                color,
                new float[4] { extraLeft, extraBottom, extraRight, extraTop }
            });
        }

        public virtual Chunk SetTextRenderMode(int mode, float strokeWidth, BaseColor strokeColor)
        {
            return SetAttribute("TEXTRENDERMODE", new object[3] { mode, strokeWidth, strokeColor });
        }

        public virtual Chunk SetSplitCharacter(ISplitCharacter splitCharacter)
        {
            return SetAttribute("SPLITCHARACTER", splitCharacter);
        }

        public virtual Chunk SetHyphenation(IHyphenationEvent hyphenation)
        {
            return SetAttribute("HYPHENATION", hyphenation);
        }

        public virtual Chunk SetRemoteGoto(string filename, string name)
        {
            return SetAttribute("REMOTEGOTO", new object[2] { filename, name });
        }

        public virtual Chunk SetRemoteGoto(string filename, int page)
        {
            return SetAttribute("REMOTEGOTO", new object[2] { filename, page });
        }

        public virtual Chunk SetLocalGoto(string name)
        {
            return SetAttribute("LOCALGOTO", name);
        }

        public virtual Chunk SetLocalDestination(string name)
        {
            return SetAttribute("LOCALDESTINATION", name);
        }

        public virtual Chunk SetGenericTag(string text)
        {
            return SetAttribute("GENERICTAG", text);
        }

        public virtual Chunk setLineHeight(float lineheight)
        {
            return SetAttribute("LINEHEIGHT", lineheight);
        }

        public virtual Image GetImage()
        {
            if (attributes != null && attributes.ContainsKey("IMAGE"))
            {
                return (Image)((object[])attributes["IMAGE"])[0];
            }

            return null;
        }

        public virtual Chunk SetAction(PdfAction action)
        {
            Role = PdfName.LINK;
            return SetAttribute("ACTION", action);
        }

        public virtual Chunk SetAnchor(Uri url)
        {
            Role = PdfName.LINK;
            string absoluteUri = url.AbsoluteUri;
            SetAccessibleAttribute(PdfName.ALT, new PdfString(absoluteUri));
            return SetAttribute("ACTION", new PdfAction(absoluteUri));
        }

        public virtual Chunk SetAnchor(string url)
        {
            Role = PdfName.LINK;
            SetAccessibleAttribute(PdfName.ALT, new PdfString(url));
            return SetAttribute("ACTION", new PdfAction(url));
        }

        public virtual Chunk SetNewPage()
        {
            return SetAttribute("NEWPAGE", null);
        }

        public virtual Chunk SetAnnotation(PdfAnnotation annotation)
        {
            return SetAttribute("PDFANNOTATION", annotation);
        }

        public virtual bool IsContent()
        {
            return true;
        }

        public virtual bool IsNestable()
        {
            return true;
        }

        public virtual IHyphenationEvent GetHyphenation()
        {
            if (attributes != null && attributes.ContainsKey("HYPHENATION"))
            {
                return (IHyphenationEvent)attributes["HYPHENATION"];
            }

            return null;
        }

        public virtual Chunk SetCharacterSpacing(float charSpace)
        {
            return SetAttribute("CHAR_SPACING", charSpace);
        }

        public virtual float GetCharacterSpacing()
        {
            if (attributes != null && attributes.ContainsKey("CHAR_SPACING"))
            {
                return (float)attributes["CHAR_SPACING"];
            }

            return 0f;
        }

        public virtual Chunk SetWordSpacing(float wordSpace)
        {
            return SetAttribute("WORD_SPACING", wordSpace);
        }

        public virtual float GetWordSpacing()
        {
            if (attributes != null && attributes.ContainsKey("WORD_SPACING"))
            {
                return (float)attributes["WORD_SPACING"];
            }

            return 0f;
        }

        public static Chunk CreateWhitespace(string content)
        {
            return CreateWhitespace(content, preserve: false);
        }

        public static Chunk CreateWhitespace(string content, bool preserve)
        {
            Chunk chunk = null;
            if (!preserve)
            {
                chunk = new Chunk(' ');
                chunk.SetAttribute("WHITESPACE", content);
            }
            else
            {
                chunk = new Chunk(content);
            }

            return chunk;
        }

        public virtual bool IsWhitespace()
        {
            if (attributes != null)
            {
                return attributes.ContainsKey("WHITESPACE");
            }

            return false;
        }

        [Obsolete]
        public static Chunk CreateTabspace()
        {
            return CreateTabspace(60f);
        }

        [Obsolete]
        public static Chunk CreateTabspace(float spacing)
        {
            return new Chunk(spacing, isWhitespace: true);
        }

        [Obsolete]
        public virtual bool IsTabspace()
        {
            if (attributes != null)
            {
                return attributes.ContainsKey("TAB");
            }

            return false;
        }

        public virtual PdfObject GetAccessibleAttribute(PdfName key)
        {
            if (GetImage() != null)
            {
                return GetImage().GetAccessibleAttribute(key);
            }

            if (accessibleAttributes != null)
            {
                accessibleAttributes.TryGetValue(key, out var value);
                return value;
            }

            return null;
        }

        public virtual void SetAccessibleAttribute(PdfName key, PdfObject value)
        {
            if (GetImage() != null)
            {
                GetImage().SetAccessibleAttribute(key, value);
                return;
            }

            if (accessibleAttributes == null)
            {
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            }

            accessibleAttributes[key] = value;
        }

        public virtual Dictionary<PdfName, PdfObject> GetAccessibleAttributes()
        {
            if (GetImage() != null)
            {
                return GetImage().GetAccessibleAttributes();
            }

            return accessibleAttributes;
        }

        public virtual string GetTextExpansion()
        {
            PdfObject accessibleAttribute = GetAccessibleAttribute(PdfName.E);
            if (accessibleAttribute is PdfString)
            {
                return ((PdfString)accessibleAttribute).ToUnicodeString();
            }

            return null;
        }

        public virtual void SetTextExpansion(string value)
        {
            SetAccessibleAttribute(PdfName.E, new PdfString(value));
        }
    }
}
