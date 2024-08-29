using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfAppearance : PdfTemplate
    {
        public static Dictionary<string, PdfName> stdFieldFontNames;

        public override PdfContentByte Duplicate
        {
            get
            {
                PdfAppearance pdfAppearance = new PdfAppearance();
                pdfAppearance.writer = writer;
                pdfAppearance.pdf = pdf;
                pdfAppearance.thisReference = thisReference;
                pdfAppearance.pageResources = pageResources;
                pdfAppearance.bBox = new Rectangle(bBox);
                pdfAppearance.group = group;
                pdfAppearance.layer = layer;
                if (matrix != null)
                {
                    pdfAppearance.matrix = new PdfArray(matrix);
                }

                pdfAppearance.separator = separator;
                return pdfAppearance;
            }
        }

        static PdfAppearance()
        {
            stdFieldFontNames = new Dictionary<string, PdfName>();
            stdFieldFontNames["Courier-BoldOblique"] = new PdfName("CoBO");
            stdFieldFontNames["Courier-Bold"] = new PdfName("CoBo");
            stdFieldFontNames["Courier-Oblique"] = new PdfName("CoOb");
            stdFieldFontNames["Courier"] = new PdfName("Cour");
            stdFieldFontNames["Helvetica-BoldOblique"] = new PdfName("HeBO");
            stdFieldFontNames["Helvetica-Bold"] = new PdfName("HeBo");
            stdFieldFontNames["Helvetica-Oblique"] = new PdfName("HeOb");
            stdFieldFontNames["Helvetica"] = PdfName.HELV;
            stdFieldFontNames["Symbol"] = new PdfName("Symb");
            stdFieldFontNames["Times-BoldItalic"] = new PdfName("TiBI");
            stdFieldFontNames["Times-Bold"] = new PdfName("TiBo");
            stdFieldFontNames["Times-Italic"] = new PdfName("TiIt");
            stdFieldFontNames["Times-Roman"] = new PdfName("TiRo");
            stdFieldFontNames["ZapfDingbats"] = PdfName.ZADB;
            stdFieldFontNames["HYSMyeongJo-Medium"] = new PdfName("HySm");
            stdFieldFontNames["HYGoThic-Medium"] = new PdfName("HyGo");
            stdFieldFontNames["HeiseiKakuGo-W5"] = new PdfName("KaGo");
            stdFieldFontNames["HeiseiMin-W3"] = new PdfName("KaMi");
            stdFieldFontNames["MHei-Medium"] = new PdfName("MHei");
            stdFieldFontNames["MSung-Light"] = new PdfName("MSun");
            stdFieldFontNames["STSong-Light"] = new PdfName("STSo");
            stdFieldFontNames["MSungStd-Light"] = new PdfName("MSun");
            stdFieldFontNames["STSongStd-Light"] = new PdfName("STSo");
            stdFieldFontNames["HYSMyeongJoStd-Medium"] = new PdfName("HySm");
            stdFieldFontNames["KozMinPro-Regular"] = new PdfName("KaMi");
        }

        internal PdfAppearance()
        {
            separator = 32;
        }

        internal PdfAppearance(PdfIndirectReference iref)
        {
            thisReference = iref;
        }

        internal PdfAppearance(PdfWriter wr)
            : base(wr)
        {
            separator = 32;
        }

        public static PdfAppearance CreateAppearance(PdfWriter writer, float width, float height)
        {
            return CreateAppearance(writer, width, height, null);
        }

        internal static PdfAppearance CreateAppearance(PdfWriter writer, float width, float height, PdfName forcedName)
        {
            PdfAppearance pdfAppearance = new PdfAppearance(writer);
            pdfAppearance.Width = width;
            pdfAppearance.Height = height;
            writer.AddDirectTemplateSimple(pdfAppearance, forcedName);
            return pdfAppearance;
        }

        public override void SetFontAndSize(BaseFont bf, float size)
        {
            CheckWriter();
            state.size = size;
            if (bf.FontType == 4)
            {
                state.fontDetails = new FontDetails(null, ((DocumentFont)bf).IndirectReference, bf);
            }
            else
            {
                state.fontDetails = writer.AddSimple(bf);
            }

            stdFieldFontNames.TryGetValue(bf.PostscriptFontName, out var value);
            if (value == null)
            {
                if (bf.Subset && bf.FontType == 3)
                {
                    value = state.fontDetails.FontName;
                }
                else
                {
                    value = new PdfName(bf.PostscriptFontName);
                    state.fontDetails.Subset = false;
                }
            }

            PageResources.AddFont(value, state.fontDetails.IndirectReference);
            content.Append(value.GetBytes()).Append(' ').Append(size)
                .Append(" Tf")
                .Append_i(separator);
        }
    }
}
