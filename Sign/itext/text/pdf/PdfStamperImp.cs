using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.text.exceptions;
using Sign.itext.text.log;
using Sign.itext.text.pdf.collection;
using Sign.itext.text.pdf.intern;
using Sign.itext.xml.xmp;
using Sign.itext.xml.xmp.options;
using Sign.SystemItext.util;
using Sign.SystemItext.util.collections;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class PdfStamperImp : PdfWriter
    {
        internal class PageStamp
        {
            internal PdfDictionary pageN;

            internal StampContent under;

            internal StampContent over;

            internal PageResources pageResources;

            internal int replacePoint;

            internal PageStamp(PdfStamperImp stamper, PdfReader reader, PdfDictionary pageN)
            {
                this.pageN = pageN;
                pageResources = new PageResources();
                PdfDictionary asDict = pageN.GetAsDict(PdfName.RESOURCES);
                pageResources.SetOriginalResources(asDict, stamper.namePtr);
            }
        }

        internal Dictionary<PdfReader, IntHashtable> readers2intrefs = new Dictionary<PdfReader, IntHashtable>();

        internal Dictionary<PdfReader, RandomAccessFileOrArray> readers2file = new Dictionary<PdfReader, RandomAccessFileOrArray>();

        protected internal RandomAccessFileOrArray file;

        protected internal PdfReader reader;

        internal IntHashtable myXref = new IntHashtable();

        internal Dictionary<PdfDictionary, PageStamp> pagesToContent = new Dictionary<PdfDictionary, PageStamp>();

        protected internal bool closed;

        private bool rotateContents = true;

        protected AcroFields acroFields;

        protected bool flat;

        protected bool flatFreeText;

        protected bool flatannotations;

        protected int[] namePtr = new int[1];

        protected HashSet2<string> partialFlattening = new HashSet2<string>();

        protected bool useVp;

        protected PdfViewerPreferencesImp viewerPreferences = new PdfViewerPreferencesImp();

        protected HashSet2<PdfTemplate> fieldTemplates = new HashSet2<PdfTemplate>();

        protected bool fieldsAdded;

        protected int sigFlags;

        protected internal bool append;

        protected IntHashtable marked;

        protected int initialXrefSize;

        protected PdfAction openAction;

        protected new ICounter COUNTER = CounterFactory.GetCounter(typeof(PdfStamper));

        internal bool RotateContents
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

        internal bool ContentWritten => body.Size > 1;

        internal bool FormFlattening
        {
            set
            {
                flat = value;
            }
        }

        internal bool FreeTextFlattening
        {
            set
            {
                flatFreeText = value;
            }
        }

        public virtual bool FlatAnnotations
        {
            set
            {
                flatannotations = value;
            }
        }

        public override int ViewerPreferences
        {
            set
            {
                useVp = true;
                viewerPreferences.ViewerPreferences = value;
            }
        }

        public override int SigFlags
        {
            set
            {
                sigFlags |= value;
            }
        }

        public override int Duration
        {
            set
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("use.setpageaction.pdfname.actiontype.pdfaction.action.int.page"));
            }
        }

        public override PdfTransition Transition
        {
            set
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("use.setpageaction.pdfname.actiontype.pdfaction.action.int.page"));
            }
        }

        public override Image Thumbnail
        {
            set
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("use.pdfstamper.setthumbnail"));
            }
        }

        public override PdfContentByte DirectContent
        {
            get
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("use.pdfstamper.getundercontent.or.pdfstamper.getovercontent"));
            }
        }

        public override PdfContentByte DirectContentUnder
        {
            get
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("use.pdfstamper.getundercontent.or.pdfstamper.getovercontent"));
            }
        }

        protected override ICounter GetCounter()
        {
            return COUNTER;
        }

        protected internal PdfStamperImp(PdfReader reader, Stream os, char pdfVersion, bool append)
            : base(new PdfDocument(), os)
        {
            if (!reader.IsOpenedWithFullPermissions)
            {
                throw new BadPasswordException(MessageLocalization.GetComposedMessage("pdfreader.not.opened.with.owner.password"));
            }

            if (reader.Tampered)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("the.original.document.was.reused.read.it.again.from.file"));
            }

            reader.Tampered = true;
            this.reader = reader;
            file = reader.SafeFile;
            this.append = append;
            if (reader.IsEncrypted() && (append || PdfReader.unethicalreading))
            {
                crypto = new PdfEncryption(reader.Decrypt);
            }

            if (append)
            {
                if (reader.IsRebuilt())
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("append.mode.requires.a.document.without.errors.even.if.recovery.was.possible"));
                }

                pdf_version.SetAppendmode(appendmode: true);
                byte[] array = new byte[8192];
                int count;
                while ((count = file.Read(array)) > 0)
                {
                    base.os.Write(array, 0, count);
                }

                prevxref = reader.LastXref;
                reader.Appendable = true;
            }
            else if (pdfVersion == '\0')
            {
                base.PdfVersion = reader.PdfVersion;
            }
            else
            {
                base.PdfVersion = pdfVersion;
            }

            if (reader.IsTagged())
            {
                SetTagged();
            }

            Open();
            pdf.AddWriter(this);
            if (append)
            {
                body.Refnum = reader.XrefSize;
                marked = new IntHashtable();
                if (reader.IsNewXrefType())
                {
                    fullCompression = true;
                }

                if (reader.IsHybridXref())
                {
                    fullCompression = false;
                }
            }

            initialXrefSize = reader.XrefSize;
            ReadColorProfile();
        }

        protected virtual void ReadColorProfile()
        {
            PdfObject asArray = reader.Catalog.GetAsArray(PdfName.OUTPUTINTENTS);
            if (asArray == null || ((PdfArray)asArray).Size <= 0)
            {
                return;
            }

            PdfStream pdfStream = null;
            for (int i = 0; i < ((PdfArray)asArray).Size; i++)
            {
                PdfDictionary asDict = ((PdfArray)asArray).GetAsDict(i);
                if (asDict != null)
                {
                    pdfStream = asDict.GetAsStream(PdfName.DESTOUTPUTPROFILE);
                    if (pdfStream != null)
                    {
                        break;
                    }
                }
            }

            if (pdfStream is PRStream)
            {
                colorProfile = ICC_Profile.GetInstance(PdfReader.GetStreamBytes((PRStream)pdfStream));
            }
        }

        protected virtual void SetViewerPreferences()
        {
            reader.SetViewerPreferences(viewerPreferences);
            MarkUsed(reader.Trailer.Get(PdfName.ROOT));
        }

        protected internal virtual void Close(IDictionary<string, string> moreInfo)
        {
            if (closed)
            {
                return;
            }

            if (useVp)
            {
                SetViewerPreferences();
            }

            if (flat)
            {
                FlatFields();
            }

            if (flatFreeText)
            {
                FlatFreeTextFields();
            }

            if (flatannotations)
            {
                FlattenAnnotations();
            }

            AddFieldResources();
            PdfDictionary catalog = reader.Catalog;
            GetPdfVersion().AddToCatalog(catalog);
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObject(catalog.Get(PdfName.ACROFORM), reader.Catalog);
            if (acroFields != null && acroFields.Xfa.Changed)
            {
                MarkUsed(pdfDictionary);
                if (!flat)
                {
                    acroFields.Xfa.SetXfa(this);
                }
            }

            if (sigFlags != 0 && pdfDictionary != null)
            {
                pdfDictionary.Put(PdfName.SIGFLAGS, new PdfNumber(sigFlags));
                MarkUsed(pdfDictionary);
                MarkUsed(catalog);
            }

            closed = true;
            AddSharedObjectsToBody();
            SetOutlines();
            SetJavaScript();
            AddFileAttachments();
            if (extraCatalog != null)
            {
                catalog.MergeDifferent(extraCatalog);
            }

            if (openAction != null)
            {
                catalog.Put(PdfName.OPENACTION, openAction);
            }

            if (pdf.pageLabels != null)
            {
                catalog.Put(PdfName.PAGELABELS, pdf.pageLabels.GetDictionary(this));
            }

            if (documentOCG.Count > 0)
            {
                FillOCProperties(erase: false);
                PdfDictionary asDict = catalog.GetAsDict(PdfName.OCPROPERTIES);
                if (asDict == null)
                {
                    reader.Catalog.Put(PdfName.OCPROPERTIES, OCProperties);
                }
                else
                {
                    asDict.Put(PdfName.OCGS, OCProperties.Get(PdfName.OCGS));
                    PdfDictionary pdfDictionary2 = asDict.GetAsDict(PdfName.D);
                    if (pdfDictionary2 == null)
                    {
                        pdfDictionary2 = new PdfDictionary();
                        asDict.Put(PdfName.D, pdfDictionary2);
                    }

                    pdfDictionary2.Put(PdfName.ORDER, OCProperties.GetAsDict(PdfName.D).Get(PdfName.ORDER));
                    pdfDictionary2.Put(PdfName.RBGROUPS, OCProperties.GetAsDict(PdfName.D).Get(PdfName.RBGROUPS));
                    pdfDictionary2.Put(PdfName.OFF, OCProperties.GetAsDict(PdfName.D).Get(PdfName.OFF));
                    pdfDictionary2.Put(PdfName.AS, OCProperties.GetAsDict(PdfName.D).Get(PdfName.AS));
                }

                PdfWriter.CheckPdfIsoConformance(this, 7, OCProperties);
            }

            int skipInfo = -1;
            PdfIndirectReference asIndirectObject = reader.Trailer.GetAsIndirectObject(PdfName.INFO);
            if (asIndirectObject != null)
            {
                skipInfo = asIndirectObject.Number;
            }

            PdfDictionary asDict2 = reader.Trailer.GetAsDict(PdfName.INFO);
            string text = null;
            if (asDict2 != null && asDict2.Get(PdfName.PRODUCER) != null)
            {
                text = asDict2.GetAsString(PdfName.PRODUCER).ToUnicodeString();
            }

            Version instance = Version.GetInstance();
            if (text == null || instance.GetVersion.IndexOf(instance.Product) == -1)
            {
                text = instance.GetVersion;
            }
            else
            {
                int num = text.IndexOf("; modified using");
                StringBuilder stringBuilder = ((num != -1) ? new StringBuilder(text.Substring(0, num)) : new StringBuilder(text));
                stringBuilder.Append("; modified using ");
                stringBuilder.Append(instance.GetVersion);
                text = stringBuilder.ToString();
            }

            PdfIndirectReference pdfIndirectReference = null;
            PdfDictionary pdfDictionary3 = new PdfDictionary();
            if (asDict2 != null)
            {
                foreach (PdfName key2 in asDict2.Keys)
                {
                    PdfObject pdfObject = PdfReader.GetPdfObject(asDict2.Get(key2));
                    pdfDictionary3.Put(key2, pdfObject);
                }
            }

            if (moreInfo != null)
            {
                foreach (KeyValuePair<string, string> item in moreInfo)
                {
                    PdfName key = new PdfName(item.Key);
                    string value = item.Value;
                    if (value == null)
                    {
                        pdfDictionary3.Remove(key);
                    }
                    else
                    {
                        pdfDictionary3.Put(key, new PdfString(value, "UnicodeBig"));
                    }
                }
            }

            PdfDate pdfDate = new PdfDate();
            pdfDictionary3.Put(PdfName.MODDATE, pdfDate);
            pdfDictionary3.Put(PdfName.PRODUCER, new PdfString(text, "UnicodeBig"));
            pdfIndirectReference = ((!append) ? AddToBody(pdfDictionary3, inObjStm: false).IndirectReference : ((asIndirectObject != null) ? AddToBody(pdfDictionary3, asIndirectObject.Number, inObjStm: false).IndirectReference : AddToBody(pdfDictionary3, inObjStm: false).IndirectReference));
            byte[] array = null;
            PdfObject pdfObject2 = PdfReader.GetPdfObject(catalog.Get(PdfName.METADATA));
            if (pdfObject2 != null && pdfObject2.IsStream())
            {
                array = PdfReader.GetStreamBytesRaw((PRStream)pdfObject2);
                PdfReader.KillIndirect(catalog.Get(PdfName.METADATA));
            }

            PdfStream pdfStream = null;
            if (xmpMetadata != null)
            {
                array = xmpMetadata;
            }
            else if (xmpWriter != null)
            {
                try
                {
                    MemoryStream memoryStream = new MemoryStream();
                    PdfProperties.SetProducer(xmpWriter.XmpMeta, text);
                    XmpBasicProperties.SetModDate(xmpWriter.XmpMeta, pdfDate.GetW3CDate());
                    XmpBasicProperties.SetMetaDataDate(xmpWriter.XmpMeta, pdfDate.GetW3CDate());
                    xmpWriter.Serialize(memoryStream);
                    xmpWriter.Close();
                    pdfStream = new PdfStream(memoryStream.ToArray());
                }
                catch (XmpException)
                {
                    xmpWriter = null;
                }
            }

            if (pdfStream == null && array != null)
            {
                try
                {
                    MemoryStream memoryStream2 = new MemoryStream();
                    if (moreInfo == null || xmpMetadata != null)
                    {
                        IXmpMeta xmpMeta = XmpMetaFactory.ParseFromBuffer(array);
                        PdfProperties.SetProducer(xmpMeta, text);
                        XmpBasicProperties.SetModDate(xmpMeta, pdfDate.GetW3CDate());
                        XmpBasicProperties.SetMetaDataDate(xmpMeta, pdfDate.GetW3CDate());
                        XmpMetaFactory.Serialize(xmpMeta, memoryStream2, new SerializeOptions
                        {
                            Padding = 2000
                        });
                    }
                    else
                    {
                        CreateXmpWriter(memoryStream2, pdfDictionary3).Close();
                    }

                    pdfStream = new PdfStream(memoryStream2.ToArray());
                }
                catch (XmpException)
                {
                    pdfStream = new PdfStream(array);
                }
                catch (IOException)
                {
                    pdfStream = new PdfStream(array);
                }
            }

            if (pdfStream != null)
            {
                pdfStream.Put(PdfName.TYPE, PdfName.METADATA);
                pdfStream.Put(PdfName.SUBTYPE, PdfName.XML);
                if (crypto != null && !crypto.IsMetadataEncrypted())
                {
                    PdfArray pdfArray = new PdfArray();
                    pdfArray.Add(PdfName.CRYPT);
                    pdfStream.Put(PdfName.FILTER, pdfArray);
                }

                if (append && pdfObject2 != null)
                {
                    body.Add(pdfStream, pdfObject2.IndRef);
                }
                else
                {
                    catalog.Put(PdfName.METADATA, body.Add(pdfStream).IndirectReference);
                    MarkUsed(catalog);
                }
            }

            Close(pdfIndirectReference, skipInfo);
        }

        protected virtual void Close(PdfIndirectReference info, int skipInfo)
        {
            AlterContents();
            int number = ((PRIndirectReference)reader.trailer.Get(PdfName.ROOT)).Number;
            if (append)
            {
                int[] keys = marked.GetKeys();
                foreach (int num in keys)
                {
                    PdfObject pdfObjectRelease = reader.GetPdfObjectRelease(num);
                    if (pdfObjectRelease != null && skipInfo != num && num < initialXrefSize)
                    {
                        AddToBody(pdfObjectRelease, pdfObjectRelease.IndRef, num != number);
                    }
                }

                for (int j = initialXrefSize; j < reader.XrefSize; j++)
                {
                    PdfObject pdfObject = reader.GetPdfObject(j);
                    if (pdfObject != null)
                    {
                        AddToBody(pdfObject, GetNewObjectNumber(reader, j, 0));
                    }
                }
            }
            else
            {
                for (int k = 1; k < reader.XrefSize; k++)
                {
                    PdfObject pdfObjectRelease2 = reader.GetPdfObjectRelease(k);
                    if (pdfObjectRelease2 != null && skipInfo != k)
                    {
                        AddToBody(pdfObjectRelease2, GetNewObjectNumber(reader, k, 0), k != number);
                    }
                }
            }

            PdfIndirectReference encryption = null;
            PdfObject pdfObject2 = null;
            if (crypto != null)
            {
                encryption = ((!append) ? AddToBody(crypto.GetEncryptionDictionary(), inObjStm: false).IndirectReference : reader.GetCryptoRef());
                pdfObject2 = crypto.GetFileID(modified: true);
            }
            else
            {
                PdfArray asArray = reader.trailer.GetAsArray(PdfName.ID);
                pdfObject2 = ((asArray == null || asArray.GetAsString(0) == null) ? PdfEncryption.CreateInfoId(PdfEncryption.CreateDocumentId(), modified: true) : PdfEncryption.CreateInfoId(asArray.GetAsString(0).GetBytes(), modified: true));
            }

            PRIndirectReference pRIndirectReference = (PRIndirectReference)reader.trailer.Get(PdfName.ROOT);
            PdfIndirectReference pdfIndirectReference = new PdfIndirectReference(0, GetNewObjectNumber(reader, pRIndirectReference.Number, 0));
            body.WriteCrossReferenceTable(os, pdfIndirectReference, info, encryption, pdfObject2, prevxref);
            if (fullCompression)
            {
                PdfWriter.WriteKeyInfo(os);
                byte[] iSOBytes = DocWriter.GetISOBytes("startxref\n");
                os.Write(iSOBytes, 0, iSOBytes.Length);
                iSOBytes = DocWriter.GetISOBytes(body.Offset.ToString());
                os.Write(iSOBytes, 0, iSOBytes.Length);
                iSOBytes = DocWriter.GetISOBytes("\n%%EOF\n");
                os.Write(iSOBytes, 0, iSOBytes.Length);
            }
            else
            {
                new PdfTrailer(body.Size, body.Offset, pdfIndirectReference, info, encryption, pdfObject2, prevxref).ToPdf(this, os);
            }

            os.Flush();
            if (CloseStream)
            {
                os.Close();
            }

            GetCounter().Written(os.Counter);
        }

        internal void ApplyRotation(PdfDictionary pageN, ByteBuffer out_p)
        {
            if (rotateContents)
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

        protected internal virtual void AlterContents()
        {
            foreach (PageStamp value in pagesToContent.Values)
            {
                PdfDictionary pageN = value.pageN;
                MarkUsed(pageN);
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
                    MarkUsed(pdfArray);
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
                if (value.under != null)
                {
                    byteBuffer.Append(PdfContents.SAVESTATE);
                    ApplyRotation(pageN, byteBuffer);
                    byteBuffer.Append(value.under.InternalBuffer);
                    byteBuffer.Append(PdfContents.RESTORESTATE);
                }

                if (value.over != null)
                {
                    byteBuffer.Append(PdfContents.SAVESTATE);
                }

                PdfStream pdfStream = new PdfStream(byteBuffer.ToByteArray());
                pdfStream.FlateCompress(compressionLevel);
                pdfArray.AddFirst(AddToBody(pdfStream).IndirectReference);
                byteBuffer.Reset();
                if (value.over != null)
                {
                    byteBuffer.Append(' ');
                    byteBuffer.Append(PdfContents.RESTORESTATE);
                    ByteBuffer internalBuffer = value.over.InternalBuffer;
                    byteBuffer.Append(internalBuffer.Buffer, 0, value.replacePoint);
                    byteBuffer.Append(PdfContents.SAVESTATE);
                    ApplyRotation(pageN, byteBuffer);
                    byteBuffer.Append(internalBuffer.Buffer, value.replacePoint, internalBuffer.Size - value.replacePoint);
                    byteBuffer.Append(PdfContents.RESTORESTATE);
                    pdfStream = new PdfStream(byteBuffer.ToByteArray());
                    pdfStream.FlateCompress(compressionLevel);
                    pdfArray.Add(AddToBody(pdfStream).IndirectReference);
                }

                AlterResources(value);
            }
        }

        internal void AlterResources(PageStamp ps)
        {
            ps.pageN.Put(PdfName.RESOURCES, ps.pageResources.Resources);
        }

        protected internal override int GetNewObjectNumber(PdfReader reader, int number, int generation)
        {
            if (readers2intrefs.TryGetValue(reader, out var value))
            {
                int num = value[number];
                if (num == 0)
                {
                    num = (value[number] = IndirectReferenceNumber);
                }

                return num;
            }

            if (currentPdfReaderInstance == null)
            {
                if (append && number < initialXrefSize)
                {
                    return number;
                }

                int num2 = myXref[number];
                if (num2 == 0)
                {
                    num2 = IndirectReferenceNumber;
                    myXref[number] = num2;
                }

                return num2;
            }

            return currentPdfReaderInstance.GetNewObjectNumber(number, generation);
        }

        internal override RandomAccessFileOrArray GetReaderFile(PdfReader reader)
        {
            if (readers2intrefs.ContainsKey(reader))
            {
                if (readers2file.TryGetValue(reader, out var value))
                {
                    return value;
                }

                return reader.SafeFile;
            }

            if (currentPdfReaderInstance == null)
            {
                return file;
            }

            return currentPdfReaderInstance.ReaderFile;
        }

        public virtual void RegisterReader(PdfReader reader, bool openFile)
        {
            if (!readers2intrefs.ContainsKey(reader))
            {
                readers2intrefs[reader] = new IntHashtable();
                if (openFile)
                {
                    RandomAccessFileOrArray safeFile = reader.SafeFile;
                    readers2file[reader] = safeFile;
                    safeFile.ReOpen();
                }
            }
        }

        public virtual void UnRegisterReader(PdfReader reader)
        {
            if (!readers2intrefs.ContainsKey(reader))
            {
                return;
            }

            readers2intrefs.Remove(reader);
            if (readers2file.TryGetValue(reader, out var value))
            {
                readers2file.Remove(reader);
                try
                {
                    value.Close();
                }
                catch
                {
                }
            }
        }

        internal static void FindAllObjects(PdfReader reader, PdfObject obj, IntHashtable hits)
        {
            if (obj == null)
            {
                return;
            }

            switch (obj.Type)
            {
                case 10:
                    {
                        PRIndirectReference pRIndirectReference = (PRIndirectReference)obj;
                        if (reader == pRIndirectReference.Reader && !hits.ContainsKey(pRIndirectReference.Number))
                        {
                            hits[pRIndirectReference.Number] = 1;
                            FindAllObjects(reader, PdfReader.GetPdfObject(obj), hits);
                        }

                        break;
                    }
                case 5:
                    {
                        PdfArray pdfArray = (PdfArray)obj;
                        for (int i = 0; i < pdfArray.Size; i++)
                        {
                            FindAllObjects(reader, pdfArray[i], hits);
                        }

                        break;
                    }
                case 6:
                case 7:
                    {
                        PdfDictionary pdfDictionary = (PdfDictionary)obj;
                        foreach (PdfName key in pdfDictionary.Keys)
                        {
                            FindAllObjects(reader, pdfDictionary.Get(key), hits);
                        }

                        break;
                    }
                case 8:
                case 9:
                    break;
            }
        }

        public virtual void AddComments(FdfReader fdf)
        {
            if (readers2intrefs.ContainsKey(fdf))
            {
                return;
            }

            PdfDictionary catalog = fdf.Catalog;
            catalog = catalog.GetAsDict(PdfName.FDF);
            if (catalog == null)
            {
                return;
            }

            PdfArray asArray = catalog.GetAsArray(PdfName.ANNOTS);
            if (asArray == null || asArray.Size == 0)
            {
                return;
            }

            RegisterReader(fdf, openFile: false);
            IntHashtable intHashtable = new IntHashtable();
            Dictionary<string, PdfObject> dictionary = new Dictionary<string, PdfObject>();
            List<PdfObject> list = new List<PdfObject>();
            for (int i = 0; i < asArray.Size; i++)
            {
                PdfObject pdfObject = asArray[i];
                PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObject(pdfObject);
                PdfNumber asNumber = pdfDictionary.GetAsNumber(PdfName.PAGE);
                if (asNumber == null || asNumber.IntValue >= reader.NumberOfPages)
                {
                    continue;
                }

                FindAllObjects(fdf, pdfObject, intHashtable);
                list.Add(pdfObject);
                if (pdfObject.Type == 10)
                {
                    PdfObject pdfObject2 = PdfReader.GetPdfObject(pdfDictionary.Get(PdfName.NM));
                    if (pdfObject2 != null && pdfObject2.Type == 3)
                    {
                        dictionary[pdfObject2.ToString()] = pdfObject;
                    }
                }
            }

            int[] keys = intHashtable.GetKeys();
            foreach (int num in keys)
            {
                PdfObject pdfObject3 = fdf.GetPdfObject(num);
                if (pdfObject3.Type == 6)
                {
                    PdfObject pdfObject4 = PdfReader.GetPdfObject(((PdfDictionary)pdfObject3).Get(PdfName.IRT));
                    if (pdfObject4 != null && pdfObject4.Type == 3)
                    {
                        dictionary.TryGetValue(pdfObject4.ToString(), out var value);
                        if (value != null)
                        {
                            PdfDictionary pdfDictionary2 = new PdfDictionary();
                            pdfDictionary2.Merge((PdfDictionary)pdfObject3);
                            pdfDictionary2.Put(PdfName.IRT, value);
                            pdfObject3 = pdfDictionary2;
                        }
                    }
                }

                AddToBody(pdfObject3, GetNewObjectNumber(fdf, num, 0));
            }

            for (int k = 0; k < list.Count; k++)
            {
                PdfObject obj = list[k];
                PdfNumber asNumber2 = ((PdfDictionary)PdfReader.GetPdfObject(obj)).GetAsNumber(PdfName.PAGE);
                PdfDictionary pageN = reader.GetPageN(asNumber2.IntValue + 1);
                PdfArray pdfArray = (PdfArray)PdfReader.GetPdfObject(pageN.Get(PdfName.ANNOTS), pageN);
                if (pdfArray == null)
                {
                    pdfArray = new PdfArray();
                    pageN.Put(PdfName.ANNOTS, pdfArray);
                    MarkUsed(pageN);
                }

                MarkUsed(pdfArray);
                pdfArray.Add(obj);
            }
        }

        internal PageStamp GetPageStamp(int pageNum)
        {
            PdfDictionary pageN = reader.GetPageN(pageNum);
            pagesToContent.TryGetValue(pageN, out var value);
            if (value == null)
            {
                value = new PageStamp(this, reader, pageN);
                pagesToContent[pageN] = value;
            }

            return value;
        }

        internal PdfContentByte GetUnderContent(int pageNum)
        {
            if (pageNum < 1 || pageNum > reader.NumberOfPages)
            {
                return null;
            }

            PageStamp pageStamp = GetPageStamp(pageNum);
            if (pageStamp.under == null)
            {
                pageStamp.under = new StampContent(this, pageStamp);
            }

            return pageStamp.under;
        }

        internal PdfContentByte GetOverContent(int pageNum)
        {
            if (pageNum < 1 || pageNum > reader.NumberOfPages)
            {
                return null;
            }

            PageStamp pageStamp = GetPageStamp(pageNum);
            if (pageStamp.over == null)
            {
                pageStamp.over = new StampContent(this, pageStamp);
            }

            return pageStamp.over;
        }

        internal void CorrectAcroFieldPages(int page)
        {
            if (acroFields == null || page > reader.NumberOfPages)
            {
                return;
            }

            foreach (AcroFields.Item value in acroFields.Fields.Values)
            {
                for (int i = 0; i < value.Size; i++)
                {
                    int page2 = value.GetPage(i);
                    if (page2 >= page)
                    {
                        value.ForcePage(i, page2 + 1);
                    }
                }
            }
        }

        private static void MoveRectangle(PdfDictionary dic2, PdfReader r, int pageImported, PdfName key, string name)
        {
            Rectangle boxSize = r.GetBoxSize(pageImported, name);
            if (boxSize == null)
            {
                dic2.Remove(key);
            }
            else
            {
                dic2.Put(key, new PdfRectangle(boxSize));
            }
        }

        internal void ReplacePage(PdfReader r, int pageImported, int pageReplaced)
        {
            PdfDictionary pageN = reader.GetPageN(pageReplaced);
            if (pagesToContent.ContainsKey(pageN))
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("this.page.cannot.be.replaced.new.content.was.already.added"));
            }

            PdfImportedPage importedPage = GetImportedPage(r, pageImported);
            PdfDictionary pageNRelease = reader.GetPageNRelease(pageReplaced);
            pageNRelease.Remove(PdfName.RESOURCES);
            pageNRelease.Remove(PdfName.CONTENTS);
            MoveRectangle(pageNRelease, r, pageImported, PdfName.MEDIABOX, "media");
            MoveRectangle(pageNRelease, r, pageImported, PdfName.CROPBOX, "crop");
            MoveRectangle(pageNRelease, r, pageImported, PdfName.TRIMBOX, "trim");
            MoveRectangle(pageNRelease, r, pageImported, PdfName.ARTBOX, "art");
            MoveRectangle(pageNRelease, r, pageImported, PdfName.BLEEDBOX, "bleed");
            pageNRelease.Put(PdfName.ROTATE, new PdfNumber(r.GetPageRotation(pageImported)));
            GetOverContent(pageReplaced).AddTemplate(importedPage, 0f, 0f);
            PageStamp pageStamp = pagesToContent[pageN];
            pageStamp.replacePoint = pageStamp.over.InternalBuffer.Size;
        }

        internal void InsertPage(int pageNumber, Rectangle mediabox)
        {
            Rectangle rectangle = new Rectangle(mediabox);
            int num = rectangle.Rotation % 360;
            PdfDictionary pdfDictionary = new PdfDictionary(PdfName.PAGE);
            pdfDictionary.Put(PdfName.RESOURCES, new PdfDictionary());
            pdfDictionary.Put(PdfName.ROTATE, new PdfNumber(num));
            pdfDictionary.Put(PdfName.MEDIABOX, new PdfRectangle(rectangle, num));
            PRIndirectReference pRIndirectReference = reader.AddPdfObject(pdfDictionary);
            PdfDictionary pdfDictionary2;
            PRIndirectReference pRIndirectReference2;
            if (pageNumber > reader.NumberOfPages)
            {
                pRIndirectReference2 = (PRIndirectReference)reader.GetPageNRelease(reader.NumberOfPages).Get(PdfName.PARENT);
                pRIndirectReference2 = new PRIndirectReference(reader, pRIndirectReference2.Number);
                pdfDictionary2 = (PdfDictionary)PdfReader.GetPdfObject(pRIndirectReference2);
                PdfArray pdfArray = (PdfArray)PdfReader.GetPdfObject(pdfDictionary2.Get(PdfName.KIDS), pdfDictionary2);
                pdfArray.Add(pRIndirectReference);
                MarkUsed(pdfArray);
                reader.pageRefs.InsertPage(pageNumber, pRIndirectReference);
            }
            else
            {
                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }

                PdfDictionary pageN = reader.GetPageN(pageNumber);
                PRIndirectReference pageOrigRef = reader.GetPageOrigRef(pageNumber);
                reader.ReleasePage(pageNumber);
                pRIndirectReference2 = (PRIndirectReference)pageN.Get(PdfName.PARENT);
                pRIndirectReference2 = new PRIndirectReference(reader, pRIndirectReference2.Number);
                pdfDictionary2 = (PdfDictionary)PdfReader.GetPdfObject(pRIndirectReference2);
                PdfArray pdfArray2 = (PdfArray)PdfReader.GetPdfObject(pdfDictionary2.Get(PdfName.KIDS), pdfDictionary2);
                int size = pdfArray2.Size;
                int number = pageOrigRef.Number;
                for (int i = 0; i < size; i++)
                {
                    PRIndirectReference pRIndirectReference3 = (PRIndirectReference)pdfArray2[i];
                    if (number == pRIndirectReference3.Number)
                    {
                        pdfArray2.Add(i, pRIndirectReference);
                        break;
                    }
                }

                if (size == pdfArray2.Size)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("internal.inconsistence"));
                }

                MarkUsed(pdfArray2);
                reader.pageRefs.InsertPage(pageNumber, pRIndirectReference);
                CorrectAcroFieldPages(pageNumber);
            }

            pdfDictionary.Put(PdfName.PARENT, pRIndirectReference2);
            while (pdfDictionary2 != null)
            {
                MarkUsed(pdfDictionary2);
                PdfNumber pdfNumber = (PdfNumber)PdfReader.GetPdfObjectRelease(pdfDictionary2.Get(PdfName.COUNT));
                pdfDictionary2.Put(PdfName.COUNT, new PdfNumber(pdfNumber.IntValue + 1));
                pdfDictionary2 = pdfDictionary2.GetAsDict(PdfName.PARENT);
            }
        }

        internal AcroFields GetAcroFields()
        {
            if (acroFields == null)
            {
                acroFields = new AcroFields(reader, this);
            }

            return acroFields;
        }

        internal bool PartialFormFlattening(string name)
        {
            GetAcroFields();
            if (acroFields.Xfa.XfaPresent)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("partial.form.flattening.is.not.supported.with.xfa.forms"));
            }

            if (!acroFields.Fields.ContainsKey(name))
            {
                return false;
            }

            partialFlattening.Add(name);
            return true;
        }

        protected internal virtual void FlatFields()
        {
            if (append)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("field.flattening.is.not.supported.in.append.mode"));
            }

            GetAcroFields();
            IDictionary<string, AcroFields.Item> fields = acroFields.Fields;
            if (fieldsAdded && partialFlattening.IsEmpty())
            {
                foreach (string key2 in fields.Keys)
                {
                    partialFlattening.Add(key2);
                }
            }

            PdfDictionary asDict = reader.Catalog.GetAsDict(PdfName.ACROFORM);
            PdfArray pdfArray = null;
            if (asDict != null)
            {
                pdfArray = (PdfArray)PdfReader.GetPdfObject(asDict.Get(PdfName.FIELDS), asDict);
            }

            foreach (KeyValuePair<string, AcroFields.Item> item in fields)
            {
                string key = item.Key;
                if (!partialFlattening.IsEmpty() && !partialFlattening.Contains(key))
                {
                    continue;
                }

                AcroFields.Item value = item.Value;
                for (int i = 0; i < value.Size; i++)
                {
                    PdfDictionary merged = value.GetMerged(i);
                    PdfNumber asNumber = merged.GetAsNumber(PdfName.F);
                    int num = 0;
                    if (asNumber != null)
                    {
                        num = asNumber.IntValue;
                    }

                    int page = value.GetPage(i);
                    if (page < 1)
                    {
                        continue;
                    }

                    PdfDictionary asDict2 = merged.GetAsDict(PdfName.AP);
                    PdfObject pdfObject = null;
                    if (asDict2 != null)
                    {
                        pdfObject = asDict2.GetAsStream(PdfName.N);
                        if (pdfObject == null)
                        {
                            pdfObject = asDict2.GetAsDict(PdfName.N);
                        }
                    }

                    if (acroFields.GenerateAppearances)
                    {
                        if (asDict2 == null || pdfObject == null)
                        {
                            try
                            {
                                acroFields.RegenerateField(key);
                                asDict2 = acroFields.GetFieldItem(key).GetMerged(i).GetAsDict(PdfName.AP);
                            }
                            catch (DocumentException)
                            {
                            }
                        }
                        else if (pdfObject.IsStream())
                        {
                            PdfStream pdfStream = (PdfStream)pdfObject;
                            PdfArray asArray = pdfStream.GetAsArray(PdfName.BBOX);
                            PdfArray asArray2 = merged.GetAsArray(PdfName.RECT);
                            if (asArray != null && asArray2 != null)
                            {
                                float num2 = asArray2.GetAsNumber(2).FloatValue - asArray2.GetAsNumber(0).FloatValue;
                                float num3 = asArray.GetAsNumber(2).FloatValue - asArray.GetAsNumber(0).FloatValue;
                                float num4 = asArray2.GetAsNumber(3).FloatValue - asArray2.GetAsNumber(1).FloatValue;
                                float num5 = asArray.GetAsNumber(3).FloatValue - asArray.GetAsNumber(1).FloatValue;
                                float num6 = Math.Abs((num3 != 0f) ? (num2 / num3) : float.MaxValue);
                                float num7 = Math.Abs((num5 != 0f) ? (num4 / num5) : float.MaxValue);
                                if (num6 != 1f || num7 != 1f)
                                {
                                    NumberArray value2 = new NumberArray(num6, 0f, 0f, num7, 0f, 0f);
                                    pdfStream.Put(PdfName.MATRIX, value2);
                                    MarkUsed(pdfStream);
                                }
                            }
                        }
                    }
                    else if (asDict2 != null && pdfObject != null)
                    {
                        PdfArray asArray3 = ((PdfDictionary)pdfObject).GetAsArray(PdfName.BBOX);
                        PdfArray asArray4 = merged.GetAsArray(PdfName.RECT);
                        if (asArray3 != null && asArray4 != null)
                        {
                            float value3 = asArray3.GetAsNumber(2).FloatValue - asArray3.GetAsNumber(0).FloatValue - (asArray4.GetAsNumber(2).FloatValue - asArray4.GetAsNumber(0).FloatValue);
                            float value4 = asArray3.GetAsNumber(3).FloatValue - asArray3.GetAsNumber(1).FloatValue - (asArray4.GetAsNumber(3).FloatValue - asArray4.GetAsNumber(1).FloatValue);
                            if (Math.Abs(value3) > 1f || Math.Abs(value4) > 1f)
                            {
                                try
                                {
                                    acroFields.GenerateAppearances = true;
                                    acroFields.RegenerateField(key);
                                    acroFields.GenerateAppearances = false;
                                    asDict2 = acroFields.GetFieldItem(key).GetMerged(i).GetAsDict(PdfName.AP);
                                }
                                catch (DocumentException)
                                {
                                }
                            }
                        }
                    }

                    if (asDict2 != null && ((uint)num & 4u) != 0 && (num & 2) == 0)
                    {
                        PdfObject pdfObject2 = asDict2.Get(PdfName.N);
                        PdfAppearance pdfAppearance = null;
                        if (pdfObject2 != null)
                        {
                            PdfObject pdfObject3 = PdfReader.GetPdfObject(pdfObject2);
                            if (pdfObject2 is PdfIndirectReference && !pdfObject2.IsIndirect())
                            {
                                pdfAppearance = new PdfAppearance((PdfIndirectReference)pdfObject2);
                            }
                            else if (pdfObject3 is PdfStream)
                            {
                                ((PdfDictionary)pdfObject3).Put(PdfName.SUBTYPE, PdfName.FORM);
                                pdfAppearance = new PdfAppearance((PdfIndirectReference)pdfObject2);
                            }
                            else if (pdfObject3 != null && pdfObject3.IsDictionary())
                            {
                                PdfName asName = merged.GetAsName(PdfName.AS);
                                if (asName != null)
                                {
                                    PdfIndirectReference pdfIndirectReference = (PdfIndirectReference)((PdfDictionary)pdfObject3).Get(asName);
                                    if (pdfIndirectReference != null)
                                    {
                                        pdfAppearance = new PdfAppearance(pdfIndirectReference);
                                        if (pdfIndirectReference.IsIndirect())
                                        {
                                            pdfObject3 = PdfReader.GetPdfObject(pdfIndirectReference);
                                            ((PdfDictionary)pdfObject3).Put(PdfName.SUBTYPE, PdfName.FORM);
                                        }
                                    }
                                }
                            }
                        }

                        if (pdfAppearance != null)
                        {
                            Rectangle normalizedRectangle = PdfReader.GetNormalizedRectangle(merged.GetAsArray(PdfName.RECT));
                            PdfContentByte overContent = GetOverContent(page);
                            overContent.SetLiteral("Q ");
                            overContent.AddTemplate(pdfAppearance, normalizedRectangle.Left, normalizedRectangle.Bottom);
                            overContent.SetLiteral("q ");
                        }
                    }

                    if (partialFlattening.IsEmpty())
                    {
                        continue;
                    }

                    PdfDictionary pageN = reader.GetPageN(page);
                    PdfArray asArray5 = pageN.GetAsArray(PdfName.ANNOTS);
                    if (asArray5 == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < asArray5.Size; j++)
                    {
                        PdfObject pdfObject4 = asArray5.GetPdfObject(j);
                        if (!pdfObject4.IsIndirect())
                        {
                            continue;
                        }

                        PdfObject widgetRef = value.GetWidgetRef(i);
                        if (!widgetRef.IsIndirect() || ((PRIndirectReference)pdfObject4).Number != ((PRIndirectReference)widgetRef).Number)
                        {
                            continue;
                        }

                        asArray5.Remove(j--);
                        PRIndirectReference pRIndirectReference = (PRIndirectReference)widgetRef;
                        while (true)
                        {
                            PRIndirectReference pRIndirectReference2 = (PRIndirectReference)((PdfDictionary)PdfReader.GetPdfObject(pRIndirectReference)).Get(PdfName.PARENT);
                            PdfReader.KillIndirect(pRIndirectReference);
                            if (pRIndirectReference2 == null)
                            {
                                for (int k = 0; k < pdfArray.Size; k++)
                                {
                                    PdfObject pdfObject5 = pdfArray.GetPdfObject(k);
                                    if (pdfObject5.IsIndirect() && ((PRIndirectReference)pdfObject5).Number == pRIndirectReference.Number)
                                    {
                                        pdfArray.Remove(k);
                                        k--;
                                    }
                                }

                                break;
                            }

                            PdfArray asArray6 = ((PdfDictionary)PdfReader.GetPdfObject(pRIndirectReference2)).GetAsArray(PdfName.KIDS);
                            for (int l = 0; l < asArray6.Size; l++)
                            {
                                PdfObject pdfObject6 = asArray6.GetPdfObject(l);
                                if (pdfObject6.IsIndirect() && ((PRIndirectReference)pdfObject6).Number == pRIndirectReference.Number)
                                {
                                    asArray6.Remove(l);
                                    l--;
                                }
                            }

                            if (!asArray6.IsEmpty())
                            {
                                break;
                            }

                            pRIndirectReference = pRIndirectReference2;
                        }
                    }

                    if (asArray5.IsEmpty())
                    {
                        PdfReader.KillIndirect(pageN.Get(PdfName.ANNOTS));
                        pageN.Remove(PdfName.ANNOTS);
                    }
                }
            }

            if (fieldsAdded || !partialFlattening.IsEmpty())
            {
                return;
            }

            for (int m = 1; m <= reader.NumberOfPages; m++)
            {
                PdfDictionary pageN2 = reader.GetPageN(m);
                PdfArray asArray7 = pageN2.GetAsArray(PdfName.ANNOTS);
                if (asArray7 == null)
                {
                    continue;
                }

                for (int n = 0; n < asArray7.Size; n++)
                {
                    PdfObject directObject = asArray7.GetDirectObject(n);
                    if ((!(directObject is PdfIndirectReference) || directObject.IsIndirect()) && (!directObject.IsDictionary() || PdfName.WIDGET.Equals(((PdfDictionary)directObject).Get(PdfName.SUBTYPE))))
                    {
                        asArray7.Remove(n);
                        n--;
                    }
                }

                if (asArray7.IsEmpty())
                {
                    PdfReader.KillIndirect(pageN2.Get(PdfName.ANNOTS));
                    pageN2.Remove(PdfName.ANNOTS);
                }
            }

            EliminateAcroformObjects();
        }

        internal void EliminateAcroformObjects()
        {
            PdfObject pdfObject = reader.Catalog.Get(PdfName.ACROFORM);
            if (pdfObject != null)
            {
                PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObject(pdfObject);
                reader.KillXref(pdfDictionary.Get(PdfName.XFA));
                pdfDictionary.Remove(PdfName.XFA);
                PdfObject pdfObject2 = pdfDictionary.Get(PdfName.FIELDS);
                if (pdfObject2 != null)
                {
                    PdfDictionary pdfDictionary2 = new PdfDictionary();
                    pdfDictionary2.Put(PdfName.KIDS, pdfObject2);
                    SweepKids(pdfDictionary2);
                    PdfReader.KillIndirect(pdfObject2);
                    pdfDictionary.Put(PdfName.FIELDS, new PdfArray());
                }

                pdfDictionary.Remove(PdfName.SIGFLAGS);
                pdfDictionary.Remove(PdfName.NEEDAPPEARANCES);
                pdfDictionary.Remove(PdfName.DR);
            }
        }

        internal void SweepKids(PdfObject obj)
        {
            PdfObject pdfObject = PdfReader.KillIndirect(obj);
            if (pdfObject == null || !pdfObject.IsDictionary())
            {
                return;
            }

            PdfArray pdfArray = (PdfArray)PdfReader.KillIndirect(((PdfDictionary)pdfObject).Get(PdfName.KIDS));
            if (pdfArray != null)
            {
                for (int i = 0; i < pdfArray.Size; i++)
                {
                    SweepKids(pdfArray.GetPdfObject(i));
                }
            }
        }

        protected internal virtual void FlattenAnnotations()
        {
            FlattenAnnotations(flattenFreeTextAnnotations: false);
        }

        private void FlattenAnnotations(bool flattenFreeTextAnnotations)
        {
            if (append)
            {
                if (flattenFreeTextAnnotations)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("freetext.flattening.is.not.supported.in.append.mode"));
                }

                throw new ArgumentException(MessageLocalization.GetComposedMessage("annotation.flattening.is.not.supported.in.append.mode"));
            }

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                PdfDictionary pageN = reader.GetPageN(i);
                PdfArray asArray = pageN.GetAsArray(PdfName.ANNOTS);
                if (asArray == null)
                {
                    continue;
                }

                for (int j = 0; j < asArray.Size; j++)
                {
                    PdfObject directObject = asArray.GetDirectObject(j);
                    if ((directObject is PdfIndirectReference && !directObject.IsIndirect()) || !(directObject is PdfDictionary))
                    {
                        continue;
                    }

                    PdfDictionary pdfDictionary = (PdfDictionary)directObject;
                    if (flattenFreeTextAnnotations)
                    {
                        if (!pdfDictionary.Get(PdfName.SUBTYPE).Equals(PdfName.FREETEXT))
                        {
                            continue;
                        }
                    }
                    else if (pdfDictionary.Get(PdfName.SUBTYPE).Equals(PdfName.WIDGET))
                    {
                        continue;
                    }

                    int num = pdfDictionary.GetAsNumber(PdfName.F)?.IntValue ?? 0;
                    if ((num & 4) == 0 || ((uint)num & 2u) != 0)
                    {
                        continue;
                    }

                    PdfObject pdfObject = pdfDictionary.Get(PdfName.AP);
                    if (pdfObject == null)
                    {
                        continue;
                    }

                    PdfDictionary pdfDictionary2 = ((pdfObject is PdfIndirectReference) ? ((PdfDictionary)PdfReader.GetPdfObject(pdfObject)) : ((PdfDictionary)pdfObject));
                    PdfObject pdfObject2 = pdfDictionary2.Get(PdfName.N);
                    PdfAppearance pdfAppearance = null;
                    PdfObject pdfObject3 = PdfReader.GetPdfObject(pdfObject2);
                    if (pdfObject2 is PdfIndirectReference && !pdfObject2.IsIndirect())
                    {
                        pdfAppearance = new PdfAppearance((PdfIndirectReference)pdfObject2);
                    }
                    else if (pdfObject3 is PdfStream)
                    {
                        ((PdfDictionary)pdfObject3).Put(PdfName.SUBTYPE, PdfName.FORM);
                        pdfAppearance = new PdfAppearance((PdfIndirectReference)pdfObject2);
                    }
                    else if (pdfObject3.IsDictionary())
                    {
                        PdfName asName = pdfDictionary2.GetAsName(PdfName.AS);
                        if (asName != null)
                        {
                            PdfIndirectReference pdfIndirectReference = (PdfIndirectReference)((PdfDictionary)pdfObject3).Get(asName);
                            if (pdfIndirectReference != null)
                            {
                                pdfAppearance = new PdfAppearance(pdfIndirectReference);
                                if (pdfIndirectReference.IsIndirect())
                                {
                                    pdfObject3 = PdfReader.GetPdfObject(pdfIndirectReference);
                                    ((PdfDictionary)pdfObject3).Put(PdfName.SUBTYPE, PdfName.FORM);
                                }
                            }
                        }
                    }

                    if (pdfAppearance != null)
                    {
                        Rectangle normalizedRectangle = PdfReader.GetNormalizedRectangle(pdfDictionary.GetAsArray(PdfName.RECT));
                        PdfContentByte overContent = GetOverContent(i);
                        overContent.SetLiteral("Q ");
                        overContent.AddTemplate(pdfAppearance, normalizedRectangle.Left, normalizedRectangle.Bottom);
                        overContent.SetLiteral("q ");
                        asArray.Remove(j);
                        j--;
                    }
                }

                if (asArray.IsEmpty())
                {
                    PdfReader.KillIndirect(pageN.Get(PdfName.ANNOTS));
                    pageN.Remove(PdfName.ANNOTS);
                }
            }
        }

        protected internal virtual void FlatFreeTextFields()
        {
            FlattenAnnotations(flattenFreeTextAnnotations: true);
        }

        public override PdfIndirectReference GetPageReference(int page)
        {
            PdfIndirectReference pageOrigRef = reader.GetPageOrigRef(page);
            if (pageOrigRef == null)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.page.number.1", page));
            }

            return pageOrigRef;
        }

        public override void AddAnnotation(PdfAnnotation annot)
        {
            throw new Exception(MessageLocalization.GetComposedMessage("unsupported.in.this.context.use.pdfstamper.addannotation"));
        }

        internal void AddDocumentField(PdfIndirectReference ref_p)
        {
            PdfDictionary catalog = reader.Catalog;
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObject(catalog.Get(PdfName.ACROFORM), catalog);
            if (pdfDictionary == null)
            {
                pdfDictionary = new PdfDictionary();
                catalog.Put(PdfName.ACROFORM, pdfDictionary);
                MarkUsed(catalog);
            }

            PdfArray pdfArray = (PdfArray)PdfReader.GetPdfObject(pdfDictionary.Get(PdfName.FIELDS), pdfDictionary);
            if (pdfArray == null)
            {
                pdfArray = new PdfArray();
                pdfDictionary.Put(PdfName.FIELDS, pdfArray);
                MarkUsed(pdfDictionary);
            }

            if (!pdfDictionary.Contains(PdfName.DA))
            {
                pdfDictionary.Put(PdfName.DA, new PdfString("/Helv 0 Tf 0 g "));
                MarkUsed(pdfDictionary);
            }

            pdfArray.Add(ref_p);
            MarkUsed(pdfArray);
        }

        protected internal virtual void AddFieldResources()
        {
            if (fieldTemplates.Count == 0)
            {
                return;
            }

            PdfDictionary catalog = reader.Catalog;
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObject(catalog.Get(PdfName.ACROFORM), catalog);
            if (pdfDictionary == null)
            {
                pdfDictionary = new PdfDictionary();
                catalog.Put(PdfName.ACROFORM, pdfDictionary);
                MarkUsed(catalog);
            }

            PdfDictionary pdfDictionary2 = (PdfDictionary)PdfReader.GetPdfObject(pdfDictionary.Get(PdfName.DR), pdfDictionary);
            if (pdfDictionary2 == null)
            {
                pdfDictionary2 = new PdfDictionary();
                pdfDictionary.Put(PdfName.DR, pdfDictionary2);
                MarkUsed(pdfDictionary);
            }

            MarkUsed(pdfDictionary2);
            foreach (PdfTemplate fieldTemplate in fieldTemplates)
            {
                PdfFormField.MergeResources(pdfDictionary2, (PdfDictionary)fieldTemplate.Resources, this);
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

            if (pdfDictionary.Get(PdfName.DA) == null)
            {
                pdfDictionary.Put(PdfName.DA, new PdfString("/Helv 0 Tf 0 g "));
                MarkUsed(pdfDictionary);
            }
        }

        internal void ExpandFields(PdfFormField field, List<PdfAnnotation> allAnnots)
        {
            allAnnots.Add(field);
            List<PdfFormField> kids = field.Kids;
            if (kids != null)
            {
                for (int i = 0; i < kids.Count; i++)
                {
                    ExpandFields(kids[i], allAnnots);
                }
            }
        }

        internal void AddAnnotation(PdfAnnotation annot, PdfDictionary pageN)
        {
            List<PdfAnnotation> list = new List<PdfAnnotation>();
            if (annot.IsForm())
            {
                fieldsAdded = true;
                GetAcroFields();
                PdfFormField pdfFormField = (PdfFormField)annot;
                if (pdfFormField.Parent != null)
                {
                    return;
                }

                ExpandFields(pdfFormField, list);
            }
            else
            {
                list.Add(annot);
            }

            for (int i = 0; i < list.Count; i++)
            {
                annot = list[i];
                if (annot.PlaceInPage > 0)
                {
                    pageN = reader.GetPageN(annot.PlaceInPage);
                }

                if (annot.IsForm())
                {
                    if (!annot.IsUsed())
                    {
                        HashSet2<PdfTemplate> templates = annot.GetTemplates();
                        if (templates != null)
                        {
                            fieldTemplates.AddAll(templates);
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
                        MarkUsed(pageN);
                    }
                    else
                    {
                        pdfArray = (PdfArray)pdfObject;
                    }

                    pdfArray.Add(annot.IndirectReference);
                    MarkUsed(pdfArray);
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
                                    annot.Put(PdfName.RECT, new PdfRectangle(pageSizeWithRotation.Top - pdfRectangle.Top, pdfRectangle.Right, pageSizeWithRotation.Top - pdfRectangle.Bottom, pdfRectangle.Left));
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
                    AddToBody(annot, annot.IndirectReference);
                }
            }
        }

        internal override void AddAnnotation(PdfAnnotation annot, int page)
        {
            annot.Page = page;
            AddAnnotation(annot, reader.GetPageN(page));
        }

        private void OutlineTravel(PRIndirectReference outline)
        {
            while (outline != null)
            {
                PdfDictionary obj = (PdfDictionary)PdfReader.GetPdfObjectRelease(outline);
                PRIndirectReference pRIndirectReference = (PRIndirectReference)obj.Get(PdfName.FIRST);
                if (pRIndirectReference != null)
                {
                    OutlineTravel(pRIndirectReference);
                }

                PdfReader.KillIndirect(obj.Get(PdfName.DEST));
                PdfReader.KillIndirect(obj.Get(PdfName.A));
                PdfReader.KillIndirect(outline);
                outline = (PRIndirectReference)obj.Get(PdfName.NEXT);
            }
        }

        internal void DeleteOutlines()
        {
            PdfDictionary catalog = reader.Catalog;
            PdfObject pdfObject = catalog.Get(PdfName.OUTLINES);
            if (pdfObject != null)
            {
                if (pdfObject is PRIndirectReference)
                {
                    PRIndirectReference pRIndirectReference = (PRIndirectReference)pdfObject;
                    OutlineTravel(pRIndirectReference);
                    PdfReader.KillIndirect(pRIndirectReference);
                }

                catalog.Remove(PdfName.OUTLINES);
                MarkUsed(catalog);
            }
        }

        protected internal virtual void SetJavaScript()
        {
            Dictionary<string, PdfObject> documentLevelJS = pdf.GetDocumentLevelJS();
            if (documentLevelJS.Count != 0)
            {
                PdfDictionary catalog = reader.Catalog;
                PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObject(catalog.Get(PdfName.NAMES), catalog);
                if (pdfDictionary == null)
                {
                    pdfDictionary = new PdfDictionary();
                    catalog.Put(PdfName.NAMES, pdfDictionary);
                    MarkUsed(catalog);
                }

                MarkUsed(pdfDictionary);
                PdfDictionary objecta = PdfNameTree.WriteTree(documentLevelJS, this);
                pdfDictionary.Put(PdfName.JAVASCRIPT, AddToBody(objecta).IndirectReference);
            }
        }

        protected virtual void AddFileAttachments()
        {
            Dictionary<string, PdfObject> documentFileAttachment = pdf.GetDocumentFileAttachment();
            if (documentFileAttachment.Count == 0)
            {
                return;
            }

            PdfDictionary catalog = reader.Catalog;
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObject(catalog.Get(PdfName.NAMES), catalog);
            if (pdfDictionary == null)
            {
                pdfDictionary = new PdfDictionary();
                catalog.Put(PdfName.NAMES, pdfDictionary);
                MarkUsed(catalog);
            }

            MarkUsed(pdfDictionary);
            Dictionary<string, PdfObject> dictionary = PdfNameTree.ReadTree((PdfDictionary)PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.EMBEDDEDFILES)));
            foreach (KeyValuePair<string, PdfObject> item in documentFileAttachment)
            {
                string key = item.Key;
                int num = 0;
                StringBuilder stringBuilder = new StringBuilder(key);
                while (dictionary.ContainsKey(stringBuilder.ToString()))
                {
                    num++;
                    stringBuilder.Append(' ').Append(num);
                }

                dictionary[stringBuilder.ToString()] = item.Value;
            }

            PdfDictionary objecta = PdfNameTree.WriteTree(dictionary, this);
            PdfObject pdfObject = pdfDictionary.Get(PdfName.EMBEDDEDFILES);
            if (pdfObject != null)
            {
                PdfReader.KillIndirect(pdfObject);
            }

            pdfDictionary.Put(PdfName.EMBEDDEDFILES, AddToBody(objecta).IndirectReference);
        }

        internal void MakePackage(PdfCollection collection)
        {
            reader.Catalog.Put(PdfName.COLLECTION, collection);
        }

        protected internal virtual void SetOutlines()
        {
            if (newBookmarks != null)
            {
                DeleteOutlines();
                if (newBookmarks.Count != 0)
                {
                    PdfDictionary catalog = reader.Catalog;
                    bool namedAsNames = catalog.Get(PdfName.DESTS) != null;
                    WriteOutlines(catalog, namedAsNames);
                    MarkUsed(catalog);
                }
            }
        }

        public override void AddViewerPreference(PdfName key, PdfObject value)
        {
            useVp = true;
            viewerPreferences.AddViewerPreference(key, value);
        }

        public override void SetPageAction(PdfName actionType, PdfAction action)
        {
            throw new InvalidOperationException(MessageLocalization.GetComposedMessage("use.setpageaction.pdfname.actiontype.pdfaction.action.int.page"));
        }

        internal void SetPageAction(PdfName actionType, PdfAction action, int page)
        {
            if (!actionType.Equals(PdfWriter.PAGE_OPEN) && !actionType.Equals(PdfWriter.PAGE_CLOSE))
            {
                throw new PdfException(MessageLocalization.GetComposedMessage("invalid.page.additional.action.type.1", actionType.ToString()));
            }

            PdfDictionary pageN = reader.GetPageN(page);
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObject(pageN.Get(PdfName.AA), pageN);
            if (pdfDictionary == null)
            {
                pdfDictionary = new PdfDictionary();
                pageN.Put(PdfName.AA, pdfDictionary);
                MarkUsed(pageN);
            }

            pdfDictionary.Put(actionType, action);
            MarkUsed(pdfDictionary);
        }

        internal void SetDuration(int seconds, int page)
        {
            PdfDictionary pageN = reader.GetPageN(page);
            if (seconds < 0)
            {
                pageN.Remove(PdfName.DUR);
            }
            else
            {
                pageN.Put(PdfName.DUR, new PdfNumber(seconds));
            }

            MarkUsed(pageN);
        }

        internal void SetTransition(PdfTransition transition, int page)
        {
            PdfDictionary pageN = reader.GetPageN(page);
            if (transition == null)
            {
                pageN.Remove(PdfName.TRANS);
            }
            else
            {
                pageN.Put(PdfName.TRANS, transition.TransitionDictionary);
            }

            MarkUsed(pageN);
        }

        public virtual void MarkUsed(PdfObject obj)
        {
            if (append && obj != null)
            {
                PRIndirectReference pRIndirectReference = null;
                pRIndirectReference = ((obj.Type != 10) ? obj.IndRef : ((PRIndirectReference)obj));
                if (pRIndirectReference != null)
                {
                    marked[pRIndirectReference.Number] = 1;
                }
            }
        }

        protected internal virtual void MarkUsed(int num)
        {
            if (append)
            {
                marked[num] = 1;
            }
        }

        internal bool IsAppend()
        {
            return append;
        }

        public override void SetAdditionalAction(PdfName actionType, PdfAction action)
        {
            if (!actionType.Equals(PdfWriter.DOCUMENT_CLOSE) && !actionType.Equals(PdfWriter.WILL_SAVE) && !actionType.Equals(PdfWriter.DID_SAVE) && !actionType.Equals(PdfWriter.WILL_PRINT) && !actionType.Equals(PdfWriter.DID_PRINT))
            {
                throw new PdfException(MessageLocalization.GetComposedMessage("invalid.additional.action.type.1", actionType.ToString()));
            }

            PdfDictionary pdfDictionary = reader.Catalog.GetAsDict(PdfName.AA);
            if (pdfDictionary == null)
            {
                if (action == null)
                {
                    return;
                }

                pdfDictionary = new PdfDictionary();
                reader.Catalog.Put(PdfName.AA, pdfDictionary);
            }

            MarkUsed(pdfDictionary);
            if (action == null)
            {
                pdfDictionary.Remove(actionType);
            }
            else
            {
                pdfDictionary.Put(actionType, action);
            }
        }

        public override void SetOpenAction(PdfAction action)
        {
            openAction = action;
        }

        public override void SetOpenAction(string name)
        {
            throw new InvalidOperationException(MessageLocalization.GetComposedMessage("open.actions.by.name.are.not.supported"));
        }

        internal void SetThumbnail(Image image, int page)
        {
            PdfIndirectReference imageReference = GetImageReference(AddDirectImageSimple(image));
            reader.ResetReleasePage();
            reader.GetPageN(page).Put(PdfName.THUMB, imageReference);
            reader.ResetReleasePage();
        }

        protected virtual void ReadOCProperties()
        {
            if (documentOCG.Count != 0)
            {
                return;
            }

            PdfDictionary asDict = reader.Catalog.GetAsDict(PdfName.OCPROPERTIES);
            if (asDict == null)
            {
                return;
            }

            PdfArray asArray = asDict.GetAsArray(PdfName.OCGS);
            Dictionary<string, PdfLayer> dictionary = new Dictionary<string, PdfLayer>();
            ListIterator<PdfObject> listIterator = asArray.GetListIterator();
            while (listIterator.HasNext())
            {
                PdfIndirectReference pdfIndirectReference = (PdfIndirectReference)listIterator.Next();
                PdfLayer pdfLayer = new PdfLayer(null);
                pdfLayer.Ref = pdfIndirectReference;
                pdfLayer.OnPanel = false;
                pdfLayer.Merge((PdfDictionary)PdfReader.GetPdfObject(pdfIndirectReference));
                dictionary[pdfIndirectReference.ToString()] = pdfLayer;
            }

            PdfDictionary asDict2 = asDict.GetAsDict(PdfName.D);
            PdfArray asArray2 = asDict2.GetAsArray(PdfName.OFF);
            if (asArray2 != null)
            {
                ListIterator<PdfObject> listIterator2 = asArray2.GetListIterator();
                while (listIterator2.HasNext())
                {
                    PdfIndirectReference pdfIndirectReference = (PdfIndirectReference)listIterator2.Next();
                    PdfLayer pdfLayer = dictionary[pdfIndirectReference.ToString()];
                    pdfLayer.On = false;
                }
            }

            PdfArray asArray3 = asDict2.GetAsArray(PdfName.ORDER);
            if (asArray3 != null)
            {
                AddOrder(null, asArray3, dictionary);
            }

            foreach (PdfLayer value in dictionary.Values)
            {
                documentOCG[value] = null;
            }

            OCGRadioGroup = asDict2.GetAsArray(PdfName.RBGROUPS);
            if (OCGRadioGroup == null)
            {
                OCGRadioGroup = new PdfArray();
            }

            OCGLocked = asDict2.GetAsArray(PdfName.LOCKED);
            if (OCGLocked == null)
            {
                OCGLocked = new PdfArray();
            }
        }

        private void AddOrder(PdfLayer parent, PdfArray arr, Dictionary<string, PdfLayer> ocgmap)
        {
            for (int i = 0; i < arr.Size; i++)
            {
                PdfObject pdfObject = arr[i];
                if (pdfObject.IsIndirect())
                {
                    PdfLayer pdfLayer = ocgmap[pdfObject.ToString()];
                    if (pdfLayer != null)
                    {
                        pdfLayer.OnPanel = true;
                        RegisterLayer(pdfLayer);
                        parent?.AddChild(pdfLayer);
                        if (arr.Size > i + 1 && arr[i + 1].IsArray())
                        {
                            i++;
                            AddOrder(pdfLayer, (PdfArray)arr[i], ocgmap);
                        }
                    }
                }
                else
                {
                    if (!pdfObject.IsArray())
                    {
                        continue;
                    }

                    PdfArray pdfArray = (PdfArray)pdfObject;
                    if (pdfArray.IsEmpty())
                    {
                        break;
                    }

                    pdfObject = pdfArray[0];
                    if (pdfObject.IsString())
                    {
                        PdfLayer pdfLayer = new PdfLayer(pdfObject.ToString());
                        pdfLayer.OnPanel = true;
                        RegisterLayer(pdfLayer);
                        parent?.AddChild(pdfLayer);
                        PdfArray pdfArray2 = new PdfArray();
                        ListIterator<PdfObject> listIterator = pdfArray.GetListIterator();
                        while (listIterator.HasNext())
                        {
                            pdfArray2.Add(listIterator.Next());
                        }

                        AddOrder(pdfLayer, pdfArray2, ocgmap);
                    }
                    else
                    {
                        AddOrder(parent, (PdfArray)pdfObject, ocgmap);
                    }
                }
            }
        }

        public virtual Dictionary<string, PdfLayer> GetPdfLayers()
        {
            if (documentOCG.Count == 0)
            {
                ReadOCProperties();
            }

            Dictionary<string, PdfLayer> dictionary = new Dictionary<string, PdfLayer>();
            foreach (PdfLayer key in documentOCG.Keys)
            {
                string text = ((key.Title != null) ? key.Title : key.GetAsString(PdfName.NAME).ToString());
                if (dictionary.ContainsKey(text))
                {
                    int num = 2;
                    string text2 = text + "(" + num + ")";
                    while (dictionary.ContainsKey(text2))
                    {
                        num++;
                        text2 = text + "(" + num + ")";
                    }

                    text = text2;
                }

                dictionary[text] = key;
            }

            return dictionary;
        }

        public override void CreateXmpMetadata()
        {
            try
            {
                xmpWriter = CreateXmpWriter(null, reader.Info);
                xmpMetadata = null;
            }
            catch (IOException)
            {
            }
        }
    }
}
