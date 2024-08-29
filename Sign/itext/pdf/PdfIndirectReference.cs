using System.Text;

namespace Sign.itext.pdf
{
    public class PdfIndirectReference : PdfObject
    {
        protected int number;

        protected int generation;

        public virtual int Number => number;

        public virtual int Generation => generation;

        protected PdfIndirectReference()
            : base(0)
        {
        }

        internal PdfIndirectReference(int type, int number, int generation)
            : base(0, new StringBuilder().Append(number).Append(' ').Append(generation)
                .Append(" R")
                .ToString())
        {
            this.number = number;
            this.generation = generation;
        }

        protected internal PdfIndirectReference(int type, int number)
            : this(type, number, 0)
        {
        }

        public override string ToString()
        {
            return new StringBuilder().Append(number).Append(' ').Append(generation)
                .Append(" R")
                .ToString();
        }
    }
}
