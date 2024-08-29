using Sign.itext.error_messages;
using Sign.itext.io;
using Sign.itext.text;
using Sign.itext.text.exceptions;
using Sign.itext.text.log;
using Sign.itext.text.pdf;
using Sign.itext.text.pdf.interfaces;
using Sign.itext.text.pdf.intern;
using Sign.Org.BouncyCastle.Cms;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Security;
using Sign.Org.BouncyCastle.X509;
using Sign.SystemItext.util;
using Sign.SystemItext.util.zlib;
using System.Text;

namespace Sign.itext.pdf
{
    public class PdfReader : IPdfViewerPreferences, IDisposable
    {
        public class PageRefs
        {
            private PdfReader reader;

            private IntHashtable refsp;

            private List<PRIndirectReference> refsn;

            private List<PdfDictionary> pageInh;

            private int lastPageRead = -1;

            private int sizep;

            private bool keepPages;

            internal int Size
            {
                get
                {
                    if (refsn != null)
                    {
                        return refsn.Count;
                    }

                    return sizep;
                }
            }

            internal PageRefs(PdfReader reader)
            {
                this.reader = reader;
                if (reader.partial)
                {
                    refsp = new IntHashtable();
                    PdfNumber pdfNumber = (PdfNumber)GetPdfObjectRelease(reader.rootPages.Get(PdfName.COUNT));
                    sizep = pdfNumber.IntValue;
                }
                else
                {
                    ReadPages();
                }
            }

            internal PageRefs(PageRefs other, PdfReader reader)
            {
                this.reader = reader;
                sizep = other.sizep;
                if (other.refsn != null)
                {
                    refsn = new List<PRIndirectReference>(other.refsn);
                    for (int i = 0; i < refsn.Count; i++)
                    {
                        refsn[i] = (PRIndirectReference)DuplicatePdfObject(refsn[i], reader);
                    }
                }
                else
                {
                    refsp = other.refsp.Clone();
                }
            }

            internal void ReadPages()
            {
                if (refsn == null)
                {
                    refsp = null;
                    refsn = new List<PRIndirectReference>();
                    pageInh = new List<PdfDictionary>();
                    IteratePages((PRIndirectReference)reader.catalog.Get(PdfName.PAGES));
                    pageInh = null;
                    reader.rootPages.Put(PdfName.COUNT, new PdfNumber(refsn.Count));
                }
            }

            internal void ReReadPages()
            {
                refsn = null;
                ReadPages();
            }

            public virtual PdfDictionary GetPageN(int pageNum)
            {
                return (PdfDictionary)GetPdfObject(GetPageOrigRef(pageNum));
            }

            public virtual PdfDictionary GetPageNRelease(int pageNum)
            {
                PdfDictionary pageN = GetPageN(pageNum);
                ReleasePage(pageNum);
                return pageN;
            }

            public virtual PRIndirectReference GetPageOrigRefRelease(int pageNum)
            {
                PRIndirectReference pageOrigRef = GetPageOrigRef(pageNum);
                ReleasePage(pageNum);
                return pageOrigRef;
            }

            public virtual PRIndirectReference GetPageOrigRef(int pageNum)
            {
                pageNum--;
                if (pageNum < 0 || pageNum >= Size)
                {
                    return null;
                }

                if (refsn != null)
                {
                    return refsn[pageNum];
                }

                int num = refsp[pageNum];
                if (num == 0)
                {
                    PRIndirectReference singlePage = GetSinglePage(pageNum);
                    if (reader.lastXrefPartial == -1)
                    {
                        lastPageRead = -1;
                    }
                    else
                    {
                        lastPageRead = pageNum;
                    }

                    reader.lastXrefPartial = -1;
                    refsp[pageNum] = singlePage.Number;
                    if (keepPages)
                    {
                        lastPageRead = -1;
                    }

                    return singlePage;
                }

                if (lastPageRead != pageNum)
                {
                    lastPageRead = -1;
                }

                if (keepPages)
                {
                    lastPageRead = -1;
                }

                return new PRIndirectReference(reader, num);
            }

            internal void KeepPages()
            {
                if (refsp != null && !keepPages)
                {
                    keepPages = true;
                    refsp.Clear();
                }
            }

            public virtual void ReleasePage(int pageNum)
            {
                if (refsp != null)
                {
                    pageNum--;
                    if (pageNum >= 0 && pageNum < Size && pageNum == lastPageRead)
                    {
                        lastPageRead = -1;
                        reader.lastXrefPartial = refsp[pageNum];
                        reader.ReleaseLastXrefPartial();
                        refsp.Remove(pageNum);
                    }
                }
            }

            public virtual void ResetReleasePage()
            {
                if (refsp != null)
                {
                    lastPageRead = -1;
                }
            }

            internal void InsertPage(int pageNum, PRIndirectReference refi)
            {
                pageNum--;
                if (refsn != null)
                {
                    if (pageNum >= refsn.Count)
                    {
                        refsn.Add(refi);
                    }
                    else
                    {
                        refsn.Insert(pageNum, refi);
                    }

                    return;
                }

                sizep++;
                lastPageRead = -1;
                if (pageNum >= Size)
                {
                    refsp[Size] = refi.Number;
                    return;
                }

                IntHashtable intHashtable = new IntHashtable((refsp.Size + 1) * 2);
                IntHashtable.IntHashtableIterator entryIterator = refsp.GetEntryIterator();
                while (entryIterator.HasNext())
                {
                    IntHashtable.IntHashtableEntry intHashtableEntry = entryIterator.Next();
                    int key = intHashtableEntry.Key;
                    intHashtable[(key >= pageNum) ? (key + 1) : key] = intHashtableEntry.Value;
                }

                intHashtable[pageNum] = refi.Number;
                refsp = intHashtable;
            }

            private void PushPageAttributes(PdfDictionary nodePages)
            {
                PdfDictionary pdfDictionary = new PdfDictionary();
                if (pageInh.Count != 0)
                {
                    pdfDictionary.Merge(pageInh[pageInh.Count - 1]);
                }

                for (int i = 0; i < pageInhCandidates.Length; i++)
                {
                    PdfObject pdfObject = nodePages.Get(pageInhCandidates[i]);
                    if (pdfObject != null)
                    {
                        pdfDictionary.Put(pageInhCandidates[i], pdfObject);
                    }
                }

                pageInh.Add(pdfDictionary);
            }

            private void PopPageAttributes()
            {
                pageInh.RemoveAt(pageInh.Count - 1);
            }

            private void IteratePages(PRIndirectReference rpage)
            {
                PdfDictionary pdfDictionary = (PdfDictionary)GetPdfObject(rpage);
                if (pdfDictionary == null)
                {
                    return;
                }

                PdfArray asArray = pdfDictionary.GetAsArray(PdfName.KIDS);
                if (asArray == null)
                {
                    pdfDictionary.Put(PdfName.TYPE, PdfName.PAGE);
                    PdfDictionary pdfDictionary2 = pageInh[pageInh.Count - 1];
                    foreach (PdfName key in pdfDictionary2.Keys)
                    {
                        if (pdfDictionary.Get(key) == null)
                        {
                            pdfDictionary.Put(key, pdfDictionary2.Get(key));
                        }
                    }

                    if (pdfDictionary.Get(PdfName.MEDIABOX) == null)
                    {
                        PdfArray value = new PdfArray(new float[4]
                        {
                            0f,
                            0f,
                            PageSize.LETTER.Right,
                            PageSize.LETTER.Top
                        });
                        pdfDictionary.Put(PdfName.MEDIABOX, value);
                    }

                    refsn.Add(rpage);
                    return;
                }

                pdfDictionary.Put(PdfName.TYPE, PdfName.PAGES);
                PushPageAttributes(pdfDictionary);
                for (int i = 0; i < asArray.Size; i++)
                {
                    PdfObject pdfObject = asArray.GetPdfObject(i);
                    if (!pdfObject.IsIndirect())
                    {
                        while (i < asArray.Size)
                        {
                            asArray.Remove(i);
                        }

                        break;
                    }

                    IteratePages((PRIndirectReference)pdfObject);
                }

                PopPageAttributes();
            }

            protected internal virtual PRIndirectReference GetSinglePage(int n)
            {
                PdfDictionary pdfDictionary = new PdfDictionary();
                PdfDictionary pdfDictionary2 = reader.rootPages;
                int num = 0;
                while (true)
                {
                    for (int i = 0; i < pageInhCandidates.Length; i++)
                    {
                        PdfObject pdfObject = pdfDictionary2.Get(pageInhCandidates[i]);
                        if (pdfObject != null)
                        {
                            pdfDictionary.Put(pageInhCandidates[i], pdfObject);
                        }
                    }

                    ListIterator<PdfObject> listIterator = new ListIterator<PdfObject>(((PdfArray)GetPdfObjectRelease(pdfDictionary2.Get(PdfName.KIDS))).ArrayList);
                    while (listIterator.HasNext())
                    {
                        PRIndirectReference pRIndirectReference = (PRIndirectReference)listIterator.Next();
                        PdfDictionary pdfDictionary3 = (PdfDictionary)GetPdfObject(pRIndirectReference);
                        int lastXrefPartial = reader.lastXrefPartial;
                        PdfObject pdfObjectRelease = GetPdfObjectRelease(pdfDictionary3.Get(PdfName.COUNT));
                        reader.lastXrefPartial = lastXrefPartial;
                        int num2 = 1;
                        if (pdfObjectRelease != null && pdfObjectRelease.Type == 2)
                        {
                            num2 = ((PdfNumber)pdfObjectRelease).IntValue;
                        }

                        if (n < num + num2)
                        {
                            if (pdfObjectRelease == null)
                            {
                                pdfDictionary3.MergeDifferent(pdfDictionary);
                                return pRIndirectReference;
                            }

                            reader.ReleaseLastXrefPartial();
                            pdfDictionary2 = pdfDictionary3;
                            break;
                        }

                        reader.ReleaseLastXrefPartial();
                        num += num2;
                    }
                }
            }

            internal void SelectPages(ICollection<int> pagesToKeep)
            {
                IntHashtable intHashtable = new IntHashtable();
                List<int> list = new List<int>();
                int size = Size;
                foreach (int item in pagesToKeep)
                {
                    if (item >= 1 && item <= size && !intHashtable.ContainsKey(item))
                    {
                        intHashtable[item] = 1;
                        list.Add(item);
                    }
                }

                if (reader.partial)
                {
                    for (int i = 1; i <= size; i++)
                    {
                        GetPageOrigRef(i);
                        ResetReleasePage();
                    }
                }

                PRIndirectReference pRIndirectReference = (PRIndirectReference)reader.catalog.Get(PdfName.PAGES);
                PdfDictionary pdfDictionary = (PdfDictionary)GetPdfObject(pRIndirectReference);
                List<PRIndirectReference> list2 = new List<PRIndirectReference>(list.Count);
                PdfArray pdfArray = new PdfArray();
                foreach (int item2 in list)
                {
                    PRIndirectReference pageOrigRef = GetPageOrigRef(item2);
                    ResetReleasePage();
                    pdfArray.Add(pageOrigRef);
                    list2.Add(pageOrigRef);
                    GetPageN(item2).Put(PdfName.PARENT, pRIndirectReference);
                }

                AcroFields acroFields = reader.AcroFields;
                bool flag = acroFields.Fields.Count > 0;
                for (int j = 1; j <= size; j++)
                {
                    if (!intHashtable.ContainsKey(j))
                    {
                        if (flag)
                        {
                            acroFields.RemoveFieldsFromPage(j);
                        }

                        int number = GetPageOrigRef(j).Number;
                        reader.xrefObj[number] = null;
                        if (reader.partial)
                        {
                            reader.xref[number * 2] = -1L;
                            reader.xref[number * 2 + 1] = 0L;
                        }
                    }
                }

                pdfDictionary.Put(PdfName.COUNT, new PdfNumber(list.Count));
                pdfDictionary.Put(PdfName.KIDS, pdfArray);
                refsp = null;
                refsn = list2;
            }
        }

        public static bool unethicalreading = false;

        public static bool debugmode = false;

        private static ILogger LOGGER = LoggerFactory.GetLogger(typeof(PdfReader));

        private static PdfName[] pageInhCandidates = new PdfName[4]
        {
            PdfName.MEDIABOX,
            PdfName.ROTATE,
            PdfName.RESOURCES,
            PdfName.CROPBOX
        };

        private static byte[] endstream = PdfEncodings.ConvertToBytes("endstream", null);

        private static byte[] endobj = PdfEncodings.ConvertToBytes("endobj", null);

        protected internal PRTokeniser tokens;

        protected internal long[] xref;

        protected internal Dictionary<int, IntHashtable> objStmMark;

        protected internal LongHashtable objStmToOffset;

        protected internal bool newXrefType;

        protected List<PdfObject> xrefObj;

        private PdfDictionary rootPages;

        protected internal PdfDictionary trailer;

        protected internal PdfDictionary catalog;

        protected internal PageRefs pageRefs;

        protected internal PRAcroForm acroForm;

        protected internal bool acroFormParsed;

        protected internal bool encrypted;

        protected internal bool rebuilt;

        protected internal int freeXref;

        protected internal bool tampered;

        protected internal long lastXref;

        protected internal long eofPos;

        protected internal char pdfVersion;

        protected internal PdfEncryption decrypt;

        protected internal byte[] password;

        protected ICipherParameters certificateKey;

        protected X509Certificate certificate;

        private bool ownerPasswordUsed;

        protected internal List<PdfString> strings = new List<PdfString>();

        protected internal bool sharedStreams = true;

        protected internal bool consolidateNamedDestinations;

        protected bool remoteToLocalNamedDestinations;

        protected internal int rValue;

        protected internal long pValue;

        private int objNum;

        private int objGen;

        private long fileLength;

        private bool hybridXref;

        private int lastXrefPartial = -1;

        private bool partial;

        private PRIndirectReference cryptoRef;

        private PdfViewerPreferencesImp viewerPreferences = new PdfViewerPreferencesImp();

        private bool encryptionError;

        private bool appendable;

        protected static ICounter COUNTER = CounterFactory.GetCounter(typeof(PdfReader));

        private int readDepth;

        public virtual RandomAccessFileOrArray SafeFile => tokens.SafeFile;

        public virtual int NumberOfPages => pageRefs.Size;

        public virtual PdfDictionary Catalog => catalog;

        public virtual PRAcroForm AcroForm
        {
            get
            {
                if (!acroFormParsed)
                {
                    acroFormParsed = true;
                    PdfObject pdfObject = catalog.Get(PdfName.ACROFORM);
                    if (pdfObject != null)
                    {
                        try
                        {
                            acroForm = new PRAcroForm(this);
                            acroForm.ReadAcroForm((PdfDictionary)GetPdfObject(pdfObject));
                        }
                        catch
                        {
                            acroForm = null;
                        }
                    }
                }

                return acroForm;
            }
        }

        public virtual Dictionary<string, string> Info
        {
            get
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                PdfDictionary asDict = trailer.GetAsDict(PdfName.INFO);
                if (asDict == null)
                {
                    return dictionary;
                }

                foreach (PdfName key in asDict.Keys)
                {
                    PdfObject pdfObject = GetPdfObject(asDict.Get(key));
                    if (pdfObject != null)
                    {
                        string text = pdfObject.ToString();
                        switch (pdfObject.Type)
                        {
                            case 3:
                                text = ((PdfString)pdfObject).ToUnicodeString();
                                break;
                            case 4:
                                text = PdfName.DecodeName(text);
                                break;
                        }

                        dictionary[PdfName.DecodeName(key.ToString())] = text;
                    }
                }

                return dictionary;
            }
        }

        public virtual bool Tampered
        {
            get
            {
                return tampered;
            }
            set
            {
                tampered = value;
                pageRefs.KeepPages();
            }
        }

        public virtual byte[] Metadata
        {
            get
            {
                PdfObject pdfObject = GetPdfObject(catalog.Get(PdfName.METADATA));
                if (!(pdfObject is PRStream))
                {
                    return null;
                }

                RandomAccessFileOrArray safeFile = SafeFile;
                byte[] array = null;
                try
                {
                    safeFile.ReOpen();
                    return GetStreamBytes((PRStream)pdfObject, safeFile);
                }
                finally
                {
                    try
                    {
                        safeFile.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public virtual long LastXref => lastXref;

        public virtual int XrefSize => xrefObj.Count;

        public virtual long EofPos => eofPos;

        public virtual char PdfVersion => pdfVersion;

        public virtual long Permissions => pValue;

        public virtual PdfDictionary Trailer => trailer;

        internal PdfEncryption Decrypt => decrypt;

        public virtual AcroFields AcroFields => new AcroFields(this, null);

        public virtual string JavaScript
        {
            get
            {
                RandomAccessFileOrArray safeFile = SafeFile;
                try
                {
                    safeFile.ReOpen();
                    return GetJavaScript(safeFile);
                }
                finally
                {
                    try
                    {
                        safeFile.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public virtual int ViewerPreferences
        {
            set
            {
                viewerPreferences.ViewerPreferences = value;
                SetViewerPreferences(viewerPreferences);
            }
        }

        public virtual int SimpleViewerPreferences => PdfViewerPreferencesImp.GetViewerPreferences(catalog).PageLayoutAndMode;

        public virtual bool Appendable
        {
            get
            {
                return appendable;
            }
            set
            {
                appendable = value;
                if (appendable)
                {
                    GetPdfObject(trailer.Get(PdfName.ROOT));
                }
            }
        }

        public virtual long FileLength => fileLength;

        public bool IsOpenedWithFullPermissions
        {
            get
            {
                if (encrypted && !ownerPasswordUsed)
                {
                    return unethicalreading;
                }

                return true;
            }
        }

        protected virtual ICounter GetCounter()
        {
            return COUNTER;
        }

        protected internal PdfReader()
        {
        }

        private PdfReader(IRandomAccessSource byteSource, bool partialRead, byte[] ownerPassword, X509Certificate certificate, ICipherParameters certificateKey, bool closeSourceOnConstructorError)
        {
            this.certificate = certificate;
            this.certificateKey = certificateKey;
            password = ownerPassword;
            partial = partialRead;
            try
            {
                tokens = GetOffsetTokeniser(byteSource);
                if (partialRead)
                {
                    ReadPdfPartial();
                }
                else
                {
                    ReadPdf();
                }
            }
            catch (IOException ex)
            {
                if (closeSourceOnConstructorError)
                {
                    byteSource.Close();
                }

                throw ex;
            }

            GetCounter().Read(fileLength);
        }

        public PdfReader(string filename)
            : this(filename, null)
        {
        }

        public PdfReader(string filename, byte[] ownerPassword)
            : this(filename, ownerPassword, partial: false)
        {
        }

        public PdfReader(string filename, byte[] ownerPassword, bool partial)
            : this(new RandomAccessSourceFactory().SetForceRead(forceRead: false).CreateBestSource(filename), partial, ownerPassword, null, null, closeSourceOnConstructorError: true)
        {
        }

        public PdfReader(byte[] pdfIn)
            : this(pdfIn, null)
        {
        }

        public PdfReader(byte[] pdfIn, byte[] ownerPassword)
            : this(new RandomAccessSourceFactory().CreateSource(pdfIn), partialRead: false, ownerPassword, null, null, closeSourceOnConstructorError: true)
        {
        }

        public PdfReader(string filename, X509Certificate certificate, ICipherParameters certificateKey)
            : this(new RandomAccessSourceFactory().SetForceRead(forceRead: false).CreateBestSource(filename), partialRead: false, null, certificate, certificateKey, closeSourceOnConstructorError: true)
        {
        }

        public PdfReader(Uri url)
            : this(url, null)
        {
        }

        public PdfReader(Uri url, byte[] ownerPassword)
            : this(new RandomAccessSourceFactory().CreateSource(url), partialRead: false, ownerPassword, null, null, closeSourceOnConstructorError: true)
        {
        }

        public PdfReader(Stream isp, byte[] ownerPassword)
            : this(new RandomAccessSourceFactory().CreateSource(isp), partialRead: false, ownerPassword, null, null, closeSourceOnConstructorError: false)
        {
        }

        public PdfReader(Stream isp)
            : this(isp, null)
        {
        }

        [Obsolete("Use the constructor that takes a RandomAccessFileOrArray")]
        public PdfReader(RandomAccessFileOrArray raf, byte[] ownerPassword)
            : this(raf.GetByteSource(), partialRead: true, ownerPassword, null, null, closeSourceOnConstructorError: false)
        {
        }

        public PdfReader(PdfReader reader)
        {
            appendable = reader.appendable;
            consolidateNamedDestinations = reader.consolidateNamedDestinations;
            encrypted = reader.encrypted;
            rebuilt = reader.rebuilt;
            sharedStreams = reader.sharedStreams;
            tampered = reader.tampered;
            password = reader.password;
            pdfVersion = reader.pdfVersion;
            eofPos = reader.eofPos;
            freeXref = reader.freeXref;
            lastXref = reader.lastXref;
            newXrefType = reader.newXrefType;
            tokens = new PRTokeniser(reader.tokens.SafeFile);
            if (reader.decrypt != null)
            {
                decrypt = new PdfEncryption(reader.decrypt);
            }

            pValue = reader.pValue;
            rValue = reader.rValue;
            xrefObj = new List<PdfObject>(reader.xrefObj);
            for (int i = 0; i < reader.xrefObj.Count; i++)
            {
                xrefObj[i] = DuplicatePdfObject(reader.xrefObj[i], this);
            }

            pageRefs = new PageRefs(reader.pageRefs, this);
            trailer = (PdfDictionary)DuplicatePdfObject(reader.trailer, this);
            catalog = trailer.GetAsDict(PdfName.ROOT);
            rootPages = catalog.GetAsDict(PdfName.PAGES);
            fileLength = reader.fileLength;
            partial = reader.partial;
            hybridXref = reader.hybridXref;
            objStmToOffset = reader.objStmToOffset;
            xref = reader.xref;
            cryptoRef = (PRIndirectReference)DuplicatePdfObject(reader.cryptoRef, this);
            ownerPasswordUsed = reader.ownerPasswordUsed;
        }

        private static PRTokeniser GetOffsetTokeniser(IRandomAccessSource byteSource)
        {
            PRTokeniser pRTokeniser = new PRTokeniser(new RandomAccessFileOrArray(byteSource));
            int headerOffset = pRTokeniser.GetHeaderOffset();
            if (headerOffset != 0)
            {
                pRTokeniser = new PRTokeniser(new RandomAccessFileOrArray(new WindowRandomAccessSource(byteSource, headerOffset)));
            }

            return pRTokeniser;
        }

        protected internal virtual PdfReaderInstance GetPdfReaderInstance(PdfWriter writer)
        {
            return new PdfReaderInstance(this, writer);
        }

        public virtual int GetPageRotation(int index)
        {
            return GetPageRotation(pageRefs.GetPageNRelease(index));
        }

        internal int GetPageRotation(PdfDictionary page)
        {
            PdfNumber asNumber = page.GetAsNumber(PdfName.ROTATE);
            if (asNumber == null)
            {
                return 0;
            }

            int intValue = asNumber.IntValue;
            intValue %= 360;
            if (intValue >= 0)
            {
                return intValue;
            }

            return intValue + 360;
        }

        public virtual Rectangle GetPageSizeWithRotation(int index)
        {
            return GetPageSizeWithRotation(pageRefs.GetPageNRelease(index));
        }

        public virtual Rectangle GetPageSizeWithRotation(PdfDictionary page)
        {
            Rectangle rectangle = GetPageSize(page);
            for (int num = GetPageRotation(page); num > 0; num -= 90)
            {
                rectangle = rectangle.Rotate();
            }

            return rectangle;
        }

        public virtual Rectangle GetPageSize(int index)
        {
            return GetPageSize(pageRefs.GetPageNRelease(index));
        }

        public virtual Rectangle GetPageSize(PdfDictionary page)
        {
            return GetNormalizedRectangle(page.GetAsArray(PdfName.MEDIABOX));
        }

        public virtual Rectangle GetCropBox(int index)
        {
            PdfDictionary pageNRelease = pageRefs.GetPageNRelease(index);
            PdfArray pdfArray = (PdfArray)GetPdfObjectRelease(pageNRelease.Get(PdfName.CROPBOX));
            if (pdfArray == null)
            {
                return GetPageSize(pageNRelease);
            }

            return GetNormalizedRectangle(pdfArray);
        }

        public virtual Rectangle GetBoxSize(int index, string boxName)
        {
            PdfDictionary pageNRelease = pageRefs.GetPageNRelease(index);
            PdfArray pdfArray = null;
            if (boxName.Equals("trim"))
            {
                pdfArray = (PdfArray)GetPdfObjectRelease(pageNRelease.Get(PdfName.TRIMBOX));
            }
            else if (boxName.Equals("art"))
            {
                pdfArray = (PdfArray)GetPdfObjectRelease(pageNRelease.Get(PdfName.ARTBOX));
            }
            else if (boxName.Equals("bleed"))
            {
                pdfArray = (PdfArray)GetPdfObjectRelease(pageNRelease.Get(PdfName.BLEEDBOX));
            }
            else if (boxName.Equals("crop"))
            {
                pdfArray = (PdfArray)GetPdfObjectRelease(pageNRelease.Get(PdfName.CROPBOX));
            }
            else if (boxName.Equals("media"))
            {
                pdfArray = (PdfArray)GetPdfObjectRelease(pageNRelease.Get(PdfName.MEDIABOX));
            }

            if (pdfArray == null)
            {
                return null;
            }

            return GetNormalizedRectangle(pdfArray);
        }

        public static Rectangle GetNormalizedRectangle(PdfArray box)
        {
            float floatValue = ((PdfNumber)GetPdfObjectRelease(box.GetPdfObject(0))).FloatValue;
            float floatValue2 = ((PdfNumber)GetPdfObjectRelease(box.GetPdfObject(1))).FloatValue;
            float floatValue3 = ((PdfNumber)GetPdfObjectRelease(box.GetPdfObject(2))).FloatValue;
            float floatValue4 = ((PdfNumber)GetPdfObjectRelease(box.GetPdfObject(3))).FloatValue;
            return new Rectangle(Math.Min(floatValue, floatValue3), Math.Min(floatValue2, floatValue4), Math.Max(floatValue, floatValue3), Math.Max(floatValue2, floatValue4));
        }

        public virtual bool IsTagged()
        {
            PdfDictionary asDict = catalog.GetAsDict(PdfName.MARKINFO);
            if (asDict == null)
            {
                return false;
            }

            if (PdfBoolean.PDFTRUE.Equals(asDict.GetAsBoolean(PdfName.MARKED)))
            {
                return catalog.GetAsDict(PdfName.STRUCTTREEROOT) != null;
            }

            return false;
        }

        protected internal virtual void ReadPdf()
        {
            fileLength = tokens.File.Length;
            pdfVersion = tokens.CheckPdfHeader();
            try
            {
                ReadXref();
            }
            catch (Exception ex2)
            {
                try
                {
                    rebuilt = true;
                    RebuildXref();
                    lastXref = -1L;
                }
                catch (Exception ex)
                {
                    throw new InvalidPdfException(MessageLocalization.GetComposedMessage("rebuild.failed.1.original.message.2", ex.Message, ex2.Message));
                }
            }

            try
            {
                ReadDocObj();
            }
            catch (Exception ex3)
            {
                if (ex3 is BadPasswordException)
                {
                    throw new BadPasswordException(ex3.Message);
                }

                if (rebuilt || encryptionError)
                {
                    throw new InvalidPdfException(ex3.Message);
                }

                rebuilt = true;
                encrypted = false;
                try
                {
                    RebuildXref();
                    lastXref = -1L;
                    ReadDocObj();
                }
                catch (Exception ex4)
                {
                    throw new InvalidPdfException(MessageLocalization.GetComposedMessage("rebuild.failed.1.original.message.2", ex4.Message, ex3.Message));
                }
            }

            strings.Clear();
            ReadPages();
            RemoveUnusedObjects();
        }

        protected internal virtual void ReadPdfPartial()
        {
            fileLength = tokens.File.Length;
            pdfVersion = tokens.CheckPdfHeader();
            try
            {
                ReadXref();
            }
            catch (Exception ex2)
            {
                try
                {
                    rebuilt = true;
                    RebuildXref();
                    lastXref = -1L;
                }
                catch (Exception ex)
                {
                    throw new InvalidPdfException(MessageLocalization.GetComposedMessage("rebuild.failed.1.original.message.2", ex.Message, ex2.Message), ex);
                }
            }

            ReadDocObjPartial();
            ReadPages();
        }

        private bool EqualsArray(byte[] ar1, byte[] ar2, int size)
        {
            for (int i = 0; i < size; i++)
            {
                if (ar1[i] != ar2[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void ReadDecryptedDocObj()
        {
            if (encrypted)
            {
                return;
            }

            PdfObject pdfObject = trailer.Get(PdfName.ENCRYPT);
            if (pdfObject == null || pdfObject.ToString().Equals("null"))
            {
                return;
            }

            encryptionError = true;
            byte[] array = null;
            encrypted = true;
            PdfDictionary pdfDictionary = (PdfDictionary)GetPdfObject(pdfObject);
            PdfArray asArray = trailer.GetAsArray(PdfName.ID);
            byte[] array2 = null;
            if (asArray != null)
            {
                PdfObject pdfObject2 = asArray.GetPdfObject(0);
                strings.Remove((PdfString)pdfObject2);
                array2 = DocWriter.GetISOBytes(pdfObject2.ToString());
                if (asArray.Size > 1)
                {
                    strings.Remove((PdfString)asArray.GetPdfObject(1));
                }
            }

            if (array2 == null)
            {
                array2 = new byte[0];
            }

            byte[] array3 = null;
            byte[] ownerKey = null;
            int num = 0;
            int num2 = 0;
            PdfObject pdfObjectRelease = GetPdfObjectRelease(pdfDictionary.Get(PdfName.FILTER));
            if (pdfObjectRelease.Equals(PdfName.STANDARD))
            {
                string text = pdfDictionary.Get(PdfName.U).ToString();
                strings.Remove((PdfString)pdfDictionary.Get(PdfName.U));
                array3 = DocWriter.GetISOBytes(text);
                string text2 = pdfDictionary.Get(PdfName.O).ToString();
                strings.Remove((PdfString)pdfDictionary.Get(PdfName.O));
                ownerKey = DocWriter.GetISOBytes(text2);
                if (pdfDictionary.Contains(PdfName.OE))
                {
                    strings.Remove((PdfString)pdfDictionary.Get(PdfName.OE));
                }

                if (pdfDictionary.Contains(PdfName.UE))
                {
                    strings.Remove((PdfString)pdfDictionary.Get(PdfName.UE));
                }

                if (pdfDictionary.Contains(PdfName.PERMS))
                {
                    strings.Remove((PdfString)pdfDictionary.Get(PdfName.PERMS));
                }

                PdfObject pdfObject2 = pdfDictionary.Get(PdfName.P);
                if (!pdfObject2.IsNumber())
                {
                    throw new InvalidPdfException(MessageLocalization.GetComposedMessage("illegal.p.value"));
                }

                pValue = ((PdfNumber)pdfObject2).LongValue;
                pdfObject2 = pdfDictionary.Get(PdfName.R);
                if (!pdfObject2.IsNumber())
                {
                    throw new InvalidPdfException(MessageLocalization.GetComposedMessage("illegal.r.value"));
                }

                rValue = ((PdfNumber)pdfObject2).IntValue;
                switch (rValue)
                {
                    case 2:
                        num = 0;
                        break;
                    case 3:
                        pdfObject2 = pdfDictionary.Get(PdfName.LENGTH);
                        if (!pdfObject2.IsNumber())
                        {
                            throw new InvalidPdfException(MessageLocalization.GetComposedMessage("illegal.length.value"));
                        }

                        num2 = ((PdfNumber)pdfObject2).IntValue;
                        if (num2 > 128 || num2 < 40 || num2 % 8 != 0)
                        {
                            throw new InvalidPdfException(MessageLocalization.GetComposedMessage("illegal.length.value"));
                        }

                        num = 1;
                        break;
                    case 4:
                        {
                            PdfDictionary pdfDictionary2 = (PdfDictionary)pdfDictionary.Get(PdfName.CF);
                            if (pdfDictionary2 == null)
                            {
                                throw new InvalidPdfException(MessageLocalization.GetComposedMessage("cf.not.found.encryption"));
                            }

                            pdfDictionary2 = (PdfDictionary)pdfDictionary2.Get(PdfName.STDCF);
                            if (pdfDictionary2 == null)
                            {
                                throw new InvalidPdfException(MessageLocalization.GetComposedMessage("stdcf.not.found.encryption"));
                            }

                            if (PdfName.V2.Equals(pdfDictionary2.Get(PdfName.CFM)))
                            {
                                num = 1;
                            }
                            else
                            {
                                if (!PdfName.AESV2.Equals(pdfDictionary2.Get(PdfName.CFM)))
                                {
                                    throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("no.compatible.encryption.found"));
                                }

                                num = 2;
                            }

                            PdfObject pdfObject4 = pdfDictionary.Get(PdfName.ENCRYPTMETADATA);
                            if (pdfObject4 != null && pdfObject4.ToString().Equals("false"))
                            {
                                num |= 8;
                            }

                            break;
                        }
                    case 5:
                        {
                            num = 3;
                            PdfObject pdfObject3 = pdfDictionary.Get(PdfName.ENCRYPTMETADATA);
                            if (pdfObject3 != null && pdfObject3.ToString().Equals("false"))
                            {
                                num |= 8;
                            }

                            break;
                        }
                    default:
                        throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("unknown.encryption.type.r.eq.1", rValue));
                }
            }
            else if (pdfObjectRelease.Equals(PdfName.PUBSEC))
            {
                bool flag = false;
                byte[] array4 = null;
                PdfArray pdfArray = null;
                PdfObject pdfObject2 = pdfDictionary.Get(PdfName.V);
                if (!pdfObject2.IsNumber())
                {
                    throw new InvalidPdfException(MessageLocalization.GetComposedMessage("illegal.v.value"));
                }

                int intValue = ((PdfNumber)pdfObject2).IntValue;
                switch (intValue)
                {
                    case 1:
                        num = 0;
                        num2 = 40;
                        pdfArray = (PdfArray)pdfDictionary.Get(PdfName.RECIPIENTS);
                        break;
                    case 2:
                        pdfObject2 = pdfDictionary.Get(PdfName.LENGTH);
                        if (!pdfObject2.IsNumber())
                        {
                            throw new InvalidPdfException(MessageLocalization.GetComposedMessage("illegal.length.value"));
                        }

                        num2 = ((PdfNumber)pdfObject2).IntValue;
                        if (num2 > 128 || num2 < 40 || num2 % 8 != 0)
                        {
                            throw new InvalidPdfException(MessageLocalization.GetComposedMessage("illegal.length.value"));
                        }

                        num = 1;
                        pdfArray = (PdfArray)pdfDictionary.Get(PdfName.RECIPIENTS);
                        break;
                    case 4:
                    case 5:
                        {
                            PdfDictionary pdfDictionary3 = (PdfDictionary)pdfDictionary.Get(PdfName.CF);
                            if (pdfDictionary3 == null)
                            {
                                throw new InvalidPdfException(MessageLocalization.GetComposedMessage("cf.not.found.encryption"));
                            }

                            pdfDictionary3 = (PdfDictionary)pdfDictionary3.Get(PdfName.DEFAULTCRYPTFILTER);
                            if (pdfDictionary3 == null)
                            {
                                throw new InvalidPdfException(MessageLocalization.GetComposedMessage("defaultcryptfilter.not.found.encryption"));
                            }

                            if (PdfName.V2.Equals(pdfDictionary3.Get(PdfName.CFM)))
                            {
                                num = 1;
                                num2 = 128;
                            }
                            else if (PdfName.AESV2.Equals(pdfDictionary3.Get(PdfName.CFM)))
                            {
                                num = 2;
                                num2 = 128;
                            }
                            else
                            {
                                if (!PdfName.AESV3.Equals(pdfDictionary3.Get(PdfName.CFM)))
                                {
                                    throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("no.compatible.encryption.found"));
                                }

                                num = 3;
                                num2 = 256;
                            }

                            PdfObject pdfObject5 = pdfDictionary3.Get(PdfName.ENCRYPTMETADATA);
                            if (pdfObject5 != null && pdfObject5.ToString().Equals("false"))
                            {
                                num |= 8;
                            }

                            pdfArray = (PdfArray)pdfDictionary3.Get(PdfName.RECIPIENTS);
                            break;
                        }
                    default:
                        throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("unknown.encryption.type.v.eq.1", intValue));
                }

                for (int i = 0; i < pdfArray.Size; i++)
                {
                    PdfObject pdfObject6 = pdfArray.GetPdfObject(i);
                    if (pdfObject6 is PdfString)
                    {
                        strings.Remove((PdfString)pdfObject6);
                    }

                    foreach (RecipientInformation recipient in new CmsEnvelopedData(pdfObject6.GetBytes()).GetRecipientInfos().GetRecipients())
                    {
                        if (recipient.RecipientID.Match(certificate) && !flag)
                        {
                            array4 = recipient.GetContent(certificateKey);
                            flag = true;
                        }
                    }
                }

                if (!flag || array4 == null)
                {
                    throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("bad.certificate.and.key"));
                }

                IDigest digest = (((num & 7) != 3) ? DigestUtilities.GetDigest("SHA-1") : DigestUtilities.GetDigest("SHA-256"));
                digest.BlockUpdate(array4, 0, 20);
                for (int j = 0; j < pdfArray.Size; j++)
                {
                    byte[] bytes = pdfArray.GetPdfObject(j).GetBytes();
                    digest.BlockUpdate(bytes, 0, bytes.Length);
                }

                if (((uint)num & 8u) != 0)
                {
                    digest.BlockUpdate(PdfEncryption.metadataPad, 0, PdfEncryption.metadataPad.Length);
                }

                array = new byte[digest.GetDigestSize()];
                digest.DoFinal(array, 0);
            }

            decrypt = new PdfEncryption();
            decrypt.SetCryptoMode(num, num2);
            if (pdfObjectRelease.Equals(PdfName.STANDARD))
            {
                if (rValue == 5)
                {
                    ownerPasswordUsed = decrypt.ReadKey(pdfDictionary, password);
                    pValue = decrypt.GetPermissions();
                }
                else
                {
                    decrypt.SetupByOwnerPassword(array2, password, array3, ownerKey, pValue);
                    if (!EqualsArray(array3, decrypt.userKey, (rValue == 3 || rValue == 4) ? 16 : 32))
                    {
                        decrypt.SetupByUserPassword(array2, password, ownerKey, pValue);
                        if (!EqualsArray(array3, decrypt.userKey, (rValue == 3 || rValue == 4) ? 16 : 32))
                        {
                            throw new BadPasswordException(MessageLocalization.GetComposedMessage("bad.user.password"));
                        }
                    }
                    else
                    {
                        ownerPasswordUsed = true;
                    }
                }
            }
            else if (pdfObjectRelease.Equals(PdfName.PUBSEC))
            {
                if ((num & 7) == 3)
                {
                    decrypt.SetKey(array);
                }
                else
                {
                    decrypt.SetupByEncryptionKey(array, num2);
                }

                ownerPasswordUsed = true;
            }

            for (int k = 0; k < strings.Count; k++)
            {
                strings[k].Decrypt(this);
            }

            if (pdfObject.IsIndirect())
            {
                cryptoRef = (PRIndirectReference)pdfObject;
                xrefObj[cryptoRef.Number] = null;
            }

            encryptionError = false;
        }

        public static PdfObject GetPdfObjectRelease(PdfObject obj)
        {
            PdfObject pdfObject = GetPdfObject(obj);
            ReleaseLastXrefPartial(obj);
            return pdfObject;
        }

        public static PdfObject GetPdfObject(PdfObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (!obj.IsIndirect())
            {
                return obj;
            }

            PRIndirectReference pRIndirectReference = (PRIndirectReference)obj;
            int number = pRIndirectReference.Number;
            bool flag = pRIndirectReference.Reader.appendable;
            obj = pRIndirectReference.Reader.GetPdfObject(number);
            if (obj == null)
            {
                return null;
            }

            if (flag)
            {
                switch (obj.Type)
                {
                    case 8:
                        obj = new PdfNull();
                        break;
                    case 1:
                        obj = new PdfBoolean(((PdfBoolean)obj).BooleanValue);
                        break;
                    case 4:
                        obj = new PdfName(obj.GetBytes());
                        break;
                }

                obj.IndRef = pRIndirectReference;
            }

            return obj;
        }

        public static PdfObject GetPdfObjectRelease(PdfObject obj, PdfObject parent)
        {
            PdfObject pdfObject = GetPdfObject(obj, parent);
            ReleaseLastXrefPartial(obj);
            return pdfObject;
        }

        public static PdfObject GetPdfObject(PdfObject obj, PdfObject parent)
        {
            if (obj == null)
            {
                return null;
            }

            if (!obj.IsIndirect())
            {
                PRIndirectReference pRIndirectReference = null;
                if (parent != null && (pRIndirectReference = parent.IndRef) != null && pRIndirectReference.Reader.Appendable)
                {
                    switch (obj.Type)
                    {
                        case 8:
                            obj = new PdfNull();
                            break;
                        case 1:
                            obj = new PdfBoolean(((PdfBoolean)obj).BooleanValue);
                            break;
                        case 4:
                            obj = new PdfName(obj.GetBytes());
                            break;
                    }

                    obj.IndRef = pRIndirectReference;
                }

                return obj;
            }

            return GetPdfObject(obj);
        }

        public virtual PdfObject GetPdfObjectRelease(int idx)
        {
            PdfObject pdfObject = GetPdfObject(idx);
            ReleaseLastXrefPartial();
            return pdfObject;
        }

        public virtual PdfObject GetPdfObject(int idx)
        {
            lastXrefPartial = -1;
            if (idx < 0 || idx >= xrefObj.Count)
            {
                return null;
            }

            PdfObject pdfObject = xrefObj[idx];
            if (!partial || pdfObject != null)
            {
                return pdfObject;
            }

            if (idx * 2 >= xref.Length)
            {
                return null;
            }

            pdfObject = ReadSingleObject(idx);
            lastXrefPartial = -1;
            if (pdfObject != null)
            {
                lastXrefPartial = idx;
            }

            return pdfObject;
        }

        public virtual void ResetLastXrefPartial()
        {
            lastXrefPartial = -1;
        }

        public virtual void ReleaseLastXrefPartial()
        {
            if (partial && lastXrefPartial != -1)
            {
                xrefObj[lastXrefPartial] = null;
                lastXrefPartial = -1;
            }
        }

        public static void ReleaseLastXrefPartial(PdfObject obj)
        {
            if (obj != null && obj.IsIndirect() && obj is PRIndirectReference)
            {
                PRIndirectReference pRIndirectReference = (PRIndirectReference)obj;
                PdfReader reader = pRIndirectReference.Reader;
                if (reader.partial && reader.lastXrefPartial != -1 && reader.lastXrefPartial == pRIndirectReference.Number)
                {
                    reader.xrefObj[reader.lastXrefPartial] = null;
                }

                reader.lastXrefPartial = -1;
            }
        }

        private void SetXrefPartialObject(int idx, PdfObject obj)
        {
            if (partial && idx >= 0)
            {
                xrefObj[idx] = obj;
            }
        }

        public virtual PRIndirectReference AddPdfObject(PdfObject obj)
        {
            xrefObj.Add(obj);
            return new PRIndirectReference(this, xrefObj.Count - 1);
        }

        protected internal virtual void ReadPages()
        {
            catalog = trailer.GetAsDict(PdfName.ROOT);
            if (catalog == null)
            {
                throw new InvalidPdfException(MessageLocalization.GetComposedMessage("the.document.has.no.catalog.object"));
            }

            rootPages = catalog.GetAsDict(PdfName.PAGES);
            if (rootPages == null)
            {
                throw new InvalidPdfException(MessageLocalization.GetComposedMessage("the.document.has.no.page.root"));
            }

            pageRefs = new PageRefs(this);
        }

        protected internal virtual void ReadDocObjPartial()
        {
            xrefObj = new List<PdfObject>(xref.Length / 2);
            for (int i = 0; i < xref.Length / 2; i++)
            {
                xrefObj.Add(null);
            }

            ReadDecryptedDocObj();
            if (objStmToOffset != null)
            {
                long[] keys = objStmToOffset.GetKeys();
                foreach (long num in keys)
                {
                    objStmToOffset[num] = xref[num * 2];
                    xref[num * 2] = -1L;
                }
            }
        }

        protected internal virtual PdfObject ReadSingleObject(int k)
        {
            strings.Clear();
            int num = k * 2;
            long num2 = xref[num];
            if (num2 < 0)
            {
                return null;
            }

            if (xref[num + 1] > 0)
            {
                num2 = objStmToOffset[xref[num + 1]];
            }

            if (num2 == 0L)
            {
                return null;
            }

            tokens.Seek(num2);
            tokens.NextValidToken();
            if (tokens.TokenType != PRTokeniser.TokType.NUMBER)
            {
                tokens.ThrowError(MessageLocalization.GetComposedMessage("invalid.object.number"));
            }

            objNum = tokens.IntValue;
            tokens.NextValidToken();
            if (tokens.TokenType != PRTokeniser.TokType.NUMBER)
            {
                tokens.ThrowError(MessageLocalization.GetComposedMessage("invalid.generation.number"));
            }

            objGen = tokens.IntValue;
            tokens.NextValidToken();
            if (!tokens.StringValue.Equals("obj"))
            {
                tokens.ThrowError(MessageLocalization.GetComposedMessage("token.obj.expected"));
            }

            PdfObject pdfObject;
            try
            {
                pdfObject = ReadPRObject();
                for (int i = 0; i < strings.Count; i++)
                {
                    strings[i].Decrypt(this);
                }

                if (pdfObject.IsStream())
                {
                    CheckPRStreamLength((PRStream)pdfObject);
                }
            }
            catch (IOException ex)
            {
                if (!debugmode)
                {
                    throw ex;
                }

                if (LOGGER.IsLogging(Level.ERROR))
                {
                    LOGGER.Error(ex.Message, ex);
                }

                pdfObject = null;
            }

            if (xref[num + 1] > 0)
            {
                pdfObject = ReadOneObjStm((PRStream)pdfObject, (int)xref[num]);
            }

            xrefObj[k] = pdfObject;
            return pdfObject;
        }

        protected internal virtual PdfObject ReadOneObjStm(PRStream stream, int idx)
        {
            int intValue = stream.GetAsNumber(PdfName.FIRST).IntValue;
            byte[] streamBytes = GetStreamBytes(stream, tokens.File);
            PRTokeniser pRTokeniser = tokens;
            tokens = new PRTokeniser(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(streamBytes)));
            try
            {
                int num = 0;
                bool flag = true;
                idx++;
                for (int i = 0; i < idx; i++)
                {
                    flag = tokens.NextToken();
                    if (!flag)
                    {
                        break;
                    }

                    if (tokens.TokenType != PRTokeniser.TokType.NUMBER)
                    {
                        flag = false;
                        break;
                    }

                    flag = tokens.NextToken();
                    if (!flag)
                    {
                        break;
                    }

                    if (tokens.TokenType != PRTokeniser.TokType.NUMBER)
                    {
                        flag = false;
                        break;
                    }

                    num = tokens.IntValue + intValue;
                }

                if (!flag)
                {
                    throw new InvalidPdfException(MessageLocalization.GetComposedMessage("error.reading.objstm"));
                }

                tokens.Seek(num);
                tokens.NextToken();
                if (tokens.TokenType == PRTokeniser.TokType.NUMBER)
                {
                    return new PdfNumber(tokens.StringValue);
                }

                tokens.Seek(num);
                return ReadPRObject();
            }
            finally
            {
                tokens = pRTokeniser;
            }
        }

        public virtual double DumpPerc()
        {
            int num = 0;
            for (int i = 0; i < xrefObj.Count; i++)
            {
                if (xrefObj[i] != null)
                {
                    num++;
                }
            }

            return (double)num * 100.0 / (double)xrefObj.Count;
        }

        protected internal virtual void ReadDocObj()
        {
            List<PRStream> list = new List<PRStream>();
            xrefObj = new List<PdfObject>(xref.Length / 2);
            for (int i = 0; i < xref.Length / 2; i++)
            {
                xrefObj.Add(null);
            }

            for (int j = 2; j < xref.Length; j += 2)
            {
                long num = xref[j];
                if (num <= 0 || xref[j + 1] > 0)
                {
                    continue;
                }

                tokens.Seek(num);
                tokens.NextValidToken();
                if (tokens.TokenType != PRTokeniser.TokType.NUMBER)
                {
                    tokens.ThrowError(MessageLocalization.GetComposedMessage("invalid.object.number"));
                }

                objNum = tokens.IntValue;
                tokens.NextValidToken();
                if (tokens.TokenType != PRTokeniser.TokType.NUMBER)
                {
                    tokens.ThrowError(MessageLocalization.GetComposedMessage("invalid.generation.number"));
                }

                objGen = tokens.IntValue;
                tokens.NextValidToken();
                if (!tokens.StringValue.Equals("obj"))
                {
                    tokens.ThrowError(MessageLocalization.GetComposedMessage("token.obj.expected"));
                }

                PdfObject pdfObject;
                try
                {
                    pdfObject = ReadPRObject();
                    if (pdfObject.IsStream())
                    {
                        list.Add((PRStream)pdfObject);
                    }
                }
                catch (IOException ex)
                {
                    if (!debugmode)
                    {
                        throw ex;
                    }

                    if (LOGGER.IsLogging(Level.ERROR))
                    {
                        LOGGER.Error(ex.Message, ex);
                    }

                    pdfObject = null;
                }

                xrefObj[j / 2] = pdfObject;
            }

            for (int k = 0; k < list.Count; k++)
            {
                CheckPRStreamLength(list[k]);
            }

            ReadDecryptedDocObj();
            if (objStmMark != null)
            {
                foreach (KeyValuePair<int, IntHashtable> item in objStmMark)
                {
                    int key = item.Key;
                    IntHashtable value = item.Value;
                    ReadObjStm((PRStream)xrefObj[key], value);
                    xrefObj[key] = null;
                }

                objStmMark = null;
            }

            xref = null;
        }

        private void CheckPRStreamLength(PRStream stream)
        {
            long length = tokens.Length;
            long offset = stream.Offset;
            bool flag = false;
            long num = 0L;
            PdfObject pdfObjectRelease = GetPdfObjectRelease(stream.Get(PdfName.LENGTH));
            if (pdfObjectRelease != null && pdfObjectRelease.Type == 2)
            {
                num = ((PdfNumber)pdfObjectRelease).IntValue;
                if (num + offset > length - 20)
                {
                    flag = true;
                }
                else
                {
                    tokens.Seek(offset + num);
                    string text = tokens.ReadString(20);
                    if (!text.StartsWith("\nendstream") && !text.StartsWith("\r\nendstream") && !text.StartsWith("\rendstream") && !text.StartsWith("endstream"))
                    {
                        flag = true;
                    }
                }
            }
            else
            {
                flag = true;
            }

            if (flag)
            {
                byte[] array = new byte[16];
                tokens.Seek(offset);
                long num2;
                while (true)
                {
                    num2 = tokens.FilePointer;
                    if (!tokens.ReadLineSegment(array, isNullWhitespace: false))
                    {
                        break;
                    }

                    if (Equalsn(array, endstream))
                    {
                        num = num2 - offset;
                        break;
                    }

                    if (Equalsn(array, endobj))
                    {
                        tokens.Seek(num2 - 16);
                        int num3 = tokens.ReadString(16).IndexOf("endstream");
                        if (num3 >= 0)
                        {
                            num2 = num2 - 16 + num3;
                        }

                        num = num2 - offset;
                        break;
                    }
                }

                tokens.Seek(num2 - 2);
                if (tokens.Read() == 13)
                {
                    num--;
                }

                tokens.Seek(num2 - 1);
                if (tokens.Read() == 10)
                {
                    num--;
                }

                if (num < 0)
                {
                    num = 0L;
                }
            }

            stream.Length = (int)num;
        }

        protected internal virtual void ReadObjStm(PRStream stream, IntHashtable map)
        {
            if (stream == null)
            {
                return;
            }

            int intValue = stream.GetAsNumber(PdfName.FIRST).IntValue;
            int intValue2 = stream.GetAsNumber(PdfName.N).IntValue;
            byte[] streamBytes = GetStreamBytes(stream, tokens.File);
            PRTokeniser pRTokeniser = tokens;
            tokens = new PRTokeniser(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(streamBytes)));
            try
            {
                int[] array = new int[intValue2];
                int[] array2 = new int[intValue2];
                bool flag = true;
                for (int i = 0; i < intValue2; i++)
                {
                    flag = tokens.NextToken();
                    if (!flag)
                    {
                        break;
                    }

                    if (tokens.TokenType != PRTokeniser.TokType.NUMBER)
                    {
                        flag = false;
                        break;
                    }

                    array2[i] = tokens.IntValue;
                    flag = tokens.NextToken();
                    if (!flag)
                    {
                        break;
                    }

                    if (tokens.TokenType != PRTokeniser.TokType.NUMBER)
                    {
                        flag = false;
                        break;
                    }

                    array[i] = tokens.IntValue + intValue;
                }

                if (!flag)
                {
                    throw new InvalidPdfException(MessageLocalization.GetComposedMessage("error.reading.objstm"));
                }

                for (int j = 0; j < intValue2; j++)
                {
                    if (map.ContainsKey(j))
                    {
                        tokens.Seek(array[j]);
                        tokens.NextToken();
                        PdfObject value;
                        if (tokens.TokenType == PRTokeniser.TokType.NUMBER)
                        {
                            value = new PdfNumber(tokens.StringValue);
                        }
                        else
                        {
                            tokens.Seek(array[j]);
                            value = ReadPRObject();
                        }

                        xrefObj[array2[j]] = value;
                    }
                }
            }
            finally
            {
                tokens = pRTokeniser;
            }
        }

        public static PdfObject KillIndirect(PdfObject obj)
        {
            if (obj == null || obj.IsNull())
            {
                return null;
            }

            PdfObject pdfObjectRelease = GetPdfObjectRelease(obj);
            if (obj.IsIndirect())
            {
                PRIndirectReference obj2 = (PRIndirectReference)obj;
                PdfReader reader = obj2.Reader;
                int number = obj2.Number;
                reader.xrefObj[number] = null;
                if (reader.partial)
                {
                    reader.xref[number * 2] = -1L;
                }
            }

            return pdfObjectRelease;
        }

        private void EnsureXrefSize(int size)
        {
            if (size != 0)
            {
                if (xref == null)
                {
                    xref = new long[size];
                }
                else if (xref.Length < size)
                {
                    long[] destinationArray = new long[size];
                    Array.Copy(xref, 0, destinationArray, 0, xref.Length);
                    xref = destinationArray;
                }
            }
        }

        protected internal virtual void ReadXref()
        {
            hybridXref = false;
            newXrefType = false;
            tokens.Seek(tokens.GetStartxref());
            tokens.NextToken();
            if (!tokens.StringValue.Equals("startxref"))
            {
                throw new InvalidPdfException(MessageLocalization.GetComposedMessage("startxref.not.found"));
            }

            tokens.NextToken();
            if (tokens.TokenType != PRTokeniser.TokType.NUMBER)
            {
                throw new InvalidPdfException(MessageLocalization.GetComposedMessage("startxref.is.not.followed.by.a.number"));
            }

            long num = (lastXref = tokens.LongValue);
            eofPos = tokens.FilePointer;
            try
            {
                if (ReadXRefStream(num))
                {
                    newXrefType = true;
                    return;
                }
            }
            catch
            {
            }

            xref = null;
            tokens.Seek(num);
            trailer = ReadXrefSection();
            PdfDictionary pdfDictionary = trailer;
            while (true)
            {
                PdfNumber pdfNumber = (PdfNumber)pdfDictionary.Get(PdfName.PREV);
                if (pdfNumber != null)
                {
                    tokens.Seek(pdfNumber.LongValue);
                    pdfDictionary = ReadXrefSection();
                    continue;
                }

                break;
            }
        }

        protected internal virtual PdfDictionary ReadXrefSection()
        {
            tokens.NextValidToken();
            if (!tokens.StringValue.Equals("xref"))
            {
                tokens.ThrowError(MessageLocalization.GetComposedMessage("xref.subsection.not.found"));
            }

            int num = 0;
            int num2 = 0;
            long num3 = 0L;
            int num4 = 0;
            while (true)
            {
                tokens.NextValidToken();
                if (tokens.StringValue.Equals("trailer"))
                {
                    break;
                }

                if (tokens.TokenType != PRTokeniser.TokType.NUMBER)
                {
                    tokens.ThrowError(MessageLocalization.GetComposedMessage("object.number.of.the.first.object.in.this.xref.subsection.not.found"));
                }

                num = tokens.IntValue;
                tokens.NextValidToken();
                if (tokens.TokenType != PRTokeniser.TokType.NUMBER)
                {
                    tokens.ThrowError(MessageLocalization.GetComposedMessage("number.of.entries.in.this.xref.subsection.not.found"));
                }

                num2 = tokens.IntValue + num;
                if (num == 1)
                {
                    long filePointer = tokens.FilePointer;
                    tokens.NextValidToken();
                    num3 = tokens.LongValue;
                    tokens.NextValidToken();
                    num4 = tokens.IntValue;
                    if (num3 == 0L && num4 == 65535)
                    {
                        num--;
                        num2--;
                    }

                    tokens.Seek(filePointer);
                }

                EnsureXrefSize(num2 * 2);
                for (int i = num; i < num2; i++)
                {
                    tokens.NextValidToken();
                    num3 = tokens.LongValue;
                    tokens.NextValidToken();
                    num4 = tokens.IntValue;
                    tokens.NextValidToken();
                    int num5 = i * 2;
                    if (tokens.StringValue.Equals("n"))
                    {
                        if (xref[num5] == 0L && xref[num5 + 1] == 0L)
                        {
                            xref[num5] = num3;
                        }
                    }
                    else if (tokens.StringValue.Equals("f"))
                    {
                        if (xref[num5] == 0L && xref[num5 + 1] == 0L)
                        {
                            xref[num5] = -1L;
                        }
                    }
                    else
                    {
                        tokens.ThrowError(MessageLocalization.GetComposedMessage("invalid.cross.reference.entry.in.this.xref.subsection"));
                    }
                }
            }

            PdfDictionary pdfDictionary = (PdfDictionary)ReadPRObject();
            PdfNumber pdfNumber = (PdfNumber)pdfDictionary.Get(PdfName.SIZE);
            EnsureXrefSize(pdfNumber.IntValue * 2);
            PdfObject pdfObject = pdfDictionary.Get(PdfName.XREFSTM);
            if (pdfObject != null && pdfObject.IsNumber())
            {
                int intValue = ((PdfNumber)pdfObject).IntValue;
                try
                {
                    ReadXRefStream(intValue);
                    newXrefType = true;
                    hybridXref = true;
                    return pdfDictionary;
                }
                catch (IOException ex)
                {
                    xref = null;
                    throw ex;
                }
            }

            return pdfDictionary;
        }

        protected internal virtual bool ReadXRefStream(long ptr)
        {
            tokens.Seek(ptr);
            int num = 0;
            if (!tokens.NextToken())
            {
                return false;
            }

            if (tokens.TokenType != PRTokeniser.TokType.NUMBER)
            {
                return false;
            }

            num = tokens.IntValue;
            if (!tokens.NextToken() || tokens.TokenType != PRTokeniser.TokType.NUMBER)
            {
                return false;
            }

            if (!tokens.NextToken() || !tokens.StringValue.Equals("obj"))
            {
                return false;
            }

            PdfObject pdfObject = ReadPRObject();
            PRStream pRStream = null;
            if (pdfObject.IsStream())
            {
                pRStream = (PRStream)pdfObject;
                if (!PdfName.XREF.Equals(pRStream.Get(PdfName.TYPE)))
                {
                    return false;
                }

                if (trailer == null)
                {
                    trailer = new PdfDictionary();
                    trailer.Merge(pRStream);
                }

                pRStream.Length = ((PdfNumber)pRStream.Get(PdfName.LENGTH)).IntValue;
                int intValue = ((PdfNumber)pRStream.Get(PdfName.SIZE)).IntValue;
                PdfObject pdfObject2 = pRStream.Get(PdfName.INDEX);
                PdfArray pdfArray;
                if (pdfObject2 == null)
                {
                    pdfArray = new PdfArray();
                    pdfArray.Add(new int[2] { 0, intValue });
                }
                else
                {
                    pdfArray = (PdfArray)pdfObject2;
                }

                PdfArray pdfArray2 = (PdfArray)pRStream.Get(PdfName.W);
                long num2 = -1L;
                pdfObject2 = pRStream.Get(PdfName.PREV);
                if (pdfObject2 != null)
                {
                    num2 = ((PdfNumber)pdfObject2).LongValue;
                }

                EnsureXrefSize(intValue * 2);
                if (objStmMark == null && !partial)
                {
                    objStmMark = new Dictionary<int, IntHashtable>();
                }

                if (objStmToOffset == null && partial)
                {
                    objStmToOffset = new LongHashtable();
                }

                byte[] streamBytes = GetStreamBytes(pRStream, tokens.File);
                int num3 = 0;
                int[] array = new int[3];
                for (int i = 0; i < 3; i++)
                {
                    array[i] = pdfArray2.GetAsNumber(i).IntValue;
                }

                for (int j = 0; j < pdfArray.Size; j += 2)
                {
                    int num4 = pdfArray.GetAsNumber(j).IntValue;
                    int intValue2 = pdfArray.GetAsNumber(j + 1).IntValue;
                    EnsureXrefSize((num4 + intValue2) * 2);
                    while (intValue2-- > 0)
                    {
                        int num5 = 1;
                        if (array[0] > 0)
                        {
                            num5 = 0;
                            for (int k = 0; k < array[0]; k++)
                            {
                                num5 = (num5 << 8) + (streamBytes[num3++] & 0xFF);
                            }
                        }

                        long num6 = 0L;
                        for (int l = 0; l < array[1]; l++)
                        {
                            num6 = (num6 << 8) + (streamBytes[num3++] & 0xFF);
                        }

                        int num7 = 0;
                        for (int m = 0; m < array[2]; m++)
                        {
                            num7 = (num7 << 8) + (streamBytes[num3++] & 0xFF);
                        }

                        int num8 = num4 * 2;
                        if (xref[num8] == 0L && xref[num8 + 1] == 0L)
                        {
                            switch (num5)
                            {
                                case 0:
                                    xref[num8] = -1L;
                                    break;
                                case 1:
                                    xref[num8] = num6;
                                    break;
                                case 2:
                                    {
                                        xref[num8] = num7;
                                        xref[num8 + 1] = num6;
                                        IntHashtable value;
                                        if (partial)
                                        {
                                            objStmToOffset[num6] = 0L;
                                        }
                                        else if (!objStmMark.TryGetValue((int)num6, out value))
                                        {
                                            value = new IntHashtable();
                                            value[num7] = 1;
                                            objStmMark[(int)num6] = value;
                                        }
                                        else
                                        {
                                            value[num7] = 1;
                                        }

                                        break;
                                    }
                            }
                        }

                        num4++;
                    }
                }

                num *= 2;
                if (num + 1 < xref.Length && xref[num] == 0L && xref[num + 1] == 0L)
                {
                    xref[num] = -1L;
                }

                if (num2 == -1)
                {
                    return true;
                }

                return ReadXRefStream(num2);
            }

            return false;
        }

        protected internal virtual void RebuildXref()
        {
            hybridXref = false;
            newXrefType = false;
            tokens.Seek(0L);
            long[][] array = new long[1024][];
            long num = 0L;
            trailer = null;
            byte[] array2 = new byte[64];
            while (true)
            {
                long filePointer = tokens.FilePointer;
                if (!tokens.ReadLineSegment(array2, isNullWhitespace: true))
                {
                    break;
                }

                if (array2[0] == 116)
                {
                    if (!PdfEncodings.ConvertToString(array2, null).StartsWith("trailer"))
                    {
                        continue;
                    }

                    tokens.Seek(filePointer);
                    tokens.NextToken();
                    filePointer = tokens.FilePointer;
                    try
                    {
                        PdfDictionary pdfDictionary = (PdfDictionary)ReadPRObject();
                        if (pdfDictionary.Get(PdfName.ROOT) != null)
                        {
                            trailer = pdfDictionary;
                        }
                        else
                        {
                            tokens.Seek(filePointer);
                        }
                    }
                    catch
                    {
                        tokens.Seek(filePointer);
                    }
                }
                else
                {
                    if (array2[0] < 48 || array2[0] > 57)
                    {
                        continue;
                    }

                    long[] array3 = PRTokeniser.CheckObjectStart(array2);
                    if (array3 != null)
                    {
                        long num2 = array3[0];
                        long num3 = array3[1];
                        if (num2 >= array.Length)
                        {
                            long[][] array4 = new long[num2 * 2][];
                            Array.Copy(array, 0L, array4, 0L, num);
                            array = array4;
                        }

                        if (num2 >= num)
                        {
                            num = num2 + 1;
                        }

                        if (array[num2] == null || num3 >= array[num2][1])
                        {
                            array3[0] = filePointer;
                            array[num2] = array3;
                        }
                    }
                }
            }

            if (trailer == null)
            {
                throw new InvalidPdfException(MessageLocalization.GetComposedMessage("trailer.not.found"));
            }

            xref = new long[num * 2];
            for (int i = 0; i < num; i++)
            {
                long[] array5 = array[i];
                if (array5 != null)
                {
                    xref[i * 2] = array5[0];
                }
            }
        }

        protected internal virtual PdfDictionary ReadDictionary()
        {
            PdfDictionary pdfDictionary = new PdfDictionary();
            while (true)
            {
                tokens.NextValidToken();
                if (tokens.TokenType == PRTokeniser.TokType.END_DIC)
                {
                    break;
                }

                if (tokens.TokenType != PRTokeniser.TokType.NAME)
                {
                    tokens.ThrowError(MessageLocalization.GetComposedMessage("dictionary.key.1.is.not.a.name", tokens.StringValue));
                }

                PdfName key = new PdfName(tokens.StringValue, lengthCheck: false);
                PdfObject pdfObject = ReadPRObject();
                int type = pdfObject.Type;
                if (-type == 8)
                {
                    tokens.ThrowError(MessageLocalization.GetComposedMessage("unexpected.gt.gt"));
                }

                if (-type == 6)
                {
                    tokens.ThrowError(MessageLocalization.GetComposedMessage("unexpected.close.bracket"));
                }

                pdfDictionary.Put(key, pdfObject);
            }

            return pdfDictionary;
        }

        protected internal virtual PdfArray ReadArray()
        {
            PdfArray pdfArray = new PdfArray();
            while (true)
            {
                PdfObject pdfObject = ReadPRObject();
                int type = pdfObject.Type;
                if (-type == 6)
                {
                    break;
                }

                if (-type == 8)
                {
                    tokens.ThrowError(MessageLocalization.GetComposedMessage("unexpected.gt.gt"));
                }

                pdfArray.Add(pdfObject);
            }

            return pdfArray;
        }

        protected internal virtual PdfObject ReadPRObject()
        {
            tokens.NextValidToken();
            PRTokeniser.TokType tokenType = tokens.TokenType;
            switch (tokenType)
            {
                case PRTokeniser.TokType.START_DIC:
                    {
                        readDepth++;
                        PdfDictionary pdfDictionary = ReadDictionary();
                        readDepth--;
                        long filePointer = tokens.FilePointer;
                        bool flag;
                        do
                        {
                            flag = tokens.NextToken();
                        }
                        while (flag && tokens.TokenType == PRTokeniser.TokType.COMMENT);
                        if (flag && tokens.StringValue.Equals("stream"))
                        {
                            int num;
                            while (true)
                            {
                                num = tokens.Read();
                                switch (num)
                                {
                                    case 0:
                                    case 9:
                                    case 12:
                                    case 32:
                                        continue;
                                    default:
                                        num = tokens.Read();
                                        break;
                                    case 10:
                                        break;
                                }

                                break;
                            }

                            if (num != 10)
                            {
                                tokens.BackOnePosition(num);
                            }

                            PRStream pRStream = new PRStream(this, tokens.FilePointer);
                            pRStream.Merge(pdfDictionary);
                            pRStream.ObjNum = objNum;
                            pRStream.ObjGen = objGen;
                            return pRStream;
                        }

                        tokens.Seek(filePointer);
                        return pdfDictionary;
                    }
                case PRTokeniser.TokType.START_ARRAY:
                    {
                        readDepth++;
                        PdfArray result = ReadArray();
                        readDepth--;
                        return result;
                    }
                case PRTokeniser.TokType.NUMBER:
                    return new PdfNumber(tokens.StringValue);
                case PRTokeniser.TokType.STRING:
                    {
                        PdfString pdfString = new PdfString(tokens.StringValue, null).SetHexWriting(tokens.IsHexString());
                        pdfString.SetObjNum(objNum, objGen);
                        if (strings != null)
                        {
                            strings.Add(pdfString);
                        }

                        return pdfString;
                    }
                case PRTokeniser.TokType.NAME:
                    {
                        PdfName.staticNames.TryGetValue(tokens.StringValue, out var value);
                        if (readDepth > 0 && value != null)
                        {
                            return value;
                        }

                        return new PdfName(tokens.StringValue, lengthCheck: false);
                    }
                case PRTokeniser.TokType.REF:
                    {
                        int reference = tokens.Reference;
                        return new PRIndirectReference(this, reference, tokens.Generation);
                    }
                case PRTokeniser.TokType.ENDOFFILE:
                    throw new IOException(MessageLocalization.GetComposedMessage("unexpected.end.of.file"));
                default:
                    {
                        string stringValue = tokens.StringValue;
                        if ("null".Equals(stringValue))
                        {
                            if (readDepth == 0)
                            {
                                return new PdfNull();
                            }

                            return PdfNull.PDFNULL;
                        }

                        if ("true".Equals(stringValue))
                        {
                            if (readDepth == 0)
                            {
                                return new PdfBoolean(value: true);
                            }

                            return PdfBoolean.PDFTRUE;
                        }

                        if ("false".Equals(stringValue))
                        {
                            if (readDepth == 0)
                            {
                                return new PdfBoolean(value: false);
                            }

                            return PdfBoolean.PDFFALSE;
                        }

                        return new PdfLiteral(0 - tokenType, tokens.StringValue);
                    }
            }
        }

        public static byte[] FlateDecode(byte[] inp)
        {
            byte[] array = FlateDecode(inp, strict: true);
            if (array == null)
            {
                return FlateDecode(inp, strict: false);
            }

            return array;
        }

        public static byte[] DecodePredictor(byte[] inp, PdfObject dicPar)
        {
            if (dicPar == null || !dicPar.IsDictionary())
            {
                return inp;
            }

            PdfDictionary pdfDictionary = (PdfDictionary)dicPar;
            PdfObject pdfObject = GetPdfObject(pdfDictionary.Get(PdfName.PREDICTOR));
            if (pdfObject == null || !pdfObject.IsNumber())
            {
                return inp;
            }

            int intValue = ((PdfNumber)pdfObject).IntValue;
            if (intValue < 10 && intValue != 2)
            {
                return inp;
            }

            int num = 1;
            pdfObject = GetPdfObject(pdfDictionary.Get(PdfName.COLUMNS));
            if (pdfObject != null && pdfObject.IsNumber())
            {
                num = ((PdfNumber)pdfObject).IntValue;
            }

            int num2 = 1;
            pdfObject = GetPdfObject(pdfDictionary.Get(PdfName.COLORS));
            if (pdfObject != null && pdfObject.IsNumber())
            {
                num2 = ((PdfNumber)pdfObject).IntValue;
            }

            int num3 = 8;
            pdfObject = GetPdfObject(pdfDictionary.Get(PdfName.BITSPERCOMPONENT));
            if (pdfObject != null && pdfObject.IsNumber())
            {
                num3 = ((PdfNumber)pdfObject).IntValue;
            }

            MemoryStream memoryStream = new MemoryStream(inp);
            MemoryStream memoryStream2 = new MemoryStream(inp.Length);
            int num4 = num2 * num3 / 8;
            int num5 = (num2 * num * num3 + 7) / 8;
            byte[] array = new byte[num5];
            byte[] array2 = new byte[num5];
            if (intValue == 2)
            {
                if (num3 == 8)
                {
                    int num6 = inp.Length / num5;
                    for (int i = 0; i < num6; i++)
                    {
                        int num7 = i * num5;
                        for (int j = num4; j < num5; j++)
                        {
                            inp[num7 + j] = (byte)(inp[num7 + j] + inp[num7 + j - num4]);
                        }
                    }
                }

                return inp;
            }

            while (true)
            {
                int num8 = 0;
                try
                {
                    num8 = memoryStream.ReadByte();
                    if (num8 < 0)
                    {
                        return memoryStream2.ToArray();
                    }

                    int num9;
                    for (int k = 0; k < num5; k += num9)
                    {
                        num9 = memoryStream.Read(array, k, num5 - k);
                        if (num9 <= 0)
                        {
                            return memoryStream2.ToArray();
                        }
                    }
                }
                catch
                {
                    return memoryStream2.ToArray();
                }

                switch (num8)
                {
                    case 1:
                        {
                            for (int num19 = num4; num19 < num5; num19++)
                            {
                                array[num19] += array[num19 - num4];
                            }

                            break;
                        }
                    case 2:
                        {
                            for (int num20 = 0; num20 < num5; num20++)
                            {
                                array[num20] += array2[num20];
                            }

                            break;
                        }
                    case 3:
                        {
                            for (int n = 0; n < num4; n++)
                            {
                                array[n] += (byte)((int)array2[n] / 2);
                            }

                            for (int num18 = num4; num18 < num5; num18++)
                            {
                                array[num18] += (byte)(((array[num18 - num4] & 0xFF) + (array2[num18] & 0xFF)) / 2);
                            }

                            break;
                        }
                    case 4:
                        {
                            for (int l = 0; l < num4; l++)
                            {
                                array[l] += array2[l];
                            }

                            for (int m = num4; m < num5; m++)
                            {
                                int num10 = array[m - num4] & 0xFF;
                                int num11 = array2[m] & 0xFF;
                                int num12 = array2[m - num4] & 0xFF;
                                int num13 = num10 + num11 - num12;
                                int num14 = Math.Abs(num13 - num10);
                                int num15 = Math.Abs(num13 - num11);
                                int num16 = Math.Abs(num13 - num12);
                                int num17 = ((num14 <= num15 && num14 <= num16) ? num10 : ((num15 > num16) ? num12 : num11));
                                array[m] += (byte)num17;
                            }

                            break;
                        }
                    default:
                        throw new Exception(MessageLocalization.GetComposedMessage("png.filter.unknown"));
                    case 0:
                        break;
                }

                memoryStream2.Write(array, 0, array.Length);
                byte[] array3 = array2;
                array2 = array;
                array = array3;
            }
        }

        public static byte[] FlateDecode(byte[] inp, bool strict)
        {
            ZInflaterInputStream zInflaterInputStream = new ZInflaterInputStream(new MemoryStream(inp));
            MemoryStream memoryStream = new MemoryStream();
            byte[] array = new byte[(!strict) ? 1 : 4092];
            try
            {
                int count;
                while ((count = zInflaterInputStream.Read(array, 0, array.Length)) > 0)
                {
                    memoryStream.Write(array, 0, count);
                }

                zInflaterInputStream.Close();
                memoryStream.Close();
                return memoryStream.ToArray();
            }
            catch
            {
                if (strict)
                {
                    return null;
                }

                return memoryStream.ToArray();
            }
        }

        public static byte[] ASCIIHexDecode(byte[] inp)
        {
            MemoryStream memoryStream = new MemoryStream();
            bool flag = true;
            int num = 0;
            for (int i = 0; i < inp.Length; i++)
            {
                int num2 = inp[i] & 0xFF;
                if (num2 == 62)
                {
                    break;
                }

                if (!PRTokeniser.IsWhitespace(num2))
                {
                    int hex = PRTokeniser.GetHex(num2);
                    if (hex == -1)
                    {
                        throw new ArgumentException(MessageLocalization.GetComposedMessage("illegal.character.in.asciihexdecode"));
                    }

                    if (flag)
                    {
                        num = hex;
                    }
                    else
                    {
                        memoryStream.WriteByte((byte)((num << 4) + hex));
                    }

                    flag = !flag;
                }
            }

            if (!flag)
            {
                memoryStream.WriteByte((byte)(num << 4));
            }

            return memoryStream.ToArray();
        }

        public static byte[] ASCII85Decode(byte[] inp)
        {
            MemoryStream memoryStream = new MemoryStream();
            int num = 0;
            int[] array = new int[5];
            for (int i = 0; i < inp.Length; i++)
            {
                int num2 = inp[i] & 0xFF;
                if (num2 == 126)
                {
                    break;
                }

                if (PRTokeniser.IsWhitespace(num2))
                {
                    continue;
                }

                if (num2 == 122 && num == 0)
                {
                    memoryStream.WriteByte(0);
                    memoryStream.WriteByte(0);
                    memoryStream.WriteByte(0);
                    memoryStream.WriteByte(0);
                    continue;
                }

                if (num2 < 33 || num2 > 117)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("illegal.character.in.ascii85decode"));
                }

                array[num] = num2 - 33;
                num++;
                if (num == 5)
                {
                    num = 0;
                    int num3 = 0;
                    for (int j = 0; j < 5; j++)
                    {
                        num3 = num3 * 85 + array[j];
                    }

                    memoryStream.WriteByte((byte)(num3 >> 24));
                    memoryStream.WriteByte((byte)(num3 >> 16));
                    memoryStream.WriteByte((byte)(num3 >> 8));
                    memoryStream.WriteByte((byte)num3);
                }
            }

            int num4 = 0;
            switch (num)
            {
                case 2:
                    num4 = array[0] * 85 * 85 * 85 * 85 + array[1] * 85 * 85 * 85 + 614125 + 7225 + 85;
                    memoryStream.WriteByte((byte)(num4 >> 24));
                    break;
                case 3:
                    num4 = array[0] * 85 * 85 * 85 * 85 + array[1] * 85 * 85 * 85 + array[2] * 85 * 85 + 7225 + 85;
                    memoryStream.WriteByte((byte)(num4 >> 24));
                    memoryStream.WriteByte((byte)(num4 >> 16));
                    break;
                case 4:
                    num4 = array[0] * 85 * 85 * 85 * 85 + array[1] * 85 * 85 * 85 + array[2] * 85 * 85 + array[3] * 85 + 85;
                    memoryStream.WriteByte((byte)(num4 >> 24));
                    memoryStream.WriteByte((byte)(num4 >> 16));
                    memoryStream.WriteByte((byte)(num4 >> 8));
                    break;
            }

            return memoryStream.ToArray();
        }

        public static byte[] LZWDecode(byte[] inp)
        {
            MemoryStream memoryStream = new MemoryStream();
            new LZWDecoder().Decode(inp, memoryStream);
            return memoryStream.ToArray();
        }

        public virtual bool IsRebuilt()
        {
            return rebuilt;
        }

        public virtual PdfDictionary GetPageN(int pageNum)
        {
            PdfDictionary pageN = pageRefs.GetPageN(pageNum);
            if (pageN == null)
            {
                return null;
            }

            if (appendable)
            {
                pageN.IndRef = pageRefs.GetPageOrigRef(pageNum);
            }

            return pageN;
        }

        public virtual PdfDictionary GetPageNRelease(int pageNum)
        {
            PdfDictionary pageN = GetPageN(pageNum);
            pageRefs.ReleasePage(pageNum);
            return pageN;
        }

        public virtual void ReleasePage(int pageNum)
        {
            pageRefs.ReleasePage(pageNum);
        }

        public virtual void ResetReleasePage()
        {
            pageRefs.ResetReleasePage();
        }

        public virtual PRIndirectReference GetPageOrigRef(int pageNum)
        {
            return pageRefs.GetPageOrigRef(pageNum);
        }

        public virtual byte[] GetPageContent(int pageNum, RandomAccessFileOrArray file)
        {
            PdfDictionary pageNRelease = GetPageNRelease(pageNum);
            if (pageNRelease == null)
            {
                return null;
            }

            PdfObject pdfObjectRelease = GetPdfObjectRelease(pageNRelease.Get(PdfName.CONTENTS));
            if (pdfObjectRelease == null)
            {
                return new byte[0];
            }

            MemoryStream memoryStream = null;
            if (pdfObjectRelease.IsStream())
            {
                return GetStreamBytes((PRStream)pdfObjectRelease, file);
            }

            if (pdfObjectRelease.IsArray())
            {
                PdfArray pdfArray = (PdfArray)pdfObjectRelease;
                memoryStream = new MemoryStream();
                for (int i = 0; i < pdfArray.Size; i++)
                {
                    PdfObject pdfObjectRelease2 = GetPdfObjectRelease(pdfArray.GetPdfObject(i));
                    if (pdfObjectRelease2 != null && pdfObjectRelease2.IsStream())
                    {
                        byte[] streamBytes = GetStreamBytes((PRStream)pdfObjectRelease2, file);
                        memoryStream.Write(streamBytes, 0, streamBytes.Length);
                        if (i != pdfArray.Size - 1)
                        {
                            memoryStream.WriteByte(10);
                        }
                    }
                }

                return memoryStream.ToArray();
            }

            return new byte[0];
        }

        public static byte[] GetPageContent(PdfDictionary page)
        {
            if (page == null)
            {
                return null;
            }

            RandomAccessFileOrArray randomAccessFileOrArray = null;
            try
            {
                PdfObject pdfObjectRelease = GetPdfObjectRelease(page.Get(PdfName.CONTENTS));
                if (pdfObjectRelease == null)
                {
                    return new byte[0];
                }

                if (pdfObjectRelease.IsStream())
                {
                    if (randomAccessFileOrArray == null)
                    {
                        randomAccessFileOrArray = ((PRStream)pdfObjectRelease).Reader.SafeFile;
                        randomAccessFileOrArray.ReOpen();
                    }

                    return GetStreamBytes((PRStream)pdfObjectRelease, randomAccessFileOrArray);
                }

                if (pdfObjectRelease.IsArray())
                {
                    PdfArray pdfArray = (PdfArray)pdfObjectRelease;
                    MemoryStream memoryStream = new MemoryStream();
                    for (int i = 0; i < pdfArray.Size; i++)
                    {
                        PdfObject pdfObjectRelease2 = GetPdfObjectRelease(pdfArray.GetPdfObject(i));
                        if (pdfObjectRelease2 != null && pdfObjectRelease2.IsStream())
                        {
                            if (randomAccessFileOrArray == null)
                            {
                                randomAccessFileOrArray = ((PRStream)pdfObjectRelease2).Reader.SafeFile;
                                randomAccessFileOrArray.ReOpen();
                            }

                            byte[] streamBytes = GetStreamBytes((PRStream)pdfObjectRelease2, randomAccessFileOrArray);
                            memoryStream.Write(streamBytes, 0, streamBytes.Length);
                            if (i != pdfArray.Size - 1)
                            {
                                memoryStream.WriteByte(10);
                            }
                        }
                    }

                    return memoryStream.ToArray();
                }

                return new byte[0];
            }
            finally
            {
                try
                {
                    randomAccessFileOrArray?.Close();
                }
                catch
                {
                }
            }
        }

        public virtual PdfDictionary GetPageResources(int pageNum)
        {
            return GetPageResources(GetPageN(pageNum));
        }

        public virtual PdfDictionary GetPageResources(PdfDictionary pageDict)
        {
            return pageDict.GetAsDict(PdfName.RESOURCES);
        }

        public virtual byte[] GetPageContent(int pageNum)
        {
            RandomAccessFileOrArray safeFile = SafeFile;
            try
            {
                safeFile.ReOpen();
                return GetPageContent(pageNum, safeFile);
            }
            finally
            {
                try
                {
                    safeFile.Close();
                }
                catch
                {
                }
            }
        }

        protected internal virtual void KillXref(PdfObject obj)
        {
            if (obj == null || (obj is PdfIndirectReference && !obj.IsIndirect()))
            {
                return;
            }

            switch (obj.Type)
            {
                case 10:
                    {
                        int number = ((PRIndirectReference)obj).Number;
                        obj = xrefObj[number];
                        xrefObj[number] = null;
                        freeXref = number;
                        KillXref(obj);
                        break;
                    }
                case 5:
                    {
                        PdfArray pdfArray = (PdfArray)obj;
                        for (int i = 0; i < pdfArray.Size; i++)
                        {
                            KillXref(pdfArray.GetPdfObject(i));
                        }

                        break;
                    }
                case 6:
                case 7:
                    {
                        PdfDictionary pdfDictionary = (PdfDictionary)obj;
                        foreach (PdfName key in pdfDictionary.Keys)
                        {
                            KillXref(pdfDictionary.Get(key));
                        }

                        break;
                    }
                case 8:
                case 9:
                    break;
            }
        }

        public virtual void SetPageContent(int pageNum, byte[] content)
        {
            SetPageContent(pageNum, content, -1);
        }

        public virtual void SetPageContent(int pageNum, byte[] content, int compressionLevel)
        {
            PdfDictionary pageN = GetPageN(pageNum);
            if (pageN != null)
            {
                PdfObject obj = pageN.Get(PdfName.CONTENTS);
                freeXref = -1;
                KillXref(obj);
                if (freeXref == -1)
                {
                    xrefObj.Add(null);
                    freeXref = xrefObj.Count - 1;
                }

                pageN.Put(PdfName.CONTENTS, new PRIndirectReference(this, freeXref));
                xrefObj[freeXref] = new PRStream(this, content, compressionLevel);
            }
        }

        public static byte[] DecodeBytes(byte[] b, PdfDictionary streamDictionary)
        {
            return DecodeBytes(b, streamDictionary, FilterHandlers.GetDefaultFilterHandlers());
        }

        public static byte[] DecodeBytes(byte[] b, PdfDictionary streamDictionary, IDictionary<PdfName, FilterHandlers.IFilterHandler> filterHandlers)
        {
            PdfObject pdfObjectRelease = GetPdfObjectRelease(streamDictionary.Get(PdfName.FILTER));
            List<PdfObject> list = new List<PdfObject>();
            if (pdfObjectRelease != null)
            {
                if (pdfObjectRelease.IsName())
                {
                    list.Add(pdfObjectRelease);
                }
                else if (pdfObjectRelease.IsArray())
                {
                    list = ((PdfArray)pdfObjectRelease).ArrayList;
                }
            }

            List<PdfObject> list2 = new List<PdfObject>();
            PdfObject pdfObjectRelease2 = GetPdfObjectRelease(streamDictionary.Get(PdfName.DECODEPARMS));
            if (pdfObjectRelease2 == null || (!pdfObjectRelease2.IsDictionary() && !pdfObjectRelease2.IsArray()))
            {
                pdfObjectRelease2 = GetPdfObjectRelease(streamDictionary.Get(PdfName.DP));
            }

            if (pdfObjectRelease2 != null)
            {
                if (pdfObjectRelease2.IsDictionary())
                {
                    list2.Add(pdfObjectRelease2);
                }
                else if (pdfObjectRelease2.IsArray())
                {
                    list2 = ((PdfArray)pdfObjectRelease2).ArrayList;
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                PdfName pdfName = (PdfName)list[i];
                filterHandlers.TryGetValue(pdfName, out var value);
                if (value == null)
                {
                    throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("the.filter.1.is.not.supported", pdfName));
                }

                PdfDictionary decodeParams;
                if (i < list2.Count)
                {
                    PdfObject pdfObject = GetPdfObject(list2[i]);
                    if (pdfObject is PdfDictionary)
                    {
                        decodeParams = (PdfDictionary)pdfObject;
                    }
                    else
                    {
                        if (pdfObject != null && !(pdfObject is PdfNull))
                        {
                            throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("the.decode.parameter.type.1.is.not.supported", pdfObject.GetType().FullName));
                        }

                        decodeParams = null;
                    }
                }
                else
                {
                    decodeParams = null;
                }

                b = value.Decode(b, pdfName, decodeParams, streamDictionary);
            }

            return b;
        }

        public static byte[] GetStreamBytes(PRStream stream, RandomAccessFileOrArray file)
        {
            return DecodeBytes(GetStreamBytesRaw(stream, file), stream);
        }

        public static byte[] GetStreamBytes(PRStream stream)
        {
            RandomAccessFileOrArray safeFile = stream.Reader.SafeFile;
            try
            {
                safeFile.ReOpen();
                return GetStreamBytes(stream, safeFile);
            }
            finally
            {
                try
                {
                    safeFile.Close();
                }
                catch
                {
                }
            }
        }

        public static byte[] GetStreamBytesRaw(PRStream stream, RandomAccessFileOrArray file)
        {
            PdfReader reader = stream.Reader;
            byte[] array;
            if (stream.Offset < 0)
            {
                array = stream.GetBytes();
            }
            else
            {
                array = new byte[stream.Length];
                file.Seek(stream.Offset);
                file.ReadFully(array);
                PdfEncryption pdfEncryption = reader.Decrypt;
                if (pdfEncryption != null)
                {
                    PdfObject pdfObjectRelease = GetPdfObjectRelease(stream.Get(PdfName.FILTER));
                    List<PdfObject> list = new List<PdfObject>();
                    if (pdfObjectRelease != null)
                    {
                        if (pdfObjectRelease.IsName())
                        {
                            list.Add(pdfObjectRelease);
                        }
                        else if (pdfObjectRelease.IsArray())
                        {
                            list = ((PdfArray)pdfObjectRelease).ArrayList;
                        }
                    }

                    bool flag = false;
                    for (int i = 0; i < list.Count; i++)
                    {
                        PdfObject pdfObjectRelease2 = GetPdfObjectRelease(list[i]);
                        if (pdfObjectRelease2 != null && pdfObjectRelease2.ToString().Equals("/Crypt"))
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (!flag)
                    {
                        pdfEncryption.SetHashKey(stream.ObjNum, stream.ObjGen);
                        array = pdfEncryption.DecryptByteArray(array);
                    }
                }
            }

            return array;
        }

        public static byte[] GetStreamBytesRaw(PRStream stream)
        {
            RandomAccessFileOrArray safeFile = stream.Reader.SafeFile;
            try
            {
                safeFile.ReOpen();
                return GetStreamBytesRaw(stream, safeFile);
            }
            finally
            {
                try
                {
                    safeFile.Close();
                }
                catch
                {
                }
            }
        }

        public virtual void EliminateSharedStreams()
        {
            if (!sharedStreams)
            {
                return;
            }

            sharedStreams = false;
            if (pageRefs.Size == 1)
            {
                return;
            }

            List<PRIndirectReference> list = new List<PRIndirectReference>();
            List<PRStream> list2 = new List<PRStream>();
            IntHashtable intHashtable = new IntHashtable();
            for (int i = 1; i <= pageRefs.Size; i++)
            {
                PdfDictionary pageN = pageRefs.GetPageN(i);
                if (pageN == null)
                {
                    continue;
                }

                PdfObject pdfObject = GetPdfObject(pageN.Get(PdfName.CONTENTS));
                if (pdfObject == null)
                {
                    continue;
                }

                if (pdfObject.IsStream())
                {
                    PRIndirectReference pRIndirectReference = (PRIndirectReference)pageN.Get(PdfName.CONTENTS);
                    if (intHashtable.ContainsKey(pRIndirectReference.Number))
                    {
                        list.Add(pRIndirectReference);
                        list2.Add(new PRStream((PRStream)pdfObject, null));
                    }
                    else
                    {
                        intHashtable[pRIndirectReference.Number] = 1;
                    }
                }
                else
                {
                    if (!pdfObject.IsArray())
                    {
                        continue;
                    }

                    PdfArray pdfArray = (PdfArray)pdfObject;
                    for (int j = 0; j < pdfArray.Size; j++)
                    {
                        PRIndirectReference pRIndirectReference2 = (PRIndirectReference)pdfArray.GetPdfObject(j);
                        if (intHashtable.ContainsKey(pRIndirectReference2.Number))
                        {
                            list.Add(pRIndirectReference2);
                            list2.Add(new PRStream((PRStream)GetPdfObject(pRIndirectReference2), null));
                        }
                        else
                        {
                            intHashtable[pRIndirectReference2.Number] = 1;
                        }
                    }
                }
            }

            if (list2.Count != 0)
            {
                for (int k = 0; k < list2.Count; k++)
                {
                    xrefObj.Add(list2[k]);
                    list[k].SetNumber(xrefObj.Count - 1, 0);
                }
            }
        }

        public virtual bool IsEncrypted()
        {
            return encrypted;
        }

        public virtual bool Is128Key()
        {
            return rValue == 3;
        }

        internal static bool Equalsn(byte[] a1, byte[] a2)
        {
            int num = a2.Length;
            for (int i = 0; i < num; i++)
            {
                if (a1[i] != a2[i])
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool ExistsName(PdfDictionary dic, PdfName key, PdfName value)
        {
            PdfObject pdfObjectRelease = GetPdfObjectRelease(dic.Get(key));
            if (pdfObjectRelease == null || !pdfObjectRelease.IsName())
            {
                return false;
            }

            return ((PdfName)pdfObjectRelease).Equals(value);
        }

        internal static string GetFontName(PdfDictionary dic)
        {
            if (dic == null)
            {
                return null;
            }

            PdfObject pdfObjectRelease = GetPdfObjectRelease(dic.Get(PdfName.BASEFONT));
            if (pdfObjectRelease == null || !pdfObjectRelease.IsName())
            {
                return null;
            }

            return PdfName.DecodeName(pdfObjectRelease.ToString());
        }

        internal static string GetSubsetPrefix(PdfDictionary dic)
        {
            if (dic == null)
            {
                return null;
            }

            string fontName = GetFontName(dic);
            if (fontName == null)
            {
                return null;
            }

            if (fontName.Length < 8 || fontName[6] != '+')
            {
                return null;
            }

            for (int i = 0; i < 6; i++)
            {
                char c = fontName[i];
                if (c < 'A' || c > 'Z')
                {
                    return null;
                }
            }

            return fontName;
        }

        public virtual int ShuffleSubsetNames()
        {
            int num = 0;
            for (int i = 1; i < xrefObj.Count; i++)
            {
                PdfObject pdfObjectRelease = GetPdfObjectRelease(i);
                if (pdfObjectRelease == null || !pdfObjectRelease.IsDictionary())
                {
                    continue;
                }

                PdfDictionary pdfDictionary = (PdfDictionary)pdfObjectRelease;
                if (!ExistsName(pdfDictionary, PdfName.TYPE, PdfName.FONT))
                {
                    continue;
                }

                if (ExistsName(pdfDictionary, PdfName.SUBTYPE, PdfName.TYPE1) || ExistsName(pdfDictionary, PdfName.SUBTYPE, PdfName.MMTYPE1) || ExistsName(pdfDictionary, PdfName.SUBTYPE, PdfName.TRUETYPE))
                {
                    string subsetPrefix = GetSubsetPrefix(pdfDictionary);
                    if (subsetPrefix != null)
                    {
                        PdfName value = new PdfName(BaseFont.CreateSubsetPrefix() + subsetPrefix.Substring(7));
                        pdfDictionary.Put(PdfName.BASEFONT, value);
                        SetXrefPartialObject(i, pdfDictionary);
                        num++;
                        pdfDictionary.GetAsDict(PdfName.FONTDESCRIPTOR)?.Put(PdfName.FONTNAME, value);
                    }
                }
                else
                {
                    if (!ExistsName(pdfDictionary, PdfName.SUBTYPE, PdfName.TYPE0))
                    {
                        continue;
                    }

                    string subsetPrefix2 = GetSubsetPrefix(pdfDictionary);
                    PdfArray asArray = pdfDictionary.GetAsArray(PdfName.DESCENDANTFONTS);
                    if (asArray == null || asArray.IsEmpty())
                    {
                        continue;
                    }

                    PdfDictionary asDict = asArray.GetAsDict(0);
                    string subsetPrefix3 = GetSubsetPrefix(asDict);
                    if (subsetPrefix3 != null)
                    {
                        string text = BaseFont.CreateSubsetPrefix();
                        if (subsetPrefix2 != null)
                        {
                            pdfDictionary.Put(PdfName.BASEFONT, new PdfName(text + subsetPrefix2.Substring(7)));
                        }

                        SetXrefPartialObject(i, pdfDictionary);
                        PdfName value2 = new PdfName(text + subsetPrefix3.Substring(7));
                        asDict.Put(PdfName.BASEFONT, value2);
                        num++;
                        asDict.GetAsDict(PdfName.FONTDESCRIPTOR)?.Put(PdfName.FONTNAME, value2);
                    }
                }
            }

            return num;
        }

        public virtual int CreateFakeFontSubsets()
        {
            int num = 0;
            for (int i = 1; i < xrefObj.Count; i++)
            {
                PdfObject pdfObjectRelease = GetPdfObjectRelease(i);
                if (pdfObjectRelease == null || !pdfObjectRelease.IsDictionary())
                {
                    continue;
                }

                PdfDictionary pdfDictionary = (PdfDictionary)pdfObjectRelease;
                if (!ExistsName(pdfDictionary, PdfName.TYPE, PdfName.FONT) || (!ExistsName(pdfDictionary, PdfName.SUBTYPE, PdfName.TYPE1) && !ExistsName(pdfDictionary, PdfName.SUBTYPE, PdfName.MMTYPE1) && !ExistsName(pdfDictionary, PdfName.SUBTYPE, PdfName.TRUETYPE)))
                {
                    continue;
                }

                string subsetPrefix = GetSubsetPrefix(pdfDictionary);
                if (subsetPrefix != null)
                {
                    continue;
                }

                subsetPrefix = GetFontName(pdfDictionary);
                if (subsetPrefix != null)
                {
                    string name = BaseFont.CreateSubsetPrefix() + subsetPrefix;
                    PdfDictionary pdfDictionary2 = (PdfDictionary)GetPdfObjectRelease(pdfDictionary.Get(PdfName.FONTDESCRIPTOR));
                    if (pdfDictionary2 != null && (pdfDictionary2.Get(PdfName.FONTFILE) != null || pdfDictionary2.Get(PdfName.FONTFILE2) != null || pdfDictionary2.Get(PdfName.FONTFILE3) != null))
                    {
                        pdfDictionary2 = pdfDictionary.GetAsDict(PdfName.FONTDESCRIPTOR);
                        PdfName value = new PdfName(name);
                        pdfDictionary.Put(PdfName.BASEFONT, value);
                        pdfDictionary2.Put(PdfName.FONTNAME, value);
                        SetXrefPartialObject(i, pdfDictionary);
                        num++;
                    }
                }
            }

            return num;
        }

        private static PdfArray GetNameArray(PdfObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            obj = GetPdfObjectRelease(obj);
            if (obj == null)
            {
                return null;
            }

            if (obj.IsArray())
            {
                return (PdfArray)obj;
            }

            if (obj.IsDictionary())
            {
                PdfObject pdfObjectRelease = GetPdfObjectRelease(((PdfDictionary)obj).Get(PdfName.D));
                if (pdfObjectRelease != null && pdfObjectRelease.IsArray())
                {
                    return (PdfArray)pdfObjectRelease;
                }
            }

            return null;
        }

        public virtual Dictionary<object, PdfObject> GetNamedDestination()
        {
            return GetNamedDestination(keepNames: false);
        }

        public virtual Dictionary<object, PdfObject> GetNamedDestination(bool keepNames)
        {
            Dictionary<object, PdfObject> namedDestinationFromNames = GetNamedDestinationFromNames(keepNames);
            foreach (KeyValuePair<string, PdfObject> namedDestinationFromString in GetNamedDestinationFromStrings())
            {
                namedDestinationFromNames[namedDestinationFromString.Key] = namedDestinationFromString.Value;
            }

            return namedDestinationFromNames;
        }

        public virtual Dictionary<string, PdfObject> GetNamedDestinationFromNames()
        {
            Dictionary<string, PdfObject> dictionary = new Dictionary<string, PdfObject>();
            foreach (KeyValuePair<object, PdfObject> namedDestinationFromName in GetNamedDestinationFromNames(keepNames: false))
            {
                dictionary[(string)namedDestinationFromName.Key] = namedDestinationFromName.Value;
            }

            return dictionary;
        }

        public virtual Dictionary<object, PdfObject> GetNamedDestinationFromNames(bool keepNames)
        {
            Dictionary<object, PdfObject> dictionary = new Dictionary<object, PdfObject>();
            if (catalog.Get(PdfName.DESTS) != null)
            {
                PdfDictionary pdfDictionary = (PdfDictionary)GetPdfObjectRelease(catalog.Get(PdfName.DESTS));
                if (pdfDictionary == null)
                {
                    return dictionary;
                }

                {
                    foreach (PdfName key2 in pdfDictionary.Keys)
                    {
                        PdfArray nameArray = GetNameArray(pdfDictionary.Get(key2));
                        if (nameArray != null)
                        {
                            if (keepNames)
                            {
                                dictionary[key2] = nameArray;
                                continue;
                            }

                            string key = PdfName.DecodeName(key2.ToString());
                            dictionary[key] = nameArray;
                        }
                    }

                    return dictionary;
                }
            }

            return dictionary;
        }

        public virtual Dictionary<string, PdfObject> GetNamedDestinationFromStrings()
        {
            if (catalog.Get(PdfName.NAMES) != null)
            {
                PdfDictionary pdfDictionary = (PdfDictionary)GetPdfObjectRelease(catalog.Get(PdfName.NAMES));
                if (pdfDictionary != null)
                {
                    pdfDictionary = (PdfDictionary)GetPdfObjectRelease(pdfDictionary.Get(PdfName.DESTS));
                    if (pdfDictionary != null)
                    {
                        Dictionary<string, PdfObject> dictionary = PdfNameTree.ReadTree(pdfDictionary);
                        string[] array = new string[dictionary.Count];
                        dictionary.Keys.CopyTo(array, 0);
                        string[] array2 = array;
                        foreach (string key in array2)
                        {
                            PdfArray nameArray = GetNameArray(dictionary[key]);
                            if (nameArray != null)
                            {
                                dictionary[key] = nameArray;
                            }
                            else
                            {
                                dictionary.Remove(key);
                            }
                        }

                        return dictionary;
                    }
                }
            }

            return new Dictionary<string, PdfObject>();
        }

        public virtual void RemoveFields()
        {
            pageRefs.ResetReleasePage();
            for (int i = 1; i <= pageRefs.Size; i++)
            {
                PdfDictionary pageN = pageRefs.GetPageN(i);
                PdfArray asArray = pageN.GetAsArray(PdfName.ANNOTS);
                if (asArray == null)
                {
                    pageRefs.ReleasePage(i);
                    continue;
                }

                for (int j = 0; j < asArray.Size; j++)
                {
                    PdfObject pdfObjectRelease = GetPdfObjectRelease(asArray.GetPdfObject(j));
                    if (pdfObjectRelease != null && pdfObjectRelease.IsDictionary())
                    {
                        PdfDictionary pdfDictionary = (PdfDictionary)pdfObjectRelease;
                        if (PdfName.WIDGET.Equals(pdfDictionary.Get(PdfName.SUBTYPE)))
                        {
                            asArray.Remove(j--);
                        }
                    }
                }

                if (asArray.IsEmpty())
                {
                    pageN.Remove(PdfName.ANNOTS);
                }
                else
                {
                    pageRefs.ReleasePage(i);
                }
            }

            catalog.Remove(PdfName.ACROFORM);
            pageRefs.ResetReleasePage();
        }

        public virtual void RemoveAnnotations()
        {
            pageRefs.ResetReleasePage();
            for (int i = 1; i <= pageRefs.Size; i++)
            {
                PdfDictionary pageN = pageRefs.GetPageN(i);
                if (pageN.Get(PdfName.ANNOTS) == null)
                {
                    pageRefs.ReleasePage(i);
                }
                else
                {
                    pageN.Remove(PdfName.ANNOTS);
                }
            }

            catalog.Remove(PdfName.ACROFORM);
            pageRefs.ResetReleasePage();
        }

        public virtual List<PdfAnnotation.PdfImportedLink> GetLinks(int page)
        {
            pageRefs.ResetReleasePage();
            List<PdfAnnotation.PdfImportedLink> list = new List<PdfAnnotation.PdfImportedLink>();
            PdfDictionary pageN = pageRefs.GetPageN(page);
            if (pageN.Get(PdfName.ANNOTS) != null)
            {
                PdfArray asArray = pageN.GetAsArray(PdfName.ANNOTS);
                for (int i = 0; i < asArray.Size; i++)
                {
                    PdfDictionary pdfDictionary = (PdfDictionary)GetPdfObjectRelease(asArray.GetPdfObject(i));
                    if (PdfName.LINK.Equals(pdfDictionary.Get(PdfName.SUBTYPE)))
                    {
                        list.Add(new PdfAnnotation.PdfImportedLink(pdfDictionary));
                    }
                }
            }

            pageRefs.ReleasePage(page);
            pageRefs.ResetReleasePage();
            return list;
        }

        private void IterateBookmarks(PdfObject outlineRef, Dictionary<object, PdfObject> names)
        {
            while (outlineRef != null)
            {
                ReplaceNamedDestination(outlineRef, names);
                PdfDictionary obj = (PdfDictionary)GetPdfObjectRelease(outlineRef);
                PdfObject pdfObject = obj.Get(PdfName.FIRST);
                if (pdfObject != null)
                {
                    IterateBookmarks(pdfObject, names);
                }

                outlineRef = obj.Get(PdfName.NEXT);
            }
        }

        public virtual void MakeRemoteNamedDestinationsLocal()
        {
            if (remoteToLocalNamedDestinations)
            {
                return;
            }

            remoteToLocalNamedDestinations = true;
            Dictionary<object, PdfObject> namedDestination = GetNamedDestination(keepNames: true);
            if (namedDestination.Count == 0)
            {
                return;
            }

            for (int i = 1; i <= pageRefs.Size; i++)
            {
                PdfObject pdfObject;
                PdfArray pdfArray = (PdfArray)GetPdfObject(pdfObject = pageRefs.GetPageN(i).Get(PdfName.ANNOTS));
                int idx = lastXrefPartial;
                ReleaseLastXrefPartial();
                if (pdfArray == null)
                {
                    pageRefs.ReleasePage(i);
                    continue;
                }

                bool flag = false;
                for (int j = 0; j < pdfArray.Size; j++)
                {
                    PdfObject pdfObject2 = pdfArray.GetPdfObject(j);
                    if (ConvertNamedDestination(pdfObject2, namedDestination) && !pdfObject2.IsIndirect())
                    {
                        flag = true;
                    }
                }

                if (flag)
                {
                    SetXrefPartialObject(idx, pdfArray);
                }

                if (!flag || pdfObject.IsIndirect())
                {
                    pageRefs.ReleasePage(i);
                }
            }
        }

        private bool ConvertNamedDestination(PdfObject obj, Dictionary<object, PdfObject> names)
        {
            obj = GetPdfObject(obj);
            int idx = lastXrefPartial;
            ReleaseLastXrefPartial();
            if (obj != null && obj.IsDictionary())
            {
                PdfObject pdfObject = GetPdfObject(((PdfDictionary)obj).Get(PdfName.A));
                if (pdfObject != null)
                {
                    int idx2 = lastXrefPartial;
                    ReleaseLastXrefPartial();
                    PdfDictionary pdfDictionary = (PdfDictionary)pdfObject;
                    PdfName obj2 = (PdfName)GetPdfObjectRelease(pdfDictionary.Get(PdfName.S));
                    if (PdfName.GOTOR.Equals(obj2))
                    {
                        PdfObject pdfObjectRelease = GetPdfObjectRelease(pdfDictionary.Get(PdfName.D));
                        object obj3 = null;
                        if (pdfObjectRelease != null)
                        {
                            if (pdfObjectRelease.IsName())
                            {
                                obj3 = pdfObjectRelease;
                            }
                            else if (pdfObjectRelease.IsString())
                            {
                                obj3 = pdfObjectRelease.ToString();
                            }

                            PdfArray pdfArray = null;
                            if (obj3 != null && names.ContainsKey(obj3))
                            {
                                pdfArray = (PdfArray)names[obj3];
                            }

                            if (pdfArray != null)
                            {
                                pdfDictionary.Remove(PdfName.F);
                                pdfDictionary.Remove(PdfName.NEWWINDOW);
                                pdfDictionary.Put(PdfName.S, PdfName.GOTO);
                                SetXrefPartialObject(idx2, pdfObject);
                                SetXrefPartialObject(idx, obj);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public virtual void ConsolidateNamedDestinations()
        {
            if (consolidateNamedDestinations)
            {
                return;
            }

            consolidateNamedDestinations = true;
            Dictionary<object, PdfObject> namedDestination = GetNamedDestination(keepNames: true);
            if (namedDestination.Count == 0)
            {
                return;
            }

            for (int i = 1; i <= pageRefs.Size; i++)
            {
                PdfObject pdfObject;
                PdfArray pdfArray = (PdfArray)GetPdfObject(pdfObject = pageRefs.GetPageN(i).Get(PdfName.ANNOTS));
                int idx = lastXrefPartial;
                ReleaseLastXrefPartial();
                if (pdfArray == null)
                {
                    pageRefs.ReleasePage(i);
                    continue;
                }

                bool flag = false;
                for (int j = 0; j < pdfArray.Size; j++)
                {
                    PdfObject pdfObject2 = pdfArray.GetPdfObject(j);
                    if (ReplaceNamedDestination(pdfObject2, namedDestination) && !pdfObject2.IsIndirect())
                    {
                        flag = true;
                    }
                }

                if (flag)
                {
                    SetXrefPartialObject(idx, pdfArray);
                }

                if (!flag || pdfObject.IsIndirect())
                {
                    pageRefs.ReleasePage(i);
                }
            }

            PdfDictionary pdfDictionary = (PdfDictionary)GetPdfObjectRelease(catalog.Get(PdfName.OUTLINES));
            if (pdfDictionary != null)
            {
                IterateBookmarks(pdfDictionary.Get(PdfName.FIRST), namedDestination);
            }
        }

        private bool ReplaceNamedDestination(PdfObject obj, Dictionary<object, PdfObject> names)
        {
            obj = GetPdfObject(obj);
            int idx = lastXrefPartial;
            ReleaseLastXrefPartial();
            if (obj != null && obj.IsDictionary())
            {
                PdfObject pdfObjectRelease = GetPdfObjectRelease(((PdfDictionary)obj).Get(PdfName.DEST));
                object obj2 = null;
                if (pdfObjectRelease != null)
                {
                    if (pdfObjectRelease.IsName())
                    {
                        obj2 = pdfObjectRelease;
                    }
                    else if (pdfObjectRelease.IsString())
                    {
                        obj2 = pdfObjectRelease.ToString();
                    }

                    if (obj2 != null)
                    {
                        PdfArray pdfArray = null;
                        if (names.ContainsKey(obj2) && names[obj2] is PdfArray)
                        {
                            pdfArray = (PdfArray)names[obj2];
                        }

                        if (pdfArray != null)
                        {
                            ((PdfDictionary)obj).Put(PdfName.DEST, pdfArray);
                            SetXrefPartialObject(idx, obj);
                            return true;
                        }
                    }
                }
                else if ((pdfObjectRelease = GetPdfObject(((PdfDictionary)obj).Get(PdfName.A))) != null)
                {
                    int idx2 = lastXrefPartial;
                    ReleaseLastXrefPartial();
                    PdfDictionary pdfDictionary = (PdfDictionary)pdfObjectRelease;
                    PdfName obj3 = (PdfName)GetPdfObjectRelease(pdfDictionary.Get(PdfName.S));
                    if (PdfName.GOTO.Equals(obj3))
                    {
                        PdfObject pdfObjectRelease2 = GetPdfObjectRelease(pdfDictionary.Get(PdfName.D));
                        if (pdfObjectRelease2 != null)
                        {
                            if (pdfObjectRelease2.IsName())
                            {
                                obj2 = pdfObjectRelease2;
                            }
                            else if (pdfObjectRelease2.IsString())
                            {
                                obj2 = pdfObjectRelease2.ToString();
                            }
                        }

                        if (obj2 != null)
                        {
                            PdfArray pdfArray2 = null;
                            if (names.ContainsKey(obj2) && names[obj2] is PdfArray)
                            {
                                pdfArray2 = (PdfArray)names[obj2];
                            }

                            if (pdfArray2 != null)
                            {
                                pdfDictionary.Put(PdfName.D, pdfArray2);
                                SetXrefPartialObject(idx2, pdfObjectRelease);
                                SetXrefPartialObject(idx, obj);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        protected internal static PdfDictionary DuplicatePdfDictionary(PdfDictionary original, PdfDictionary copy, PdfReader newReader)
        {
            if (copy == null)
            {
                copy = new PdfDictionary();
            }

            foreach (PdfName key in original.Keys)
            {
                copy.Put(key, DuplicatePdfObject(original.Get(key), newReader));
            }

            return copy;
        }

        protected internal static PdfObject DuplicatePdfObject(PdfObject original, PdfReader newReader)
        {
            if (original == null)
            {
                return null;
            }

            switch (original.Type)
            {
                case 6:
                    return DuplicatePdfDictionary((PdfDictionary)original, null, newReader);
                case 7:
                    {
                        PRStream obj = (PRStream)original;
                        PRStream pRStream = new PRStream(obj, null, newReader);
                        DuplicatePdfDictionary(obj, pRStream, newReader);
                        return pRStream;
                    }
                case 5:
                    {
                        PdfArray pdfArray = new PdfArray();
                        ListIterator<PdfObject> listIterator = ((PdfArray)original).GetListIterator();
                        while (listIterator.HasNext())
                        {
                            pdfArray.Add(DuplicatePdfObject(listIterator.Next(), newReader));
                        }

                        return pdfArray;
                    }
                case 10:
                    {
                        PRIndirectReference pRIndirectReference = (PRIndirectReference)original;
                        return new PRIndirectReference(newReader, pRIndirectReference.Number, pRIndirectReference.Generation);
                    }
                default:
                    return original;
            }
        }

        public virtual void Close()
        {
            tokens.Close();
        }

        protected internal virtual void RemoveUnusedNode(PdfObject obj, bool[] hits)
        {
            Stack<object> stack = new Stack<object>();
            stack.Push(obj);
            while (stack.Count != 0)
            {
                object obj2 = stack.Pop();
                if (obj2 == null)
                {
                    continue;
                }

                List<PdfObject> list = null;
                PdfDictionary pdfDictionary = null;
                PdfName[] array = null;
                object[] array2 = null;
                int num = 0;
                if (obj2 is PdfObject)
                {
                    obj = (PdfObject)obj2;
                    switch (obj.Type)
                    {
                        case 6:
                        case 7:
                            pdfDictionary = (PdfDictionary)obj;
                            array = new PdfName[pdfDictionary.Size];
                            pdfDictionary.Keys.CopyTo(array, 0);
                            break;
                        case 5:
                            list = ((PdfArray)obj).ArrayList;
                            break;
                        case 10:
                            {
                                PRIndirectReference pRIndirectReference = (PRIndirectReference)obj;
                                int number = pRIndirectReference.Number;
                                if (!hits[number])
                                {
                                    hits[number] = true;
                                    stack.Push(GetPdfObjectRelease(pRIndirectReference));
                                }

                                continue;
                            }
                        default:
                            continue;
                    }
                }
                else
                {
                    array2 = (object[])obj2;
                    if (array2[0] is List<PdfObject>)
                    {
                        list = (List<PdfObject>)array2[0];
                        num = (int)array2[1];
                    }
                    else
                    {
                        array = (PdfName[])array2[0];
                        pdfDictionary = (PdfDictionary)array2[1];
                        num = (int)array2[2];
                    }
                }

                if (list != null)
                {
                    int num2 = num;
                    while (num2 < list.Count)
                    {
                        PdfObject pdfObject = list[num2];
                        if (pdfObject.IsIndirect())
                        {
                            int number2 = ((PRIndirectReference)pdfObject).Number;
                            if (number2 >= xrefObj.Count || (!partial && xrefObj[number2] == null))
                            {
                                list[num2] = PdfNull.PDFNULL;
                                num2++;
                                continue;
                            }
                        }

                        if (array2 == null)
                        {
                            stack.Push(new object[2]
                            {
                                list,
                                num2 + 1
                            });
                        }
                        else
                        {
                            array2[1] = num2 + 1;
                            stack.Push(array2);
                        }

                        stack.Push(pdfObject);
                        break;
                    }

                    continue;
                }

                int num3 = num;
                while (num3 < array.Length)
                {
                    PdfName key = array[num3];
                    PdfObject pdfObject2 = pdfDictionary.Get(key);
                    if (pdfObject2.IsIndirect())
                    {
                        int number3 = ((PRIndirectReference)pdfObject2).Number;
                        if (number3 < 0 || number3 >= xrefObj.Count || (!partial && xrefObj[number3] == null))
                        {
                            pdfDictionary.Put(key, PdfNull.PDFNULL);
                            num3++;
                            continue;
                        }
                    }

                    if (array2 == null)
                    {
                        stack.Push(new object[3]
                        {
                            array,
                            pdfDictionary,
                            num3 + 1
                        });
                    }
                    else
                    {
                        array2[2] = num3 + 1;
                        stack.Push(array2);
                    }

                    stack.Push(pdfObject2);
                    break;
                }
            }
        }

        public virtual int RemoveUnusedObjects()
        {
            bool[] array = new bool[xrefObj.Count];
            RemoveUnusedNode(trailer, array);
            int num = 0;
            if (partial)
            {
                for (int i = 1; i < array.Length; i++)
                {
                    if (!array[i])
                    {
                        xref[i * 2] = -1L;
                        xref[i * 2 + 1] = 0L;
                        xrefObj[i] = null;
                        num++;
                    }
                }
            }
            else
            {
                for (int j = 1; j < array.Length; j++)
                {
                    if (!array[j])
                    {
                        xrefObj[j] = null;
                        num++;
                    }
                }
            }

            return num;
        }

        public virtual string GetJavaScript(RandomAccessFileOrArray file)
        {
            PdfDictionary pdfDictionary = (PdfDictionary)GetPdfObjectRelease(catalog.Get(PdfName.NAMES));
            if (pdfDictionary == null)
            {
                return null;
            }

            PdfDictionary pdfDictionary2 = (PdfDictionary)GetPdfObjectRelease(pdfDictionary.Get(PdfName.JAVASCRIPT));
            if (pdfDictionary2 == null)
            {
                return null;
            }

            Dictionary<string, PdfObject> dictionary = PdfNameTree.ReadTree(pdfDictionary2);
            string[] array = new string[dictionary.Count];
            dictionary.Keys.CopyTo(array, 0);
            Array.Sort(array);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                PdfDictionary pdfDictionary3 = (PdfDictionary)GetPdfObjectRelease(dictionary[array[i]]);
                if (pdfDictionary3 == null)
                {
                    continue;
                }

                PdfObject pdfObjectRelease = GetPdfObjectRelease(pdfDictionary3.Get(PdfName.JS));
                if (pdfObjectRelease == null)
                {
                    continue;
                }

                if (pdfObjectRelease.IsString())
                {
                    stringBuilder.Append(((PdfString)pdfObjectRelease).ToUnicodeString()).Append('\n');
                }
                else if (pdfObjectRelease.IsStream())
                {
                    byte[] streamBytes = GetStreamBytes((PRStream)pdfObjectRelease, file);
                    if (streamBytes.Length >= 2 && streamBytes[0] == 254 && streamBytes[1] == byte.MaxValue)
                    {
                        stringBuilder.Append(PdfEncodings.ConvertToString(streamBytes, "UnicodeBig"));
                    }
                    else
                    {
                        stringBuilder.Append(PdfEncodings.ConvertToString(streamBytes, "PDF"));
                    }

                    stringBuilder.Append('\n');
                }
            }

            return stringBuilder.ToString();
        }

        public virtual void SelectPages(string ranges)
        {
            SelectPages(SequenceList.Expand(ranges, NumberOfPages));
        }

        public virtual void SelectPages(ICollection<int> pagesToKeep)
        {
            SelectPages(pagesToKeep, removeUnused: true);
        }

        internal void SelectPages(ICollection<int> pagesToKeep, bool removeUnused)
        {
            pageRefs.SelectPages(pagesToKeep);
            if (removeUnused)
            {
                RemoveUnusedObjects();
            }
        }

        public virtual void AddViewerPreference(PdfName key, PdfObject value)
        {
            viewerPreferences.AddViewerPreference(key, value);
            SetViewerPreferences(viewerPreferences);
        }

        public virtual void SetViewerPreferences(PdfViewerPreferencesImp vp)
        {
            vp.AddToCatalog(catalog);
        }

        public virtual bool IsNewXrefType()
        {
            return newXrefType;
        }

        public virtual bool IsHybridXref()
        {
            return hybridXref;
        }

        internal PdfIndirectReference GetCryptoRef()
        {
            if (cryptoRef == null)
            {
                return null;
            }

            return new PdfIndirectReference(0, cryptoRef.Number, cryptoRef.Generation);
        }

        public virtual bool HasUsageRights()
        {
            PdfDictionary asDict = catalog.GetAsDict(PdfName.PERMS);
            if (asDict == null)
            {
                return false;
            }

            if (!asDict.Contains(PdfName.UR))
            {
                return asDict.Contains(PdfName.UR3);
            }

            return true;
        }

        public virtual void RemoveUsageRights()
        {
            PdfDictionary asDict = catalog.GetAsDict(PdfName.PERMS);
            if (asDict != null)
            {
                asDict.Remove(PdfName.UR);
                asDict.Remove(PdfName.UR3);
                if (asDict.Size == 0)
                {
                    catalog.Remove(PdfName.PERMS);
                }
            }
        }

        public virtual int GetCertificationLevel()
        {
            PdfDictionary asDict = catalog.GetAsDict(PdfName.PERMS);
            if (asDict == null)
            {
                return 0;
            }

            asDict = asDict.GetAsDict(PdfName.DOCMDP);
            if (asDict == null)
            {
                return 0;
            }

            PdfArray asArray = asDict.GetAsArray(PdfName.REFERENCE);
            if (asArray == null || asArray.Size == 0)
            {
                return 0;
            }

            asDict = asArray.GetAsDict(0);
            if (asDict == null)
            {
                return 0;
            }

            asDict = asDict.GetAsDict(PdfName.TRANSFORMPARAMS);
            if (asDict == null)
            {
                return 0;
            }

            return asDict.GetAsNumber(PdfName.P)?.IntValue ?? 0;
        }

        public virtual int GetCryptoMode()
        {
            if (decrypt == null)
            {
                return -1;
            }

            return decrypt.GetCryptoMode();
        }

        public virtual bool IsMetadataEncrypted()
        {
            if (decrypt == null)
            {
                return false;
            }

            return decrypt.IsMetadataEncrypted();
        }

        public virtual byte[] ComputeUserPassword()
        {
            if (!encrypted || !ownerPasswordUsed)
            {
                return null;
            }

            return decrypt.ComputeUserPassword(password);
        }

        public virtual void Dispose()
        {
            Close();
        }
    }
}
