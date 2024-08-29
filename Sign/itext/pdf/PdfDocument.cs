using Sign.itext.error_messages;
using Sign.itext.pdf.draw;
using Sign.itext.pdf.interfaces;
using Sign.itext.text;
using Sign.itext.text.api;
using Sign.itext.text.pdf;
using Sign.itext.text.pdf.collection;
using Sign.itext.text.pdf.intern;

namespace Sign.itext.pdf
{
    public class PdfDocument : Document
    {
        public class PdfInfo : PdfDictionary
        {
            internal PdfInfo()
            {
                AddProducer();
                AddCreationDate();
            }

            internal PdfInfo(string author, string title, string subject)
            {
                AddTitle(title);
                AddSubject(subject);
                AddAuthor(author);
            }

            internal void AddTitle(string title)
            {
                Put(PdfName.TITLE, new PdfString(title, "UnicodeBig"));
            }

            internal void AddSubject(string subject)
            {
                Put(PdfName.SUBJECT, new PdfString(subject, "UnicodeBig"));
            }

            internal void AddKeywords(string keywords)
            {
                Put(PdfName.KEYWORDS, new PdfString(keywords, "UnicodeBig"));
            }

            internal void AddAuthor(string author)
            {
                Put(PdfName.AUTHOR, new PdfString(author, "UnicodeBig"));
            }

            internal void AddCreator(string creator)
            {
                Put(PdfName.CREATOR, new PdfString(creator, "UnicodeBig"));
            }

            internal void AddProducer()
            {
                Put(PdfName.PRODUCER, new PdfString(Sign.itext.text.Version.GetInstance().GetVersion));
            }

            internal void AddCreationDate()
            {
                PdfString value = new PdfDate();
                Put(PdfName.CREATIONDATE, value);
                Put(PdfName.MODDATE, value);
            }

            internal void Addkey(string key, string value)
            {
                if (!key.Equals("Producer") && !key.Equals("CreationDate"))
                {
                    Put(new PdfName(key), new PdfString(value, "UnicodeBig"));
                }
            }
        }

        internal class PdfCatalog : PdfDictionary
        {
            internal PdfWriter writer;

            internal PdfAction OpenAction
            {
                set
                {
                    Put(PdfName.OPENACTION, value);
                }
            }

            internal PdfDictionary AdditionalActions
            {
                set
                {
                    Put(PdfName.AA, writer.AddToBody(value).IndirectReference);
                }
            }

            internal PdfCatalog(PdfIndirectReference pages, PdfWriter writer)
                : base(PdfDictionary.CATALOG)
            {
                this.writer = writer;
                Put(PdfName.PAGES, pages);
            }

            internal void AddNames(SortedDictionary<string, Destination> localDestinations, Dictionary<string, PdfObject> documentLevelJS, Dictionary<string, PdfObject> documentFileAttachment, PdfWriter writer)
            {
                if (localDestinations.Count == 0 && documentLevelJS.Count == 0 && documentFileAttachment.Count == 0)
                {
                    return;
                }

                PdfDictionary pdfDictionary = new PdfDictionary();
                if (localDestinations.Count > 0)
                {
                    PdfArray pdfArray = new PdfArray();
                    foreach (string key in localDestinations.Keys)
                    {
                        if (localDestinations.TryGetValue(key, out var value))
                        {
                            PdfIndirectReference reference = value.reference;
                            pdfArray.Add(new PdfString(key, "UnicodeBig"));
                            pdfArray.Add(reference);
                        }
                    }

                    if (pdfArray.Size > 0)
                    {
                        PdfDictionary pdfDictionary2 = new PdfDictionary();
                        pdfDictionary2.Put(PdfName.NAMES, pdfArray);
                        pdfDictionary.Put(PdfName.DESTS, writer.AddToBody(pdfDictionary2).IndirectReference);
                    }
                }

                if (documentLevelJS.Count > 0)
                {
                    PdfDictionary objecta = PdfNameTree.WriteTree(documentLevelJS, writer);
                    pdfDictionary.Put(PdfName.JAVASCRIPT, writer.AddToBody(objecta).IndirectReference);
                }

                if (documentFileAttachment.Count > 0)
                {
                    pdfDictionary.Put(PdfName.EMBEDDEDFILES, writer.AddToBody(PdfNameTree.WriteTree(documentFileAttachment, writer)).IndirectReference);
                }

                if (pdfDictionary.Size > 0)
                {
                    Put(PdfName.NAMES, writer.AddToBody(pdfDictionary).IndirectReference);
                }
            }
        }

        public class Indentation
        {
            internal float indentLeft;

            internal float sectionIndentLeft;

            internal float listIndentLeft;

            internal float imageIndentLeft;

            internal float indentRight;

            internal float sectionIndentRight;

            internal float imageIndentRight;

            internal float indentTop;

            internal float indentBottom;
        }

        public class Destination
        {
            public PdfAction action;

            public PdfIndirectReference reference;

            public PdfDestination destination;
        }

        protected internal PdfWriter writer;

        internal Dictionary<AccessibleElementId, PdfStructureElement> structElements = new Dictionary<AccessibleElementId, PdfStructureElement>();

        protected internal bool openMCDocument;

        protected Dictionary<object, int[]> structParentIndices = new Dictionary<object, int[]>();

        protected Dictionary<object, int> markPoints = new Dictionary<object, int>();

        protected internal PdfContentByte text;

        protected internal PdfContentByte graphics;

        protected internal float leading;

        protected internal float currentHeight;

        protected bool isSectionTitle;

        protected internal int alignment;

        protected internal PdfAction anchorAction;

        protected TabSettings tabSettings;

        private Stack<float> leadingStack = new Stack<float>();

        protected internal int textEmptySize;

        protected float nextMarginLeft;

        protected float nextMarginRight;

        protected float nextMarginTop;

        protected float nextMarginBottom;

        protected internal bool firstPageEvent = true;

        protected internal PdfLine line;

        protected internal List<PdfLine> lines = new List<PdfLine>();

        protected internal int lastElementType = -1;

        internal const string hangingPunctuation = ".,;:'";

        protected internal Indentation indentation = new Indentation();

        protected internal PdfInfo info = new PdfInfo();

        protected internal PdfOutline rootOutline;

        protected internal PdfOutline currentOutline;

        protected PdfViewerPreferencesImp viewerPreferences = new PdfViewerPreferencesImp();

        protected internal PdfPageLabels pageLabels;

        protected internal SortedDictionary<string, Destination> localDestinations = new SortedDictionary<string, Destination>(StringComparer.Ordinal);

        private int jsCounter;

        protected internal Dictionary<string, PdfObject> documentLevelJS = new Dictionary<string, PdfObject>();

        protected internal Dictionary<string, PdfObject> documentFileAttachment = new Dictionary<string, PdfObject>();

        protected internal string openActionName;

        protected internal PdfAction openActionAction;

        protected internal PdfDictionary additionalActions;

        protected internal PdfCollection collection;

        internal PdfAnnotationsImp annotationsImp;

        protected PdfString language;

        protected Rectangle nextPageSize;

        protected Dictionary<string, PdfRectangle> thisBoxSize = new Dictionary<string, PdfRectangle>();

        protected Dictionary<string, PdfRectangle> boxSize = new Dictionary<string, PdfRectangle>();

        private bool pageEmpty = true;

        protected PdfDictionary pageAA;

        protected internal PageResources pageResources;

        protected internal bool strictImageSequence;

        protected internal float imageEnd = -1f;

        protected internal Image imageWait;

        internal List<IElement> floatingElements = new List<IElement>();

        public virtual float Leading
        {
            get
            {
                return leading;
            }
            set
            {
                leading = value;
            }
        }

        public virtual TabSettings TabSettings
        {
            get
            {
                return tabSettings;
            }
            set
            {
                tabSettings = value;
            }
        }

        public virtual byte[] XmpMetadata
        {
            set
            {
                PdfStream pdfStream = new PdfStream(value);
                pdfStream.Put(PdfName.TYPE, PdfName.METADATA);
                pdfStream.Put(PdfName.SUBTYPE, PdfName.XML);
                PdfEncryption encryption = writer.Encryption;
                if (encryption != null && !encryption.IsMetadataEncrypted())
                {
                    PdfArray pdfArray = new PdfArray();
                    pdfArray.Add(PdfName.CRYPT);
                    pdfStream.Put(PdfName.FILTER, pdfArray);
                }

                writer.AddPageDictEntry(PdfName.METADATA, writer.AddToBody(pdfStream).IndirectReference);
            }
        }

        public override int PageCount
        {
            set
            {
                if (writer == null || !writer.IsPaused())
                {
                    base.PageCount = value;
                }
            }
        }

        protected internal virtual float IndentLeft => GetLeft(indentation.indentLeft + indentation.listIndentLeft + indentation.imageIndentLeft + indentation.sectionIndentLeft);

        protected internal virtual float IndentRight => GetRight(indentation.indentRight + indentation.sectionIndentRight + indentation.imageIndentRight);

        protected internal virtual float IndentTop => GetTop(indentation.indentTop);

        protected internal virtual float IndentBottom => GetBottom(indentation.indentBottom);

        internal PdfInfo Info => info;

        public virtual PdfOutline RootOutline => rootOutline;

        internal int ViewerPreferences
        {
            set
            {
                viewerPreferences.ViewerPreferences = value;
            }
        }

        public virtual PdfPageLabels PageLabels
        {
            get
            {
                return pageLabels;
            }
            internal set
            {
                pageLabels = value;
            }
        }

        public virtual PdfCollection Collection
        {
            set
            {
                collection = value;
            }
        }

        public virtual PdfAcroForm AcroForm => annotationsImp.AcroForm;

        internal int SigFlags
        {
            set
            {
                annotationsImp.SigFlags = value;
            }
        }

        internal Rectangle CropBoxSize
        {
            set
            {
                SetBoxSize("crop", value);
            }
        }

        internal bool PageEmpty
        {
            get
            {
                if (IsTagged(writer))
                {
                    if (writer != null)
                    {
                        if (writer.DirectContent.GetSize(includeMarkedContentSize: false) == 0 && writer.DirectContentUnder.GetSize(includeMarkedContentSize: false) == 0 && text.GetSize(includeMarkedContentSize: false) - textEmptySize == 0)
                        {
                            if (!pageEmpty)
                            {
                                return writer.IsPaused();
                            }

                            return true;
                        }

                        return false;
                    }

                    return true;
                }

                if (writer != null)
                {
                    if (writer.DirectContent.Size == 0 && writer.DirectContentUnder.Size == 0)
                    {
                        if (!pageEmpty)
                        {
                            return writer.IsPaused();
                        }

                        return true;
                    }

                    return false;
                }

                return true;
            }
            set
            {
                pageEmpty = value;
            }
        }

        internal int Duration
        {
            set
            {
                if (value > 0)
                {
                    writer.AddPageDictEntry(PdfName.DUR, new PdfNumber(value));
                }
            }
        }

        internal PdfTransition Transition
        {
            set
            {
                writer.AddPageDictEntry(PdfName.TRANS, value.TransitionDictionary);
            }
        }

        internal Image Thumbnail
        {
            set
            {
                writer.AddPageDictEntry(PdfName.THUMB, writer.GetImageReference(writer.AddDirectImageSimple(value)));
            }
        }

        internal PageResources PageResources => pageResources;

        internal bool StrictImageSequence
        {
            get
            {
                return strictImageSequence;
            }
            set
            {
                strictImageSequence = value;
            }
        }

        public PdfDocument()
        {
            AddProducer();
            AddCreationDate();
        }

        public virtual void AddWriter(PdfWriter writer)
        {
            if (this.writer == null)
            {
                this.writer = writer;
                annotationsImp = new PdfAnnotationsImp(writer);
                return;
            }

            throw new DocumentException(MessageLocalization.GetComposedMessage("you.can.only.add.a.writer.to.a.pdfdocument.once"));
        }

        protected virtual void PushLeading()
        {
            leadingStack.Push(leading);
        }

        protected virtual void PopLeading()
        {
            leading = leadingStack.Pop();
            if (leadingStack.Count > 0)
            {
                leading = leadingStack.Peek();
            }
        }

        public override bool Add(IElement element)
        {
            if (writer != null && writer.IsPaused())
            {
                return false;
            }

            try
            {
                if (element.Type != 37)
                {
                    FlushFloatingElements();
                }

                switch (element.Type)
                {
                    case 0:
                        info.Addkey(((Meta)element).Name, ((Meta)element).Content);
                        break;
                    case 1:
                        info.AddTitle(((Meta)element).Content);
                        break;
                    case 2:
                        info.AddSubject(((Meta)element).Content);
                        break;
                    case 3:
                        info.AddKeywords(((Meta)element).Content);
                        break;
                    case 4:
                        info.AddAuthor(((Meta)element).Content);
                        break;
                    case 7:
                        info.AddCreator(((Meta)element).Content);
                        break;
                    case 8:
                        SetLanguage(((Meta)element).Content);
                        break;
                    case 5:
                        info.AddProducer();
                        break;
                    case 6:
                        info.AddCreationDate();
                        break;
                    case 10:
                        {
                            if (line == null)
                            {
                                CarriageReturn();
                            }

                            PdfChunk pdfChunk = new PdfChunk((Chunk)element, anchorAction, this.tabSettings);
                            PdfChunk pdfChunk2;
                            while ((pdfChunk2 = line.Add(pdfChunk)) != null)
                            {
                                CarriageReturn();
                                bool num2 = pdfChunk.IsNewlineSplit();
                                pdfChunk = pdfChunk2;
                                if (!num2)
                                {
                                    pdfChunk.TrimFirstSpace();
                                }
                            }

                            pageEmpty = false;
                            if (pdfChunk.IsAttribute("NEWPAGE"))
                            {
                                NewPage();
                            }

                            break;
                        }
                    case 17:
                        {
                            Anchor anchor = (Anchor)element;
                            string reference = anchor.Reference;
                            leading = anchor.Leading;
                            PushLeading();
                            if (reference != null)
                            {
                                anchorAction = new PdfAction(reference);
                            }

                            element.Process(this);
                            anchorAction = null;
                            PopLeading();
                            break;
                        }
                    case 29:
                        {
                            if (line == null)
                            {
                                CarriageReturn();
                            }

                            Annotation annotation = (Annotation)element;
                            Rectangle defaultRect = new Rectangle(0f, 0f);
                            if (line != null)
                            {
                                defaultRect = new Rectangle(annotation.GetLlx(IndentRight - line.WidthLeft), annotation.GetUry(IndentTop - currentHeight - 20f), annotation.GetUrx(IndentRight - line.WidthLeft + 20f), annotation.GetLly(IndentTop - currentHeight));
                            }

                            PdfAnnotation annot = PdfAnnotationsImp.ConvertAnnotation(writer, annotation, defaultRect);
                            annotationsImp.AddPlainAnnotation(annot);
                            pageEmpty = false;
                            break;
                        }
                    case 11:
                        {
                            TabSettings tabSettings2 = this.tabSettings;
                            if (((Phrase)element).TabSettings != null)
                            {
                                this.tabSettings = ((Phrase)element).TabSettings;
                            }

                            leading = ((Phrase)element).TotalLeading;
                            PushLeading();
                            element.Process(this);
                            this.tabSettings = tabSettings2;
                            PopLeading();
                            break;
                        }
                    case 12:
                        {
                            TabSettings tabSettings = this.tabSettings;
                            if (((Phrase)element).TabSettings != null)
                            {
                                this.tabSettings = ((Phrase)element).TabSettings;
                            }

                            Paragraph paragraph = (Paragraph)element;
                            if (IsTagged(writer))
                            {
                                FlushLines();
                                text.OpenMCBlock(paragraph);
                            }

                            AddSpacing(paragraph.SpacingBefore, leading, paragraph.Font);
                            alignment = paragraph.Alignment;
                            leading = paragraph.TotalLeading;
                            PushLeading();
                            CarriageReturn();
                            if (currentHeight + CalculateLineHeight() > IndentTop - IndentBottom)
                            {
                                NewPage();
                            }

                            indentation.indentLeft += paragraph.IndentationLeft;
                            indentation.indentRight += paragraph.IndentationRight;
                            CarriageReturn();
                            IPdfPageEvent pageEvent = writer.PageEvent;
                            if (pageEvent != null && !isSectionTitle)
                            {
                                pageEvent.OnParagraph(writer, this, IndentTop - currentHeight);
                            }

                            if (paragraph.KeepTogether)
                            {
                                CarriageReturn();
                                PdfPTable pdfPTable = new PdfPTable(1);
                                pdfPTable.KeepTogether = paragraph.KeepTogether;
                                pdfPTable.WidthPercentage = 100f;
                                PdfPCell pdfPCell = new PdfPCell();
                                pdfPCell.AddElement(paragraph);
                                pdfPCell.Border = 0;
                                pdfPCell.Padding = 0f;
                                pdfPTable.AddCell(pdfPCell);
                                indentation.indentLeft -= paragraph.IndentationLeft;
                                indentation.indentRight -= paragraph.IndentationRight;
                                Add(pdfPTable);
                                indentation.indentLeft += paragraph.IndentationLeft;
                                indentation.indentRight += paragraph.IndentationRight;
                            }
                            else
                            {
                                line.SetExtraIndent(paragraph.FirstLineIndent);
                                element.Process(this);
                                CarriageReturn();
                                AddSpacing(paragraph.SpacingAfter, paragraph.TotalLeading, paragraph.Font, spacingAfter: true);
                            }

                            if (pageEvent != null && !isSectionTitle)
                            {
                                pageEvent.OnParagraphEnd(writer, this, IndentTop - currentHeight);
                            }

                            alignment = 0;
                            if (floatingElements != null && floatingElements.Count != 0)
                            {
                                FlushFloatingElements();
                            }

                            indentation.indentLeft -= paragraph.IndentationLeft;
                            indentation.indentRight -= paragraph.IndentationRight;
                            CarriageReturn();
                            this.tabSettings = tabSettings;
                            PopLeading();
                            if (IsTagged(writer))
                            {
                                FlushLines();
                                text.CloseMCBlock(paragraph);
                            }

                            break;
                        }
                    case 13:
                    case 16:
                        {
                            Section section = (Section)element;
                            IPdfPageEvent pageEvent2 = writer.PageEvent;
                            bool flag = section.NotAddedYet && section.Title != null;
                            if (section.TriggerNewPage)
                            {
                                NewPage();
                            }

                            if (flag)
                            {
                                float num = IndentTop - currentHeight;
                                int rotation = pageSize.Rotation;
                                if (rotation == 90 || rotation == 180)
                                {
                                    num = pageSize.Height - num;
                                }

                                PdfDestination destination = new PdfDestination(2, num);
                                while (currentOutline.Level >= section.Depth)
                                {
                                    currentOutline = currentOutline.Parent;
                                }

                                PdfOutline pdfOutline = (currentOutline = new PdfOutline(currentOutline, destination, section.GetBookmarkTitle(), section.BookmarkOpen));
                            }

                            CarriageReturn();
                            indentation.sectionIndentLeft += section.IndentationLeft;
                            indentation.sectionIndentRight += section.IndentationRight;
                            if (section.NotAddedYet && pageEvent2 != null)
                            {
                                if (element.Type == 16)
                                {
                                    pageEvent2.OnChapter(writer, this, IndentTop - currentHeight, section.Title);
                                }
                                else
                                {
                                    pageEvent2.OnSection(writer, this, IndentTop - currentHeight, section.Depth, section.Title);
                                }
                            }

                            if (flag)
                            {
                                isSectionTitle = true;
                                Add(section.Title);
                                isSectionTitle = false;
                            }

                            indentation.sectionIndentLeft += section.Indentation;
                            element.Process(this);
                            indentation.sectionIndentLeft -= section.IndentationLeft + section.Indentation;
                            indentation.sectionIndentRight -= section.IndentationRight;
                            if (section.ElementComplete && pageEvent2 != null)
                            {
                                if (element.Type == 16)
                                {
                                    pageEvent2.OnChapterEnd(writer, this, IndentTop - currentHeight);
                                }
                                else
                                {
                                    pageEvent2.OnSectionEnd(writer, this, IndentTop - currentHeight);
                                }
                            }

                            break;
                        }
                    case 14:
                        {
                            List list = (List)element;
                            if (IsTagged(writer))
                            {
                                FlushLines();
                                text.OpenMCBlock(list);
                            }

                            if (list.Alignindent)
                            {
                                list.NormalizeIndentation();
                            }

                            indentation.listIndentLeft += list.IndentationLeft;
                            indentation.indentRight += list.IndentationRight;
                            element.Process(this);
                            indentation.listIndentLeft -= list.IndentationLeft;
                            indentation.indentRight -= list.IndentationRight;
                            CarriageReturn();
                            if (IsTagged(writer))
                            {
                                FlushLines();
                                text.CloseMCBlock(list);
                            }

                            break;
                        }
                    case 15:
                        {
                            ListItem listItem = (ListItem)element;
                            if (IsTagged(writer))
                            {
                                FlushLines();
                                text.OpenMCBlock(listItem);
                            }

                            AddSpacing(listItem.SpacingBefore, leading, listItem.Font);
                            alignment = listItem.Alignment;
                            indentation.listIndentLeft += listItem.IndentationLeft;
                            indentation.indentRight += listItem.IndentationRight;
                            leading = listItem.TotalLeading;
                            PushLeading();
                            CarriageReturn();
                            line.ListItem = listItem;
                            element.Process(this);
                            AddSpacing(listItem.SpacingAfter, listItem.TotalLeading, listItem.Font, spacingAfter: true);
                            if (line.HasToBeJustified())
                            {
                                line.ResetAlignment();
                            }

                            CarriageReturn();
                            indentation.listIndentLeft -= listItem.IndentationLeft;
                            indentation.indentRight -= listItem.IndentationRight;
                            PopLeading();
                            if (IsTagged(writer))
                            {
                                FlushLines();
                                text.CloseMCBlock(listItem.ListBody);
                                text.CloseMCBlock(listItem);
                            }

                            break;
                        }
                    case 30:
                        {
                            Rectangle rectangle = (Rectangle)element;
                            graphics.Rectangle(rectangle);
                            pageEmpty = false;
                            break;
                        }
                    case 23:
                        {
                            PdfPTable pdfPTable2 = (PdfPTable)element;
                            if (pdfPTable2.Size > pdfPTable2.HeaderRows)
                            {
                                EnsureNewLine();
                                FlushLines();
                                AddPTable(pdfPTable2);
                                pageEmpty = false;
                                NewLine();
                            }

                            break;
                        }
                    case 32:
                    case 33:
                    case 34:
                    case 35:
                    case 36:
                        if (IsTagged(writer) && !((Image)element).IsImgTemplate())
                        {
                            FlushLines();
                            text.OpenMCBlock((Image)element);
                        }

                        Add((Image)element);
                        if (IsTagged(writer) && !((Image)element).IsImgTemplate())
                        {
                            FlushLines();
                            text.CloseMCBlock((Image)element);
                        }

                        break;
                    case 55:
                        ((IDrawInterface)element).Draw(graphics, IndentLeft, IndentBottom, IndentRight, IndentTop, IndentTop - currentHeight - ((leadingStack.Count > 0) ? leading : 0f));
                        pageEmpty = false;
                        break;
                    case 50:
                        {
                            if (element is MarkedSection)
                            {
                                ((MarkedSection)element).Title?.Process(this);
                            }

                            MarkedObject markedObject = (MarkedObject)element;
                            markedObject.Process(this);
                            break;
                        }
                    case 666:
                        if (writer != null)
                        {
                            ((IWriterOperation)element).Write(writer, this);
                        }

                        break;
                    case 37:
                        EnsureNewLine();
                        FlushLines();
                        AddDiv((PdfDiv)element);
                        pageEmpty = false;
                        break;
                    default:
                        return false;
                }

                lastElementType = element.Type;
                return true;
            }
            catch (Exception ex)
            {
                throw new DocumentException(ex.Message);
            }
        }

        public override void Open()
        {
            if (!open)
            {
                base.Open();
                writer.Open();
                rootOutline = new PdfOutline(writer);
                currentOutline = rootOutline;
            }

            InitPage();
            if (IsTagged(writer))
            {
                openMCDocument = true;
            }
        }

        public override void Close()
        {
            if (close)
            {
                return;
            }

            if (IsTagged(writer))
            {
                FlushFloatingElements();
                FlushLines();
                writer.DirectContent.CloseMCBlock(this);
                writer.FlushAcroFields();
                writer.FlushTaggedObjects();
                if (PageEmpty)
                {
                    int count = writer.pageReferences.Count;
                    if (count > 0 && writer.CurrentPageNumber == count)
                    {
                        writer.pageReferences.RemoveAt(count - 1);
                    }
                }
            }
            else
            {
                writer.FlushAcroFields();
            }

            bool flag = imageWait != null;
            NewPage();
            if (imageWait != null || flag)
            {
                NewPage();
            }

            if (annotationsImp.HasUnusedAnnotations())
            {
                throw new Exception(MessageLocalization.GetComposedMessage("not.all.annotations.could.be.added.to.the.document.the.document.doesn.t.have.enough.pages"));
            }

            writer.PageEvent?.OnCloseDocument(writer, this);
            base.Close();
            writer.AddLocalDestinations(localDestinations);
            CalculateOutlineCount();
            WriteOutlines();
            writer.Close();
        }

        public override bool NewPage()
        {
            FlushFloatingElements();
            lastElementType = -1;
            if (PageEmpty)
            {
                SetNewPageSizeAndMargins();
                return false;
            }

            if (!open || close)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("the.document.is.not.open"));
            }

            writer.PageEvent?.OnEndPage(writer, this);
            base.NewPage();
            indentation.imageIndentLeft = 0f;
            indentation.imageIndentRight = 0f;
            FlushLines();
            int rotation = pageSize.Rotation;
            if (writer.IsPdfIso())
            {
                if (thisBoxSize.ContainsKey("art") && thisBoxSize.ContainsKey("trim"))
                {
                    throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("only.one.of.artbox.or.trimbox.can.exist.in.the.page"));
                }

                if (!thisBoxSize.ContainsKey("art") && !thisBoxSize.ContainsKey("trim"))
                {
                    if (thisBoxSize.ContainsKey("crop"))
                    {
                        thisBoxSize["trim"] = thisBoxSize["crop"];
                    }
                    else
                    {
                        thisBoxSize["trim"] = new PdfRectangle(pageSize, pageSize.Rotation);
                    }
                }
            }

            pageResources.AddDefaultColorDiff(writer.DefaultColorspace);
            if (writer.RgbTransparencyBlending)
            {
                PdfDictionary pdfDictionary = new PdfDictionary();
                pdfDictionary.Put(PdfName.CS, PdfName.DEVICERGB);
                pageResources.AddDefaultColorDiff(pdfDictionary);
            }

            PdfDictionary resources = pageResources.Resources;
            PdfPage pdfPage = new PdfPage(new PdfRectangle(pageSize, rotation), thisBoxSize, resources, rotation);
            if (IsTagged(writer))
            {
                pdfPage.Put(PdfName.TABS, PdfName.S);
            }
            else
            {
                pdfPage.Put(PdfName.TABS, writer.Tabs);
            }

            pdfPage.Merge(writer.PageDictEntries);
            writer.ResetPageDictEntries();
            if (pageAA != null)
            {
                pdfPage.Put(PdfName.AA, writer.AddToBody(pageAA).IndirectReference);
                pageAA = null;
            }

            if (annotationsImp.HasUnusedAnnotations())
            {
                PdfArray pdfArray = annotationsImp.RotateAnnotations(writer, pageSize);
                if (pdfArray.Size != 0)
                {
                    pdfPage.Put(PdfName.ANNOTS, pdfArray);
                }
            }

            if (IsTagged(writer))
            {
                pdfPage.Put(PdfName.STRUCTPARENTS, new PdfNumber(GetStructParentIndex(writer.CurrentPage)));
            }

            if (text.Size > textEmptySize || IsTagged(writer))
            {
                text.EndText();
            }
            else
            {
                text = null;
            }

            IList<IAccessibleElement> mcElements = null;
            if (IsTagged(writer))
            {
                mcElements = writer.DirectContent.SaveMCBlocks();
            }

            writer.Add(pdfPage, new PdfContents(writer.DirectContentUnder, graphics, (!IsTagged(writer)) ? text : null, writer.DirectContent, pageSize));
            InitPage();
            if (IsTagged(writer))
            {
                writer.DirectContentUnder.RestoreMCBlocks(mcElements);
            }

            return true;
        }

        public override bool SetPageSize(Rectangle pageSize)
        {
            if (writer != null && writer.IsPaused())
            {
                return false;
            }

            nextPageSize = new Rectangle(pageSize);
            return true;
        }

        public override bool SetMargins(float marginLeft, float marginRight, float marginTop, float marginBottom)
        {
            if (writer != null && writer.IsPaused())
            {
                return false;
            }

            nextMarginLeft = marginLeft;
            nextMarginRight = marginRight;
            nextMarginTop = marginTop;
            nextMarginBottom = marginBottom;
            return true;
        }

        public override bool SetMarginMirroring(bool MarginMirroring)
        {
            if (writer != null && writer.IsPaused())
            {
                return false;
            }

            return base.SetMarginMirroring(MarginMirroring);
        }

        public override bool SetMarginMirroringTopBottom(bool MarginMirroringTopBottom)
        {
            if (writer != null && writer.IsPaused())
            {
                return false;
            }

            return base.SetMarginMirroringTopBottom(MarginMirroringTopBottom);
        }

        public override void ResetPageCount()
        {
            if (writer == null || !writer.IsPaused())
            {
                base.ResetPageCount();
            }
        }

        protected internal virtual void InitPage()
        {
            pageN++;
            annotationsImp.ResetAnnotations();
            pageResources = new PageResources();
            writer.ResetContent();
            if (IsTagged(writer))
            {
                graphics = writer.DirectContentUnder.Duplicate;
                writer.DirectContent.duplicatedFrom = graphics;
            }
            else
            {
                graphics = new PdfContentByte(writer);
            }

            SetNewPageSizeAndMargins();
            imageEnd = -1f;
            indentation.imageIndentRight = 0f;
            indentation.imageIndentLeft = 0f;
            indentation.indentBottom = 0f;
            indentation.indentTop = 0f;
            currentHeight = 0f;
            thisBoxSize = new Dictionary<string, PdfRectangle>(boxSize);
            if (pageSize.BackgroundColor != null || pageSize.HasBorders() || pageSize.BorderColor != null)
            {
                Add(pageSize);
            }

            float num = leading;
            int num2 = alignment;
            pageEmpty = true;
            if (imageWait != null)
            {
                Add(imageWait);
                imageWait = null;
            }

            leading = num;
            alignment = num2;
            CarriageReturn();
            IPdfPageEvent pageEvent = writer.PageEvent;
            if (pageEvent != null)
            {
                if (firstPageEvent)
                {
                    pageEvent.OnOpenDocument(writer, this);
                }

                pageEvent.OnStartPage(writer, this);
            }

            firstPageEvent = false;
        }

        protected internal virtual void NewLine()
        {
            lastElementType = -1;
            CarriageReturn();
            if (lines != null && lines.Count > 0)
            {
                lines.Add(line);
                currentHeight += line.Height;
            }

            line = new PdfLine(IndentLeft, IndentRight, alignment, leading);
        }

        protected virtual float CalculateLineHeight()
        {
            float num = line.Height;
            if (num != leading)
            {
                num += leading;
            }

            return num;
        }

        protected internal virtual void CarriageReturn()
        {
            if (lines == null)
            {
                lines = new List<PdfLine>();
            }

            if (line != null && line.Size > 0)
            {
                if (currentHeight + CalculateLineHeight() > IndentTop - IndentBottom && currentHeight != 0f)
                {
                    PdfLine pdfLine = line;
                    line = null;
                    NewPage();
                    line = pdfLine;
                    pdfLine.left = IndentLeft;
                }

                currentHeight += line.Height;
                lines.Add(line);
                pageEmpty = false;
            }

            if (imageEnd > -1f && currentHeight > imageEnd)
            {
                imageEnd = -1f;
                indentation.imageIndentRight = 0f;
                indentation.imageIndentLeft = 0f;
            }

            line = new PdfLine(IndentLeft, IndentRight, alignment, leading);
        }

        public virtual float GetVerticalPosition(bool ensureNewLine)
        {
            if (ensureNewLine)
            {
                EnsureNewLine();
            }

            return Top - currentHeight - indentation.indentTop;
        }

        protected internal virtual void EnsureNewLine()
        {
            if (lastElementType == 11 || lastElementType == 10)
            {
                NewLine();
                FlushLines();
            }
        }

        protected internal virtual float FlushLines()
        {
            if (lines == null)
            {
                return 0f;
            }

            if (line != null && line.Size > 0)
            {
                lines.Add(line);
                line = new PdfLine(IndentLeft, IndentRight, alignment, leading);
            }

            if (lines.Count == 0)
            {
                return 0f;
            }

            object[] array = new object[2];
            PdfFont pdfFont = null;
            float num = 0f;
            array[1] = 0f;
            foreach (PdfLine line in lines)
            {
                float num2 = line.IndentLeft - IndentLeft + indentation.indentLeft + indentation.listIndentLeft + indentation.sectionIndentLeft;
                text.MoveText(num2, 0f - line.Height);
                line.Flush();
                if (line.ListSymbol != null)
                {
                    ListLabel listLabel = null;
                    Chunk chunk = line.ListSymbol;
                    if (IsTagged(writer))
                    {
                        listLabel = line.listItem.ListLabel;
                        graphics.OpenMCBlock(listLabel);
                        chunk = new Chunk(chunk);
                        chunk.Role = null;
                    }

                    ColumnText.ShowTextAligned(graphics, 0, new Phrase(chunk), text.XTLM - line.ListIndent, text.YTLM, 0f);
                    if (listLabel != null)
                    {
                        graphics.CloseMCBlock(listLabel);
                    }
                }

                array[0] = pdfFont;
                if (IsTagged(writer) && line.ListItem != null)
                {
                    text.OpenMCBlock(line.listItem.ListBody);
                }

                WriteLineToContent(line, text, graphics, array, writer.SpaceCharRatio);
                pdfFont = (PdfFont)array[0];
                num += line.Height;
                text.MoveText(0f - num2, 0f);
            }

            lines = new List<PdfLine>();
            return num;
        }

        internal float WriteLineToContent(PdfLine line, PdfContentByte text, PdfContentByte graphics, object[] currentValues, float ratio)
        {
            PdfFont pdfFont = (PdfFont)currentValues[0];
            float num = (float)currentValues[1];
            float num2 = 0f;
            float num3 = 1f;
            float num4 = float.NaN;
            float num5 = 0f;
            float num6 = 0f;
            float num7 = 0f;
            float num8 = text.XTLM + line.OriginalWidth;
            int numberOfSpaces = line.NumberOfSpaces;
            int lineLengthUtf = line.GetLineLengthUtf32();
            bool flag = line.HasToBeJustified() && (numberOfSpaces != 0 || lineLengthUtf > 1);
            int separatorCount = line.GetSeparatorCount();
            if (separatorCount > 0)
            {
                num7 = line.WidthLeft / (float)separatorCount;
            }
            else if (flag && separatorCount == 0)
            {
                if (line.NewlineSplit && line.WidthLeft >= num * (ratio * (float)numberOfSpaces + (float)lineLengthUtf - 1f))
                {
                    if (line.RTL)
                    {
                        text.MoveText(line.WidthLeft - num * (ratio * (float)numberOfSpaces + (float)lineLengthUtf - 1f), 0f);
                    }

                    num5 = ratio * num;
                    num6 = num;
                }
                else
                {
                    float num9 = line.WidthLeft;
                    PdfChunk chunk = line.GetChunk(line.Size - 1);
                    if (chunk != null)
                    {
                        string text2 = chunk.ToString();
                        char character;
                        if (text2.Length > 0 && ".,;:'".IndexOf(character = text2[text2.Length - 1]) >= 0)
                        {
                            float num10 = num9;
                            num9 += chunk.Font.Width(character) * 0.4f;
                            num2 = num9 - num10;
                        }
                    }

                    float num11 = num9 / (ratio * (float)numberOfSpaces + (float)lineLengthUtf - 1f);
                    num5 = ratio * num11;
                    num6 = num11;
                    num = num11;
                }
            }
            else if (line.alignment == 0 || line.alignment == -1)
            {
                num8 -= line.WidthLeft;
            }

            int lastStrokeChunk = line.LastStrokeChunk;
            int num12 = 0;
            float num13 = text.XTLM;
            float num14 = num13;
            float yTLM = text.YTLM;
            bool flag2 = false;
            float num15 = 0f;
            foreach (PdfChunk item in line)
            {
                if (IsTagged(writer) && item.accessibleElement != null)
                {
                    text.OpenMCBlock(item.accessibleElement);
                }

                BaseColor color = item.Color;
                float size = item.Font.Size;
                float num16;
                float num17;
                if (item.IsImage())
                {
                    num16 = item.Height();
                    num17 = 0f;
                }
                else
                {
                    num16 = item.Font.Font.GetFontDescriptor(1, size);
                    num17 = item.Font.Font.GetFontDescriptor(3, size);
                }

                num3 = 1f;
                if (num12 <= lastStrokeChunk)
                {
                    float num18 = ((!flag) ? item.Width() : item.GetWidthCorrected(num6, num5));
                    if (item.IsStroked())
                    {
                        PdfChunk chunk2 = line.GetChunk(num12 + 1);
                        if (item.IsSeparator())
                        {
                            num18 = num7;
                            object[] obj = (object[])item.GetAttribute("SEPARATOR");
                            IDrawInterface drawInterface = (IDrawInterface)obj[0];
                            if ((bool)obj[1])
                            {
                                drawInterface.Draw(graphics, num14, yTLM + num17, num14 + line.OriginalWidth, num16 - num17, yTLM);
                            }
                            else
                            {
                                drawInterface.Draw(graphics, num13, yTLM + num17, num13 + num18, num16 - num17, yTLM);
                            }
                        }

                        if (item.IsTab())
                        {
                            if (item.IsAttribute("TABSETTINGS"))
                            {
                                TabStop tabStop = item.TabStop;
                                if (tabStop != null)
                                {
                                    num15 = tabStop.Position + num14;
                                    if (tabStop.Leader != null)
                                    {
                                        tabStop.Leader.Draw(graphics, num13, yTLM + num17, num15, num16 - num17, yTLM);
                                    }
                                }
                                else
                                {
                                    num15 = num13;
                                }
                            }
                            else
                            {
                                object[] array = (object[])item.GetAttribute("TAB");
                                IDrawInterface drawInterface2 = (IDrawInterface)array[0];
                                num15 = (float)array[1] + (float)array[3];
                                if (num15 > num13)
                                {
                                    drawInterface2.Draw(graphics, num13, yTLM + num17, num15, num16 - num17, yTLM);
                                }
                            }

                            float num19 = num13;
                            num13 = num15;
                            num15 = num19;
                        }

                        if (item.IsAttribute("BACKGROUND"))
                        {
                            bool inText = graphics.InText;
                            if (inText && IsTagged(writer))
                            {
                                graphics.EndText();
                            }

                            float num20 = num;
                            if (chunk2 != null && chunk2.IsAttribute("BACKGROUND"))
                            {
                                num20 = 0f;
                            }

                            if (chunk2 == null)
                            {
                                num20 += num2;
                            }

                            object[] array2 = (object[])item.GetAttribute("BACKGROUND");
                            graphics.SetColorFill((BaseColor)array2[0]);
                            float[] array3 = (float[])array2[1];
                            graphics.Rectangle(num13 - array3[0], yTLM + num17 - array3[1] + item.TextRise, num18 - num20 + array3[0] + array3[2], num16 - num17 + array3[1] + array3[3]);
                            graphics.Fill();
                            graphics.SetGrayFill(0f);
                            if (inText && IsTagged(writer))
                            {
                                graphics.BeginText(restoreTM: true);
                            }
                        }

                        if (item.IsAttribute("UNDERLINE") && !item.IsNewlineSplit())
                        {
                            bool inText2 = graphics.InText;
                            if (inText2 && IsTagged(writer))
                            {
                                graphics.EndText();
                            }

                            float num21 = num;
                            if (chunk2 != null && chunk2.IsAttribute("UNDERLINE"))
                            {
                                num21 = 0f;
                            }

                            if (chunk2 == null)
                            {
                                num21 += num2;
                            }

                            object[][] array4 = (object[][])item.GetAttribute("UNDERLINE");
                            BaseColor baseColor = null;
                            foreach (object[] obj2 in array4)
                            {
                                baseColor = (BaseColor)obj2[0];
                                float[] array5 = (float[])obj2[1];
                                if (baseColor == null)
                                {
                                    baseColor = color;
                                }

                                if (baseColor != null)
                                {
                                    graphics.SetColorStroke(baseColor);
                                }

                                graphics.SetLineWidth(array5[0] + size * array5[1]);
                                float num22 = array5[2] + size * array5[3];
                                int num23 = (int)array5[4];
                                if (num23 != 0)
                                {
                                    graphics.SetLineCap(num23);
                                }

                                graphics.MoveTo(num13, yTLM + num22);
                                graphics.LineTo(num13 + num18 - num21, yTLM + num22);
                                graphics.Stroke();
                                if (baseColor != null)
                                {
                                    graphics.ResetGrayStroke();
                                }

                                if (num23 != 0)
                                {
                                    graphics.SetLineCap(0);
                                }
                            }

                            graphics.SetLineWidth(1f);
                            if (inText2 && IsTagged(writer))
                            {
                                graphics.BeginText(restoreTM: true);
                            }
                        }

                        if (item.IsAttribute("ACTION"))
                        {
                            float num24 = num;
                            if (chunk2 != null && chunk2.IsAttribute("ACTION"))
                            {
                                num24 = 0f;
                            }

                            if (chunk2 == null)
                            {
                                num24 += num2;
                            }

                            PdfAnnotation pdfAnnotation = null;
                            pdfAnnotation = ((!item.IsImage()) ? writer.CreateAnnotation(num13, yTLM + num17 + item.TextRise, num13 + num18 - num24, yTLM + num16 + item.TextRise, (PdfAction)item.GetAttribute("ACTION"), null) : writer.CreateAnnotation(num13, yTLM + item.ImageOffsetY, num13 + num18 - num24, yTLM + item.ImageHeight + item.ImageOffsetY, (PdfAction)item.GetAttribute("ACTION"), null));
                            text.AddAnnotation(pdfAnnotation, applyCTM: true);
                            if (IsTagged(writer) && item.accessibleElement != null)
                            {
                                structElements.TryGetValue(item.accessibleElement.ID, out var value);
                                if (value != null)
                                {
                                    int structParentIndex = GetStructParentIndex(pdfAnnotation);
                                    pdfAnnotation.Put(PdfName.STRUCTPARENT, new PdfNumber(structParentIndex));
                                    value.SetAnnotation(pdfAnnotation, writer.CurrentPage);
                                    writer.StructureTreeRoot.SetAnnotationMark(structParentIndex, value.Reference);
                                }
                            }
                        }

                        if (item.IsAttribute("REMOTEGOTO"))
                        {
                            float num25 = num;
                            if (chunk2 != null && chunk2.IsAttribute("REMOTEGOTO"))
                            {
                                num25 = 0f;
                            }

                            if (chunk2 == null)
                            {
                                num25 += num2;
                            }

                            object[] array6 = (object[])item.GetAttribute("REMOTEGOTO");
                            string filename = (string)array6[0];
                            if (array6[1] is string)
                            {
                                RemoteGoto(filename, (string)array6[1], num13, yTLM + num17 + item.TextRise, num13 + num18 - num25, yTLM + num16 + item.TextRise);
                            }
                            else
                            {
                                RemoteGoto(filename, (int)array6[1], num13, yTLM + num17 + item.TextRise, num13 + num18 - num25, yTLM + num16 + item.TextRise);
                            }
                        }

                        if (item.IsAttribute("LOCALGOTO"))
                        {
                            float num26 = num;
                            if (chunk2 != null && chunk2.IsAttribute("LOCALGOTO"))
                            {
                                num26 = 0f;
                            }

                            if (chunk2 == null)
                            {
                                num26 += num2;
                            }

                            LocalGoto((string)item.GetAttribute("LOCALGOTO"), num13, yTLM, num13 + num18 - num26, yTLM + size);
                        }

                        if (item.IsAttribute("LOCALDESTINATION"))
                        {
                            LocalDestination((string)item.GetAttribute("LOCALDESTINATION"), new PdfDestination(0, num13, yTLM + size, 0f));
                        }

                        if (item.IsAttribute("GENERICTAG"))
                        {
                            float num27 = num;
                            if (chunk2 != null && chunk2.IsAttribute("GENERICTAG"))
                            {
                                num27 = 0f;
                            }

                            if (chunk2 == null)
                            {
                                num27 += num2;
                            }

                            Rectangle rect = new Rectangle(num13, yTLM, num13 + num18 - num27, yTLM + size);
                            writer.PageEvent?.OnGenericTag(writer, this, rect, (string)item.GetAttribute("GENERICTAG"));
                        }

                        if (item.IsAttribute("PDFANNOTATION"))
                        {
                            float num28 = num;
                            if (chunk2 != null && chunk2.IsAttribute("PDFANNOTATION"))
                            {
                                num28 = 0f;
                            }

                            if (chunk2 == null)
                            {
                                num28 += num2;
                            }

                            PdfAnnotation pdfAnnotation2 = PdfAnnotation.ShallowDuplicate((PdfAnnotation)item.GetAttribute("PDFANNOTATION"));
                            pdfAnnotation2.Put(PdfName.RECT, new PdfRectangle(num13, yTLM + num17, num13 + num18 - num28, yTLM + num16));
                            text.AddAnnotation(pdfAnnotation2, applyCTM: true);
                        }

                        float[] array7 = (float[])item.GetAttribute("SKEW");
                        object attribute = item.GetAttribute("HSCALE");
                        if (array7 != null || attribute != null)
                        {
                            float b = 0f;
                            float c = 0f;
                            if (array7 != null)
                            {
                                b = array7[0];
                                c = array7[1];
                            }

                            if (attribute != null)
                            {
                                num3 = (float)attribute;
                            }

                            text.SetTextMatrix(num3, b, c, 1f, num13, yTLM);
                        }

                        if (!flag)
                        {
                            if (item.IsAttribute("WORD_SPACING"))
                            {
                                float wordSpacing = (float)item.GetAttribute("WORD_SPACING");
                                text.SetWordSpacing(wordSpacing);
                            }

                            if (item.IsAttribute("CHAR_SPACING"))
                            {
                                text.SetCharacterSpacing(((float?)item.GetAttribute("CHAR_SPACING")).Value);
                            }
                        }

                        if (item.IsImage())
                        {
                            Image image = item.Image;
                            num18 = item.ImageWidth;
                            float[] matrix = image.GetMatrix(item.ImageScalePercentage);
                            matrix[4] = num13 + item.ImageOffsetX - matrix[4];
                            matrix[5] = yTLM + item.ImageOffsetY - matrix[5];
                            graphics.AddImage(image, matrix[0], matrix[1], matrix[2], matrix[3], matrix[4], matrix[5]);
                            text.MoveText(num13 + num + item.ImageWidth - text.XTLM, 0f);
                        }
                    }

                    num13 += num18;
                    num12++;
                }

                if (!item.IsImage() && item.Font.CompareTo(pdfFont) != 0)
                {
                    pdfFont = item.Font;
                    text.SetFontAndSize(pdfFont.Font, pdfFont.Size);
                }

                float num29 = 0f;
                object[] array8 = (object[])item.GetAttribute("TEXTRENDERMODE");
                int num30 = 0;
                float num31 = 1f;
                BaseColor baseColor2 = null;
                object attribute2 = item.GetAttribute("SUBSUPSCRIPT");
                if (array8 != null)
                {
                    num30 = (int)array8[0] & 3;
                    if (num30 != 0)
                    {
                        text.SetTextRenderingMode(num30);
                    }

                    if (num30 == 1 || num30 == 2)
                    {
                        num31 = (float)array8[1];
                        if (num31 != 1f)
                        {
                            text.SetLineWidth(num31);
                        }

                        baseColor2 = (BaseColor)array8[2];
                        if (baseColor2 == null)
                        {
                            baseColor2 = color;
                        }

                        if (baseColor2 != null)
                        {
                            text.SetColorStroke(baseColor2);
                        }
                    }
                }

                if (attribute2 != null)
                {
                    num29 = (float)attribute2;
                }

                if (color != null)
                {
                    text.SetColorFill(color);
                }

                if (num29 != 0f)
                {
                    text.SetTextRise(num29);
                }

                if (item.IsImage())
                {
                    flag2 = true;
                }
                else if (item.IsHorizontalSeparator())
                {
                    PdfTextArray pdfTextArray = new PdfTextArray();
                    pdfTextArray.Add((0f - num7) * 1000f / item.Font.Size / num3);
                    text.ShowText(pdfTextArray);
                }
                else if (item.IsTab() && num15 != num13)
                {
                    PdfTextArray pdfTextArray2 = new PdfTextArray();
                    pdfTextArray2.Add((num15 - num13) * 1000f / item.Font.Size / num3);
                    text.ShowText(pdfTextArray2);
                }
                else if (flag && numberOfSpaces > 0 && item.IsSpecialEncoding())
                {
                    if (num3 != num4)
                    {
                        num4 = num3;
                        text.SetWordSpacing(num5 / num3);
                        text.SetCharacterSpacing(num6 / num3 + text.CharacterSpacing);
                    }

                    string text3 = item.ToString();
                    int num32 = text3.IndexOf(' ');
                    if (num32 < 0)
                    {
                        text.ShowText(text3);
                    }
                    else
                    {
                        float number = (0f - num5) * 1000f / item.Font.Size / num3;
                        PdfTextArray pdfTextArray3 = new PdfTextArray(text3.Substring(0, num32));
                        int num33 = num32;
                        while ((num32 = text3.IndexOf(' ', num33 + 1)) >= 0)
                        {
                            pdfTextArray3.Add(number);
                            pdfTextArray3.Add(text3.Substring(num33, num32 - num33));
                            num33 = num32;
                        }

                        pdfTextArray3.Add(number);
                        pdfTextArray3.Add(text3.Substring(num33));
                        text.ShowText(pdfTextArray3);
                    }
                }
                else
                {
                    if (flag && num3 != num4)
                    {
                        num4 = num3;
                        text.SetWordSpacing(num5 / num3);
                        text.SetCharacterSpacing(num6 / num3 + text.CharacterSpacing);
                    }

                    text.ShowText(item.ToString());
                }

                if (num29 != 0f)
                {
                    text.SetTextRise(0f);
                }

                if (color != null)
                {
                    text.ResetRGBColorFill();
                }

                if (num30 != 0)
                {
                    text.SetTextRenderingMode(0);
                }

                if (baseColor2 != null)
                {
                    text.ResetRGBColorStroke();
                }

                if (num31 != 1f)
                {
                    text.SetLineWidth(1f);
                }

                if (item.IsAttribute("SKEW") || item.IsAttribute("HSCALE"))
                {
                    flag2 = true;
                    text.SetTextMatrix(num13, yTLM);
                }

                if (!flag)
                {
                    if (item.IsAttribute("CHAR_SPACING"))
                    {
                        text.SetCharacterSpacing(num6);
                    }

                    if (item.IsAttribute("WORD_SPACING"))
                    {
                        text.SetWordSpacing(num5);
                    }
                }

                if (IsTagged(writer) && item.accessibleElement != null)
                {
                    text.CloseMCBlock(item.accessibleElement);
                }
            }

            if (flag)
            {
                text.SetWordSpacing(0f);
                text.SetCharacterSpacing(0f);
                if (line.NewlineSplit)
                {
                    num = 0f;
                }
            }

            if (flag2)
            {
                text.MoveText(num14 - text.XTLM, 0f);
            }

            currentValues[0] = pdfFont;
            currentValues[1] = num;
            return num8;
        }

        protected internal virtual void AddSpacing(float extraspace, float oldleading, Font f)
        {
            AddSpacing(extraspace, oldleading, f, spacingAfter: false);
        }

        protected internal virtual void AddSpacing(float extraspace, float oldleading, Font f, bool spacingAfter)
        {
            if (extraspace == 0f || pageEmpty || (spacingAfter && !pageEmpty && lines.Count == 0 && line.Size == 0))
            {
                return;
            }

            float num = (spacingAfter ? extraspace : CalculateLineHeight());
            if (currentHeight + num > IndentTop - IndentBottom)
            {
                NewPage();
                return;
            }

            leading = extraspace;
            CarriageReturn();
            if (f.IsUnderlined() || f.IsStrikethru())
            {
                f = new Font(f);
                int style = f.Style;
                style &= -5;
                style &= -9;
                f.SetStyle(style);
            }

            Chunk chunk = new Chunk(" ", f);
            if (spacingAfter && pageEmpty)
            {
                chunk = new Chunk("", f);
            }

            chunk.Process(this);
            CarriageReturn();
            leading = oldleading;
        }

        internal PdfCatalog GetCatalog(PdfIndirectReference pages)
        {
            PdfCatalog pdfCatalog = new PdfCatalog(pages, writer);
            if (rootOutline.Kids.Count > 0)
            {
                pdfCatalog.Put(PdfName.PAGEMODE, PdfName.USEOUTLINES);
                pdfCatalog.Put(PdfName.OUTLINES, rootOutline.IndirectReference);
            }

            writer.GetPdfVersion().AddToCatalog(pdfCatalog);
            viewerPreferences.AddToCatalog(pdfCatalog);
            if (pageLabels != null)
            {
                pdfCatalog.Put(PdfName.PAGELABELS, pageLabels.GetDictionary(writer));
            }

            pdfCatalog.AddNames(localDestinations, GetDocumentLevelJS(), documentFileAttachment, writer);
            if (openActionName != null)
            {
                PdfAction pdfAction = (pdfCatalog.OpenAction = GetLocalGotoAction(openActionName));
            }
            else if (openActionAction != null)
            {
                pdfCatalog.OpenAction = openActionAction;
            }

            if (additionalActions != null)
            {
                pdfCatalog.AdditionalActions = additionalActions;
            }

            if (collection != null)
            {
                pdfCatalog.Put(PdfName.COLLECTION, collection);
            }

            if (annotationsImp.HasValidAcroForm())
            {
                pdfCatalog.Put(PdfName.ACROFORM, writer.AddToBody(annotationsImp.AcroForm).IndirectReference);
            }

            if (language != null)
            {
                pdfCatalog.Put(PdfName.LANG, language);
            }

            return pdfCatalog;
        }

        internal void AddOutline(PdfOutline outline, string name)
        {
            LocalDestination(name, outline.PdfDestination);
        }

        internal void CalculateOutlineCount()
        {
            if (rootOutline.Kids.Count != 0)
            {
                TraverseOutlineCount(rootOutline);
            }
        }

        internal void TraverseOutlineCount(PdfOutline outline)
        {
            List<PdfOutline> kids = outline.Kids;
            PdfOutline parent = outline.Parent;
            if (kids.Count == 0)
            {
                if (parent != null)
                {
                    parent.Count++;
                }

                return;
            }

            for (int i = 0; i < kids.Count; i++)
            {
                TraverseOutlineCount(kids[i]);
            }

            if (parent != null)
            {
                if (outline.Open)
                {
                    parent.Count = outline.Count + parent.Count + 1;
                    return;
                }

                parent.Count++;
                outline.Count = -outline.Count;
            }
        }

        internal void WriteOutlines()
        {
            if (rootOutline.Kids.Count != 0)
            {
                OutlineTree(rootOutline);
                writer.AddToBody(rootOutline, rootOutline.IndirectReference);
            }
        }

        internal void OutlineTree(PdfOutline outline)
        {
            outline.IndirectReference = writer.PdfIndirectReference;
            if (outline.Parent != null)
            {
                outline.Put(PdfName.PARENT, outline.Parent.IndirectReference);
            }

            List<PdfOutline> kids = outline.Kids;
            int count = kids.Count;
            for (int i = 0; i < count; i++)
            {
                OutlineTree(kids[i]);
            }

            for (int j = 0; j < count; j++)
            {
                if (j > 0)
                {
                    kids[j].Put(PdfName.PREV, kids[j - 1].IndirectReference);
                }

                if (j < count - 1)
                {
                    kids[j].Put(PdfName.NEXT, kids[j + 1].IndirectReference);
                }
            }

            if (count > 0)
            {
                outline.Put(PdfName.FIRST, kids[0].IndirectReference);
                outline.Put(PdfName.LAST, kids[count - 1].IndirectReference);
            }

            for (int k = 0; k < count; k++)
            {
                PdfOutline pdfOutline = kids[k];
                writer.AddToBody(pdfOutline, pdfOutline.IndirectReference);
            }
        }

        internal void AddViewerPreference(PdfName key, PdfObject value)
        {
            viewerPreferences.AddViewerPreference(key, value);
        }

        internal void LocalGoto(string name, float llx, float lly, float urx, float ury)
        {
            PdfAction localGotoAction = GetLocalGotoAction(name);
            annotationsImp.AddPlainAnnotation(writer.CreateAnnotation(llx, lly, urx, ury, localGotoAction, null));
        }

        internal void RemoteGoto(string filename, string name, float llx, float lly, float urx, float ury)
        {
            annotationsImp.AddPlainAnnotation(writer.CreateAnnotation(llx, lly, urx, ury, new PdfAction(filename, name), null));
        }

        internal void RemoteGoto(string filename, int page, float llx, float lly, float urx, float ury)
        {
            AddAnnotation(writer.CreateAnnotation(llx, lly, urx, ury, new PdfAction(filename, page), null));
        }

        internal void SetAction(PdfAction action, float llx, float lly, float urx, float ury)
        {
            AddAnnotation(writer.CreateAnnotation(llx, lly, urx, ury, action, null));
        }

        internal PdfAction GetLocalGotoAction(string name)
        {
            Destination destination = ((!localDestinations.ContainsKey(name)) ? new Destination() : localDestinations[name]);
            PdfAction result;
            if (destination.action == null)
            {
                if (destination.reference == null)
                {
                    destination.reference = writer.PdfIndirectReference;
                }

                result = (destination.action = new PdfAction(destination.reference));
                localDestinations[name] = destination;
            }
            else
            {
                result = destination.action;
            }

            return result;
        }

        internal bool LocalDestination(string name, PdfDestination destination)
        {
            Destination destination2 = ((!localDestinations.ContainsKey(name)) ? new Destination() : localDestinations[name]);
            if (destination2.destination != null)
            {
                return false;
            }

            destination2.destination = destination;
            localDestinations[name] = destination2;
            if (!destination.HasPage())
            {
                destination.AddPage(writer.CurrentPage);
            }

            return true;
        }

        internal void AddJavaScript(PdfAction js)
        {
            if (js.Get(PdfName.JS) == null)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("only.javascript.actions.are.allowed"));
            }

            documentLevelJS[jsCounter.ToString().PadLeft(16, '0')] = writer.AddToBody(js).IndirectReference;
            jsCounter++;
        }

        internal void AddJavaScript(string name, PdfAction js)
        {
            if (js.Get(PdfName.JS) == null)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("only.javascript.actions.are.allowed"));
            }

            documentLevelJS[name] = writer.AddToBody(js).IndirectReference;
        }

        internal Dictionary<string, PdfObject> GetDocumentLevelJS()
        {
            return documentLevelJS;
        }

        internal void AddFileAttachment(string description, PdfFileSpecification fs)
        {
            if (description == null)
            {
                PdfString pdfString = (PdfString)fs.Get(PdfName.DESC);
                description = ((pdfString != null) ? PdfEncodings.ConvertToString(pdfString.GetBytes(), null) : "");
            }

            fs.AddDescription(description, unicode: true);
            if (description.Length == 0)
            {
                description = "Unnamed";
            }

            string key = PdfEncodings.ConvertToString(new PdfString(description, "UnicodeBig").GetBytes(), null);
            int num = 0;
            while (documentFileAttachment.ContainsKey(key))
            {
                num++;
                key = PdfEncodings.ConvertToString(new PdfString(description + " " + num, "UnicodeBig").GetBytes(), null);
            }

            documentFileAttachment[key] = fs.Reference;
        }

        internal Dictionary<string, PdfObject> GetDocumentFileAttachment()
        {
            return documentFileAttachment;
        }

        internal void SetOpenAction(string name)
        {
            openActionName = name;
            openActionAction = null;
        }

        internal void SetOpenAction(PdfAction action)
        {
            openActionAction = action;
            openActionName = null;
        }

        internal void AddAdditionalAction(PdfName actionType, PdfAction action)
        {
            if (additionalActions == null)
            {
                additionalActions = new PdfDictionary();
            }

            if (action == null)
            {
                additionalActions.Remove(actionType);
            }
            else
            {
                additionalActions.Put(actionType, action);
            }

            if (additionalActions.Size == 0)
            {
                additionalActions = null;
            }
        }

        internal void AddCalculationOrder(PdfFormField formField)
        {
            annotationsImp.AddCalculationOrder(formField);
        }

        internal void AddAnnotation(PdfAnnotation annot)
        {
            pageEmpty = false;
            annotationsImp.AddAnnotation(annot);
        }

        internal void SetLanguage(string language)
        {
            this.language = new PdfString(language);
        }

        internal void SetBoxSize(string boxName, Rectangle size)
        {
            if (size == null)
            {
                boxSize.Remove(boxName);
            }
            else
            {
                boxSize[boxName] = new PdfRectangle(size);
            }
        }

        protected internal virtual void SetNewPageSizeAndMargins()
        {
            pageSize = nextPageSize;
            if (marginMirroring && (PageNumber & 1) == 0)
            {
                marginRight = nextMarginLeft;
                marginLeft = nextMarginRight;
            }
            else
            {
                marginLeft = nextMarginLeft;
                marginRight = nextMarginRight;
            }

            if (marginMirroringTopBottom && (PageNumber & 1) == 0)
            {
                marginTop = nextMarginBottom;
                marginBottom = nextMarginTop;
            }
            else
            {
                marginTop = nextMarginTop;
                marginBottom = nextMarginBottom;
            }

            if (!IsTagged(writer))
            {
                text = new PdfContentByte(writer);
                text.Reset();
            }
            else
            {
                text = graphics;
            }

            text.BeginText();
            text.MoveText(Left, Top);
            if (IsTagged(writer))
            {
                textEmptySize = text.Size;
            }
        }

        internal Rectangle GetBoxSize(string boxName)
        {
            thisBoxSize.TryGetValue(boxName, out var value);
            return value?.Rectangle;
        }

        internal void SetPageAction(PdfName actionType, PdfAction action)
        {
            if (pageAA == null)
            {
                pageAA = new PdfDictionary();
            }

            pageAA.Put(actionType, action);
        }

        public virtual void ClearTextWrap()
        {
            float num = imageEnd - currentHeight;
            if (line != null)
            {
                num += line.Height;
            }

            if (imageEnd > -1f && num > 0f)
            {
                CarriageReturn();
                currentHeight += num;
            }
        }

        public virtual int GetStructParentIndex(object obj)
        {
            structParentIndices.TryGetValue(obj, out var value);
            if (value == null)
            {
                value = new int[2] { structParentIndices.Count, 0 };
                structParentIndices[obj] = value;
            }

            return value[0];
        }

        public virtual int GetNextMarkPoint(object obj)
        {
            structParentIndices.TryGetValue(obj, out var value);
            if (value == null)
            {
                value = new int[2] { structParentIndices.Count, 0 };
                structParentIndices[obj] = value;
            }

            int result = value[1];
            value[1]++;
            return result;
        }

        public virtual int[] GetStructParentIndexAndNextMarkPoint(object obj)
        {
            structParentIndices.TryGetValue(obj, out var value);
            if (value == null)
            {
                value = new int[2] { structParentIndices.Count, 0 };
                structParentIndices[obj] = value;
            }

            int num = value[1];
            value[1]++;
            return new int[2]
            {
                value[0],
                num
            };
        }

        protected internal virtual void Add(Image image)
        {
            if (image.HasAbsolutePosition())
            {
                graphics.AddImage(image);
                pageEmpty = false;
                return;
            }

            if (currentHeight != 0f && IndentTop - currentHeight - image.ScaledHeight < IndentBottom)
            {
                if (!strictImageSequence && imageWait == null)
                {
                    imageWait = image;
                    return;
                }

                NewPage();
                if (currentHeight != 0f && IndentTop - currentHeight - image.ScaledHeight < IndentBottom)
                {
                    imageWait = image;
                    return;
                }
            }

            pageEmpty = false;
            if (image == imageWait)
            {
                imageWait = null;
            }

            bool num = (image.Alignment & 4) == 4 && (image.Alignment & 1) != 1;
            bool flag = (image.Alignment & 8) == 8;
            float num2 = leading / 2f;
            if (num)
            {
                num2 += leading;
            }

            float num3 = IndentTop - currentHeight - image.ScaledHeight - num2;
            float[] matrix = image.GetMatrix();
            float num4 = IndentLeft - matrix[4];
            if ((image.Alignment & 2) == 2)
            {
                num4 = IndentRight - image.ScaledWidth - matrix[4];
            }

            if ((image.Alignment & 1) == 1)
            {
                num4 = IndentLeft + (IndentRight - IndentLeft - image.ScaledWidth) / 2f - matrix[4];
            }

            if (image.HasAbsoluteX())
            {
                num4 = image.AbsoluteX;
            }

            if (!num)
            {
                num4 = (((image.Alignment & 2) == 2) ? (num4 - image.IndentationRight) : (((image.Alignment & 1) != 1) ? (num4 - image.IndentationRight) : (num4 + (image.IndentationLeft - image.IndentationRight))));
            }
            else
            {
                if (imageEnd < 0f || imageEnd < currentHeight + image.ScaledHeight + num2)
                {
                    imageEnd = currentHeight + image.ScaledHeight + num2;
                }

                if ((image.Alignment & 2) == 2)
                {
                    indentation.imageIndentRight += image.ScaledWidth + image.IndentationLeft;
                }
                else
                {
                    indentation.imageIndentLeft += image.ScaledWidth + image.IndentationRight;
                }
            }

            graphics.AddImage(image, matrix[0], matrix[1], matrix[2], matrix[3], num4, num3 - matrix[5]);
            if (!(num || flag))
            {
                currentHeight += image.ScaledHeight + num2;
                FlushLines();
                text.MoveText(0f, 0f - (image.ScaledHeight + num2));
                NewLine();
            }
        }

        internal void AddPTable(PdfPTable ptable)
        {
            ColumnText columnText = new ColumnText(writer.DirectContent);
            columnText.RunDirection = ptable.RunDirection;
            if (ptable.KeepTogether && !FitsPage(ptable, 0f) && currentHeight > 0f)
            {
                NewPage();
            }

            if (currentHeight == 0f)
            {
                columnText.AdjustFirstLine = false;
            }

            columnText.AddElement(ptable);
            bool headersInEvent = ptable.HeadersInEvent;
            ptable.HeadersInEvent = true;
            int num = 0;
            while (true)
            {
                columnText.SetSimpleColumn(IndentLeft, IndentBottom, IndentRight, IndentTop - currentHeight);
                if (((uint)columnText.Go() & (true ? 1u : 0u)) != 0)
                {
                    if (IsTagged(writer))
                    {
                        text.SetTextMatrix(IndentLeft, columnText.YLine);
                    }
                    else
                    {
                        text.MoveText(0f, columnText.YLine - IndentTop + currentHeight);
                    }

                    currentHeight = IndentTop - columnText.YLine;
                    ptable.HeadersInEvent = headersInEvent;
                    return;
                }

                num = ((IndentTop - currentHeight == columnText.YLine) ? (num + 1) : 0);
                if (num == 3)
                {
                    break;
                }

                NewPage();
                if (IsTagged(writer))
                {
                    columnText.Canvas = text;
                }
            }

            throw new DocumentException(MessageLocalization.GetComposedMessage("infinite.table.loop"));
        }

        internal void AddDiv(PdfDiv div)
        {
            if (floatingElements == null)
            {
                floatingElements = new List<IElement>();
            }

            floatingElements.Add(div);
        }

        internal void FlushFloatingElements()
        {
            if (floatingElements == null || floatingElements.Count <= 0)
            {
                return;
            }

            List<IElement> elements = floatingElements;
            floatingElements = null;
            FloatLayout floatLayout = new FloatLayout(elements, useAscender: false);
            int num = 0;
            while (true)
            {
                _ = IndentLeft;
                floatLayout.SetSimpleColumn(IndentLeft, IndentBottom, IndentRight, IndentTop - currentHeight);
                try
                {
                    if (((uint)floatLayout.Layout(IsTagged(writer) ? text : writer.DirectContent, simulate: false) & (true ? 1u : 0u)) != 0)
                    {
                        if (IsTagged(writer))
                        {
                            text.SetTextMatrix(IndentLeft, floatLayout.YLine);
                        }
                        else
                        {
                            text.MoveText(0f, floatLayout.YLine - IndentTop + currentHeight);
                        }

                        currentHeight = IndentTop - floatLayout.YLine;
                        return;
                    }
                }
                catch (Exception)
                {
                    return;
                }

                num = ((IndentTop - currentHeight == floatLayout.YLine || PageEmpty) ? (num + 1) : 0);
                if (num == 2)
                {
                    break;
                }

                NewPage();
            }
        }

        internal bool FitsPage(PdfPTable table, float margin)
        {
            if (!table.LockedWidth)
            {
                float num2 = (table.TotalWidth = (IndentRight - IndentLeft) * table.WidthPercentage / 100f);
            }

            EnsureNewLine();
            return (table.SkipFirstHeader ? (table.TotalHeight - table.HeaderHeight) : table.TotalHeight) + ((currentHeight > 0f) ? table.SpacingBefore : 0f) <= IndentTop - currentHeight - IndentBottom - margin;
        }

        private static bool IsTagged(PdfWriter writer)
        {
            return writer?.IsTagged() ?? false;
        }

        private PdfLine GetLastLine()
        {
            if (lines.Count > 0)
            {
                return lines[lines.Count - 1];
            }

            return null;
        }
    }
}
