using Sign.itext.text;

namespace Sign.SystemItext.util
{
    public class RectangleJ
    {
        public const int OUT_LEFT = 1;

        public const int OUT_TOP = 2;

        public const int OUT_RIGHT = 4;

        public const int OUT_BOTTOM = 8;

        private float x;

        private float y;

        private float width;

        private float height;

        public virtual float X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        public virtual float Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        public virtual float Width
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

        public virtual float Height
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

        public RectangleJ(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public RectangleJ(Rectangle rect)
        {
            rect.Normalize();
            x = rect.Left;
            y = rect.Bottom;
            width = rect.Width;
            height = rect.Height;
        }

        public virtual void Add(RectangleJ rect)
        {
            float num = Math.Min(Math.Min(x, x + width), Math.Min(rect.x, rect.x + rect.width));
            float num2 = Math.Max(Math.Max(x, x + width), Math.Max(rect.x, rect.x + rect.width));
            float num3 = Math.Min(Math.Min(y, y + height), Math.Min(rect.y, rect.y + rect.height));
            float num4 = Math.Max(Math.Max(y, y + height), Math.Max(rect.y, rect.y + rect.height));
            x = num;
            y = num3;
            width = num2 - num;
            height = num4 - num3;
        }

        public virtual int Outcode(double x, double y)
        {
            int num = 0;
            if (width <= 0f)
            {
                num |= 5;
            }
            else if (x < (double)this.x)
            {
                num |= 1;
            }
            else if (x > (double)this.x + (double)width)
            {
                num |= 4;
            }

            if (height <= 0f)
            {
                num |= 0xA;
            }
            else if (y < (double)this.y)
            {
                num |= 2;
            }
            else if (y > (double)this.y + (double)height)
            {
                num |= 8;
            }

            return num;
        }

        public virtual bool IntersectsLine(double x1, double y1, double x2, double y2)
        {
            int num;
            if ((num = Outcode(x2, y2)) == 0)
            {
                return true;
            }

            int num2;
            while ((num2 = Outcode(x1, y1)) != 0)
            {
                if ((num2 & num) != 0)
                {
                    return false;
                }

                if (((uint)num2 & 5u) != 0)
                {
                    float num3 = X;
                    if (((uint)num2 & 4u) != 0)
                    {
                        num3 += Width;
                    }

                    y1 += ((double)num3 - x1) * (y2 - y1) / (x2 - x1);
                    x1 = num3;
                }
                else
                {
                    float num4 = Y;
                    if (((uint)num2 & 8u) != 0)
                    {
                        num4 += Height;
                    }

                    x1 += ((double)num4 - y1) * (x2 - x1) / (y2 - y1);
                    y1 = num4;
                }
            }

            return true;
        }

        public virtual RectangleJ Intersection(RectangleJ r)
        {
            float num = Math.Max(x, r.x);
            float num2 = Math.Max(y, r.y);
            float num3 = Math.Min(x + width, r.x + r.width);
            float num4 = Math.Min(y + height, r.y + r.height);
            return new RectangleJ(num, num2, num3 - num, num4 - num2);
        }

        public virtual bool IsEmpty()
        {
            if (!(width <= 0f))
            {
                return height <= 0f;
            }

            return true;
        }
    }
}
