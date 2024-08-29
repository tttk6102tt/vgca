using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.pdf.draw;
using Sign.itext.text.log;

namespace Sign.itext.text.pdf
{
    public class ColumnText
    {
        private readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(PdfPTable));

        public int AR_NOVOWEL = 1;

        public const int AR_COMPOSEDTASHKEEL = 4;

        public const int AR_LIG = 8;

        public const int DIGITS_EN2AN = 32;

        public const int DIGITS_AN2EN = 64;

        public const int DIGITS_EN2AN_INIT_LR = 96;

        public const int DIGITS_EN2AN_INIT_AL = 128;

        public const int DIGIT_TYPE_AN = 0;

        public const int DIGIT_TYPE_AN_EXTENDED = 256;

        protected int runDirection;

        public static float GLOBAL_SPACE_CHAR_RATIO;

        public const int NO_MORE_TEXT = 1;

        public const int NO_MORE_COLUMN = 2;

        protected const int LINE_STATUS_OK = 0;

        protected const int LINE_STATUS_OFFLIMITS = 1;

        protected const int LINE_STATUS_NOLINE = 2;

        protected float maxY;

        protected float minY;

        protected float leftX;

        protected float rightX;

        protected int alignment;

        protected List<float[]> leftWall;

        protected List<float[]> rightWall;

        protected BidiLine bidiLine;

        protected bool isWordSplit;

        protected float yLine;

        protected float lastX;

        protected float currentLeading = 16f;

        protected float fixedLeading = 16f;

        protected float multipliedLeading;

        protected PdfContentByte canvas;

        protected PdfContentByte[] canvases;

        protected int lineStatus;

        protected float indent;

        protected float followingIndent;

        protected float rightIndent;

        protected float extraParagraphSpace;

        protected float rectangularWidth = -1f;

        protected bool rectangularMode;

        private float spaceCharRatio = GLOBAL_SPACE_CHAR_RATIO;

        private bool lastWasNewline = true;

        private bool repeatFirstLineIndent = true;

        private int linesWritten;

        private float firstLineY;

        private bool firstLineYDone;

        private int arabicOptions;

        protected float descender;

        protected bool composite;

        protected ColumnText compositeColumn;

        protected internal List<IElement> compositeElements;

        protected int listIdx;

        protected int rowIdx;

        private int splittedRow = -1;

        protected Phrase waitPhrase;

        private bool useAscender;

        private bool inheritGraphicState;

        private float filledWidth;

        private bool adjustFirstLine = true;

        public virtual float Leading
        {
            get
            {
                return fixedLeading;
            }
            set
            {
                fixedLeading = value;
                multipliedLeading = 0f;
            }
        }

        public virtual float MultipliedLeading => multipliedLeading;

        public virtual float YLine
        {
            get
            {
                return yLine;
            }
            set
            {
                yLine = value;
            }
        }

        public virtual int RowsDrawn => rowIdx;

        public virtual int Alignment
        {
            get
            {
                return alignment;
            }
            set
            {
                alignment = value;
            }
        }

        public virtual float Indent
        {
            get
            {
                return indent;
            }
            set
            {
                SetIndent(value, repeatFirstLineIndent: true);
            }
        }

        public virtual float FollowingIndent
        {
            get
            {
                return followingIndent;
            }
            set
            {
                followingIndent = value;
                lastWasNewline = true;
            }
        }

        public virtual float RightIndent
        {
            get
            {
                return rightIndent;
            }
            set
            {
                rightIndent = value;
                lastWasNewline = true;
            }
        }

        public virtual float CurrentLeading => currentLeading;

        public virtual bool InheritGraphicState
        {
            get
            {
                return inheritGraphicState;
            }
            set
            {
                inheritGraphicState = value;
            }
        }

        public virtual float ExtraParagraphSpace
        {
            get
            {
                return extraParagraphSpace;
            }
            set
            {
                extraParagraphSpace = value;
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
                spaceCharRatio = value;
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
                if (value < 0 || value > 3)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("invalid.run.direction.1", value));
                }

                runDirection = value;
            }
        }

        public virtual int LinesWritten => linesWritten;

        public virtual float LastX => lastX;

        public virtual int ArabicOptions
        {
            get
            {
                return arabicOptions;
            }
            set
            {
                arabicOptions = value;
            }
        }

        public virtual float Descender => descender;

        public virtual PdfContentByte Canvas
        {
            get
            {
                return canvas;
            }
            set
            {
                canvas = value;
                canvases = null;
                if (compositeColumn != null)
                {
                    compositeColumn.Canvas = value;
                }
            }
        }

        public virtual PdfContentByte[] Canvases
        {
            get
            {
                return canvases;
            }
            set
            {
                canvases = value;
                canvas = canvases[3];
                if (compositeColumn != null)
                {
                    compositeColumn.Canvases = canvases;
                }
            }
        }

        public virtual IList<IElement> CompositeElements => compositeElements;

        public virtual bool UseAscender
        {
            get
            {
                return useAscender;
            }
            set
            {
                useAscender = value;
            }
        }

        public virtual float FilledWidth
        {
            get
            {
                return filledWidth;
            }
            set
            {
                filledWidth = value;
            }
        }

        public virtual bool AdjustFirstLine
        {
            get
            {
                return adjustFirstLine;
            }
            set
            {
                adjustFirstLine = value;
            }
        }

        public ColumnText(PdfContentByte canvas)
        {
            this.canvas = canvas;
        }

        public static ColumnText Duplicate(ColumnText org)
        {
            ColumnText columnText = new ColumnText(null);
            columnText.SetACopy(org);
            return columnText;
        }

        public virtual ColumnText SetACopy(ColumnText org)
        {
            SetSimpleVars(org);
            if (org.bidiLine != null)
            {
                bidiLine = new BidiLine(org.bidiLine);
            }

            return this;
        }

        protected internal virtual void SetSimpleVars(ColumnText org)
        {
            maxY = org.maxY;
            minY = org.minY;
            alignment = org.alignment;
            leftWall = null;
            if (org.leftWall != null)
            {
                leftWall = new List<float[]>(org.leftWall);
            }

            rightWall = null;
            if (org.rightWall != null)
            {
                rightWall = new List<float[]>(org.rightWall);
            }

            yLine = org.yLine;
            currentLeading = org.currentLeading;
            fixedLeading = org.fixedLeading;
            multipliedLeading = org.multipliedLeading;
            canvas = org.canvas;
            canvases = org.canvases;
            lineStatus = org.lineStatus;
            indent = org.indent;
            followingIndent = org.followingIndent;
            rightIndent = org.rightIndent;
            extraParagraphSpace = org.extraParagraphSpace;
            rectangularWidth = org.rectangularWidth;
            rectangularMode = org.rectangularMode;
            spaceCharRatio = org.spaceCharRatio;
            lastWasNewline = org.lastWasNewline;
            repeatFirstLineIndent = org.repeatFirstLineIndent;
            linesWritten = org.linesWritten;
            arabicOptions = org.arabicOptions;
            runDirection = org.runDirection;
            descender = org.descender;
            composite = org.composite;
            splittedRow = org.splittedRow;
            if (org.composite)
            {
                compositeElements = new List<IElement>();
                foreach (IElement compositeElement in org.compositeElements)
                {
                    if (compositeElement is PdfPTable)
                    {
                        compositeElements.Add(new PdfPTable((PdfPTable)compositeElement));
                    }
                    else
                    {
                        compositeElements.Add(compositeElement);
                    }
                }

                if (org.compositeColumn != null)
                {
                    compositeColumn = Duplicate(org.compositeColumn);
                }
            }

            listIdx = org.listIdx;
            rowIdx = org.rowIdx;
            firstLineY = org.firstLineY;
            leftX = org.leftX;
            rightX = org.rightX;
            firstLineYDone = org.firstLineYDone;
            waitPhrase = org.waitPhrase;
            useAscender = org.useAscender;
            filledWidth = org.filledWidth;
            adjustFirstLine = org.adjustFirstLine;
            inheritGraphicState = org.inheritGraphicState;
        }

        private void AddWaitingPhrase()
        {
            if (bidiLine != null || waitPhrase == null)
            {
                return;
            }

            bidiLine = new BidiLine();
            foreach (Chunk chunk in waitPhrase.Chunks)
            {
                bidiLine.AddChunk(new PdfChunk(chunk, null, waitPhrase.TabSettings));
            }

            waitPhrase = null;
        }

        public virtual void AddText(Phrase phrase)
        {
            if (phrase == null || composite)
            {
                return;
            }

            AddWaitingPhrase();
            if (bidiLine == null)
            {
                waitPhrase = phrase;
                return;
            }

            foreach (Chunk chunk in phrase.Chunks)
            {
                bidiLine.AddChunk(new PdfChunk(chunk, null, phrase.TabSettings));
            }
        }

        public virtual void SetText(Phrase phrase)
        {
            bidiLine = null;
            composite = false;
            compositeColumn = null;
            compositeElements = null;
            listIdx = 0;
            rowIdx = 0;
            splittedRow = -1;
            waitPhrase = phrase;
        }

        public virtual void AddText(Chunk chunk)
        {
            if (chunk != null && !composite)
            {
                AddText(new Phrase(chunk));
            }
        }

        public virtual void AddElement(IElement element)
        {
            if (element == null)
            {
                return;
            }

            if (element is Image)
            {
                Image image = (Image)element;
                PdfPTable pdfPTable = new PdfPTable(1);
                float widthPercentage = image.WidthPercentage;
                if (widthPercentage == 0f)
                {
                    pdfPTable.TotalWidth = image.ScaledWidth;
                    pdfPTable.LockedWidth = true;
                }
                else
                {
                    pdfPTable.WidthPercentage = widthPercentage;
                }

                pdfPTable.SpacingAfter = image.SpacingAfter;
                pdfPTable.SpacingBefore = image.SpacingBefore;
                switch (image.Alignment)
                {
                    case 0:
                        pdfPTable.HorizontalAlignment = 0;
                        break;
                    case 2:
                        pdfPTable.HorizontalAlignment = 2;
                        break;
                    default:
                        pdfPTable.HorizontalAlignment = 1;
                        break;
                }

                PdfPCell pdfPCell = new PdfPCell(image, fit: true);
                pdfPCell.Padding = 0f;
                pdfPCell.Border = image.Border;
                pdfPCell.BorderColor = image.BorderColor;
                pdfPCell.BorderWidth = image.BorderWidth;
                pdfPCell.BackgroundColor = image.BackgroundColor;
                pdfPTable.AddCell(pdfPCell);
                element = pdfPTable;
            }

            if (element.Type == 10)
            {
                element = new Paragraph((Chunk)element);
            }
            else if (element.Type == 11)
            {
                element = new Paragraph((Phrase)element);
            }
            else if (element.Type == 23)
            {
                ((PdfPTable)element).Init();
            }

            if (element.Type != 12 && element.Type != 14 && element.Type != 23 && element.Type != 55 && element.Type != 37)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("element.not.allowed"));
            }

            if (!composite)
            {
                composite = true;
                compositeElements = new List<IElement>();
                bidiLine = null;
                waitPhrase = null;
            }

            if (element.Type == 12)
            {
                foreach (IElement item in ((Paragraph)element).BreakUp())
                {
                    compositeElements.Add(item);
                }
            }
            else
            {
                compositeElements.Add(element);
            }
        }

        public static bool isAllowedElement(IElement element)
        {
            int type = element.Type;
            if (type == 10 || type == 11 || type == 12 || type == 14 || type == 55 || type == 23 || type == 37)
            {
                return true;
            }

            if (element is Image)
            {
                return true;
            }

            return false;
        }

        protected virtual List<float[]> ConvertColumn(float[] cLine)
        {
            if (cLine.Length < 4)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("no.valid.column.line.found"));
            }

            List<float[]> list = new List<float[]>();
            for (int i = 0; i < cLine.Length - 2; i += 2)
            {
                float num = cLine[i];
                float num2 = cLine[i + 1];
                float num3 = cLine[i + 2];
                float num4 = cLine[i + 3];
                if (num2 != num4)
                {
                    float num5 = (num - num3) / (num2 - num4);
                    float num6 = num - num5 * num2;
                    float[] array = new float[4]
                    {
                        Math.Min(num2, num4),
                        Math.Max(num2, num4),
                        num5,
                        num6
                    };
                    list.Add(array);
                    maxY = Math.Max(maxY, array[1]);
                    minY = Math.Min(minY, array[0]);
                }
            }

            if (list.Count == 0)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("no.valid.column.line.found"));
            }

            return list;
        }

        protected virtual float FindLimitsPoint(List<float[]> wall)
        {
            lineStatus = 0;
            if (yLine < minY || yLine > maxY)
            {
                lineStatus = 1;
                return 0f;
            }

            for (int i = 0; i < wall.Count; i++)
            {
                float[] array = wall[i];
                if (!(yLine < array[0]) && !(yLine > array[1]))
                {
                    return array[2] * yLine + array[3];
                }
            }

            lineStatus = 2;
            return 0f;
        }

        protected virtual float[] FindLimitsOneLine()
        {
            float num = FindLimitsPoint(leftWall);
            if (lineStatus == 1 || lineStatus == 2)
            {
                return null;
            }

            float num2 = FindLimitsPoint(rightWall);
            if (lineStatus == 2)
            {
                return null;
            }

            return new float[2] { num, num2 };
        }

        protected virtual float[] FindLimitsTwoLines()
        {
            bool flag = false;
            float[] array;
            float[] array2;
            while (true)
            {
                if (flag && currentLeading == 0f)
                {
                    return null;
                }

                flag = true;
                array = FindLimitsOneLine();
                if (lineStatus == 1)
                {
                    return null;
                }

                yLine -= currentLeading;
                if (lineStatus != 2)
                {
                    array2 = FindLimitsOneLine();
                    if (lineStatus == 1)
                    {
                        return null;
                    }

                    if (lineStatus == 2)
                    {
                        yLine -= currentLeading;
                    }
                    else if (!(array[0] >= array2[1]) && !(array2[0] >= array[1]))
                    {
                        break;
                    }
                }
            }

            return new float[4]
            {
                array[0],
                array[1],
                array2[0],
                array2[1]
            };
        }

        public virtual void SetColumns(float[] leftLine, float[] rightLine)
        {
            maxY = -1E+21f;
            minY = 1E+21f;
            YLine = Math.Max(leftLine[1], leftLine[^1]);
            rightWall = ConvertColumn(rightLine);
            leftWall = ConvertColumn(leftLine);
            rectangularWidth = -1f;
            rectangularMode = false;
        }

        public virtual void SetSimpleColumn(Phrase phrase, float llx, float lly, float urx, float ury, float leading, int alignment)
        {
            AddText(phrase);
            SetSimpleColumn(llx, lly, urx, ury, leading, alignment);
        }

        public virtual void SetSimpleColumn(float llx, float lly, float urx, float ury, float leading, int alignment)
        {
            Leading = leading;
            this.alignment = alignment;
            SetSimpleColumn(llx, lly, urx, ury);
        }

        public virtual void SetSimpleColumn(float llx, float lly, float urx, float ury)
        {
            leftX = Math.Min(llx, urx);
            maxY = Math.Max(lly, ury);
            minY = Math.Min(lly, ury);
            rightX = Math.Max(llx, urx);
            yLine = maxY;
            rectangularWidth = rightX - leftX;
            if (rectangularWidth < 0f)
            {
                rectangularWidth = 0f;
            }

            rectangularMode = true;
        }

        public virtual void SetSimpleColumn(Rectangle rect)
        {
            SetSimpleColumn(rect.Left, rect.Bottom, rect.Right, rect.Top);
        }

        public virtual void SetLeading(float fixedLeading, float multipliedLeading)
        {
            this.fixedLeading = fixedLeading;
            this.multipliedLeading = multipliedLeading;
        }

        public virtual void SetIndent(float indent, bool repeatFirstLineIndent)
        {
            this.indent = indent;
            lastWasNewline = true;
            this.repeatFirstLineIndent = repeatFirstLineIndent;
        }

        public virtual int Go()
        {
            return Go(simulate: false);
        }

        public virtual int Go(bool simulate)
        {
            return Go(simulate, null);
        }

        public virtual int Go(bool simulate, IElement elementToGo)
        {
            isWordSplit = false;
            if (composite)
            {
                return GoComposite(simulate);
            }

            ListBody listBody = null;
            if (IsTagged(canvas) && elementToGo is ListItem)
            {
                listBody = ((ListItem)elementToGo).ListBody;
            }

            AddWaitingPhrase();
            if (bidiLine == null)
            {
                return 1;
            }

            descender = 0f;
            linesWritten = 0;
            lastX = 0f;
            bool flag = false;
            float num = spaceCharRatio;
            object[] array = new object[2];
            PdfFont pdfFont = null;
            float num2 = 0f;
            array[1] = num2;
            PdfDocument pdfDocument = null;
            PdfContentByte graphics = null;
            PdfContentByte pdfContentByte = null;
            firstLineY = float.NaN;
            int num3 = 1;
            if (runDirection != 0)
            {
                num3 = runDirection;
            }

            if (canvas != null)
            {
                graphics = canvas;
                pdfDocument = canvas.PdfDocument;
                pdfContentByte = (IsTagged(canvas) ? canvas : canvas.GetDuplicate(inheritGraphicState));
            }
            else if (!simulate)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("columntext.go.with.simulate.eq.eq.false.and.text.eq.eq.null"));
            }

            if (!simulate)
            {
                if (num == GLOBAL_SPACE_CHAR_RATIO)
                {
                    num = pdfContentByte.PdfWriter.SpaceCharRatio;
                }
                else if (num < 0.001f)
                {
                    num = 0.001f;
                }
            }

            if (!rectangularMode)
            {
                float num4 = 0f;
                foreach (PdfChunk chunk2 in bidiLine.chunks)
                {
                    num4 = Math.Max(num4, chunk2.Height());
                }

                currentLeading = fixedLeading + num4 * multipliedLeading;
            }

            float num5 = 0f;
            int num6 = 0;
            while (true)
            {
                num5 = (lastWasNewline ? indent : followingIndent);
                PdfLine pdfLine;
                float num7;
                if (rectangularMode)
                {
                    if (rectangularWidth <= num5 + rightIndent)
                    {
                        num6 = 2;
                        if (bidiLine.IsEmpty())
                        {
                            num6 |= 1;
                        }

                        break;
                    }

                    if (bidiLine.IsEmpty())
                    {
                        num6 = 1;
                        break;
                    }

                    pdfLine = bidiLine.ProcessLine(leftX, rectangularWidth - num5 - rightIndent, alignment, num3, arabicOptions, minY, yLine, descender);
                    isWordSplit |= bidiLine.IsWordSplit();
                    if (pdfLine == null)
                    {
                        num6 = 1;
                        break;
                    }

                    float[] maxSize = pdfLine.GetMaxSize(fixedLeading, multipliedLeading);
                    if (UseAscender && float.IsNaN(firstLineY))
                    {
                        currentLeading = pdfLine.Ascender;
                    }
                    else
                    {
                        currentLeading = Math.Max(maxSize[0], maxSize[1] - descender);
                    }

                    if (yLine > maxY || yLine - currentLeading < minY)
                    {
                        num6 = 2;
                        bidiLine.Restore();
                        break;
                    }

                    yLine -= currentLeading;
                    if (!simulate && !flag)
                    {
                        pdfContentByte.BeginText();
                        flag = true;
                    }

                    if (float.IsNaN(firstLineY))
                    {
                        firstLineY = yLine;
                    }

                    UpdateFilledWidth(rectangularWidth - pdfLine.WidthLeft);
                    num7 = leftX;
                }
                else
                {
                    float num8 = yLine - currentLeading;
                    float[] array2 = FindLimitsTwoLines();
                    if (array2 == null)
                    {
                        num6 = 2;
                        if (bidiLine.IsEmpty())
                        {
                            num6 |= 1;
                        }

                        yLine = num8;
                        break;
                    }

                    if (bidiLine.IsEmpty())
                    {
                        num6 = 1;
                        yLine = num8;
                        break;
                    }

                    num7 = Math.Max(array2[0], array2[2]);
                    float num9 = Math.Min(array2[1], array2[3]);
                    if (num9 - num7 <= num5 + rightIndent)
                    {
                        continue;
                    }

                    if (!simulate && !flag)
                    {
                        pdfContentByte.BeginText();
                        flag = true;
                    }

                    pdfLine = bidiLine.ProcessLine(num7, num9 - num7 - num5 - rightIndent, alignment, num3, arabicOptions, minY, yLine, descender);
                    if (pdfLine == null)
                    {
                        num6 = 1;
                        yLine = num8;
                        break;
                    }
                }

                if (IsTagged(canvas) && elementToGo is ListItem && !float.IsNaN(firstLineY) && !firstLineYDone)
                {
                    if (!simulate)
                    {
                        ListLabel listLabel = ((ListItem)elementToGo).ListLabel;
                        canvas.OpenMCBlock(listLabel);
                        Chunk chunk = new Chunk(((ListItem)elementToGo).ListSymbol);
                        chunk.Role = null;
                        ShowTextAligned(canvas, 0, new Phrase(chunk), leftX + listLabel.Indentation, firstLineY, 0f);
                        canvas.CloseMCBlock(listLabel);
                    }

                    firstLineYDone = true;
                }

                if (!simulate)
                {
                    if (listBody != null)
                    {
                        canvas.OpenMCBlock(listBody);
                        listBody = null;
                    }

                    array[0] = pdfFont;
                    pdfContentByte.SetTextMatrix(num7 + (pdfLine.RTL ? rightIndent : num5) + pdfLine.IndentLeft, yLine);
                    lastX = pdfDocument.WriteLineToContent(pdfLine, pdfContentByte, graphics, array, num);
                    pdfFont = (PdfFont)array[0];
                }

                lastWasNewline = repeatFirstLineIndent && pdfLine.NewlineSplit;
                yLine -= (pdfLine.NewlineSplit ? extraParagraphSpace : 0f);
                linesWritten++;
                descender = pdfLine.Descender;
            }

            if (flag)
            {
                pdfContentByte.EndText();
                if (canvas != pdfContentByte)
                {
                    canvas.Add(pdfContentByte);
                }
            }

            return num6;
        }

        public virtual bool IsWordSplit()
        {
            return isWordSplit;
        }

        public virtual void ClearChunks()
        {
            if (bidiLine != null)
            {
                bidiLine.ClearChunks();
            }
        }

        public static float GetWidth(Phrase phrase, int runDirection, int arabicOptions)
        {
            ColumnText columnText = new ColumnText(null);
            columnText.AddText(phrase);
            columnText.AddWaitingPhrase();
            PdfLine pdfLine = columnText.bidiLine.ProcessLine(0f, 20000f, 0, runDirection, arabicOptions, 0f, 0f, 0f);
            if (pdfLine == null)
            {
                return 0f;
            }

            return 20000f - pdfLine.WidthLeft;
        }

        public static float GetWidth(Phrase phrase)
        {
            return GetWidth(phrase, 1, 0);
        }

        public static void ShowTextAligned(PdfContentByte canvas, int alignment, Phrase phrase, float x, float y, float rotation, int runDirection, int arabicOptions)
        {
            if (alignment != 0 && alignment != 1 && alignment != 2)
            {
                alignment = 0;
            }

            canvas.SaveState();
            ColumnText columnText = new ColumnText(canvas);
            float num = -1f;
            float num2 = 2f;
            float num3;
            float num4;
            switch (alignment)
            {
                case 0:
                    num3 = 0f;
                    num4 = 20000f;
                    break;
                case 2:
                    num3 = -20000f;
                    num4 = 0f;
                    break;
                default:
                    num3 = -20000f;
                    num4 = 20000f;
                    break;
            }

            if (rotation == 0f)
            {
                num3 += x;
                num += y;
                num4 += x;
                num2 += y;
            }
            else
            {
                double num5 = (double)rotation * Math.PI / 180.0;
                float num6 = (float)Math.Cos(num5);
                float num7 = (float)Math.Sin(num5);
                canvas.ConcatCTM(num6, num7, 0f - num7, num6, x, y);
            }

            columnText.SetSimpleColumn(phrase, num3, num, num4, num2, 2f, alignment);
            if (runDirection == 3)
            {
                switch (alignment)
                {
                    case 0:
                        alignment = 2;
                        break;
                    case 2:
                        alignment = 0;
                        break;
                }
            }

            columnText.Alignment = alignment;
            columnText.ArabicOptions = arabicOptions;
            columnText.RunDirection = runDirection;
            columnText.Go();
            canvas.RestoreState();
        }

        public static void ShowTextAligned(PdfContentByte canvas, int alignment, Phrase phrase, float x, float y, float rotation)
        {
            ShowTextAligned(canvas, alignment, phrase, x, y, rotation, 1, 0);
        }

        public static float FitText(Font font, string text, Rectangle rect, float maxFontSize, int runDirection)
        {
            if (maxFontSize <= 0f)
            {
                int num = 0;
                int num2 = 0;
                char[] array = text.ToCharArray();
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] == '\n')
                    {
                        num2++;
                    }
                    else if (array[i] == '\r')
                    {
                        num++;
                    }
                }

                int num3 = Math.Max(num, num2) + 1;
                maxFontSize = Math.Abs(rect.Height) / (float)num3 - 0.001f;
            }

            font.Size = maxFontSize;
            Phrase phrase = new Phrase(text, font);
            ColumnText columnText = new ColumnText(null);
            columnText.SetSimpleColumn(phrase, rect.Left, rect.Bottom, rect.Right, rect.Top, maxFontSize, 0);
            columnText.RunDirection = runDirection;
            if (((uint)columnText.Go(simulate: true) & (true ? 1u : 0u)) != 0)
            {
                return maxFontSize;
            }

            float num4 = 0.1f;
            float num5 = 0f;
            float num6 = maxFontSize;
            float num7 = maxFontSize;
            for (int j = 0; j < 50; j++)
            {
                num7 = (num5 + num6) / 2f;
                ColumnText columnText2 = new ColumnText(null);
                font.Size = num7;
                columnText2.SetSimpleColumn(new Phrase(text, font), rect.Left, rect.Bottom, rect.Right, rect.Top, num7, 0);
                columnText2.RunDirection = runDirection;
                if (((uint)columnText2.Go(simulate: true) & (true ? 1u : 0u)) != 0)
                {
                    if (num6 - num5 < num7 * num4)
                    {
                        return num7;
                    }

                    num5 = num7;
                }
                else
                {
                    num6 = num7;
                }
            }

            return num7;
        }

        protected virtual int GoComposite(bool simulate)
        {
            if (canvas != null)
            {
                _ = canvas.pdf;
            }

            if (!rectangularMode)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("irregular.columns.are.not.supported.in.composite.mode"));
            }

            linesWritten = 0;
            descender = 0f;
            bool flag = true;
            bool flag2 = runDirection == 3;
            List<IElement> list2;
            int num18;
            while (true)
            {
                if (compositeElements.Count == 0)
                {
                    return 1;
                }

                IElement element = compositeElements[0];
                if (element.Type == 12)
                {
                    Paragraph paragraph = (Paragraph)element;
                    int num = 0;
                    for (int i = 0; i < 2; i++)
                    {
                        float num2 = yLine;
                        bool flag3 = false;
                        if (compositeColumn == null)
                        {
                            compositeColumn = new ColumnText(canvas);
                            compositeColumn.Alignment = paragraph.Alignment;
                            compositeColumn.SetIndent(paragraph.IndentationLeft + paragraph.FirstLineIndent, repeatFirstLineIndent: false);
                            compositeColumn.ExtraParagraphSpace = paragraph.ExtraParagraphSpace;
                            compositeColumn.FollowingIndent = paragraph.IndentationLeft;
                            compositeColumn.RightIndent = paragraph.IndentationRight;
                            compositeColumn.SetLeading(paragraph.Leading, paragraph.MultipliedLeading);
                            compositeColumn.RunDirection = runDirection;
                            compositeColumn.ArabicOptions = arabicOptions;
                            compositeColumn.SpaceCharRatio = spaceCharRatio;
                            compositeColumn.AddText(paragraph);
                            if (!flag || !adjustFirstLine)
                            {
                                yLine -= paragraph.SpacingBefore;
                            }

                            flag3 = true;
                        }

                        compositeColumn.UseAscender = (flag || descender == 0f) && adjustFirstLine && useAscender;
                        compositeColumn.InheritGraphicState = inheritGraphicState;
                        compositeColumn.leftX = leftX;
                        compositeColumn.rightX = rightX;
                        compositeColumn.yLine = yLine;
                        compositeColumn.rectangularWidth = rectangularWidth;
                        compositeColumn.rectangularMode = rectangularMode;
                        compositeColumn.minY = minY;
                        compositeColumn.maxY = maxY;
                        bool flag4 = paragraph.KeepTogether && flag3 && (!flag || !adjustFirstLine);
                        bool flag5 = simulate || (flag4 && i == 0);
                        if (IsTagged(canvas) && !flag5)
                        {
                            canvas.OpenMCBlock(paragraph);
                        }

                        num = compositeColumn.Go(flag5);
                        if (IsTagged(canvas) && !flag5)
                        {
                            canvas.CloseMCBlock(paragraph);
                        }

                        lastX = compositeColumn.LastX;
                        UpdateFilledWidth(compositeColumn.filledWidth);
                        if ((num & 1) == 0 && flag4)
                        {
                            compositeColumn = null;
                            yLine = num2;
                            return 2;
                        }

                        if (simulate || !flag4)
                        {
                            break;
                        }

                        if (i == 0)
                        {
                            compositeColumn = null;
                            yLine = num2;
                        }
                    }

                    flag = false;
                    if (compositeColumn.linesWritten > 0)
                    {
                        yLine = compositeColumn.yLine;
                        linesWritten += compositeColumn.linesWritten;
                        descender = compositeColumn.descender;
                        isWordSplit |= compositeColumn.IsWordSplit();
                    }

                    currentLeading = compositeColumn.currentLeading;
                    if (((uint)num & (true ? 1u : 0u)) != 0)
                    {
                        compositeColumn = null;
                        compositeElements.RemoveAt(0);
                        yLine -= paragraph.SpacingAfter;
                    }

                    if (((uint)num & 2u) != 0)
                    {
                        return 2;
                    }
                }
                else if (element.Type == 14)
                {
                    List list = (List)element;
                    List<IElement> items = list.Items;
                    ListItem listItem = null;
                    float num3 = list.IndentationLeft;
                    int num4 = 0;
                    Stack<object[]> stack = new Stack<object[]>();
                    for (int j = 0; j < items.Count; j++)
                    {
                        object obj = items[j];
                        if (obj is ListItem)
                        {
                            if (num4 == listIdx)
                            {
                                listItem = (ListItem)obj;
                                break;
                            }

                            num4++;
                        }
                        else if (obj is List)
                        {
                            stack.Push(new object[3] { list, j, num3 });
                            list = (List)obj;
                            items = list.Items;
                            num3 += list.IndentationLeft;
                            j = -1;
                            continue;
                        }

                        if (j == items.Count - 1 && stack.Count > 0)
                        {
                            object[] array = stack.Pop();
                            list = (List)array[0];
                            items = list.Items;
                            j = (int)array[1];
                            num3 = (float)array[2];
                        }
                    }

                    int num5 = 0;
                    int num6 = 0;
                    while (true)
                    {
                        if (num6 < 2)
                        {
                            float num7 = yLine;
                            bool flag6 = false;
                            if (compositeColumn == null)
                            {
                                if (listItem == null)
                                {
                                    listIdx = 0;
                                    compositeElements.RemoveAt(0);
                                    break;
                                }

                                compositeColumn = new ColumnText(canvas);
                                compositeColumn.UseAscender = (flag || descender == 0f) && adjustFirstLine && useAscender;
                                compositeColumn.InheritGraphicState = inheritGraphicState;
                                compositeColumn.Alignment = listItem.Alignment;
                                compositeColumn.SetIndent(listItem.IndentationLeft + num3 + listItem.FirstLineIndent, repeatFirstLineIndent: false);
                                compositeColumn.ExtraParagraphSpace = listItem.ExtraParagraphSpace;
                                compositeColumn.FollowingIndent = compositeColumn.Indent;
                                compositeColumn.RightIndent = listItem.IndentationRight + list.IndentationRight;
                                compositeColumn.SetLeading(listItem.Leading, listItem.MultipliedLeading);
                                compositeColumn.RunDirection = runDirection;
                                compositeColumn.ArabicOptions = arabicOptions;
                                compositeColumn.SpaceCharRatio = spaceCharRatio;
                                compositeColumn.AddText(listItem);
                                if (!flag || !adjustFirstLine)
                                {
                                    yLine -= listItem.SpacingBefore;
                                }

                                flag6 = true;
                            }

                            compositeColumn.leftX = leftX;
                            compositeColumn.rightX = rightX;
                            compositeColumn.yLine = yLine;
                            compositeColumn.rectangularWidth = rectangularWidth;
                            compositeColumn.rectangularMode = rectangularMode;
                            compositeColumn.minY = minY;
                            compositeColumn.maxY = maxY;
                            bool flag7 = listItem.KeepTogether && flag6 && (!flag || !adjustFirstLine);
                            bool flag8 = simulate || (flag7 && num6 == 0);
                            if (IsTagged(canvas) && !flag8)
                            {
                                listItem.ListLabel.Indentation = num3;
                                if (list.GetFirstItem() == listItem || (compositeColumn != null && compositeColumn.bidiLine != null))
                                {
                                    canvas.OpenMCBlock(list);
                                }

                                canvas.OpenMCBlock(listItem);
                            }

                            num5 = compositeColumn.Go(simulate || (flag7 && num6 == 0), listItem);
                            if (IsTagged(canvas) && !flag8)
                            {
                                canvas.CloseMCBlock(listItem.ListBody);
                                canvas.CloseMCBlock(listItem);
                                if ((list.GetLastItem() == listItem && ((uint)num5 & (true ? 1u : 0u)) != 0) || ((uint)num5 & 2u) != 0)
                                {
                                    canvas.CloseMCBlock(list);
                                }
                            }

                            lastX = compositeColumn.LastX;
                            UpdateFilledWidth(compositeColumn.filledWidth);
                            if ((num5 & 1) == 0 && flag7)
                            {
                                compositeColumn = null;
                                yLine = num7;
                                return 2;
                            }

                            if (!simulate && flag7)
                            {
                                if (num6 == 0)
                                {
                                    compositeColumn = null;
                                    yLine = num7;
                                }

                                num6++;
                                continue;
                            }
                        }

                        flag = false;
                        yLine = compositeColumn.yLine;
                        linesWritten += compositeColumn.linesWritten;
                        descender = compositeColumn.descender;
                        currentLeading = compositeColumn.currentLeading;
                        if (!IsTagged(canvas) && !float.IsNaN(compositeColumn.firstLineY) && !compositeColumn.firstLineYDone)
                        {
                            if (!simulate)
                            {
                                if (flag2)
                                {
                                    ShowTextAligned(canvas, 2, new Phrase(listItem.ListSymbol), compositeColumn.lastX + listItem.IndentationLeft, compositeColumn.firstLineY, 0f, runDirection, arabicOptions);
                                }
                                else
                                {
                                    ShowTextAligned(canvas, 0, new Phrase(listItem.ListSymbol), compositeColumn.leftX + num3, compositeColumn.firstLineY, 0f);
                                }
                            }

                            compositeColumn.firstLineYDone = true;
                        }

                        if (((uint)num5 & (true ? 1u : 0u)) != 0)
                        {
                            compositeColumn = null;
                            listIdx++;
                            yLine -= listItem.SpacingAfter;
                        }

                        if ((num5 & 2) == 0)
                        {
                            break;
                        }

                        return 2;
                    }
                }
                else if (element.Type == 23)
                {
                    PdfPTable pdfPTable = (PdfPTable)element;
                    if (pdfPTable.Size <= pdfPTable.HeaderRows)
                    {
                        compositeElements.RemoveAt(0);
                        continue;
                    }

                    float num8 = yLine;
                    num8 += descender;
                    if (rowIdx == 0 && adjustFirstLine)
                    {
                        num8 -= pdfPTable.SpacingBefore;
                    }

                    if (num8 < minY || num8 > maxY)
                    {
                        return 2;
                    }

                    float yPos = num8;
                    float num9 = leftX;
                    currentLeading = 0f;
                    float num11;
                    if (!pdfPTable.LockedWidth)
                    {
                        num11 = (pdfPTable.TotalWidth = rectangularWidth * pdfPTable.WidthPercentage / 100f);
                    }
                    else
                    {
                        num11 = pdfPTable.TotalWidth;
                        UpdateFilledWidth(num11);
                    }

                    pdfPTable.NormalizeHeadersFooters();
                    int headerRows = pdfPTable.HeaderRows;
                    int num12 = pdfPTable.FooterRows;
                    int num13 = headerRows - num12;
                    float headerHeight = pdfPTable.HeaderHeight;
                    float footerHeight = pdfPTable.FooterHeight;
                    bool flag9 = pdfPTable.SkipFirstHeader && rowIdx <= num13 && (pdfPTable.ElementComplete || rowIdx != num13);
                    if (!pdfPTable.Complete && pdfPTable.TotalHeight - headerHeight > num8 - minY)
                    {
                        pdfPTable.SkipFirstHeader = false;
                        return 2;
                    }

                    if (!flag9)
                    {
                        num8 -= headerHeight;
                        if (num8 < minY || num8 > maxY)
                        {
                            return 2;
                        }
                    }

                    int num14 = 0;
                    if (rowIdx < headerRows)
                    {
                        rowIdx = headerRows;
                    }

                    if (!pdfPTable.ElementComplete)
                    {
                        num8 -= footerHeight;
                    }

                    PdfPTable.FittingRows fittingRows = pdfPTable.GetFittingRows(num8 - minY, rowIdx);
                    num14 = fittingRows.lastRow + 1;
                    num8 -= fittingRows.height;
                    LOGGER.Info("Want to split at row " + num14);
                    int num15 = num14;
                    while (num15 > rowIdx && num15 < pdfPTable.Size && pdfPTable.GetRow(num15).MayNotBreak)
                    {
                        num15--;
                    }

                    if ((num15 > rowIdx && num15 < num14) || (num15 == 0 && pdfPTable.GetRow(0).MayNotBreak && pdfPTable.LoopCheck))
                    {
                        num8 = minY;
                        num14 = num15;
                        pdfPTable.LoopCheck = false;
                    }

                    LOGGER.Info("Will split at row " + num14);
                    if (pdfPTable.SplitLate && num14 > 0)
                    {
                        fittingRows.CorrectLastRowChosen(pdfPTable, num14 - 1);
                    }

                    if (!pdfPTable.ElementComplete)
                    {
                        num8 += footerHeight;
                    }

                    if (!pdfPTable.SplitRows)
                    {
                        splittedRow = -1;
                        if (num14 == rowIdx)
                        {
                            if (num14 == pdfPTable.Size)
                            {
                                compositeElements.RemoveAt(0);
                                continue;
                            }

                            pdfPTable.Rows.RemoveAt(num14);
                            return 2;
                        }
                    }
                    else if (pdfPTable.SplitLate && rowIdx < num14)
                    {
                        splittedRow = -1;
                    }
                    else if (num14 < pdfPTable.Size)
                    {
                        num8 -= fittingRows.completedRowsHeight - fittingRows.height;
                        float new_height = num8 - minY;
                        PdfPRow pdfPRow = pdfPTable.GetRow(num14).SplitRow(pdfPTable, num14, new_height);
                        if (pdfPRow == null)
                        {
                            LOGGER.Info("Didn't split row!");
                            splittedRow = -1;
                            if (rowIdx == num14)
                            {
                                return 2;
                            }
                        }
                        else
                        {
                            if (num14 != splittedRow)
                            {
                                splittedRow = num14 + 1;
                                pdfPTable = new PdfPTable(pdfPTable);
                                compositeElements[0] = pdfPTable;
                                List<PdfPRow> rows = pdfPTable.Rows;
                                for (int k = headerRows; k < rowIdx; k++)
                                {
                                    rows[k] = null;
                                }
                            }

                            num8 = minY;
                            pdfPTable.Rows.Insert(++num14, pdfPRow);
                            LOGGER.Info("Inserting row at position " + num14);
                        }
                    }

                    flag = false;
                    if (!simulate)
                    {
                        switch (pdfPTable.HorizontalAlignment)
                        {
                            case 2:
                                if (!flag2)
                                {
                                    num9 += rectangularWidth - num11;
                                }

                                break;
                            case 1:
                                num9 += (rectangularWidth - num11) / 2f;
                                break;
                            default:
                                if (flag2)
                                {
                                    num9 += rectangularWidth - num11;
                                }

                                break;
                        }

                        PdfPTable pdfPTable2 = PdfPTable.ShallowCopy(pdfPTable);
                        List<PdfPRow> rows2 = pdfPTable2.Rows;
                        if (!flag9 && num13 > 0)
                        {
                            List<PdfPRow> rows3 = pdfPTable.GetRows(0, num13);
                            if (IsTagged(canvas))
                            {
                                pdfPTable2.GetHeader().rows = rows3;
                            }

                            rows2.AddRange(rows3);
                        }
                        else
                        {
                            pdfPTable2.HeaderRows = num12;
                        }

                        List<PdfPRow> rows4 = pdfPTable.GetRows(rowIdx, num14);
                        if (IsTagged(canvas))
                        {
                            pdfPTable2.GetBody().rows = rows4;
                        }

                        rows2.AddRange(rows4);
                        bool flag10 = !pdfPTable.SkipLastFooter;
                        bool flag11 = false;
                        if (num14 < pdfPTable.Size)
                        {
                            pdfPTable2.ElementComplete = true;
                            flag10 = true;
                            flag11 = true;
                        }

                        if (num12 > 0 && pdfPTable2.ElementComplete && flag10)
                        {
                            List<PdfPRow> rows5 = pdfPTable.GetRows(num13, num13 + num12);
                            if (IsTagged(canvas))
                            {
                                pdfPTable2.GetFooter().rows = rows5;
                            }

                            rows2.AddRange(rows5);
                        }
                        else
                        {
                            num12 = 0;
                        }

                        float num16 = 0f;
                        int num17 = rows2.Count - 1 - num12;
                        PdfPRow pdfPRow2 = rows2[num17];
                        if (pdfPTable.IsExtendLastRow(flag11))
                        {
                            num16 = pdfPRow2.MaxHeights;
                            pdfPRow2.MaxHeights = num8 - minY + num16;
                            num8 = minY;
                        }

                        if (flag11)
                        {
                            IPdfPTableEvent tableEvent = pdfPTable.TableEvent;
                            if (tableEvent is IPdfPTableEventSplit)
                            {
                                ((IPdfPTableEventSplit)tableEvent).SplitTable(pdfPTable);
                            }
                        }

                        if (canvases != null)
                        {
                            if (IsTagged(canvases[3]))
                            {
                                canvases[3].OpenMCBlock(pdfPTable);
                            }

                            pdfPTable2.WriteSelectedRows(0, -1, 0, -1, num9, yPos, canvases, reusable: false);
                            if (IsTagged(canvases[3]))
                            {
                                canvases[3].CloseMCBlock(pdfPTable);
                            }
                        }
                        else
                        {
                            if (IsTagged(canvas))
                            {
                                canvas.OpenMCBlock(pdfPTable);
                            }

                            pdfPTable2.WriteSelectedRows(0, -1, 0, -1, num9, yPos, canvas, reusable: false);
                            if (IsTagged(canvas))
                            {
                                canvas.CloseMCBlock(pdfPTable);
                            }
                        }

                        if (!pdfPTable.Complete)
                        {
                            pdfPTable.AddNumberOfRowsWritten(num14);
                        }

                        if (splittedRow == num14 && num14 < pdfPTable.Size)
                        {
                            pdfPTable.Rows[num14].CopyRowContent(pdfPTable2, num17);
                        }
                        else if (num14 > 0 && num14 < pdfPTable.Size)
                        {
                            pdfPTable.GetRow(num14).SplitRowspans(pdfPTable, num14 - 1, pdfPTable2, num17);
                        }

                        if (pdfPTable.IsExtendLastRow(flag11))
                        {
                            pdfPRow2.MaxHeights = num16;
                        }

                        if (flag11)
                        {
                            IPdfPTableEvent tableEvent2 = pdfPTable.TableEvent;
                            if (tableEvent2 is IPdfPTableEventAfterSplit)
                            {
                                PdfPRow row = pdfPTable.GetRow(num14);
                                ((IPdfPTableEventAfterSplit)tableEvent2).AfterSplitTable(pdfPTable, row, num14);
                            }
                        }
                    }
                    else if (pdfPTable.ExtendLastRow && minY > -1.07374182E+09f)
                    {
                        num8 = minY;
                    }

                    yLine = num8;
                    descender = 0f;
                    currentLeading = 0f;
                    if (!flag9 && !pdfPTable.ElementComplete)
                    {
                        yLine += footerHeight;
                    }

                    for (; num14 < pdfPTable.Size && !(pdfPTable.GetRowHeight(num14) > 0f) && !pdfPTable.HasRowspan(num14); num14++)
                    {
                    }

                    if (num14 < pdfPTable.Size)
                    {
                        if (splittedRow != -1)
                        {
                            List<PdfPRow> rows6 = pdfPTable.Rows;
                            for (int l = rowIdx; l < num14; l++)
                            {
                                rows6[l] = null;
                            }
                        }

                        rowIdx = num14;
                        return 2;
                    }

                    if (yLine - pdfPTable.SpacingAfter < minY)
                    {
                        yLine = minY;
                    }
                    else
                    {
                        yLine -= pdfPTable.SpacingAfter;
                    }

                    compositeElements.RemoveAt(0);
                    splittedRow = -1;
                    rowIdx = 0;
                }
                else if (element.Type == 55)
                {
                    if (!simulate)
                    {
                        ((IDrawInterface)element).Draw(canvas, leftX, minY, rightX, maxY, yLine);
                    }

                    compositeElements.RemoveAt(0);
                }
                else if (element.Type == 37)
                {
                    list2 = new List<IElement>();
                    do
                    {
                        list2.Add(element);
                        compositeElements.RemoveAt(0);
                        element = ((compositeElements.Count > 0) ? compositeElements[0] : null);
                    }
                    while (element != null && element.Type == 37);
                    FloatLayout floatLayout = new FloatLayout(list2, useAscender);
                    floatLayout.SetSimpleColumn(leftX, minY, rightX, yLine);
                    num18 = floatLayout.Layout(canvas, simulate);
                    yLine = floatLayout.YLine;
                    descender = 0f;
                    if ((num18 & 1) == 0)
                    {
                        break;
                    }
                }
                else
                {
                    compositeElements.RemoveAt(0);
                }
            }

            foreach (IElement item in list2)
            {
                compositeElements.Add(item);
            }

            return num18;
        }

        public virtual bool ZeroHeightElement()
        {
            if (composite && compositeElements.Count != 0)
            {
                return compositeElements[0].Type == 55;
            }

            return false;
        }

        public static bool HasMoreText(int status)
        {
            return (status & 1) == 0;
        }

        public virtual void UpdateFilledWidth(float w)
        {
            if (w > filledWidth)
            {
                filledWidth = w;
            }
        }

        private static bool IsTagged(PdfContentByte canvas)
        {
            if (canvas != null && canvas.pdf != null && canvas.writer != null)
            {
                return canvas.writer.IsTagged();
            }

            return false;
        }
    }
}
