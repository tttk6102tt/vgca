namespace Sign.itext.text.pdf.codec.wmf
{
    public class MetaBrush : MetaObject
    {
        public const int BS_SOLID = 0;

        public const int BS_NULL = 1;

        public const int BS_HATCHED = 2;

        public const int BS_PATTERN = 3;

        public const int BS_DIBPATTERN = 5;

        public const int HS_HORIZONTAL = 0;

        public const int HS_VERTICAL = 1;

        public const int HS_FDIAGONAL = 2;

        public const int HS_BDIAGONAL = 3;

        public const int HS_CROSS = 4;

        public const int HS_DIAGCROSS = 5;

        private int style;

        private int hatch;

        private BaseColor color = BaseColor.WHITE;

        public virtual int Style => style;

        public virtual int Hatch => hatch;

        public virtual BaseColor Color => color;

        public MetaBrush()
        {
            type = 2;
        }

        public virtual void Init(InputMeta meta)
        {
            style = meta.ReadWord();
            color = meta.ReadColor();
            hatch = meta.ReadWord();
        }
    }
}
