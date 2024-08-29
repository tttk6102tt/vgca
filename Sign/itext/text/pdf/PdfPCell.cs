using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;
using Sign.itext.text.pdf.events;

namespace Sign.itext.text.pdf
{
    public class PdfPCell : Rectangle, IAccessibleElement
    {
        private ColumnText column = new ColumnText(null);

        private int verticalAlignment = 4;

        private float paddingLeft = 2f;

        private float paddingRight = 2f;

        private float paddingTop = 2f;

        private float paddingBottom = 2f;

        private float fixedHeight;

        private float calculatedHeight;

        private bool noWrap;

        private PdfPTable table;

        private float minimumHeight;

        private int colspan = 1;

        private int rowspan = 1;

        private Image image;

        private IPdfPCellEvent cellEvent;

        private bool useDescender;

        private bool useBorderPadding;

        protected Phrase phrase;

        private new int rotation;

        protected PdfName role = PdfName.TD;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes;

        protected internal AccessibleElementId id = new AccessibleElementId();

        protected List<PdfPHeaderCell> headers;

        public virtual Phrase Phrase
        {
            get
            {
                return phrase;
            }
            set
            {
                table = null;
                image = null;
                column.SetText(phrase = value);
            }
        }

        public virtual int HorizontalAlignment
        {
            get
            {
                return column.Alignment;
            }
            set
            {
                column.Alignment = value;
            }
        }

        public virtual int VerticalAlignment
        {
            get
            {
                return verticalAlignment;
            }
            set
            {
                verticalAlignment = value;
                if (table != null)
                {
                    table.ExtendLastRow = verticalAlignment == 4;
                }
            }
        }

        public virtual float EffectivePaddingLeft
        {
            get
            {
                if (UseBorderPadding)
                {
                    float num = BorderWidthLeft / (UseVariableBorders ? 1f : 2f);
                    return paddingLeft + num;
                }

                return paddingLeft;
            }
        }

        public virtual float PaddingLeft
        {
            get
            {
                return paddingLeft;
            }
            set
            {
                paddingLeft = value;
            }
        }

        public virtual float EffectivePaddingRight
        {
            get
            {
                if (UseBorderPadding)
                {
                    float num = BorderWidthRight / (UseVariableBorders ? 1f : 2f);
                    return paddingRight + num;
                }

                return paddingRight;
            }
        }

        public virtual float PaddingRight
        {
            get
            {
                return paddingRight;
            }
            set
            {
                paddingRight = value;
            }
        }

        public virtual float EffectivePaddingTop
        {
            get
            {
                if (UseBorderPadding)
                {
                    float num = BorderWidthTop / (UseVariableBorders ? 1f : 2f);
                    return paddingTop + num;
                }

                return paddingTop;
            }
        }

        public virtual float PaddingTop
        {
            get
            {
                return paddingTop;
            }
            set
            {
                paddingTop = value;
            }
        }

        public virtual float EffectivePaddingBottom
        {
            get
            {
                if (UseBorderPadding)
                {
                    float num = BorderWidthBottom / (UseVariableBorders ? 1f : 2f);
                    return paddingBottom + num;
                }

                return paddingBottom;
            }
        }

        public virtual float PaddingBottom
        {
            get
            {
                return paddingBottom;
            }
            set
            {
                paddingBottom = value;
            }
        }

        public virtual float Padding
        {
            set
            {
                paddingBottom = value;
                paddingTop = value;
                paddingLeft = value;
                paddingRight = value;
            }
        }

        public virtual bool UseBorderPadding
        {
            get
            {
                return useBorderPadding;
            }
            set
            {
                useBorderPadding = value;
            }
        }

        public virtual float Leading => column.Leading;

        public virtual float MultipliedLeading => column.MultipliedLeading;

        public virtual float Indent
        {
            get
            {
                return column.Indent;
            }
            set
            {
                column.Indent = value;
            }
        }

        public virtual float ExtraParagraphSpace
        {
            get
            {
                return column.ExtraParagraphSpace;
            }
            set
            {
                column.ExtraParagraphSpace = value;
            }
        }

        public virtual float CalculatedHeight
        {
            get
            {
                return calculatedHeight;
            }
            set
            {
                calculatedHeight = value;
            }
        }

        public virtual float FixedHeight
        {
            get
            {
                return fixedHeight;
            }
            set
            {
                fixedHeight = value;
                minimumHeight = 0f;
            }
        }

        public virtual bool NoWrap
        {
            get
            {
                return noWrap;
            }
            set
            {
                noWrap = value;
            }
        }

        public virtual PdfPTable Table
        {
            get
            {
                return table;
            }
            set
            {
                table = value;
                column.SetText(null);
                image = null;
                if (table != null)
                {
                    table.ExtendLastRow = verticalAlignment == 4;
                    column.AddElement(table);
                    table.WidthPercentage = 100f;
                }
            }
        }

        public virtual float MinimumHeight
        {
            get
            {
                return minimumHeight;
            }
            set
            {
                minimumHeight = value;
                fixedHeight = 0f;
            }
        }

        public virtual int Colspan
        {
            get
            {
                return colspan;
            }
            set
            {
                colspan = value;
            }
        }

        public virtual int Rowspan
        {
            get
            {
                return rowspan;
            }
            set
            {
                rowspan = value;
            }
        }

        public virtual float FollowingIndent
        {
            get
            {
                return column.FollowingIndent;
            }
            set
            {
                column.FollowingIndent = value;
            }
        }

        public virtual float RightIndent
        {
            get
            {
                return column.RightIndent;
            }
            set
            {
                column.RightIndent = value;
            }
        }

        public virtual float SpaceCharRatio
        {
            get
            {
                return column.SpaceCharRatio;
            }
            set
            {
                column.SpaceCharRatio = value;
            }
        }

        public virtual int RunDirection
        {
            get
            {
                return column.RunDirection;
            }
            set
            {
                column.RunDirection = value;
            }
        }

        public virtual Image Image
        {
            get
            {
                return image;
            }
            set
            {
                column.SetText(null);
                table = null;
                image = value;
            }
        }

        public virtual IPdfPCellEvent CellEvent
        {
            get
            {
                return cellEvent;
            }
            set
            {
                if (value == null)
                {
                    cellEvent = null;
                    return;
                }

                if (cellEvent == null)
                {
                    cellEvent = value;
                    return;
                }

                if (cellEvent is PdfPCellEventForwarder)
                {
                    ((PdfPCellEventForwarder)cellEvent).AddCellEvent(value);
                    return;
                }

                PdfPCellEventForwarder pdfPCellEventForwarder = new PdfPCellEventForwarder();
                pdfPCellEventForwarder.AddCellEvent(cellEvent);
                pdfPCellEventForwarder.AddCellEvent(value);
                cellEvent = pdfPCellEventForwarder;
            }
        }

        public virtual int ArabicOptions
        {
            get
            {
                return column.ArabicOptions;
            }
            set
            {
                column.ArabicOptions = value;
            }
        }

        public virtual bool UseAscender
        {
            get
            {
                return column.UseAscender;
            }
            set
            {
                column.UseAscender = value;
            }
        }

        public virtual bool UseDescender
        {
            get
            {
                return useDescender;
            }
            set
            {
                useDescender = value;
            }
        }

        public virtual ColumnText Column
        {
            get
            {
                return column;
            }
            set
            {
                column = value;
            }
        }

        public virtual List<IElement> CompositeElements => column.compositeElements;

        public new int Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                int num = value % 360;
                if (num < 0)
                {
                    num += 360;
                }

                if (num % 90 != 0)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("rotation.must.be.a.multiple.of.90"));
                }

                rotation = num;
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

        public virtual List<PdfPHeaderCell> Headers => headers;

        public PdfPCell()
            : base(0f, 0f, 0f, 0f)
        {
            borderWidth = 0.5f;
            border = 15;
            column.SetLeading(0f, 1f);
        }

        public PdfPCell(Phrase phrase)
            : base(0f, 0f, 0f, 0f)
        {
            borderWidth = 0.5f;
            border = 15;
            column.AddText(this.phrase = phrase);
            column.SetLeading(0f, 1f);
        }

        public PdfPCell(Image image)
            : this(image, fit: false)
        {
        }

        public PdfPCell(Image image, bool fit)
            : base(0f, 0f, 0f, 0f)
        {
            borderWidth = 0.5f;
            border = 15;
            column.SetLeading(0f, 1f);
            if (fit)
            {
                this.image = image;
                Padding = borderWidth / 2f;
            }
            else
            {
                image.ScaleToFitLineWhenOverflow = false;
                column.AddText(phrase = new Phrase(new Chunk(image, 0f, 0f, changeLeading: true)));
                Padding = 0f;
            }
        }

        public PdfPCell(PdfPTable table)
            : this(table, null)
        {
        }

        public PdfPCell(PdfPTable table, PdfPCell style)
            : base(0f, 0f, 0f, 0f)
        {
            borderWidth = 0.5f;
            border = 15;
            column.SetLeading(0f, 1f);
            this.table = table;
            table.WidthPercentage = 100f;
            table.ExtendLastRow = true;
            column.AddElement(table);
            if (style != null)
            {
                CloneNonPositionParameters(style);
                verticalAlignment = style.verticalAlignment;
                paddingLeft = style.paddingLeft;
                paddingRight = style.paddingRight;
                paddingTop = style.paddingTop;
                paddingBottom = style.paddingBottom;
                colspan = style.colspan;
                rowspan = style.rowspan;
                cellEvent = style.cellEvent;
                useDescender = style.useDescender;
                useBorderPadding = style.useBorderPadding;
                rotation = style.rotation;
            }
            else
            {
                Padding = 0f;
            }
        }

        public PdfPCell(PdfPCell cell)
            : base(cell.llx, cell.lly, cell.urx, cell.ury)
        {
            CloneNonPositionParameters(cell);
            verticalAlignment = cell.verticalAlignment;
            paddingLeft = cell.paddingLeft;
            paddingRight = cell.paddingRight;
            paddingTop = cell.paddingTop;
            paddingBottom = cell.paddingBottom;
            phrase = cell.phrase;
            fixedHeight = cell.fixedHeight;
            minimumHeight = cell.minimumHeight;
            noWrap = cell.noWrap;
            colspan = cell.colspan;
            rowspan = cell.rowspan;
            if (cell.table != null)
            {
                table = new PdfPTable(cell.table);
            }

            image = Image.GetInstance(cell.image);
            cellEvent = cell.cellEvent;
            useDescender = cell.useDescender;
            column = ColumnText.Duplicate(cell.column);
            useBorderPadding = cell.useBorderPadding;
            rotation = cell.rotation;
            id = cell.id;
            role = cell.role;
            if (cell.accessibleAttributes != null)
            {
                accessibleAttributes = new Dictionary<PdfName, PdfObject>(cell.accessibleAttributes);
            }

            headers = cell.headers;
        }

        public virtual void AddElement(IElement element)
        {
            if (table != null)
            {
                table = null;
                column.SetText(null);
            }

            if (element is PdfPTable)
            {
                ((PdfPTable)element).SplitLate = false;
            }
            else if (element is PdfDiv)
            {
                foreach (IElement item in ((PdfDiv)element).Content)
                {
                    if (item is PdfPTable)
                    {
                        ((PdfPTable)item).SplitLate = false;
                    }
                }
            }

            column.AddElement(element);
        }

        public virtual void SetLeading(float fixedLeading, float multipliedLeading)
        {
            column.SetLeading(fixedLeading, multipliedLeading);
        }

        public virtual bool HasCalculatedHeight()
        {
            return calculatedHeight > 0f;
        }

        public virtual bool HasFixedHeight()
        {
            return FixedHeight > 0f;
        }

        public virtual bool HasMinimumHeight()
        {
            return MinimumHeight > 0f;
        }

        public virtual float GetMaxHeight()
        {
            bool flag = Rotation == 90 || Rotation == 270;
            Image image = Image;
            if (image != null)
            {
                image.ScalePercent(100f);
                float num = (flag ? image.ScaledHeight : image.ScaledWidth);
                float num2 = (Right - EffectivePaddingRight - EffectivePaddingLeft - Left) / num;
                image.ScalePercent(num2 * 100f);
                float num3 = (flag ? image.ScaledWidth : image.ScaledHeight);
                Bottom = Top - EffectivePaddingTop - EffectivePaddingBottom - num3;
            }
            else if (flag && HasFixedHeight())
            {
                Bottom = Top - FixedHeight;
            }
            else
            {
                ColumnText columnText = ColumnText.Duplicate(Column);
                float right;
                float top;
                float left;
                float bottom;
                if (flag)
                {
                    right = 20000f;
                    top = Right - EffectivePaddingRight;
                    left = 0f;
                    bottom = Left + EffectivePaddingLeft;
                }
                else
                {
                    right = (NoWrap ? 20000f : (Right - EffectivePaddingRight));
                    top = Top - EffectivePaddingTop;
                    left = Left + EffectivePaddingLeft;
                    bottom = (HasCalculatedHeight() ? (Top + EffectivePaddingBottom - CalculatedHeight) : (-1.07374182E+09f));
                }

                PdfPRow.SetColumn(columnText, left, bottom, right, top);
                columnText.Go(simulate: true);
                if (flag)
                {
                    Bottom = Top - EffectivePaddingTop - EffectivePaddingBottom - columnText.FilledWidth;
                }
                else
                {
                    float num4 = columnText.YLine;
                    if (UseDescender)
                    {
                        num4 += columnText.Descender;
                    }

                    Bottom = num4 - EffectivePaddingBottom;
                }
            }

            float num5 = Height;
            if (num5 == EffectivePaddingTop + EffectivePaddingBottom)
            {
                num5 = 0f;
            }

            if (HasFixedHeight())
            {
                num5 = FixedHeight;
            }
            else if (HasMinimumHeight() && num5 < MinimumHeight)
            {
                num5 = MinimumHeight;
            }

            return num5;
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

        public virtual void AddHeader(PdfPHeaderCell header)
        {
            if (headers == null)
            {
                headers = new List<PdfPHeaderCell>();
            }

            headers.Add(header);
        }
    }
}
