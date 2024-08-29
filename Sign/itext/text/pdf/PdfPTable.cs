using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;
using Sign.itext.text.api;
using Sign.itext.text.log;
using Sign.itext.text.pdf.events;

namespace Sign.itext.text.pdf
{
    public class PdfPTable : ILargeElement, IElement, ISpaceable, IAccessibleElement
    {
        public class FittingRows
        {
            public readonly int firstRow;

            public readonly int lastRow;

            public readonly float height;

            public readonly float completedRowsHeight;

            private readonly Dictionary<int, float> correctedHeightsForLastRow;

            public FittingRows(int firstRow, int lastRow, float height, float completedRowsHeight, Dictionary<int, float> correctedHeightsForLastRow)
            {
                this.firstRow = firstRow;
                this.lastRow = lastRow;
                this.height = height;
                this.completedRowsHeight = completedRowsHeight;
                this.correctedHeightsForLastRow = correctedHeightsForLastRow;
            }

            public virtual void CorrectLastRowChosen(PdfPTable table, int k)
            {
                PdfPRow row = table.GetRow(k);
                if (correctedHeightsForLastRow.TryGetValue(k, out var value))
                {
                    row.SetFinalMaxHeights(value);
                }
            }
        }

        public class ColumnMeasurementState
        {
            public float height;

            public int rowspan = 1;

            public int colspan = 1;

            public virtual void BeginCell(PdfPCell cell, float completedRowsHeight, float rowHeight)
            {
                rowspan = cell.Rowspan;
                colspan = cell.Colspan;
                height = completedRowsHeight + Math.Max(cell.GetMaxHeight(), rowHeight);
            }

            public virtual void ConsumeRowspan(float completedRowsHeight, float rowHeight)
            {
                rowspan--;
            }

            public virtual bool CellEnds()
            {
                return rowspan == 1;
            }
        }

        private readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(PdfPTable));

        public const int BASECANVAS = 0;

        public const int BACKGROUNDCANVAS = 1;

        public const int LINECANVAS = 2;

        public const int TEXTCANVAS = 3;

        protected List<PdfPRow> rows = new List<PdfPRow>();

        protected float totalHeight;

        protected PdfPCell[] currentRow;

        protected int currentColIdx;

        protected PdfPCell defaultCell = new PdfPCell((Phrase)null);

        protected float totalWidth;

        protected float[] relativeWidths;

        protected float[] absoluteWidths;

        protected IPdfPTableEvent tableEvent;

        protected int headerRows;

        protected float widthPercentage = 80f;

        private int horizontalAlignment = 1;

        private bool skipFirstHeader;

        private bool skipLastFooter;

        protected bool isColspan;

        protected int runDirection;

        private bool lockedWidth;

        private bool splitRows = true;

        protected float spacingBefore;

        protected float spacingAfter;

        private bool[] extendLastRow = new bool[2];

        private bool headersInEvent;

        private bool splitLate = true;

        private bool keepTogether;

        protected bool complete = true;

        private int footerRows;

        protected bool rowCompleted = true;

        protected bool loopCheck = true;

        protected bool rowsNotChecked = true;

        protected PdfName role = PdfName.TABLE;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes;

        protected AccessibleElementId id = new AccessibleElementId();

        private PdfPTableHeader header;

        private PdfPTableBody body;

        private PdfPTableFooter footer;

        private int numberOfWrittenRows;

        public virtual bool Complete
        {
            get
            {
                return complete;
            }
            set
            {
                complete = value;
            }
        }

        public virtual float TotalWidth
        {
            get
            {
                return totalWidth;
            }
            set
            {
                if (totalWidth != value)
                {
                    totalWidth = value;
                    totalHeight = 0f;
                    CalculateWidths();
                    CalculateHeights();
                }
            }
        }

        public virtual PdfPCell DefaultCell => defaultCell;

        public virtual int Size => rows.Count;

        public virtual float TotalHeight => totalHeight;

        public virtual float HeaderHeight
        {
            get
            {
                float num = 0f;
                int num2 = Math.Min(rows.Count, headerRows);
                for (int i = 0; i < num2; i++)
                {
                    PdfPRow pdfPRow = rows[i];
                    if (pdfPRow != null)
                    {
                        num += pdfPRow.MaxHeights;
                    }
                }

                return num;
            }
        }

        public virtual float FooterHeight
        {
            get
            {
                float num = 0f;
                int num2 = Math.Max(0, headerRows - footerRows);
                int num3 = Math.Min(rows.Count, headerRows);
                for (int i = num2; i < num3; i++)
                {
                    PdfPRow pdfPRow = rows[i];
                    if (pdfPRow != null)
                    {
                        num += pdfPRow.MaxHeights;
                    }
                }

                return num;
            }
        }

        public virtual int NumberOfColumns => relativeWidths.Length;

        public virtual int HeaderRows
        {
            get
            {
                return headerRows;
            }
            set
            {
                headerRows = value;
                if (headerRows < 0)
                {
                    headerRows = 0;
                }
            }
        }

        public virtual int FooterRows
        {
            get
            {
                return footerRows;
            }
            set
            {
                footerRows = value;
                if (footerRows < 0)
                {
                    footerRows = 0;
                }
            }
        }

        public virtual IList<Chunk> Chunks => new List<Chunk>();

        public virtual int Type => 23;

        public virtual float WidthPercentage
        {
            get
            {
                return widthPercentage;
            }
            set
            {
                widthPercentage = value;
            }
        }

        public virtual int HorizontalAlignment
        {
            get
            {
                return horizontalAlignment;
            }
            set
            {
                horizontalAlignment = value;
            }
        }

        public virtual List<PdfPRow> Rows => rows;

        public virtual IPdfPTableEvent TableEvent
        {
            get
            {
                return tableEvent;
            }
            set
            {
                if (value == null)
                {
                    tableEvent = null;
                    return;
                }

                if (tableEvent == null)
                {
                    tableEvent = value;
                    return;
                }

                if (tableEvent is PdfPTableEventForwarder)
                {
                    ((PdfPTableEventForwarder)tableEvent).AddTableEvent(value);
                    return;
                }

                PdfPTableEventForwarder pdfPTableEventForwarder = new PdfPTableEventForwarder();
                pdfPTableEventForwarder.AddTableEvent(tableEvent);
                pdfPTableEventForwarder.AddTableEvent(value);
                tableEvent = pdfPTableEventForwarder;
            }
        }

        public virtual float[] AbsoluteWidths => absoluteWidths;

        public virtual bool SkipFirstHeader
        {
            get
            {
                return skipFirstHeader;
            }
            set
            {
                skipFirstHeader = value;
            }
        }

        public virtual bool SkipLastFooter
        {
            get
            {
                return skipLastFooter;
            }
            set
            {
                skipLastFooter = value;
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
                switch (value)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        runDirection = value;
                        break;
                    default:
                        throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.run.direction.1", runDirection));
                }
            }
        }

        public virtual bool LockedWidth
        {
            get
            {
                return lockedWidth;
            }
            set
            {
                lockedWidth = value;
            }
        }

        public virtual bool SplitRows
        {
            get
            {
                return splitRows;
            }
            set
            {
                splitRows = value;
            }
        }

        public virtual float SpacingBefore
        {
            get
            {
                return spacingBefore;
            }
            set
            {
                spacingBefore = value;
            }
        }

        public virtual float SpacingAfter
        {
            get
            {
                return spacingAfter;
            }
            set
            {
                spacingAfter = value;
            }
        }

        public virtual string Summary
        {
            get
            {
                return GetAccessibleAttribute(PdfName.SUMMARY).ToString();
            }
            set
            {
                SetAccessibleAttribute(PdfName.SUMMARY, new PdfString(value));
            }
        }

        public virtual bool ExtendLastRow
        {
            get
            {
                return extendLastRow[0];
            }
            set
            {
                extendLastRow[0] = value;
                extendLastRow[1] = value;
            }
        }

        public virtual bool HeadersInEvent
        {
            get
            {
                return headersInEvent;
            }
            set
            {
                headersInEvent = value;
            }
        }

        public virtual bool SplitLate
        {
            get
            {
                return splitLate;
            }
            set
            {
                splitLate = value;
            }
        }

        public virtual bool KeepTogether
        {
            get
            {
                return keepTogether;
            }
            set
            {
                keepTogether = value;
            }
        }

        public virtual bool ElementComplete
        {
            get
            {
                return complete;
            }
            set
            {
                complete = value;
            }
        }

        public virtual bool LoopCheck
        {
            get
            {
                return loopCheck;
            }
            set
            {
                loopCheck = value;
            }
        }

        public virtual PdfName Role
        {
            get
            {
                return role;
            }
            set
            {
                role = value;
            }
        }

        public virtual AccessibleElementId ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public virtual bool IsInline => false;

        protected PdfPTable()
        {
        }

        public PdfPTable(float[] relativeWidths)
        {
            if (relativeWidths == null)
            {
                throw new ArgumentNullException(MessageLocalization.GetComposedMessage("the.widths.array.in.pdfptable.constructor.can.not.be.null"));
            }

            if (relativeWidths.Length == 0)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.widths.array.in.pdfptable.constructor.can.not.have.zero.length"));
            }

            this.relativeWidths = new float[relativeWidths.Length];
            Array.Copy(relativeWidths, 0, this.relativeWidths, 0, relativeWidths.Length);
            absoluteWidths = new float[relativeWidths.Length];
            CalculateWidths();
            currentRow = new PdfPCell[absoluteWidths.Length];
            keepTogether = false;
        }

        public PdfPTable(int numColumns)
        {
            if (numColumns <= 0)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.number.of.columns.in.pdfptable.constructor.must.be.greater.than.zero"));
            }

            relativeWidths = new float[numColumns];
            for (int i = 0; i < numColumns; i++)
            {
                relativeWidths[i] = 1f;
            }

            absoluteWidths = new float[relativeWidths.Length];
            CalculateWidths();
            currentRow = new PdfPCell[absoluteWidths.Length];
            keepTogether = false;
        }

        public PdfPTable(PdfPTable table)
        {
            CopyFormat(table);
            for (int i = 0; i < currentRow.Length && table.currentRow[i] != null; i++)
            {
                currentRow[i] = new PdfPCell(table.currentRow[i]);
            }

            for (int j = 0; j < table.rows.Count; j++)
            {
                PdfPRow pdfPRow = table.rows[j];
                if (pdfPRow != null)
                {
                    pdfPRow = new PdfPRow(pdfPRow);
                }

                rows.Add(pdfPRow);
            }
        }

        public virtual void Init()
        {
            LOGGER.Info("Initialize row and cell heights");
            foreach (PdfPRow row in Rows)
            {
                if (row == null)
                {
                    continue;
                }

                row.calculated = false;
                PdfPCell[] cells = row.GetCells();
                foreach (PdfPCell pdfPCell in cells)
                {
                    if (pdfPCell != null)
                    {
                        pdfPCell.CalculatedHeight = 0f;
                    }
                }
            }
        }

        public static PdfPTable ShallowCopy(PdfPTable table)
        {
            PdfPTable pdfPTable = new PdfPTable();
            pdfPTable.CopyFormat(table);
            return pdfPTable;
        }

        protected internal virtual void CopyFormat(PdfPTable sourceTable)
        {
            rowsNotChecked = sourceTable.rowsNotChecked;
            relativeWidths = new float[sourceTable.NumberOfColumns];
            absoluteWidths = new float[sourceTable.NumberOfColumns];
            Array.Copy(sourceTable.relativeWidths, 0, relativeWidths, 0, NumberOfColumns);
            Array.Copy(sourceTable.absoluteWidths, 0, absoluteWidths, 0, NumberOfColumns);
            totalWidth = sourceTable.totalWidth;
            totalHeight = sourceTable.totalHeight;
            currentColIdx = 0;
            tableEvent = sourceTable.tableEvent;
            runDirection = sourceTable.runDirection;
            if (sourceTable.defaultCell is PdfPHeaderCell)
            {
                defaultCell = new PdfPHeaderCell((PdfPHeaderCell)sourceTable.defaultCell);
            }
            else
            {
                defaultCell = new PdfPCell(sourceTable.defaultCell);
            }

            currentRow = new PdfPCell[sourceTable.currentRow.Length];
            isColspan = sourceTable.isColspan;
            splitRows = sourceTable.splitRows;
            spacingAfter = sourceTable.spacingAfter;
            spacingBefore = sourceTable.spacingBefore;
            headerRows = sourceTable.headerRows;
            footerRows = sourceTable.footerRows;
            lockedWidth = sourceTable.lockedWidth;
            extendLastRow = sourceTable.extendLastRow;
            headersInEvent = sourceTable.headersInEvent;
            widthPercentage = sourceTable.widthPercentage;
            splitLate = sourceTable.splitLate;
            skipFirstHeader = sourceTable.skipFirstHeader;
            skipLastFooter = sourceTable.skipLastFooter;
            horizontalAlignment = sourceTable.horizontalAlignment;
            keepTogether = sourceTable.keepTogether;
            complete = sourceTable.complete;
            loopCheck = sourceTable.loopCheck;
            id = sourceTable.ID;
            role = sourceTable.Role;
            if (sourceTable.accessibleAttributes != null)
            {
                accessibleAttributes = new Dictionary<PdfName, PdfObject>(sourceTable.accessibleAttributes);
            }

            header = sourceTable.GetHeader();
            body = sourceTable.GetBody();
            footer = sourceTable.GetFooter();
        }

        public virtual void SetWidths(float[] relativeWidths)
        {
            if (relativeWidths.Length != NumberOfColumns)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("wrong.number.of.columns"));
            }

            this.relativeWidths = new float[relativeWidths.Length];
            Array.Copy(relativeWidths, 0, this.relativeWidths, 0, relativeWidths.Length);
            absoluteWidths = new float[relativeWidths.Length];
            totalHeight = 0f;
            CalculateWidths();
            CalculateHeights();
        }

        public virtual void SetWidths(int[] relativeWidths)
        {
            float[] array = new float[relativeWidths.Length];
            for (int i = 0; i < relativeWidths.Length; i++)
            {
                array[i] = relativeWidths[i];
            }

            SetWidths(array);
        }

        protected internal virtual void CalculateWidths()
        {
            if (!(totalWidth <= 0f))
            {
                float num = 0f;
                int numberOfColumns = NumberOfColumns;
                for (int i = 0; i < numberOfColumns; i++)
                {
                    num += relativeWidths[i];
                }

                for (int j = 0; j < numberOfColumns; j++)
                {
                    absoluteWidths[j] = totalWidth * relativeWidths[j] / num;
                }
            }
        }

        public virtual void SetTotalWidth(float[] columnWidth)
        {
            if (columnWidth.Length != NumberOfColumns)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("wrong.number.of.columns"));
            }

            totalWidth = 0f;
            for (int i = 0; i < columnWidth.Length; i++)
            {
                totalWidth += columnWidth[i];
            }

            SetWidths(columnWidth);
        }

        public virtual void SetWidthPercentage(float[] columnWidth, Rectangle pageSize)
        {
            if (columnWidth.Length != NumberOfColumns)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("wrong.number.of.columns"));
            }

            float num = 0f;
            for (int i = 0; i < columnWidth.Length; i++)
            {
                num += columnWidth[i];
            }

            widthPercentage = num / (pageSize.Right - pageSize.Left) * 100f;
            SetWidths(columnWidth);
        }

        public virtual float CalculateHeights()
        {
            if (totalWidth <= 0f)
            {
                return 0f;
            }

            totalHeight = 0f;
            for (int i = 0; i < rows.Count; i++)
            {
                totalHeight += GetRowHeight(i, firsttime: true);
            }

            return totalHeight;
        }

        public virtual void ResetColumnCount(int newColCount)
        {
            if (newColCount <= 0)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.number.of.columns.in.pdfptable.constructor.must.be.greater.than.zero"));
            }

            relativeWidths = new float[newColCount];
            for (int i = 0; i < newColCount; i++)
            {
                relativeWidths[i] = 1f;
            }

            absoluteWidths = new float[relativeWidths.Length];
            CalculateWidths();
            currentRow = new PdfPCell[absoluteWidths.Length];
            totalHeight = 0f;
        }

        public virtual PdfPCell AddCell(PdfPCell cell)
        {
            rowCompleted = false;
            PdfPCell pdfPCell = ((!(cell is PdfPHeaderCell)) ? new PdfPCell(cell) : new PdfPHeaderCell((PdfPHeaderCell)cell));
            int colspan = pdfPCell.Colspan;
            colspan = Math.Max(colspan, 1);
            colspan = (pdfPCell.Colspan = Math.Min(colspan, currentRow.Length - currentColIdx));
            if (colspan != 1)
            {
                isColspan = true;
            }

            if (pdfPCell.RunDirection == 0)
            {
                pdfPCell.RunDirection = runDirection;
            }

            SkipColsWithRowspanAbove();
            bool flag = false;
            if (currentColIdx < currentRow.Length)
            {
                currentRow[currentColIdx] = pdfPCell;
                currentColIdx += colspan;
                flag = true;
            }

            SkipColsWithRowspanAbove();
            while (currentColIdx >= currentRow.Length)
            {
                int numberOfColumns = NumberOfColumns;
                if (runDirection == 3)
                {
                    PdfPCell[] array = new PdfPCell[numberOfColumns];
                    int num2 = currentRow.Length;
                    int num3;
                    for (num3 = 0; num3 < currentRow.Length; num3++)
                    {
                        PdfPCell pdfPCell2 = currentRow[num3];
                        int colspan2 = pdfPCell2.Colspan;
                        num2 -= colspan2;
                        array[num2] = pdfPCell2;
                        num3 += colspan2 - 1;
                    }

                    currentRow = array;
                }

                PdfPRow pdfPRow = new PdfPRow(currentRow);
                if (totalWidth > 0f)
                {
                    pdfPRow.SetWidths(absoluteWidths);
                    totalHeight += pdfPRow.MaxHeights;
                }

                rows.Add(pdfPRow);
                currentRow = new PdfPCell[numberOfColumns];
                currentColIdx = 0;
                SkipColsWithRowspanAbove();
                rowCompleted = true;
            }

            if (!flag)
            {
                currentRow[currentColIdx] = pdfPCell;
                currentColIdx += colspan;
            }

            return pdfPCell;
        }

        private void SkipColsWithRowspanAbove()
        {
            int num = 1;
            if (runDirection == 3)
            {
                num = -1;
            }

            while (RowSpanAbove(rows.Count, currentColIdx))
            {
                currentColIdx += num;
            }
        }

        internal PdfPCell CellAt(int row, int col)
        {
            PdfPCell[] cells = rows[row].GetCells();
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null && col >= i && col < i + cells[i].Colspan)
                {
                    return cells[i];
                }
            }

            return null;
        }

        internal bool RowSpanAbove(int currRow, int currCol)
        {
            if (currCol >= NumberOfColumns || currCol < 0 || currRow < 1)
            {
                return false;
            }

            int num = currRow - 1;
            PdfPRow pdfPRow = rows[num];
            if (pdfPRow == null)
            {
                return false;
            }

            PdfPCell pdfPCell = CellAt(num, currCol);
            while (pdfPCell == null && num > 0)
            {
                pdfPRow = rows[--num];
                if (pdfPRow == null)
                {
                    return false;
                }

                pdfPCell = CellAt(num, currCol);
            }

            int num2 = currRow - num;
            if (pdfPCell.Rowspan == 1 && num2 > 1)
            {
                int num3 = currCol - 1;
                pdfPRow = rows[num + 1];
                num2--;
                pdfPCell = pdfPRow.GetCells()[num3];
                while (pdfPCell == null && num3 > 0)
                {
                    pdfPCell = pdfPRow.GetCells()[--num3];
                }
            }

            if (pdfPCell != null)
            {
                return pdfPCell.Rowspan > num2;
            }

            return false;
        }

        public virtual void AddCell(string text)
        {
            AddCell(new Phrase(text));
        }

        public virtual void AddCell(PdfPTable table)
        {
            defaultCell.Table = table;
            AddCell(defaultCell).id = new AccessibleElementId();
            defaultCell.Table = null;
        }

        public virtual void AddCell(Image image)
        {
            defaultCell.Image = image;
            AddCell(defaultCell).id = new AccessibleElementId();
            defaultCell.Image = null;
        }

        public virtual void AddCell(Phrase phrase)
        {
            defaultCell.Phrase = phrase;
            AddCell(defaultCell).id = new AccessibleElementId();
            defaultCell.Phrase = null;
        }

        public virtual float WriteSelectedRows(int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte[] canvases)
        {
            return WriteSelectedRows(0, -1, rowStart, rowEnd, xPos, yPos, canvases);
        }

        public virtual float WriteSelectedRows(int colStart, int colEnd, int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte[] canvases)
        {
            return WriteSelectedRows(colStart, colEnd, rowStart, rowEnd, xPos, yPos, canvases, reusable: true);
        }

        public virtual float WriteSelectedRows(int colStart, int colEnd, int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte[] canvases, bool reusable)
        {
            if (totalWidth <= 0f)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.table.width.must.be.greater.than.zero"));
            }

            int count = rows.Count;
            if (rowStart < 0)
            {
                rowStart = 0;
            }

            rowEnd = ((rowEnd >= 0) ? Math.Min(rowEnd, count) : count);
            if (rowStart >= rowEnd)
            {
                return yPos;
            }

            int numberOfColumns = NumberOfColumns;
            colStart = ((colStart >= 0) ? Math.Min(colStart, numberOfColumns) : 0);
            colEnd = ((colEnd >= 0) ? Math.Min(colEnd, numberOfColumns) : numberOfColumns);
            LOGGER.Info($"Writing row {rowStart} to {rowEnd}; column {colStart} to {colEnd}");
            float num = yPos;
            PdfPTableBody pdfPTableBody = null;
            if (rowsNotChecked)
            {
                GetFittingRows(float.MaxValue, rowStart);
            }

            List<PdfPRow> list = GetRows(rowStart, rowEnd);
            int num2 = rowStart;
            foreach (PdfPRow item in list)
            {
                if (GetHeader().rows != null && GetHeader().rows.Contains(item) && pdfPTableBody == null)
                {
                    pdfPTableBody = OpenTableBlock(GetHeader(), canvases[3]);
                }
                else if (GetBody().rows != null && GetBody().rows.Contains(item) && pdfPTableBody == null)
                {
                    pdfPTableBody = OpenTableBlock(GetBody(), canvases[3]);
                }
                else if (GetFooter().rows != null && GetFooter().rows.Contains(item) && pdfPTableBody == null)
                {
                    pdfPTableBody = OpenTableBlock(GetFooter(), canvases[3]);
                }

                if (item != null)
                {
                    item.WriteCells(colStart, colEnd, xPos, yPos, canvases, reusable);
                    yPos -= item.MaxHeights;
                }

                if (GetHeader().rows != null && GetHeader().rows.Contains(item) && (num2 == rowEnd - 1 || !GetHeader().rows.Contains(list[num2 + 1])))
                {
                    pdfPTableBody = CloseTableBlock(GetHeader(), canvases[3]);
                }
                else if (GetBody().rows != null && GetBody().rows.Contains(item) && (num2 == rowEnd - 1 || !GetBody().rows.Contains(list[num2 + 1])))
                {
                    pdfPTableBody = CloseTableBlock(GetBody(), canvases[3]);
                }
                else if (GetFooter().rows != null && GetFooter().rows.Contains(item) && (num2 == rowEnd - 1 || !GetFooter().rows.Contains(list[num2 + 1])))
                {
                    pdfPTableBody = CloseTableBlock(GetFooter(), canvases[3]);
                }

                num2++;
            }

            if (tableEvent != null && colStart == 0 && colEnd == numberOfColumns)
            {
                float[] array = new float[rowEnd - rowStart + 1];
                array[0] = num;
                for (num2 = rowStart; num2 < rowEnd; num2++)
                {
                    PdfPRow pdfPRow = list[num2];
                    float num3 = 0f;
                    if (pdfPRow != null)
                    {
                        num3 = pdfPRow.MaxHeights;
                    }

                    array[num2 - rowStart + 1] = array[num2 - rowStart] - num3;
                }

                tableEvent.TableLayout(this, GetEventWidths(xPos, rowStart, rowEnd, headersInEvent), array, headersInEvent ? headerRows : 0, rowStart, canvases);
            }

            return yPos;
        }

        private PdfPTableBody OpenTableBlock(PdfPTableBody block, PdfContentByte canvas)
        {
            if (canvas.writer.GetStandardStructElems().Contains(block.Role))
            {
                canvas.OpenMCBlock(block);
                return block;
            }

            return null;
        }

        private PdfPTableBody CloseTableBlock(PdfPTableBody block, PdfContentByte canvas)
        {
            if (canvas.writer.GetStandardStructElems().Contains(block.Role))
            {
                canvas.CloseMCBlock(block);
            }

            return null;
        }

        public virtual float WriteSelectedRows(int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte canvas)
        {
            return WriteSelectedRows(0, -1, rowStart, rowEnd, xPos, yPos, canvas);
        }

        public virtual float WriteSelectedRows(int colStart, int colEnd, int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte canvas)
        {
            return WriteSelectedRows(colStart, colEnd, rowStart, rowEnd, xPos, yPos, canvas, reusable: true);
        }

        public virtual float WriteSelectedRows(int colStart, int colEnd, int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte canvas, bool reusable)
        {
            int numberOfColumns = NumberOfColumns;
            colStart = ((colStart >= 0) ? Math.Min(colStart, numberOfColumns) : 0);
            colEnd = ((colEnd >= 0) ? Math.Min(colEnd, numberOfColumns) : numberOfColumns);
            bool flag = colStart != 0 || colEnd != numberOfColumns;
            if (flag)
            {
                float num = 0f;
                for (int i = colStart; i < colEnd; i++)
                {
                    num += absoluteWidths[i];
                }

                canvas.SaveState();
                float num2 = ((colStart == 0) ? 10000 : 0);
                float num3 = ((colEnd == numberOfColumns) ? 10000 : 0);
                canvas.Rectangle(xPos - num2, -10000f, num + num2 + num3, 20000f);
                canvas.Clip();
                canvas.NewPath();
            }

            PdfContentByte[] canvases = BeginWritingRows(canvas);
            float result = WriteSelectedRows(colStart, colEnd, rowStart, rowEnd, xPos, yPos, canvases, reusable);
            EndWritingRows(canvases);
            if (flag)
            {
                canvas.RestoreState();
            }

            return result;
        }

        public static PdfContentByte[] BeginWritingRows(PdfContentByte canvas)
        {
            return new PdfContentByte[4] { canvas, canvas.Duplicate, canvas.Duplicate, canvas.Duplicate };
        }

        public static void EndWritingRows(PdfContentByte[] canvases)
        {
            PdfContentByte obj = canvases[0];
            PdfArtifact element = new PdfArtifact();
            obj.OpenMCBlock(element);
            obj.SaveState();
            obj.Add(canvases[1]);
            obj.RestoreState();
            obj.SaveState();
            obj.SetLineCap(2);
            obj.ResetRGBColorStroke();
            obj.Add(canvases[2]);
            obj.RestoreState();
            obj.CloseMCBlock(element);
            obj.Add(canvases[3]);
        }

        public virtual float GetRowHeight(int idx)
        {
            return GetRowHeight(idx, firsttime: false);
        }

        protected internal virtual float GetRowHeight(int idx, bool firsttime)
        {
            if (totalWidth <= 0f || idx < 0 || idx >= rows.Count)
            {
                return 0f;
            }

            PdfPRow pdfPRow = rows[idx];
            if (pdfPRow == null)
            {
                return 0f;
            }

            if (firsttime)
            {
                pdfPRow.SetWidths(absoluteWidths);
            }

            float num = pdfPRow.MaxHeights;
            for (int i = 0; i < relativeWidths.Length; i++)
            {
                if (!RowSpanAbove(idx, i))
                {
                    continue;
                }

                int j;
                for (j = 1; RowSpanAbove(idx - j, i); j++)
                {
                }

                PdfPCell pdfPCell = rows[idx - j].GetCells()[i];
                float num2 = 0f;
                if (pdfPCell != null && pdfPCell.Rowspan == j + 1)
                {
                    num2 = pdfPCell.GetMaxHeight();
                    while (j > 0)
                    {
                        num2 -= GetRowHeight(idx - j);
                        j--;
                    }
                }

                if (num2 > num)
                {
                    num = num2;
                }
            }

            pdfPRow.MaxHeights = num;
            return num;
        }

        public virtual float GetRowspanHeight(int rowIndex, int cellIndex)
        {
            if (totalWidth <= 0f || rowIndex < 0 || rowIndex >= rows.Count)
            {
                return 0f;
            }

            PdfPRow pdfPRow = rows[rowIndex];
            if (pdfPRow == null || cellIndex >= pdfPRow.GetCells().Length)
            {
                return 0f;
            }

            PdfPCell pdfPCell = pdfPRow.GetCells()[cellIndex];
            if (pdfPCell == null)
            {
                return 0f;
            }

            float num = 0f;
            for (int i = 0; i < pdfPCell.Rowspan; i++)
            {
                num += GetRowHeight(rowIndex + i);
            }

            return num;
        }

        public virtual bool HasRowspan(int rowIdx)
        {
            if (rowIdx < rows.Count && GetRow(rowIdx).HasRowspan())
            {
                return true;
            }

            PdfPRow pdfPRow = ((rowIdx > 0) ? GetRow(rowIdx - 1) : null);
            if (pdfPRow != null && pdfPRow.HasRowspan())
            {
                return true;
            }

            for (int i = 0; i < NumberOfColumns; i++)
            {
                if (RowSpanAbove(rowIdx - 1, i))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void NormalizeHeadersFooters()
        {
            if (footerRows > headerRows)
            {
                footerRows = headerRows;
            }
        }

        public virtual bool DeleteRow(int rowNumber)
        {
            if (rowNumber < 0 || rowNumber >= rows.Count)
            {
                return false;
            }

            if (totalWidth > 0f)
            {
                PdfPRow pdfPRow = rows[rowNumber];
                if (pdfPRow != null)
                {
                    totalHeight -= pdfPRow.MaxHeights;
                }
            }

            rows.RemoveAt(rowNumber);
            if (rowNumber < headerRows)
            {
                headerRows--;
                if (rowNumber >= headerRows - footerRows)
                {
                    footerRows--;
                }
            }

            return true;
        }

        public virtual bool DeleteLastRow()
        {
            return DeleteRow(rows.Count - 1);
        }

        public virtual void DeleteBodyRows()
        {
            List<PdfPRow> list = new List<PdfPRow>();
            for (int i = 0; i < headerRows; i++)
            {
                list.Add(rows[i]);
            }

            rows = list;
            totalHeight = 0f;
            if (totalWidth > 0f)
            {
                totalHeight = HeaderHeight;
            }
        }

        public virtual bool IsContent()
        {
            return true;
        }

        public virtual bool IsNestable()
        {
            return true;
        }

        public virtual bool Process(IElementListener listener)
        {
            try
            {
                return listener.Add(this);
            }
            catch (DocumentException)
            {
                return false;
            }
        }

        public virtual PdfPRow GetRow(int idx)
        {
            return rows[idx];
        }

        public virtual int getLastCompletedRowIndex()
        {
            return rows.Count - 1;
        }

        public virtual void SetBreakPoints(int[] breakPoints)
        {
            KeepRowsTogether(0, rows.Count);
            for (int i = 0; i < breakPoints.Length; i++)
            {
                GetRow(breakPoints[i]).MayNotBreak = false;
            }
        }

        public virtual void KeepRowsTogether(int[] rows)
        {
            for (int i = 0; i < rows.Length; i++)
            {
                GetRow(rows[i]).MayNotBreak = true;
            }
        }

        public virtual void KeepRowsTogether(int start, int end)
        {
            if (start < end)
            {
                while (start < end)
                {
                    GetRow(start).MayNotBreak = true;
                    start++;
                }
            }
        }

        public virtual void KeepRowsTogether(int start)
        {
            KeepRowsTogether(start, rows.Count);
        }

        public virtual List<PdfPRow> GetRows(int start, int end)
        {
            List<PdfPRow> list = new List<PdfPRow>();
            if (start < 0 || end > Size)
            {
                return list;
            }

            for (int i = start; i < end; i++)
            {
                list.Add(AdjustCellsInRow(i, end));
            }

            return list;
        }

        protected virtual PdfPRow AdjustCellsInRow(int start, int end)
        {
            PdfPRow row = GetRow(start);
            if (row.Adjusted)
            {
                return row;
            }

            row = new PdfPRow(row);
            PdfPCell[] cells = row.GetCells();
            for (int i = 0; i < cells.Length; i++)
            {
                PdfPCell pdfPCell = cells[i];
                if (pdfPCell != null && pdfPCell.Rowspan != 1)
                {
                    int num = Math.Min(end, start + pdfPCell.Rowspan);
                    float num2 = 0f;
                    for (int j = start + 1; j < num; j++)
                    {
                        num2 += GetRow(j).MaxHeights;
                    }

                    row.SetExtraHeight(i, num2);
                }
            }

            row.Adjusted = true;
            return row;
        }

        internal float[][] GetEventWidths(float xPos, int firstRow, int lastRow, bool includeHeaders)
        {
            if (includeHeaders)
            {
                firstRow = Math.Max(firstRow, headerRows);
                lastRow = Math.Max(lastRow, headerRows);
            }

            float[][] array = new float[(includeHeaders ? headerRows : 0) + lastRow - firstRow][];
            if (isColspan)
            {
                int num = 0;
                if (includeHeaders)
                {
                    for (int i = 0; i < headerRows; i++)
                    {
                        PdfPRow pdfPRow = rows[i];
                        if (pdfPRow == null)
                        {
                            num++;
                        }
                        else
                        {
                            array[num++] = pdfPRow.GetEventWidth(xPos, absoluteWidths);
                        }
                    }
                }

                while (firstRow < lastRow)
                {
                    PdfPRow pdfPRow2 = rows[firstRow];
                    if (pdfPRow2 == null)
                    {
                        num++;
                    }
                    else
                    {
                        array[num++] = pdfPRow2.GetEventWidth(xPos, absoluteWidths);
                    }

                    firstRow++;
                }
            }
            else
            {
                int numberOfColumns = NumberOfColumns;
                float[] array2 = new float[numberOfColumns + 1];
                array2[0] = xPos;
                for (int j = 0; j < numberOfColumns; j++)
                {
                    array2[j + 1] = array2[j] + absoluteWidths[j];
                }

                for (int k = 0; k < array.Length; k++)
                {
                    array[k] = array2;
                }
            }

            return array;
        }

        public virtual void SetExtendLastRow(bool extendLastRows, bool extendFinalRow)
        {
            extendLastRow[0] = extendLastRows;
            extendLastRow[1] = extendFinalRow;
        }

        public virtual bool IsExtendLastRow(bool newPageFollows)
        {
            if (newPageFollows)
            {
                return extendLastRow[0];
            }

            return extendLastRow[1];
        }

        public virtual void CompleteRow()
        {
            while (!rowCompleted)
            {
                AddCell(defaultCell);
            }
        }

        public virtual void FlushContent()
        {
            DeleteBodyRows();
            if (numberOfWrittenRows > 0)
            {
                SkipFirstHeader = true;
            }
        }

        internal virtual void AddNumberOfRowsWritten(int numberOfWrittenRows)
        {
            this.numberOfWrittenRows += numberOfWrittenRows;
        }

        public virtual PdfObject GetAccessibleAttribute(PdfName key)
        {
            if (accessibleAttributes != null)
            {
                accessibleAttributes.TryGetValue(key, out var value);
                return value;
            }

            return null;
        }

        public virtual void SetAccessibleAttribute(PdfName key, PdfObject value)
        {
            if (accessibleAttributes == null)
            {
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            }

            accessibleAttributes[key] = value;
        }

        public virtual Dictionary<PdfName, PdfObject> GetAccessibleAttributes()
        {
            return accessibleAttributes;
        }

        public virtual PdfPTableHeader GetHeader()
        {
            if (header == null)
            {
                header = new PdfPTableHeader();
            }

            return header;
        }

        public virtual PdfPTableBody GetBody()
        {
            if (body == null)
            {
                body = new PdfPTableBody();
            }

            return body;
        }

        public virtual PdfPTableFooter GetFooter()
        {
            if (footer == null)
            {
                footer = new PdfPTableFooter();
            }

            return footer;
        }

        public virtual int GetCellStartRowIndex(int rowIdx, int colIdx)
        {
            int num = rowIdx;
            while (GetRow(num).GetCells()[colIdx] == null && num > 0)
            {
                num--;
            }

            return num;
        }

        public virtual FittingRows GetFittingRows(float availableHeight, int startIdx)
        {
            LOGGER.Info($"GetFittingRows({availableHeight}, {startIdx})");
            int numberOfColumns = NumberOfColumns;
            ColumnMeasurementState[] array = new ColumnMeasurementState[numberOfColumns];
            for (int i = 0; i < numberOfColumns; i++)
            {
                array[i] = new ColumnMeasurementState();
            }

            float num = 0f;
            float height = 0f;
            Dictionary<int, float> dictionary = new Dictionary<int, float>();
            int j;
            for (j = startIdx; j < Size; j++)
            {
                PdfPRow row = GetRow(j);
                float maxRowHeightsWithoutCalculating = row.GetMaxRowHeightsWithoutCalculating();
                float num2 = 0f;
                ColumnMeasurementState columnMeasurementState;
                for (int k = 0; k < numberOfColumns; k += columnMeasurementState.colspan)
                {
                    PdfPCell pdfPCell = row.GetCells()[k];
                    columnMeasurementState = array[k];
                    if (pdfPCell == null)
                    {
                        columnMeasurementState.ConsumeRowspan(num, maxRowHeightsWithoutCalculating);
                    }
                    else
                    {
                        columnMeasurementState.BeginCell(pdfPCell, num, maxRowHeightsWithoutCalculating);
                        LOGGER.Info($"Height after BeginCell: {columnMeasurementState.height} (cell: {pdfPCell.GetMaxHeight()})");
                    }

                    if (columnMeasurementState.CellEnds() && columnMeasurementState.height > num2)
                    {
                        num2 = columnMeasurementState.height;
                    }

                    for (int l = 1; l < columnMeasurementState.colspan; l++)
                    {
                        array[k + l].height = columnMeasurementState.height;
                    }
                }

                float num3 = 0f;
                ColumnMeasurementState[] array2 = array;
                foreach (ColumnMeasurementState columnMeasurementState2 in array2)
                {
                    if (columnMeasurementState2.height > num3)
                    {
                        num3 = columnMeasurementState2.height;
                    }
                }

                row.SetFinalMaxHeights(num2 - num);
                if (availableHeight - (SplitLate ? num3 : num2) < 0f)
                {
                    break;
                }

                dictionary[j] = num3 - num;
                num = num2;
                height = num3;
            }

            rowsNotChecked = false;
            return new FittingRows(startIdx, j - 1, height, num, dictionary);
        }
    }
}
