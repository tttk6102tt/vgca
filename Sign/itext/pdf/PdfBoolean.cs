using Sign.itext.error_messages;

namespace Sign.itext.pdf
{
    public class PdfBoolean : PdfObject
    {
        public static readonly PdfBoolean PDFTRUE = new PdfBoolean(value: true);

        public static readonly PdfBoolean PDFFALSE = new PdfBoolean(value: false);

        public const string TRUE = "true";

        public const string FALSE = "false";

        private bool value;

        public virtual bool BooleanValue => value;

        public PdfBoolean(bool value)
            : base(1)
        {
            if (value)
            {
                Content = "true";
            }
            else
            {
                Content = "false";
            }

            this.value = value;
        }

        public PdfBoolean(string value)
            : base(1, value)
        {
            if (value.Equals("true"))
            {
                this.value = true;
                return;
            }

            if (value.Equals("false"))
            {
                this.value = false;
                return;
            }

            throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("the.value.has.to.be.true.of.false.instead.of.1", value));
        }

        public override string ToString()
        {
            if (!value)
            {
                return "false";
            }

            return "true";
        }
    }
}
