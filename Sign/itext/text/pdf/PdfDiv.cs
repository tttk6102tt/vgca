using Sign.itext.awt.geom;
using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;
using Sign.itext.text.api;

namespace Sign.itext.text.pdf
{
    public class PdfDiv : IElement, ISpaceable, IAccessibleElement
    {
        public enum FloatType
        {
            NONE,
            LEFT,
            RIGHT
        }

        public enum PositionType
        {
            STATIC,
            ABSOLUTE,
            FIXED,
            RELATIVE
        }

        public enum DisplayType
        {
            DEFAULT_NULL_VALUE,
            NONE,
            BLOCK,
            INLINE,
            INLINE_BLOCK,
            INLINE_TABLE,
            LIST_ITEM,
            RUN_IN,
            TABLE,
            TABLE_CAPTION,
            TABLE_CELL,
            TABLE_COLUMN_GROUP,
            TABLE_COLUMN,
            TABLE_FOOTER_GROUP,
            TABLE_HEADER_GROUP,
            TABLE_ROW,
            TABLE_ROW_GROUP
        }

        private List<IElement> content;

        private float? left;

        private float? top;

        private float? right;

        private float? bottom;

        private float? width;

        private float? height;

        private float? percentageHeight;

        private float? percentageWidth;

        private float contentWidth;

        private float contentHeight;

        private int textAlignment = -1;

        private float paddingLeft;

        private float paddingRight;

        private float paddingTop;

        private float paddingBottom;

        private BaseColor backgroundColor;

        protected float spacingBefore;

        protected float spacingAfter;

        private FloatType floatType;

        private PositionType position;

        private DisplayType display;

        private FloatLayout floatLayout;

        private float yLine;

        protected int runDirection;

        protected PdfName role = PdfName.DIV;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes;

        protected AccessibleElementId id = new AccessibleElementId();

        public virtual float? Left
        {
            get
            {
                return left;
            }
            set
            {
                left = value;
            }
        }

        public virtual float? Top
        {
            get
            {
                return top;
            }
            set
            {
                top = value;
            }
        }

        public virtual float? Right
        {
            get
            {
                return right;
            }
            set
            {
                right = value;
            }
        }

        public virtual float? Bottom
        {
            get
            {
                return bottom;
            }
            set
            {
                bottom = value;
            }
        }

        public virtual float? Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        public virtual float? Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        public virtual float? PercentageHeight
        {
            get
            {
                return percentageHeight;
            }
            set
            {
                percentageHeight = value;
            }
        }

        public virtual float? PercentageWidth
        {
            get
            {
                return percentageWidth;
            }
            set
            {
                percentageWidth = value;
            }
        }

        public virtual float ContentWidth
        {
            get
            {
                return contentWidth;
            }
            set
            {
                contentWidth = value;
            }
        }

        public virtual float ContentHeight
        {
            get
            {
                return contentHeight;
            }
            set
            {
                contentHeight = value;
            }
        }

        public virtual int TextAlignment
        {
            get
            {
                return textAlignment;
            }
            set
            {
                textAlignment = value;
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

        public virtual FloatType Float
        {
            get
            {
                return floatType;
            }
            set
            {
                floatType = value;
            }
        }

        public virtual PositionType Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        public virtual FloatLayout FloatLayout
        {
            get
            {
                return floatLayout;
            }
            set
            {
                floatLayout = value;
            }
        }

        public virtual DisplayType Display
        {
            get
            {
                return display;
            }
            set
            {
                display = value;
            }
        }

        public virtual BaseColor BackgroundColor
        {
            get
            {
                return backgroundColor;
            }
            set
            {
                backgroundColor = value;
            }
        }

        public virtual float YLine => yLine;

        public virtual int RunDirection
        {
            get
            {
                return runDirection;
            }
            set
            {
                runDirection = value;
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

        public virtual List<IElement> Content => content;

        public virtual IList<Chunk> Chunks => new List<Chunk>();

        public virtual int Type => 37;

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

        public virtual float getActualHeight()
        {
            if (!height.HasValue || !(height >= contentHeight))
            {
                return contentHeight;
            }

            return height.Value;
        }

        public virtual float getActualWidth()
        {
            if (!width.HasValue || !(width >= contentWidth))
            {
                return contentWidth;
            }

            return width.Value;
        }

        public PdfDiv()
        {
            content = new List<IElement>();
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

        public virtual void AddElement(IElement element)
        {
            content.Add(element);
        }

        public virtual int Layout(PdfContentByte canvas, bool useAscender, bool simulate, float llx, float lly, float urx, float ury)
        {
            float num = Math.Min(llx, urx);
            float num2 = Math.Max(lly, ury);
            float num3 = Math.Min(lly, ury);
            float num4 = Math.Max(llx, urx);
            yLine = num2;
            bool flag = false;
            if (width.HasValue && width > 0f)
            {
                if (width < num4 - num)
                {
                    num4 = num + width.Value;
                }
                else if (width > num4 - num)
                {
                    return 2;
                }
            }
            else if (percentageWidth.HasValue)
            {
                contentWidth = (num4 - num) * percentageWidth.Value;
                num4 = num + contentWidth;
            }
            else if (!percentageWidth.HasValue && floatType == FloatType.NONE && (display == DisplayType.DEFAULT_NULL_VALUE || display == DisplayType.BLOCK || display == DisplayType.LIST_ITEM || display == DisplayType.RUN_IN))
            {
                contentWidth = num4 - num;
            }

            if (height.HasValue && height > 0f)
            {
                if (height < num2 - num3)
                {
                    flag = true;
                    num3 = num2 - height.Value;
                }
                else if (height > num2 - num3)
                {
                    return 2;
                }
            }
            else if (percentageHeight.HasValue)
            {
                if ((double?)percentageHeight < 1.0)
                {
                    flag = true;
                }

                contentHeight = (num2 - num3) * percentageHeight.Value;
                num3 = num2 - contentHeight;
            }

            if (!simulate && position == PositionType.RELATIVE)
            {
                float? num5 = null;
                num5 = (left.HasValue ? left : ((!right.HasValue) ? new float?(0f) : (0f - right)));
                float? num6 = null;
                num6 = (top.HasValue ? (0f - top) : ((!bottom.HasValue) ? new float?(0f) : bottom));
                canvas.SaveState();
                canvas.Transform(new AffineTransform(1f, 0f, 0f, 1f, num5.Value, num6.Value));
            }

            if (!simulate && backgroundColor != null && getActualWidth() > 0f && getActualHeight() > 0f)
            {
                float num7 = getActualWidth();
                float num8 = getActualHeight();
                if (width.HasValue)
                {
                    num7 = ((width > 0f) ? width.Value : 0f);
                }

                if (height.HasValue)
                {
                    num8 = ((height > 0f) ? height.Value : 0f);
                }

                if (num7 > 0f && num8 > 0f)
                {
                    Rectangle rectangle = new Rectangle(num, num2 - num8, num + num7, num2);
                    rectangle.BackgroundColor = backgroundColor;
                    PdfArtifact element = new PdfArtifact();
                    canvas.OpenMCBlock(element);
                    canvas.Rectangle(rectangle);
                    canvas.CloseMCBlock(element);
                }
            }

            if (!percentageWidth.HasValue)
            {
                contentWidth = 0f;
            }

            if (!percentageHeight.HasValue)
            {
                contentHeight = 0f;
            }

            num3 += paddingBottom;
            num += paddingLeft;
            num4 -= paddingRight;
            yLine -= paddingTop;
            int result = 1;
            if (content.Count > 0)
            {
                if (floatLayout == null)
                {
                    List<IElement> elements = new List<IElement>(content);
                    floatLayout = new FloatLayout(elements, useAscender);
                    floatLayout.RunDirection = runDirection;
                }

                floatLayout.SetSimpleColumn(num, num3, num4, yLine);
                result = floatLayout.Layout(canvas, simulate);
                yLine = floatLayout.YLine;
                if (!percentageWidth.HasValue && contentWidth < floatLayout.FilledWidth)
                {
                    contentWidth = floatLayout.FilledWidth;
                }
            }

            if (!simulate && position == PositionType.RELATIVE)
            {
                canvas.RestoreState();
            }

            yLine -= paddingBottom;
            if (!percentageHeight.HasValue)
            {
                contentHeight = num2 - yLine;
            }

            if (!percentageWidth.HasValue)
            {
                contentWidth += paddingLeft + paddingRight;
            }

            if (!flag)
            {
                return result;
            }

            return 1;
        }

        public virtual PdfObject GetAccessibleAttribute(PdfName key)
        {
            if (accessibleAttributes != null && accessibleAttributes.TryGetValue(key, out var value))
            {
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
    }
}
