namespace Sign.itext.text.pdf
{
    public abstract class ExtendedColor : BaseColor
    {
        public const int TYPE_RGB = 0;

        public const int TYPE_GRAY = 1;

        public const int TYPE_CMYK = 2;

        public const int TYPE_SEPARATION = 3;

        public const int TYPE_PATTERN = 4;

        public const int TYPE_SHADING = 5;

        public const int TYPE_DEVICEN = 6;

        public const int TYPE_LAB = 7;

        protected int type;

        public virtual int Type => type;

        public ExtendedColor(int type)
            : base(0, 0, 0)
        {
            this.type = type;
        }

        public ExtendedColor(int type, float red, float green, float blue)
            : base(Normalize(red), Normalize(green), Normalize(blue))
        {
            this.type = type;
        }

        public ExtendedColor(int type, int red, int green, int blue, int alpha)
            : base(Normalize((float)red / 255f), Normalize((float)green / 255f), Normalize((float)blue / 255f), Normalize((float)alpha / 255f))
        {
            this.type = type;
        }

        public static int GetType(object color)
        {
            if (color is ExtendedColor)
            {
                return ((ExtendedColor)color).Type;
            }

            return 0;
        }

        internal static float Normalize(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > 1f)
            {
                return 1f;
            }

            return value;
        }
    }
}
