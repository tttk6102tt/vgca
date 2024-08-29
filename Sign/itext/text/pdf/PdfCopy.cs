using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.text.exceptions;
using Sign.itext.text.log;
using Sign.SystemItext.util;
using Sign.SystemItext.util.collections;

namespace Sign.itext.text.pdf
{
    public class PdfCopy : PdfWriter
    {
        public class IndirectReferences
        {
            private PdfIndirectReference theRef;

            private bool hasCopied;

            internal bool Copied
            {
                get
                {
                    return hasCopied;
                }
                set
                {
                    hasCopied = value;
                }
            }

            internal PdfIndirectReference Ref => theRef;

            internal IndirectReferences(PdfIndirectReference refi)
            {
                theRef = refi;
                hasCopied = false;
            }

            internal void SetCopied()
            {
                hasCopied = true;
            }

            public override string ToString()
            {
                string text = "";
                if (hasCopied)
                {
                    text += " Copied";
                }

                return string.Concat(Ref, text);
            }
        }

        protected class ImportedPage
        {
            internal readonly int pageNumber;

            internal readonly PdfReader reader;

            internal readonly PdfArray mergedFields;

            internal PdfIndirectReference annotsIndirectReference;

            internal ImportedPage(PdfReader reader, int pageNumber, bool keepFields)
            {
                this.pageNumber = pageNumber;
                this.reader = reader;
                if (keepFields)
                {
                    mergedFields = new PdfArray();
                }
            }

            public override bool Equals(object o)
            {
                if (!(o is ImportedPage))
                {
                    return false;
                }

                ImportedPage importedPage = (ImportedPage)o;
                if (pageNumber == importedPage.pageNumber)
                {
                    return reader.Equals(importedPage.reader);
                }

                return false;
            }

            public override string ToString()
            {
                int num = pageNumber;
                return num.ToString();
            }
        }

        public class PageStamp
        {
            private PdfDictionary pageN;

            private StampContent under;

            private StampContent over;

            private PageResources pageResources;

            private PdfReader reader;

            private PdfCopy cstp;

            internal PageStamp(PdfReader reader, PdfDictionary pageN, PdfCopy cstp)
            {
                this.pageN = pageN;
                this.reader = reader;
                this.cstp = cstp;
            }

            public virtual PdfContentByte GetUnderContent()
            {
                if (under == null)
                {
                    if (pageResources == null)
                    {
                        pageResources = new PageResources();
                        PdfDictionary asDict = pageN.GetAsDict(PdfName.RESOURCES);
                        pageResources.SetOriginalResources(asDict, cstp.namePtr);
                    }

                    under = new StampContent(cstp, pageResources);
                }

                return under;
            }

            public virtual PdfContentByte GetOverContent()
            {
                if (over == null)
                {
                    if (pageResources == null)
                    {
                        pageResources = new PageResources();
                        PdfDictionary asDict = pageN.GetAsDict(PdfName.RESOURCES);
                        pageResources.SetOriginalResources(asDict, cstp.namePtr);
                    }

                    over = new StampContent(cstp, pageResources);
                }

                return over;
            }

            public virtual void AlterContents()
            {
                if (over != null || under != null)
                {
                    PdfArray pdfArray = null;
                    PdfObject pdfObject = PdfReader.GetPdfObject(pageN.Get(PdfName.CONTENTS), pageN);
                    if (pdfObject == null)
                    {
                        pdfArray = new PdfArray();
                        pageN.Put(PdfName.CONTENTS, pdfArray);
                    }
                    else if (pdfObject.IsArray())
                    {
                        pdfArray = (PdfArray)pdfObject;
                    }
                    else if (pdfObject.IsStream())
                    {
                        pdfArray = new PdfArray();
                        pdfArray.Add(pageN.Get(PdfName.CONTENTS));
                        pageN.Put(PdfName.CONTENTS, pdfArray);
                    }
                    else
                    {
                        pdfArray = new PdfArray();
                        pageN.Put(PdfName.CONTENTS, pdfArray);
                    }

                    ByteBuffer byteBuffer = new ByteBuffer();
                    if (under != null)
                    {
                        byteBuffer.Append(PdfContents.SAVESTATE);
                        ApplyRotation(pageN, byteBuffer);
                        byteBuffer.Append(under.InternalBuffer);
                        byteBuffer.Append(PdfContents.RESTORESTATE);
                    }

                    if (over != null)
                    {
                        byteBuffer.Append(PdfContents.SAVESTATE);
                    }

                    PdfStream pdfStream = new PdfStream(byteBuffer.ToByteArray());
                    pdfStream.FlateCompress(cstp.CompressionLevel);
                    PdfIndirectReference indirectReference = cstp.AddToBody(pdfStream).IndirectReference;
                    pdfArray.AddFirst(indirectReference);
                    byteBuffer.Reset();
                    if (over != null)
                    {
                        byteBuffer.Append(' ');
                        byteBuffer.Append(PdfContents.RESTORESTATE);
                        byteBuffer.Append(PdfContents.SAVESTATE);
                        ApplyRotation(pageN, byteBuffer);
                        byteBuffer.Append(over.InternalBuffer);
                        byteBuffer.Append(PdfContents.RESTORESTATE);
                        pdfStream = new PdfStream(byteBuffer.ToByteArray());
                        pdfStream.FlateCompress(cstp.CompressionLevel);
                        pdfArray.Add(cstp.AddToBody(pdfStream).IndirectReference);
                    }

                    pageN.Put(PdfName.RESOURCES, pageResources.Resources);
                }
            }

            private void ApplyRotation(PdfDictionary pageN, ByteBuffer out_p)
            {
                if (cstp.rotateContents)
                {
                    Rectangle pageSizeWithRotation = reader.GetPageSizeWithRotation(pageN);
                    switch (pageSizeWithRotation.Rotation)
                    {
                        case 90:
                            out_p.Append(PdfContents.ROTATE90);
                            out_p.Append(pageSizeWithRotation.Top);
                            out_p.Append(' ').Append('0').Append(PdfContents.ROTATEFINAL);
                            break;
                        case 180:
                            out_p.Append(PdfContents.ROTATE180);
                            out_p.Append(pageSizeWithRotation.Right);
                            out_p.Append(' ');
                            out_p.Append(pageSizeWithRotation.Top);
                            out_p.Append(PdfContents.ROTATEFINAL);
                            break;
                        case 270:
                            out_p.Append(PdfContents.ROTATE270);
                            out_p.Append('0').Append(' ');
                            out_p.Append(pageSizeWithRotation.Right);
                            out_p.Append(PdfContents.ROTATEFINAL);
                            break;
                    }
                }
            }

            private void AddDocumentField(PdfIndirectReference refi)
            {
                if (cstp.fieldArray == null)
                {
                    cstp.fieldArray = new PdfArray();
                }

                cstp.fieldArray.Add(refi);
            }

            private void ExpandFields(PdfFormField field, List<PdfAnnotation> allAnnots)
            {
                allAnnots.Add(field);
                List<PdfFormField> kids = field.Kids;
                if (kids == null)
                {
                    return;
                }

                foreach (PdfFormField item in kids)
                {
                    ExpandFields(item, allAnnots);
                }
            }

            public virtual void AddAnnotation(PdfAnnotation annot)
            {
                List<PdfAnnotation> list = new List<PdfAnnotation>();
                if (annot.IsForm())
                {
                    PdfFormField pdfFormField = (PdfFormField)annot;
                    if (pdfFormField.Parent != null)
                    {
                        return;
                    }

                    ExpandFields(pdfFormField, list);
                    if (cstp.fieldTemplates == null)
                    {
                        cstp.fieldTemplates = new HashSet2<PdfTemplate>();
                    }
                }
                else
                {
                    list.Add(annot);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    annot = list[i];
                    if (annot.IsForm())
                    {
                        if (!annot.IsUsed())
                        {
                            Dictionary<PdfTemplate, object> templates = annot.Templates;
                            if (templates != null)
                            {
                                foreach (PdfTemplate key in templates.Keys)
                                {
                                    cstp.fieldTemplates.Add(key);
                                }
                            }
                        }

                        PdfFormField pdfFormField2 = (PdfFormField)annot;
                        if (pdfFormField2.Parent == null)
                        {
                            AddDocumentField(pdfFormField2.IndirectReference);
                        }
                    }

                    if (annot.IsAnnotation())
                    {
                        PdfObject pdfObject = PdfReader.GetPdfObject(pageN.Get(PdfName.ANNOTS), pageN);
                        PdfArray pdfArray = null;
                        if (pdfObject == null || !pdfObject.IsArray())
                        {
                            pdfArray = new PdfArray();
                            pageN.Put(PdfName.ANNOTS, pdfArray);
                        }
                        else
                        {
                            pdfArray = (PdfArray)pdfObject;
                        }

                        pdfArray.Add(annot.IndirectReference);
                        if (!annot.IsUsed())
                        {
                            PdfRectangle pdfRectangle = (PdfRectangle)annot.Get(PdfName.RECT);
                            if (pdfRectangle != null && (pdfRectangle.Left != 0f || pdfRectangle.Right != 0f || pdfRectangle.Top != 0f || pdfRectangle.Bottom != 0f))
                            {
                                int pageRotation = reader.GetPageRotation(pageN);
                                Rectangle pageSizeWithRotation = reader.GetPageSizeWithRotation(pageN);
                                switch (pageRotation)
                                {
                                    case 90:
                                        annot.Put(PdfName.RECT, new PdfRectangle(pageSizeWithRotation.Top - pdfRectangle.Bottom, pdfRectangle.Left, pageSizeWithRotation.Top - pdfRectangle.Top, pdfRectangle.Right));
                                        break;
                                    case 180:
                                        annot.Put(PdfName.RECT, new PdfRectangle(pageSizeWithRotation.Right - pdfRectangle.Left, pageSizeWithRotation.Top - pdfRectangle.Bottom, pageSizeWithRotation.Right - pdfRectangle.Right, pageSizeWithRotation.Top - pdfRectangle.Top));
                                        break;
                                    case 270:
                                        annot.Put(PdfName.RECT, new PdfRectangle(pdfRectangle.Bottom, pageSizeWithRotation.Right - pdfRectangle.Left, pdfRectangle.Top, pageSizeWithRotation.Right - pdfRectangle.Right));
                                        break;
                                }
                            }
                        }
                    }

                    if (!annot.IsUsed())
                    {
                        annot.SetUsed();
                        cstp.AddToBody(annot, annot.IndirectReference);
                    }
                }
            }
        }

        public class StampContent : PdfContentByte
        {
            private PageResources pageResources;

            public override PdfContentByte Duplicate => new StampContent(writer, pageResources);

            internal override PageResources PageResources => pageResources;

            internal StampContent(PdfWriter writer, PageResources pageResources)
                : base(writer)
            {
                this.pageResources = pageResources;
            }
        }

        protected new static ICounter COUNTER;

        protected internal Dictionary<RefKey, IndirectReferences> indirects;

        protected Dictionary<PdfReader, Dictionary<RefKey, IndirectReferences>> indirectMap;

        protected Dictionary<PdfObject, PdfObject> parentObjects;

        protected HashSet2<PdfObject> disableIndirects;

        protected PdfReader reader;

        protected int[] namePtr = new int[1];

        private bool rotateContents = true;

        protected internal PdfArray fieldArray;

        protected internal HashSet2<PdfTemplate> fieldTemplates;

        private PdfStructTreeController structTreeController;

        private int currentStructArrayNumber;

        protected PRIndirectReference structTreeRootReference;

        protected Dictionary<RefKey, PdfIndirectObject> indirectObjects;

        protected List<PdfIndirectObject> savedObjects;

        protected List<ImportedPage> importedPages;

        internal bool updateRootKids;

        private static readonly PdfName annotId;

        private static int annotIdCnt;

        protected bool mergeFields;

        private bool needAppearances;

        private bool hasSignature;

        private PdfIndirectReference acroForm;

        private Dictionary<PdfArray, List<int>> tabOrder;

        private List<object> calculationOrderRefs;

        private PdfDictionary resources;

        protected List<AcroFields> fields;

        private List<string> calculationOrder;

        private Dictionary<string, object> fieldTree;

        private Dictionary<int, PdfIndirectObject> unmergedMap;

        private HashSet2<PdfIndirectObject> unmergedSet;

        private Dictionary<int, PdfIndirectObject> mergedMap;

        private HashSet2<PdfIndirectObject> mergedSet;

        private bool mergeFieldsInternalCall;

        private static readonly PdfName iTextTag;

        internal static int zero;

        private HashSet2<object> mergedRadioButtons = new HashSet2<object>();

        private Dictionary<object, PdfString> mergedTextFields = new Dictionary<object, PdfString>();

        private HashSet2<PdfReader> readersWithImportedStructureTreeRootKids = new HashSet2<PdfReader>();

        protected static readonly HashSet2<PdfName> widgetKeys;

        protected static readonly HashSet2<PdfName> fieldKeys;

        public override IPdfPageEvent PageEvent
        {
            set
            {
                throw new InvalidOperationException();
            }
        }

        public virtual bool RotateContents
        {
            get
            {
                return rotateContents;
            }
            set
            {
                rotateContents = value;
            }
        }

        protected override ICounter GetCounter()
        {
            return COUNTER;
        }

        public PdfCopy(Document document, Stream os)
            : base(new PdfDocument(), os)
        {
            document.AddDocListener(pdf);
            pdf.AddWriter(this);
            indirectMap = new Dictionary<PdfReader, Dictionary<RefKey, IndirectReferences>>();
            parentObjects = new Dictionary<PdfObject, PdfObject>();
            disableIndirects = new HashSet2<PdfObject>();
            indirectObjects = new Dictionary<RefKey, PdfIndirectObject>();
            savedObjects = new List<PdfIndirectObject>();
            importedPages = new List<ImportedPage>();
        }

        public virtual void SetMergeFields()
        {
            mergeFields = true;
            resources = new PdfDictionary();
            fields = new List<AcroFields>();
            calculationOrder = new List<string>();
            fieldTree = new Dictionary<string, object>();
            unmergedMap = new Dictionary<int, PdfIndirectObject>();
            unmergedSet = new HashSet2<PdfIndirectObject>();
            mergedMap = new Dictionary<int, PdfIndirectObject>();
            mergedSet = new HashSet2<PdfIndirectObject>();
        }

        public override PdfImportedPage GetImportedPage(PdfReader reader, int pageNumber)
        {
            if (mergeFields && !mergeFieldsInternalCall)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("1.method.cannot.be.used.in.mergeFields.mode.please.use.addDocument", "getImportedPage"));
            }

            if (mergeFields)
            {
                ImportedPage item = new ImportedPage(reader, pageNumber, mergeFields);
                importedPages.Add(item);
            }

            if (structTreeController != null)
            {
                structTreeController.reader = null;
            }

            disableIndirects.Clear();
            parentObjects.Clear();
            return GetImportedPageImpl(reader, pageNumber);
        }

        public virtual PdfImportedPage GetImportedPage(PdfReader reader, int pageNumber, bool keepTaggedPdfStructure)
        {
            if (mergeFields && !mergeFieldsInternalCall)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("1.method.cannot.be.used.in.mergeFields.mode.please.use.addDocument", "getImportedPage"));
            }

            ImportedPage importedPage = null;
            updateRootKids = false;
            if (!keepTaggedPdfStructure)
            {
                if (mergeFields)
                {
                    importedPage = new ImportedPage(reader, pageNumber, mergeFields);
                    importedPages.Add(importedPage);
                }

                return GetImportedPageImpl(reader, pageNumber);
            }

            if (structTreeController != null)
            {
                if (reader != structTreeController.reader)
                {
                    structTreeController.SetReader(reader);
                }
            }
            else
            {
                structTreeController = new PdfStructTreeController(reader, this);
            }

            importedPage = new ImportedPage(reader, pageNumber, mergeFields);
            switch (CheckStructureTreeRootKids(importedPage))
            {
                case -1:
                    ClearIndirects(reader);
                    updateRootKids = true;
                    break;
                case 0:
                    updateRootKids = false;
                    break;
                case 1:
                    updateRootKids = true;
                    break;
            }

            importedPages.Add(importedPage);
            disableIndirects.Clear();
            parentObjects.Clear();
            return GetImportedPageImpl(reader, pageNumber);
        }

        private void ClearIndirects(PdfReader reader)
        {
            Dictionary<RefKey, IndirectReferences> dictionary = indirectMap[reader];
            List<RefKey> list = new List<RefKey>();
            foreach (KeyValuePair<RefKey, IndirectReferences> item in dictionary)
            {
                RefKey key = new RefKey(item.Value.Ref);
                if (!indirectObjects.TryGetValue(key, out var value))
                {
                    list.Add(item.Key);
                }
                else if (value.objecti.IsArray() || value.objecti.IsDictionary() || value.objecti.IsStream())
                {
                    list.Add(item.Key);
                }
            }

            foreach (RefKey item2 in list)
            {
                dictionary.Remove(item2);
            }
        }

        private int CheckStructureTreeRootKids(ImportedPage newPage)
        {
            if (importedPages.Count == 0)
            {
                return 1;
            }

            bool flag = false;
            foreach (ImportedPage importedPage2 in importedPages)
            {
                if (importedPage2.reader.Equals(newPage.reader))
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                return 1;
            }

            ImportedPage importedPage = importedPages[importedPages.Count - 1];
            if (importedPage.reader.Equals(newPage.reader) && newPage.pageNumber > importedPage.pageNumber)
            {
                if (readersWithImportedStructureTreeRootKids.Contains(newPage.reader))
                {
                    return 0;
                }

                return 1;
            }

            return -1;
        }

        protected internal virtual void StructureTreeRootKidsForReaderImported(PdfReader reader)
        {
            readersWithImportedStructureTreeRootKids.Add(reader);
        }

        internal virtual void FixStructureTreeRoot(HashSet2<RefKey> activeKeys, HashSet2<PdfName> activeClassMaps)
        {
            Dictionary<PdfName, PdfObject> dictionary = new Dictionary<PdfName, PdfObject>(activeClassMaps.Count);
            foreach (PdfName activeClassMap in activeClassMaps)
            {
                PdfObject pdfObject = structureTreeRoot.classes[activeClassMap];
                if (pdfObject != null)
                {
                    dictionary[activeClassMap] = pdfObject;
                }
            }

            structureTreeRoot.classes = dictionary;
            PdfArray asArray = structureTreeRoot.GetAsArray(PdfName.K);
            if (asArray == null)
            {
                return;
            }

            for (int i = 0; i < asArray.Size; i++)
            {
                RefKey item = new RefKey((PdfIndirectReference)asArray.GetPdfObject(i));
                if (!activeKeys.Contains(item))
                {
                    asArray.Remove(i--);
                }
            }
        }

        protected virtual PdfImportedPage GetImportedPageImpl(PdfReader reader, int pageNumber)
        {
            if (currentPdfReaderInstance != null)
            {
                if (currentPdfReaderInstance.Reader != reader)
                {
                    currentPdfReaderInstance = base.GetPdfReaderInstance(reader);
                }
            }
            else
            {
                currentPdfReaderInstance = base.GetPdfReaderInstance(reader);
            }

            return currentPdfReaderInstance.GetImportedPage(pageNumber);
        }

        protected internal virtual PdfIndirectReference CopyIndirect(PRIndirectReference inp, bool keepStructure, bool directRootKids)
        {
            RefKey key = new RefKey(inp);
            indirects.TryGetValue(key, out var value);
            PdfObject pdfObjectRelease = PdfReader.GetPdfObjectRelease(inp);
            if (keepStructure && directRootKids && pdfObjectRelease is PdfDictionary && ((PdfDictionary)pdfObjectRelease).Contains(PdfName.PG))
            {
                return null;
            }

            PdfIndirectReference pdfIndirectReference;
            if (value != null)
            {
                pdfIndirectReference = value.Ref;
                if (value.Copied)
                {
                    return pdfIndirectReference;
                }
            }
            else
            {
                pdfIndirectReference = body.PdfIndirectReference;
                value = new IndirectReferences(pdfIndirectReference);
                indirects[key] = value;
            }

            if (pdfObjectRelease != null && pdfObjectRelease.IsDictionary())
            {
                PdfObject pdfObjectRelease2 = PdfReader.GetPdfObjectRelease(((PdfDictionary)pdfObjectRelease).Get(PdfName.TYPE));
                if (pdfObjectRelease2 != null && PdfName.PAGE.Equals(pdfObjectRelease2))
                {
                    return pdfIndirectReference;
                }
            }

            value.SetCopied();
            if (pdfObjectRelease != null)
            {
                parentObjects[pdfObjectRelease] = inp;
            }

            PdfObject pdfObject = CopyObject(pdfObjectRelease, keepStructure, directRootKids);
            if (pdfObjectRelease != null && disableIndirects.Contains(pdfObjectRelease))
            {
                value.Copied = false;
            }

            if (pdfObject != null)
            {
                AddToBody(pdfObject, pdfIndirectReference);
                return pdfIndirectReference;
            }

            indirects.Remove(key);
            return null;
        }

        protected virtual PdfIndirectReference CopyIndirect(PRIndirectReference inp)
        {
            return CopyIndirect(inp, keepStructure: false, directRootKids: false);
        }

        protected virtual PdfDictionary CopyDictionary(PdfDictionary inp, bool keepStruct, bool directRootKids)
        {
            PdfDictionary pdfDictionary = new PdfDictionary();
            PdfObject pdfObjectRelease = PdfReader.GetPdfObjectRelease(inp.Get(PdfName.TYPE));
            if (keepStruct)
            {
                if (directRootKids && inp.Contains(PdfName.PG))
                {
                    PdfObject pdfObject = inp;
                    disableIndirects.Add(pdfObject);
                    while (parentObjects.ContainsKey(pdfObject) && !disableIndirects.Contains(pdfObject))
                    {
                        pdfObject = parentObjects[pdfObject];
                        disableIndirects.Add(pdfObject);
                    }

                    return null;
                }

                PdfName asName = inp.GetAsName(PdfName.S);
                structTreeController.AddRole(asName);
                structTreeController.AddClass(inp);
            }

            if (structTreeController != null && structTreeController.reader != null && (inp.Contains(PdfName.STRUCTPARENTS) || inp.Contains(PdfName.STRUCTPARENT)))
            {
                PdfName key = PdfName.STRUCTPARENT;
                if (inp.Contains(PdfName.STRUCTPARENTS))
                {
                    key = PdfName.STRUCTPARENTS;
                }

                PdfObject pdfObject2 = inp.Get(key);
                pdfDictionary.Put(key, new PdfNumber(currentStructArrayNumber));
                structTreeController.CopyStructTreeForPage((PdfNumber)pdfObject2, currentStructArrayNumber++);
            }

            foreach (PdfName key2 in inp.Keys)
            {
                PdfObject pdfObject3 = inp.Get(key2);
                if (structTreeController != null && structTreeController.reader != null && (key2.Equals(PdfName.STRUCTPARENTS) || key2.Equals(PdfName.STRUCTPARENT)))
                {
                    continue;
                }

                if (PdfName.PAGE.Equals(pdfObjectRelease))
                {
                    if (!key2.Equals(PdfName.B) && !key2.Equals(PdfName.PARENT))
                    {
                        parentObjects[pdfObject3] = inp;
                        PdfObject pdfObject4 = CopyObject(pdfObject3, keepStruct, directRootKids);
                        if (pdfObject4 != null)
                        {
                            pdfDictionary.Put(key2, pdfObject4);
                        }
                    }
                }
                else
                {
                    PdfObject pdfObject5 = ((!tagged || !pdfObject3.IsIndirect() || !IsStructTreeRootReference((PRIndirectReference)pdfObject3)) ? CopyObject(pdfObject3, keepStruct, directRootKids) : structureTreeRoot.Reference);
                    if (pdfObject5 != null)
                    {
                        pdfDictionary.Put(key2, pdfObject5);
                    }
                }
            }

            return pdfDictionary;
        }

        protected virtual PdfDictionary CopyDictionary(PdfDictionary inp)
        {
            return CopyDictionary(inp, keepStruct: false, directRootKids: false);
        }

        protected virtual PdfStream CopyStream(PRStream inp)
        {
            PRStream pRStream = new PRStream(inp, null);
            foreach (PdfName key in inp.Keys)
            {
                PdfObject pdfObject = inp.Get(key);
                parentObjects[pdfObject] = inp;
                PdfObject pdfObject2 = CopyObject(pdfObject);
                if (pdfObject2 != null)
                {
                    pRStream.Put(key, pdfObject2);
                }
            }

            return pRStream;
        }

        protected virtual PdfArray CopyArray(PdfArray inp, bool keepStruct, bool directRootKids)
        {
            PdfArray pdfArray = new PdfArray();
            foreach (PdfObject array in inp.ArrayList)
            {
                parentObjects[array] = inp;
                PdfObject pdfObject = CopyObject(array, keepStruct, directRootKids);
                if (pdfObject != null)
                {
                    pdfArray.Add(pdfObject);
                }
            }

            return pdfArray;
        }

        protected virtual PdfArray CopyArray(PdfArray inp)
        {
            return CopyArray(inp, keepStruct: false, directRootKids: false);
        }

        protected internal virtual PdfObject CopyObject(PdfObject inp, bool keepStruct, bool directRootKids)
        {
            if (inp == null)
            {
                return PdfNull.PDFNULL;
            }

            switch (inp.Type)
            {
                case 6:
                    return CopyDictionary((PdfDictionary)inp, keepStruct, directRootKids);
                case 10:
                    if (!keepStruct && !directRootKids)
                    {
                        return CopyIndirect((PRIndirectReference)inp);
                    }

                    return CopyIndirect((PRIndirectReference)inp, keepStruct, directRootKids);
                case 5:
                    return CopyArray((PdfArray)inp, keepStruct, directRootKids);
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 8:
                    return inp;
                case 7:
                    return CopyStream((PRStream)inp);
                default:
                    if (inp.Type < 0)
                    {
                        string text = ((PdfLiteral)inp).ToString();
                        if (text.Equals("true") || text.Equals("false"))
                        {
                            return new PdfBoolean(text);
                        }

                        return new PdfLiteral(text);
                    }

                    return null;
            }
        }

        protected internal virtual PdfObject CopyObject(PdfObject inp)
        {
            return CopyObject(inp, keepStruct: false, directRootKids: false);
        }

        protected virtual int SetFromIPage(PdfImportedPage iPage)
        {
            int pageNumber = iPage.PageNumber;
            reader = (currentPdfReaderInstance = iPage.PdfReaderInstance).Reader;
            SetFromReader(reader);
            return pageNumber;
        }

        protected virtual void SetFromReader(PdfReader reader)
        {
            this.reader = reader;
            if (!indirectMap.TryGetValue(reader, out indirects))
            {
                indirects = new Dictionary<RefKey, IndirectReferences>();
                indirectMap[reader] = indirects;
            }
        }

        public virtual void AddPage(PdfImportedPage iPage)
        {
            if (mergeFields && !mergeFieldsInternalCall)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("1.method.cannot.be.used.in.mergeFields.mode.please.use.addDocument", "addPage"));
            }

            int pageNum = SetFromIPage(iPage);
            PdfDictionary pageN = reader.GetPageN(pageNum);
            PRIndirectReference pageOrigRef = reader.GetPageOrigRef(pageNum);
            reader.ReleasePage(pageNum);
            RefKey key = new RefKey(pageOrigRef);
            if (indirects.TryGetValue(key, out var value) && !value.Copied)
            {
                pageReferences.Add(value.Ref);
                value.SetCopied();
            }

            PdfIndirectReference currentPage = CurrentPage;
            if (value == null)
            {
                value = new IndirectReferences(currentPage);
                indirects[key] = value;
            }

            value.SetCopied();
            if (tagged)
            {
                structTreeRootReference = (PRIndirectReference)reader.Catalog.Get(PdfName.STRUCTTREEROOT);
            }

            PdfDictionary pdfDictionary = CopyDictionary(pageN);
            if (mergeFields)
            {
                ImportedPage importedPage = importedPages[importedPages.Count - 1];
                importedPage.annotsIndirectReference = body.PdfIndirectReference;
                pdfDictionary.Put(PdfName.ANNOTS, importedPage.annotsIndirectReference);
            }

            root.AddPage(pdfDictionary);
            iPage.SetCopied();
            currentPageNumber++;
            pdf.PageCount = currentPageNumber;
            structTreeRootReference = null;
        }

        public virtual void AddPage(Rectangle rect, int rotation)
        {
            if (mergeFields && !mergeFieldsInternalCall)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("1.method.cannot.be.used.in.mergeFields.mode.please.use.addDocument", "addPage"));
            }

            PdfPage pdfPage = new PdfPage(new PdfRectangle(rect, rotation), resources: new PageResources().Resources, boxSize: new Dictionary<string, PdfRectangle>(), rotate: 0);
            pdfPage.Put(PdfName.TABS, Tabs);
            root.AddPage(pdfPage);
            currentPageNumber++;
            pdf.PageCount = currentPageNumber;
        }

        public virtual void AddDocument(PdfReader reader, List<int> pagesToKeep)
        {
            if (indirectMap.ContainsKey(reader))
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("document.1.has.already.been.added", reader.ToString()));
            }

            reader.SelectPages(pagesToKeep, removeUnused: false);
            AddDocument(reader);
        }

        public virtual void CopyDocumentFields(PdfReader reader)
        {
            if (!document.IsOpen())
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("the.document.is.not.open.yet.you.can.only.add.meta.information"));
            }

            if (indirectMap.ContainsKey(reader))
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("document.1.has.already.been.added", reader.ToString()));
            }

            if (!reader.IsOpenedWithFullPermissions)
            {
                throw new BadPasswordException(MessageLocalization.GetComposedMessage("pdfreader.not.opened.with.owner.password"));
            }

            if (!mergeFields)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("1.method.can.be.only.used.in.mergeFields.mode.please.use.addDocument", "copyDocumentFields"));
            }

            indirects = new Dictionary<RefKey, IndirectReferences>();
            indirectMap[reader] = indirects;
            reader.ConsolidateNamedDestinations();
            reader.ShuffleSubsetNames();
            if (tagged && PdfStructTreeController.CheckTagged(reader))
            {
                structTreeRootReference = (PRIndirectReference)reader.Catalog.Get(PdfName.STRUCTTREEROOT);
                if (structTreeController != null)
                {
                    if (reader != structTreeController.reader)
                    {
                        structTreeController.SetReader(reader);
                    }
                }
                else
                {
                    structTreeController = new PdfStructTreeController(reader, this);
                }
            }

            IList<PdfObject> list = new List<PdfObject>();
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                PdfDictionary pageNRelease = reader.GetPageNRelease(i);
                if (pageNRelease == null || !pageNRelease.Contains(PdfName.ANNOTS))
                {
                    continue;
                }

                PdfArray asArray = pageNRelease.GetAsArray(PdfName.ANNOTS);
                if (asArray == null || asArray.Size <= 0)
                {
                    continue;
                }

                if (importedPages.Count < i)
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("there.are.not.enough.imported.pages.for.copied.fields"));
                }

                indirectMap[reader][new RefKey(reader.pageRefs.GetPageOrigRef(i))] = new IndirectReferences(pageReferences[i - 1]);
                for (int j = 0; j < asArray.Size; j++)
                {
                    PdfDictionary asDict = asArray.GetAsDict(j);
                    if (asDict != null)
                    {
                        asDict.Put(annotId, new PdfNumber(++annotIdCnt));
                        list.Add(asArray.GetPdfObject(j));
                    }
                }
            }

            foreach (PdfObject item in list)
            {
                CopyObject(item);
            }

            if (tagged && structTreeController != null)
            {
                structTreeController.AttachStructTreeRootKids(null);
            }

            AcroFields acroFields = reader.AcroFields;
            if (!acroFields.GenerateAppearances)
            {
                needAppearances = true;
            }

            fields.Add(acroFields);
            UpdateCalculationOrder(reader);
            structTreeRootReference = null;
        }

        public virtual void AddDocument(PdfReader reader)
        {
            if (!document.IsOpen())
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("the.document.is.not.open.yet.you.can.only.add.meta.information"));
            }

            if (indirectMap.ContainsKey(reader))
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("document.1.has.already.been.added", reader.ToString()));
            }

            if (!reader.IsOpenedWithFullPermissions)
            {
                throw new BadPasswordException(MessageLocalization.GetComposedMessage("pdfreader.not.opened.with.owner.password"));
            }

            if (mergeFields)
            {
                reader.ConsolidateNamedDestinations();
                reader.ShuffleSubsetNames();
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    PdfDictionary pageNRelease = reader.GetPageNRelease(i);
                    if (pageNRelease == null || !pageNRelease.Contains(PdfName.ANNOTS))
                    {
                        continue;
                    }

                    PdfArray asArray = pageNRelease.GetAsArray(PdfName.ANNOTS);
                    if (asArray != null)
                    {
                        for (int j = 0; j < asArray.Size; j++)
                        {
                            asArray.GetAsDict(j)?.Put(annotId, new PdfNumber(++annotIdCnt));
                        }
                    }
                }

                AcroFields acroFields = reader.AcroFields;
                if (!acroFields.GenerateAppearances)
                {
                    needAppearances = true;
                }

                fields.Add(acroFields);
                UpdateCalculationOrder(reader);
            }

            bool keepTaggedPdfStructure = tagged && PdfStructTreeController.CheckTagged(reader);
            mergeFieldsInternalCall = true;
            for (int k = 1; k <= reader.NumberOfPages; k++)
            {
                AddPage(GetImportedPage(reader, k, keepTaggedPdfStructure));
            }

            mergeFieldsInternalCall = false;
        }

        public override PdfIndirectObject AddToBody(PdfObject objecta, PdfIndirectReference refa)
        {
            return AddToBody(objecta, refa, formBranching: false);
        }

        public override PdfIndirectObject AddToBody(PdfObject objecta, PdfIndirectReference refa, bool formBranching)
        {
            if (formBranching)
            {
                UpdateReferences(objecta);
            }

            PdfIndirectObject pdfIndirectObject;
            if ((tagged || mergeFields) && indirectObjects != null && (objecta.IsArray() || objecta.IsDictionary() || objecta.IsStream() || objecta.IsNull()))
            {
                RefKey key = new RefKey(refa);
                if (!indirectObjects.TryGetValue(key, out var value))
                {
                    value = new PdfIndirectObject(refa, objecta, this);
                    indirectObjects[key] = value;
                }

                pdfIndirectObject = value;
            }
            else
            {
                pdfIndirectObject = base.AddToBody(objecta, refa);
            }

            if (mergeFields && objecta.IsDictionary())
            {
                PdfNumber asNumber = ((PdfDictionary)objecta).GetAsNumber(annotId);
                if (asNumber != null)
                {
                    if (formBranching)
                    {
                        mergedMap[asNumber.IntValue] = pdfIndirectObject;
                        mergedSet.Add(pdfIndirectObject);
                    }
                    else
                    {
                        unmergedMap[asNumber.IntValue] = pdfIndirectObject;
                        unmergedSet.Add(pdfIndirectObject);
                    }
                }
            }

            return pdfIndirectObject;
        }

        protected internal override void CacheObject(PdfIndirectObject iobj)
        {
            if ((tagged || mergeFields) && indirectObjects != null)
            {
                savedObjects.Add(iobj);
                RefKey key = new RefKey(iobj.Number, iobj.Generation);
                if (!indirectObjects.ContainsKey(key))
                {
                    indirectObjects[key] = iobj;
                }
            }
        }

        internal override void FlushTaggedObjects()
        {
            try
            {
                FixTaggedStructure();
            }
            catch (InvalidCastException)
            {
            }
            finally
            {
                FlushIndirectObjects();
            }
        }

        internal override void FlushAcroFields()
        {
            if (!mergeFields)
            {
                return;
            }

            try
            {
                foreach (ImportedPage importedPage in importedPages)
                {
                    PdfDictionary pageN = importedPage.reader.GetPageN(importedPage.pageNumber);
                    if (pageN == null)
                    {
                        continue;
                    }

                    PdfArray asArray = pageN.GetAsArray(PdfName.ANNOTS);
                    if (asArray == null || asArray.Size == 0)
                    {
                        continue;
                    }

                    foreach (AcroFields.Item value in importedPage.reader.AcroFields.Fields.Values)
                    {
                        foreach (PdfIndirectReference widget_ref in value.widget_refs)
                        {
                            asArray.ArrayList.Remove(widget_ref);
                        }
                    }

                    foreach (PdfObject array in asArray.ArrayList)
                    {
                        importedPage.mergedFields.Add(CopyObject(array));
                    }
                }

                foreach (PdfReader key in indirectMap.Keys)
                {
                    key.RemoveFields();
                }

                MergeFields();
                CreateAcroForms();
            }
            catch (InvalidCastException)
            {
            }
            finally
            {
                if (!tagged)
                {
                    FlushIndirectObjects();
                }
            }
        }

        protected virtual void FixTaggedStructure()
        {
            Dictionary<int, PdfIndirectReference> numTree = structureTreeRoot.NumTree;
            HashSet2<RefKey> hashSet = new HashSet2<RefKey>();
            List<PdfIndirectReference> list = new List<PdfIndirectReference>();
            int num = 0;
            if (mergeFields && acroForm != null)
            {
                list.Add(acroForm);
                hashSet.Add(new RefKey(acroForm));
            }

            foreach (PdfIndirectReference pageReference in pageReferences)
            {
                list.Add(pageReference);
                hashSet.Add(new RefKey(pageReference));
            }

            for (int num2 = numTree.Count - 1; num2 >= 0; num2--)
            {
                PdfIndirectReference pdfIndirectReference = numTree[num2];
                RefKey refKey = new RefKey(pdfIndirectReference);
                PdfObject objecti = indirectObjects[refKey].objecti;
                if (objecti.IsDictionary())
                {
                    bool flag = false;
                    if (pageReferences.Contains((PdfIndirectReference)((PdfDictionary)objecti).Get(PdfName.PG)))
                    {
                        flag = true;
                    }
                    else
                    {
                        PdfDictionary kDict = PdfStructTreeController.GetKDict((PdfDictionary)objecti);
                        if (kDict != null && pageReferences.Contains((PdfIndirectReference)kDict.Get(PdfName.PG)))
                        {
                            flag = true;
                        }
                    }

                    if (flag)
                    {
                        hashSet.Add(refKey);
                        list.Add(pdfIndirectReference);
                    }
                    else
                    {
                        numTree.Remove(num2);
                    }
                }
                else if (objecti.IsArray())
                {
                    hashSet.Add(refKey);
                    list.Add(pdfIndirectReference);
                    PdfArray pdfArray = (PdfArray)objecti;
                    PdfIndirectReference pdfIndirectReference2 = pageReferences[num++];
                    list.Add(pdfIndirectReference2);
                    hashSet.Add(new RefKey(pdfIndirectReference2));
                    PdfIndirectReference obj = null;
                    for (int i = 0; i < pdfArray.Size; i++)
                    {
                        PdfIndirectReference pdfIndirectReference3 = (PdfIndirectReference)pdfArray.GetDirectObject(i);
                        if (pdfIndirectReference3.Equals(obj))
                        {
                            continue;
                        }

                        RefKey refKey2 = new RefKey(pdfIndirectReference3);
                        hashSet.Add(refKey2);
                        list.Add(pdfIndirectReference3);
                        PdfIndirectObject pdfIndirectObject = indirectObjects[refKey2];
                        if (pdfIndirectObject.objecti.IsDictionary())
                        {
                            PdfDictionary pdfDictionary = (PdfDictionary)pdfIndirectObject.objecti;
                            PdfIndirectReference pdfIndirectReference4 = (PdfIndirectReference)pdfDictionary.Get(PdfName.PG);
                            if (pdfIndirectReference4 != null && !pageReferences.Contains(pdfIndirectReference4) && !pdfIndirectReference4.Equals(pdfIndirectReference2))
                            {
                                pdfDictionary.Put(PdfName.PG, pdfIndirectReference2);
                                PdfArray asArray = pdfDictionary.GetAsArray(PdfName.K);
                                if (asArray != null && asArray.GetDirectObject(0).IsNumber())
                                {
                                    asArray.Remove(0);
                                }
                            }
                        }

                        obj = pdfIndirectReference3;
                    }
                }
            }

            HashSet2<PdfName> activeClassMaps = new HashSet2<PdfName>();
            FindActives(list, hashSet, activeClassMaps);
            List<PdfIndirectReference> newRefs = FindActiveParents(hashSet);
            FixPgKey(newRefs, hashSet);
            FixStructureTreeRoot(hashSet, activeClassMaps);
            List<RefKey> list2 = new List<RefKey>();
            foreach (KeyValuePair<RefKey, PdfIndirectObject> indirectObject in indirectObjects)
            {
                if (!hashSet.Contains(indirectObject.Key))
                {
                    list2.Add(indirectObject.Key);
                }
                else if (indirectObject.Value.objecti.IsArray())
                {
                    RemoveInactiveReferences((PdfArray)indirectObject.Value.objecti, hashSet);
                }
                else if (indirectObject.Value.objecti.IsDictionary())
                {
                    PdfObject pdfObject = ((PdfDictionary)indirectObject.Value.objecti).Get(PdfName.K);
                    if (pdfObject != null && pdfObject.IsArray())
                    {
                        RemoveInactiveReferences((PdfArray)pdfObject, hashSet);
                    }
                }
            }

            foreach (RefKey item in list2)
            {
                indirectObjects[item] = null;
            }
        }

        private void RemoveInactiveReferences(PdfArray array, HashSet2<RefKey> activeKeys)
        {
            for (int i = 0; i < array.Size; i++)
            {
                PdfObject pdfObject = array.GetPdfObject(i);
                if ((pdfObject.Type == 0 && !activeKeys.Contains(new RefKey((PdfIndirectReference)pdfObject))) || (pdfObject.IsDictionary() && ContainsInactivePg((PdfDictionary)pdfObject, activeKeys)))
                {
                    array.Remove(i--);
                }
            }
        }

        private bool ContainsInactivePg(PdfDictionary dict, HashSet2<RefKey> activeKeys)
        {
            PdfObject pdfObject = dict.Get(PdfName.PG);
            if (pdfObject != null && !activeKeys.Contains(new RefKey((PdfIndirectReference)pdfObject)))
            {
                return true;
            }

            return false;
        }

        private List<PdfIndirectReference> FindActiveParents(HashSet2<RefKey> activeKeys)
        {
            List<PdfIndirectReference> list = new List<PdfIndirectReference>();
            List<RefKey> list2 = new List<RefKey>(activeKeys);
            for (int i = 0; i < list2.Count; i++)
            {
                if (!indirectObjects.TryGetValue(list2[i], out var value) || !value.objecti.IsDictionary())
                {
                    continue;
                }

                PdfObject pdfObject = ((PdfDictionary)value.objecti).Get(PdfName.P);
                if (pdfObject != null && pdfObject.Type == 0)
                {
                    RefKey item = new RefKey((PdfIndirectReference)pdfObject);
                    if (!activeKeys.Contains(item))
                    {
                        activeKeys.Add(item);
                        list2.Add(item);
                        list.Add((PdfIndirectReference)pdfObject);
                    }
                }
            }

            return list;
        }

        private void FixPgKey(List<PdfIndirectReference> newRefs, HashSet2<RefKey> activeKeys)
        {
            foreach (PdfIndirectReference newRef in newRefs)
            {
                if (!indirectObjects.TryGetValue(new RefKey(newRef), out var value) || !value.objecti.IsDictionary())
                {
                    continue;
                }

                PdfDictionary pdfDictionary = (PdfDictionary)value.objecti;
                PdfObject pdfObject = pdfDictionary.Get(PdfName.PG);
                if (pdfObject == null || activeKeys.Contains(new RefKey((PdfIndirectReference)pdfObject)))
                {
                    continue;
                }

                PdfArray asArray = pdfDictionary.GetAsArray(PdfName.K);
                if (asArray == null)
                {
                    continue;
                }

                for (int i = 0; i < asArray.Size; i++)
                {
                    PdfObject pdfObject2 = asArray.GetPdfObject(i);
                    PdfIndirectObject value2;
                    if (pdfObject2.Type != 0)
                    {
                        asArray.Remove(i--);
                    }
                    else if (indirectObjects.TryGetValue(new RefKey((PdfIndirectReference)pdfObject2), out value2) && value2.objecti.IsDictionary())
                    {
                        PdfObject pdfObject3 = ((PdfDictionary)value2.objecti).Get(PdfName.PG);
                        if (pdfObject3 != null && activeKeys.Contains(new RefKey((PdfIndirectReference)pdfObject3)))
                        {
                            pdfDictionary.Put(PdfName.PG, pdfObject3);
                            break;
                        }
                    }
                }
            }
        }

        private void FindActives(List<PdfIndirectReference> actives, HashSet2<RefKey> activeKeys, HashSet2<PdfName> activeClassMaps)
        {
            for (int i = 0; i < actives.Count; i++)
            {
                RefKey key = new RefKey(actives[i]);
                if (indirectObjects.TryGetValue(key, out var value) && value.objecti != null)
                {
                    switch (value.objecti.Type)
                    {
                        case 0:
                            FindActivesFromReference((PdfIndirectReference)value.objecti, actives, activeKeys);
                            break;
                        case 5:
                            FindActivesFromArray((PdfArray)value.objecti, actives, activeKeys, activeClassMaps);
                            break;
                        case 6:
                        case 7:
                            FindActivesFromDict((PdfDictionary)value.objecti, actives, activeKeys, activeClassMaps);
                            break;
                    }
                }
            }
        }

        private void FindActivesFromReference(PdfIndirectReference iref, List<PdfIndirectReference> actives, HashSet2<RefKey> activeKeys)
        {
            RefKey refKey = new RefKey(iref);
            if ((!indirectObjects.TryGetValue(refKey, out var value) || !value.objecti.IsDictionary() || !ContainsInactivePg((PdfDictionary)value.objecti, activeKeys)) && !activeKeys.Contains(refKey))
            {
                activeKeys.Add(refKey);
                actives.Add(iref);
            }
        }

        private void FindActivesFromArray(PdfArray array, List<PdfIndirectReference> actives, HashSet2<RefKey> activeKeys, HashSet2<PdfName> activeClassMaps)
        {
            foreach (PdfObject item in array)
            {
                switch (item.Type)
                {
                    case 0:
                        FindActivesFromReference((PdfIndirectReference)item, actives, activeKeys);
                        break;
                    case 5:
                        FindActivesFromArray((PdfArray)item, actives, activeKeys, activeClassMaps);
                        break;
                    case 6:
                    case 7:
                        FindActivesFromDict((PdfDictionary)item, actives, activeKeys, activeClassMaps);
                        break;
                }
            }
        }

        private void FindActivesFromDict(PdfDictionary dict, List<PdfIndirectReference> actives, HashSet2<RefKey> activeKeys, HashSet2<PdfName> activeClassMaps)
        {
            if (ContainsInactivePg(dict, activeKeys))
            {
                return;
            }

            foreach (PdfName key in dict.Keys)
            {
                PdfObject pdfObject = dict.Get(key);
                if (key.Equals(PdfName.P))
                {
                    continue;
                }

                if (key.Equals(PdfName.C))
                {
                    if (pdfObject.IsArray())
                    {
                        foreach (PdfObject item in (PdfArray)pdfObject)
                        {
                            if (item.IsName())
                            {
                                activeClassMaps.Add((PdfName)item);
                            }
                        }
                    }
                    else if (pdfObject.IsName())
                    {
                        activeClassMaps.Add((PdfName)pdfObject);
                    }
                }
                else
                {
                    switch (pdfObject.Type)
                    {
                        case 0:
                            FindActivesFromReference((PdfIndirectReference)pdfObject, actives, activeKeys);
                            break;
                        case 5:
                            FindActivesFromArray((PdfArray)pdfObject, actives, activeKeys, activeClassMaps);
                            break;
                        case 6:
                        case 7:
                            FindActivesFromDict((PdfDictionary)pdfObject, actives, activeKeys, activeClassMaps);
                            break;
                    }
                }
            }
        }

        protected virtual void FlushIndirectObjects()
        {
            foreach (PdfIndirectObject savedObject in savedObjects)
            {
                indirectObjects.Remove(new RefKey(savedObject.Number, savedObject.Generation));
            }

            HashSet2<RefKey> hashSet = new HashSet2<RefKey>();
            foreach (KeyValuePair<RefKey, PdfIndirectObject> indirectObject in indirectObjects)
            {
                if (indirectObject.Value != null)
                {
                    WriteObjectToBody(indirectObject.Value);
                }
                else
                {
                    hashSet.Add(indirectObject.Key);
                }
            }

            List<PdfBody.PdfCrossReference> list = new List<PdfBody.PdfCrossReference>();
            foreach (PdfBody.PdfCrossReference key in body.xrefs.Keys)
            {
                list.Add(key);
            }

            foreach (PdfBody.PdfCrossReference item3 in list)
            {
                if (item3 != null)
                {
                    RefKey item2 = new RefKey(item3.Refnum, 0);
                    if (hashSet.Contains(item2))
                    {
                        body.xrefs.Remove(item3);
                    }
                }
            }

            indirectObjects = null;
        }

        private void WriteObjectToBody(PdfIndirectObject objecta)
        {
            bool flag = false;
            if (mergeFields)
            {
                UpdateAnnotationReferences(objecta.objecti);
                if (objecta.objecti.IsDictionary() || objecta.objecti.IsStream())
                {
                    PdfDictionary pdfDictionary = (PdfDictionary)objecta.objecti;
                    if (unmergedSet.Contains(objecta))
                    {
                        PdfNumber asNumber = pdfDictionary.GetAsNumber(annotId);
                        if (asNumber != null && mergedMap.ContainsKey(asNumber.IntValue))
                        {
                            flag = true;
                        }
                    }

                    if (mergedSet.Contains(objecta))
                    {
                        PdfNumber asNumber2 = pdfDictionary.GetAsNumber(annotId);
                        if (asNumber2 != null && unmergedMap.TryGetValue(asNumber2.IntValue, out var value) && value.objecti.IsDictionary())
                        {
                            PdfNumber asNumber3 = ((PdfDictionary)value.objecti).GetAsNumber(PdfName.STRUCTPARENT);
                            if (asNumber3 != null)
                            {
                                pdfDictionary.Put(PdfName.STRUCTPARENT, asNumber3);
                            }
                        }
                    }
                }
            }

            if (flag)
            {
                return;
            }

            PdfDictionary pdfDictionary2 = null;
            PdfNumber pdfNumber = null;
            if (mergeFields && objecta.objecti.IsDictionary())
            {
                pdfDictionary2 = (PdfDictionary)objecta.objecti;
                pdfNumber = pdfDictionary2.GetAsNumber(annotId);
                if (pdfNumber != null)
                {
                    pdfDictionary2.Remove(annotId);
                }
            }

            body.Add(objecta.objecti, objecta.Number, objecta.Generation, inObjStm: true);
            if (pdfNumber != null)
            {
                pdfDictionary2.Put(annotId, pdfNumber);
            }
        }

        private void UpdateAnnotationReferences(PdfObject obj)
        {
            if (obj.IsArray())
            {
                PdfArray pdfArray = (PdfArray)obj;
                for (int i = 0; i < pdfArray.Size; i++)
                {
                    PdfObject pdfObject = pdfArray.GetPdfObject(i);
                    if (pdfObject != null && pdfObject.Type == 0)
                    {
                        foreach (PdfIndirectObject item in unmergedSet)
                        {
                            if (item.IndirectReference.Number == ((PdfIndirectReference)pdfObject).Number && item.IndirectReference.Generation == ((PdfIndirectReference)pdfObject).Generation && item.objecti.IsDictionary())
                            {
                                PdfNumber asNumber = ((PdfDictionary)item.objecti).GetAsNumber(annotId);
                                if (asNumber != null && mergedMap.TryGetValue(asNumber.IntValue, out var value))
                                {
                                    pdfArray.Set(i, value.IndirectReference);
                                }
                            }
                        }
                    }
                    else
                    {
                        UpdateAnnotationReferences(pdfObject);
                    }
                }
            }
            else
            {
                if (!obj.IsDictionary() && !obj.IsStream())
                {
                    return;
                }

                PdfDictionary pdfDictionary = (PdfDictionary)obj;
                foreach (PdfName item2 in new List<PdfName>(pdfDictionary.Keys))
                {
                    PdfObject pdfObject2 = pdfDictionary.Get(item2);
                    if (pdfObject2 != null && pdfObject2.Type == 0)
                    {
                        foreach (PdfIndirectObject item3 in unmergedSet)
                        {
                            if (item3.IndirectReference.Number == ((PdfIndirectReference)pdfObject2).Number && item3.IndirectReference.Generation == ((PdfIndirectReference)pdfObject2).Generation && item3.objecti.IsDictionary())
                            {
                                PdfNumber asNumber2 = ((PdfDictionary)item3.objecti).GetAsNumber(annotId);
                                if (asNumber2 != null && mergedMap.TryGetValue(asNumber2.IntValue, out var value2))
                                {
                                    pdfDictionary.Put(item2, value2.IndirectReference);
                                }
                            }
                        }
                    }
                    else
                    {
                        UpdateAnnotationReferences(pdfObject2);
                    }
                }
            }
        }

        private void UpdateCalculationOrder(PdfReader reader)
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
                PdfObject pdfObject = asArray.GetPdfObject(i);
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

        private static string GetCOName(PdfReader reader, PRIndirectReference refa)
        {
            string text = "";
            while (refa != null)
            {
                PdfObject pdfObject = PdfReader.GetPdfObject(refa);
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

                refa = (PRIndirectReference)obj.Get(PdfName.PARENT);
            }

            if (text.EndsWith("."))
            {
                text = text.Substring(0, text.Length - 2);
            }

            return text;
        }

        private void MergeFields()
        {
            int num = 0;
            for (int i = 0; i < fields.Count; i++)
            {
                AcroFields acroFields = fields[i];
                IDictionary<string, AcroFields.Item> fd = acroFields.Fields;
                if (num < importedPages.Count && importedPages[num].reader == acroFields.reader)
                {
                    AddPageOffsetToField(fd, num);
                    num += acroFields.reader.NumberOfPages;
                }

                MergeWithMaster(fd);
            }
        }

        private void AddPageOffsetToField(IDictionary<string, AcroFields.Item> fd, int pageOffset)
        {
            if (pageOffset == 0)
            {
                return;
            }

            foreach (AcroFields.Item value in fd.Values)
            {
                for (int i = 0; i < value.Size; i++)
                {
                    int page = value.GetPage(i);
                    value.ForcePage(i, page + pageOffset);
                }
            }
        }

        private void MergeWithMaster(IDictionary<string, AcroFields.Item> fd)
        {
            foreach (KeyValuePair<string, AcroFields.Item> item in fd)
            {
                MergeField(item.Key, item.Value);
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
                    if (fieldKeys.Contains(key2))
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

        private void CreateWidgets(List<object> list, AcroFields.Item item)
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
                    if (widgetKeys.Contains(key))
                    {
                        pdfDictionary.Put(key, merged.Get(key));
                    }
                }

                pdfDictionary.Put(iTextTag, new PdfNumber(item.GetTabOrder(i) + 1));
                list.Add(pdfDictionary);
            }
        }

        private PdfObject Propagate(PdfObject obj)
        {
            if (obj == null)
            {
                return new PdfNull();
            }

            if (obj.IsArray())
            {
                PdfArray pdfArray = (PdfArray)obj;
                for (int i = 0; i < pdfArray.Size; i++)
                {
                    pdfArray.Set(i, Propagate(pdfArray.GetPdfObject(i)));
                }

                return pdfArray;
            }

            if (obj.IsDictionary() || obj.IsStream())
            {
                PdfDictionary pdfDictionary = (PdfDictionary)obj;
                {
                    foreach (PdfName item in new List<PdfName>(pdfDictionary.Keys))
                    {
                        pdfDictionary.Put(item, Propagate(pdfDictionary.Get(item)));
                    }

                    return pdfDictionary;
                }
            }

            if (obj.IsIndirect())
            {
                obj = PdfReader.GetPdfObject(obj);
                return AddToBody(Propagate(obj)).IndirectReference;
            }

            return obj;
        }

        private void CreateAcroForms()
        {
            if (fieldTree.Count == 0)
            {
                foreach (ImportedPage importedPage in importedPages)
                {
                    if (importedPage.mergedFields.Size > 0)
                    {
                        AddToBody(importedPage.mergedFields, importedPage.annotsIndirectReference);
                    }
                }

                return;
            }

            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.DR, Propagate(resources));
            if (needAppearances)
            {
                pdfDictionary.Put(PdfName.NEEDAPPEARANCES, PdfBoolean.PDFTRUE);
            }

            pdfDictionary.Put(PdfName.DA, new PdfString("/Helv 0 Tf 0 g "));
            tabOrder = new Dictionary<PdfArray, List<int>>();
            calculationOrderRefs = new List<object>();
            foreach (string item in calculationOrder)
            {
                calculationOrderRefs.Add(item);
            }

            pdfDictionary.Put(PdfName.FIELDS, BranchForm(fieldTree, null, ""));
            if (hasSignature)
            {
                pdfDictionary.Put(PdfName.SIGFLAGS, new PdfNumber(3));
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
                pdfDictionary.Put(PdfName.CO, pdfArray);
            }

            acroForm = AddToBody(pdfDictionary).IndirectReference;
            foreach (ImportedPage importedPage2 in importedPages)
            {
                AddToBody(importedPage2.mergedFields, importedPage2.annotsIndirectReference);
            }
        }

        private void UpdateReferences(PdfObject obj)
        {
            if (obj.IsDictionary() || obj.IsStream())
            {
                PdfDictionary pdfDictionary = (PdfDictionary)obj;
                foreach (PdfName item in new List<PdfName>(pdfDictionary.Keys))
                {
                    PdfObject pdfObject = pdfDictionary.Get(item);
                    if (pdfObject.IsIndirect())
                    {
                        PdfReader key = ((PRIndirectReference)pdfObject).Reader;
                        if (indirectMap[key].TryGetValue(new RefKey((PRIndirectReference)pdfObject), out var value))
                        {
                            pdfDictionary.Put(item, value.Ref);
                        }
                    }
                    else
                    {
                        UpdateReferences(pdfObject);
                    }
                }
            }
            else
            {
                if (!obj.IsArray())
                {
                    return;
                }

                PdfArray pdfArray = (PdfArray)obj;
                for (int i = 0; i < pdfArray.Size; i++)
                {
                    PdfObject pdfObject2 = pdfArray.GetPdfObject(i);
                    if (pdfObject2.IsIndirect())
                    {
                        PdfReader key2 = ((PRIndirectReference)pdfObject2).Reader;
                        if (indirectMap[key2].TryGetValue(new RefKey((PRIndirectReference)pdfObject2), out var value2))
                        {
                            pdfArray.Set(i, value2.Ref);
                        }
                    }
                    else
                    {
                        UpdateReferences(pdfObject2);
                    }
                }
            }
        }

        private PdfArray BranchForm(Dictionary<string, object> level, PdfIndirectReference parent, string fname)
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
                    AddToBody(pdfDictionary, pdfIndirectReference, formBranching: true);
                    continue;
                }

                List<object> list = (List<object>)value;
                pdfDictionary.MergeDifferent((PdfDictionary)list[0]);
                if (list.Count == 3)
                {
                    pdfDictionary.MergeDifferent((PdfDictionary)list[2]);
                    int num2 = (int)list[1];
                    PdfArray mergedFields = importedPages[num2 - 1].mergedFields;
                    PdfNumber nn = (PdfNumber)pdfDictionary.Get(iTextTag);
                    pdfDictionary.Remove(iTextTag);
                    pdfDictionary.Put(PdfName.TYPE, PdfName.ANNOT);
                    AdjustTabOrder(mergedFields, pdfIndirectReference, nn);
                }
                else
                {
                    PdfDictionary pdfDictionary2 = (PdfDictionary)list[0];
                    PdfArray pdfArray2 = new PdfArray();
                    for (int i = 1; i < list.Count; i += 2)
                    {
                        int num3 = (int)list[i];
                        PdfArray mergedFields2 = importedPages[num3 - 1].mergedFields;
                        PdfDictionary pdfDictionary3 = new PdfDictionary();
                        pdfDictionary3.Merge((PdfDictionary)list[i + 1]);
                        pdfDictionary3.Put(PdfName.PARENT, pdfIndirectReference);
                        PdfNumber nn2 = (PdfNumber)pdfDictionary3.Get(iTextTag);
                        pdfDictionary3.Remove(iTextTag);
                        if (IsTextField(pdfDictionary2))
                        {
                            PdfString asString = pdfDictionary2.GetAsString(PdfName.V);
                            PdfObject directObject = pdfDictionary3.GetDirectObject(PdfName.AP);
                            if (asString != null && directObject != null)
                            {
                                if (!mergedTextFields.ContainsKey(list))
                                {
                                    mergedTextFields[list] = asString;
                                }
                                else
                                {
                                    try
                                    {
                                        TextField textField = new TextField(this, null, null);
                                        fields[0].DecodeGenericDictionary(pdfDictionary3, textField);
                                        Rectangle rectangle = PdfReader.GetNormalizedRectangle(pdfDictionary3.GetAsArray(PdfName.RECT));
                                        if (textField.Rotation == 90 || textField.Rotation == 270)
                                        {
                                            rectangle = rectangle.Rotate();
                                        }

                                        textField.Box = rectangle;
                                        textField.Text = mergedTextFields[list].ToUnicodeString();
                                        PdfAppearance appearance = textField.GetAppearance();
                                        ((PdfDictionary)directObject).Put(PdfName.N, appearance.IndirectReference);
                                    }
                                    catch (DocumentException)
                                    {
                                    }
                                }
                            }
                        }
                        else if (IsCheckButton(pdfDictionary2))
                        {
                            PdfName asName = pdfDictionary2.GetAsName(PdfName.V);
                            PdfName asName2 = pdfDictionary3.GetAsName(PdfName.AS);
                            if (asName != null && asName2 != null)
                            {
                                pdfDictionary3.Put(PdfName.AS, asName);
                            }
                        }
                        else if (IsRadioButton(pdfDictionary2))
                        {
                            PdfName asName3 = pdfDictionary2.GetAsName(PdfName.V);
                            PdfName asName4 = pdfDictionary3.GetAsName(PdfName.AS);
                            if (asName3 != null && asName4 != null && !asName4.Equals(GetOffStateName(pdfDictionary3)))
                            {
                                if (!mergedRadioButtons.Contains(list))
                                {
                                    mergedRadioButtons.Add(list);
                                    pdfDictionary3.Put(PdfName.AS, asName3);
                                }
                                else
                                {
                                    pdfDictionary3.Put(PdfName.AS, GetOffStateName(pdfDictionary3));
                                }
                            }
                        }

                        pdfDictionary3.Put(PdfName.TYPE, PdfName.ANNOT);
                        PdfIndirectReference indirectReference = AddToBody(pdfDictionary3, PdfIndirectReference, formBranching: true).IndirectReference;
                        AdjustTabOrder(mergedFields2, indirectReference, nn2);
                        pdfArray2.Add(indirectReference);
                    }

                    pdfDictionary.Put(PdfName.KIDS, pdfArray2);
                }

                pdfArray.Add(pdfIndirectReference);
                AddToBody(pdfDictionary, pdfIndirectReference, formBranching: true);
            }

            return pdfArray;
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

        protected override PdfDictionary GetCatalog(PdfIndirectReference rootObj)
        {
            PdfDictionary catalog = pdf.GetCatalog(rootObj);
            BuildStructTreeRootForTagged(catalog);
            if (fieldArray != null)
            {
                AddFieldResources(catalog);
            }
            else if (mergeFields && acroForm != null)
            {
                catalog.Put(PdfName.ACROFORM, acroForm);
            }

            return catalog;
        }

        protected virtual bool IsStructTreeRootReference(PdfIndirectReference prRef)
        {
            if (prRef == null || structTreeRootReference == null)
            {
                return false;
            }

            if (prRef.Number == structTreeRootReference.Number)
            {
                return prRef.Generation == structTreeRootReference.Generation;
            }

            return false;
        }

        private void AddFieldResources(PdfDictionary catalog)
        {
            if (fieldArray == null)
            {
                return;
            }

            PdfDictionary pdfDictionary = new PdfDictionary();
            catalog.Put(PdfName.ACROFORM, pdfDictionary);
            pdfDictionary.Put(PdfName.FIELDS, fieldArray);
            pdfDictionary.Put(PdfName.DA, new PdfString("/Helv 0 Tf 0 g "));
            if (fieldTemplates.Count == 0)
            {
                return;
            }

            PdfDictionary pdfDictionary2 = new PdfDictionary();
            pdfDictionary.Put(PdfName.DR, pdfDictionary2);
            foreach (PdfTemplate fieldTemplate in fieldTemplates)
            {
                PdfFormField.MergeResources(pdfDictionary2, (PdfDictionary)fieldTemplate.Resources);
            }

            PdfDictionary pdfDictionary3 = pdfDictionary2.GetAsDict(PdfName.FONT);
            if (pdfDictionary3 == null)
            {
                pdfDictionary3 = new PdfDictionary();
                pdfDictionary2.Put(PdfName.FONT, pdfDictionary3);
            }

            if (!pdfDictionary3.Contains(PdfName.HELV))
            {
                PdfDictionary pdfDictionary4 = new PdfDictionary(PdfName.FONT);
                pdfDictionary4.Put(PdfName.BASEFONT, PdfName.HELVETICA);
                pdfDictionary4.Put(PdfName.ENCODING, PdfName.WIN_ANSI_ENCODING);
                pdfDictionary4.Put(PdfName.NAME, PdfName.HELV);
                pdfDictionary4.Put(PdfName.SUBTYPE, PdfName.TYPE1);
                pdfDictionary3.Put(PdfName.HELV, AddToBody(pdfDictionary4).IndirectReference);
            }

            if (!pdfDictionary3.Contains(PdfName.ZADB))
            {
                PdfDictionary pdfDictionary5 = new PdfDictionary(PdfName.FONT);
                pdfDictionary5.Put(PdfName.BASEFONT, PdfName.ZAPFDINGBATS);
                pdfDictionary5.Put(PdfName.NAME, PdfName.ZADB);
                pdfDictionary5.Put(PdfName.SUBTYPE, PdfName.TYPE1);
                pdfDictionary3.Put(PdfName.ZADB, AddToBody(pdfDictionary5).IndirectReference);
            }
        }

        public override void Close()
        {
            if (open)
            {
                pdf.Close();
                base.Close();
            }
        }

        public override void AddAnnotation(PdfAnnotation annot)
        {
        }

        internal override PdfIndirectReference Add(PdfPage page, PdfContents contents)
        {
            return null;
        }

        public override void FreeReader(PdfReader reader)
        {
            if (mergeFields)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("it.is.not.possible.to.free.reader.in.merge.fields.mode"));
            }

            PdfArray asArray = reader.trailer.GetAsArray(PdfName.ID);
            if (asArray != null)
            {
                originalFileID = asArray.GetAsString(0).GetBytes();
            }

            indirectMap.Remove(reader);
            currentPdfReaderInstance = null;
            base.FreeReader(reader);
        }

        protected virtual PdfName GetOffStateName(PdfDictionary widget)
        {
            return PdfName.Off_;
        }

        static PdfCopy()
        {
            COUNTER = CounterFactory.GetCounter(typeof(PdfCopy));
            annotId = new PdfName("iTextAnnotId");
            annotIdCnt = 0;
            iTextTag = new PdfName("_iTextTag_");
            zero = 0;
            widgetKeys = new HashSet2<PdfName>();
            fieldKeys = new HashSet2<PdfName>();
            widgetKeys.Add(PdfName.SUBTYPE);
            widgetKeys.Add(PdfName.CONTENTS);
            widgetKeys.Add(PdfName.RECT);
            widgetKeys.Add(PdfName.NM);
            widgetKeys.Add(PdfName.M);
            widgetKeys.Add(PdfName.F);
            widgetKeys.Add(PdfName.BS);
            widgetKeys.Add(PdfName.BORDER);
            widgetKeys.Add(PdfName.AP);
            widgetKeys.Add(PdfName.AS);
            widgetKeys.Add(PdfName.C);
            widgetKeys.Add(PdfName.A);
            widgetKeys.Add(PdfName.STRUCTPARENT);
            widgetKeys.Add(PdfName.OC);
            widgetKeys.Add(PdfName.H);
            widgetKeys.Add(PdfName.MK);
            widgetKeys.Add(PdfName.DA);
            widgetKeys.Add(PdfName.Q);
            widgetKeys.Add(PdfName.P);
            widgetKeys.Add(PdfName.TYPE);
            widgetKeys.Add(annotId);
            fieldKeys.Add(PdfName.AA);
            fieldKeys.Add(PdfName.FT);
            fieldKeys.Add(PdfName.TU);
            fieldKeys.Add(PdfName.TM);
            fieldKeys.Add(PdfName.FF);
            fieldKeys.Add(PdfName.V);
            fieldKeys.Add(PdfName.DV);
            fieldKeys.Add(PdfName.DS);
            fieldKeys.Add(PdfName.RV);
            fieldKeys.Add(PdfName.OPT);
            fieldKeys.Add(PdfName.MAXLEN);
            fieldKeys.Add(PdfName.TI);
            fieldKeys.Add(PdfName.I);
            fieldKeys.Add(PdfName.LOCK);
            fieldKeys.Add(PdfName.SV);
        }

        internal static int? GetFlags(PdfDictionary field)
        {
            PdfName asName = field.GetAsName(PdfName.FT);
            if (!PdfName.BTN.Equals(asName))
            {
                return null;
            }

            return field.GetAsNumber(PdfName.FF)?.IntValue;
        }

        internal static bool IsCheckButton(PdfDictionary field)
        {
            int? flags = GetFlags(field);
            if (flags.HasValue)
            {
                if ((flags.Value & 0x10000) == 0)
                {
                    return (flags.Value & 0x8000) == 0;
                }

                return false;
            }

            return true;
        }

        internal static bool IsRadioButton(PdfDictionary field)
        {
            int? flags = GetFlags(field);
            if (flags.HasValue && (flags.Value & 0x10000) == 0)
            {
                return (flags.Value & 0x8000) != 0;
            }

            return false;
        }

        internal static bool IsTextField(PdfDictionary field)
        {
            PdfName asName = field.GetAsName(PdfName.FT);
            return PdfName.TX.Equals(asName);
        }

        public virtual PageStamp CreatePageStamp(PdfImportedPage iPage)
        {
            int pageNumber = iPage.PageNumber;
            PdfReader pdfReader = iPage.PdfReaderInstance.Reader;
            if (IsTagged())
            {
                throw new Exception(MessageLocalization.GetComposedMessage("creating.page.stamp.not.allowed.for.tagged.reader"));
            }

            PdfDictionary pageN = pdfReader.GetPageN(pageNumber);
            return new PageStamp(pdfReader, pageN, this);
        }
    }
}
