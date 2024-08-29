namespace Sign.itext.text.pdf.interfaces
{
    public interface IPdfXConformance : IPdfIsoConformance
    {
        int PDFXConformance { get; set; }

        bool IsPdfX();
    }
}
