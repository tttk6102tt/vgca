namespace Sign.itext.text.pdf.codec.wmf
{
    public class MetaObject
    {
        public const int META_NOT_SUPPORTED = 0;

        public const int META_PEN = 1;

        public const int META_BRUSH = 2;

        public const int META_FONT = 3;

        public int type;

        public virtual int Type => type;

        public MetaObject()
        {
        }

        public MetaObject(int type)
        {
            this.type = type;
        }
    }
}
