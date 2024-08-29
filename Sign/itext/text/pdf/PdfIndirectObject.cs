using Sign.itext.pdf;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class PdfIndirectObject
    {
        protected int number;

        protected int generation;

        internal static byte[] STARTOBJ = DocWriter.GetISOBytes(" obj\n");

        internal static byte[] ENDOBJ = DocWriter.GetISOBytes("\nendobj\n");

        internal static int SIZEOBJ = STARTOBJ.Length + ENDOBJ.Length;

        protected internal PdfObject objecti;

        protected internal PdfWriter writer;

        public virtual int Number => number;

        public virtual int Generation => generation;

        public virtual PdfIndirectReference IndirectReference => new PdfIndirectReference(objecti.Type, number, generation);

        public PdfIndirectObject(int number, PdfObject objecti, PdfWriter writer)
            : this(number, 0, objecti, writer)
        {
        }

        public PdfIndirectObject(PdfIndirectReference refi, PdfObject objecti, PdfWriter writer)
            : this(refi.Number, refi.Generation, objecti, writer)
        {
        }

        public PdfIndirectObject(int number, int generation, PdfObject objecti, PdfWriter writer)
        {
            this.writer = writer;
            this.number = number;
            this.generation = generation;
            this.objecti = objecti;
            PdfEncryption pdfEncryption = null;
            if (writer != null)
            {
                pdfEncryption = writer.Encryption;
            }

            pdfEncryption?.SetHashKey(number, generation);
        }

        public virtual void WriteTo(Stream os)
        {
            byte[] iSOBytes = DocWriter.GetISOBytes(number.ToString());
            os.Write(iSOBytes, 0, iSOBytes.Length);
            os.WriteByte(32);
            iSOBytes = DocWriter.GetISOBytes(generation.ToString());
            os.Write(iSOBytes, 0, iSOBytes.Length);
            os.Write(STARTOBJ, 0, STARTOBJ.Length);
            objecti.ToPdf(writer, os);
            os.Write(ENDOBJ, 0, ENDOBJ.Length);
        }

        public override string ToString()
        {
            return new StringBuilder().Append(number).Append(' ').Append(generation)
                .Append(" R: ")
                .Append((objecti != null) ? objecti.ToString() : "null")
                .ToString();
        }
    }
}
