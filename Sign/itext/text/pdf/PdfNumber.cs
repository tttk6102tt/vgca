using Sign.itext.error_messages;
using Sign.itext.pdf;
using System.Globalization;

namespace Sign.itext.text.pdf
{
    public class PdfNumber : PdfObject
    {
        private double value;

        public virtual int IntValue => (int)value;

        public virtual long LongValue => (long)value;

        public virtual double DoubleValue => value;

        public virtual float FloatValue => (float)value;

        public PdfNumber(string content)
            : base(2)
        {
            try
            {
                value = double.Parse(content.Trim(), NumberFormatInfo.InvariantInfo);
                Content = content;
            }
            catch (Exception ex)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("1.is.not.a.valid.number.2", content, ex.ToString()));
            }
        }

        public PdfNumber(int value)
            : base(2)
        {
            this.value = value;
            Content = value.ToString();
        }

        public PdfNumber(long value)
            : base(2)
        {
            this.value = value;
            Content = value.ToString();
        }

        public PdfNumber(double value)
            : base(2)
        {
            this.value = value;
            Content = ByteBuffer.FormatDouble(value);
        }

        public PdfNumber(float value)
            : this((double)value)
        {
        }

        public virtual void Increment()
        {
            value += 1.0;
            Content = ByteBuffer.FormatDouble(value);
        }
    }
}
