using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfDestination : PdfArray
    {
        public const int XYZ = 0;

        public const int FIT = 1;

        public const int FITH = 2;

        public const int FITV = 3;

        public const int FITR = 4;

        public const int FITB = 5;

        public const int FITBH = 6;

        public const int FITBV = 7;

        private bool status;

        public PdfDestination(int type)
        {
            if (type == 5)
            {
                Add(PdfName.FITB);
            }
            else
            {
                Add(PdfName.FIT);
            }
        }

        public PdfDestination(int type, float parameter)
            : base(new PdfNumber(parameter))
        {
            switch (type)
            {
                default:
                    AddFirst(PdfName.FITH);
                    break;
                case 3:
                    AddFirst(PdfName.FITV);
                    break;
                case 6:
                    AddFirst(PdfName.FITBH);
                    break;
                case 7:
                    AddFirst(PdfName.FITBV);
                    break;
            }
        }

        public PdfDestination(int type, float left, float top, float zoom)
            : base(PdfName.XYZ)
        {
            if (left < 0f)
            {
                Add(PdfNull.PDFNULL);
            }
            else
            {
                Add(new PdfNumber(left));
            }

            if (top < 0f)
            {
                Add(PdfNull.PDFNULL);
            }
            else
            {
                Add(new PdfNumber(top));
            }

            Add(new PdfNumber(zoom));
        }

        public PdfDestination(int type, float left, float bottom, float right, float top)
            : base(PdfName.FITR)
        {
            Add(new PdfNumber(left));
            Add(new PdfNumber(bottom));
            Add(new PdfNumber(right));
            Add(new PdfNumber(top));
        }

        public PdfDestination(string dest)
        {
            string[] array = dest.Trim().Split((char[]?)null);
            if (array.Length != 0)
            {
                Add(new PdfName(array[0]));
            }

            for (int i = 1; i < array.Length; i++)
            {
                if (array[i].Length == 0)
                {
                    continue;
                }

                if ("null".Equals(array[i]))
                {
                    Add(new PdfNull());
                    continue;
                }

                try
                {
                    Add(new PdfNumber(array[i]));
                }
                catch (Exception)
                {
                    Add(new PdfNull());
                }
            }
        }

        public virtual bool HasPage()
        {
            return status;
        }

        public virtual bool AddPage(PdfIndirectReference page)
        {
            if (!status)
            {
                AddFirst(page);
                status = true;
                return true;
            }

            return false;
        }
    }
}
