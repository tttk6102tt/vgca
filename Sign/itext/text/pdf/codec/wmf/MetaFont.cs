using Sign.itext.pdf;
using System.Globalization;
using System.Text;

namespace Sign.itext.text.pdf.codec.wmf
{
    public class MetaFont : MetaObject
    {
        private static string[] fontNames = new string[14]
        {
            "Courier", "Courier-Bold", "Courier-Oblique", "Courier-BoldOblique", "Helvetica", "Helvetica-Bold", "Helvetica-Oblique", "Helvetica-BoldOblique", "Times-Roman", "Times-Bold",
            "Times-Italic", "Times-BoldItalic", "Symbol", "ZapfDingbats"
        };

        internal const int MARKER_BOLD = 1;

        internal const int MARKER_ITALIC = 2;

        internal const int MARKER_COURIER = 0;

        internal const int MARKER_HELVETICA = 4;

        internal const int MARKER_TIMES = 8;

        internal const int MARKER_SYMBOL = 12;

        internal const int DEFAULT_PITCH = 0;

        internal const int FIXED_PITCH = 1;

        internal const int VARIABLE_PITCH = 2;

        internal const int FF_DONTCARE = 0;

        internal const int FF_ROMAN = 1;

        internal const int FF_SWISS = 2;

        internal const int FF_MODERN = 3;

        internal const int FF_SCRIPT = 4;

        internal const int FF_DECORATIVE = 5;

        internal const int BOLDTHRESHOLD = 600;

        internal const int nameSize = 32;

        internal const int ETO_OPAQUE = 2;

        internal const int ETO_CLIPPED = 4;

        private int height;

        private float angle;

        private int bold;

        private int italic;

        private bool underline;

        private bool strikeout;

        private int charset;

        private int pitchAndFamily;

        private string faceName = "arial";

        private BaseFont font;

        public virtual BaseFont Font
        {
            get
            {
                if (this.font != null)
                {
                    return this.font;
                }

                Font font = FontFactory.GetFont(faceName, "Cp1252", embedded: true, 10f, ((italic != 0) ? 2 : 0) | ((bold != 0) ? 1 : 0));
                this.font = font.BaseFont;
                if (this.font != null)
                {
                    return this.font;
                }

                string name;
                if (faceName.IndexOf("courier") != -1 || faceName.IndexOf("terminal") != -1 || faceName.IndexOf("fixedsys") != -1)
                {
                    name = fontNames[italic + bold];
                }
                else if (faceName.IndexOf("ms sans serif") != -1 || faceName.IndexOf("arial") != -1 || faceName.IndexOf("system") != -1)
                {
                    name = fontNames[4 + italic + bold];
                }
                else if (faceName.IndexOf("arial black") != -1)
                {
                    name = fontNames[4 + italic + 1];
                }
                else if (faceName.IndexOf("times") != -1 || faceName.IndexOf("ms serif") != -1 || faceName.IndexOf("roman") != -1)
                {
                    name = fontNames[8 + italic + bold];
                }
                else if (faceName.IndexOf("symbol") != -1)
                {
                    name = fontNames[12];
                }
                else
                {
                    int num = pitchAndFamily & 3;
                    switch ((pitchAndFamily >> 4) & 7)
                    {
                        case 3:
                            name = fontNames[italic + bold];
                            break;
                        case 1:
                            name = fontNames[8 + italic + bold];
                            break;
                        case 2:
                        case 4:
                        case 5:
                            name = fontNames[4 + italic + bold];
                            break;
                        default:
                            name = ((num != 1) ? fontNames[4 + italic + bold] : fontNames[italic + bold]);
                            break;
                    }
                }

                this.font = BaseFont.CreateFont(name, "Cp1252", embedded: false);
                return this.font;
            }
        }

        public virtual float Angle => angle;

        public MetaFont()
        {
            type = 3;
        }

        public virtual void Init(InputMeta meta)
        {
            height = Math.Abs(meta.ReadShort());
            meta.Skip(2);
            angle = (float)((double)meta.ReadShort() / 1800.0 * Math.PI);
            meta.Skip(2);
            bold = ((meta.ReadShort() >= 600) ? 1 : 0);
            italic = ((meta.ReadByte() != 0) ? 2 : 0);
            underline = meta.ReadByte() != 0;
            strikeout = meta.ReadByte() != 0;
            charset = meta.ReadByte();
            meta.Skip(3);
            pitchAndFamily = meta.ReadByte();
            byte[] array = new byte[32];
            int i;
            for (i = 0; i < 32; i++)
            {
                int num = meta.ReadByte();
                if (num == 0)
                {
                    break;
                }

                array[i] = (byte)num;
            }

            try
            {
                faceName = Encoding.GetEncoding(1252).GetString(array, 0, i);
            }
            catch
            {
                faceName = Encoding.ASCII.GetString(array, 0, i);
            }

            faceName = faceName.ToLower(CultureInfo.InvariantCulture);
        }

        public virtual bool IsUnderline()
        {
            return underline;
        }

        public virtual bool IsStrikeout()
        {
            return strikeout;
        }

        public virtual float GetFontSize(MetaState state)
        {
            return Math.Abs(state.TransformY(height) - state.TransformY(0)) * Document.WmfFontCorrection;
        }
    }
}
