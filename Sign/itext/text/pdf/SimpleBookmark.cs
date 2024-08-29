using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.xml;
using Sign.itext.xml.simpleparser;
using Sign.SystemItext.util;
using System.Globalization;
using System.Text;

namespace Sign.itext.text.pdf
{
    public sealed class SimpleBookmark : ISimpleXMLDocHandler
    {
        private List<Dictionary<string, object>> topList;

        private Stack<Dictionary<string, object>> attr = new Stack<Dictionary<string, object>>();

        private SimpleBookmark()
        {
        }

        private static IList<Dictionary<string, object>> BookmarkDepth(PdfReader reader, PdfDictionary outline, IntHashtable pages, bool processCurrentOutlineOnly)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            while (outline != null)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                PdfString pdfString = (PdfString)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.TITLE));
                dictionary["Title"] = pdfString.ToUnicodeString();
                PdfArray pdfArray = (PdfArray)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.C));
                if (pdfArray != null && pdfArray.Size == 3)
                {
                    ByteBuffer byteBuffer = new ByteBuffer();
                    byteBuffer.Append(pdfArray.GetAsNumber(0).FloatValue).Append(' ');
                    byteBuffer.Append(pdfArray.GetAsNumber(1).FloatValue).Append(' ');
                    byteBuffer.Append(pdfArray.GetAsNumber(2).FloatValue);
                    dictionary["Color"] = PdfEncodings.ConvertToString(byteBuffer.ToByteArray(), null);
                }

                PdfNumber pdfNumber = (PdfNumber)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.F));
                if (pdfNumber != null)
                {
                    int intValue = pdfNumber.IntValue;
                    string text = "";
                    if (((uint)intValue & (true ? 1u : 0u)) != 0)
                    {
                        text += "italic ";
                    }

                    if (((uint)intValue & 2u) != 0)
                    {
                        text += "bold ";
                    }

                    text = text.Trim();
                    if (text.Length != 0)
                    {
                        dictionary["Style"] = text;
                    }
                }

                PdfNumber pdfNumber2 = (PdfNumber)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.COUNT));
                if (pdfNumber2 != null && pdfNumber2.IntValue < 0)
                {
                    dictionary["Open"] = "false";
                }

                try
                {
                    PdfObject pdfObjectRelease = PdfReader.GetPdfObjectRelease(outline.Get(PdfName.DEST));
                    if (pdfObjectRelease != null)
                    {
                        MapGotoBookmark(dictionary, pdfObjectRelease, pages);
                    }
                    else
                    {
                        PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.A));
                        if (pdfDictionary != null)
                        {
                            if (PdfName.GOTO.Equals(PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.S))))
                            {
                                pdfObjectRelease = PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.D));
                                if (pdfObjectRelease != null)
                                {
                                    MapGotoBookmark(dictionary, pdfObjectRelease, pages);
                                }
                            }
                            else if (PdfName.URI.Equals(PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.S))))
                            {
                                dictionary["Action"] = "URI";
                                dictionary["URI"] = ((PdfString)PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.URI))).ToUnicodeString();
                            }
                            else if (PdfName.JAVASCRIPT.Equals(PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.S))))
                            {
                                dictionary["Action"] = "JS";
                                dictionary["Code"] = PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.JS)).ToString();
                            }
                            else if (PdfName.GOTOR.Equals(PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.S))))
                            {
                                pdfObjectRelease = PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.D));
                                if (pdfObjectRelease != null)
                                {
                                    if (pdfObjectRelease.IsString())
                                    {
                                        dictionary["Named"] = pdfObjectRelease.ToString();
                                    }
                                    else if (pdfObjectRelease.IsName())
                                    {
                                        dictionary["NamedN"] = PdfName.DecodeName(pdfObjectRelease.ToString());
                                    }
                                    else if (pdfObjectRelease.IsArray())
                                    {
                                        PdfArray pdfArray2 = (PdfArray)pdfObjectRelease;
                                        StringBuilder stringBuilder = new StringBuilder();
                                        stringBuilder.Append(pdfArray2[0].ToString());
                                        stringBuilder.Append(' ').Append(pdfArray2[1].ToString());
                                        for (int i = 2; i < pdfArray2.Size; i++)
                                        {
                                            stringBuilder.Append(' ').Append(pdfArray2[i].ToString());
                                        }

                                        dictionary["Page"] = stringBuilder.ToString();
                                    }
                                }

                                dictionary["Action"] = "GoToR";
                                PdfObject pdfObjectRelease2 = PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.F));
                                if (pdfObjectRelease2 != null)
                                {
                                    if (pdfObjectRelease2.IsString())
                                    {
                                        dictionary["File"] = ((PdfString)pdfObjectRelease2).ToUnicodeString();
                                    }
                                    else if (pdfObjectRelease2.IsDictionary())
                                    {
                                        pdfObjectRelease2 = PdfReader.GetPdfObject(((PdfDictionary)pdfObjectRelease2).Get(PdfName.F));
                                        if (pdfObjectRelease2.IsString())
                                        {
                                            dictionary["File"] = ((PdfString)pdfObjectRelease2).ToUnicodeString();
                                        }
                                    }
                                }

                                PdfObject pdfObjectRelease3 = PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.NEWWINDOW));
                                if (pdfObjectRelease3 != null)
                                {
                                    dictionary["NewWindow"] = pdfObjectRelease3.ToString();
                                }
                            }
                            else if (PdfName.LAUNCH.Equals(PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.S))))
                            {
                                dictionary["Action"] = "Launch";
                                PdfObject pdfObjectRelease4 = PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.F));
                                if (pdfObjectRelease4 == null)
                                {
                                    pdfObjectRelease4 = PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.WIN));
                                }

                                if (pdfObjectRelease4 != null)
                                {
                                    if (pdfObjectRelease4.IsString())
                                    {
                                        dictionary["File"] = ((PdfString)pdfObjectRelease4).ToUnicodeString();
                                    }
                                    else if (pdfObjectRelease4.IsDictionary())
                                    {
                                        pdfObjectRelease4 = PdfReader.GetPdfObjectRelease(((PdfDictionary)pdfObjectRelease4).Get(PdfName.F));
                                        if (pdfObjectRelease4.IsString())
                                        {
                                            dictionary["File"] = ((PdfString)pdfObjectRelease4).ToUnicodeString();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                }

                PdfDictionary pdfDictionary2 = (PdfDictionary)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.FIRST));
                if (pdfDictionary2 != null)
                {
                    dictionary["Kids"] = BookmarkDepth(reader, pdfDictionary2, pages, processCurrentOutlineOnly: false);
                }

                list.Add(dictionary);
                outline = (processCurrentOutlineOnly ? null : ((PdfDictionary)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.NEXT))));
            }

            return list;
        }

        private static void MapGotoBookmark(Dictionary<string, object> map, PdfObject dest, IntHashtable pages)
        {
            if (dest.IsString())
            {
                map["Named"] = dest.ToString();
            }
            else if (dest.IsName())
            {
                map["Named"] = PdfName.DecodeName(dest.ToString());
            }
            else if (dest.IsArray())
            {
                map["Page"] = MakeBookmarkParam((PdfArray)dest, pages);
            }

            map["Action"] = "GoTo";
        }

        private static string MakeBookmarkParam(PdfArray dest, IntHashtable pages)
        {
            StringBuilder stringBuilder = new StringBuilder();
            PdfObject pdfObject = dest[0];
            if (pdfObject.IsNumber())
            {
                stringBuilder.Append(((PdfNumber)pdfObject).IntValue + 1);
            }
            else
            {
                stringBuilder.Append(pages[GetNumber((PdfIndirectReference)pdfObject)]);
            }

            stringBuilder.Append(' ').Append(dest[1].ToString().Substring(1));
            for (int i = 2; i < dest.Size; i++)
            {
                stringBuilder.Append(' ').Append(dest[i].ToString());
            }

            return stringBuilder.ToString();
        }

        private static int GetNumber(PdfIndirectReference indirect)
        {
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObjectRelease(indirect);
            if (pdfDictionary.Contains(PdfName.TYPE) && pdfDictionary.Get(PdfName.TYPE).Equals(PdfName.PAGES) && pdfDictionary.Contains(PdfName.KIDS))
            {
                indirect = (PdfIndirectReference)((PdfArray)pdfDictionary.Get(PdfName.KIDS))[0];
            }

            return indirect.Number;
        }

        public static IList<Dictionary<string, object>> GetBookmark(PdfReader reader)
        {
            PdfObject pdfObjectRelease = PdfReader.GetPdfObjectRelease(reader.Catalog.Get(PdfName.OUTLINES));
            if (pdfObjectRelease == null || !pdfObjectRelease.IsDictionary())
            {
                return null;
            }

            PdfDictionary outline = (PdfDictionary)pdfObjectRelease;
            return GetBookmark(reader, outline, includeRoot: false);
        }

        public static IList<Dictionary<string, object>> GetBookmark(PdfReader reader, PdfDictionary outline, bool includeRoot)
        {
            if (outline == null)
            {
                return null;
            }

            IntHashtable intHashtable = new IntHashtable();
            int numberOfPages = reader.NumberOfPages;
            for (int i = 1; i <= numberOfPages; i++)
            {
                intHashtable[reader.GetPageOrigRef(i).Number] = i;
                reader.ReleasePage(i);
            }

            if (includeRoot)
            {
                return BookmarkDepth(reader, outline, intHashtable, processCurrentOutlineOnly: true);
            }

            return BookmarkDepth(reader, (PdfDictionary)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.FIRST)), intHashtable, processCurrentOutlineOnly: false);
        }

        public static void EliminatePages(IList<Dictionary<string, object>> list, int[] pageRange)
        {
            if (list == null)
            {
                return;
            }

            ListIterator<Dictionary<string, object>> listIterator = new ListIterator<Dictionary<string, object>>(list);
            while (listIterator.HasNext())
            {
                Dictionary<string, object> dictionary = listIterator.Next();
                bool flag = false;
                if (dictionary.ContainsKey("Action") && "GoTo".Equals(dictionary["Action"]))
                {
                    string text = null;
                    if (dictionary.ContainsKey("Page"))
                    {
                        text = (string)dictionary["Page"];
                    }

                    if (text != null)
                    {
                        text = text.Trim();
                        int num = text.IndexOf(' ');
                        int num2 = ((num >= 0) ? int.Parse(text.Substring(0, num)) : int.Parse(text));
                        int num3 = pageRange.Length & 0x7FFFFFFE;
                        for (int i = 0; i < num3; i += 2)
                        {
                            if (num2 >= pageRange[i] && num2 <= pageRange[i + 1])
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                }

                IList<Dictionary<string, object>> list2 = null;
                if (dictionary.ContainsKey("Kids"))
                {
                    list2 = (IList<Dictionary<string, object>>)dictionary["Kids"];
                }

                if (list2 != null)
                {
                    EliminatePages(list2, pageRange);
                    if (list2.Count == 0)
                    {
                        dictionary.Remove("Kids");
                        list2 = null;
                    }
                }

                if (flag)
                {
                    if (list2 == null)
                    {
                        listIterator.Remove();
                        continue;
                    }

                    dictionary.Remove("Action");
                    dictionary.Remove("Page");
                    dictionary.Remove("Named");
                }
            }
        }

        public static void ShiftPageNumbers(IList<Dictionary<string, object>> list, int pageShift, int[] pageRange)
        {
            if (list == null)
            {
                return;
            }

            foreach (Dictionary<string, object> item in list)
            {
                if (item.ContainsKey("Action") && "GoTo".Equals(item["Action"]))
                {
                    string text = null;
                    if (item.ContainsKey("Page"))
                    {
                        text = (string)item["Page"];
                    }

                    if (text != null)
                    {
                        text = text.Trim();
                        int num = text.IndexOf(' ');
                        int num2 = ((num >= 0) ? int.Parse(text.Substring(0, num)) : int.Parse(text));
                        bool flag = false;
                        if (pageRange == null)
                        {
                            flag = true;
                        }
                        else
                        {
                            int num3 = pageRange.Length & 0x7FFFFFFE;
                            for (int i = 0; i < num3; i += 2)
                            {
                                if (num2 >= pageRange[i] && num2 <= pageRange[i + 1])
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }

                        if (flag)
                        {
                            text = ((num >= 0) ? (num2 + pageShift + text.Substring(num)) : string.Concat(num2 + pageShift));
                        }

                        item["Page"] = text;
                    }
                }

                IList<Dictionary<string, object>> list2 = null;
                if (item.ContainsKey("Kids"))
                {
                    list2 = (IList<Dictionary<string, object>>)item["Kids"];
                }

                if (list2 != null)
                {
                    ShiftPageNumbers(list2, pageShift, pageRange);
                }
            }
        }

        public static string GetVal(Dictionary<string, object> map, string key)
        {
            map.TryGetValue(key, out var value);
            return (string)value;
        }

        internal static void CreateOutlineAction(PdfDictionary outline, Dictionary<string, object> map, PdfWriter writer, bool namedAsNames)
        {
            try
            {
                string val = GetVal(map, "Action");
                if ("GoTo".Equals(val))
                {
                    string val2;
                    if ((val2 = GetVal(map, "Named")) != null)
                    {
                        if (namedAsNames)
                        {
                            outline.Put(PdfName.DEST, new PdfName(val2));
                        }
                        else
                        {
                            outline.Put(PdfName.DEST, new PdfString(val2, null));
                        }
                    }
                    else
                    {
                        if ((val2 = GetVal(map, "Page")) == null)
                        {
                            return;
                        }

                        PdfArray pdfArray = new PdfArray();
                        StringTokenizer stringTokenizer = new StringTokenizer(val2);
                        int page = int.Parse(stringTokenizer.NextToken());
                        pdfArray.Add(writer.GetPageReference(page));
                        if (!stringTokenizer.HasMoreTokens())
                        {
                            pdfArray.Add(PdfName.XYZ);
                            pdfArray.Add(new float[3] { 0f, 10000f, 0f });
                        }
                        else
                        {
                            string text = stringTokenizer.NextToken();
                            if (text.StartsWith("/"))
                            {
                                text = text.Substring(1);
                            }

                            pdfArray.Add(new PdfName(text));
                            for (int i = 0; i < 4; i++)
                            {
                                if (!stringTokenizer.HasMoreTokens())
                                {
                                    break;
                                }

                                text = stringTokenizer.NextToken();
                                if (text.Equals("null"))
                                {
                                    pdfArray.Add(PdfNull.PDFNULL);
                                }
                                else
                                {
                                    pdfArray.Add(new PdfNumber(text));
                                }
                            }
                        }

                        outline.Put(PdfName.DEST, pdfArray);
                        return;
                    }
                }
                else
                {
                    if ("GoToR".Equals(val))
                    {
                        PdfDictionary pdfDictionary = new PdfDictionary();
                        string val3;
                        if ((val3 = GetVal(map, "Named")) != null)
                        {
                            pdfDictionary.Put(PdfName.D, new PdfString(val3, null));
                        }
                        else if ((val3 = GetVal(map, "NamedN")) != null)
                        {
                            pdfDictionary.Put(PdfName.D, new PdfName(val3));
                        }
                        else if ((val3 = GetVal(map, "Page")) != null)
                        {
                            PdfArray pdfArray2 = new PdfArray();
                            StringTokenizer stringTokenizer2 = new StringTokenizer(val3);
                            pdfArray2.Add(new PdfNumber(stringTokenizer2.NextToken()));
                            if (!stringTokenizer2.HasMoreTokens())
                            {
                                pdfArray2.Add(PdfName.XYZ);
                                pdfArray2.Add(new float[3] { 0f, 10000f, 0f });
                            }
                            else
                            {
                                string text2 = stringTokenizer2.NextToken();
                                if (text2.StartsWith("/"))
                                {
                                    text2 = text2.Substring(1);
                                }

                                pdfArray2.Add(new PdfName(text2));
                                for (int j = 0; j < 4; j++)
                                {
                                    if (!stringTokenizer2.HasMoreTokens())
                                    {
                                        break;
                                    }

                                    text2 = stringTokenizer2.NextToken();
                                    if (text2.Equals("null"))
                                    {
                                        pdfArray2.Add(PdfNull.PDFNULL);
                                    }
                                    else
                                    {
                                        pdfArray2.Add(new PdfNumber(text2));
                                    }
                                }
                            }

                            pdfDictionary.Put(PdfName.D, pdfArray2);
                        }

                        string val4 = GetVal(map, "File");
                        if (pdfDictionary.Size <= 0 || val4 == null)
                        {
                            return;
                        }

                        pdfDictionary.Put(PdfName.S, PdfName.GOTOR);
                        pdfDictionary.Put(PdfName.F, new PdfString(val4));
                        string val5 = GetVal(map, "NewWindow");
                        if (val5 != null)
                        {
                            if (val5.Equals("true"))
                            {
                                pdfDictionary.Put(PdfName.NEWWINDOW, PdfBoolean.PDFTRUE);
                            }
                            else if (val5.Equals("false"))
                            {
                                pdfDictionary.Put(PdfName.NEWWINDOW, PdfBoolean.PDFFALSE);
                            }
                        }

                        outline.Put(PdfName.A, pdfDictionary);
                        return;
                    }

                    if ("URI".Equals(val))
                    {
                        string val6 = GetVal(map, "URI");
                        if (val6 != null)
                        {
                            PdfDictionary pdfDictionary2 = new PdfDictionary();
                            pdfDictionary2.Put(PdfName.S, PdfName.URI);
                            pdfDictionary2.Put(PdfName.URI, new PdfString(val6));
                            outline.Put(PdfName.A, pdfDictionary2);
                        }
                    }
                    else if ("JS".Equals(val))
                    {
                        string val7 = GetVal(map, "Code");
                        if (val7 != null)
                        {
                            outline.Put(PdfName.A, PdfAction.JavaScript(val7, writer));
                        }
                    }
                    else if ("Launch".Equals(val))
                    {
                        string val8 = GetVal(map, "File");
                        if (val8 != null)
                        {
                            PdfDictionary pdfDictionary3 = new PdfDictionary();
                            pdfDictionary3.Put(PdfName.S, PdfName.LAUNCH);
                            pdfDictionary3.Put(PdfName.F, new PdfString(val8));
                            outline.Put(PdfName.A, pdfDictionary3);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public static object[] IterateOutlines(PdfWriter writer, PdfIndirectReference parent, IList<Dictionary<string, object>> kids, bool namedAsNames)
        {
            PdfIndirectReference[] array = new PdfIndirectReference[kids.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = writer.PdfIndirectReference;
            }

            int num = 0;
            int num2 = 0;
            foreach (Dictionary<string, object> kid in kids)
            {
                object[] array2 = null;
                IList<Dictionary<string, object>> list = null;
                if (kid.ContainsKey("Kids"))
                {
                    list = (IList<Dictionary<string, object>>)kid["Kids"];
                }

                if (list != null && list.Count > 0)
                {
                    array2 = IterateOutlines(writer, array[num], list, namedAsNames);
                }

                PdfDictionary pdfDictionary = new PdfDictionary();
                num2++;
                if (array2 != null)
                {
                    pdfDictionary.Put(PdfName.FIRST, (PdfIndirectReference)array2[0]);
                    pdfDictionary.Put(PdfName.LAST, (PdfIndirectReference)array2[1]);
                    int num3 = (int)array2[2];
                    if (kid.ContainsKey("Open") && "false".Equals(kid["Open"]))
                    {
                        pdfDictionary.Put(PdfName.COUNT, new PdfNumber(-num3));
                    }
                    else
                    {
                        pdfDictionary.Put(PdfName.COUNT, new PdfNumber(num3));
                        num2 += num3;
                    }
                }

                pdfDictionary.Put(PdfName.PARENT, parent);
                if (num > 0)
                {
                    pdfDictionary.Put(PdfName.PREV, array[num - 1]);
                }

                if (num < array.Length - 1)
                {
                    pdfDictionary.Put(PdfName.NEXT, array[num + 1]);
                }

                pdfDictionary.Put(PdfName.TITLE, new PdfString((string)kid["Title"], "UnicodeBig"));
                string text = null;
                if (kid.ContainsKey("Color"))
                {
                    text = (string)kid["Color"];
                }

                if (text != null)
                {
                    try
                    {
                        PdfArray pdfArray = new PdfArray();
                        StringTokenizer stringTokenizer = new StringTokenizer(text);
                        for (int j = 0; j < 3; j++)
                        {
                            float num4 = float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                            if (num4 < 0f)
                            {
                                num4 = 0f;
                            }

                            if (num4 > 1f)
                            {
                                num4 = 1f;
                            }

                            pdfArray.Add(new PdfNumber(num4));
                        }

                        pdfDictionary.Put(PdfName.C, pdfArray);
                    }
                    catch
                    {
                    }
                }

                string val = GetVal(kid, "Style");
                if (val != null)
                {
                    val = val.ToLower(CultureInfo.InvariantCulture);
                    int num5 = 0;
                    if (val.IndexOf("italic") >= 0)
                    {
                        num5 |= 1;
                    }

                    if (val.IndexOf("bold") >= 0)
                    {
                        num5 |= 2;
                    }

                    if (num5 != 0)
                    {
                        pdfDictionary.Put(PdfName.F, new PdfNumber(num5));
                    }
                }

                CreateOutlineAction(pdfDictionary, kid, writer, namedAsNames);
                writer.AddToBody(pdfDictionary, array[num]);
                num++;
            }

            return new object[3]
            {
                array[0],
                array[^1],
                num2
            };
        }

        public static void ExportToXMLNode(IList<Dictionary<string, object>> list, TextWriter outp, int indent, bool onlyASCII)
        {
            string text = "";
            if (indent != -1)
            {
                for (int i = 0; i < indent; i++)
                {
                    text += "  ";
                }
            }

            foreach (Dictionary<string, object> item in list)
            {
                string text2 = null;
                outp.Write(text);
                outp.Write("<Title ");
                IList<Dictionary<string, object>> list2 = null;
                foreach (KeyValuePair<string, object> item2 in item)
                {
                    string key = item2.Key;
                    if (key.Equals("Title"))
                    {
                        text2 = (string)item2.Value;
                        continue;
                    }

                    if (key.Equals("Kids"))
                    {
                        list2 = (IList<Dictionary<string, object>>)item2.Value;
                        continue;
                    }

                    outp.Write(key);
                    outp.Write("=\"");
                    string s = (string)item2.Value;
                    if (key.Equals("Named") || key.Equals("NamedN"))
                    {
                        s = EscapeBinaryString(s);
                    }

                    outp.Write(XMLUtil.EscapeXML(s, onlyASCII));
                    outp.Write("\" ");
                }

                outp.Write(">");
                if (text2 == null)
                {
                    text2 = "";
                }

                outp.Write(XMLUtil.EscapeXML(text2, onlyASCII));
                if (list2 != null)
                {
                    outp.Write("\n");
                    ExportToXMLNode(list2, outp, (indent == -1) ? indent : (indent + 1), onlyASCII);
                    outp.Write(text);
                }

                outp.Write("</Title>\n");
            }
        }

        public static void ExportToXML(IList<Dictionary<string, object>> list, Stream outp, string encoding, bool onlyASCII)
        {
            StreamWriter wrt = new StreamWriter(outp, IanaEncodings.GetEncodingEncoding(encoding));
            ExportToXML(list, wrt, encoding, onlyASCII);
        }

        public static void ExportToXML(IList<Dictionary<string, object>> list, TextWriter wrt, string encoding, bool onlyASCII)
        {
            wrt.Write("<?xml version=\"1.0\" encoding=\"");
            wrt.Write(XMLUtil.EscapeXML(encoding, onlyASCII));
            wrt.Write("\"?>\n<Bookmark>\n");
            ExportToXMLNode(list, wrt, 1, onlyASCII);
            wrt.Write("</Bookmark>\n");
            wrt.Flush();
        }

        public static IList<Dictionary<string, object>> ImportFromXML(Stream inp)
        {
            SimpleBookmark simpleBookmark = new SimpleBookmark();
            SimpleXMLParser.Parse(simpleBookmark, inp);
            return simpleBookmark.topList;
        }

        public static IList<Dictionary<string, object>> ImportFromXML(TextReader inp)
        {
            SimpleBookmark simpleBookmark = new SimpleBookmark();
            SimpleXMLParser.Parse(simpleBookmark, inp);
            return simpleBookmark.topList;
        }

        public static string EscapeBinaryString(string s)
        {
            StringBuilder stringBuilder = new StringBuilder();
            char[] array = s.ToCharArray();
            int num = array.Length;
            for (int i = 0; i < num; i++)
            {
                char c = array[i];
                if (c < ' ')
                {
                    stringBuilder.Append('\\');
                    int num2 = c;
                    string text = "";
                    do
                    {
                        text = num2 % 8 + text;
                        num2 /= 8;
                    }
                    while (num2 > 0);
                    stringBuilder.Append(text.PadLeft(3, '0'));
                }
                else if (c == '\\')
                {
                    stringBuilder.Append("\\\\");
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }

        public static string UnEscapeBinaryString(string s)
        {
            StringBuilder stringBuilder = new StringBuilder();
            char[] array = s.ToCharArray();
            int num = array.Length;
            for (int i = 0; i < num; i++)
            {
                char c = array[i];
                if (c == '\\')
                {
                    if (++i >= num)
                    {
                        stringBuilder.Append('\\');
                        break;
                    }

                    c = array[i];
                    if (c >= '0' && c <= '7')
                    {
                        int num2 = c - 48;
                        i++;
                        for (int j = 0; j < 2; j++)
                        {
                            if (i >= num)
                            {
                                break;
                            }

                            c = array[i];
                            if (c < '0' || c > '7')
                            {
                                break;
                            }

                            i++;
                            num2 = num2 * 8 + c - 48;
                        }

                        i--;
                        stringBuilder.Append((char)num2);
                    }
                    else
                    {
                        stringBuilder.Append(c);
                    }
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }

        public void EndDocument()
        {
        }

        public void EndElement(string tag)
        {
            if (tag.Equals("Bookmark"))
            {
                if (attr.Count != 0)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("bookmark.end.tag.out.of.place"));
                }

                return;
            }

            if (!tag.Equals("Title"))
            {
                throw new Exception(MessageLocalization.GetComposedMessage("invalid.end.tag.1", tag));
            }

            Dictionary<string, object> dictionary = attr.Pop();
            string text = (string)dictionary["Title"];
            dictionary["Title"] = text.Trim();
            string val = GetVal(dictionary, "Named");
            if (val != null)
            {
                dictionary["Named"] = UnEscapeBinaryString(val);
            }

            val = GetVal(dictionary, "NamedN");
            if (val != null)
            {
                dictionary["NamedN"] = UnEscapeBinaryString(val);
            }

            if (attr.Count == 0)
            {
                topList.Add(dictionary);
                return;
            }

            Dictionary<string, object> dictionary2 = attr.Peek();
            IList<Dictionary<string, object>> list = null;
            if (dictionary2.ContainsKey("Kids"))
            {
                list = (IList<Dictionary<string, object>>)dictionary2["Kids"];
            }

            if (list == null)
            {
                list = (IList<Dictionary<string, object>>)(dictionary2["Kids"] = new List<Dictionary<string, object>>());
            }

            list.Add(dictionary);
        }

        public void StartDocument()
        {
        }

        public void StartElement(string tag, IDictionary<string, string> h)
        {
            if (topList == null)
            {
                if (tag.Equals("Bookmark"))
                {
                    topList = new List<Dictionary<string, object>>();
                    return;
                }

                throw new Exception(MessageLocalization.GetComposedMessage("root.element.is.not.bookmark.1", tag));
            }

            if (!tag.Equals("Title"))
            {
                throw new Exception(MessageLocalization.GetComposedMessage("tag.1.not.allowed", tag));
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (KeyValuePair<string, string> item in h)
            {
                dictionary[item.Key] = item.Value;
            }

            dictionary["Title"] = "";
            dictionary.Remove("Kids");
            attr.Push(dictionary);
        }

        public void Text(string str)
        {
            if (attr.Count != 0)
            {
                Dictionary<string, object> dictionary = attr.Peek();
                string text = (string)dictionary["Title"];
                text = (string)(dictionary["Title"] = text + str);
            }
        }
    }
}
