using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;
using Sign.itext.text.log;
using Sign.itext.text.pdf.collection;
using Sign.itext.text.pdf.events;
using Sign.itext.text.pdf.interfaces;
using Sign.itext.text.pdf.intern;
using Sign.itext.xml.xmp;
using Sign.itext.xml.xmp.options;
using Sign.Org.BouncyCastle.Utilities;
using Sign.Org.BouncyCastle.X509;
using Sign.SystemItext.util;
using Sign.SystemItext.util.collections;
using System.Collections;

namespace Sign.itext.text.pdf
{
    public class PdfWriter : DocWriter, IPdfViewerPreferences, IPdfEncryptionSettings, IPdfVersion, IPdfDocumentActions, IPdfPageActions, IPdfIsoConformance, IPdfRunDirection, IPdfAnnotations
    {
        public class PdfBody
        {
            public class PdfCrossReference : IComparable
            {
                private readonly int type;

                private readonly long offset;

                private readonly int refnum;

                private readonly int generation;

                public virtual int Refnum => refnum;

                public PdfCrossReference(int refnum, long offset, int generation)
                {
                    type = 0;
                    this.offset = offset;
                    this.refnum = refnum;
                    this.generation = generation;
                }

                public PdfCrossReference(int refnum, long offset)
                {
                    type = 1;
                    this.offset = offset;
                    this.refnum = refnum;
                    generation = 0;
                }

                public PdfCrossReference(int type, int refnum, long offset, int generation)
                {
                    this.type = type;
                    this.offset = offset;
                    this.refnum = refnum;
                    this.generation = generation;
                }

                public virtual void ToPdf(Stream os)
                {
                    long num = offset;
                    string str = num.ToString().PadLeft(10, '0');
                    int num2 = generation;
                    string str2 = num2.ToString().PadLeft(5, '0');
                    ByteBuffer byteBuffer = new ByteBuffer(40);
                    if (generation == 65535)
                    {
                        byteBuffer.Append(str).Append(' ').Append(str2)
                            .Append(" f \n");
                    }
                    else
                    {
                        byteBuffer.Append(str).Append(' ').Append(str2)
                            .Append(" n \n");
                    }

                    os.Write(byteBuffer.Buffer, 0, byteBuffer.Size);
                }

                public virtual void ToPdf(int midSize, Stream os)
                {
                    os.WriteByte((byte)type);
                    while (--midSize >= 0)
                    {
                        os.WriteByte((byte)((offset >> 8 * midSize) & 0xFF));
                    }

                    os.WriteByte((byte)((uint)(generation >> 8) & 0xFFu));
                    os.WriteByte((byte)((uint)generation & 0xFFu));
                }

                public virtual int CompareTo(object o)
                {
                    PdfCrossReference pdfCrossReference = (PdfCrossReference)o;
                    if (refnum >= pdfCrossReference.refnum)
                    {
                        if (refnum != pdfCrossReference.refnum)
                        {
                            return 1;
                        }

                        return 0;
                    }

                    return -1;
                }

                public override bool Equals(object obj)
                {
                    if (obj is PdfCrossReference)
                    {
                        PdfCrossReference pdfCrossReference = (PdfCrossReference)obj;
                        return refnum == pdfCrossReference.refnum;
                    }

                    return false;
                }

                public override int GetHashCode()
                {
                    return refnum;
                }
            }

            private const int OBJSINSTREAM = 200;

            protected internal OrderedTree xrefs;

            protected int refnum;

            protected long position;

            protected PdfWriter writer;

            protected ByteBuffer index;

            protected ByteBuffer streamObjects;

            protected int currentObjNum;

            protected int numObj;

            internal int Refnum
            {
                set
                {
                    refnum = value;
                }
            }

            public virtual PdfIndirectReference PdfIndirectReference => new PdfIndirectReference(0, IndirectReferenceNumber);

            protected internal virtual int IndirectReferenceNumber
            {
                get
                {
                    int result = refnum++;
                    xrefs[new PdfCrossReference(result, 0L, 65535)] = null;
                    return result;
                }
            }

            public virtual long Offset => position;

            public virtual int Size => Math.Max(((PdfCrossReference)xrefs.GetMaxKey()).Refnum + 1, refnum);

            protected internal PdfBody(PdfWriter writer)
            {
                xrefs = new OrderedTree();
                xrefs[new PdfCrossReference(0, 0L, 65535)] = null;
                position = writer.Os.Counter;
                refnum = 1;
                this.writer = writer;
            }

            protected virtual PdfCrossReference AddToObjStm(PdfObject obj, int nObj)
            {
                if (numObj >= 200)
                {
                    FlushObjStm();
                }

                if (index == null)
                {
                    index = new ByteBuffer();
                    streamObjects = new ByteBuffer();
                    currentObjNum = IndirectReferenceNumber;
                    numObj = 0;
                }

                int size = streamObjects.Size;
                int generation = numObj++;
                PdfEncryption crypto = writer.crypto;
                writer.crypto = null;
                obj.ToPdf(writer, streamObjects);
                writer.crypto = crypto;
                streamObjects.Append(' ');
                index.Append(nObj).Append(' ').Append(size)
                    .Append(' ');
                return new PdfCrossReference(2, nObj, currentObjNum, generation);
            }

            public virtual void FlushObjStm()
            {
                if (numObj != 0)
                {
                    int size = index.Size;
                    index.Append(streamObjects);
                    PdfStream pdfStream = new PdfStream(index.ToByteArray());
                    pdfStream.FlateCompress(writer.CompressionLevel);
                    pdfStream.Put(PdfName.TYPE, PdfName.OBJSTM);
                    pdfStream.Put(PdfName.N, new PdfNumber(numObj));
                    pdfStream.Put(PdfName.FIRST, new PdfNumber(size));
                    Add(pdfStream, currentObjNum);
                    index = null;
                    streamObjects = null;
                    numObj = 0;
                }
            }

            internal PdfIndirectObject Add(PdfObject objecta)
            {
                return Add(objecta, IndirectReferenceNumber);
            }

            internal PdfIndirectObject Add(PdfObject objecta, bool inObjStm)
            {
                return Add(objecta, IndirectReferenceNumber, 0, inObjStm);
            }

            internal PdfIndirectObject Add(PdfObject objecta, PdfIndirectReference refa)
            {
                return Add(objecta, refa, inObjStm: true);
            }

            internal PdfIndirectObject Add(PdfObject objecta, PdfIndirectReference refa, bool inObjStm)
            {
                return Add(objecta, refa.Number, refa.Generation, inObjStm);
            }

            internal PdfIndirectObject Add(PdfObject objecta, int refNumber)
            {
                return Add(objecta, refNumber, 0, inObjStm: true);
            }

            protected internal virtual PdfIndirectObject Add(PdfObject objecta, int refNumber, int generation, bool inObjStm)
            {
                if (inObjStm && objecta.CanBeInObjStm() && writer.FullCompression)
                {
                    PdfCrossReference key = AddToObjStm(objecta, refNumber);
                    PdfIndirectObject result = new PdfIndirectObject(refNumber, objecta, writer);
                    xrefs.Remove(key);
                    xrefs[key] = null;
                    return result;
                }

                PdfIndirectObject pdfIndirectObject;
                if (writer.FullCompression)
                {
                    pdfIndirectObject = new PdfIndirectObject(refNumber, objecta, writer);
                    Write(pdfIndirectObject, refNumber);
                }
                else
                {
                    pdfIndirectObject = new PdfIndirectObject(refNumber, generation, objecta, writer);
                    Write(pdfIndirectObject, refNumber, generation);
                }

                return pdfIndirectObject;
            }

            protected internal virtual void Write(PdfIndirectObject indirect, int refNumber, int generation)
            {
                PdfCrossReference key = new PdfCrossReference(refNumber, position, generation);
                xrefs.Remove(key);
                xrefs[key] = null;
                indirect.WriteTo(writer.Os);
                position = writer.Os.Counter;
            }

            protected internal virtual void Write(PdfIndirectObject indirect, int refNumber)
            {
                PdfCrossReference key = new PdfCrossReference(refNumber, position);
                xrefs.Remove(key);
                xrefs[key] = null;
                indirect.WriteTo(writer.Os);
                position = writer.Os.Counter;
            }

            public virtual void WriteCrossReferenceTable(Stream os, PdfIndirectReference root, PdfIndirectReference info, PdfIndirectReference encryption, PdfObject fileID, long prevxref)
            {
                int number = 0;
                if (writer.FullCompression)
                {
                    FlushObjStm();
                    number = IndirectReferenceNumber;
                    xrefs[new PdfCrossReference(number, position)] = null;
                }

                int num = ((PdfCrossReference)xrefs.GetMinKey()).Refnum;
                int num2 = 0;
                List<int> list = new List<int>();
                foreach (PdfCrossReference key in xrefs.Keys)
                {
                    if (num + num2 == key.Refnum)
                    {
                        num2++;
                        continue;
                    }

                    list.Add(num);
                    list.Add(num2);
                    num = key.Refnum;
                    num2 = 1;
                }

                list.Add(num);
                list.Add(num2);
                if (writer.FullCompression)
                {
                    int num3 = 5;
                    long num4 = 1095216660480L;
                    while (num3 > 1 && (num4 & position) == 0L)
                    {
                        num4 >>= 8;
                        num3--;
                    }

                    ByteBuffer byteBuffer = new ByteBuffer();
                    foreach (PdfCrossReference key2 in xrefs.Keys)
                    {
                        key2.ToPdf(num3, byteBuffer);
                    }

                    PdfStream pdfStream = new PdfStream(byteBuffer.ToByteArray());
                    byteBuffer = null;
                    pdfStream.FlateCompress(writer.CompressionLevel);
                    pdfStream.Put(PdfName.SIZE, new PdfNumber(Size));
                    pdfStream.Put(PdfName.ROOT, root);
                    if (info != null)
                    {
                        pdfStream.Put(PdfName.INFO, info);
                    }

                    if (encryption != null)
                    {
                        pdfStream.Put(PdfName.ENCRYPT, encryption);
                    }

                    if (fileID != null)
                    {
                        pdfStream.Put(PdfName.ID, fileID);
                    }

                    pdfStream.Put(PdfName.W, new PdfArray(new int[3] { 1, num3, 2 }));
                    pdfStream.Put(PdfName.TYPE, PdfName.XREF);
                    PdfArray pdfArray = new PdfArray();
                    for (int i = 0; i < list.Count; i++)
                    {
                        pdfArray.Add(new PdfNumber(list[i]));
                    }

                    pdfStream.Put(PdfName.INDEX, pdfArray);
                    if (prevxref > 0)
                    {
                        pdfStream.Put(PdfName.PREV, new PdfNumber(prevxref));
                    }

                    PdfEncryption crypto = writer.crypto;
                    writer.crypto = null;
                    new PdfIndirectObject(number, pdfStream, writer).WriteTo(writer.Os);
                    writer.crypto = crypto;
                    return;
                }

                byte[] iSOBytes = DocWriter.GetISOBytes("xref\n");
                os.Write(iSOBytes, 0, iSOBytes.Length);
                IEnumerator keys = xrefs.Keys;
                keys.MoveNext();
                for (int j = 0; j < list.Count; j += 2)
                {
                    num = list[j];
                    num2 = list[j + 1];
                    iSOBytes = DocWriter.GetISOBytes(num.ToString());
                    os.Write(iSOBytes, 0, iSOBytes.Length);
                    os.WriteByte(32);
                    iSOBytes = DocWriter.GetISOBytes(num2.ToString());
                    os.Write(iSOBytes, 0, iSOBytes.Length);
                    os.WriteByte(10);
                    while (num2-- > 0)
                    {
                        ((PdfCrossReference)keys.Current).ToPdf(os);
                        keys.MoveNext();
                    }
                }
            }
        }

        public class PdfTrailer : PdfDictionary
        {
            internal long offset;

            public PdfTrailer(int size, long offset, PdfIndirectReference root, PdfIndirectReference info, PdfIndirectReference encryption, PdfObject fileID, long prevxref)
            {
                this.offset = offset;
                Put(PdfName.SIZE, new PdfNumber(size));
                Put(PdfName.ROOT, root);
                if (info != null)
                {
                    Put(PdfName.INFO, info);
                }

                if (encryption != null)
                {
                    Put(PdfName.ENCRYPT, encryption);
                }

                if (fileID != null)
                {
                    Put(PdfName.ID, fileID);
                }

                if (prevxref > 0)
                {
                    Put(PdfName.PREV, new PdfNumber(prevxref));
                }
            }

            public override void ToPdf(PdfWriter writer, Stream os)
            {
                CheckPdfIsoConformance(writer, 8, this);
                byte[] iSOBytes = DocWriter.GetISOBytes("trailer\n");
                os.Write(iSOBytes, 0, iSOBytes.Length);
                base.ToPdf((PdfWriter)null, os);
                iSOBytes = DocWriter.GetISOBytes("startxref\n");
                os.Write(new byte[1] { 10 }, 0, 1);
                WriteKeyInfo(os);
                os.Write(iSOBytes, 0, iSOBytes.Length);
                iSOBytes = DocWriter.GetISOBytes(offset.ToString());
                os.Write(iSOBytes, 0, iSOBytes.Length);
                iSOBytes = DocWriter.GetISOBytes("\n%%EOF\n");
                os.Write(iSOBytes, 0, iSOBytes.Length);
            }
        }

        public const int GENERATION_MAX = 65535;

        protected static ICounter COUNTER = CounterFactory.GetCounter(typeof(PdfWriter));

        protected internal PdfDocument pdf;

        protected PdfContentByte directContent;

        protected PdfContentByte directContentUnder;

        protected internal PdfBody body;

        protected ICC_Profile colorProfile;

        protected internal PdfDictionary extraCatalog;

        protected PdfPages root;

        internal List<PdfIndirectReference> pageReferences = new List<PdfIndirectReference>();

        protected int currentPageNumber = 1;

        protected PdfName tabs;

        protected PdfDictionary pageDictEntries = new PdfDictionary();

        private IPdfPageEvent pageEvent;

        protected long prevxref;

        protected byte[] originalFileID;

        protected IList<Dictionary<string, object>> newBookmarks;

        public const char VERSION_1_2 = '2';

        public const char VERSION_1_3 = '3';

        public const char VERSION_1_4 = '4';

        public const char VERSION_1_5 = '5';

        public const char VERSION_1_6 = '6';

        public const char VERSION_1_7 = '7';

        public static readonly PdfName PDF_VERSION_1_2 = new PdfName("1.2");

        public static readonly PdfName PDF_VERSION_1_3 = new PdfName("1.3");

        public static readonly PdfName PDF_VERSION_1_4 = new PdfName("1.4");

        public static readonly PdfName PDF_VERSION_1_5 = new PdfName("1.5");

        public static readonly PdfName PDF_VERSION_1_6 = new PdfName("1.6");

        public static readonly PdfName PDF_VERSION_1_7 = new PdfName("1.7");

        protected PdfVersionImp pdf_version = new PdfVersionImp();

        public const int PageLayoutSinglePage = 1;

        public const int PageLayoutOneColumn = 2;

        public const int PageLayoutTwoColumnLeft = 4;

        public const int PageLayoutTwoColumnRight = 8;

        public const int PageLayoutTwoPageLeft = 16;

        public const int PageLayoutTwoPageRight = 32;

        public const int PageModeUseNone = 64;

        public const int PageModeUseOutlines = 128;

        public const int PageModeUseThumbs = 256;

        public const int PageModeFullScreen = 512;

        public const int PageModeUseOC = 1024;

        public const int PageModeUseAttachments = 2048;

        public const int HideToolbar = 4096;

        public const int HideMenubar = 8192;

        public const int HideWindowUI = 16384;

        public const int FitWindow = 32768;

        public const int CenterWindow = 65536;

        public const int DisplayDocTitle = 131072;

        public const int NonFullScreenPageModeUseNone = 262144;

        public const int NonFullScreenPageModeUseOutlines = 524288;

        public const int NonFullScreenPageModeUseThumbs = 1048576;

        public const int NonFullScreenPageModeUseOC = 2097152;

        public const int DirectionL2R = 4194304;

        public const int DirectionR2L = 8388608;

        public const int PrintScalingNone = 16777216;

        public static PdfName DOCUMENT_CLOSE = PdfName.WC;

        public static PdfName WILL_SAVE = PdfName.WS;

        public static PdfName DID_SAVE = PdfName.DS;

        public static PdfName WILL_PRINT = PdfName.WP;

        public static PdfName DID_PRINT = PdfName.DP;

        public const int SIGNATURE_EXISTS = 1;

        public const int SIGNATURE_APPEND_ONLY = 2;

        protected byte[] xmpMetadata;

        protected XmpWriter xmpWriter;

        public const int PDFXNONE = 0;

        public const int PDFX1A2001 = 1;

        public const int PDFX32002 = 2;

        protected IPdfIsoConformance pdfIsoConformance;

        public const int STANDARD_ENCRYPTION_40 = 0;

        public const int STANDARD_ENCRYPTION_128 = 1;

        public const int ENCRYPTION_AES_128 = 2;

        public const int ENCRYPTION_AES_256 = 3;

        internal const int ENCRYPTION_MASK = 7;

        public const int DO_NOT_ENCRYPT_METADATA = 8;

        public const int EMBEDDED_FILES_ONLY = 24;

        public const int ALLOW_PRINTING = 2052;

        public const int ALLOW_MODIFY_CONTENTS = 8;

        public const int ALLOW_COPY = 16;

        public const int ALLOW_MODIFY_ANNOTATIONS = 32;

        public const int ALLOW_FILL_IN = 256;

        public const int ALLOW_SCREENREADERS = 512;

        public const int ALLOW_ASSEMBLY = 1024;

        public const int ALLOW_DEGRADED_PRINTING = 4;

        public const int AllowPrinting = 2052;

        public const int AllowModifyContents = 8;

        public const int AllowCopy = 16;

        public const int AllowModifyAnnotations = 32;

        public const int AllowFillIn = 256;

        public const int AllowScreenReaders = 512;

        public const int AllowAssembly = 1024;

        public const int AllowDegradedPrinting = 4;

        public const bool STRENGTH40BITS = false;

        public const bool STRENGTH128BITS = true;

        protected PdfEncryption crypto;

        internal bool fullCompression;

        protected internal int compressionLevel = -1;

        protected Dictionary<BaseFont, FontDetails> documentFonts = new Dictionary<BaseFont, FontDetails>();

        protected int fontNumber = 1;

        protected Dictionary<PdfIndirectReference, object[]> formXObjects = new Dictionary<PdfIndirectReference, object[]>();

        protected int formXObjectsCounter = 1;

        protected Dictionary<PdfReader, PdfReaderInstance> readerInstances = new Dictionary<PdfReader, PdfReaderInstance>();

        protected PdfReaderInstance currentPdfReaderInstance;

        protected Dictionary<ICachedColorSpace, ColorDetails> documentColors = new Dictionary<ICachedColorSpace, ColorDetails>();

        protected int colorNumber = 1;

        protected Dictionary<PdfPatternPainter, PdfName> documentPatterns = new Dictionary<PdfPatternPainter, PdfName>();

        protected int patternNumber = 1;

        protected Dictionary<PdfShadingPattern, object> documentShadingPatterns = new Dictionary<PdfShadingPattern, object>();

        protected Dictionary<PdfShading, object> documentShadings = new Dictionary<PdfShading, object>();

        protected Dictionary<PdfDictionary, PdfObject[]> documentExtGState = new Dictionary<PdfDictionary, PdfObject[]>();

        protected Dictionary<object, PdfObject[]> documentProperties = new Dictionary<object, PdfObject[]>();

        public const int markAll = 0;

        public const int markInlineElementsOnly = 1;

        protected bool tagged;

        protected int taggingMode = 1;

        protected PdfStructureTreeRoot structureTreeRoot;

        protected Dictionary<IPdfOCG, object> documentOCG = new Dictionary<IPdfOCG, object>();

        protected List<IPdfOCG> documentOCGorder = new List<IPdfOCG>();

        protected PdfOCProperties vOCProperties;

        protected PdfArray OCGRadioGroup = new PdfArray();

        protected PdfArray OCGLocked = new PdfArray();

        public static readonly PdfName PAGE_OPEN = PdfName.O;

        public static readonly PdfName PAGE_CLOSE = PdfName.C;

        protected PdfDictionary group;

        public const float SPACE_CHAR_RATIO_DEFAULT = 2.5f;

        public const float NO_SPACE_CHAR_RATIO = 1E+07f;

        private float spaceCharRatio = 2.5f;

        public const int RUN_DIRECTION_DEFAULT = 0;

        public const int RUN_DIRECTION_NO_BIDI = 1;

        public const int RUN_DIRECTION_LTR = 2;

        public const int RUN_DIRECTION_RTL = 3;

        protected int runDirection = 1;

        protected PdfDictionary defaultColorspace = new PdfDictionary();

        protected Dictionary<ColorDetails, ColorDetails> documentSpotPatterns = new Dictionary<ColorDetails, ColorDetails>();

        protected ColorDetails patternColorspaceRGB;

        protected ColorDetails patternColorspaceGRAY;

        protected ColorDetails patternColorspaceCMYK;

        protected PdfDictionary imageDictionary = new PdfDictionary();

        private Dictionary<long, PdfName> images = new Dictionary<long, PdfName>();

        protected Dictionary<PdfStream, PdfIndirectReference> JBIG2Globals = new Dictionary<PdfStream, PdfIndirectReference>();

        private bool userProperties;

        private bool rgbTransparencyBlending;

        protected TtfUnicodeWriter ttfUnicodeWriter;

        private static readonly List<PdfName> standardStructElems_1_4 = new List<PdfName>(new PdfName[38]
        {
            PdfName.DOCUMENT,
            PdfName.PART,
            PdfName.ART,
            PdfName.SECT,
            PdfName.DIV,
            PdfName.BLOCKQUOTE,
            PdfName.CAPTION,
            PdfName.TOC,
            PdfName.TOCI,
            PdfName.INDEX,
            PdfName.NONSTRUCT,
            PdfName.PRIVATE,
            PdfName.P,
            PdfName.H,
            PdfName.H1,
            PdfName.H2,
            PdfName.H3,
            PdfName.H4,
            PdfName.H5,
            PdfName.H6,
            PdfName.L,
            PdfName.LBL,
            PdfName.LI,
            PdfName.LBODY,
            PdfName.TABLE,
            PdfName.TR,
            PdfName.TH,
            PdfName.TD,
            PdfName.SPAN,
            PdfName.QUOTE,
            PdfName.NOTE,
            PdfName.REFERENCE,
            PdfName.BIBENTRY,
            PdfName.CODE,
            PdfName.LINK,
            PdfName.FIGURE,
            PdfName.FORMULA,
            PdfName.FORM
        });

        private static readonly List<PdfName> standardStructElems_1_7 = new List<PdfName>(new PdfName[49]
        {
            PdfName.DOCUMENT,
            PdfName.PART,
            PdfName.ART,
            PdfName.SECT,
            PdfName.DIV,
            PdfName.BLOCKQUOTE,
            PdfName.CAPTION,
            PdfName.TOC,
            PdfName.TOCI,
            PdfName.INDEX,
            PdfName.NONSTRUCT,
            PdfName.PRIVATE,
            PdfName.P,
            PdfName.H,
            PdfName.H1,
            PdfName.H2,
            PdfName.H3,
            PdfName.H4,
            PdfName.H5,
            PdfName.H6,
            PdfName.L,
            PdfName.LBL,
            PdfName.LI,
            PdfName.LBODY,
            PdfName.TABLE,
            PdfName.TR,
            PdfName.TH,
            PdfName.TD,
            PdfName.THEAD,
            PdfName.TBODY,
            PdfName.TFOOT,
            PdfName.SPAN,
            PdfName.QUOTE,
            PdfName.NOTE,
            PdfName.REFERENCE,
            PdfName.BIBENTRY,
            PdfName.CODE,
            PdfName.LINK,
            PdfName.ANNOT,
            PdfName.RUBY,
            PdfName.RB,
            PdfName.RT,
            PdfName.RP,
            PdfName.WARICHU,
            PdfName.WT,
            PdfName.WP,
            PdfName.FIGURE,
            PdfName.FORMULA,
            PdfName.FORM
        });

        internal PdfDocument PdfDocument => pdf;

        public virtual PdfDictionary Info => ((PdfDocument)document).Info;

        public virtual float InitialLeading
        {
            set
            {
                if (open)
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("you.can.t.set.the.initial.leading.if.the.document.is.already.open"));
                }

                pdf.Leading = value;
            }
        }

        public virtual PdfContentByte DirectContent
        {
            get
            {
                if (!open)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("the.document.is.not.open"));
                }

                return directContent;
            }
        }

        public virtual PdfContentByte DirectContentUnder
        {
            get
            {
                if (!open)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("the.document.is.not.open"));
                }

                return directContentUnder;
            }
        }

        public virtual ICC_Profile ColorProfile => colorProfile;

        public virtual PdfIndirectReference PdfIndirectReference => body.PdfIndirectReference;

        protected internal virtual int IndirectReferenceNumber => body.IndirectReferenceNumber;

        public virtual OutputStreamCounter Os => os;

        public virtual PdfDictionary ExtraCatalog
        {
            get
            {
                if (extraCatalog == null)
                {
                    extraCatalog = new PdfDictionary();
                }

                return extraCatalog;
            }
        }

        public virtual PdfDictionary PageDictEntries => pageDictEntries;

        public virtual int PageNumber => pdf.PageNumber;

        internal virtual PdfIndirectReference CurrentPage => GetPageReference(currentPageNumber);

        public virtual int CurrentPageNumber => currentPageNumber;

        public virtual PdfName Tabs
        {
            get
            {
                return tabs;
            }
            set
            {
                tabs = value;
            }
        }

        public virtual IPdfPageEvent PageEvent
        {
            get
            {
                return pageEvent;
            }
            set
            {
                if (value == null)
                {
                    pageEvent = null;
                    return;
                }

                if (pageEvent == null)
                {
                    pageEvent = value;
                    return;
                }

                if (pageEvent is PdfPageEventForwarder)
                {
                    ((PdfPageEventForwarder)pageEvent).AddPageEvent(value);
                    return;
                }

                PdfPageEventForwarder pdfPageEventForwarder = new PdfPageEventForwarder();
                pdfPageEventForwarder.AddPageEvent(pageEvent);
                pdfPageEventForwarder.AddPageEvent(value);
                pageEvent = pdfPageEventForwarder;
            }
        }

        public virtual PdfOutline RootOutline => directContent.RootOutline;

        public virtual IList<Dictionary<string, object>> Outlines
        {
            set
            {
                newBookmarks = value;
            }
        }

        public virtual char PdfVersion
        {
            set
            {
                pdf_version.PdfVersion = value;
            }
        }

        public virtual int ViewerPreferences
        {
            set
            {
                pdf.ViewerPreferences = value;
            }
        }

        public virtual PdfPageLabels PageLabels
        {
            set
            {
                pdf.PageLabels = value;
            }
        }

        public virtual PdfCollection Collection
        {
            set
            {
                SetAtLeastPdfVersion('7');
                pdf.Collection = value;
            }
        }

        public virtual PdfAcroForm AcroForm => pdf.AcroForm;

        public virtual int SigFlags
        {
            set
            {
                pdf.SigFlags = value;
            }
        }

        public virtual byte[] XmpMetadata
        {
            get
            {
                return xmpMetadata;
            }
            set
            {
                xmpMetadata = value;
            }
        }

        public virtual byte[] PageXmpMetadata
        {
            set
            {
                pdf.XmpMetadata = value;
            }
        }

        public virtual XmpWriter XmpWriter => xmpWriter;

        public virtual int PDFXConformance
        {
            get
            {
                if (pdfIsoConformance is PdfXConformanceImp)
                {
                    return ((IPdfXConformance)pdfIsoConformance).PDFXConformance;
                }

                return 0;
            }
            set
            {
                if (pdfIsoConformance is PdfXConformanceImp && ((IPdfXConformance)pdfIsoConformance).PDFXConformance != value)
                {
                    if (pdf.IsOpen())
                    {
                        throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("pdfx.conformance.can.only.be.set.before.opening.the.document"));
                    }

                    if (crypto != null)
                    {
                        throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("a.pdfx.conforming.document.cannot.be.encrypted"));
                    }

                    if (value != 0)
                    {
                        PdfVersion = '3';
                    }

                    ((IPdfXConformance)pdfIsoConformance).PDFXConformance = value;
                }
            }
        }

        internal PdfEncryption Encryption => crypto;

        public virtual bool FullCompression => fullCompression;

        public virtual int CompressionLevel
        {
            get
            {
                return compressionLevel;
            }
            set
            {
                if (value < 0 || value > 9)
                {
                    compressionLevel = -1;
                }
                else
                {
                    compressionLevel = value;
                }
            }
        }

        public virtual long CurrentDocumentSize => body.Offset + body.Size * 20 + 72;

        public virtual PdfStructureTreeRoot StructureTreeRoot
        {
            get
            {
                if (tagged && structureTreeRoot == null)
                {
                    structureTreeRoot = new PdfStructureTreeRoot(this);
                }

                return structureTreeRoot;
            }
        }

        public virtual PdfOCProperties OCProperties
        {
            get
            {
                FillOCProperties(erase: true);
                return vOCProperties;
            }
        }

        public virtual Rectangle PageSize => pdf.PageSize;

        public virtual Rectangle CropBoxSize
        {
            set
            {
                pdf.CropBoxSize = value;
            }
        }

        public virtual bool PageEmpty
        {
            get
            {
                return pdf.PageEmpty;
            }
            set
            {
                if (!value)
                {
                    pdf.PageEmpty = value;
                }
            }
        }

        public virtual int Duration
        {
            set
            {
                pdf.Duration = value;
            }
        }

        public virtual PdfTransition Transition
        {
            set
            {
                pdf.Transition = value;
            }
        }

        public virtual Image Thumbnail
        {
            set
            {
                pdf.Thumbnail = value;
            }
        }

        public virtual PdfDictionary Group
        {
            get
            {
                return group;
            }
            set
            {
                group = value;
            }
        }

        public virtual float SpaceCharRatio
        {
            get
            {
                return spaceCharRatio;
            }
            set
            {
                if (value < 0.001f)
                {
                    spaceCharRatio = 0.001f;
                }
                else
                {
                    spaceCharRatio = value;
                }
            }
        }

        public virtual int RunDirection
        {
            get
            {
                return runDirection;
            }
            set
            {
                if (value < 1 || value > 3)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("invalid.run.direction.1", value));
                }

                runDirection = value;
            }
        }

        public virtual float Userunit
        {
            set
            {
                if (value < 1f || value > 75000f)
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("userunit.should.be.a.value.between.1.and.75000"));
                }

                AddPageDictEntry(PdfName.USERUNIT, new PdfNumber(value));
                SetAtLeastPdfVersion('6');
            }
        }

        public virtual PdfDictionary DefaultColorspace => defaultColorspace;

        public virtual bool StrictImageSequence
        {
            get
            {
                return pdf.StrictImageSequence;
            }
            set
            {
                pdf.StrictImageSequence = value;
            }
        }

        public virtual bool UserProperties
        {
            get
            {
                return userProperties;
            }
            set
            {
                userProperties = value;
            }
        }

        public virtual bool RgbTransparencyBlending
        {
            get
            {
                return rgbTransparencyBlending;
            }
            set
            {
                rgbTransparencyBlending = value;
            }
        }

        protected virtual ICounter GetCounter()
        {
            return COUNTER;
        }

        protected PdfWriter()
        {
            pdfIsoConformance = InitPdfIsoConformance();
            root = new PdfPages(this);
        }

        protected PdfWriter(PdfDocument document, Stream os)
            : base(document, os)
        {
            pdfIsoConformance = InitPdfIsoConformance();
            root = new PdfPages(this);
            pdf = document;
            directContentUnder = new PdfContentByte(this);
            directContent = directContentUnder.Duplicate;
        }

        public static PdfWriter GetInstance(Document document, Stream os)
        {
            PdfDocument pdfDocument = new PdfDocument();
            document.AddDocListener(pdfDocument);
            PdfWriter pdfWriter = new PdfWriter(pdfDocument, os);
            pdfDocument.AddWriter(pdfWriter);
            return pdfWriter;
        }

        public static PdfWriter GetInstance(Document document, Stream os, IDocListener listener)
        {
            PdfDocument pdfDocument = new PdfDocument();
            pdfDocument.AddDocListener(listener);
            document.AddDocListener(pdfDocument);
            PdfWriter pdfWriter = new PdfWriter(pdfDocument, os);
            pdfDocument.AddWriter(pdfWriter);
            return pdfWriter;
        }

        public virtual float GetVerticalPosition(bool ensureNewLine)
        {
            return pdf.GetVerticalPosition(ensureNewLine);
        }

        internal void ResetContent()
        {
            directContent.Reset();
            directContentUnder.Reset();
        }

        internal void AddLocalDestinations(SortedDictionary<string, PdfDocument.Destination> desto)
        {
            foreach (string key in desto.Keys)
            {
                PdfDocument.Destination destination = desto[key];
                PdfDestination destination2 = destination.destination;
                if (destination.reference == null)
                {
                    destination.reference = PdfIndirectReference;
                }

                if (destination2 == null)
                {
                    AddToBody(new PdfString("invalid_" + key), destination.reference);
                }
                else
                {
                    AddToBody(destination2, destination.reference);
                }
            }
        }

        public virtual PdfIndirectObject AddToBody(PdfObject objecta)
        {
            PdfIndirectObject pdfIndirectObject = body.Add(objecta);
            CacheObject(pdfIndirectObject);
            return pdfIndirectObject;
        }

        public virtual PdfIndirectObject AddToBody(PdfObject objecta, bool inObjStm)
        {
            PdfIndirectObject pdfIndirectObject = body.Add(objecta, inObjStm);
            CacheObject(pdfIndirectObject);
            return pdfIndirectObject;
        }

        public virtual PdfIndirectObject AddToBody(PdfObject objecta, PdfIndirectReference refa)
        {
            PdfIndirectObject pdfIndirectObject = body.Add(objecta, refa);
            CacheObject(pdfIndirectObject);
            return pdfIndirectObject;
        }

        public virtual PdfIndirectObject AddToBody(PdfObject objecta, PdfIndirectReference refa, bool inObjStm)
        {
            PdfIndirectObject pdfIndirectObject = body.Add(objecta, refa, inObjStm);
            CacheObject(pdfIndirectObject);
            return pdfIndirectObject;
        }

        public virtual PdfIndirectObject AddToBody(PdfObject objecta, int refNumber)
        {
            PdfIndirectObject pdfIndirectObject = body.Add(objecta, refNumber);
            CacheObject(pdfIndirectObject);
            return pdfIndirectObject;
        }

        public virtual PdfIndirectObject AddToBody(PdfObject objecta, int refNumber, bool inObjStm)
        {
            PdfIndirectObject pdfIndirectObject = body.Add(objecta, refNumber, 0, inObjStm);
            CacheObject(pdfIndirectObject);
            return pdfIndirectObject;
        }

        protected internal virtual void CacheObject(PdfIndirectObject iobj)
        {
        }

        protected virtual PdfDictionary GetCatalog(PdfIndirectReference rootObj)
        {
            PdfDictionary catalog = pdf.GetCatalog(rootObj);
            BuildStructTreeRootForTagged(catalog);
            if (documentOCG.Count > 0)
            {
                FillOCProperties(erase: false);
                catalog.Put(PdfName.OCPROPERTIES, OCProperties);
            }

            return catalog;
        }

        protected virtual void BuildStructTreeRootForTagged(PdfDictionary catalog)
        {
            if (tagged)
            {
                StructureTreeRoot.BuildTree();
                catalog.Put(PdfName.STRUCTTREEROOT, structureTreeRoot.Reference);
                PdfDictionary pdfDictionary = new PdfDictionary();
                pdfDictionary.Put(PdfName.MARKED, PdfBoolean.PDFTRUE);
                if (userProperties)
                {
                    pdfDictionary.Put(PdfName.USERPROPERTIES, PdfBoolean.PDFTRUE);
                }

                catalog.Put(PdfName.MARKINFO, pdfDictionary);
            }
        }

        public virtual void AddPageDictEntry(PdfName key, PdfObject obj)
        {
            pageDictEntries.Put(key, obj);
        }

        public virtual void ResetPageDictEntries()
        {
            pageDictEntries = new PdfDictionary();
        }

        public virtual void SetLinearPageMode()
        {
            root.SetLinearMode(null);
        }

        public virtual int ReorderPages(int[] order)
        {
            return root.ReorderPages(order);
        }

        public virtual PdfIndirectReference GetPageReference(int page)
        {
            page--;
            if (page < 0)
            {
                throw new ArgumentOutOfRangeException(MessageLocalization.GetComposedMessage("the.page.number.must.be.gt.eq.1"));
            }

            PdfIndirectReference pdfIndirectReference;
            if (page < pageReferences.Count)
            {
                pdfIndirectReference = pageReferences[page];
                if (pdfIndirectReference == null)
                {
                    pdfIndirectReference = body.PdfIndirectReference;
                    pageReferences[page] = pdfIndirectReference;
                }
            }
            else
            {
                int num = page - pageReferences.Count;
                for (int i = 0; i < num; i++)
                {
                    pageReferences.Add(null);
                }

                pdfIndirectReference = body.PdfIndirectReference;
                pageReferences.Add(pdfIndirectReference);
            }

            return pdfIndirectReference;
        }

        public virtual void SetPageViewport(PdfArray vp)
        {
            AddPageDictEntry(PdfName.VP, vp);
        }

        internal virtual PdfIndirectReference Add(PdfPage page, PdfContents contents)
        {
            if (!open)
            {
                throw new PdfException(MessageLocalization.GetComposedMessage("the.document.is.not.open"));
            }

            PdfIndirectObject pdfIndirectObject = AddToBody(contents);
            page.Add(pdfIndirectObject.IndirectReference);
            if (group != null)
            {
                page.Put(PdfName.GROUP, group);
                group = null;
            }
            else if (rgbTransparencyBlending)
            {
                PdfDictionary pdfDictionary = new PdfDictionary();
                pdfDictionary.Put(PdfName.TYPE, PdfName.GROUP);
                pdfDictionary.Put(PdfName.S, PdfName.TRANSPARENCY);
                pdfDictionary.Put(PdfName.CS, PdfName.DEVICERGB);
                page.Put(PdfName.GROUP, pdfDictionary);
            }

            root.AddPage(page);
            currentPageNumber++;
            return null;
        }

        public override void Open()
        {
            base.Open();
            pdf_version.WriteHeader(os);
            body = new PdfBody(this);
            if (IsPdfX() && ((PdfXConformanceImp)pdfIsoConformance).IsPdfX32002())
            {
                PdfDictionary pdfDictionary = new PdfDictionary();
                pdfDictionary.Put(PdfName.GAMMA, new PdfArray(new float[3] { 2.2f, 2.2f, 2.2f }));
                pdfDictionary.Put(PdfName.MATRIX, new PdfArray(new float[9] { 0.4124f, 0.2126f, 0.0193f, 0.3576f, 0.7152f, 0.1192f, 0.1805f, 0.0722f, 0.9505f }));
                pdfDictionary.Put(PdfName.WHITEPOINT, new PdfArray(new float[3] { 0.9505f, 1f, 1.089f }));
                PdfArray pdfArray = new PdfArray(PdfName.CALRGB);
                pdfArray.Add(pdfDictionary);
                SetDefaultColorspace(PdfName.DEFAULTRGB, AddToBody(pdfArray).IndirectReference);
            }
        }

        public override void Close()
        {
            if (open)
            {
                if (currentPageNumber - 1 != pageReferences.Count)
                {
                    throw new Exception("The page " + pageReferences.Count + " was requested but the document has only " + (currentPageNumber - 1) + " pages.");
                }

                pdf.Close();
                AddSharedObjectsToBody();
                foreach (IPdfOCG key in documentOCG.Keys)
                {
                    AddToBody(key.PdfObject, key.Ref);
                }

                PdfIndirectReference rootObj = root.WritePageTree();
                PdfDictionary catalog = GetCatalog(rootObj);
                if (documentOCG.Count > 0)
                {
                    CheckPdfIsoConformance(this, 7, OCProperties);
                }

                if (xmpMetadata == null && xmpWriter != null)
                {
                    try
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        xmpWriter.Serialize(memoryStream);
                        xmpWriter.Close();
                        xmpMetadata = memoryStream.ToArray();
                    }
                    catch (IOException)
                    {
                        xmpWriter = null;
                    }
                    catch (XmpException)
                    {
                        xmpWriter = null;
                    }
                }

                if (xmpMetadata != null)
                {
                    PdfStream pdfStream = new PdfStream(xmpMetadata);
                    pdfStream.Put(PdfName.TYPE, PdfName.METADATA);
                    pdfStream.Put(PdfName.SUBTYPE, PdfName.XML);
                    if (crypto != null && !crypto.IsMetadataEncrypted())
                    {
                        PdfArray pdfArray = new PdfArray();
                        pdfArray.Add(PdfName.CRYPT);
                        pdfStream.Put(PdfName.FILTER, pdfArray);
                    }

                    catalog.Put(PdfName.METADATA, body.Add(pdfStream).IndirectReference);
                }

                if (IsPdfX())
                {
                    CompleteInfoDictionary(Info);
                    CompleteExtraCatalog(ExtraCatalog);
                }

                if (extraCatalog != null)
                {
                    catalog.MergeDifferent(extraCatalog);
                }

                WriteOutlines(catalog, namedAsNames: false);
                PdfIndirectObject pdfIndirectObject = AddToBody(catalog, inObjStm: false);
                PdfIndirectObject pdfIndirectObject2 = AddToBody(Info, inObjStm: false);
                PdfIndirectReference encryption = null;
                PdfObject pdfObject = null;
                body.FlushObjStm();
                bool flag = originalFileID != null;
                if (crypto != null)
                {
                    encryption = AddToBody(crypto.GetEncryptionDictionary(), inObjStm: false).IndirectReference;
                    pdfObject = crypto.GetFileID(flag);
                }
                else
                {
                    pdfObject = PdfEncryption.CreateInfoId(flag ? originalFileID : PdfEncryption.CreateDocumentId(), flag);
                }

                body.WriteCrossReferenceTable(os, pdfIndirectObject.IndirectReference, pdfIndirectObject2.IndirectReference, encryption, pdfObject, prevxref);
                if (fullCompression)
                {
                    WriteKeyInfo(os);
                    byte[] iSOBytes = DocWriter.GetISOBytes("startxref\n");
                    os.Write(iSOBytes, 0, iSOBytes.Length);
                    iSOBytes = DocWriter.GetISOBytes(body.Offset.ToString());
                    os.Write(iSOBytes, 0, iSOBytes.Length);
                    iSOBytes = DocWriter.GetISOBytes("\n%%EOF\n");
                    os.Write(iSOBytes, 0, iSOBytes.Length);
                }
                else
                {
                    new PdfTrailer(body.Size, body.Offset, pdfIndirectObject.IndirectReference, pdfIndirectObject2.IndirectReference, encryption, pdfObject, prevxref).ToPdf(this, os);
                }

                base.Close();
            }

            GetCounter().Written(os.Counter);
        }

        protected virtual void AddXFormsToBody()
        {
            foreach (object[] value in formXObjects.Values)
            {
                PdfTemplate pdfTemplate = (PdfTemplate)value[1];
                if ((pdfTemplate == null || !(pdfTemplate.IndirectReference is PRIndirectReference)) && pdfTemplate != null && pdfTemplate.Type == 1)
                {
                    AddToBody(pdfTemplate.GetFormXObject(compressionLevel), pdfTemplate.IndirectReference);
                }
            }
        }

        protected virtual void AddSharedObjectsToBody()
        {
            foreach (FontDetails value3 in documentFonts.Values)
            {
                value3.WriteFont(this);
            }

            AddXFormsToBody();
            foreach (PdfReaderInstance value4 in readerInstances.Values)
            {
                PdfReaderInstance pdfReaderInstance = (currentPdfReaderInstance = value4);
                currentPdfReaderInstance.WriteAllPages();
            }

            currentPdfReaderInstance = null;
            foreach (ColorDetails value5 in documentColors.Values)
            {
                AddToBody(value5.GetPdfObject(this), value5.IndirectReference);
            }

            foreach (PdfPatternPainter key3 in documentPatterns.Keys)
            {
                AddToBody(key3.GetPattern(compressionLevel), key3.IndirectReference);
            }

            foreach (PdfShadingPattern key4 in documentShadingPatterns.Keys)
            {
                key4.AddToBody();
            }

            foreach (PdfShading key5 in documentShadings.Keys)
            {
                key5.AddToBody();
            }

            foreach (KeyValuePair<PdfDictionary, PdfObject[]> item in documentExtGState)
            {
                PdfDictionary key = item.Key;
                PdfObject[] value = item.Value;
                AddToBody(key, (PdfIndirectReference)value[1]);
            }

            foreach (KeyValuePair<object, PdfObject[]> documentProperty in documentProperties)
            {
                object key2 = documentProperty.Key;
                PdfObject[] value2 = documentProperty.Value;
                if (key2 is PdfLayerMembership)
                {
                    PdfLayerMembership pdfLayerMembership = (PdfLayerMembership)key2;
                    AddToBody(pdfLayerMembership.PdfObject, pdfLayerMembership.Ref);
                }
                else if (key2 is PdfDictionary && !(key2 is PdfLayer))
                {
                    AddToBody((PdfDictionary)key2, (PdfIndirectReference)value2[1]);
                }
            }
        }

        protected internal virtual void WriteOutlines(PdfDictionary catalog, bool namedAsNames)
        {
            if (newBookmarks != null && newBookmarks.Count != 0)
            {
                PdfDictionary pdfDictionary = new PdfDictionary();
                PdfIndirectReference pdfIndirectReference = PdfIndirectReference;
                object[] array = SimpleBookmark.IterateOutlines(this, pdfIndirectReference, newBookmarks, namedAsNames);
                pdfDictionary.Put(PdfName.FIRST, (PdfIndirectReference)array[0]);
                pdfDictionary.Put(PdfName.LAST, (PdfIndirectReference)array[1]);
                pdfDictionary.Put(PdfName.COUNT, new PdfNumber((int)array[2]));
                AddToBody(pdfDictionary, pdfIndirectReference);
                catalog.Put(PdfName.OUTLINES, pdfIndirectReference);
            }
        }

        public virtual void SetAtLeastPdfVersion(char version)
        {
            pdf_version.SetAtLeastPdfVersion(version);
        }

        public virtual void SetPdfVersion(PdfName version)
        {
            pdf_version.SetPdfVersion(version);
        }

        public virtual void AddDeveloperExtension(PdfDeveloperExtension de)
        {
            pdf_version.AddDeveloperExtension(de);
        }

        internal PdfVersionImp GetPdfVersion()
        {
            return pdf_version;
        }

        public virtual void AddViewerPreference(PdfName key, PdfObject value)
        {
            pdf.AddViewerPreference(key, value);
        }

        public virtual void AddNamedDestinations(IDictionary<string, string> map, int page_offset)
        {
            foreach (KeyValuePair<string, string> item in map)
            {
                string value = item.Value;
                int num = int.Parse(value.Substring(0, value.IndexOf(" ")));
                PdfDestination dest = new PdfDestination(value.Substring(value.IndexOf(" ") + 1));
                AddNamedDestination(item.Key, num + page_offset, dest);
            }
        }

        public virtual void AddNamedDestination(string name, int page, PdfDestination dest)
        {
            dest.AddPage(GetPageReference(page));
            pdf.LocalDestination(name, dest);
        }

        public virtual void AddJavaScript(PdfAction js)
        {
            pdf.AddJavaScript(js);
        }

        public virtual void AddJavaScript(string code, bool unicode)
        {
            AddJavaScript(PdfAction.JavaScript(code, this, unicode));
        }

        public virtual void AddJavaScript(string code)
        {
            AddJavaScript(code, unicode: false);
        }

        public virtual void AddJavaScript(string name, PdfAction js)
        {
            pdf.AddJavaScript(name, js);
        }

        public virtual void AddJavaScript(string name, string code, bool unicode)
        {
            AddJavaScript(name, PdfAction.JavaScript(code, this, unicode));
        }

        public virtual void AddJavaScript(string name, string code)
        {
            AddJavaScript(name, code, unicode: false);
        }

        public virtual void AddFileAttachment(string description, byte[] fileStore, string file, string fileDisplay)
        {
            AddFileAttachment(description, PdfFileSpecification.FileEmbedded(this, file, fileDisplay, fileStore));
        }

        public virtual void AddFileAttachment(string description, PdfFileSpecification fs)
        {
            pdf.AddFileAttachment(description, fs);
        }

        public virtual void AddFileAttachment(PdfFileSpecification fs)
        {
            pdf.AddFileAttachment(null, fs);
        }

        public virtual void SetOpenAction(string name)
        {
            pdf.SetOpenAction(name);
        }

        public virtual void SetOpenAction(PdfAction action)
        {
            pdf.SetOpenAction(action);
        }

        public virtual void SetAdditionalAction(PdfName actionType, PdfAction action)
        {
            if (!actionType.Equals(DOCUMENT_CLOSE) && !actionType.Equals(WILL_SAVE) && !actionType.Equals(DID_SAVE) && !actionType.Equals(WILL_PRINT) && !actionType.Equals(DID_PRINT))
            {
                throw new PdfException(MessageLocalization.GetComposedMessage("invalid.additional.action.type.1", actionType.ToString()));
            }

            pdf.AddAdditionalAction(actionType, action);
        }

        public virtual void AddAnnotation(PdfAnnotation annot)
        {
            pdf.AddAnnotation(annot);
        }

        internal virtual void AddAnnotation(PdfAnnotation annot, int page)
        {
            AddAnnotation(annot);
        }

        public virtual void AddCalculationOrder(PdfFormField annot)
        {
            pdf.AddCalculationOrder(annot);
        }

        public virtual void SetLanguage(string language)
        {
            pdf.SetLanguage(language);
        }

        public virtual void CreateXmpMetadata()
        {
            try
            {
                xmpWriter = CreateXmpWriter(null, pdf.Info);
                if (IsTagged())
                {
                    xmpWriter.XmpMeta.SetPropertyInteger("http://www.aiim.org/pdfua/ns/id/", PdfProperties.PART, 1, new PropertyOptions(1073741824u));
                }

                xmpMetadata = null;
            }
            catch (IOException)
            {
            }
        }

        public virtual IPdfIsoConformance InitPdfIsoConformance()
        {
            return new PdfXConformanceImp(this);
        }

        public virtual bool IsPdfIso()
        {
            return pdfIsoConformance.IsPdfIso();
        }

        public virtual bool IsPdfX()
        {
            if (pdfIsoConformance is PdfXConformanceImp)
            {
                return ((IPdfXConformance)pdfIsoConformance).IsPdfX();
            }

            return false;
        }

        public virtual void SetOutputIntents(string outputConditionIdentifier, string outputCondition, string registryName, string info, ICC_Profile colorProfile)
        {
            CheckPdfIsoConformance(this, 19, colorProfile);
            PdfDictionary pdfDictionary = ExtraCatalog;
            pdfDictionary = new PdfDictionary(PdfName.OUTPUTINTENT);
            if (outputCondition != null)
            {
                pdfDictionary.Put(PdfName.OUTPUTCONDITION, new PdfString(outputCondition, "UnicodeBig"));
            }

            if (outputConditionIdentifier != null)
            {
                pdfDictionary.Put(PdfName.OUTPUTCONDITIONIDENTIFIER, new PdfString(outputConditionIdentifier, "UnicodeBig"));
            }

            if (registryName != null)
            {
                pdfDictionary.Put(PdfName.REGISTRYNAME, new PdfString(registryName, "UnicodeBig"));
            }

            if (info != null)
            {
                pdfDictionary.Put(PdfName.INFO, new PdfString(info, "UnicodeBig"));
            }

            if (colorProfile != null)
            {
                PdfStream objecta = new PdfICCBased(colorProfile, compressionLevel);
                pdfDictionary.Put(PdfName.DESTOUTPUTPROFILE, AddToBody(objecta).IndirectReference);
            }

            pdfDictionary.Put(PdfName.S, PdfName.GTS_PDFX);
            extraCatalog.Put(PdfName.OUTPUTINTENTS, new PdfArray(pdfDictionary));
            this.colorProfile = colorProfile;
        }

        public virtual void SetOutputIntents(string outputConditionIdentifier, string outputCondition, string registryName, string info, byte[] destOutputProfile)
        {
            ICC_Profile iCC_Profile = ((destOutputProfile == null) ? null : ICC_Profile.GetInstance(destOutputProfile));
            SetOutputIntents(outputConditionIdentifier, outputCondition, registryName, info, iCC_Profile);
        }

        public virtual bool SetOutputIntents(PdfReader reader, bool checkExistence)
        {
            PdfArray asArray = reader.Catalog.GetAsArray(PdfName.OUTPUTINTENTS);
            if (asArray == null)
            {
                return false;
            }

            if (asArray.Size == 0)
            {
                return false;
            }

            PdfDictionary asDict = asArray.GetAsDict(0);
            PdfObject pdfObject = PdfReader.GetPdfObject(asDict.Get(PdfName.S));
            if (pdfObject == null || !PdfName.GTS_PDFX.Equals(pdfObject))
            {
                return false;
            }

            if (checkExistence)
            {
                return true;
            }

            PRStream pRStream = (PRStream)PdfReader.GetPdfObject(asDict.Get(PdfName.DESTOUTPUTPROFILE));
            byte[] destOutputProfile = null;
            if (pRStream != null)
            {
                destOutputProfile = PdfReader.GetStreamBytes(pRStream);
            }

            SetOutputIntents(GetNameString(asDict, PdfName.OUTPUTCONDITIONIDENTIFIER), GetNameString(asDict, PdfName.OUTPUTCONDITION), GetNameString(asDict, PdfName.REGISTRYNAME), GetNameString(asDict, PdfName.INFO), destOutputProfile);
            return true;
        }

        private static string GetNameString(PdfDictionary dic, PdfName key)
        {
            PdfObject pdfObject = PdfReader.GetPdfObject(dic.Get(key));
            if (pdfObject == null || !pdfObject.IsString())
            {
                return null;
            }

            return ((PdfString)pdfObject).ToUnicodeString();
        }

        public virtual void SetEncryption(byte[] userPassword, byte[] ownerPassword, int permissions, int encryptionType)
        {
            if (pdf.IsOpen())
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("encryption.can.only.be.added.before.opening.the.document"));
            }

            crypto = new PdfEncryption();
            crypto.SetCryptoMode(encryptionType, 0);
            crypto.SetupAllKeys(userPassword, ownerPassword, permissions);
        }

        public virtual void SetEncryption(X509Certificate[] certs, int[] permissions, int encryptionType)
        {
            if (pdf.IsOpen())
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("encryption.can.only.be.added.before.opening.the.document"));
            }

            crypto = new PdfEncryption();
            if (certs != null)
            {
                for (int i = 0; i < certs.Length; i++)
                {
                    crypto.AddRecipient(certs[i], permissions[i]);
                }
            }

            crypto.SetCryptoMode(encryptionType, 0);
            crypto.GetEncryptionDictionary();
        }

        public virtual void SetEncryption(byte[] userPassword, byte[] ownerPassword, int permissions, bool strength128Bits)
        {
            SetEncryption(userPassword, ownerPassword, permissions, strength128Bits ? 1 : 0);
        }

        public virtual void SetEncryption(bool strength, string userPassword, string ownerPassword, int permissions)
        {
            SetEncryption(DocWriter.GetISOBytes(userPassword), DocWriter.GetISOBytes(ownerPassword), permissions, strength);
        }

        public virtual void SetEncryption(int encryptionType, string userPassword, string ownerPassword, int permissions)
        {
            SetEncryption(DocWriter.GetISOBytes(userPassword), DocWriter.GetISOBytes(ownerPassword), permissions, encryptionType);
        }

        public virtual void SetFullCompression()
        {
            if (open)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("you.can.t.set.the.full.compression.if.the.document.is.already.open"));
            }

            fullCompression = true;
            SetAtLeastPdfVersion('5');
        }

        internal FontDetails AddSimple(BaseFont bf)
        {
            if (!documentFonts.TryGetValue(bf, out var value))
            {
                CheckPdfIsoConformance(this, 4, bf);
                value = ((bf.FontType != 4) ? new FontDetails(new PdfName("F" + fontNumber++), body.PdfIndirectReference, bf) : new FontDetails(new PdfName("F" + fontNumber++), ((DocumentFont)bf).IndirectReference, bf));
                documentFonts[bf] = value;
            }

            return value;
        }

        internal void EliminateFontSubset(PdfDictionary fonts)
        {
            foreach (FontDetails value in documentFonts.Values)
            {
                if (fonts.Get(value.FontName) != null)
                {
                    value.Subset = false;
                }
            }
        }

        internal PdfName AddDirectTemplateSimple(PdfTemplate template, PdfName forcedName)
        {
            PdfIndirectReference indirectReference = template.IndirectReference;
            formXObjects.TryGetValue(indirectReference, out var value);
            PdfName pdfName = null;
            if (value == null)
            {
                if (forcedName == null)
                {
                    pdfName = new PdfName("Xf" + formXObjectsCounter);
                    formXObjectsCounter++;
                }
                else
                {
                    pdfName = forcedName;
                }

                if (template.Type == 2)
                {
                    PdfImportedPage pdfImportedPage = (PdfImportedPage)template;
                    PdfReader reader = pdfImportedPage.PdfReaderInstance.Reader;
                    if (!readerInstances.ContainsKey(reader))
                    {
                        readerInstances[reader] = pdfImportedPage.PdfReaderInstance;
                    }

                    template = null;
                }

                formXObjects[indirectReference] = new object[2] { pdfName, template };
            }
            else
            {
                pdfName = (PdfName)value[0];
            }

            return pdfName;
        }

        public virtual void ReleaseTemplate(PdfTemplate tp)
        {
            PdfIndirectReference indirectReference = tp.IndirectReference;
            formXObjects.TryGetValue(indirectReference, out var value);
            if (value != null && value[1] != null)
            {
                PdfTemplate pdfTemplate = (PdfTemplate)value[1];
                if (!(pdfTemplate.IndirectReference is PRIndirectReference) && pdfTemplate.Type == 1)
                {
                    AddToBody(pdfTemplate.GetFormXObject(compressionLevel), pdfTemplate.IndirectReference);
                    value[1] = null;
                }
            }
        }

        public virtual PdfImportedPage GetImportedPage(PdfReader reader, int pageNumber)
        {
            return GetPdfReaderInstance(reader).GetImportedPage(pageNumber);
        }

        protected virtual PdfReaderInstance GetPdfReaderInstance(PdfReader reader)
        {
            readerInstances.TryGetValue(reader, out var value);
            if (value == null)
            {
                value = reader.GetPdfReaderInstance(this);
                readerInstances[reader] = value;
            }

            return value;
        }

        public virtual void FreeReader(PdfReader reader)
        {
            readerInstances.TryGetValue(reader, out currentPdfReaderInstance);
            if (currentPdfReaderInstance != null)
            {
                currentPdfReaderInstance.WriteAllPages();
                currentPdfReaderInstance = null;
                readerInstances.Remove(reader);
            }
        }

        protected internal virtual int GetNewObjectNumber(PdfReader reader, int number, int generation)
        {
            if (currentPdfReaderInstance == null || currentPdfReaderInstance.Reader != reader)
            {
                currentPdfReaderInstance = GetPdfReaderInstance(reader);
            }

            return currentPdfReaderInstance.GetNewObjectNumber(number, generation);
        }

        internal virtual RandomAccessFileOrArray GetReaderFile(PdfReader reader)
        {
            return currentPdfReaderInstance.ReaderFile;
        }

        internal PdfName GetColorspaceName()
        {
            return new PdfName("CS" + colorNumber++);
        }

        internal virtual ColorDetails AddSimple(ICachedColorSpace spc)
        {
            documentColors.TryGetValue(spc, out var value);
            if (value == null)
            {
                value = new ColorDetails(GetColorspaceName(), body.PdfIndirectReference, spc);
                if (spc is IPdfSpecialColorSpace)
                {
                    ((IPdfSpecialColorSpace)spc).GetColorantDetails(this);
                }

                documentColors[spc] = value;
            }

            return value;
        }

        internal virtual PdfName AddSimplePattern(PdfPatternPainter painter)
        {
            documentPatterns.TryGetValue(painter, out var value);
            if (value == null)
            {
                value = new PdfName("P" + patternNumber);
                patternNumber++;
                documentPatterns[painter] = value;
            }

            return value;
        }

        internal void AddSimpleShadingPattern(PdfShadingPattern shading)
        {
            if (!documentShadingPatterns.ContainsKey(shading))
            {
                shading.Name = patternNumber;
                patternNumber++;
                documentShadingPatterns[shading] = null;
                AddSimpleShading(shading.Shading);
            }
        }

        internal void AddSimpleShading(PdfShading shading)
        {
            if (!documentShadings.ContainsKey(shading))
            {
                documentShadings[shading] = null;
                shading.Name = documentShadings.Count;
            }
        }

        internal PdfObject[] AddSimpleExtGState(PdfDictionary gstate)
        {
            if (!documentExtGState.ContainsKey(gstate))
            {
                documentExtGState[gstate] = new PdfObject[2]
                {
                    new PdfName("GS" + (documentExtGState.Count + 1)),
                    PdfIndirectReference
                };
            }

            return documentExtGState[gstate];
        }

        internal PdfObject[] AddSimpleProperty(object prop, PdfIndirectReference refi)
        {
            if (!documentProperties.ContainsKey(prop))
            {
                if (prop is IPdfOCG)
                {
                    CheckPdfIsoConformance(this, 7, prop);
                }

                documentProperties[prop] = new PdfObject[2]
                {
                    new PdfName("Pr" + (documentProperties.Count + 1)),
                    refi
                };
            }

            return documentProperties[prop];
        }

        internal bool PropertyExists(object prop)
        {
            return documentProperties.ContainsKey(prop);
        }

        public virtual void SetTagged()
        {
            SetTagged(1);
        }

        public virtual void SetTagged(int taggingMode)
        {
            if (open)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("tagging.must.be.set.before.opening.the.document"));
            }

            tagged = true;
            this.taggingMode = taggingMode;
        }

        public virtual bool NeedToBeMarkedInContent(IAccessibleElement element)
        {
            if (((uint)taggingMode & (true ? 1u : 0u)) != 0)
            {
                if (element.IsInline || PdfName.ARTIFACT.Equals(element.Role))
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        public virtual void CheckElementRole(IAccessibleElement element, IAccessibleElement parent)
        {
            if (parent != null && (parent.Role == null || PdfName.ARTIFACT.Equals(parent.Role)))
            {
                element.Role = null;
            }
            else if (((uint)taggingMode & (true ? 1u : 0u)) != 0 && element.IsInline && element.Role == null && (parent == null || !parent.IsInline))
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("inline.elements.with.role.null.are.not.allowed"));
            }
        }

        public virtual bool IsTagged()
        {
            return tagged;
        }

        internal virtual void FlushTaggedObjects()
        {
        }

        internal virtual void FlushAcroFields()
        {
        }

        public virtual void AddOCGRadioGroup(List<PdfLayer> group)
        {
            PdfArray pdfArray = new PdfArray();
            for (int i = 0; i < group.Count; i++)
            {
                PdfLayer pdfLayer = group[i];
                if (pdfLayer.Title == null)
                {
                    pdfArray.Add(pdfLayer.Ref);
                }
            }

            if (pdfArray.Size != 0)
            {
                OCGRadioGroup.Add(pdfArray);
            }
        }

        public virtual void LockLayer(PdfLayer layer)
        {
            OCGLocked.Add(layer.Ref);
        }

        private static void GetOCGOrder(PdfArray order, PdfLayer layer)
        {
            if (!layer.OnPanel)
            {
                return;
            }

            if (layer.Title == null)
            {
                order.Add(layer.Ref);
            }

            List<PdfLayer> children = layer.Children;
            if (children != null)
            {
                PdfArray pdfArray = new PdfArray();
                if (layer.Title != null)
                {
                    pdfArray.Add(new PdfString(layer.Title, "UnicodeBig"));
                }

                for (int i = 0; i < children.Count; i++)
                {
                    GetOCGOrder(pdfArray, children[i]);
                }

                if (pdfArray.Size > 0)
                {
                    order.Add(pdfArray);
                }
            }
        }

        private void AddASEvent(PdfName eventa, PdfName category)
        {
            PdfArray pdfArray = new PdfArray();
            foreach (PdfLayer key in documentOCG.Keys)
            {
                PdfDictionary asDict = key.GetAsDict(PdfName.USAGE);
                if (asDict != null && asDict.Get(category) != null)
                {
                    pdfArray.Add(key.Ref);
                }
            }

            if (pdfArray.Size != 0)
            {
                PdfDictionary asDict2 = vOCProperties.GetAsDict(PdfName.D);
                PdfArray pdfArray2 = asDict2.GetAsArray(PdfName.AS);
                if (pdfArray2 == null)
                {
                    pdfArray2 = new PdfArray();
                    asDict2.Put(PdfName.AS, pdfArray2);
                }

                PdfDictionary pdfDictionary = new PdfDictionary();
                pdfDictionary.Put(PdfName.EVENT, eventa);
                pdfDictionary.Put(PdfName.CATEGORY, new PdfArray(category));
                pdfDictionary.Put(PdfName.OCGS, pdfArray);
                pdfArray2.Add(pdfDictionary);
            }
        }

        protected virtual void FillOCProperties(bool erase)
        {
            if (vOCProperties == null)
            {
                vOCProperties = new PdfOCProperties();
            }

            if (erase)
            {
                vOCProperties.Remove(PdfName.OCGS);
                vOCProperties.Remove(PdfName.D);
            }

            if (vOCProperties.Get(PdfName.OCGS) == null)
            {
                PdfArray pdfArray = new PdfArray();
                foreach (PdfLayer key in documentOCG.Keys)
                {
                    pdfArray.Add(key.Ref);
                }

                vOCProperties.Put(PdfName.OCGS, pdfArray);
            }

            if (vOCProperties.Get(PdfName.D) != null)
            {
                return;
            }

            List<IPdfOCG> list = new List<IPdfOCG>(documentOCGorder);
            ListIterator<IPdfOCG> listIterator = new ListIterator<IPdfOCG>(list);
            while (listIterator.HasNext())
            {
                if (((PdfLayer)listIterator.Next()).Parent != null)
                {
                    listIterator.Remove();
                }
            }

            PdfArray pdfArray2 = new PdfArray();
            foreach (PdfLayer item in list)
            {
                GetOCGOrder(pdfArray2, item);
            }

            PdfDictionary pdfDictionary = new PdfDictionary();
            vOCProperties.Put(PdfName.D, pdfDictionary);
            pdfDictionary.Put(PdfName.ORDER, pdfArray2);
            if (list.Count > 0 && list[0] is PdfLayer)
            {
                PdfString asString = ((PdfLayer)list[0]).GetAsString(PdfName.NAME);
                if (asString != null)
                {
                    pdfDictionary.Put(PdfName.NAME, asString);
                }
            }

            PdfArray pdfArray3 = new PdfArray();
            foreach (PdfLayer key2 in documentOCG.Keys)
            {
                if (!key2.On)
                {
                    pdfArray3.Add(key2.Ref);
                }
            }

            if (pdfArray3.Size > 0)
            {
                pdfDictionary.Put(PdfName.OFF, pdfArray3);
            }

            if (OCGRadioGroup.Size > 0)
            {
                pdfDictionary.Put(PdfName.RBGROUPS, OCGRadioGroup);
            }

            if (OCGLocked.Size > 0)
            {
                pdfDictionary.Put(PdfName.LOCKED, OCGLocked);
            }

            AddASEvent(PdfName.VIEW, PdfName.ZOOM);
            AddASEvent(PdfName.VIEW, PdfName.VIEW);
            AddASEvent(PdfName.PRINT, PdfName.PRINT);
            AddASEvent(PdfName.EXPORT, PdfName.EXPORT);
            pdfDictionary.Put(PdfName.LISTMODE, PdfName.VISIBLEPAGES);
        }

        internal void RegisterLayer(IPdfOCG layer)
        {
            CheckPdfIsoConformance(this, 7, layer);
            if (layer is PdfLayer)
            {
                if (((PdfLayer)layer).Title == null)
                {
                    if (!documentOCG.ContainsKey(layer))
                    {
                        documentOCG[layer] = null;
                        documentOCGorder.Add(layer);
                    }
                }
                else
                {
                    documentOCGorder.Add(layer);
                }

                return;
            }

            throw new ArgumentException(MessageLocalization.GetComposedMessage("only.pdflayer.is.accepted"));
        }

        public virtual void SetBoxSize(string boxName, Rectangle size)
        {
            pdf.SetBoxSize(boxName, size);
        }

        public virtual Rectangle GetBoxSize(string boxName)
        {
            return pdf.GetBoxSize(boxName);
        }

        public virtual Rectangle GetBoxSize(string boxName, Rectangle intersectingRectangle)
        {
            Rectangle boxSize = pdf.GetBoxSize(boxName);
            if (boxSize == null || intersectingRectangle == null)
            {
                return null;
            }

            RectangleJ rectangleJ = new RectangleJ(boxSize);
            RectangleJ r = new RectangleJ(intersectingRectangle);
            RectangleJ rectangleJ2 = rectangleJ.Intersection(r);
            if (rectangleJ2.IsEmpty())
            {
                return null;
            }

            Rectangle rectangle = new Rectangle(rectangleJ2.X, rectangleJ2.Y, rectangleJ2.X + rectangleJ2.Width, rectangleJ2.Y + rectangleJ2.Height);
            rectangle.Normalize();
            return rectangle;
        }

        public virtual void SetPageAction(PdfName actionType, PdfAction action)
        {
            if (!actionType.Equals(PAGE_OPEN) && !actionType.Equals(PAGE_CLOSE))
            {
                throw new PdfException(MessageLocalization.GetComposedMessage("invalid.page.additional.action.type.1", actionType.ToString()));
            }

            pdf.SetPageAction(actionType, action);
        }

        public virtual void SetDefaultColorspace(PdfName key, PdfObject cs)
        {
            if (cs == null || cs.IsNull())
            {
                defaultColorspace.Remove(key);
            }

            defaultColorspace.Put(key, cs);
        }

        internal ColorDetails AddSimplePatternColorspace(BaseColor color)
        {
            switch (ExtendedColor.GetType(color))
            {
                case 4:
                case 5:
                    throw new Exception(MessageLocalization.GetComposedMessage("an.uncolored.tile.pattern.can.not.have.another.pattern.or.shading.as.color"));
                case 0:
                    if (patternColorspaceRGB == null)
                    {
                        patternColorspaceRGB = new ColorDetails(GetColorspaceName(), body.PdfIndirectReference, null);
                        PdfArray pdfArray4 = new PdfArray(PdfName.PATTERN);
                        pdfArray4.Add(PdfName.DEVICERGB);
                        AddToBody(pdfArray4, patternColorspaceRGB.IndirectReference);
                    }

                    return patternColorspaceRGB;
                case 2:
                    if (patternColorspaceCMYK == null)
                    {
                        patternColorspaceCMYK = new ColorDetails(GetColorspaceName(), body.PdfIndirectReference, null);
                        PdfArray pdfArray2 = new PdfArray(PdfName.PATTERN);
                        pdfArray2.Add(PdfName.DEVICECMYK);
                        AddToBody(pdfArray2, patternColorspaceCMYK.IndirectReference);
                    }

                    return patternColorspaceCMYK;
                case 1:
                    if (patternColorspaceGRAY == null)
                    {
                        patternColorspaceGRAY = new ColorDetails(GetColorspaceName(), body.PdfIndirectReference, null);
                        PdfArray pdfArray3 = new PdfArray(PdfName.PATTERN);
                        pdfArray3.Add(PdfName.DEVICEGRAY);
                        AddToBody(pdfArray3, patternColorspaceGRAY.IndirectReference);
                    }

                    return patternColorspaceGRAY;
                case 3:
                    {
                        ColorDetails colorDetails = AddSimple(((SpotColor)color).PdfSpotColor);
                        documentSpotPatterns.TryGetValue(colorDetails, out var value);
                        if (value == null)
                        {
                            value = new ColorDetails(GetColorspaceName(), body.PdfIndirectReference, null);
                            PdfArray pdfArray = new PdfArray(PdfName.PATTERN);
                            pdfArray.Add(colorDetails.IndirectReference);
                            AddToBody(pdfArray, value.IndirectReference);
                            documentSpotPatterns[colorDetails] = value;
                        }

                        return value;
                    }
                default:
                    throw new Exception(MessageLocalization.GetComposedMessage("invalid.color.type"));
            }
        }

        public virtual void ClearTextWrap()
        {
            pdf.ClearTextWrap();
        }

        public virtual PdfName AddDirectImageSimple(Image image)
        {
            return AddDirectImageSimple(image, null);
        }

        public virtual PdfName AddDirectImageSimple(Image image, PdfIndirectReference fixedRef)
        {
            PdfName pdfName;
            if (images.ContainsKey(image.MySerialId))
            {
                pdfName = images[image.MySerialId];
            }
            else
            {
                if (image.IsImgTemplate())
                {
                    pdfName = new PdfName("img" + images.Count);
                    if (image is ImgWMF)
                    {
                        ((ImgWMF)image).ReadWMF(PdfTemplate.CreateTemplate(this, 0f, 0f));
                    }
                }
                else
                {
                    PdfIndirectReference directReference = image.DirectReference;
                    if (directReference != null)
                    {
                        PdfName pdfName2 = new PdfName("img" + images.Count);
                        images[image.MySerialId] = pdfName2;
                        imageDictionary.Put(pdfName2, directReference);
                        return pdfName2;
                    }

                    Image imageMask = image.ImageMask;
                    PdfIndirectReference maskRef = null;
                    if (imageMask != null)
                    {
                        PdfName name = images[imageMask.MySerialId];
                        maskRef = GetImageReference(name);
                    }

                    PdfImage pdfImage = new PdfImage(image, "img" + images.Count, maskRef);
                    if (image is ImgJBIG2)
                    {
                        byte[] globalBytes = ((ImgJBIG2)image).GlobalBytes;
                        if (globalBytes != null)
                        {
                            PdfDictionary pdfDictionary = new PdfDictionary();
                            pdfDictionary.Put(PdfName.JBIG2GLOBALS, GetReferenceJBIG2Globals(globalBytes));
                            pdfImage.Put(PdfName.DECODEPARMS, pdfDictionary);
                        }
                    }

                    if (image.HasICCProfile())
                    {
                        PdfICCBased icc = new PdfICCBased(image.TagICC, image.CompressionLevel);
                        PdfIndirectReference obj = Add(icc);
                        PdfArray pdfArray = new PdfArray();
                        pdfArray.Add(PdfName.ICCBASED);
                        pdfArray.Add(obj);
                        PdfArray asArray = pdfImage.GetAsArray(PdfName.COLORSPACE);
                        if (asArray != null)
                        {
                            if (asArray.Size > 1 && PdfName.INDEXED.Equals(asArray.GetPdfObject(0)))
                            {
                                asArray.Set(1, pdfArray);
                            }
                            else
                            {
                                pdfImage.Put(PdfName.COLORSPACE, pdfArray);
                            }
                        }
                        else
                        {
                            pdfImage.Put(PdfName.COLORSPACE, pdfArray);
                        }
                    }

                    Add(pdfImage, fixedRef);
                    pdfName = pdfImage.Name;
                }

                images[image.MySerialId] = pdfName;
            }

            return pdfName;
        }

        internal virtual PdfIndirectReference Add(PdfImage pdfImage, PdfIndirectReference fixedRef)
        {
            if (!imageDictionary.Contains(pdfImage.Name))
            {
                CheckPdfIsoConformance(this, 5, pdfImage);
                if (fixedRef is PRIndirectReference)
                {
                    PRIndirectReference pRIndirectReference = (PRIndirectReference)fixedRef;
                    fixedRef = new PdfIndirectReference(0, GetNewObjectNumber(pRIndirectReference.Reader, pRIndirectReference.Number, pRIndirectReference.Generation));
                }

                if (fixedRef == null)
                {
                    fixedRef = AddToBody(pdfImage).IndirectReference;
                }
                else
                {
                    AddToBody(pdfImage, fixedRef);
                }

                imageDictionary.Put(pdfImage.Name, fixedRef);
                return fixedRef;
            }

            return (PdfIndirectReference)imageDictionary.Get(pdfImage.Name);
        }

        internal virtual PdfIndirectReference GetImageReference(PdfName name)
        {
            return (PdfIndirectReference)imageDictionary.Get(name);
        }

        protected virtual PdfIndirectReference Add(PdfICCBased icc)
        {
            return AddToBody(icc).IndirectReference;
        }

        protected internal virtual PdfIndirectReference GetReferenceJBIG2Globals(byte[] content)
        {
            if (content == null)
            {
                return null;
            }

            foreach (PdfStream key in JBIG2Globals.Keys)
            {
                if (Arrays.AreEqual(content, key.GetBytes()))
                {
                    return JBIG2Globals[key];
                }
            }

            PdfStream pdfStream = new PdfStream(content);
            PdfIndirectObject pdfIndirectObject;
            try
            {
                pdfIndirectObject = AddToBody(pdfStream);
            }
            catch (IOException)
            {
                return null;
            }

            JBIG2Globals[pdfStream] = pdfIndirectObject.IndirectReference;
            return pdfIndirectObject.IndirectReference;
        }

        protected static void WriteKeyInfo(Stream os)
        {
            Version instance = Version.GetInstance();
            string arg = instance.Key ?? "iText";
            byte[] iSOBytes = DocWriter.GetISOBytes($"%{arg}-{instance.Release}\n");
            os.Write(iSOBytes, 0, iSOBytes.Length);
        }

        protected internal virtual TtfUnicodeWriter GetTtfUnicodeWriter()
        {
            if (ttfUnicodeWriter == null)
            {
                ttfUnicodeWriter = new TtfUnicodeWriter(this);
            }

            return ttfUnicodeWriter;
        }

        protected internal virtual XmpWriter CreateXmpWriter(MemoryStream baos, PdfDictionary info)
        {
            return new XmpWriter(baos, info);
        }

        protected internal virtual XmpWriter CreateXmpWriter(MemoryStream baos, IDictionary<string, string> info)
        {
            return new XmpWriter(baos, info);
        }

        public virtual PdfAnnotation CreateAnnotation(Rectangle rect, PdfName subtype)
        {
            PdfAnnotation pdfAnnotation = new PdfAnnotation(this, rect);
            if (subtype != null)
            {
                pdfAnnotation.Put(PdfName.SUBTYPE, subtype);
            }

            return pdfAnnotation;
        }

        public virtual PdfAnnotation CreateAnnotation(float llx, float lly, float urx, float ury, PdfString title, PdfString content, PdfName subtype)
        {
            PdfAnnotation pdfAnnotation = new PdfAnnotation(this, llx, lly, urx, ury, title, content);
            if (subtype != null)
            {
                pdfAnnotation.Put(PdfName.SUBTYPE, subtype);
            }

            return pdfAnnotation;
        }

        public virtual PdfAnnotation CreateAnnotation(float llx, float lly, float urx, float ury, PdfAction action, PdfName subtype)
        {
            PdfAnnotation pdfAnnotation = new PdfAnnotation(this, llx, lly, urx, ury, action);
            if (subtype != null)
            {
                pdfAnnotation.Put(PdfName.SUBTYPE, subtype);
            }

            return pdfAnnotation;
        }

        public static void CheckPdfIsoConformance(PdfWriter writer, int key, object obj1)
        {
            writer?.CheckPdfIsoConformance(key, obj1);
        }

        public virtual void CheckPdfIsoConformance(int key, object obj1)
        {
            pdfIsoConformance.CheckPdfIsoConformance(key, obj1);
        }

        private void CompleteInfoDictionary(PdfDictionary info)
        {
            if (!IsPdfX())
            {
                return;
            }

            if (info.Get(PdfName.GTS_PDFXVERSION) == null)
            {
                if (((PdfXConformanceImp)pdfIsoConformance).IsPdfX1A2001())
                {
                    info.Put(PdfName.GTS_PDFXVERSION, new PdfString("PDF/X-1:2001"));
                    info.Put(new PdfName("GTS_PDFXConformance"), new PdfString("PDF/X-1a:2001"));
                }
                else if (((PdfXConformanceImp)pdfIsoConformance).IsPdfX32002())
                {
                    info.Put(PdfName.GTS_PDFXVERSION, new PdfString("PDF/X-3:2002"));
                }
            }

            if (info.Get(PdfName.TITLE) == null)
            {
                info.Put(PdfName.TITLE, new PdfString("Pdf document"));
            }

            if (info.Get(PdfName.CREATOR) == null)
            {
                info.Put(PdfName.CREATOR, new PdfString("Unknown"));
            }

            if (info.Get(PdfName.TRAPPED) == null)
            {
                info.Put(PdfName.TRAPPED, new PdfName("False"));
            }
        }

        private void CompleteExtraCatalog(PdfDictionary extraCatalog)
        {
            if (IsPdfX() && extraCatalog.Get(PdfName.OUTPUTINTENTS) == null)
            {
                PdfDictionary pdfDictionary = new PdfDictionary(PdfName.OUTPUTINTENT);
                pdfDictionary.Put(PdfName.OUTPUTCONDITION, new PdfString("SWOP CGATS TR 001-1995"));
                pdfDictionary.Put(PdfName.OUTPUTCONDITIONIDENTIFIER, new PdfString("CGATS TR 001"));
                pdfDictionary.Put(PdfName.REGISTRYNAME, new PdfString("http://www.color.org"));
                pdfDictionary.Put(PdfName.INFO, new PdfString(""));
                pdfDictionary.Put(PdfName.S, PdfName.GTS_PDFX);
                extraCatalog.Put(PdfName.OUTPUTINTENTS, new PdfArray(pdfDictionary));
            }
        }

        public virtual List<PdfName> GetStandardStructElems()
        {
            if (pdf_version.Version < '7')
            {
                return standardStructElems_1_4;
            }

            return standardStructElems_1_7;
        }
    }
}
