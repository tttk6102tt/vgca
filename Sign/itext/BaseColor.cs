using Sign.itext.error_messages;
using System.Drawing;

namespace Sign.itext
{
    public class BaseColor
    {
        public static readonly BaseColor WHITE = new BaseColor(255, 255, 255);

        public static readonly BaseColor LIGHT_GRAY = new BaseColor(192, 192, 192);

        public static readonly BaseColor GRAY = new BaseColor(128, 128, 128);

        public static readonly BaseColor DARK_GRAY = new BaseColor(64, 64, 64);

        public static readonly BaseColor BLACK = new BaseColor(0, 0, 0);

        public static readonly BaseColor RED = new BaseColor(255, 0, 0);

        public static readonly BaseColor PINK = new BaseColor(255, 175, 175);

        public static readonly BaseColor ORANGE = new BaseColor(255, 200, 0);

        public static readonly BaseColor YELLOW = new BaseColor(255, 255, 0);

        public static readonly BaseColor GREEN = new BaseColor(0, 255, 0);

        public static readonly BaseColor MAGENTA = new BaseColor(255, 0, 255);

        public static readonly BaseColor CYAN = new BaseColor(0, 255, 255);

        public static readonly BaseColor BLUE = new BaseColor(0, 0, 255);

        private const double FACTOR = 0.7;

        private int value;

        public virtual int RGB => value;

        public virtual int R => (RGB >> 16) & 0xFF;

        public virtual int G => (RGB >> 8) & 0xFF;

        public virtual int B => RGB & 0xFF;

        public virtual int A => (RGB >> 24) & 0xFF;

        public BaseColor(int red, int green, int blue, int alpha)
        {
            SetValue(red, green, blue, alpha);
        }

        public BaseColor(int red, int green, int blue)
            : this(red, green, blue, 255)
        {
        }

        public BaseColor(float red, float green, float blue, float alpha)
            : this((int)((double)(red * 255f) + 0.5), (int)((double)(green * 255f) + 0.5), (int)((double)(blue * 255f) + 0.5), (int)((double)(alpha * 255f) + 0.5))
        {
        }

        public BaseColor(float red, float green, float blue)
            : this(red, green, blue, 1f)
        {
        }

        public BaseColor(int argb)
        {
            value = argb;
        }

        public BaseColor(Color color)
            : this(color.R, color.G, color.B, color.A)
        {
        }

        public virtual BaseColor Brighter()
        {
            int num = R;
            int num2 = G;
            int num3 = B;
            int num4 = 3;
            if (num == 0 && num2 == 0 && num3 == 0)
            {
                return new BaseColor(num4, num4, num4);
            }

            if (num > 0 && num < num4)
            {
                num = num4;
            }

            if (num2 > 0 && num2 < num4)
            {
                num2 = num4;
            }

            if (num3 > 0 && num3 < num4)
            {
                num3 = num4;
            }

            return new BaseColor(Math.Min((int)((double)num / 0.7), 255), Math.Min((int)((double)num2 / 0.7), 255), Math.Min((int)((double)num3 / 0.7), 255));
        }

        public virtual BaseColor Darker()
        {
            return new BaseColor(Math.Max((int)((double)R * 0.7), 0), Math.Max((int)((double)G * 0.7), 0), Math.Max((int)((double)B * 0.7), 0));
        }

        [Obsolete("Use RGB instead")]
        public virtual int ToArgb()
        {
            return value;
        }

        public override bool Equals(object obj)
        {
            if (obj is BaseColor)
            {
                return ((BaseColor)obj).value == value;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return value;
        }

        protected internal virtual void SetValue(int red, int green, int blue, int alpha)
        {
            Validate(red);
            Validate(green);
            Validate(blue);
            Validate(alpha);
            value = ((alpha & 0xFF) << 24) | ((red & 0xFF) << 16) | ((green & 0xFF) << 8) | (blue & 0xFF);
        }

        private static void Validate(int value)
        {
            if (value < 0 || value > 255)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("color.value.outside.range.0.255"));
            }
        }

        public override string ToString()
        {
            return "Color value[" + value.ToString("X") + "]";
        }
    }
}
