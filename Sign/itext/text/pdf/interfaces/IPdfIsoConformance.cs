namespace Sign.itext.text.pdf.interfaces
{
    public interface IPdfIsoConformance
    {
        bool IsPdfIso();

        void CheckPdfIsoConformance(int key, object obj1);
    }
}
