namespace Sign.itext.text.pdf.interfaces
{
    public interface IPdfAnnotations
    {
        PdfAcroForm AcroForm { get; }

        int SigFlags { set; }

        void AddAnnotation(PdfAnnotation annot);

        void AddCalculationOrder(PdfFormField annot);
    }
}
