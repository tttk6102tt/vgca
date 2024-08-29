using Sign.itext.error_messages;
using System.Text;

namespace Sign.itext.text
{
    public class RectangleReadOnly : Rectangle
    {
        public override float Top
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override int Border
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override float GrayFill
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override float Left
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override float Right
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override float Bottom
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override BaseColor BorderColorBottom
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override BaseColor BorderColorTop
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override BaseColor BorderColorLeft
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override BaseColor BorderColorRight
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override float BorderWidth
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override BaseColor BorderColor
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override BaseColor BackgroundColor
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override int Rotation
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override float BorderWidthLeft
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override float BorderWidthRight
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override float BorderWidthTop
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override float BorderWidthBottom
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public override bool UseVariableBorders
        {
            set
            {
                ThrowReadOnlyError();
            }
        }

        public RectangleReadOnly(float llx, float lly, float urx, float ury)
            : base(llx, lly, urx, ury)
        {
        }

        public RectangleReadOnly(float llx, float lly, float urx, float ury, int rotation)
            : base(llx, lly, urx, ury)
        {
            base.Rotation = rotation;
        }

        public RectangleReadOnly(float urx, float ury)
            : base(0f, 0f, urx, ury)
        {
        }

        public RectangleReadOnly(float urx, float ury, int rotation)
            : base(0f, 0f, urx, ury)
        {
            base.Rotation = rotation;
        }

        public RectangleReadOnly(Rectangle rect)
            : base(rect.Left, rect.Bottom, rect.Right, rect.Top)
        {
            base.CloneNonPositionParameters(rect);
        }

        public override void CloneNonPositionParameters(Rectangle rect)
        {
            ThrowReadOnlyError();
        }

        private void ThrowReadOnlyError()
        {
            throw new InvalidOperationException(MessageLocalization.GetComposedMessage("rectanglereadonly.this.rectangle.is.read.only"));
        }

        public override void SoftCloneNonPositionParameters(Rectangle rect)
        {
            ThrowReadOnlyError();
        }

        public override void Normalize()
        {
            ThrowReadOnlyError();
        }

        public override void EnableBorderSide(int side)
        {
            ThrowReadOnlyError();
        }

        public override void DisableBorderSide(int side)
        {
            ThrowReadOnlyError();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("RectangleReadOnly: ");
            stringBuilder.Append(Width);
            stringBuilder.Append('x');
            stringBuilder.Append(Height);
            stringBuilder.Append(" (rot: ");
            stringBuilder.Append(rotation);
            stringBuilder.Append(" degrees)");
            return stringBuilder.ToString();
        }
    }
}
