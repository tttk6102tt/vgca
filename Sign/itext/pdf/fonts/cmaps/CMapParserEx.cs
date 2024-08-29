using Sign.itext.text.pdf;

namespace Sign.itext.pdf.fonts.cmaps
{
    public class CMapParserEx
    {
        private static readonly PdfName CMAPNAME = new PdfName("CMapName");

        private const string DEF = "def";

        private const string ENDCIDRANGE = "endcidrange";

        private const string ENDCIDCHAR = "endcidchar";

        private const string ENDBFRANGE = "endbfrange";

        private const string ENDBFCHAR = "endbfchar";

        private const string USECMAP = "usecmap";

        private const int MAXLEVEL = 10;

        public static void ParseCid(string cmapName, AbstractCMap cmap, ICidLocation location)
        {
            ParseCid(cmapName, cmap, location, 0);
        }

        private static void ParseCid(string cmapName, AbstractCMap cmap, ICidLocation location, int level)
        {
            if (level >= 10)
            {
                return;
            }

            PRTokeniser location2 = location.GetLocation(cmapName);
            try
            {
                List<PdfObject> list = new List<PdfObject>();
                PdfContentParser pdfContentParser = new PdfContentParser(location2);
                int num = 50;
                while (true)
                {
                    try
                    {
                        pdfContentParser.Parse(list);
                    }
                    catch
                    {
                        if (--num < 0)
                        {
                            return;
                        }

                        continue;
                    }

                    if (list.Count == 0)
                    {
                        break;
                    }

                    string text = list[list.Count - 1].ToString();
                    if (level == 0 && list.Count == 3 && text.Equals("def"))
                    {
                        PdfObject obj2 = list[0];
                        if (PdfName.REGISTRY.Equals(obj2))
                        {
                            cmap.Registry = list[1].ToString();
                        }
                        else if (PdfName.ORDERING.Equals(obj2))
                        {
                            cmap.Ordering = list[1].ToString();
                        }
                        else if (CMAPNAME.Equals(obj2))
                        {
                            cmap.Name = list[1].ToString();
                        }
                        else if (PdfName.SUPPLEMENT.Equals(obj2))
                        {
                            try
                            {
                                cmap.Supplement = ((PdfNumber)list[1]).IntValue;
                            }
                            catch
                            {
                            }
                        }
                    }
                    else if ((text.Equals("endcidchar") || text.Equals("endbfchar")) && list.Count >= 3)
                    {
                        int num2 = list.Count - 2;
                        for (int i = 0; i < num2; i += 2)
                        {
                            if (list[i] is PdfString)
                            {
                                cmap.AddChar((PdfString)list[i], list[i + 1]);
                            }
                        }
                    }
                    else if ((text.Equals("endcidrange") || text.Equals("endbfrange")) && list.Count >= 4)
                    {
                        int num3 = list.Count - 3;
                        for (int j = 0; j < num3; j += 3)
                        {
                            if (list[j] is PdfString && list[j + 1] is PdfString)
                            {
                                cmap.AddRange((PdfString)list[j], (PdfString)list[j + 1], list[j + 2]);
                            }
                        }
                    }
                    else if (text.Equals("usecmap") && list.Count == 2 && list[0] is PdfName)
                    {
                        ParseCid(PdfName.DecodeName(list[0].ToString()), cmap, location, level + 1);
                    }
                }
            }
            finally
            {
                location2.Close();
            }
        }
    }
}
