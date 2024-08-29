namespace Sign.itext.awt.geom
{
    public abstract class Point2D
    {
        public class Float : Point2D
        {
            public float x;

            public float y;

            public Float()
            {
            }

            public Float(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            public override double GetX()
            {
                return x;
            }

            public override double GetY()
            {
                return y;
            }

            public virtual void SetLocation(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            public override void SetLocation(double x, double y)
            {
                this.x = (float)x;
                this.y = (float)y;
            }

            public override string ToString()
            {
                return "Point2D:[x=" + x + ",y=" + y + "]";
            }
        }

        public class Double : Point2D
        {
            public double x;

            public double y;

            public Double()
            {
            }

            public Double(double x, double y)
            {
                this.x = x;
                this.y = y;
            }

            public override double GetX()
            {
                return x;
            }

            public override double GetY()
            {
                return y;
            }

            public override void SetLocation(double x, double y)
            {
                this.x = x;
                this.y = y;
            }

            public override string ToString()
            {
                return "Point2D: [x=" + x + ",y=" + y + "]";
            }
        }

        public abstract double GetX();

        public abstract double GetY();

        public abstract void SetLocation(double x, double y);

        public virtual void SetLocation(Point2D p)
        {
            SetLocation(p.GetX(), p.GetY());
        }

        public static double DistanceSq(double x1, double y1, double x2, double y2)
        {
            x2 -= x1;
            y2 -= y1;
            return x2 * x2 + y2 * y2;
        }

        public virtual double DistanceSq(double px, double py)
        {
            return DistanceSq(GetX(), GetY(), px, py);
        }

        public virtual double DistanceSq(Point2D p)
        {
            return DistanceSq(GetX(), GetY(), p.GetX(), p.GetY());
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(DistanceSq(x1, y1, x2, y2));
        }

        public virtual double Distance(double px, double py)
        {
            return Math.Sqrt(DistanceSq(px, py));
        }

        public virtual double Distance(Point2D p)
        {
            return Math.Sqrt(DistanceSq(p));
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj is Point2D)
            {
                Point2D point2D = (Point2D)obj;
                if (GetX() == point2D.GetX())
                {
                    return GetY() == point2D.GetY();
                }

                return false;
            }

            return false;
        }
    }
}
