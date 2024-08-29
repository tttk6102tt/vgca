namespace Sign.itext.awt.geom
{
    public class AffineTransform : ICloneable
    {
        public const int TYPE_IDENTITY = 0;

        public const int TYPE_TRANSLATION = 1;

        public const int TYPE_UNIFORM_SCALE = 2;

        public const int TYPE_GENERAL_SCALE = 4;

        public const int TYPE_QUADRANT_ROTATION = 8;

        public const int TYPE_GENERAL_ROTATION = 16;

        public const int TYPE_GENERAL_TRANSFORM = 32;

        public const int TYPE_FLIP = 64;

        public const int TYPE_MASK_SCALE = 6;

        public const int TYPE_MASK_ROTATION = 24;

        private const int TYPE_UNKNOWN = -1;

        private const double ZERO = 1E-10;

        private double m00;

        private double m10;

        private double m01;

        private double m11;

        private double m02;

        private double m12;

        private int type;

        public virtual int Type
        {
            get
            {
                if (type != -1)
                {
                    return type;
                }

                int num = 0;
                if (m00 * m01 + m10 * m11 != 0.0)
                {
                    return num | 0x20;
                }

                if (m02 != 0.0 || m12 != 0.0)
                {
                    num |= 1;
                }
                else if (m00 == 1.0 && m11 == 1.0 && m01 == 0.0 && m10 == 0.0)
                {
                    return 0;
                }

                if (m00 * m11 - m01 * m10 < 0.0)
                {
                    num |= 0x40;
                }

                double num2 = m00 * m00 + m10 * m10;
                double num3 = m01 * m01 + m11 * m11;
                if (num2 != num3)
                {
                    num |= 4;
                }
                else if (num2 != 1.0)
                {
                    num |= 2;
                }

                if ((m00 == 0.0 && m11 == 0.0) || (m10 == 0.0 && m01 == 0.0 && (m00 < 0.0 || m11 < 0.0)))
                {
                    num |= 8;
                }
                else if (m01 != 0.0 || m10 != 0.0)
                {
                    num |= 0x10;
                }

                return num;
            }
        }

        public AffineTransform()
        {
            type = 0;
            m00 = (m11 = 1.0);
            m10 = (m01 = (m02 = (m12 = 0.0)));
        }

        public AffineTransform(AffineTransform t)
        {
            type = t.type;
            m00 = t.m00;
            m10 = t.m10;
            m01 = t.m01;
            m11 = t.m11;
            m02 = t.m02;
            m12 = t.m12;
        }

        public AffineTransform(float m00, float m10, float m01, float m11, float m02, float m12)
        {
            type = -1;
            this.m00 = m00;
            this.m10 = m10;
            this.m01 = m01;
            this.m11 = m11;
            this.m02 = m02;
            this.m12 = m12;
        }

        public AffineTransform(double m00, double m10, double m01, double m11, double m02, double m12)
        {
            type = -1;
            this.m00 = m00;
            this.m10 = m10;
            this.m01 = m01;
            this.m11 = m11;
            this.m02 = m02;
            this.m12 = m12;
        }

        public AffineTransform(float[] matrix)
        {
            type = -1;
            m00 = matrix[0];
            m10 = matrix[1];
            m01 = matrix[2];
            m11 = matrix[3];
            if (matrix.Length > 4)
            {
                m02 = matrix[4];
                m12 = matrix[5];
            }
        }

        public AffineTransform(double[] matrix)
        {
            type = -1;
            m00 = matrix[0];
            m10 = matrix[1];
            m01 = matrix[2];
            m11 = matrix[3];
            if (matrix.Length > 4)
            {
                m02 = matrix[4];
                m12 = matrix[5];
            }
        }

        public virtual double GetScaleX()
        {
            return m00;
        }

        public virtual double GetScaleY()
        {
            return m11;
        }

        public virtual double GetShearX()
        {
            return m01;
        }

        public virtual double GetShearY()
        {
            return m10;
        }

        public virtual double GetTranslateX()
        {
            return m02;
        }

        public virtual double GetTranslateY()
        {
            return m12;
        }

        public virtual bool IsIdentity()
        {
            return Type == 0;
        }

        public virtual void GetMatrix(double[] matrix)
        {
            matrix[0] = m00;
            matrix[1] = m10;
            matrix[2] = m01;
            matrix[3] = m11;
            if (matrix.Length > 4)
            {
                matrix[4] = m02;
                matrix[5] = m12;
            }
        }

        public virtual double GetDeterminant()
        {
            return m00 * m11 - m01 * m10;
        }

        public virtual void SetTransform(double m00, double m10, double m01, double m11, double m02, double m12)
        {
            type = -1;
            this.m00 = m00;
            this.m10 = m10;
            this.m01 = m01;
            this.m11 = m11;
            this.m02 = m02;
            this.m12 = m12;
        }

        public virtual void SetTransform(AffineTransform t)
        {
            type = t.type;
            SetTransform(t.m00, t.m10, t.m01, t.m11, t.m02, t.m12);
        }

        public virtual void SetToIdentity()
        {
            type = 0;
            m00 = (m11 = 1.0);
            m10 = (m01 = (m02 = (m12 = 0.0)));
        }

        public virtual void SetToTranslation(double mx, double my)
        {
            m00 = (m11 = 1.0);
            m01 = (m10 = 0.0);
            m02 = mx;
            m12 = my;
            if (mx == 0.0 && my == 0.0)
            {
                type = 0;
            }
            else
            {
                type = 1;
            }
        }

        public virtual void SetToScale(double scx, double scy)
        {
            m00 = scx;
            m11 = scy;
            m10 = (m01 = (m02 = (m12 = 0.0)));
            if (scx != 1.0 || scy != 1.0)
            {
                type = -1;
            }
            else
            {
                type = 0;
            }
        }

        public virtual void SetToShear(double shx, double shy)
        {
            m00 = (m11 = 1.0);
            m02 = (m12 = 0.0);
            m01 = shx;
            m10 = shy;
            if (shx != 0.0 || shy != 0.0)
            {
                type = -1;
            }
            else
            {
                type = 0;
            }
        }

        public virtual void SetToRotation(double angle)
        {
            double num = Math.Sin(angle);
            double num2 = Math.Cos(angle);
            if (Math.Abs(num2) < 1E-10)
            {
                num2 = 0.0;
                num = ((num > 0.0) ? 1.0 : (-1.0));
            }
            else if (Math.Abs(num) < 1E-10)
            {
                num = 0.0;
                num2 = ((num2 > 0.0) ? 1.0 : (-1.0));
            }

            m00 = (m11 = num2);
            m01 = 0.0 - num;
            m10 = num;
            m02 = (m12 = 0.0);
            type = -1;
        }

        public virtual void SetToRotation(double angle, double px, double py)
        {
            SetToRotation(angle);
            m02 = px * (1.0 - m00) + py * m10;
            m12 = py * (1.0 - m00) - px * m10;
            type = -1;
        }

        public static AffineTransform GetTranslateInstance(double mx, double my)
        {
            AffineTransform affineTransform = new AffineTransform();
            affineTransform.SetToTranslation(mx, my);
            return affineTransform;
        }

        public static AffineTransform GetScaleInstance(double scx, double scY)
        {
            AffineTransform affineTransform = new AffineTransform();
            affineTransform.SetToScale(scx, scY);
            return affineTransform;
        }

        public static AffineTransform GetShearInstance(double shx, double shy)
        {
            AffineTransform affineTransform = new AffineTransform();
            affineTransform.SetToShear(shx, shy);
            return affineTransform;
        }

        public static AffineTransform GetRotateInstance(double angle)
        {
            AffineTransform affineTransform = new AffineTransform();
            affineTransform.SetToRotation(angle);
            return affineTransform;
        }

        public static AffineTransform GetRotateInstance(double angle, double x, double y)
        {
            AffineTransform affineTransform = new AffineTransform();
            affineTransform.SetToRotation(angle, x, y);
            return affineTransform;
        }

        public virtual void Translate(double mx, double my)
        {
            Concatenate(GetTranslateInstance(mx, my));
        }

        public virtual void Scale(double scx, double scy)
        {
            Concatenate(GetScaleInstance(scx, scy));
        }

        public virtual void Shear(double shx, double shy)
        {
            Concatenate(GetShearInstance(shx, shy));
        }

        public virtual void Rotate(double angle)
        {
            Concatenate(GetRotateInstance(angle));
        }

        public virtual void Rotate(double angle, double px, double py)
        {
            Concatenate(GetRotateInstance(angle, px, py));
        }

        private AffineTransform Multiply(AffineTransform t1, AffineTransform t2)
        {
            return new AffineTransform(t1.m00 * t2.m00 + t1.m10 * t2.m01, t1.m00 * t2.m10 + t1.m10 * t2.m11, t1.m01 * t2.m00 + t1.m11 * t2.m01, t1.m01 * t2.m10 + t1.m11 * t2.m11, t1.m02 * t2.m00 + t1.m12 * t2.m01 + t2.m02, t1.m02 * t2.m10 + t1.m12 * t2.m11 + t2.m12);
        }

        public virtual void Concatenate(AffineTransform t)
        {
            SetTransform(Multiply(t, this));
        }

        public virtual void preConcatenate(AffineTransform t)
        {
            SetTransform(Multiply(this, t));
        }

        public virtual AffineTransform CreateInverse()
        {
            double determinant = GetDeterminant();
            if (Math.Abs(determinant) < 1E-10)
            {
                throw new InvalidOperationException("awt.204");
            }

            return new AffineTransform(m11 / determinant, (0.0 - m10) / determinant, (0.0 - m01) / determinant, m00 / determinant, (m01 * m12 - m11 * m02) / determinant, (m10 * m02 - m00 * m12) / determinant);
        }

        public virtual Point2D Transform(Point2D src, Point2D dst)
        {
            if (dst == null)
            {
                dst = ((!(src is Point2D.Double)) ? ((Point2D)new Point2D.Float()) : ((Point2D)new Point2D.Double()));
            }

            double x = src.GetX();
            double y = src.GetY();
            dst.SetLocation(x * m00 + y * m01 + m02, x * m10 + y * m11 + m12);
            return dst;
        }

        public virtual void Transform(Point2D[] src, int srcOff, Point2D[] dst, int dstOff, int length)
        {
            while (--length >= 0)
            {
                Point2D point2D = src[srcOff++];
                double x = point2D.GetX();
                double y = point2D.GetY();
                Point2D point2D2 = dst[dstOff];
                if (point2D2 == null)
                {
                    point2D2 = ((!(point2D is Point2D.Double)) ? ((Point2D)new Point2D.Float()) : ((Point2D)new Point2D.Double()));
                }

                point2D2.SetLocation(x * m00 + y * m01 + m02, x * m10 + y * m11 + m12);
                dst[dstOff++] = point2D2;
            }
        }

        public virtual void Transform(double[] src, int srcOff, double[] dst, int dstOff, int length)
        {
            int num = 2;
            if (src == dst && srcOff < dstOff && dstOff < srcOff + length * 2)
            {
                srcOff = srcOff + length * 2 - 2;
                dstOff = dstOff + length * 2 - 2;
                num = -2;
            }

            while (--length >= 0)
            {
                double num2 = src[srcOff];
                double num3 = src[srcOff + 1];
                dst[dstOff] = num2 * m00 + num3 * m01 + m02;
                dst[dstOff + 1] = num2 * m10 + num3 * m11 + m12;
                srcOff += num;
                dstOff += num;
            }
        }

        public virtual void Transform(float[] src, int srcOff, float[] dst, int dstOff, int length)
        {
            int num = 2;
            if (src == dst && srcOff < dstOff && dstOff < srcOff + length * 2)
            {
                srcOff = srcOff + length * 2 - 2;
                dstOff = dstOff + length * 2 - 2;
                num = -2;
            }

            while (--length >= 0)
            {
                float num2 = src[srcOff];
                float num3 = src[srcOff + 1];
                dst[dstOff] = (float)((double)num2 * m00 + (double)num3 * m01 + m02);
                dst[dstOff + 1] = (float)((double)num2 * m10 + (double)num3 * m11 + m12);
                srcOff += num;
                dstOff += num;
            }
        }

        public virtual void Transform(float[] src, int srcOff, double[] dst, int dstOff, int length)
        {
            while (--length >= 0)
            {
                float num = src[srcOff++];
                float num2 = src[srcOff++];
                dst[dstOff++] = (double)num * m00 + (double)num2 * m01 + m02;
                dst[dstOff++] = (double)num * m10 + (double)num2 * m11 + m12;
            }
        }

        public virtual void Transform(double[] src, int srcOff, float[] dst, int dstOff, int length)
        {
            while (--length >= 0)
            {
                double num = src[srcOff++];
                double num2 = src[srcOff++];
                dst[dstOff++] = (float)(num * m00 + num2 * m01 + m02);
                dst[dstOff++] = (float)(num * m10 + num2 * m11 + m12);
            }
        }

        public virtual Point2D DeltaTransform(Point2D src, Point2D dst)
        {
            if (dst == null)
            {
                dst = ((!(src is Point2D.Double)) ? ((Point2D)new Point2D.Float()) : ((Point2D)new Point2D.Double()));
            }

            double x = src.GetX();
            double y = src.GetY();
            dst.SetLocation(x * m00 + y * m01, x * m10 + y * m11);
            return dst;
        }

        public virtual void DeltaTransform(double[] src, int srcOff, double[] dst, int dstOff, int length)
        {
            while (--length >= 0)
            {
                double num = src[srcOff++];
                double num2 = src[srcOff++];
                dst[dstOff++] = num * m00 + num2 * m01;
                dst[dstOff++] = num * m10 + num2 * m11;
            }
        }

        public virtual Point2D InverseTransform(Point2D src, Point2D dst)
        {
            double determinant = GetDeterminant();
            if (Math.Abs(determinant) < 1E-10)
            {
                throw new InvalidOperationException("awt.204");
            }

            if (dst == null)
            {
                dst = ((!(src is Point2D.Double)) ? ((Point2D)new Point2D.Float()) : ((Point2D)new Point2D.Double()));
            }

            double num = src.GetX() - m02;
            double num2 = src.GetY() - m12;
            dst.SetLocation((num * m11 - num2 * m01) / determinant, (num2 * m00 - num * m10) / determinant);
            return dst;
        }

        public virtual void InverseTransform(double[] src, int srcOff, double[] dst, int dstOff, int length)
        {
            double determinant = GetDeterminant();
            if (Math.Abs(determinant) < 1E-10)
            {
                throw new InvalidOperationException("awt.204");
            }

            while (--length >= 0)
            {
                double num = src[srcOff++] - m02;
                double num2 = src[srcOff++] - m12;
                dst[dstOff++] = (num * m11 - num2 * m01) / determinant;
                dst[dstOff++] = (num2 * m00 - num * m10) / determinant;
            }
        }

        public virtual void InverseTransform(float[] src, int srcOff, float[] dst, int dstOff, int length)
        {
            float num = (float)GetDeterminant();
            if ((double)Math.Abs(num) < 1E-10)
            {
                throw new InvalidOperationException("awt.204");
            }

            while (--length >= 0)
            {
                float num2 = src[srcOff++] - (float)m02;
                float num3 = src[srcOff++] - (float)m12;
                dst[dstOff++] = (num2 * (float)m11 - num3 * (float)m01) / num;
                dst[dstOff++] = (num3 * (float)m00 - num2 * (float)m10) / num;
            }
        }

        public virtual object Clone()
        {
            return new AffineTransform(this);
        }

        public override string ToString()
        {
            return "Transform: [[" + m00 + ", " + m01 + ", " + m02 + "], [" + m10 + ", " + m11 + ", " + m12 + "]]";
        }
    }
}
