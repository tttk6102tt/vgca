namespace Sign.itext.text.pdf
{
    public class NumberArray : PdfArray
    {
        public NumberArray()
        {
        }

        public NumberArray(float[] numbers)
        {
            foreach (float value in numbers)
            {
                Add(new PdfNumber(value));
            }
        }

        public NumberArray(float n1)
        {
            Add(new PdfNumber(n1));
        }

        public NumberArray(float n1, float n2)
        {
            Add(new PdfNumber(n1));
            Add(new PdfNumber(n2));
        }

        public NumberArray(float n1, float n2, float n3)
        {
            Add(new PdfNumber(n1));
            Add(new PdfNumber(n2));
            Add(new PdfNumber(n3));
        }

        public NumberArray(float n1, float n2, float n3, float n4)
        {
            Add(new PdfNumber(n1));
            Add(new PdfNumber(n2));
            Add(new PdfNumber(n3));
            Add(new PdfNumber(n4));
        }

        public NumberArray(float n1, float n2, float n3, float n4, float n5)
        {
            Add(new PdfNumber(n1));
            Add(new PdfNumber(n2));
            Add(new PdfNumber(n3));
            Add(new PdfNumber(n4));
            Add(new PdfNumber(n5));
        }

        public NumberArray(float n1, float n2, float n3, float n4, float n5, float n6)
        {
            Add(new PdfNumber(n1));
            Add(new PdfNumber(n2));
            Add(new PdfNumber(n3));
            Add(new PdfNumber(n4));
            Add(new PdfNumber(n5));
            Add(new PdfNumber(n6));
        }

        public NumberArray(IList<PdfNumber> numbers)
        {
            foreach (PdfNumber number in numbers)
            {
                Add(number);
            }
        }
    }
}
