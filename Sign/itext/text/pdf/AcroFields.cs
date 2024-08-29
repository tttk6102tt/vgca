using Sign.itext.error_messages;
using Sign.itext.io;
using Sign.itext.pdf;
using Sign.itext.pdf.security;
using Sign.itext.xml;
using Sign.SystemItext.util;
using Sign.SystemItext.util.collections;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Sign.itext.text.pdf
{
    public class AcroFields
    {
        public class Item
        {
            public const int WRITE_MERGED = 1;

            public const int WRITE_WIDGET = 2;

            public const int WRITE_VALUE = 4;

            protected internal List<PdfDictionary> values = new List<PdfDictionary>();

            protected internal List<PdfDictionary> widgets = new List<PdfDictionary>();

            protected internal List<PdfIndirectReference> widget_refs = new List<PdfIndirectReference>();

            protected internal List<PdfDictionary> merged = new List<PdfDictionary>();

            protected internal List<int> page = new List<int>();

            protected internal List<int> tabOrder = new List<int>();

            public virtual int Size => values.Count;

            public virtual void WriteToAll(PdfName key, PdfObject value, int writeFlags)
            {
                if (((uint)writeFlags & (true ? 1u : 0u)) != 0)
                {
                    for (int i = 0; i < merged.Count; i++)
                    {
                        GetMerged(i).Put(key, value);
                    }
                }

                if (((uint)writeFlags & 2u) != 0)
                {
                    for (int i = 0; i < widgets.Count; i++)
                    {
                        GetWidget(i).Put(key, value);
                    }
                }

                if (((uint)writeFlags & 4u) != 0)
                {
                    for (int i = 0; i < values.Count; i++)
                    {
                        GetValue(i).Put(key, value);
                    }
                }
            }

            public virtual void MarkUsed(AcroFields parentFields, int writeFlags)
            {
                if (((uint)writeFlags & 4u) != 0)
                {
                    for (int i = 0; i < Size; i++)
                    {
                        parentFields.MarkUsed(GetValue(i));
                    }
                }

                if (((uint)writeFlags & 2u) != 0)
                {
                    for (int j = 0; j < Size; j++)
                    {
                        parentFields.MarkUsed(GetWidget(j));
                    }
                }
            }

            internal void Remove(int killIdx)
            {
                values.RemoveAt(killIdx);
                widgets.RemoveAt(killIdx);
                widget_refs.RemoveAt(killIdx);
                merged.RemoveAt(killIdx);
                page.RemoveAt(killIdx);
                tabOrder.RemoveAt(killIdx);
            }

            public virtual PdfDictionary GetValue(int idx)
            {
                return values[idx];
            }

            internal void AddValue(PdfDictionary value)
            {
                values.Add(value);
            }

            public virtual PdfDictionary GetWidget(int idx)
            {
                return widgets[idx];
            }

            internal void AddWidget(PdfDictionary widget)
            {
                widgets.Add(widget);
            }

            public virtual PdfIndirectReference GetWidgetRef(int idx)
            {
                return widget_refs[idx];
            }

            internal void AddWidgetRef(PdfIndirectReference widgRef)
            {
                widget_refs.Add(widgRef);
            }

            public virtual PdfDictionary GetMerged(int idx)
            {
                return merged[idx];
            }

            internal void AddMerged(PdfDictionary mergeDict)
            {
                merged.Add(mergeDict);
            }

            public virtual int GetPage(int idx)
            {
                return page[idx];
            }

            internal void AddPage(int pg)
            {
                page.Add(pg);
            }

            internal void ForcePage(int idx, int pg)
            {
                page[idx] = pg;
            }

            public virtual int GetTabOrder(int idx)
            {
                return tabOrder[idx];
            }

            internal void AddTabOrder(int order)
            {
                tabOrder.Add(order);
            }
        }

        private class InstHit
        {
            private IntHashtable hits;

            public InstHit(int[] inst)
            {
                if (inst != null)
                {
                    hits = new IntHashtable();
                    for (int i = 0; i < inst.Length; i++)
                    {
                        hits[inst[i]] = 1;
                    }
                }
            }

            public virtual bool IsHit(int n)
            {
                if (hits == null)
                {
                    return true;
                }

                return hits.ContainsKey(n);
            }
        }

        private class ISorterComparator : IComparer<object[]>
        {
            public virtual int Compare(object[] o1, object[] o2)
            {
                int num = ((int[])o1[1])[0];
                int num2 = ((int[])o2[1])[0];
                return num - num2;
            }
        }

        public class FieldPosition
        {
            public int page;

            public Rectangle position;
        }

        internal PdfReader reader;

        internal PdfWriter writer;

        internal IDictionary<string, Item> fields;

        private int topFirst;

        private Dictionary<string, int[]> sigNames;

        private bool append;

        public const int DA_FONT = 0;

        public const int DA_SIZE = 1;

        public const int DA_COLOR = 2;

        private Dictionary<int, BaseFont> extensionFonts = new Dictionary<int, BaseFont>();

        private XfaForm xfa;

        public const int FIELD_TYPE_NONE = 0;

        public const int FIELD_TYPE_PUSHBUTTON = 1;

        public const int FIELD_TYPE_CHECKBOX = 2;

        public const int FIELD_TYPE_RADIOBUTTON = 3;

        public const int FIELD_TYPE_TEXT = 4;

        public const int FIELD_TYPE_LIST = 5;

        public const int FIELD_TYPE_COMBO = 6;

        public const int FIELD_TYPE_SIGNATURE = 7;

        private bool lastWasString;

        private bool generateAppearances = true;

        private Dictionary<string, BaseFont> localFonts = new Dictionary<string, BaseFont>();

        private float extraMarginLeft;

        private float extraMarginTop;

        private List<BaseFont> substitutionFonts;

        private List<string> orderedSignatureNames;

        private static Dictionary<string, string[]> stdFieldFontNames;

        private IDictionary<string, TextField> fieldCache;

        private int totalRevisions;

        private static readonly PdfName[] buttonRemove;

        public virtual IDictionary<string, Item> Fields => fields;

        public virtual bool GenerateAppearances
        {
            get
            {
                return generateAppearances;
            }
            set
            {
                generateAppearances = value;
                PdfDictionary asDict = reader.Catalog.GetAsDict(PdfName.ACROFORM);
                if (generateAppearances)
                {
                    asDict.Remove(PdfName.NEEDAPPEARANCES);
                }
                else
                {
                    asDict.Put(PdfName.NEEDAPPEARANCES, PdfBoolean.PDFTRUE);
                }
            }
        }

        public virtual int TotalRevisions
        {
            get
            {
                FindSignatureNames();
                return totalRevisions;
            }
        }

        public virtual IDictionary<string, TextField> FieldCache
        {
            get
            {
                return fieldCache;
            }
            set
            {
                fieldCache = value;
            }
        }

        public virtual List<BaseFont> SubstitutionFonts
        {
            get
            {
                return substitutionFonts;
            }
            set
            {
                substitutionFonts = value;
            }
        }

        public virtual XfaForm Xfa => xfa;

        internal AcroFields(PdfReader reader, PdfWriter writer)
        {
            this.reader = reader;
            this.writer = writer;
            xfa = new XfaForm(reader);
            if (writer is PdfStamperImp)
            {
                append = ((PdfStamperImp)writer).append;
            }

            Fill();
        }

        internal void Fill()
        {
            fields = new LinkedDictionary<string, Item>();
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObjectRelease(reader.Catalog.Get(PdfName.ACROFORM));
            if (pdfDictionary == null)
            {
                return;
            }

            PdfBoolean asBoolean = pdfDictionary.GetAsBoolean(PdfName.NEEDAPPEARANCES);
            if (asBoolean == null || !asBoolean.BooleanValue)
            {
                GenerateAppearances = true;
            }
            else
            {
                GenerateAppearances = false;
            }

            PdfArray pdfArray = (PdfArray)PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.FIELDS));
            if (pdfArray == null || pdfArray.Size == 0)
            {
                return;
            }

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                PdfDictionary pageNRelease = reader.GetPageNRelease(i);
                PdfArray pdfArray2 = (PdfArray)PdfReader.GetPdfObjectRelease(pageNRelease.Get(PdfName.ANNOTS), pageNRelease);
                if (pdfArray2 == null)
                {
                    continue;
                }

                for (int j = 0; j < pdfArray2.Size; j++)
                {
                    PdfDictionary asDict = pdfArray2.GetAsDict(j);
                    if (asDict == null)
                    {
                        PdfReader.ReleaseLastXrefPartial(pdfArray2.GetAsIndirectObject(j));
                        continue;
                    }

                    if (!PdfName.WIDGET.Equals(asDict.GetAsName(PdfName.SUBTYPE)))
                    {
                        PdfReader.ReleaseLastXrefPartial(pdfArray2.GetAsIndirectObject(j));
                        continue;
                    }

                    PdfDictionary pdfDictionary2 = asDict;
                    PdfDictionary pdfDictionary3 = new PdfDictionary();
                    pdfDictionary3.Merge(asDict);
                    string text = "";
                    PdfDictionary pdfDictionary4 = null;
                    PdfObject pdfObject = null;
                    while (asDict != null)
                    {
                        pdfDictionary3.MergeDifferent(asDict);
                        PdfString asString = asDict.GetAsString(PdfName.T);
                        if (asString != null)
                        {
                            text = asString.ToUnicodeString() + "." + text;
                        }

                        if (pdfObject == null && asDict.Get(PdfName.V) != null)
                        {
                            pdfObject = PdfReader.GetPdfObjectRelease(asDict.Get(PdfName.V));
                        }

                        if (pdfDictionary4 == null && asString != null)
                        {
                            pdfDictionary4 = asDict;
                            if (asDict.Get(PdfName.V) == null && pdfObject != null)
                            {
                                pdfDictionary4.Put(PdfName.V, pdfObject);
                            }
                        }

                        asDict = asDict.GetAsDict(PdfName.PARENT);
                    }

                    if (text.Length > 0)
                    {
                        text = text.Substring(0, text.Length - 1);
                    }

                    if (!fields.TryGetValue(text, out var value))
                    {
                        value = new Item();
                        fields[text] = value;
                    }

                    if (pdfDictionary4 == null)
                    {
                        value.AddValue(pdfDictionary2);
                    }
                    else
                    {
                        value.AddValue(pdfDictionary4);
                    }

                    value.AddWidget(pdfDictionary2);
                    value.AddWidgetRef(pdfArray2.GetAsIndirectObject(j));
                    if (pdfDictionary != null)
                    {
                        pdfDictionary3.MergeDifferent(pdfDictionary);
                    }

                    value.AddMerged(pdfDictionary3);
                    value.AddPage(i);
                    value.AddTabOrder(j);
                }
            }

            PdfNumber asNumber = pdfDictionary.GetAsNumber(PdfName.SIGFLAGS);
            if (asNumber == null || (asNumber.IntValue & 1) != 1)
            {
                return;
            }

            for (int k = 0; k < pdfArray.Size; k++)
            {
                PdfDictionary asDict2 = pdfArray.GetAsDict(k);
                if (asDict2 == null)
                {
                    PdfReader.ReleaseLastXrefPartial(pdfArray.GetAsIndirectObject(k));
                }
                else if (!PdfName.WIDGET.Equals(asDict2.GetAsName(PdfName.SUBTYPE)))
                {
                    PdfReader.ReleaseLastXrefPartial(pdfArray.GetAsIndirectObject(k));
                }
                else
                {
                    if ((PdfArray)PdfReader.GetPdfObjectRelease(asDict2.Get(PdfName.KIDS)) != null)
                    {
                        continue;
                    }

                    PdfDictionary pdfDictionary5 = new PdfDictionary();
                    pdfDictionary5.Merge(asDict2);
                    PdfString asString2 = asDict2.GetAsString(PdfName.T);
                    if (asString2 != null)
                    {
                        string key = asString2.ToUnicodeString();
                        if (!fields.ContainsKey(key))
                        {
                            Item item = new Item();
                            fields[key] = item;
                            item.AddValue(pdfDictionary5);
                            item.AddWidget(pdfDictionary5);
                            item.AddWidgetRef(pdfArray.GetAsIndirectObject(k));
                            item.AddMerged(pdfDictionary5);
                            item.AddPage(-1);
                            item.AddTabOrder(-1);
                        }
                    }
                }
            }
        }

        public virtual string[] GetAppearanceStates(string fieldName)
        {
            if (!fields.ContainsKey(fieldName))
            {
                return null;
            }

            Item item = fields[fieldName];
            HashSet2<string> hashSet = new HashSet2<string>();
            PdfDictionary value = item.GetValue(0);
            PdfString asString = value.GetAsString(PdfName.OPT);
            if (asString != null)
            {
                hashSet.Add(asString.ToUnicodeString());
            }
            else
            {
                PdfArray asArray = value.GetAsArray(PdfName.OPT);
                if (asArray != null)
                {
                    for (int i = 0; i < asArray.Size; i++)
                    {
                        PdfObject directObject = asArray.GetDirectObject(i);
                        PdfString pdfString = null;
                        switch (directObject.Type)
                        {
                            case 5:
                                pdfString = ((PdfArray)directObject).GetAsString(1);
                                break;
                            case 3:
                                pdfString = (PdfString)directObject;
                                break;
                        }

                        if (pdfString != null)
                        {
                            hashSet.Add(pdfString.ToUnicodeString());
                        }
                    }
                }
            }

            for (int j = 0; j < item.Size; j++)
            {
                PdfDictionary widget = item.GetWidget(j);
                widget = widget.GetAsDict(PdfName.AP);
                if (widget == null)
                {
                    continue;
                }

                widget = widget.GetAsDict(PdfName.N);
                if (widget == null)
                {
                    continue;
                }

                foreach (PdfName key in widget.Keys)
                {
                    string item2 = PdfName.DecodeName(key.ToString());
                    hashSet.Add(item2);
                }
            }

            string[] array = new string[hashSet.Count];
            hashSet.CopyTo(array, 0);
            return array;
        }

        private string[] GetListOption(string fieldName, int idx)
        {
            Item fieldItem = GetFieldItem(fieldName);
            if (fieldItem == null)
            {
                return null;
            }

            PdfArray asArray = fieldItem.GetMerged(0).GetAsArray(PdfName.OPT);
            if (asArray == null)
            {
                return null;
            }

            string[] array = new string[asArray.Size];
            for (int i = 0; i < asArray.Size; i++)
            {
                PdfObject directObject = asArray.GetDirectObject(i);
                try
                {
                    if (directObject.IsArray())
                    {
                        directObject = ((PdfArray)directObject).GetDirectObject(idx);
                    }

                    if (directObject.IsString())
                    {
                        array[i] = ((PdfString)directObject).ToUnicodeString();
                    }
                    else
                    {
                        array[i] = directObject.ToString();
                    }
                }
                catch
                {
                    array[i] = "";
                }
            }

            return array;
        }

        public virtual string[] GetListOptionExport(string fieldName)
        {
            return GetListOption(fieldName, 0);
        }

        public virtual string[] GetListOptionDisplay(string fieldName)
        {
            return GetListOption(fieldName, 1);
        }

        public virtual bool SetListOption(string fieldName, string[] exportValues, string[] displayValues)
        {
            if (exportValues == null && displayValues == null)
            {
                return false;
            }

            if (exportValues != null && displayValues != null && exportValues.Length != displayValues.Length)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.export.and.the.display.array.must.have.the.same.size"));
            }

            int fieldType = GetFieldType(fieldName);
            if (fieldType != 6 && fieldType != 5)
            {
                return false;
            }

            Item item = fields[fieldName];
            string[] array = null;
            if (exportValues == null && displayValues != null)
            {
                array = displayValues;
            }
            else if (exportValues != null && displayValues == null)
            {
                array = exportValues;
            }

            PdfArray pdfArray = new PdfArray();
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    pdfArray.Add(new PdfString(array[i], "UnicodeBig"));
                }
            }
            else
            {
                for (int j = 0; j < exportValues.Length; j++)
                {
                    PdfArray pdfArray2 = new PdfArray();
                    pdfArray2.Add(new PdfString(exportValues[j], "UnicodeBig"));
                    pdfArray2.Add(new PdfString(displayValues[j], "UnicodeBig"));
                    pdfArray.Add(pdfArray2);
                }
            }

            item.WriteToAll(PdfName.OPT, pdfArray, 5);
            return true;
        }

        public virtual int GetFieldType(string fieldName)
        {
            Item fieldItem = GetFieldItem(fieldName);
            if (fieldItem == null)
            {
                return 0;
            }

            PdfDictionary merged = fieldItem.GetMerged(0);
            PdfName asName = merged.GetAsName(PdfName.FT);
            if (asName == null)
            {
                return 0;
            }

            int num = 0;
            PdfNumber asNumber = merged.GetAsNumber(PdfName.FF);
            if (asNumber != null)
            {
                num = asNumber.IntValue;
            }

            if (PdfName.BTN.Equals(asName))
            {
                if (((uint)num & 0x10000u) != 0)
                {
                    return 1;
                }

                if (((uint)num & 0x8000u) != 0)
                {
                    return 3;
                }

                return 2;
            }

            if (PdfName.TX.Equals(asName))
            {
                return 4;
            }

            if (PdfName.CH.Equals(asName))
            {
                if (((uint)num & 0x20000u) != 0)
                {
                    return 6;
                }

                return 5;
            }

            if (PdfName.SIG.Equals(asName))
            {
                return 7;
            }

            return 0;
        }

        public virtual void ExportAsFdf(FdfWriter writer)
        {
            foreach (KeyValuePair<string, Item> field2 in fields)
            {
                Item value = field2.Value;
                string key = field2.Key;
                if (value.GetMerged(0).Get(PdfName.V) != null)
                {
                    string field = GetField(key);
                    if (lastWasString)
                    {
                        writer.SetFieldAsString(key, field);
                    }
                    else
                    {
                        writer.SetFieldAsName(key, field);
                    }
                }
            }
        }

        public virtual bool RenameField(string oldName, string newName)
        {
            int num = oldName.LastIndexOf('.') + 1;
            int num2 = newName.LastIndexOf('.') + 1;
            if (num != num2)
            {
                return false;
            }

            if (!oldName.Substring(0, num).Equals(newName.Substring(0, num2)))
            {
                return false;
            }

            if (fields.ContainsKey(newName))
            {
                return false;
            }

            if (!fields.ContainsKey(oldName))
            {
                return false;
            }

            Item item = fields[oldName];
            newName = newName.Substring(num2);
            PdfString value = new PdfString(newName, "UnicodeBig");
            item.WriteToAll(PdfName.T, value, 5);
            item.MarkUsed(this, 4);
            fields.Remove(oldName);
            fields[newName] = item;
            return true;
        }

        public static object[] SplitDAelements(string da)
        {
            PRTokeniser pRTokeniser = new PRTokeniser(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(PdfEncodings.ConvertToBytes(da, null))));
            List<string> list = new List<string>();
            object[] array = new object[3];
            while (pRTokeniser.NextToken())
            {
                if (pRTokeniser.TokenType == PRTokeniser.TokType.COMMENT)
                {
                    continue;
                }

                if (pRTokeniser.TokenType == PRTokeniser.TokType.OTHER)
                {
                    string stringValue = pRTokeniser.StringValue;
                    if (stringValue.Equals("Tf"))
                    {
                        if (list.Count >= 2)
                        {
                            array[0] = list[list.Count - 2];
                            array[1] = float.Parse(list[list.Count - 1], NumberFormatInfo.InvariantInfo);
                        }
                    }
                    else if (stringValue.Equals("g"))
                    {
                        if (list.Count >= 1)
                        {
                            float num = float.Parse(list[list.Count - 1], NumberFormatInfo.InvariantInfo);
                            if (num != 0f)
                            {
                                array[2] = new GrayColor(num);
                            }
                        }
                    }
                    else if (stringValue.Equals("rg"))
                    {
                        if (list.Count >= 3)
                        {
                            float red = float.Parse(list[list.Count - 3], NumberFormatInfo.InvariantInfo);
                            float green = float.Parse(list[list.Count - 2], NumberFormatInfo.InvariantInfo);
                            float blue = float.Parse(list[list.Count - 1], NumberFormatInfo.InvariantInfo);
                            array[2] = new BaseColor(red, green, blue);
                        }
                    }
                    else if (stringValue.Equals("k") && list.Count >= 4)
                    {
                        float floatCyan = float.Parse(list[list.Count - 4], NumberFormatInfo.InvariantInfo);
                        float floatMagenta = float.Parse(list[list.Count - 3], NumberFormatInfo.InvariantInfo);
                        float floatYellow = float.Parse(list[list.Count - 2], NumberFormatInfo.InvariantInfo);
                        float floatBlack = float.Parse(list[list.Count - 1], NumberFormatInfo.InvariantInfo);
                        array[2] = new CMYKColor(floatCyan, floatMagenta, floatYellow, floatBlack);
                    }

                    list.Clear();
                }
                else
                {
                    list.Add(pRTokeniser.StringValue);
                }
            }

            return array;
        }

        public virtual void DecodeGenericDictionary(PdfDictionary merged, BaseField tx)
        {
            int num = 0;
            PdfString asString = merged.GetAsString(PdfName.DA);
            if (asString != null)
            {
                object[] array = SplitDAelements(asString.ToUnicodeString());
                if (array[1] != null)
                {
                    tx.FontSize = (float)array[1];
                }

                if (array[2] != null)
                {
                    tx.TextColor = (BaseColor)array[2];
                }

                if (array[0] != null)
                {
                    PdfDictionary asDict = merged.GetAsDict(PdfName.DR);
                    if (asDict != null)
                    {
                        PdfDictionary asDict2 = asDict.GetAsDict(PdfName.FONT);
                        if (asDict2 != null)
                        {
                            PdfObject pdfObject = asDict2.Get(new PdfName((string)array[0]));
                            BaseFont value2;
                            if (pdfObject != null && pdfObject.Type == 10)
                            {
                                PRIndirectReference obj = (PRIndirectReference)pdfObject;
                                BaseFont baseFont2 = (tx.Font = new DocumentFont((PRIndirectReference)pdfObject, asDict.GetAsDict(PdfName.ENCODING)));
                                int number = obj.Number;
                                extensionFonts.TryGetValue(number, out var value);
                                if (value == null && !extensionFonts.ContainsKey(number))
                                {
                                    PdfDictionary asDict3 = ((PdfDictionary)PdfReader.GetPdfObject(pdfObject)).GetAsDict(PdfName.FONTDESCRIPTOR);
                                    if (asDict3 != null)
                                    {
                                        PRStream pRStream = (PRStream)PdfReader.GetPdfObject(asDict3.Get(PdfName.FONTFILE2));
                                        if (pRStream == null)
                                        {
                                            pRStream = (PRStream)PdfReader.GetPdfObject(asDict3.Get(PdfName.FONTFILE3));
                                        }

                                        if (pRStream == null)
                                        {
                                            extensionFonts[number] = null;
                                        }
                                        else
                                        {
                                            try
                                            {
                                                value = BaseFont.CreateFont("font.ttf", "Identity-H", embedded: true, cached: false, PdfReader.GetStreamBytes(pRStream), null);
                                            }
                                            catch
                                            {
                                            }

                                            extensionFonts[number] = value;
                                        }
                                    }
                                }

                                if (tx is TextField)
                                {
                                    ((TextField)tx).ExtensionFont = value;
                                }
                            }
                            else if (!localFonts.TryGetValue((string)array[0], out value2))
                            {
                                stdFieldFontNames.TryGetValue((string)array[0], out var value3);
                                if (value3 != null)
                                {
                                    try
                                    {
                                        string encoding = "winansi";
                                        if (value3.Length > 1)
                                        {
                                            encoding = value3[1];
                                        }

                                        value2 = (tx.Font = BaseFont.CreateFont(value3[0], encoding, embedded: false));
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                            else
                            {
                                tx.Font = value2;
                            }
                        }
                    }
                }
            }

            PdfDictionary asDict4 = merged.GetAsDict(PdfName.MK);
            if (asDict4 != null)
            {
                PdfArray asArray = asDict4.GetAsArray(PdfName.BC);
                BaseColor baseColor = (tx.BorderColor = GetMKColor(asArray));
                if (baseColor != null)
                {
                    tx.BorderWidth = 1f;
                }

                asArray = asDict4.GetAsArray(PdfName.BG);
                tx.BackgroundColor = GetMKColor(asArray);
                PdfNumber asNumber = asDict4.GetAsNumber(PdfName.R);
                if (asNumber != null)
                {
                    tx.Rotation = asNumber.IntValue;
                }
            }

            PdfNumber asNumber2 = merged.GetAsNumber(PdfName.F);
            num = 0;
            tx.Visibility = 2;
            if (asNumber2 != null)
            {
                num = asNumber2.IntValue;
                if (((uint)num & 4u) != 0 && ((uint)num & 2u) != 0)
                {
                    tx.Visibility = 1;
                }
                else if (((uint)num & 4u) != 0 && ((uint)num & 0x20u) != 0)
                {
                    tx.Visibility = 3;
                }
                else if (((uint)num & 4u) != 0)
                {
                    tx.Visibility = 0;
                }
            }

            asNumber2 = merged.GetAsNumber(PdfName.FF);
            num = 0;
            if (asNumber2 != null)
            {
                num = asNumber2.IntValue;
            }

            tx.Options = num;
            if (((uint)num & 0x1000000u) != 0)
            {
                PdfNumber asNumber3 = merged.GetAsNumber(PdfName.MAXLEN);
                int maxCharacterLength = 0;
                if (asNumber3 != null)
                {
                    maxCharacterLength = asNumber3.IntValue;
                }

                tx.MaxCharacterLength = maxCharacterLength;
            }

            asNumber2 = merged.GetAsNumber(PdfName.Q);
            if (asNumber2 != null)
            {
                if (asNumber2.IntValue == 1)
                {
                    tx.Alignment = 1;
                }
                else if (asNumber2.IntValue == 2)
                {
                    tx.Alignment = 2;
                }
            }

            PdfDictionary asDict5 = merged.GetAsDict(PdfName.BS);
            if (asDict5 != null)
            {
                PdfNumber asNumber4 = asDict5.GetAsNumber(PdfName.W);
                if (asNumber4 != null)
                {
                    tx.BorderWidth = asNumber4.FloatValue;
                }

                PdfName asName = asDict5.GetAsName(PdfName.S);
                if (PdfName.D.Equals(asName))
                {
                    tx.BorderStyle = 1;
                }
                else if (PdfName.B.Equals(asName))
                {
                    tx.BorderStyle = 2;
                }
                else if (PdfName.I.Equals(asName))
                {
                    tx.BorderStyle = 3;
                }
                else if (PdfName.U.Equals(asName))
                {
                    tx.BorderStyle = 4;
                }

                return;
            }

            PdfArray asArray2 = merged.GetAsArray(PdfName.BORDER);
            if (asArray2 != null)
            {
                if (asArray2.Size >= 3)
                {
                    tx.BorderWidth = asArray2.GetAsNumber(2).FloatValue;
                }

                if (asArray2.Size >= 4)
                {
                    tx.BorderStyle = 1;
                }
            }
        }

        internal PdfAppearance GetAppearance(PdfDictionary merged, string[] values, string fieldName)
        {
            PdfName asName = merged.GetAsName(PdfName.FT);
            if (PdfName.BTN.Equals(asName))
            {
                RadioCheckField radioCheckField = new RadioCheckField(writer, null, null, null);
                DecodeGenericDictionary(merged, radioCheckField);
                Rectangle rectangle = PdfReader.GetNormalizedRectangle(merged.GetAsArray(PdfName.RECT));
                if (radioCheckField.Rotation == 90 || radioCheckField.Rotation == 270)
                {
                    rectangle = rectangle.Rotate();
                }

                radioCheckField.Box = rectangle;
                radioCheckField.CheckType = 3;
                return radioCheckField.GetAppearance(isRadio: false, !merged.GetAsName(PdfName.AS).Equals(PdfName.Off_));
            }

            topFirst = 0;
            string text = ((values.Length != 0) ? values[0] : null);
            TextField textField = null;
            if (fieldCache == null || !fieldCache.ContainsKey(fieldName))
            {
                textField = new TextField(writer, null, null);
                textField.SetExtraMargin(extraMarginLeft, extraMarginTop);
                textField.BorderWidth = 0f;
                textField.SubstitutionFonts = substitutionFonts;
                DecodeGenericDictionary(merged, textField);
                Rectangle rectangle2 = PdfReader.GetNormalizedRectangle(merged.GetAsArray(PdfName.RECT));
                if (textField.Rotation == 90 || textField.Rotation == 270)
                {
                    rectangle2 = rectangle2.Rotate();
                }

                textField.Box = rectangle2;
                if (fieldCache != null)
                {
                    fieldCache[fieldName] = textField;
                }
            }
            else
            {
                textField = fieldCache[fieldName];
                textField.Writer = writer;
            }

            if (PdfName.TX.Equals(asName))
            {
                if (values.Length != 0 && values[0] != null)
                {
                    textField.Text = values[0];
                }

                return textField.GetAppearance();
            }

            if (!PdfName.CH.Equals(asName))
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("an.appearance.was.requested.without.a.variable.text.field"));
            }

            PdfArray asArray = merged.GetAsArray(PdfName.OPT);
            int num = 0;
            PdfNumber asNumber = merged.GetAsNumber(PdfName.FF);
            if (asNumber != null)
            {
                num = asNumber.IntValue;
            }

            if (((uint)num & 0x20000u) != 0 && asArray == null)
            {
                textField.Text = text;
                return textField.GetAppearance();
            }

            if (asArray != null)
            {
                string[] array = new string[asArray.Size];
                string[] array2 = new string[asArray.Size];
                for (int i = 0; i < asArray.Size; i++)
                {
                    PdfObject pdfObject = asArray.GetPdfObject(i);
                    if (pdfObject.IsString())
                    {
                        array[i] = (array2[i] = ((PdfString)pdfObject).ToUnicodeString());
                        continue;
                    }

                    PdfArray pdfArray = (PdfArray)pdfObject;
                    array2[i] = pdfArray.GetAsString(0).ToUnicodeString();
                    array[i] = pdfArray.GetAsString(1).ToUnicodeString();
                }

                if (((uint)num & 0x20000u) != 0)
                {
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (text.Equals(array2[j]))
                        {
                            text = array[j];
                            break;
                        }
                    }

                    textField.Text = text;
                    return textField.GetAppearance();
                }

                List<int> list = new List<int>();
                for (int k = 0; k < array2.Length; k++)
                {
                    foreach (string text2 in values)
                    {
                        if (text2 != null && text2.Equals(array2[k]))
                        {
                            list.Add(k);
                            break;
                        }
                    }
                }

                textField.Choices = array;
                textField.ChoiceExports = array2;
                textField.ChoiceSelections = list;
            }

            PdfAppearance listAppearance = textField.GetListAppearance();
            topFirst = textField.TopFirst;
            return listAppearance;
        }

        internal PdfAppearance GetAppearance(PdfDictionary merged, string text, string fieldName)
        {
            string[] values = new string[1] { text };
            return GetAppearance(merged, values, fieldName);
        }

        internal BaseColor GetMKColor(PdfArray ar)
        {
            if (ar == null)
            {
                return null;
            }

            return ar.Size switch
            {
                1 => new GrayColor(ar.GetAsNumber(0).FloatValue),
                3 => new BaseColor(ExtendedColor.Normalize(ar.GetAsNumber(0).FloatValue), ExtendedColor.Normalize(ar.GetAsNumber(1).FloatValue), ExtendedColor.Normalize(ar.GetAsNumber(2).FloatValue)),
                4 => new CMYKColor(ar.GetAsNumber(0).FloatValue, ar.GetAsNumber(1).FloatValue, ar.GetAsNumber(2).FloatValue, ar.GetAsNumber(3).FloatValue),
                _ => null,
            };
        }

        public virtual string GetFieldRichValue(string name)
        {
            if (xfa.XfaPresent)
            {
                return null;
            }

            fields.TryGetValue(name, out var value);
            if (value == null)
            {
                return null;
            }

            PdfString asString = value.GetMerged(0).GetAsString(PdfName.RV);
            string result = null;
            if (asString != null)
            {
                result = asString.ToString();
            }

            return result;
        }

        public virtual string GetField(string name)
        {
            if (xfa.XfaPresent)
            {
                name = xfa.FindFieldName(name, this);
                if (name == null)
                {
                    return null;
                }

                name = XfaForm.Xml2Som.GetShortName(name);
                return XfaForm.GetNodeText(xfa.FindDatasetsNode(name));
            }

            if (!fields.ContainsKey(name))
            {
                return null;
            }

            Item item = fields[name];
            lastWasString = false;
            PdfDictionary merged = item.GetMerged(0);
            PdfObject pdfObject = PdfReader.GetPdfObject(merged.Get(PdfName.V));
            if (pdfObject == null)
            {
                return "";
            }

            if (pdfObject is PRStream)
            {
                return PdfEncodings.ConvertToString(PdfReader.GetStreamBytes((PRStream)pdfObject), "Cp1252");
            }

            PdfName asName = merged.GetAsName(PdfName.FT);
            if (PdfName.BTN.Equals(asName))
            {
                PdfNumber asNumber = merged.GetAsNumber(PdfName.FF);
                int num = 0;
                if (asNumber != null)
                {
                    num = asNumber.IntValue;
                }

                if (((uint)num & 0x10000u) != 0)
                {
                    return "";
                }

                string text = "";
                if (pdfObject is PdfName)
                {
                    text = PdfName.DecodeName(pdfObject.ToString());
                }
                else if (pdfObject is PdfString)
                {
                    text = ((PdfString)pdfObject).ToUnicodeString();
                }

                PdfArray asArray = item.GetValue(0).GetAsArray(PdfName.OPT);
                if (asArray != null)
                {
                    int num2 = 0;
                    try
                    {
                        num2 = int.Parse(text);
                        text = asArray.GetAsString(num2).ToUnicodeString();
                        lastWasString = true;
                        return text;
                    }
                    catch
                    {
                        return text;
                    }
                }

                return text;
            }

            if (pdfObject is PdfString)
            {
                lastWasString = true;
                return ((PdfString)pdfObject).ToUnicodeString();
            }

            if (pdfObject is PdfName)
            {
                return PdfName.DecodeName(pdfObject.ToString());
            }

            return "";
        }

        public virtual string[] GetListSelection(string name)
        {
            string field = GetField(name);
            string[] result = ((field != null) ? new string[1] { field } : new string[0]);
            if (!fields.ContainsKey(name))
            {
                return null;
            }

            PdfArray asArray = fields[name].GetMerged(0).GetAsArray(PdfName.I);
            if (asArray == null)
            {
                return result;
            }

            result = new string[asArray.Size];
            string[] listOptionExport = GetListOptionExport(name);
            int num = 0;
            foreach (PdfNumber array in asArray.ArrayList)
            {
                result[num++] = listOptionExport[array.IntValue];
            }

            return result;
        }

        public virtual bool SetFieldProperty(string field, string name, object value, int[] inst)
        {
            if (writer == null)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("this.acrofields.instance.is.read.only"));
            }

            if (!fields.ContainsKey(field))
            {
                return false;
            }

            Item item = fields[field];
            InstHit instHit = new InstHit(inst);
            if (Util.EqualsIgnoreCase(name, "textfont"))
            {
                for (int i = 0; i < item.Size; i++)
                {
                    if (!instHit.IsHit(i))
                    {
                        continue;
                    }

                    PdfDictionary merged = item.GetMerged(i);
                    PdfString asString = merged.GetAsString(PdfName.DA);
                    PdfDictionary pdfDictionary = merged.GetAsDict(PdfName.DR);
                    if (asString == null)
                    {
                        continue;
                    }

                    if (pdfDictionary == null)
                    {
                        pdfDictionary = new PdfDictionary();
                        merged.Put(PdfName.DR, pdfDictionary);
                    }

                    object[] array = SplitDAelements(asString.ToUnicodeString());
                    PdfAppearance pdfAppearance = new PdfAppearance();
                    if (array[0] == null)
                    {
                        continue;
                    }

                    BaseFont baseFont = (BaseFont)value;
                    if (!PdfAppearance.stdFieldFontNames.TryGetValue(baseFont.PostscriptFontName, out var value2))
                    {
                        value2 = new PdfName(baseFont.PostscriptFontName);
                    }

                    PdfDictionary pdfDictionary2 = pdfDictionary.GetAsDict(PdfName.FONT);
                    if (pdfDictionary2 == null)
                    {
                        pdfDictionary2 = new PdfDictionary();
                        pdfDictionary.Put(PdfName.FONT, pdfDictionary2);
                    }

                    PdfIndirectReference pdfIndirectReference = (PdfIndirectReference)pdfDictionary2.Get(value2);
                    PdfDictionary asDict = reader.Catalog.GetAsDict(PdfName.ACROFORM);
                    MarkUsed(asDict);
                    pdfDictionary = asDict.GetAsDict(PdfName.DR);
                    if (pdfDictionary == null)
                    {
                        pdfDictionary = new PdfDictionary();
                        asDict.Put(PdfName.DR, pdfDictionary);
                    }

                    MarkUsed(pdfDictionary);
                    PdfDictionary pdfDictionary3 = pdfDictionary.GetAsDict(PdfName.FONT);
                    if (pdfDictionary3 == null)
                    {
                        pdfDictionary3 = new PdfDictionary();
                        pdfDictionary.Put(PdfName.FONT, pdfDictionary3);
                    }

                    MarkUsed(pdfDictionary3);
                    PdfIndirectReference pdfIndirectReference2 = (PdfIndirectReference)pdfDictionary3.Get(value2);
                    if (pdfIndirectReference2 != null)
                    {
                        if (pdfIndirectReference == null)
                        {
                            pdfDictionary2.Put(value2, pdfIndirectReference2);
                        }
                    }
                    else if (pdfIndirectReference == null)
                    {
                        FontDetails fontDetails;
                        if (baseFont.FontType == 4)
                        {
                            fontDetails = new FontDetails(null, ((DocumentFont)baseFont).IndirectReference, baseFont);
                        }
                        else
                        {
                            baseFont.Subset = false;
                            fontDetails = writer.AddSimple(baseFont);
                            localFonts[value2.ToString().Substring(1)] = baseFont;
                        }

                        pdfDictionary3.Put(value2, fontDetails.IndirectReference);
                        pdfDictionary2.Put(value2, fontDetails.IndirectReference);
                    }

                    pdfAppearance.InternalBuffer.Append(value2.GetBytes()).Append(' ').Append((float)array[1])
                        .Append(" Tf ");
                    if (array[2] != null)
                    {
                        pdfAppearance.SetColorFill((BaseColor)array[2]);
                    }

                    PdfString value3 = new PdfString(pdfAppearance.ToString());
                    item.GetMerged(i).Put(PdfName.DA, value3);
                    item.GetWidget(i).Put(PdfName.DA, value3);
                    MarkUsed(item.GetWidget(i));
                }
            }
            else if (Util.EqualsIgnoreCase(name, "textcolor"))
            {
                for (int j = 0; j < item.Size; j++)
                {
                    if (!instHit.IsHit(j))
                    {
                        continue;
                    }

                    PdfDictionary merged = item.GetMerged(j);
                    PdfString asString = merged.GetAsString(PdfName.DA);
                    if (asString != null)
                    {
                        object[] array2 = SplitDAelements(asString.ToUnicodeString());
                        PdfAppearance pdfAppearance2 = new PdfAppearance();
                        if (array2[0] != null)
                        {
                            pdfAppearance2.InternalBuffer.Append(new PdfName((string)array2[0]).GetBytes()).Append(' ').Append((float)array2[1])
                                .Append(" Tf ");
                            pdfAppearance2.SetColorFill((BaseColor)value);
                            PdfString value4 = new PdfString(pdfAppearance2.ToString());
                            item.GetMerged(j).Put(PdfName.DA, value4);
                            item.GetWidget(j).Put(PdfName.DA, value4);
                            MarkUsed(item.GetWidget(j));
                        }
                    }
                }
            }
            else if (Util.EqualsIgnoreCase(name, "textsize"))
            {
                for (int k = 0; k < item.Size; k++)
                {
                    if (!instHit.IsHit(k))
                    {
                        continue;
                    }

                    PdfDictionary merged = item.GetMerged(k);
                    PdfString asString = merged.GetAsString(PdfName.DA);
                    if (asString == null)
                    {
                        continue;
                    }

                    object[] array3 = SplitDAelements(asString.ToUnicodeString());
                    PdfAppearance pdfAppearance3 = new PdfAppearance();
                    if (array3[0] != null)
                    {
                        pdfAppearance3.InternalBuffer.Append(new PdfName((string)array3[0]).GetBytes()).Append(' ').Append((float)value)
                            .Append(" Tf ");
                        if (array3[2] != null)
                        {
                            pdfAppearance3.SetColorFill((BaseColor)array3[2]);
                        }

                        PdfString value5 = new PdfString(pdfAppearance3.ToString());
                        item.GetMerged(k).Put(PdfName.DA, value5);
                        item.GetWidget(k).Put(PdfName.DA, value5);
                        MarkUsed(item.GetWidget(k));
                    }
                }
            }
            else
            {
                if (!Util.EqualsIgnoreCase(name, "bgcolor") && !Util.EqualsIgnoreCase(name, "bordercolor"))
                {
                    return false;
                }

                PdfName key = (Util.EqualsIgnoreCase(name, "bgcolor") ? PdfName.BG : PdfName.BC);
                for (int l = 0; l < item.Size; l++)
                {
                    if (!instHit.IsHit(l))
                    {
                        continue;
                    }

                    PdfDictionary merged = item.GetMerged(l);
                    PdfDictionary pdfDictionary4 = merged.GetAsDict(PdfName.MK);
                    if (pdfDictionary4 == null)
                    {
                        if (value == null)
                        {
                            return true;
                        }

                        pdfDictionary4 = new PdfDictionary();
                        item.GetMerged(l).Put(PdfName.MK, pdfDictionary4);
                        item.GetWidget(l).Put(PdfName.MK, pdfDictionary4);
                        MarkUsed(item.GetWidget(l));
                    }
                    else
                    {
                        MarkUsed(pdfDictionary4);
                    }

                    if (value == null)
                    {
                        pdfDictionary4.Remove(key);
                    }
                    else
                    {
                        pdfDictionary4.Put(key, PdfAnnotation.GetMKColor((BaseColor)value));
                    }
                }
            }

            return true;
        }

        public virtual bool SetFieldProperty(string field, string name, int value, int[] inst)
        {
            if (writer == null)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("this.acrofields.instance.is.read.only"));
            }

            if (!fields.ContainsKey(field))
            {
                return false;
            }

            Item item = fields[field];
            InstHit instHit = new InstHit(inst);
            if (Util.EqualsIgnoreCase(name, "flags"))
            {
                PdfNumber value2 = new PdfNumber(value);
                for (int i = 0; i < item.Size; i++)
                {
                    if (instHit.IsHit(i))
                    {
                        item.GetMerged(i).Put(PdfName.F, value2);
                        item.GetWidget(i).Put(PdfName.F, value2);
                        MarkUsed(item.GetWidget(i));
                    }
                }
            }
            else if (Util.EqualsIgnoreCase(name, "setflags"))
            {
                for (int j = 0; j < item.Size; j++)
                {
                    if (instHit.IsHit(j))
                    {
                        PdfNumber asNumber = item.GetWidget(j).GetAsNumber(PdfName.F);
                        int num = 0;
                        if (asNumber != null)
                        {
                            num = asNumber.IntValue;
                        }

                        asNumber = new PdfNumber(num | value);
                        item.GetMerged(j).Put(PdfName.F, asNumber);
                        item.GetWidget(j).Put(PdfName.F, asNumber);
                        MarkUsed(item.GetWidget(j));
                    }
                }
            }
            else if (Util.EqualsIgnoreCase(name, "clrflags"))
            {
                for (int k = 0; k < item.Size; k++)
                {
                    if (instHit.IsHit(k))
                    {
                        PdfDictionary widget = item.GetWidget(k);
                        PdfNumber asNumber2 = widget.GetAsNumber(PdfName.F);
                        int num2 = 0;
                        if (asNumber2 != null)
                        {
                            num2 = asNumber2.IntValue;
                        }

                        asNumber2 = new PdfNumber(num2 & ~value);
                        item.GetMerged(k).Put(PdfName.F, asNumber2);
                        widget.Put(PdfName.F, asNumber2);
                        MarkUsed(widget);
                    }
                }
            }
            else if (Util.EqualsIgnoreCase(name, "fflags"))
            {
                PdfNumber value3 = new PdfNumber(value);
                for (int l = 0; l < item.Size; l++)
                {
                    if (instHit.IsHit(l))
                    {
                        item.GetMerged(l).Put(PdfName.FF, value3);
                        item.GetValue(l).Put(PdfName.FF, value3);
                        MarkUsed(item.GetValue(l));
                    }
                }
            }
            else if (Util.EqualsIgnoreCase(name, "setfflags"))
            {
                for (int m = 0; m < item.Size; m++)
                {
                    if (instHit.IsHit(m))
                    {
                        PdfDictionary value4 = item.GetValue(m);
                        PdfNumber asNumber3 = value4.GetAsNumber(PdfName.FF);
                        int num3 = 0;
                        if (asNumber3 != null)
                        {
                            num3 = asNumber3.IntValue;
                        }

                        asNumber3 = new PdfNumber(num3 | value);
                        item.GetMerged(m).Put(PdfName.FF, asNumber3);
                        value4.Put(PdfName.FF, asNumber3);
                        MarkUsed(value4);
                    }
                }
            }
            else
            {
                if (!Util.EqualsIgnoreCase(name, "clrfflags"))
                {
                    return false;
                }

                for (int n = 0; n < item.Size; n++)
                {
                    if (instHit.IsHit(n))
                    {
                        PdfDictionary value5 = item.GetValue(n);
                        PdfNumber asNumber4 = value5.GetAsNumber(PdfName.FF);
                        int num4 = 0;
                        if (asNumber4 != null)
                        {
                            num4 = asNumber4.IntValue;
                        }

                        asNumber4 = new PdfNumber(num4 & ~value);
                        item.GetMerged(n).Put(PdfName.FF, asNumber4);
                        value5.Put(PdfName.FF, asNumber4);
                        MarkUsed(value5);
                    }
                }
            }

            return true;
        }

        public virtual void MergeXfaData(XmlNode n)
        {
            XfaForm.Xml2SomDatasets xml2SomDatasets = new XfaForm.Xml2SomDatasets(n);
            foreach (string item in xml2SomDatasets.Order)
            {
                string nodeText = XfaForm.GetNodeText(xml2SomDatasets.Name2Node[item]);
                SetField(item, nodeText);
            }
        }

        public virtual void SetFields(FdfReader fdf)
        {
            foreach (string key in fdf.Fields.Keys)
            {
                string fieldValue = fdf.GetFieldValue(key);
                if (fieldValue != null)
                {
                    SetField(key, fieldValue);
                }
            }
        }

        public virtual void SetFields(XfdfReader xfdf)
        {
            foreach (string key in xfdf.Fields.Keys)
            {
                string fieldValue = xfdf.GetFieldValue(key);
                if (fieldValue != null)
                {
                    SetField(key, fieldValue);
                }

                List<string> listValues = xfdf.GetListValues(key);
                if (listValues != null)
                {
                    string[] value = listValues.ToArray();
                    SetListSelection(fieldValue, value);
                }
            }
        }

        public virtual bool RegenerateField(string name)
        {
            string field = GetField(name);
            return SetField(name, field, field);
        }

        public virtual bool SetField(string name, string value)
        {
            return SetField(name, value, null);
        }

        public virtual bool SetFieldRichValue(string name, string richValue)
        {
            if (writer == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("this.acrofields.instance.is.read.only"));
            }

            Item fieldItem = GetFieldItem(name);
            if (fieldItem == null)
            {
                return false;
            }

            if (GetFieldType(name) != 4)
            {
                return false;
            }

            PdfNumber asNumber = fieldItem.GetMerged(0).GetAsNumber(PdfName.FF);
            int num = 0;
            if (asNumber != null)
            {
                num = asNumber.IntValue;
            }

            if ((num & 0x2000000) == 0)
            {
                return false;
            }

            PdfString value = new PdfString(richValue);
            fieldItem.WriteToAll(PdfName.RV, value, 5);
            PdfString value2 = new PdfString(XmlToTxt.Parse(new MemoryStream(Encoding.Default.GetBytes(richValue))));
            fieldItem.WriteToAll(PdfName.V, value2, 5);
            return true;
        }

        public virtual bool SetField(string name, string value, string display)
        {
            if (writer == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("this.acrofields.instance.is.read.only"));
            }

            if (xfa.XfaPresent)
            {
                name = xfa.FindFieldName(name, this);
                if (name == null)
                {
                    return false;
                }

                string shortName = XfaForm.Xml2Som.GetShortName(name);
                XmlNode xmlNode = xfa.FindDatasetsNode(shortName);
                if (xmlNode == null)
                {
                    xmlNode = xfa.DatasetsSom.InsertNode(xfa.DatasetsNode, shortName);
                }

                xfa.SetNodeText(xmlNode, value);
            }

            if (!fields.ContainsKey(name))
            {
                return false;
            }

            Item item = fields[name];
            PdfDictionary merged = item.GetMerged(0);
            PdfName asName = merged.GetAsName(PdfName.FT);
            if (PdfName.TX.Equals(asName))
            {
                PdfNumber asNumber = merged.GetAsNumber(PdfName.MAXLEN);
                int num = 0;
                if (asNumber != null)
                {
                    num = asNumber.IntValue;
                }

                if (num > 0)
                {
                    value = value.Substring(0, Math.Min(num, value.Length));
                }
            }

            if (display == null)
            {
                display = value;
            }

            if (PdfName.TX.Equals(asName) || PdfName.CH.Equals(asName))
            {
                PdfString value2 = new PdfString(value, "UnicodeBig");
                for (int i = 0; i < item.Size; i++)
                {
                    PdfDictionary value3 = item.GetValue(i);
                    value3.Put(PdfName.V, value2);
                    value3.Remove(PdfName.I);
                    MarkUsed(value3);
                    merged = item.GetMerged(i);
                    merged.Remove(PdfName.I);
                    merged.Put(PdfName.V, value2);
                    PdfDictionary widget = item.GetWidget(i);
                    if (generateAppearances)
                    {
                        PdfAppearance appearance = GetAppearance(merged, display, name);
                        if (PdfName.CH.Equals(asName))
                        {
                            PdfNumber value4 = new PdfNumber(topFirst);
                            widget.Put(PdfName.TI, value4);
                            merged.Put(PdfName.TI, value4);
                        }

                        PdfDictionary pdfDictionary = widget.GetAsDict(PdfName.AP);
                        if (pdfDictionary == null)
                        {
                            pdfDictionary = new PdfDictionary();
                            widget.Put(PdfName.AP, pdfDictionary);
                            merged.Put(PdfName.AP, pdfDictionary);
                        }

                        pdfDictionary.Put(PdfName.N, appearance.IndirectReference);
                        writer.ReleaseTemplate(appearance);
                    }
                    else
                    {
                        widget.Remove(PdfName.AP);
                        merged.Remove(PdfName.AP);
                    }

                    MarkUsed(widget);
                }

                return true;
            }

            if (PdfName.BTN.Equals(asName))
            {
                PdfNumber asNumber2 = item.GetMerged(0).GetAsNumber(PdfName.FF);
                int num2 = 0;
                if (asNumber2 != null)
                {
                    num2 = asNumber2.IntValue;
                }

                if (((uint)num2 & 0x10000u) != 0)
                {
                    Image instance;
                    try
                    {
                        instance = Image.GetInstance(Convert.FromBase64String(value));
                    }
                    catch
                    {
                        return false;
                    }

                    PushbuttonField newPushbuttonFromField = GetNewPushbuttonFromField(name);
                    newPushbuttonFromField.Image = instance;
                    ReplacePushbuttonField(name, newPushbuttonFromField.Field);
                    return true;
                }

                PdfName pdfName = new PdfName(value);
                List<string> list = new List<string>();
                PdfArray asArray = item.GetValue(0).GetAsArray(PdfName.OPT);
                if (asArray != null)
                {
                    for (int j = 0; j < asArray.Size; j++)
                    {
                        PdfString asString = asArray.GetAsString(j);
                        if (asString != null)
                        {
                            list.Add(asString.ToUnicodeString());
                        }
                        else
                        {
                            list.Add(null);
                        }
                    }
                }

                int num3 = list.IndexOf(value);
                PdfName pdfName2 = ((num3 < 0) ? pdfName : new PdfName(num3.ToString()));
                for (int k = 0; k < item.Size; k++)
                {
                    merged = item.GetMerged(k);
                    PdfDictionary widget2 = item.GetWidget(k);
                    PdfDictionary value5 = item.GetValue(k);
                    MarkUsed(item.GetValue(k));
                    value5.Put(PdfName.V, pdfName2);
                    merged.Put(PdfName.V, pdfName2);
                    MarkUsed(widget2);
                    PdfDictionary asDict = widget2.GetAsDict(PdfName.AP);
                    if (asDict == null)
                    {
                        return false;
                    }

                    PdfDictionary asDict2 = asDict.GetAsDict(PdfName.N);
                    if (IsInAP(asDict2, pdfName2) || asDict2 == null)
                    {
                        merged.Put(PdfName.AS, pdfName2);
                        widget2.Put(PdfName.AS, pdfName2);
                    }
                    else
                    {
                        merged.Put(PdfName.AS, PdfName.Off_);
                        widget2.Put(PdfName.AS, PdfName.Off_);
                    }

                    if (generateAppearances)
                    {
                        PdfAppearance appearance2 = GetAppearance(merged, display, name);
                        if (asDict2 != null)
                        {
                            asDict2.Put(merged.GetAsName(PdfName.AS), appearance2.IndirectReference);
                        }
                        else
                        {
                            asDict.Put(PdfName.N, appearance2.IndirectReference);
                        }

                        writer.ReleaseTemplate(appearance2);
                    }
                }

                return true;
            }

            return false;
        }

        public virtual bool SetListSelection(string name, string[] value)
        {
            Item fieldItem = GetFieldItem(name);
            if (fieldItem == null)
            {
                return false;
            }

            PdfDictionary merged = fieldItem.GetMerged(0);
            PdfName asName = merged.GetAsName(PdfName.FT);
            if (!PdfName.CH.Equals(asName))
            {
                return false;
            }

            string[] listOptionExport = GetListOptionExport(name);
            PdfArray pdfArray = new PdfArray();
            foreach (string value2 in value)
            {
                for (int j = 0; j < listOptionExport.Length; j++)
                {
                    if (listOptionExport[j].Equals(value2))
                    {
                        pdfArray.Add(new PdfNumber(j));
                    }
                }
            }

            fieldItem.WriteToAll(PdfName.I, pdfArray, 5);
            PdfArray pdfArray2 = new PdfArray();
            for (int k = 0; k < value.Length; k++)
            {
                pdfArray2.Add(new PdfString(value[k]));
            }

            fieldItem.WriteToAll(PdfName.V, pdfArray2, 5);
            PdfAppearance appearance = GetAppearance(merged, value, name);
            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.N, appearance.IndirectReference);
            fieldItem.WriteToAll(PdfName.AP, pdfDictionary, 3);
            writer.ReleaseTemplate(appearance);
            fieldItem.MarkUsed(this, 6);
            return true;
        }

        internal bool IsInAP(PdfDictionary nDic, PdfName check)
        {
            if (nDic != null)
            {
                return nDic.Get(check) != null;
            }

            return false;
        }

        public virtual Item GetFieldItem(string name)
        {
            if (xfa.XfaPresent)
            {
                name = xfa.FindFieldName(name, this);
                if (name == null)
                {
                    return null;
                }
            }

            if (!fields.ContainsKey(name))
            {
                return null;
            }

            return fields[name];
        }

        public virtual string GetTranslatedFieldName(string name)
        {
            if (xfa.XfaPresent)
            {
                string text = xfa.FindFieldName(name, this);
                if (text != null)
                {
                    name = text;
                }
            }

            return name;
        }

        public virtual IList<FieldPosition> GetFieldPositions(string name)
        {
            Item fieldItem = GetFieldItem(name);
            if (fieldItem == null)
            {
                return null;
            }

            List<FieldPosition> list = new List<FieldPosition>();
            for (int i = 0; i < fieldItem.Size; i++)
            {
                try
                {
                    PdfArray asArray = fieldItem.GetWidget(i).GetAsArray(PdfName.RECT);
                    if (asArray == null)
                    {
                        continue;
                    }

                    Rectangle rectangle = PdfReader.GetNormalizedRectangle(asArray);
                    int page = fieldItem.GetPage(i);
                    int pageRotation = reader.GetPageRotation(page);
                    FieldPosition fieldPosition = new FieldPosition();
                    fieldPosition.page = page;
                    if (pageRotation != 0)
                    {
                        Rectangle pageSize = reader.GetPageSize(page);
                        switch (pageRotation)
                        {
                            case 270:
                                rectangle = new Rectangle(pageSize.Top - rectangle.Bottom, rectangle.Left, pageSize.Top - rectangle.Top, rectangle.Right);
                                break;
                            case 180:
                                rectangle = new Rectangle(pageSize.Right - rectangle.Left, pageSize.Top - rectangle.Bottom, pageSize.Right - rectangle.Right, pageSize.Top - rectangle.Top);
                                break;
                            case 90:
                                rectangle = new Rectangle(rectangle.Bottom, pageSize.Right - rectangle.Left, rectangle.Top, pageSize.Right - rectangle.Right);
                                break;
                        }

                        rectangle.Normalize();
                    }

                    fieldPosition.position = rectangle;
                    list.Add(fieldPosition);
                }
                catch
                {
                }
            }

            return list;
        }

        private int RemoveRefFromArray(PdfArray array, PdfObject refo)
        {
            if (refo == null || !refo.IsIndirect())
            {
                return array.Size;
            }

            PdfIndirectReference pdfIndirectReference = (PdfIndirectReference)refo;
            for (int i = 0; i < array.Size; i++)
            {
                PdfObject pdfObject = array.GetPdfObject(i);
                if (pdfObject.IsIndirect() && ((PdfIndirectReference)pdfObject).Number == pdfIndirectReference.Number)
                {
                    array.Remove(i--);
                }
            }

            return array.Size;
        }

        public virtual bool RemoveFieldsFromPage(int page)
        {
            if (page < 1)
            {
                return false;
            }

            string[] array = new string[fields.Count];
            fields.Keys.CopyTo(array, 0);
            bool flag = false;
            for (int i = 0; i < array.Length; i++)
            {
                bool flag2 = RemoveField(array[i], page);
                flag = flag || flag2;
            }

            return flag;
        }

        public virtual bool RemoveField(string name, int page)
        {
            Item fieldItem = GetFieldItem(name);
            if (fieldItem == null)
            {
                return false;
            }

            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObject(reader.Catalog.Get(PdfName.ACROFORM), reader.Catalog);
            if (pdfDictionary == null)
            {
                return false;
            }

            PdfArray asArray = pdfDictionary.GetAsArray(PdfName.FIELDS);
            if (asArray == null)
            {
                return false;
            }

            for (int i = 0; i < fieldItem.Size; i++)
            {
                int page2 = fieldItem.GetPage(i);
                if (page != -1 && page != page2)
                {
                    continue;
                }

                PdfIndirectReference widgetRef = fieldItem.GetWidgetRef(i);
                PdfDictionary pdfDictionary2 = fieldItem.GetWidget(i);
                PdfDictionary pageN = reader.GetPageN(page2);
                PdfArray asArray2 = pageN.GetAsArray(PdfName.ANNOTS);
                if (asArray2 != null)
                {
                    if (RemoveRefFromArray(asArray2, widgetRef) == 0)
                    {
                        pageN.Remove(PdfName.ANNOTS);
                        MarkUsed(pageN);
                    }
                    else
                    {
                        MarkUsed(asArray2);
                    }
                }

                PdfReader.KillIndirect(widgetRef);
                PdfIndirectReference refo = widgetRef;
                while ((widgetRef = pdfDictionary2.GetAsIndirectObject(PdfName.PARENT)) != null)
                {
                    pdfDictionary2 = pdfDictionary2.GetAsDict(PdfName.PARENT);
                    PdfArray asArray3 = pdfDictionary2.GetAsArray(PdfName.KIDS);
                    if (RemoveRefFromArray(asArray3, refo) != 0)
                    {
                        break;
                    }

                    refo = widgetRef;
                    PdfReader.KillIndirect(widgetRef);
                }

                if (widgetRef == null)
                {
                    RemoveRefFromArray(asArray, refo);
                    MarkUsed(asArray);
                }

                if (page != -1)
                {
                    fieldItem.Remove(i);
                    i--;
                }
            }

            if (page == -1 || fieldItem.Size == 0)
            {
                fields.Remove(name);
            }

            return true;
        }

        public virtual bool RemoveField(string name)
        {
            return RemoveField(name, -1);
        }

        public virtual bool ClearSignatureField(string name)
        {
            sigNames = null;
            FindSignatureNames();
            if (!sigNames.ContainsKey(name))
            {
                return false;
            }

            Item item = fields[name];
            item.MarkUsed(this, 6);
            int size = item.Size;
            for (int i = 0; i < size; i++)
            {
                ClearSigDic(item.GetMerged(i));
                ClearSigDic(item.GetWidget(i));
                ClearSigDic(item.GetValue(i));
            }

            return true;
        }

        private static void ClearSigDic(PdfDictionary dic)
        {
            dic.Remove(PdfName.AP);
            dic.Remove(PdfName.AS);
            dic.Remove(PdfName.V);
            dic.Remove(PdfName.DV);
            dic.Remove(PdfName.SV);
            dic.Remove(PdfName.FF);
            dic.Put(PdfName.F, new PdfNumber(4));
        }

        private void FindSignatureNames()
        {
            if (sigNames != null)
            {
                return;
            }

            sigNames = new Dictionary<string, int[]>();
            orderedSignatureNames = new List<string>();
            List<object[]> list = new List<object[]>();
            foreach (KeyValuePair<string, Item> field in fields)
            {
                PdfDictionary merged = field.Value.GetMerged(0);
                if (!PdfName.SIG.Equals(merged.Get(PdfName.FT)))
                {
                    continue;
                }

                PdfDictionary asDict = merged.GetAsDict(PdfName.V);
                if (asDict == null || asDict.GetAsString(PdfName.CONTENTS) == null)
                {
                    continue;
                }

                PdfArray asArray = asDict.GetAsArray(PdfName.BYTERANGE);
                if (asArray != null)
                {
                    int size = asArray.Size;
                    if (size >= 2)
                    {
                        int num = asArray.GetAsNumber(size - 1).IntValue + asArray.GetAsNumber(size - 2).IntValue;
                        list.Add(new object[2]
                        {
                            field.Key,
                            new int[2] { num, 0 }
                        });
                    }
                }
            }

            list.Sort(new ISorterComparator());
            if (list.Count > 0)
            {
                if (((int[])list[list.Count - 1][1])[0] == reader.FileLength)
                {
                    totalRevisions = list.Count;
                }
                else
                {
                    totalRevisions = list.Count + 1;
                }

                for (int i = 0; i < list.Count; i++)
                {
                    object[] array = list[i];
                    string text = (string)array[0];
                    int[] array2 = (int[])array[1];
                    array2[1] = i + 1;
                    sigNames[text] = array2;
                    orderedSignatureNames.Add(text);
                }
            }
        }

        public virtual List<string> GetSignatureNames()
        {
            FindSignatureNames();
            return new List<string>(orderedSignatureNames);
        }

        public virtual List<string> GetBlankSignatureNames()
        {
            FindSignatureNames();
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, Item> field in fields)
            {
                PdfDictionary merged = field.Value.GetMerged(0);
                if (PdfName.SIG.Equals(merged.GetAsName(PdfName.FT)) && !sigNames.ContainsKey(field.Key))
                {
                    list.Add(field.Key);
                }
            }

            return list;
        }

        public virtual PdfDictionary GetSignatureDictionary(string name)
        {
            FindSignatureNames();
            name = GetTranslatedFieldName(name);
            if (!sigNames.ContainsKey(name))
            {
                return null;
            }

            return fields[name].GetMerged(0).GetAsDict(PdfName.V);
        }

        public virtual PdfIndirectReference GetNormalAppearance(string name)
        {
            GetSignatureNames();
            name = GetTranslatedFieldName(name);
            Item item = fields[name];
            if (item == null)
            {
                return null;
            }

            PdfDictionary asDict = item.GetMerged(0).GetAsDict(PdfName.AP);
            if (asDict == null)
            {
                return null;
            }

            PdfIndirectReference asIndirectObject = asDict.GetAsIndirectObject(PdfName.N);
            if (asIndirectObject == null)
            {
                return null;
            }

            return asIndirectObject;
        }

        public virtual bool SignatureCoversWholeDocument(string name)
        {
            FindSignatureNames();
            name = GetTranslatedFieldName(name);
            if (!sigNames.ContainsKey(name))
            {
                return false;
            }

            return sigNames[name][0] == reader.FileLength;
        }

        public virtual PdfPKCS7 VerifySignature(string name)
        {
            PdfDictionary signatureDictionary = GetSignatureDictionary(name);
            if (signatureDictionary == null)
            {
                return null;
            }

            PdfName asName = signatureDictionary.GetAsName(PdfName.SUBFILTER);
            PdfString asString = signatureDictionary.GetAsString(PdfName.CONTENTS);
            PdfPKCS7 pdfPKCS = null;
            if (asName.Equals(PdfName.ADBE_X509_RSA_SHA1))
            {
                PdfString asString2 = signatureDictionary.GetAsString(PdfName.CERT);
                if (asString2 == null)
                {
                    asString2 = signatureDictionary.GetAsArray(PdfName.CERT).GetAsString(0);
                }

                pdfPKCS = new PdfPKCS7(asString.GetOriginalBytes(), asString2.GetBytes());
            }
            else
            {
                pdfPKCS = new PdfPKCS7(asString.GetOriginalBytes(), asName);
            }

            UpdateByteRange(pdfPKCS, signatureDictionary);
            PdfString asString3 = signatureDictionary.GetAsString(PdfName.M);
            if (asString3 != null)
            {
                pdfPKCS.SignDate = PdfDate.Decode(asString3.ToString());
            }

            PdfObject pdfObject = PdfReader.GetPdfObject(signatureDictionary.Get(PdfName.NAME));
            if (pdfObject != null)
            {
                if (pdfObject.IsString())
                {
                    pdfPKCS.SignName = ((PdfString)pdfObject).ToUnicodeString();
                }
                else if (pdfObject.IsName())
                {
                    pdfPKCS.SignName = PdfName.DecodeName(pdfObject.ToString());
                }
            }

            asString3 = signatureDictionary.GetAsString(PdfName.REASON);
            if (asString3 != null)
            {
                pdfPKCS.Reason = asString3.ToUnicodeString();
            }

            asString3 = signatureDictionary.GetAsString(PdfName.LOCATION);
            if (asString3 != null)
            {
                pdfPKCS.Location = asString3.ToUnicodeString();
            }

            return pdfPKCS;
        }

        private void UpdateByteRange(PdfPKCS7 pkcs7, PdfDictionary v)
        {
            PdfArray asArray = v.GetAsArray(PdfName.BYTERANGE);
            RandomAccessFileOrArray safeFile = reader.SafeFile;
            Stream stream = null;
            try
            {
                stream = new RASInputStream(new RandomAccessSourceFactory().CreateRanged(safeFile.CreateSourceView(), asArray.AsLongArray()));
                byte[] array = new byte[8192];
                int len;
                while ((len = stream.Read(array, 0, array.Length)) > 0)
                {
                    pkcs7.Update(array, 0, len);
                }
            }
            finally
            {
                stream?.Close();
            }
        }

        public virtual int GetRevision(string field)
        {
            FindSignatureNames();
            field = GetTranslatedFieldName(field);
            if (!sigNames.ContainsKey(field))
            {
                return 0;
            }

            return sigNames[field][1];
        }

        public virtual Stream ExtractRevision(string field)
        {
            FindSignatureNames();
            field = GetTranslatedFieldName(field);
            if (!sigNames.ContainsKey(field))
            {
                return null;
            }

            int num = sigNames[field][0];
            RandomAccessFileOrArray safeFile = reader.SafeFile;
            safeFile.ReOpen();
            safeFile.Seek(0);
            return new RASInputStream(new WindowRandomAccessSource(safeFile.CreateSourceView(), 0L, num));
        }

        private void MarkUsed(PdfObject obj)
        {
            if (append)
            {
                ((PdfStamperImp)writer).MarkUsed(obj);
            }
        }

        public virtual void SetExtraMargin(float extraMarginLeft, float extraMarginTop)
        {
            this.extraMarginLeft = extraMarginLeft;
            this.extraMarginTop = extraMarginTop;
        }

        public virtual void AddSubstitutionFont(BaseFont font)
        {
            if (substitutionFonts == null)
            {
                substitutionFonts = new List<BaseFont>();
            }

            substitutionFonts.Add(font);
        }

        static AcroFields()
        {
            stdFieldFontNames = new Dictionary<string, string[]>();
            buttonRemove = new PdfName[6]
            {
                PdfName.MK,
                PdfName.F,
                PdfName.FF,
                PdfName.Q,
                PdfName.BS,
                PdfName.BORDER
            };
            stdFieldFontNames["CoBO"] = new string[1] { "Courier-BoldOblique" };
            stdFieldFontNames["CoBo"] = new string[1] { "Courier-Bold" };
            stdFieldFontNames["CoOb"] = new string[1] { "Courier-Oblique" };
            stdFieldFontNames["Cour"] = new string[1] { "Courier" };
            stdFieldFontNames["HeBO"] = new string[1] { "Helvetica-BoldOblique" };
            stdFieldFontNames["HeBo"] = new string[1] { "Helvetica-Bold" };
            stdFieldFontNames["HeOb"] = new string[1] { "Helvetica-Oblique" };
            stdFieldFontNames["Helv"] = new string[1] { "Helvetica" };
            stdFieldFontNames["Symb"] = new string[1] { "Symbol" };
            stdFieldFontNames["TiBI"] = new string[1] { "Times-BoldItalic" };
            stdFieldFontNames["TiBo"] = new string[1] { "Times-Bold" };
            stdFieldFontNames["TiIt"] = new string[1] { "Times-Italic" };
            stdFieldFontNames["TiRo"] = new string[1] { "Times-Roman" };
            stdFieldFontNames["ZaDb"] = new string[1] { "ZapfDingbats" };
            stdFieldFontNames["HySm"] = new string[2] { "HYSMyeongJo-Medium", "UniKS-UCS2-H" };
            stdFieldFontNames["HyGo"] = new string[2] { "HYGoThic-Medium", "UniKS-UCS2-H" };
            stdFieldFontNames["KaGo"] = new string[2] { "HeiseiKakuGo-W5", "UniKS-UCS2-H" };
            stdFieldFontNames["KaMi"] = new string[2] { "HeiseiMin-W3", "UniJIS-UCS2-H" };
            stdFieldFontNames["MHei"] = new string[2] { "MHei-Medium", "UniCNS-UCS2-H" };
            stdFieldFontNames["MSun"] = new string[2] { "MSung-Light", "UniCNS-UCS2-H" };
            stdFieldFontNames["STSo"] = new string[2] { "STSong-Light", "UniGB-UCS2-H" };
        }

        public virtual void RemoveXfa()
        {
            reader.Catalog.GetAsDict(PdfName.ACROFORM).Remove(PdfName.XFA);
            xfa = new XfaForm(reader);
        }

        public virtual PushbuttonField GetNewPushbuttonFromField(string field)
        {
            return GetNewPushbuttonFromField(field, 0);
        }

        public virtual PushbuttonField GetNewPushbuttonFromField(string field, int order)
        {
            if (GetFieldType(field) != 1)
            {
                return null;
            }

            Item fieldItem = GetFieldItem(field);
            if (order >= fieldItem.Size)
            {
                return null;
            }

            Rectangle position = GetFieldPositions(field)[order].position;
            PushbuttonField pushbuttonField = new PushbuttonField(writer, position, null);
            PdfDictionary merged = fieldItem.GetMerged(order);
            DecodeGenericDictionary(merged, pushbuttonField);
            PdfDictionary asDict = merged.GetAsDict(PdfName.MK);
            if (asDict != null)
            {
                PdfString asString = asDict.GetAsString(PdfName.CA);
                if (asString != null)
                {
                    pushbuttonField.Text = asString.ToUnicodeString();
                }

                PdfNumber asNumber = asDict.GetAsNumber(PdfName.TP);
                if (asNumber != null)
                {
                    pushbuttonField.Layout = asNumber.IntValue + 1;
                }

                PdfDictionary asDict2 = asDict.GetAsDict(PdfName.IF);
                if (asDict2 != null)
                {
                    PdfName asName = asDict2.GetAsName(PdfName.SW);
                    if (asName != null)
                    {
                        int scaleIcon = 1;
                        if (asName.Equals(PdfName.B))
                        {
                            scaleIcon = 3;
                        }
                        else if (asName.Equals(PdfName.S))
                        {
                            scaleIcon = 4;
                        }
                        else if (asName.Equals(PdfName.N))
                        {
                            scaleIcon = 2;
                        }

                        pushbuttonField.ScaleIcon = scaleIcon;
                    }

                    asName = asDict2.GetAsName(PdfName.S);
                    if (asName != null && asName.Equals(PdfName.A))
                    {
                        pushbuttonField.ProportionalIcon = false;
                    }

                    PdfArray asArray = asDict2.GetAsArray(PdfName.A);
                    if (asArray != null && asArray.Size == 2)
                    {
                        float floatValue = asArray.GetAsNumber(0).FloatValue;
                        float floatValue2 = asArray.GetAsNumber(1).FloatValue;
                        pushbuttonField.IconHorizontalAdjustment = floatValue;
                        pushbuttonField.IconVerticalAdjustment = floatValue2;
                    }

                    PdfBoolean asBoolean = asDict2.GetAsBoolean(PdfName.FB);
                    if (asBoolean != null && asBoolean.BooleanValue)
                    {
                        pushbuttonField.IconFitToBounds = true;
                    }
                }

                PdfObject pdfObject = asDict.Get(PdfName.I);
                if (pdfObject != null && pdfObject.IsIndirect())
                {
                    pushbuttonField.IconReference = (PRIndirectReference)pdfObject;
                }
            }

            return pushbuttonField;
        }

        public virtual bool ReplacePushbuttonField(string field, PdfFormField button)
        {
            return ReplacePushbuttonField(field, button, 0);
        }

        public virtual bool ReplacePushbuttonField(string field, PdfFormField button, int order)
        {
            if (GetFieldType(field) != 1)
            {
                return false;
            }

            Item fieldItem = GetFieldItem(field);
            if (order >= fieldItem.Size)
            {
                return false;
            }

            PdfDictionary merged = fieldItem.GetMerged(order);
            PdfDictionary value = fieldItem.GetValue(order);
            PdfDictionary widget = fieldItem.GetWidget(order);
            for (int i = 0; i < buttonRemove.Length; i++)
            {
                merged.Remove(buttonRemove[i]);
                value.Remove(buttonRemove[i]);
                widget.Remove(buttonRemove[i]);
            }

            foreach (PdfName key in button.Keys)
            {
                if (!key.Equals(PdfName.T))
                {
                    if (key.Equals(PdfName.FF))
                    {
                        value.Put(key, button.Get(key));
                    }
                    else
                    {
                        widget.Put(key, button.Get(key));
                    }

                    merged.Put(key, button.Get(key));
                    MarkUsed(value);
                    MarkUsed(widget);
                }
            }

            return true;
        }

        public virtual bool DoesSignatureFieldExist(string name)
        {
            if (!GetBlankSignatureNames().Contains(name))
            {
                return GetSignatureNames().Contains(name);
            }

            return true;
        }
    }
}
