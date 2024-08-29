using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfVisibilityExpression : PdfArray
    {
        public const int OR = 0;

        public const int AND = 1;

        public const int NOT = -1;

        public PdfVisibilityExpression(int type)
        {
            switch (type)
            {
                case 0:
                    base.Add(PdfName.OR);
                    break;
                case 1:
                    base.Add(PdfName.AND);
                    break;
                case -1:
                    base.Add(PdfName.NOT);
                    break;
                default:
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("illegal.ve.value"));
            }
        }

        public override void Add(int index, PdfObject element)
        {
            throw new ArgumentException(MessageLocalization.GetComposedMessage("illegal.ve.value"));
        }

        public override bool Add(PdfObject obj)
        {
            if (obj is PdfLayer)
            {
                return base.Add(((PdfLayer)obj).Ref);
            }

            if (obj is PdfVisibilityExpression)
            {
                return base.Add(obj);
            }

            throw new ArgumentException(MessageLocalization.GetComposedMessage("illegal.ve.value"));
        }

        public override void AddFirst(PdfObject obj)
        {
            throw new ArgumentException(MessageLocalization.GetComposedMessage("illegal.ve.value"));
        }

        public override bool Add(float[] values)
        {
            throw new ArgumentException(MessageLocalization.GetComposedMessage("illegal.ve.value"));
        }

        public override bool Add(int[] values)
        {
            throw new ArgumentException(MessageLocalization.GetComposedMessage("illegal.ve.value"));
        }
    }
}
