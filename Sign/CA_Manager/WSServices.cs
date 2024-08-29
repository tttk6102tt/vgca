using Sign.itext.text.pdf;
using Sign.PDF;
using System.Security.Cryptography.X509Certificates;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Sign.CA_Manager
{
    public class WSServices : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                var log = new LogRepository();
                log.Save("", "Đã vào trình ký", "", "");
                var data = Convert.FromBase64String(e.Data);
                if (data != null)
                {
                    log.Save("", "File có giá trị", "", "");
                    var signData = SigPDF(data);
                    if (signData != null)
                    {
                        log.Save("", "Ký thành công", "", "");
                        Send(signData);
                    }
                    else
                    {
                        log.Save("", "Ký thất bại: File output = null", "", "");
                        Send("Fail");
                    }
                }
                else
                {
                    log.Save("", "Ký thất bại: File input = null", "", "");
                    Send("Fail");
                }
            }
            catch (Exception ex)
            {
                Send(ex.Message);
            }

        }

        /// <summary>
        /// Thực hiện ký file
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] SigPDF(byte[] fi)
        {
            X509Certificate2Collection keyStore = new X509Certificate2Collection();
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            keyStore.AddRange(store.Certificates);
            store.Close();
            //Chung thu so nguoi ky
            X509Certificate2 cert = null;
            //[1] Chon chung thu so
            Console.WriteLine("Chon chung thu so ky");
            try
            {
                foreach (X509Certificate2 item in keyStore)
                {
                    if (item.Subject.Contains("Chứng thư số ký"))
                    {
                        cert = item;
                        break;
                    }
                }
                //cert = X509Certificate2UI.SelectFromCollection(keyStore, "Chứng thư số ký", "Chọn chứng thư số ký", X509SelectionFlag.SingleSelection)[0];
                Console.WriteLine(cert.Subject);
            }
            catch { }

            if (cert == null)
            {
                Console.WriteLine("Chua chon chung thu so ky");
                var log = new LogRepository();
                log.Save("", "Chưa chọn chứng thư số", "", "");
                return null;
            }
            // Khởi tạo ký
            PdfSigner pdf = new PdfSigner(fi, cert);
            pdf.SigningLocation = "Hà Nội";
            pdf.SigningReason = "Ký test";
            pdf.SignatureAppearance = PdfSignatureAppearance.RenderingMode.DESCRIPTION;
            // pdf.SignatureAppearance = PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION;
            // pdf.SignatureAppearance = PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION;
            //Image image = Image.FromFile("Anh_chu_ky_so.png");
            //pdf.SignatureImage = (Bitmap)image;
            //pdf.SignatureImage = (Bitmap)image;
            pdf.TsaUrl = "http://ca.gov.vn/tsa";
            try
            {
                return pdf.SignByFile(1, 450, 700, 70, 70, 0); //iPage: trang; llx: toa do X, lly: Toa do y; iWidth: rong; iHeight: cao
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi: " + ex.Message);
                var log = new LogRepository();
                log.Save("", "Ký PDF lỗi", ex.Message, ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// Ký file đang dùng cho desktop
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        public byte[] SignatureFile(byte[] fi)
        {
            X509Certificate2Collection keyStore = new X509Certificate2Collection();

            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            //store.Open(OpenFlags.OpenExistingOnly);
            keyStore.AddRange(store.Certificates);
            store.Close();
            //Chung thu so nguoi ky
            X509Certificate2 cert = null;
            //[1] Chon chung thu so
            Console.WriteLine("Chon chung thu so ky");
            try
            {
                //foreach (X509Certificate2 item in store.Certificates)
                //{
                //    if (item.Subject.Contains("efy"))
                //    {
                //        cert = item;
                //        break;
                //    }
                //}
                cert = X509Certificate2UI.SelectFromCollection(keyStore, "Chứng thư số ký", "Chọn chứng thư số ký", X509SelectionFlag.SingleSelection)[0];
                Console.WriteLine(cert.Subject);
            }
            catch { }

            if (cert == null)
            {
                Console.WriteLine("Chua chon chung thu so ky");
                var log = new LogRepository();
                log.Save("", "Chưa chọn chứng thư số", "", "");
                return null;
            }
            // Khởi tạo ký
            PdfSigner pdf = new PdfSigner(fi, cert);
            pdf.SigningLocation = "Hà Nội";
            pdf.SigningReason = "Ký test";
            pdf.SignatureAppearance = PdfSignatureAppearance.RenderingMode.DESCRIPTION;
            //pdf.SignatureAppearance = PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION;
            //Image image = Image.FromFile("Anh_chu_ky_so.png");
            //pdf.SignatureImage = (Bitmap)image;
            //pdf.SignatureImage = (Bitmap)image;
            pdf.TsaUrl = "http://ca.gov.vn/tsa";
            try
            {
                return pdf.SignByFile(1, 250, 550, 70, 70, 0); //iPage: trang; llx: toa do X, lly: Toa do y; iWidth: rong; iHeight: cao
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi: " + ex.Message);
                var log = new LogRepository();
                log.Save("", "Ký PDF lỗi", ex.Message, ex.StackTrace);
                throw ex;
            }
        }
    }
}
