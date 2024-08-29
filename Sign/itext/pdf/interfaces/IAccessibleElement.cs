namespace Sign.itext.pdf.interfaces
{
    public interface IAccessibleElement
    {
        PdfName Role { get; set; }

        AccessibleElementId ID { get; set; }

        bool IsInline { get; }

        PdfObject GetAccessibleAttribute(PdfName key);

        void SetAccessibleAttribute(PdfName key, PdfObject value);

        Dictionary<PdfName, PdfObject> GetAccessibleAttributes();
    }
}
