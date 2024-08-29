using Sign.itext.error_messages;
using Sign.itext.text.pdf;
using Sign.SystemItext.util;
using System.Text;

namespace Sign.itext.text
{
    public class Rectangle : Element, IElement
    {
        public const int UNDEFINED = -1;

        public const int TOP_BORDER = 1;

        public const int BOTTOM_BORDER = 2;

        public const int LEFT_BORDER = 4;

        public const int RIGHT_BORDER = 8;

        public const int NO_BORDER = 0;

        public const int BOX = 15;

        protected float llx;

        protected float lly;

        protected float urx;

        protected float ury;

        protected int border = -1;

        protected float borderWidth = -1f;

        protected BaseColor borderColor;

        protected BaseColor borderColorLeft;

        protected BaseColor borderColorRight;

        protected BaseColor borderColorTop;

        protected BaseColor borderColorBottom;

        protected float borderWidthLeft = -1f;

        protected float borderWidthRight = -1f;

        protected float borderWidthTop = -1f;

        protected float borderWidthBottom = -1f;

        protected bool useVariableBorders;

        protected BaseColor backgroundColor;

        protected int rotation;

        public virtual int Type => 30;

        public virtual IList<Chunk> Chunks => new List<Chunk>();

        public virtual float Top
        {
            get
            {
                return ury;
            }
            set
            {
                ury = value;
            }
        }

        public virtual int Border
        {
            get
            {
                return border;
            }
            set
            {
                border = value;
            }
        }

        public virtual float GrayFill
        {
            get
            {
                if (backgroundColor is GrayColor)
                {
                    return ((GrayColor)backgroundColor).Gray;
                }

                return 0f;
            }
            set
            {
                backgroundColor = new GrayColor(value);
            }
        }

        public virtual float Left
        {
            get
            {
                return llx;
            }
            set
            {
                llx = value;
            }
        }

        public virtual float Right
        {
            get
            {
                return urx;
            }
            set
            {
                urx = value;
            }
        }

        public virtual float Bottom
        {
            get
            {
                return lly;
            }
            set
            {
                lly = value;
            }
        }

        public virtual BaseColor BorderColorBottom
        {
            get
            {
                if (borderColorBottom == null)
                {
                    return borderColor;
                }

                return borderColorBottom;
            }
            set
            {
                borderColorBottom = value;
            }
        }

        public virtual BaseColor BorderColorTop
        {
            get
            {
                if (borderColorTop == null)
                {
                    return borderColor;
                }

                return borderColorTop;
            }
            set
            {
                borderColorTop = value;
            }
        }

        public virtual BaseColor BorderColorLeft
        {
            get
            {
                if (borderColorLeft == null)
                {
                    return borderColor;
                }

                return borderColorLeft;
            }
            set
            {
                borderColorLeft = value;
            }
        }

        public virtual BaseColor BorderColorRight
        {
            get
            {
                if (borderColorRight == null)
                {
                    return borderColor;
                }

                return borderColorRight;
            }
            set
            {
                borderColorRight = value;
            }
        }

        public virtual float Width
        {
            get
            {
                return urx - llx;
            }
            set
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("the.width.cannot.be.set"));
            }
        }

        public virtual float Height => ury - lly;

        public virtual float BorderWidth
        {
            get
            {
                return borderWidth;
            }
            set
            {
                borderWidth = value;
            }
        }

        public virtual BaseColor BorderColor
        {
            get
            {
                return borderColor;
            }
            set
            {
                borderColor = value;
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

        public virtual int Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value % 360;
                int num = rotation;
                if (num != 90 && num != 180 && num != 270)
                {
                    rotation = 0;
                }
            }
        }

        public virtual float BorderWidthLeft
        {
            get
            {
                return GetVariableBorderWidth(borderWidthLeft, 4);
            }
            set
            {
                borderWidthLeft = value;
                UpdateBorderBasedOnWidth(value, 4);
            }
        }

        public virtual float BorderWidthRight
        {
            get
            {
                return GetVariableBorderWidth(borderWidthRight, 8);
            }
            set
            {
                borderWidthRight = value;
                UpdateBorderBasedOnWidth(value, 8);
            }
        }

        public virtual float BorderWidthTop
        {
            get
            {
                return GetVariableBorderWidth(borderWidthTop, 1);
            }
            set
            {
                borderWidthTop = value;
                UpdateBorderBasedOnWidth(value, 1);
            }
        }

        public virtual float BorderWidthBottom
        {
            get
            {
                return GetVariableBorderWidth(borderWidthBottom, 2);
            }
            set
            {
                borderWidthBottom = value;
                UpdateBorderBasedOnWidth(value, 2);
            }
        }

        public virtual bool UseVariableBorders
        {
            get
            {
                return useVariableBorders;
            }
            set
            {
                useVariableBorders = value;
            }
        }

        public Rectangle(float llx, float lly, float urx, float ury)
        {
            this.llx = llx;
            this.lly = lly;
            this.urx = urx;
            this.ury = ury;
        }

        public Rectangle(float llx, float lly, float urx, float ury, int rotation)
            : this(llx, lly, urx, ury)
        {
            Rotation = rotation;
        }

        public Rectangle(float urx, float ury)
            : this(0f, 0f, urx, ury)
        {
        }

        public Rectangle(float urx, float ury, int rotation)
            : this(0f, 0f, urx, ury, rotation)
        {
        }

        public Rectangle(Rectangle rect)
            : this(rect.llx, rect.lly, rect.urx, rect.ury)
        {
            CloneNonPositionParameters(rect);
        }

        public Rectangle(RectangleJ rect)
            : this(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height)
        {
        }

        public virtual void CloneNonPositionParameters(Rectangle rect)
        {
            rotation = rect.rotation;
            border = rect.border;
            borderWidth = rect.borderWidth;
            borderColor = rect.borderColor;
            backgroundColor = rect.backgroundColor;
            borderColorLeft = rect.borderColorLeft;
            borderColorRight = rect.borderColorRight;
            borderColorTop = rect.borderColorTop;
            borderColorBottom = rect.borderColorBottom;
            borderWidthLeft = rect.borderWidthLeft;
            borderWidthRight = rect.borderWidthRight;
            borderWidthTop = rect.borderWidthTop;
            borderWidthBottom = rect.borderWidthBottom;
            useVariableBorders = rect.useVariableBorders;
        }

        public virtual void SoftCloneNonPositionParameters(Rectangle rect)
        {
            if (rect.rotation != 0)
            {
                rotation = rect.rotation;
            }

            if (rect.border != -1)
            {
                border = rect.border;
            }

            if (rect.borderWidth != -1f)
            {
                borderWidth = rect.borderWidth;
            }

            if (rect.borderColor != null)
            {
                borderColor = rect.borderColor;
            }

            if (rect.backgroundColor != null)
            {
                backgroundColor = rect.backgroundColor;
            }

            if (rect.borderColorLeft != null)
            {
                borderColorLeft = rect.borderColorLeft;
            }

            if (rect.borderColorRight != null)
            {
                borderColorRight = rect.borderColorRight;
            }

            if (rect.borderColorTop != null)
            {
                borderColorTop = rect.borderColorTop;
            }

            if (rect.borderColorBottom != null)
            {
                borderColorBottom = rect.borderColorBottom;
            }

            if (rect.borderWidthLeft != -1f)
            {
                borderWidthLeft = rect.borderWidthLeft;
            }

            if (rect.borderWidthRight != -1f)
            {
                borderWidthRight = rect.borderWidthRight;
            }

            if (rect.borderWidthTop != -1f)
            {
                borderWidthTop = rect.borderWidthTop;
            }

            if (rect.borderWidthBottom != -1f)
            {
                borderWidthBottom = rect.borderWidthBottom;
            }

            if (useVariableBorders)
            {
                useVariableBorders = rect.useVariableBorders;
            }
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

        public virtual bool IsContent()
        {
            return true;
        }

        public virtual bool IsNestable()
        {
            return false;
        }

        public virtual void Normalize()
        {
            if (llx > urx)
            {
                float num = llx;
                llx = urx;
                urx = num;
            }

            if (lly > ury)
            {
                float num2 = lly;
                lly = ury;
                ury = num2;
            }
        }

        public virtual Rectangle GetRectangle(float top, float bottom)
        {
            Rectangle rectangle = new Rectangle(this);
            if (Top > top)
            {
                rectangle.Top = top;
                rectangle.Border = border - (border & 1);
            }

            if (Bottom < bottom)
            {
                rectangle.Bottom = bottom;
                rectangle.Border = border - (border & 2);
            }

            return rectangle;
        }

        public virtual Rectangle Rotate()
        {
            return new Rectangle(lly, llx, ury, urx)
            {
                Rotation = rotation + 90
            };
        }

        public virtual void EnableBorderSide(int side)
        {
            if (border == -1)
            {
                border = 0;
            }

            border |= side;
        }

        public virtual void DisableBorderSide(int side)
        {
            if (border == -1)
            {
                border = 0;
            }

            border &= ~side;
        }

        public virtual float GetLeft(float margin)
        {
            return llx + margin;
        }

        public virtual float GetRight(float margin)
        {
            return urx - margin;
        }

        public virtual float GetTop(float margin)
        {
            return ury - margin;
        }

        public virtual float GetBottom(float margin)
        {
            return lly + margin;
        }

        public virtual bool HasBorders()
        {
            int num = border;
            if (num == -1 || num == 0)
            {
                return false;
            }

            if (!(borderWidth > 0f) && !(borderWidthLeft > 0f) && !(borderWidthRight > 0f) && !(borderWidthTop > 0f))
            {
                return borderWidthBottom > 0f;
            }

            return true;
        }

        public virtual bool HasBorder(int type)
        {
            if (border == -1)
            {
                return false;
            }

            return (border & type) == type;
        }

        private void UpdateBorderBasedOnWidth(float width, int side)
        {
            useVariableBorders = true;
            if (width > 0f)
            {
                EnableBorderSide(side);
            }
            else
            {
                DisableBorderSide(side);
            }
        }

        private float GetVariableBorderWidth(float variableWidthValue, int side)
        {
            if ((border & side) != 0)
            {
                if (variableWidthValue == -1f)
                {
                    return borderWidth;
                }

                return variableWidthValue;
            }

            return 0f;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("Rectangle: ");
            stringBuilder.Append(Width);
            stringBuilder.Append('x');
            stringBuilder.Append(Height);
            stringBuilder.Append(" (rot: ");
            stringBuilder.Append(rotation);
            stringBuilder.Append(" degrees)");
            return stringBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is Rectangle)
            {
                Rectangle rectangle = (Rectangle)obj;
                if (rectangle.llx == llx && rectangle.lly == lly && rectangle.urx == urx && rectangle.ury == ury)
                {
                    return rectangle.rotation == rotation;
                }

                return false;
            }

            return false;
        }
    }
}
