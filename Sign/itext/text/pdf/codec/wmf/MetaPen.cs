namespace Sign.itext.text.pdf.codec.wmf
{
    public class MetaPen : MetaObject
    {
        public const int PS_SOLID = 0;

        public const int PS_DASH = 1;

        public const int PS_DOT = 2;

        public const int PS_DASHDOT = 3;

        public const int PS_DASHDOTDOT = 4;

        public const int PS_NULL = 5;

        public const int PS_INSIDEFRAME = 6;

        private int style;

        private int penWidth = 1;

        private BaseColor color = BaseColor.BLACK;

        public virtual int Style => style;

        public virtual int PenWidth => penWidth;

        public virtual BaseColor Color => color;

        public MetaPen()
        {
            type = 1;
        }

        public virtual void Init(InputMeta meta)
        {
            style = meta.ReadWord();
            penWidth = meta.ReadShort();
            meta.ReadWord();
            color = meta.ReadColor();
        }
    }
}
