namespace Sign.itext.pdf
{
    public class PdfTransparencyGroup : PdfDictionary
    {
        public virtual bool Isolated
        {
            set
            {
                if (value)
                {
                    Put(PdfName.I, PdfBoolean.PDFTRUE);
                }
                else
                {
                    Remove(PdfName.I);
                }
            }
        }

        public virtual bool Knockout
        {
            set
            {
                if (value)
                {
                    Put(PdfName.K, PdfBoolean.PDFTRUE);
                }
                else
                {
                    Remove(PdfName.K);
                }
            }
        }

        public PdfTransparencyGroup()
        {
            Put(PdfName.S, PdfName.TRANSPARENCY);
        }
    }
}
