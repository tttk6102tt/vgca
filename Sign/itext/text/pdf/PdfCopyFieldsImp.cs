using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.text.exceptions;
using Sign.itext.text.log;
using Sign.SystemItext.util;

namespace Sign.itext.text.pdf
{
    [Obsolete]
    internal class PdfCopyFieldsImp : PdfWriter
    {
        internal static readonly PdfName iTextTag;

        internal static int zero;

        internal List<PdfReader> readers = new List<PdfReader>();

        internal Dictionary<PdfReader, IntHashtable> readers2intrefs = new Dictionary<PdfReader, IntHashtable>();

        internal Dictionary<PdfReader, IntHashtable> pages2intrefs = new Dictionary<PdfReader, IntHashtable>();

        internal Dictionary<PdfReader, IntHashtable> visited = new Dictionary<PdfReader, IntHashtable>();

        internal List<AcroFields> fields = new List<AcroFields>();

        internal RandomAccessFileOrArray file;

        internal Dictionary<string, object> fieldTree = new Dictionary<string, object>();

        internal List<PdfIndirectReference> pageRefs = new List<PdfIndirectReference>();

        internal List<PdfDictionary> pageDics = new List<PdfDictionary>();

        internal PdfDictionary resources = new PdfDictionary();

        internal PdfDictionary form;

        private bool closing;

        internal Document nd;

        private Dictionary<PdfArray, List<int>> tabOrder;

        private List<string> calculationOrder = new List<string>();

        private List<object> calculationOrderRefs;

        private bool hasSignature;

        private bool needAppearances;

        private Dictionary<object, object> mergedRadioButtons = new Dictionary<object, object>();

        protected new ICounter COUNTER = CounterFactory.GetCounter(typeof(PdfCopyFields));

        protected internal static Dictionary<PdfName, int> widgetKeys;

        protected internal static Dictionary<PdfName, int> fieldKeys;

        protected override ICounter GetCounter()
        {
            return COUNTER;
        }

        internal PdfCopyFieldsImp(Stream os)
            : this(os, '\0')
        {
        }

        internal PdfCopyFieldsImp(Stream os, char pdfVersion)
            : base(new PdfDocument(), os)
        {
            pdf.AddWriter(this);
            if (pdfVersion != 0)
            {
                base.PdfVersion = pdfVersion;
            }

            nd = new Document();
            nd.AddDocListener(pdf);
        }

        internal void AddDocument(PdfReader reader, ICollection<int> pagesToKeep)
        {
            if (!readers2intrefs.ContainsKey(reader) && reader.Tampered)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("the.document.was.reused"));
            }

            reader = new PdfReader(reader);
            reader.SelectPages(pagesToKeep);
            if (reader.NumberOfPages != 0)
            {
                reader.Tampered = false;
                AddDocument(reader);
            }
        }

        internal void AddDocument(PdfReader reader)
        {
            if (!reader.IsOpenedWithFullPermissions)
            {
                throw new BadPasswordException(MessageLocalization.GetComposedMessage("pdfreader.not.opened.with.owner.password"));
            }

            OpenDoc();
            if (readers2intrefs.ContainsKey(reader))
            {
                reader = new PdfReader(reader);
            }
            else
            {
                if (reader.Tampered)
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("the.document.was.reused"));
                }

                reader.ConsolidateNamedDestinations();
                reader.Tampered = true;
            }

            reader.ShuffleSubsetNames();
            readers2intrefs[reader] = new IntHashtable();
            readers.Add(reader);
            int numberOfPages = reader.NumberOfPages;
            IntHashtable intHashtable = new IntHashtable();
            for (int i = 1; i <= numberOfPages; i++)
            {
                intHashtable[reader.GetPageOrigRef(i).Number] = 1;
                reader.ReleasePage(i);
            }

            pages2intrefs[reader] = intHashtable;
            visited[reader] = new IntHashtable();
            AcroFields acroFields = reader.AcroFields;
            if (!acroFields.GenerateAppearances)
            {
                needAppearances = true;
            }

            fields.Add(acroFields);
            UpdateCalculationOrder(reader);
        }

        internal static string GetCOName(PdfReader reader, PRIndirectReference refi)
        {
            string text = "";
            while (refi != null)
            {
                PdfObject pdfObject = PdfReader.GetPdfObject(refi);
                if (pdfObject == null || pdfObject.Type != 6)
                {
                    break;
                }

                PdfDictionary obj = (PdfDictionary)pdfObject;
                PdfString asString = obj.GetAsString(PdfName.T);
                if (asString != null)
                {
                    text = asString.ToUnicodeString() + "." + text;
                }

                refi = (PRIndirectReference)obj.Get(PdfName.PARENT);
            }

            if (text.EndsWith("."))
            {
                text = text.Substring(0, text.Length - 1);
            }

            return text;
        }

        protected internal virtual void UpdateCalculationOrder(PdfReader reader)
        {
            PdfDictionary asDict = reader.Catalog.GetAsDict(PdfName.ACROFORM);
            if (asDict == null)
            {
                return;
            }

            PdfArray asArray = asDict.GetAsArray(PdfName.CO);
            if (asArray == null || asArray.Size == 0)
            {
                return;
            }

            AcroFields acroFields = reader.AcroFields;
            for (int i = 0; i < asArray.Size; i++)
            {
                PdfObject pdfObject = asArray[i];
                if (pdfObject == null || !pdfObject.IsIndirect())
                {
                    continue;
                }

                string cOName = GetCOName(reader, (PRIndirectReference)pdfObject);
                if (acroFields.GetFieldItem(cOName) != null)
                {
                    cOName = "." + cOName;
                    if (!calculationOrder.Contains(cOName))
                    {
                        calculationOrder.Add(cOName);
                    }
                }
            }
        }

        internal void Propagate(PdfObject obj, PdfIndirectReference refo, bool restricted)
        {
            if (obj == null || obj is PdfIndirectReference)
            {
                return;
            }

            switch (obj.Type)
            {
                case 6:
                case 7:
                    {
                        PdfDictionary pdfDictionary = (PdfDictionary)obj;
                        foreach (PdfName key in pdfDictionary.Keys)
                        {
                            if (restricted && (key.Equals(PdfName.PARENT) || key.Equals(PdfName.KIDS)))
                            {
                                continue;
                            }

                            PdfObject pdfObject2 = pdfDictionary.Get(key);
                            if (pdfObject2 != null && pdfObject2.IsIndirect())
                            {
                                PRIndirectReference pRIndirectReference2 = (PRIndirectReference)pdfObject2;
                                if (!SetVisited(pRIndirectReference2) && !IsPage(pRIndirectReference2))
                                {
                                    PdfIndirectReference newReference2 = GetNewReference(pRIndirectReference2);
                                    Propagate(PdfReader.GetPdfObjectRelease(pRIndirectReference2), newReference2, restricted);
                                }
                            }
                            else
                            {
                                Propagate(pdfObject2, null, restricted);
                            }
                        }

                        break;
                    }
                case 5:
                    {
                        ListIterator<PdfObject> listIterator = ((PdfArray)obj).GetListIterator();
                        while (listIterator.HasNext())
                        {
                            PdfObject pdfObject = listIterator.Next();
                            if (pdfObject != null && pdfObject.IsIndirect())
                            {
                                PRIndirectReference pRIndirectReference = (PRIndirectReference)pdfObject;
                                if (!IsVisited(pRIndirectReference) && !IsPage(pRIndirectReference))
                                {
                                    PdfIndirectReference newReference = GetNewReference(pRIndirectReference);
                                    Propagate(PdfReader.GetPdfObjectRelease(pRIndirectReference), newReference, restricted);
                                }
                            }
                            else
                            {
                                Propagate(pdfObject, null, restricted);
                            }
                        }

                        break;
                    }
                case 10:
                    throw new Exception(MessageLocalization.GetComposedMessage("reference.pointing.to.reference"));
                case 8:
                case 9:
                    break;
            }
        }

        private void AdjustTabOrder(PdfArray annots, PdfIndirectReference ind, PdfNumber nn)
        {
            int intValue = nn.IntValue;
            if (!tabOrder.TryGetValue(annots, out var value))
            {
                value = new List<int>();
                int num = annots.Size - 1;
                for (int i = 0; i < num; i++)
                {
                    value.Add(zero);
                }

                value.Add(intValue);
                tabOrder[annots] = value;
                annots.Add(ind);
                return;
            }

            int num2 = value.Count - 1;
            for (int num3 = num2; num3 >= 0; num3--)
            {
                if (value[num3] <= intValue)
                {
                    value.Insert(num3 + 1, intValue);
                    annots.Add(num3 + 1, ind);
                    num2 = -2;
                    break;
                }
            }

            if (num2 != -2)
            {
                value.Insert(0, intValue);
                annots.Add(0, ind);
            }
        }

        protected virtual PdfArray BranchForm(Dictionary<string, object> level, PdfIndirectReference parent, string fname)
        {
            PdfArray pdfArray = new PdfArray();
            foreach (KeyValuePair<string, object> item in level)
            {
                string key = item.Key;
                object value = item.Value;
                PdfIndirectReference pdfIndirectReference = PdfIndirectReference;
                PdfDictionary pdfDictionary = new PdfDictionary();
                if (parent != null)
                {
                    pdfDictionary.Put(PdfName.PARENT, parent);
                }

                pdfDictionary.Put(PdfName.T, new PdfString(key, "UnicodeBig"));
                string text = fname + "." + key;
                int num = calculationOrder.IndexOf(text);
                if (num >= 0)
                {
                    calculationOrderRefs[num] = pdfIndirectReference;
                }

                if (value is Dictionary<string, object>)
                {
                    pdfDictionary.Put(PdfName.KIDS, BranchForm((Dictionary<string, object>)value, pdfIndirectReference, text));
                    pdfArray.Add(pdfIndirectReference);
                    AddToBody(pdfDictionary, pdfIndirectReference);
                    continue;
                }

                List<object> list = (List<object>)value;
                pdfDictionary.MergeDifferent((PdfDictionary)list[0]);
                if (list.Count == 3)
                {
                    pdfDictionary.MergeDifferent((PdfDictionary)list[2]);
                    int num2 = (int)list[1];
                    PdfDictionary pdfDictionary2 = pageDics[num2 - 1];
                    PdfArray pdfArray2 = pdfDictionary2.GetAsArray(PdfName.ANNOTS);
                    if (pdfArray2 == null)
                    {
                        pdfArray2 = new PdfArray();
                        pdfDictionary2.Put(PdfName.ANNOTS, pdfArray2);
                    }

                    PdfNumber nn = (PdfNumber)pdfDictionary.Get(iTextTag);
                    pdfDictionary.Remove(iTextTag);
                    AdjustTabOrder(pdfArray2, pdfIndirectReference, nn);
                }
                else
                {
                    PdfDictionary pdfDictionary3 = (PdfDictionary)list[0];
                    PdfName asName = pdfDictionary3.GetAsName(PdfName.V);
                    PdfArray pdfArray3 = new PdfArray();
                    for (int i = 1; i < list.Count; i += 2)
                    {
                        int num3 = (int)list[i];
                        PdfDictionary pdfDictionary4 = pageDics[num3 - 1];
                        PdfArray pdfArray4 = pdfDictionary4.GetAsArray(PdfName.ANNOTS);
                        if (pdfArray4 == null)
                        {
                            pdfArray4 = new PdfArray();
                            pdfDictionary4.Put(PdfName.ANNOTS, pdfArray4);
                        }

                        PdfDictionary pdfDictionary5 = new PdfDictionary();
                        pdfDictionary5.Merge((PdfDictionary)list[i + 1]);
                        pdfDictionary5.Put(PdfName.PARENT, pdfIndirectReference);
                        PdfNumber nn2 = (PdfNumber)pdfDictionary5.Get(iTextTag);
                        pdfDictionary5.Remove(iTextTag);
                        if (PdfCopy.IsCheckButton(pdfDictionary3))
                        {
                            PdfName asName2 = pdfDictionary5.GetAsName(PdfName.AS);
                            if (asName != null && asName2 != null)
                            {
                                pdfDictionary5.Put(PdfName.AS, asName);
                            }
                        }
                        else if (PdfCopy.IsRadioButton(pdfDictionary3))
                        {
                            PdfName asName3 = pdfDictionary5.GetAsName(PdfName.AS);
                            if (asName != null && asName3 != null && !asName3.Equals(GetOffStateName(pdfDictionary5)))
                            {
                                if (!mergedRadioButtons.ContainsKey(list))
                                {
                                    mergedRadioButtons[list] = null;
                                    pdfDictionary5.Put(PdfName.AS, asName);
                                }
                                else
                                {
                                    pdfDictionary5.Put(PdfName.AS, GetOffStateName(pdfDictionary5));
                                }
                            }
                        }

                        PdfIndirectReference indirectReference = AddToBody(pdfDictionary5).IndirectReference;
                        AdjustTabOrder(pdfArray4, indirectReference, nn2);
                        pdfArray3.Add(indirectReference);
                        Propagate(pdfDictionary5, null, restricted: false);
                    }

                    pdfDictionary.Put(PdfName.KIDS, pdfArray3);
                }

                pdfArray.Add(pdfIndirectReference);
                AddToBody(pdfDictionary, pdfIndirectReference);
                Propagate(pdfDictionary, null, restricted: false);
            }

            return pdfArray;
        }

        protected virtual PdfName GetOffStateName(PdfDictionary widget)
        {
            return PdfName.Off_;
        }

        protected virtual void CreateAcroForms()
        {
            if (fieldTree.Count == 0)
            {
                return;
            }

            form = new PdfDictionary();
            form.Put(PdfName.DR, resources);
            Propagate(resources, null, restricted: false);
            if (needAppearances)
            {
                form.Put(PdfName.NEEDAPPEARANCES, PdfBoolean.PDFTRUE);
            }

            form.Put(PdfName.DA, new PdfString("/Helv 0 Tf 0 g "));
            tabOrder = new Dictionary<PdfArray, List<int>>();
            calculationOrderRefs = new List<object>();
            foreach (string item in calculationOrder)
            {
                calculationOrderRefs.Add(item);
            }

            form.Put(PdfName.FIELDS, BranchForm(fieldTree, null, ""));
            if (hasSignature)
            {
                form.Put(PdfName.SIGFLAGS, new PdfNumber(3));
            }

            PdfArray pdfArray = new PdfArray();
            for (int i = 0; i < calculationOrderRefs.Count; i++)
            {
                object obj = calculationOrderRefs[i];
                if (obj is PdfIndirectReference)
                {
                    pdfArray.Add((PdfIndirectReference)obj);
                }
            }

            if (pdfArray.Size > 0)
            {
                form.Put(PdfName.CO, pdfArray);
            }
        }

        public override void Close()
        {
            if (closing)
            {
                base.Close();
                return;
            }

            closing = true;
            CloseIt();
        }

        protected virtual void CloseIt()
        {
            for (int i = 0; i < readers.Count; i++)
            {
                readers[i].RemoveFields();
            }

            for (int j = 0; j < readers.Count; j++)
            {
                PdfReader pdfReader = readers[j];
                for (int k = 1; k <= pdfReader.NumberOfPages; k++)
                {
                    pageRefs.Add(GetNewReference(pdfReader.GetPageOrigRef(k)));
                    pageDics.Add(pdfReader.GetPageN(k));
                }
            }

            MergeFields();
            CreateAcroForms();
            for (int l = 0; l < readers.Count; l++)
            {
                PdfReader pdfReader2 = readers[l];
                for (int m = 1; m <= pdfReader2.NumberOfPages; m++)
                {
                    PdfDictionary pageN = pdfReader2.GetPageN(m);
                    PdfIndirectReference newReference = GetNewReference(pdfReader2.GetPageOrigRef(m));
                    PdfIndirectReference value = root.AddPageRef(newReference);
                    pageN.Put(PdfName.PARENT, value);
                    Propagate(pageN, newReference, restricted: false);
                }
            }

            foreach (KeyValuePair<PdfReader, IntHashtable> readers2intref in readers2intrefs)
            {
                PdfReader key = readers2intref.Key;
                try
                {
                    file = key.SafeFile;
                    file.ReOpen();
                    IntHashtable value2 = readers2intref.Value;
                    int[] array = value2.ToOrderedKeys();
                    for (int n = 0; n < array.Length; n++)
                    {
                        PRIndirectReference obj = new PRIndirectReference(key, array[n]);
                        AddToBody(PdfReader.GetPdfObjectRelease(obj), value2[array[n]]);
                    }
                }
                finally
                {
                    try
                    {
                        file.Close();
                    }
                    catch
                    {
                    }
                }
            }

            pdf.Close();
        }

        internal void AddPageOffsetToField(IDictionary<string, AcroFields.Item> fd, int pageOffset)
        {
            if (pageOffset == 0)
            {
                return;
            }

            foreach (AcroFields.Item value in fd.Values)
            {
                List<int> page = value.page;
                for (int i = 0; i < page.Count; i++)
                {
                    int page2 = value.GetPage(i);
                    value.ForcePage(i, page2 + pageOffset);
                }
            }
        }

        internal void CreateWidgets(List<object> list, AcroFields.Item item)
        {
            for (int i = 0; i < item.Size; i++)
            {
                list.Add(item.GetPage(i));
                PdfDictionary merged = item.GetMerged(i);
                PdfObject pdfObject = merged.Get(PdfName.DR);
                if (pdfObject != null)
                {
                    PdfFormField.MergeResources(resources, (PdfDictionary)PdfReader.GetPdfObject(pdfObject));
                }

                PdfDictionary pdfDictionary = new PdfDictionary();
                foreach (PdfName key in merged.Keys)
                {
                    if (widgetKeys.ContainsKey(key))
                    {
                        pdfDictionary.Put(key, merged.Get(key));
                    }
                }

                pdfDictionary.Put(iTextTag, new PdfNumber(item.GetTabOrder(i) + 1));
                list.Add(pdfDictionary);
            }
        }

        internal void MergeField(string name, AcroFields.Item item)
        {
            Dictionary<string, object> dictionary = fieldTree;
            StringTokenizer stringTokenizer = new StringTokenizer(name, ".");
            if (!stringTokenizer.HasMoreTokens())
            {
                return;
            }

            string key;
            object value;
            while (true)
            {
                key = stringTokenizer.NextToken();
                dictionary.TryGetValue(key, out value);
                if (!stringTokenizer.HasMoreTokens())
                {
                    break;
                }

                if (value == null)
                {
                    value = (dictionary[key] = new Dictionary<string, object>());
                    dictionary = (Dictionary<string, object>)value;
                    continue;
                }

                if (value is Dictionary<string, object>)
                {
                    dictionary = (Dictionary<string, object>)value;
                    continue;
                }

                return;
            }

            if (value is Dictionary<string, object>)
            {
                return;
            }

            PdfDictionary merged = item.GetMerged(0);
            if (value == null)
            {
                PdfDictionary pdfDictionary = new PdfDictionary();
                if (PdfName.SIG.Equals(merged.Get(PdfName.FT)))
                {
                    hasSignature = true;
                }

                foreach (PdfName key2 in merged.Keys)
                {
                    if (fieldKeys.ContainsKey(key2))
                    {
                        pdfDictionary.Put(key2, merged.Get(key2));
                    }
                }

                List<object> list = new List<object>();
                list.Add(pdfDictionary);
                CreateWidgets(list, item);
                dictionary[key] = list;
                return;
            }

            List<object> list2 = (List<object>)value;
            PdfDictionary pdfDictionary2 = (PdfDictionary)list2[0];
            PdfName pdfName = (PdfName)pdfDictionary2.Get(PdfName.FT);
            PdfName obj = (PdfName)merged.Get(PdfName.FT);
            if (pdfName == null || !pdfName.Equals(obj))
            {
                return;
            }

            int num = 0;
            PdfObject pdfObject = pdfDictionary2.Get(PdfName.FF);
            if (pdfObject != null && pdfObject.IsNumber())
            {
                num = ((PdfNumber)pdfObject).IntValue;
            }

            int num2 = 0;
            PdfObject pdfObject2 = merged.Get(PdfName.FF);
            if (pdfObject2 != null && pdfObject2.IsNumber())
            {
                num2 = ((PdfNumber)pdfObject2).IntValue;
            }

            if (pdfName.Equals(PdfName.BTN))
            {
                if (((uint)(num ^ num2) & 0x10000u) != 0 || ((num & 0x10000) == 0 && ((uint)(num ^ num2) & 0x8000u) != 0))
                {
                    return;
                }
            }
            else if (pdfName.Equals(PdfName.CH) && ((uint)(num ^ num2) & 0x20000u) != 0)
            {
                return;
            }

            CreateWidgets(list2, item);
        }

        internal void MergeWithMaster(IDictionary<string, AcroFields.Item> fd)
        {
            foreach (KeyValuePair<string, AcroFields.Item> item in fd)
            {
                string key = item.Key;
                MergeField(key, item.Value);
            }
        }

        internal virtual void MergeFields()
        {
            int num = 0;
            for (int i = 0; i < fields.Count; i++)
            {
                IDictionary<string, AcroFields.Item> fd = fields[i].Fields;
                AddPageOffsetToField(fd, num);
                MergeWithMaster(fd);
                num += readers[i].NumberOfPages;
            }
        }

        public override PdfIndirectReference GetPageReference(int page)
        {
            return pageRefs[page - 1];
        }

        protected override PdfDictionary GetCatalog(PdfIndirectReference rootObj)
        {
            PdfDictionary catalog = pdf.GetCatalog(rootObj);
            if (form != null)
            {
                PdfIndirectReference indirectReference = AddToBody(form).IndirectReference;
                catalog.Put(PdfName.ACROFORM, indirectReference);
            }

            return catalog;
        }

        protected virtual PdfIndirectReference GetNewReference(PRIndirectReference refi)
        {
            return new PdfIndirectReference(0, GetNewObjectNumber(refi.Reader, refi.Number, 0));
        }

        protected internal override int GetNewObjectNumber(PdfReader reader, int number, int generation)
        {
            IntHashtable intHashtable = readers2intrefs[reader];
            int num = intHashtable[number];
            if (num == 0)
            {
                num = (intHashtable[number] = IndirectReferenceNumber);
            }

            return num;
        }

        protected internal virtual bool SetVisited(PRIndirectReference refi)
        {
            if (visited.TryGetValue(refi.Reader, out var value))
            {
                int num = value[refi.Number];
                value[refi.Number] = 1;
                return num != 0;
            }

            return false;
        }

        protected internal virtual bool IsVisited(PRIndirectReference refi)
        {
            if (visited.TryGetValue(refi.Reader, out var value))
            {
                return value.ContainsKey(refi.Number);
            }

            return false;
        }

        protected internal virtual bool IsVisited(PdfReader reader, int number, int generation)
        {
            return readers2intrefs[reader].ContainsKey(number);
        }

        protected internal virtual bool IsPage(PRIndirectReference refi)
        {
            if (pages2intrefs.TryGetValue(refi.Reader, out var value))
            {
                return value.ContainsKey(refi.Number);
            }

            return false;
        }

        internal override RandomAccessFileOrArray GetReaderFile(PdfReader reader)
        {
            return file;
        }

        public virtual void OpenDoc()
        {
            if (!nd.IsOpen())
            {
                nd.Open();
            }
        }

        static PdfCopyFieldsImp()
        {
            iTextTag = new PdfName("_iTextTag_");
            zero = 0;
            widgetKeys = new Dictionary<PdfName, int>();
            fieldKeys = new Dictionary<PdfName, int>();
            int value = 1;
            widgetKeys[PdfName.SUBTYPE] = value;
            widgetKeys[PdfName.CONTENTS] = value;
            widgetKeys[PdfName.RECT] = value;
            widgetKeys[PdfName.NM] = value;
            widgetKeys[PdfName.M] = value;
            widgetKeys[PdfName.F] = value;
            widgetKeys[PdfName.BS] = value;
            widgetKeys[PdfName.BORDER] = value;
            widgetKeys[PdfName.AP] = value;
            widgetKeys[PdfName.AS] = value;
            widgetKeys[PdfName.C] = value;
            widgetKeys[PdfName.A] = value;
            widgetKeys[PdfName.STRUCTPARENT] = value;
            widgetKeys[PdfName.OC] = value;
            widgetKeys[PdfName.H] = value;
            widgetKeys[PdfName.MK] = value;
            widgetKeys[PdfName.DA] = value;
            widgetKeys[PdfName.Q] = value;
            widgetKeys[PdfName.P] = value;
            fieldKeys[PdfName.AA] = value;
            fieldKeys[PdfName.FT] = value;
            fieldKeys[PdfName.TU] = value;
            fieldKeys[PdfName.TM] = value;
            fieldKeys[PdfName.FF] = value;
            fieldKeys[PdfName.V] = value;
            fieldKeys[PdfName.DV] = value;
            fieldKeys[PdfName.DS] = value;
            fieldKeys[PdfName.RV] = value;
            fieldKeys[PdfName.OPT] = value;
            fieldKeys[PdfName.MAXLEN] = value;
            fieldKeys[PdfName.TI] = value;
            fieldKeys[PdfName.I] = value;
            fieldKeys[PdfName.LOCK] = value;
            fieldKeys[PdfName.SV] = value;
        }
    }
}
