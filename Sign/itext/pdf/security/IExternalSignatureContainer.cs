namespace Sign.itext.pdf.security
{
    public interface IExternalSignatureContainer
    {
        byte[] Sign(Stream data);

        void ModifySigningDictionary(PdfDictionary signDic);
    }
}
