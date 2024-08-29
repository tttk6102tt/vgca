using Sign.itext.text.pdf;
using System.Text;

namespace Sign.itext.pdf
{
    public class PRIndirectReference : PdfIndirectReference
    {
        protected PdfReader reader;

        public virtual PdfReader Reader => reader;

        public PRIndirectReference(PdfReader reader, int number, int generation)
        {
            type = 10;
            base.number = number;
            base.generation = generation;
            this.reader = reader;
        }

        public PRIndirectReference(PdfReader reader, int number)
            : this(reader, number, 0)
        {
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            int newObjectNumber = writer.GetNewObjectNumber(reader, number, generation);
            byte[] array = PdfEncodings.ConvertToBytes(new StringBuilder().Append(newObjectNumber).Append(" ").Append(reader.Appendable ? generation : 0)
                .Append(" R")
                .ToString(), null);
            os.Write(array, 0, array.Length);
        }

        public virtual void SetNumber(int number, int generation)
        {
            base.number = number;
            base.generation = generation;
        }
    }
}
