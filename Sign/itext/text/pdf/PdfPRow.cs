using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;
using Sign.itext.text.log;

namespace Sign.itext.text.pdf
{
    public class PdfPRow : IAccessibleElement
    {
        private readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(PdfPTable));

        public bool mayNotBreak;

        public const float BOTTOM_LIMIT = -1.07374182E+09f;

        public const float RIGHT_LIMIT = 20000f;

        protected PdfPCell[] cells;

        protected float[] widths;

        protected float[] extraHeights;

        protected internal float maxHeight;

        protected internal bool calculated;

        protected bool adjusted;

        private int[] canvasesPos;

        protected PdfName role = PdfName.TR;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes;

        protected AccessibleElementId id = new AccessibleElementId();

        public virtual bool MayNotBreak
        {
            get
            {
                return mayNotBreak;
            }
            set
            {
                mayNotBreak = value;
            }
        }

        public virtual float MaxHeights
        {
            get
            {
                if (!calculated)
                {
                    CalculateHeights();
                }

                return maxHeight;
            }
            set
            {
                maxHeight = value;
            }
        }

        public virtual bool Adjusted
        {
            get
            {
                return adjusted;
            }
            set
            {
                adjusted = value;
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

        public PdfPRow(PdfPCell[] cells)
            : this(cells, null)
        {
        }

        public PdfPRow(PdfPCell[] cells, PdfPRow source)
        {
            this.cells = cells;
            widths = new float[cells.Length];
            InitExtraHeights();
            if (source != null)
            {
                id = source.ID;
                role = source.Role;
                if (source.accessibleAttributes != null)
                {
                    accessibleAttributes = new Dictionary<PdfName, PdfObject>(source.GetAccessibleAttributes());
                }
            }
        }

        public PdfPRow(PdfPRow row)
        {
            mayNotBreak = row.mayNotBreak;
            maxHeight = row.maxHeight;
            calculated = row.calculated;
            cells = new PdfPCell[row.cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                if (row.cells[i] != null)
                {
                    if (row.cells[i] is PdfPHeaderCell)
                    {
                        cells[i] = new PdfPHeaderCell((PdfPHeaderCell)row.cells[i]);
                    }
                    else
                    {
                        cells[i] = new PdfPCell(row.cells[i]);
                    }
                }
            }

            widths = new float[cells.Length];
            Array.Copy(row.widths, 0, widths, 0, cells.Length);
            InitExtraHeights();
            id = row.ID;
            role = row.Role;
            if (row.accessibleAttributes != null)
            {
                accessibleAttributes = new Dictionary<PdfName, PdfObject>(row.GetAccessibleAttributes());
            }
        }

        public virtual bool SetWidths(float[] widths)
        {
            if (widths.Length != cells.Length)
            {
                return false;
            }

            Array.Copy(widths, 0, this.widths, 0, cells.Length);
            float num = 0f;
            calculated = false;
            for (int i = 0; i < widths.Length; i++)
            {
                PdfPCell pdfPCell = cells[i];
                if (pdfPCell == null)
                {
                    num += widths[i];
                    continue;
                }

                pdfPCell.Left = num;
                for (int num2 = i + pdfPCell.Colspan; i < num2; i++)
                {
                    num += widths[i];
                }

                i--;
                pdfPCell.Right = num;
                pdfPCell.Top = 0f;
            }

            return true;
        }

        protected internal virtual void InitExtraHeights()
        {
            extraHeights = new float[cells.Length];
            for (int i = 0; i < extraHeights.Length; i++)
            {
                extraHeights[i] = 0f;
            }
        }

        public virtual void SetExtraHeight(int cell, float height)
        {
            if (cell >= 0 && cell < cells.Length)
            {
                extraHeights[cell] = height;
            }
        }

        protected internal virtual void CalculateHeights()
        {
            maxHeight = 0f;
            LOGGER.Info("CalculateHeights");
            for (int i = 0; i < cells.Length; i++)
            {
                PdfPCell pdfPCell = cells[i];
                float num = 0f;
                if (pdfPCell != null)
                {
                    num = (pdfPCell.HasCalculatedHeight() ? pdfPCell.CalculatedHeight : pdfPCell.GetMaxHeight());
                    if (num > maxHeight && pdfPCell.Rowspan == 1)
                    {
                        maxHeight = num;
                    }
                }
            }

            calculated = true;
        }

        public virtual void WriteBorderAndBackground(float xPos, float yPos, float currentMaxHeight, PdfPCell cell, PdfContentByte[] canvases)
        {
            BaseColor backgroundColor = cell.BackgroundColor;
            if (backgroundColor != null || cell.HasBorders())
            {
                float num = cell.Right + xPos;
                float num2 = cell.Top + yPos;
                float num3 = cell.Left + xPos;
                float num4 = num2 - currentMaxHeight;
                if (backgroundColor != null)
                {
                    PdfContentByte obj = canvases[1];
                    obj.SetColorFill(backgroundColor);
                    obj.Rectangle(num3, num4, num - num3, num2 - num4);
                    obj.Fill();
                }

                if (cell.HasBorders())
                {
                    Rectangle rectangle = new Rectangle(num3, num4, num, num2);
                    rectangle.CloneNonPositionParameters(cell);
                    rectangle.BackgroundColor = null;
                    canvases[2].Rectangle(rectangle);
                }
            }
        }

        protected virtual void SaveAndRotateCanvases(PdfContentByte[] canvases, float a, float b, float c, float d, float e, float f)
        {
            int num = 4;
            if (canvasesPos == null)
            {
                canvasesPos = new int[num * 2];
            }

            for (int i = 0; i < num; i++)
            {
                ByteBuffer internalBuffer = canvases[i].InternalBuffer;
                canvasesPos[i * 2] = internalBuffer.Size;
                canvases[i].SaveState();
                canvases[i].ConcatCTM(a, b, c, d, e, f);
                canvasesPos[i * 2 + 1] = internalBuffer.Size;
            }
        }

        protected virtual void RestoreCanvases(PdfContentByte[] canvases)
        {
            int num = 4;
            for (int i = 0; i < num; i++)
            {
                ByteBuffer internalBuffer = canvases[i].InternalBuffer;
                int size = internalBuffer.Size;
                canvases[i].RestoreState();
                if (size == canvasesPos[i * 2 + 1])
                {
                    internalBuffer.Size = canvasesPos[i * 2];
                }
            }
        }

        public static float SetColumn(ColumnText ct, float left, float bottom, float right, float top)
        {
            if (left > right)
            {
                right = left;
            }

            if (bottom > top)
            {
                top = bottom;
            }

            ct.SetSimpleColumn(left, bottom, right, top);
            return top;
        }

        public virtual void WriteCells(int colStart, int colEnd, float xPos, float yPos, PdfContentByte[] canvases, bool reusable)
        {
            if (!calculated)
            {
                CalculateHeights();
            }

            colEnd = ((colEnd >= 0) ? Math.Min(colEnd, cells.Length) : cells.Length);
            if (colStart < 0)
            {
                colStart = 0;
            }

            if (colStart >= colEnd)
            {
                return;
            }

            int num = colStart;
            while (num >= 0 && cells[num] == null)
            {
                if (num > 0)
                {
                    xPos -= widths[num - 1];
                }

                num--;
            }

            if (num < 0)
            {
                num = 0;
            }

            if (cells[num] != null)
            {
                xPos -= cells[num].Left;
            }

            if (IsTagged(canvases[3]))
            {
                canvases[3].OpenMCBlock(this);
            }

            for (int i = num; i < colEnd; i++)
            {
                PdfPCell pdfPCell = cells[i];
                if (pdfPCell == null)
                {
                    continue;
                }

                if (IsTagged(canvases[3]))
                {
                    canvases[3].OpenMCBlock(pdfPCell);
                }

                float num2 = maxHeight + extraHeights[i];
                WriteBorderAndBackground(xPos, yPos, num2, pdfPCell, canvases);
                Image image = pdfPCell.Image;
                float num3 = pdfPCell.Top + yPos - pdfPCell.EffectivePaddingTop;
                if (pdfPCell.Height <= num2)
                {
                    switch (pdfPCell.VerticalAlignment)
                    {
                        case 6:
                            num3 = pdfPCell.Top + yPos - num2 + pdfPCell.Height - pdfPCell.EffectivePaddingTop;
                            break;
                        case 5:
                            num3 = pdfPCell.Top + yPos + (pdfPCell.Height - num2) / 2f - pdfPCell.EffectivePaddingTop;
                            break;
                    }
                }

                if (image != null)
                {
                    if (pdfPCell.Rotation != 0)
                    {
                        image = Image.GetInstance(image);
                        image.Rotation = image.GetImageRotation() + (float)((double)pdfPCell.Rotation * Math.PI / 180.0);
                    }

                    bool flag = false;
                    if (pdfPCell.Height > num2)
                    {
                        if (!image.ScaleToFitHeight)
                        {
                            continue;
                        }

                        image.ScalePercent(100f);
                        float num4 = (num2 - pdfPCell.EffectivePaddingTop - pdfPCell.EffectivePaddingBottom) / image.ScaledHeight;
                        image.ScalePercent(num4 * 100f);
                        flag = true;
                    }

                    float absoluteX = pdfPCell.Left + xPos + pdfPCell.EffectivePaddingLeft;
                    if (flag)
                    {
                        switch (pdfPCell.HorizontalAlignment)
                        {
                            case 1:
                                absoluteX = xPos + (pdfPCell.Left + pdfPCell.EffectivePaddingLeft + pdfPCell.Right - pdfPCell.EffectivePaddingRight - image.ScaledWidth) / 2f;
                                break;
                            case 2:
                                absoluteX = xPos + pdfPCell.Right - pdfPCell.EffectivePaddingRight - image.ScaledWidth;
                                break;
                        }

                        num3 = pdfPCell.Top + yPos - pdfPCell.EffectivePaddingTop;
                    }

                    image.SetAbsolutePosition(absoluteX, num3 - image.ScaledHeight);
                    if (IsTagged(canvases[3]))
                    {
                        canvases[3].OpenMCBlock(image);
                    }

                    canvases[3].AddImage(image);
                    if (IsTagged(canvases[3]))
                    {
                        canvases[3].CloseMCBlock(image);
                    }
                }
                else if (pdfPCell.Rotation == 90 || pdfPCell.Rotation == 270)
                {
                    float num5 = num2 - pdfPCell.EffectivePaddingTop - pdfPCell.EffectivePaddingBottom;
                    float num6 = pdfPCell.Width - pdfPCell.EffectivePaddingLeft - pdfPCell.EffectivePaddingRight;
                    ColumnText columnText = ColumnText.Duplicate(pdfPCell.Column);
                    columnText.Canvases = canvases;
                    columnText.SetSimpleColumn(0f, 0f, num5 + 0.001f, 0f - num6);
                    columnText.Go(simulate: true);
                    float num7 = 0f - columnText.YLine;
                    if (num5 <= 0f || num6 <= 0f)
                    {
                        num7 = 0f;
                    }

                    if (num7 > 0f)
                    {
                        if (pdfPCell.UseDescender)
                        {
                            num7 -= columnText.Descender;
                        }

                        columnText = ((!reusable) ? pdfPCell.Column : ColumnText.Duplicate(pdfPCell.Column));
                        columnText.Canvases = canvases;
                        columnText.SetSimpleColumn(-0.003f, -0.001f, num5 + 0.003f, num7);
                        if (pdfPCell.Rotation == 90)
                        {
                            float f = pdfPCell.Top + yPos - num2 + pdfPCell.EffectivePaddingBottom;
                            SaveAndRotateCanvases(canvases, 0f, 1f, -1f, 0f, pdfPCell.VerticalAlignment switch
                            {
                                6 => pdfPCell.Left + xPos + pdfPCell.Width - pdfPCell.EffectivePaddingRight,
                                5 => pdfPCell.Left + xPos + (pdfPCell.Width + pdfPCell.EffectivePaddingLeft - pdfPCell.EffectivePaddingRight + num7) / 2f,
                                _ => pdfPCell.Left + xPos + pdfPCell.EffectivePaddingLeft + num7,
                            }, f);
                        }
                        else
                        {
                            float f = pdfPCell.Top + yPos - pdfPCell.EffectivePaddingTop;
                            SaveAndRotateCanvases(canvases, 0f, -1f, 1f, 0f, pdfPCell.VerticalAlignment switch
                            {
                                6 => pdfPCell.Left + xPos + pdfPCell.EffectivePaddingLeft,
                                5 => pdfPCell.Left + xPos + (pdfPCell.Width + pdfPCell.EffectivePaddingLeft - pdfPCell.EffectivePaddingRight - num7) / 2f,
                                _ => pdfPCell.Left + xPos + pdfPCell.Width - pdfPCell.EffectivePaddingRight - num7,
                            }, f);
                        }

                        try
                        {
                            columnText.Go();
                        }
                        finally
                        {
                            RestoreCanvases(canvases);
                        }
                    }
                }
                else
                {
                    float fixedHeight = pdfPCell.FixedHeight;
                    float num8 = pdfPCell.Right + xPos - pdfPCell.EffectivePaddingRight;
                    float num9 = pdfPCell.Left + xPos + pdfPCell.EffectivePaddingLeft;
                    if (pdfPCell.NoWrap)
                    {
                        switch (pdfPCell.HorizontalAlignment)
                        {
                            case 1:
                                num8 += 10000f;
                                num9 -= 10000f;
                                break;
                            case 2:
                                if (pdfPCell.Rotation == 180)
                                {
                                    num8 += 20000f;
                                }
                                else
                                {
                                    num9 -= 20000f;
                                }

                                break;
                            default:
                                if (pdfPCell.Rotation == 180)
                                {
                                    num9 -= 20000f;
                                }
                                else
                                {
                                    num8 += 20000f;
                                }

                                break;
                        }
                    }

                    ColumnText columnText2 = ((!reusable) ? pdfPCell.Column : ColumnText.Duplicate(pdfPCell.Column));
                    columnText2.Canvases = canvases;
                    float num10 = num3 - (num2 - pdfPCell.EffectivePaddingTop - pdfPCell.EffectivePaddingBottom);
                    if (fixedHeight > 0f && pdfPCell.Height > num2)
                    {
                        num3 = pdfPCell.Top + yPos - pdfPCell.EffectivePaddingTop;
                        num10 = pdfPCell.Top + yPos - num2 + pdfPCell.EffectivePaddingBottom;
                    }

                    if ((num3 > num10 || columnText2.ZeroHeightElement()) && num9 < num8)
                    {
                        columnText2.SetSimpleColumn(num9, num10 - 0.001f, num8, num3);
                        if (pdfPCell.Rotation == 180)
                        {
                            float e = num9 + num8;
                            float f2 = yPos + yPos - num2 + pdfPCell.EffectivePaddingBottom - pdfPCell.EffectivePaddingTop;
                            SaveAndRotateCanvases(canvases, -1f, 0f, 0f, -1f, e, f2);
                        }

                        try
                        {
                            columnText2.Go();
                        }
                        finally
                        {
                            if (pdfPCell.Rotation == 180)
                            {
                                RestoreCanvases(canvases);
                            }
                        }
                    }
                }

                IPdfPCellEvent cellEvent = pdfPCell.CellEvent;
                if (cellEvent != null)
                {
                    Rectangle position = new Rectangle(pdfPCell.Left + xPos, pdfPCell.Top + yPos - num2, pdfPCell.Right + xPos, pdfPCell.Top + yPos);
                    cellEvent.CellLayout(pdfPCell, position, canvases);
                }

                if (IsTagged(canvases[3]))
                {
                    canvases[3].CloseMCBlock(pdfPCell);
                }
            }

            if (IsTagged(canvases[3]))
            {
                canvases[3].CloseMCBlock(this);
            }
        }

        public virtual bool IsCalculated()
        {
            return calculated;
        }

        internal float[] GetEventWidth(float xPos, float[] absoluteWidths)
        {
            int num = 1;
            int i = 0;
            while (i < cells.Length)
            {
                if (cells[i] != null)
                {
                    num++;
                    i += cells[i].Colspan;
                }
                else
                {
                    for (; i < cells.Length && cells[i] == null; i++)
                    {
                        num++;
                    }
                }
            }

            float[] array = new float[num];
            array[0] = xPos;
            num = 1;
            int num2 = 0;
            while (num2 < cells.Length && num < array.Length)
            {
                if (cells[num2] != null)
                {
                    int colspan = cells[num2].Colspan;
                    array[num] = array[num - 1];
                    for (int j = 0; j < colspan; j++)
                    {
                        if (num2 >= absoluteWidths.Length)
                        {
                            break;
                        }

                        array[num] += absoluteWidths[num2++];
                    }

                    num++;
                }
                else
                {
                    array[num] = array[num - 1];
                    while (num2 < cells.Length && cells[num2] == null)
                    {
                        array[num] += absoluteWidths[num2++];
                    }

                    num++;
                }
            }

            return array;
        }

        public virtual void CopyRowContent(PdfPTable table, int idx)
        {
            if (table == null)
            {
                return;
            }

            for (int i = 0; i < cells.Length; i++)
            {
                int num = idx;
                PdfPCell pdfPCell = table.GetRow(num).GetCells()[i];
                while (pdfPCell == null && num > 0)
                {
                    pdfPCell = table.GetRow(--num).GetCells()[i];
                }

                if (cells[i] != null && pdfPCell != null)
                {
                    cells[i].Column = pdfPCell.Column;
                    calculated = false;
                }
            }
        }

        public virtual PdfPRow SplitRow(PdfPTable table, int rowIndex, float new_height)
        {
            LOGGER.Info("Splitting " + rowIndex + " " + new_height);
            PdfPCell[] array = new PdfPCell[cells.Length];
            float[] array2 = new float[cells.Length];
            float[] array3 = new float[cells.Length];
            float[] array4 = new float[cells.Length];
            bool flag = true;
            for (int i = 0; i < cells.Length; i++)
            {
                float num = new_height;
                PdfPCell pdfPCell = cells[i];
                if (pdfPCell == null)
                {
                    int num2 = rowIndex;
                    if (table.RowSpanAbove(num2, i))
                    {
                        while (table.RowSpanAbove(--num2, i))
                        {
                            num += table.GetRow(num2).MaxHeights;
                        }

                        PdfPRow row = table.GetRow(num2);
                        if (row != null && row.GetCells()[i] != null)
                        {
                            array[i] = new PdfPCell(row.GetCells()[i]);
                            array[i].Column = null;
                            array[i].Rowspan = row.GetCells()[i].Rowspan - rowIndex + num2;
                            flag = false;
                        }
                    }

                    continue;
                }

                array2[i] = pdfPCell.CalculatedHeight;
                array3[i] = pdfPCell.FixedHeight;
                array4[i] = pdfPCell.MinimumHeight;
                Image image = pdfPCell.Image;
                PdfPCell pdfPCell2 = new PdfPCell(pdfPCell);
                if (image != null)
                {
                    float num3 = pdfPCell.EffectivePaddingBottom + pdfPCell.EffectivePaddingTop + 2f;
                    if ((image.ScaleToFitHeight || image.ScaledHeight + num3 < num) && num > num3)
                    {
                        pdfPCell2.Phrase = null;
                        flag = false;
                    }
                }
                else
                {
                    ColumnText columnText = ColumnText.Duplicate(pdfPCell.Column);
                    float num4 = pdfPCell.Left + pdfPCell.EffectivePaddingLeft;
                    float num5 = pdfPCell.Top + pdfPCell.EffectivePaddingBottom - num;
                    float num6 = pdfPCell.Right - pdfPCell.EffectivePaddingRight;
                    float num7 = pdfPCell.Top - pdfPCell.EffectivePaddingTop;
                    int rotation = pdfPCell.Rotation;
                    float num8 = ((rotation != 90 && rotation != 270) ? SetColumn(columnText, num4, num5 + 1E-05f, pdfPCell.NoWrap ? 20000f : num6, num7) : SetColumn(columnText, num5, num4, num7, num6));
                    int num9 = columnText.Go(simulate: true);
                    bool flag2 = columnText.YLine == num8;
                    if (flag2)
                    {
                        pdfPCell2.Column = ColumnText.Duplicate(pdfPCell.Column);
                        columnText.FilledWidth = 0f;
                    }
                    else if ((num9 & 1) == 0)
                    {
                        pdfPCell2.Column = columnText;
                        columnText.FilledWidth = 0f;
                    }
                    else
                    {
                        pdfPCell2.Phrase = null;
                    }

                    flag = flag && flag2;
                }

                array[i] = pdfPCell2;
                pdfPCell.CalculatedHeight = num;
            }

            if (flag)
            {
                for (int j = 0; j < cells.Length; j++)
                {
                    PdfPCell pdfPCell3 = cells[j];
                    if (pdfPCell3 != null)
                    {
                        pdfPCell3.CalculatedHeight = array2[j];
                        if (array3[j] > 0f)
                        {
                            pdfPCell3.FixedHeight = array3[j];
                        }
                        else
                        {
                            pdfPCell3.MinimumHeight = array4[j];
                        }
                    }
                }

                return null;
            }

            CalculateHeights();
            return new PdfPRow(array, this)
            {
                widths = (float[])widths.Clone()
            };
        }

        public virtual float GetMaxRowHeightsWithoutCalculating()
        {
            return maxHeight;
        }

        public virtual void SetFinalMaxHeights(float maxHeight)
        {
            MaxHeights = maxHeight;
            calculated = true;
        }

        public virtual void SplitRowspans(PdfPTable original, int originalIdx, PdfPTable part, int partIdx)
        {
            if (original == null || part == null)
            {
                return;
            }

            int num = 0;
            while (num < cells.Length)
            {
                if (cells[num] == null)
                {
                    int cellStartRowIndex = original.GetCellStartRowIndex(originalIdx, num);
                    int cellStartRowIndex2 = part.GetCellStartRowIndex(partIdx, num);
                    PdfPCell pdfPCell = original.GetRow(cellStartRowIndex).GetCells()[num];
                    PdfPCell pdfPCell2 = part.GetRow(cellStartRowIndex2).GetCells()[num];
                    if (pdfPCell != null)
                    {
                        cells[num] = new PdfPCell(pdfPCell2);
                        int num2 = partIdx - cellStartRowIndex2 + 1;
                        cells[num].Rowspan = pdfPCell2.Rowspan - num2;
                        pdfPCell.Rowspan = num2;
                        calculated = false;
                    }

                    num++;
                }
                else
                {
                    num += cells[num].Colspan;
                }
            }
        }

        public virtual PdfPCell[] GetCells()
        {
            return cells;
        }

        public virtual bool HasRowspan()
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null && cells[i].Rowspan > 1)
                {
                    return true;
                }
            }

            return false;
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

        private static bool IsTagged(PdfContentByte canvas)
        {
            if (canvas != null && canvas.writer != null)
            {
                return canvas.writer.IsTagged();
            }

            return false;
        }
    }
}
