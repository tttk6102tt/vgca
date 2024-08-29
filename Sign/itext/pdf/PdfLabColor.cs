using Sign.itext.error_messages;
using Sign.itext.text.pdf;
using Sign.SystemItext.util;

namespace Sign.itext.pdf
{
    public class PdfLabColor : ICachedColorSpace
    {
        private float[] whitePoint = new float[3] { 0.9505f, 1f, 1.089f };

        private float[] blackPoint;

        private float[] range;

        public PdfLabColor()
        {
        }

        public PdfLabColor(float[] whitePoint)
        {
            if (whitePoint == null || whitePoint.Length != 3 || whitePoint[0] < 1E-06f || whitePoint[2] < 1E-06f || whitePoint[1] < 0.999999f || whitePoint[1] > 1.000001f)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("lab.cs.white.point"));
            }

            this.whitePoint = whitePoint;
        }

        public PdfLabColor(float[] whitePoint, float[] blackPoint)
            : this(whitePoint)
        {
            this.blackPoint = blackPoint;
        }

        public PdfLabColor(float[] whitePoint, float[] blackPoint, float[] range)
            : this(whitePoint, blackPoint)
        {
            this.range = range;
        }

        public virtual PdfObject GetPdfObject(PdfWriter writer)
        {
            PdfArray pdfArray = new PdfArray(PdfName.LAB);
            PdfDictionary pdfDictionary = new PdfDictionary();
            if (whitePoint == null || whitePoint.Length != 3 || whitePoint[0] < 1E-06f || whitePoint[2] < 1E-06f || whitePoint[1] < 0.999999f || whitePoint[1] > 1.000001f)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("lab.cs.white.point"));
            }

            pdfDictionary.Put(PdfName.WHITEPOINT, new PdfArray(whitePoint));
            if (blackPoint != null)
            {
                if (blackPoint.Length != 3 || blackPoint[0] < -1E-06f || blackPoint[1] < -1E-06f || blackPoint[2] < -1E-06f)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("lab.cs.black.point"));
                }

                pdfDictionary.Put(PdfName.BLACKPOINT, new PdfArray(blackPoint));
            }

            if (range != null)
            {
                if (range.Length != 4 || range[0] > range[1] || range[2] > range[3])
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("lab.cs.range"));
                }

                pdfDictionary.Put(PdfName.RANGE, new PdfArray(range));
            }

            pdfArray.Add(pdfDictionary);
            return pdfArray;
        }

        public virtual BaseColor Lab2Rgb(float l, float a, float b)
        {
            double[] array = Lab2RgbLinear(l, a, b);
            return new BaseColor((float)array[0], (float)array[1], (float)array[2]);
        }

        internal virtual CMYKColor Lab2Cmyk(float l, float a, float b)
        {
            double[] array = Lab2RgbLinear(l, a, b);
            double num = array[0];
            double num2 = array[1];
            double num3 = array[2];
            double num4 = 0.0;
            double num5 = 0.0;
            double num6 = 0.0;
            double num7 = 0.0;
            if (num == 0.0 && num2 == 0.0 && b == 0f)
            {
                num7 = 1.0;
            }
            else
            {
                num4 = 1.0 - num;
                num5 = 1.0 - num2;
                num6 = 1.0 - num3;
                double num8 = Math.Min(num4, Math.Min(num5, num6));
                num4 = (num4 - num8) / (1.0 - num8);
                num5 = (num5 - num8) / (1.0 - num8);
                num6 = (num6 - num8) / (1.0 - num8);
                num7 = num8;
            }

            return new CMYKColor((float)num4, (float)num5, (float)num6, (float)num7);
        }

        protected virtual double[] Lab2RgbLinear(float l, float a, float b)
        {
            if (range != null && range.Length == 4)
            {
                if (a < range[0])
                {
                    a = range[0];
                }

                if (a > range[1])
                {
                    a = range[1];
                }

                if (b < range[2])
                {
                    b = range[2];
                }

                if (b > range[3])
                {
                    b = range[3];
                }
            }

            double num = 0.20689655172413793;
            double num2 = (double)(l + 16f) / 116.0;
            double num3 = num2 + (double)a / 500.0;
            double num4 = num2 - (double)b / 200.0;
            double num5 = ((num3 > num) ? ((double)whitePoint[0] * (num3 * num3 * num3)) : ((num3 - 0.13793103448275862) * 3.0 * (num * num) * (double)whitePoint[0]));
            double num6 = ((num2 > num) ? ((double)whitePoint[1] * (num2 * num2 * num2)) : ((num2 - 0.13793103448275862) * 3.0 * (num * num) * (double)whitePoint[1]));
            double num7 = ((num4 > num) ? ((double)whitePoint[2] * (num4 * num4 * num4)) : ((num4 - 0.13793103448275862) * 3.0 * (num * num) * (double)whitePoint[2]));
            double[] array = new double[3]
            {
                num5 * 3.241 - num6 * 1.5374 - num7 * 0.4986,
                (0.0 - num5) * 0.9692 + num6 * 1.876 - num7 * 0.0416,
                num5 * 0.0556 - num6 * 0.204 + num7 * 1.057
            };
            for (int i = 0; i < 3; i++)
            {
                array[i] = ((array[i] <= 0.0031308) ? (12.92 * array[i]) : (1.055 * Math.Pow(array[i], 5.0 / 12.0) - 0.055));
                if (array[i] < 0.0)
                {
                    array[i] = 0.0;
                }
                else if (array[i] > 1.0)
                {
                    array[i] = 1.0;
                }
            }

            return array;
        }

        public virtual LabColor Rgb2lab(BaseColor baseColor)
        {
            double num = (float)baseColor.R / 255f;
            double num2 = (float)baseColor.G / 255f;
            double num3 = (float)baseColor.B / 255f;
            double num4 = ((num > 0.04045) ? Math.Pow((num + 0.055) / 1.055, 2.2) : (num / 12.92));
            double num5 = ((num2 > 0.04045) ? Math.Pow((num2 + 0.055) / 1.055, 2.2) : (num2 / 12.92));
            double num6 = ((num3 > 0.04045) ? Math.Pow((num3 + 0.055) / 1.055, 2.2) : (num3 / 12.92));
            double num7 = num4 * 0.4124 + num5 * 0.3576 + num6 * 0.1805;
            double num8 = num4 * 0.2126 + num5 * 0.7152 + num6 * 0.0722;
            double num9 = num4 * 0.0193 + num5 * 0.1192 + num6 * 0.9505;
            float l = (float)Math.Round((116.0 * FXyz(num8 / (double)whitePoint[1]) - 16.0) * 1000.0) / 1000f;
            float a = (float)Math.Round(500.0 * (FXyz(num7 / (double)whitePoint[0]) - FXyz(num8 / (double)whitePoint[1])) * 1000.0) / 1000f;
            float b = (float)Math.Round(200.0 * (FXyz(num8 / (double)whitePoint[1]) - FXyz(num9 / (double)whitePoint[2])) * 1000.0) / 1000f;
            return new LabColor(this, l, a, b);
        }

        private static double FXyz(double t)
        {
            if (!(t > 0.008856))
            {
                return 7.787 * t + 0.13793103448275862;
            }

            return Math.Pow(t, 0.33333333333333331);
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }

            if (!(o is PdfLabColor))
            {
                return false;
            }

            PdfLabColor pdfLabColor = (PdfLabColor)o;
            if (!Util.ArraysAreEqual(blackPoint, pdfLabColor.blackPoint))
            {
                return false;
            }

            if (!Util.ArraysAreEqual(range, pdfLabColor.range))
            {
                return false;
            }

            if (!Util.ArraysAreEqual(whitePoint, pdfLabColor.whitePoint))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int arrayHashCode = Util.GetArrayHashCode(whitePoint);
            arrayHashCode = 31 * arrayHashCode + ((blackPoint != null) ? Util.GetArrayHashCode(blackPoint) : 0);
            return 31 * arrayHashCode + ((range != null) ? Util.GetArrayHashCode(range) : 0);
        }
    }
}
