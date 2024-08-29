using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public class PdfFunction
    {
        protected PdfWriter writer;

        protected PdfIndirectReference reference;

        protected PdfDictionary dictionary;

        internal PdfIndirectReference Reference
        {
            get
            {
                if (reference == null)
                {
                    reference = writer.AddToBody(dictionary).IndirectReference;
                }

                return reference;
            }
        }

        protected PdfFunction(PdfWriter writer)
        {
            this.writer = writer;
        }

        public static PdfFunction Type0(PdfWriter writer, float[] domain, float[] range, int[] size, int bitsPerSample, int order, float[] encode, float[] decode, byte[] stream)
        {
            PdfFunction pdfFunction = new PdfFunction(writer);
            pdfFunction.dictionary = new PdfStream(stream);
            ((PdfStream)pdfFunction.dictionary).FlateCompress(writer.CompressionLevel);
            pdfFunction.dictionary.Put(PdfName.FUNCTIONTYPE, new PdfNumber(0));
            pdfFunction.dictionary.Put(PdfName.DOMAIN, new PdfArray(domain));
            pdfFunction.dictionary.Put(PdfName.RANGE, new PdfArray(range));
            pdfFunction.dictionary.Put(PdfName.SIZE, new PdfArray(size));
            pdfFunction.dictionary.Put(PdfName.BITSPERSAMPLE, new PdfNumber(bitsPerSample));
            if (order != 1)
            {
                pdfFunction.dictionary.Put(PdfName.ORDER, new PdfNumber(order));
            }

            if (encode != null)
            {
                pdfFunction.dictionary.Put(PdfName.ENCODE, new PdfArray(encode));
            }

            if (decode != null)
            {
                pdfFunction.dictionary.Put(PdfName.DECODE, new PdfArray(decode));
            }

            return pdfFunction;
        }

        public static PdfFunction Type2(PdfWriter writer, float[] domain, float[] range, float[] c0, float[] c1, float n)
        {
            PdfFunction pdfFunction = new PdfFunction(writer);
            pdfFunction.dictionary = new PdfDictionary();
            pdfFunction.dictionary.Put(PdfName.FUNCTIONTYPE, new PdfNumber(2));
            pdfFunction.dictionary.Put(PdfName.DOMAIN, new PdfArray(domain));
            if (range != null)
            {
                pdfFunction.dictionary.Put(PdfName.RANGE, new PdfArray(range));
            }

            if (c0 != null)
            {
                pdfFunction.dictionary.Put(PdfName.C0, new PdfArray(c0));
            }

            if (c1 != null)
            {
                pdfFunction.dictionary.Put(PdfName.C1, new PdfArray(c1));
            }

            pdfFunction.dictionary.Put(PdfName.N, new PdfNumber(n));
            return pdfFunction;
        }

        public static PdfFunction Type3(PdfWriter writer, float[] domain, float[] range, PdfFunction[] functions, float[] bounds, float[] encode)
        {
            PdfFunction pdfFunction = new PdfFunction(writer);
            pdfFunction.dictionary = new PdfDictionary();
            pdfFunction.dictionary.Put(PdfName.FUNCTIONTYPE, new PdfNumber(3));
            pdfFunction.dictionary.Put(PdfName.DOMAIN, new PdfArray(domain));
            if (range != null)
            {
                pdfFunction.dictionary.Put(PdfName.RANGE, new PdfArray(range));
            }

            PdfArray pdfArray = new PdfArray();
            for (int i = 0; i < functions.Length; i++)
            {
                pdfArray.Add(functions[i].Reference);
            }

            pdfFunction.dictionary.Put(PdfName.FUNCTIONS, pdfArray);
            pdfFunction.dictionary.Put(PdfName.BOUNDS, new PdfArray(bounds));
            pdfFunction.dictionary.Put(PdfName.ENCODE, new PdfArray(encode));
            return pdfFunction;
        }

        public static PdfFunction Type4(PdfWriter writer, float[] domain, float[] range, string postscript)
        {
            byte[] array = new byte[postscript.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (byte)postscript[i];
            }

            PdfFunction obj = new PdfFunction(writer)
            {
                dictionary = new PdfStream(array)
            };
            ((PdfStream)obj.dictionary).FlateCompress(writer.CompressionLevel);
            obj.dictionary.Put(PdfName.FUNCTIONTYPE, new PdfNumber(4));
            obj.dictionary.Put(PdfName.DOMAIN, new PdfArray(domain));
            obj.dictionary.Put(PdfName.RANGE, new PdfArray(range));
            return obj;
        }
    }
}
