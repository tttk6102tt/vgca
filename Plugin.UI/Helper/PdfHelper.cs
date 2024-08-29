using Sign.itext.pdf;
using Sign.itext.text.pdf;
using Sign.PDF;

namespace Plugin.UI.Helper
{
    /// <summary>
    /// Helper_1
    /// </summary>
    internal class PdfHelper
    {
        public static List<SignatureInfo> getListSignInfo(
          string fileName)
        {
            List<SignatureInfo> pdfSignatureInfoList = new List<SignatureInfo>();
            PdfReader pdfReader = (PdfReader)null;
            try
            {
                pdfReader = new PdfReader(fileName);
                AcroFields acroFields = pdfReader.AcroFields;
                List<string> signatureNames = acroFields.GetSignatureNames();
                for (int index = 0; index < signatureNames.Count; ++index)
                {
                    SignatureInfo pdfSignatureInfo = new SignatureInfo();
                    string name = signatureNames[index];
                    pdfSignatureInfo.SignatureName = name;
                    pdfSignatureInfo.SignatureCoversWholeDocument = acroFields.SignatureCoversWholeDocument(name);
                    AcroFields.FieldPosition fieldPosition = acroFields.GetFieldPositions(name)[0];
                    pdfSignatureInfo.Position = new Rectangle((int)fieldPosition.position.Left, (int)fieldPosition.position.Top, (int)fieldPosition.position.Width, (int)fieldPosition.position.Height);
                    pdfSignatureInfo.PageIndex = fieldPosition.page;
                    pdfSignatureInfoList.Add(pdfSignatureInfo);
                }
                return pdfSignatureInfoList;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi định dạng tệp", ex);
            }
            finally
            {
                if (pdfReader != null)
                {
                    pdfReader.Close();
                }
            }
        }
    }
}
