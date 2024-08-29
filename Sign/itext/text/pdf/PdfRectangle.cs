using Sign.itext.awt.geom;
using Sign.itext.pdf;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sign.itext.text.pdf
{
    public class PdfRectangle : NumberArray
    {
        private float llx;

        private float lly;

        private float urx;

        private float ury;

        public virtual Rectangle Rectangle => new Rectangle(Left, Bottom, Right, Top);

        public virtual float Left => llx;

        public virtual float Right => urx;

        public virtual float Top => ury;

        public virtual float Bottom => lly;

        public virtual float Width => urx - llx;

        public virtual float Height => ury - lly;

        public virtual PdfRectangle Rotate => new PdfRectangle(lly, llx, ury, urx, 0);

        public PdfRectangle(float llx, float lly, float urx, float ury, int rotation)
        {
            if (rotation == 90 || rotation == 270)
            {
                this.llx = lly;
                this.lly = llx;
                this.urx = ury;
                this.ury = urx;
            }
            else
            {
                this.llx = llx;
                this.lly = lly;
                this.urx = urx;
                this.ury = ury;
            }

            base.Add(new PdfNumber(this.llx));
            base.Add(new PdfNumber(this.lly));
            base.Add(new PdfNumber(this.urx));
            base.Add(new PdfNumber(this.ury));
        }

        public PdfRectangle(float llx, float lly, float urx, float ury)
            : this(llx, lly, urx, ury, 0)
        {
        }

        public PdfRectangle(float urx, float ury, int rotation)
            : this(0f, 0f, urx, ury, rotation)
        {
        }

        public PdfRectangle(float urx, float ury)
            : this(0f, 0f, urx, ury, 0)
        {
        }

        public PdfRectangle(Rectangle rectangle, int rotation)
            : this(rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Top, rotation)
        {
        }

        public PdfRectangle(Rectangle rectangle)
            : this(rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Top, 0)
        {
        }

        public override bool Add(PdfObject obj)
        {
            return false;
        }

        public override bool Add(float[] values)
        {
            return false;
        }

        public override bool Add(int[] values)
        {
            return false;
        }

        public override void AddFirst(PdfObject obj)
        {
        }

        public virtual float GetLeft(int margin)
        {
            return llx + (float)margin;
        }

        public virtual float GetRight(int margin)
        {
            return urx - (float)margin;
        }

        public virtual float GetTop(int margin)
        {
            return ury - (float)margin;
        }

        public virtual float GetBottom(int margin)
        {
            return lly + (float)margin;
        }

        [Obsolete]
        public PdfRectangle Transform(Matrix transform)
        {
            PointF[] array = new PointF[2]
            {
                new PointF(llx, lly),
                new PointF(urx, ury)
            };
            float[] array2 = new float[4]
            {
                array[0].X,
                array[0].Y,
                array[1].X,
                array[1].Y
            };
            transform.TransformPoints(array);
            if (array2[0] > array2[2])
            {
                array[0].X = array2[2];
                array[1].X = array2[0];
            }

            if (array2[1] > array2[3])
            {
                array[0].Y = array2[3];
                array[1].Y = array2[1];
            }

            return new PdfRectangle(array[0].X, array[0].Y, array[1].X, array[1].Y);
        }

        public virtual PdfRectangle Transform(AffineTransform transform)
        {
            float[] array = new float[4] { llx, lly, urx, ury };
            transform.Transform(array, 0, array, 0, 2);
            float[] array2 = new float[4]
            {
                array[0],
                array[1],
                array[2],
                array[3]
            };
            if (array[0] > array[2])
            {
                array2[0] = array[2];
                array2[2] = array[0];
            }

            if (array[1] > array[3])
            {
                array2[1] = array[3];
                array2[3] = array[1];
            }

            return new PdfRectangle(array2[0], array2[1], array2[2], array2[3]);
        }
    }
}
