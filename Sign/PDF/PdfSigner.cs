using Sign.itext;
using Sign.itext.awt.geom;
using Sign.itext.pdf;
using Sign.itext.text;
using Sign.itext.text.pdf;
using Sign.itext.text.pdf.security;
using Sign.Org.BouncyCastle.X509;
using Sign.Properties;
using Sign.X509;
using System.Globalization;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Sign.PDF
{

    public class PdfSigner
    {

        /// <summary>
        /// Create By DoanhLV - File cần ký 
        /// </summary>
        private byte[] _fileData;
        /// <summary>
        /// Lý do ký
        /// </summary>
        private string _signingReason;
        /// <summary>
        /// vị trí thực hiện ký
        /// </summary>
        private string _signingLocation;

        /// <summary>
        /// đường dẫn file đầu vào
        /// </summary>
        private string _inputFilePath;

        /// <summary>
        /// đường dẫn file đầu ra
        /// </summary>
        private string _outputFilePath;

        /// <summary>
        /// Xác thực Cer
        /// </summary>
        private X509Certificate2 _certificate;

        /// <summary>
        /// Ảnh chữ ký
        /// </summary>
        private System.Drawing.Image _signatureImage;

        /// <summary>
        /// lấy vị trí ký
        /// </summary>
        public string SigningLocation
        {
            get
            {
                return _signingLocation;
            }
            set
            {
                _signingLocation = value;
            }
        }
        /// <summary>
        /// Lấy lý do ký
        /// </summary>
        public string SigningReason
        {
            get
            {
                return _signingReason;
            }
            set
            {
                _signingReason = value;
            }
        }

        /// <summary>
        /// Link cer
        /// </summary>
        private string _tsaUrl = "http://ca.gov.vn/tsa";

        /// <summary>
        /// Lấy link cer
        /// </summary>
        public string TsaUrl
        {
            get
            {
                return _tsaUrl;
            }
            set
            {
                _tsaUrl = value;
            }
        }

        private string Sha1Algorithm = "SHA1";

        /// <summary>
        /// File ảnh ký
        /// </summary>
        private PdfSignatureAppearance.RenderingMode _signatureAppearanceMode = PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION;

        private object _syncLock = new object();
        /// <summary>
        /// Hiển thị tên người ký
        /// </summary>
        public bool ShowLabel { get; set; } = true;
        /// <summary>
        /// Hiển thị email
        /// </summary>
        public bool ShowEmail { get; set; } = true;
        /// <summary>
        /// Hiển thị thông tin cơ quan 1
        /// </summary>
        public bool ShowCQ1 { get; set; } = true;
        /// <summary>
        /// Hiển thị ngày ký
        /// </summary>
        public bool ShowDate { get; set; } = true;
        /// <summary>
        /// Sử dụng thông tin tổ chức
        /// </summary>
        public bool IsOrgProfile { get; set; } = true;
        /// <summary>
        /// Hiển thị thông tin cơ quan 2
        /// </summary>
        public bool ShowCQ2 { get; set; } = true;
        /// <summary>
        /// Hiển thị thông tin cơ quan 3
        /// </summary>
        public bool ShowCQ3 { get; set; } = true;
        /// <summary>
        /// Chức vụ
        /// </summary>
        private string _orgPosition;

        public PdfSignatureAppearance.RenderingMode SignatureAppearance
        {
            get
            {
                return _signatureAppearanceMode;
            }
            set
            {
                _signatureAppearanceMode = value;
            }
        }

        public System.Drawing.Image SignatureImage
        {
            get
            {
                return _signatureImage;
            }
            set
            {
                _signatureImage = value;
            }
        }
        public PdfSigner(string inputFilePath, string outputFilePath, X509Certificate2 certificate)
        {
            _inputFilePath = inputFilePath;
            _outputFilePath = outputFilePath;
            _certificate = certificate;
            //_tsaUrl = null;
            //Authorization.smethod_1();
        }
        public PdfSigner(string inputFilePath, X509Certificate2 certificate)
        {
            _inputFilePath = inputFilePath;
            _certificate = certificate;
            //_tsaUrl = null;
            //Authorization.smethod_1();
        }
        /// <summary>
        /// Create by DoanhLV - thực hiện ký vs input là byte
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="certificate"></param>
        public PdfSigner(byte[] fileData, X509Certificate2 certificate)
        {
            _fileData = fileData;
            _certificate = certificate;
        }

        private string GenerateSignatureText()
        {
            CertInfo certInfo = new CertInfo(_certificate);
            StringBuilder stringBuilder = new StringBuilder();
            string commonName = certInfo.CommonName;
            string email = certInfo.Email;
            string value = string.IsNullOrEmpty(certInfo.OU) ? certInfo.O : (certInfo.OU + ", " + certInfo.O);
            if (ShowLabel)
            {
                stringBuilder.Append("Người ký: " + commonName);
                if (ShowEmail && !string.IsNullOrEmpty(email))
                {
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append("Email: ");
                    stringBuilder.Append(email);
                }
                if (ShowCQ1)
                {
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append("Cơ quan: ");
                    stringBuilder.Append(value);
                }
                if (IsOrgProfile && !string.IsNullOrEmpty(_orgPosition))
                {
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append("Chức vụ: ");
                    stringBuilder.Append(_orgPosition);
                }
                if (ShowDate)
                {
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append("Thời gian ký: ");
                    stringBuilder.Append(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss zzz", CultureInfo.CreateSpecificCulture("en-US")));
                }
            }
            else
            {
                stringBuilder.Append(commonName);
                if (ShowEmail && !string.IsNullOrEmpty(email))
                {
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append(email);
                }
                if (ShowCQ1)
                {
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append(value);
                }
                if (IsOrgProfile && !string.IsNullOrEmpty(_orgPosition))
                {
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append(_orgPosition);
                }
                if (ShowDate)
                {
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss zzz", CultureInfo.CreateSpecificCulture("en-US")));
                }
            }
            return stringBuilder.ToString();
        }


        private void DrawSignatureContent(PdfSignatureAppearance signatureAppearance, Rectangle signatureRectangle, int rotation = 1)
        {
            Rectangle imageRectangle = null;
            Rectangle textRectangle = null;
            float signatureWidth = signatureRectangle.Width;
            float signatureHeight = signatureRectangle.Height;
            AffineTransform transformationMatrix = new AffineTransform();
            switch (rotation)
            {
                case 1:
                    signatureWidth = signatureRectangle.Height;
                    signatureHeight = signatureRectangle.Width;
                    transformationMatrix = new AffineTransform(0f, 1f, -1f, 0f, signatureRectangle.Width, 0f);
                    break;
                case 2:
                    transformationMatrix = new AffineTransform(-1f, 0f, 0f, -1f, signatureRectangle.Width, signatureRectangle.Height);
                    break;
                case 3:
                    signatureWidth = signatureRectangle.Height;
                    signatureHeight = signatureRectangle.Width;
                    transformationMatrix = new AffineTransform(0f, -1f, 1f, 0f, 0f, signatureRectangle.Height);
                    break;
            }
            if (signatureAppearance.SignatureRenderingMode != PdfSignatureAppearance.RenderingMode.NAME_AND_DESCRIPTION
                && (signatureAppearance.SignatureRenderingMode != PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION
                || signatureAppearance.SignatureGraphic == null))
            {
                if (signatureAppearance.SignatureRenderingMode == PdfSignatureAppearance.RenderingMode.GRAPHIC)
                {
                    imageRectangle = new Rectangle(0f, 0f, signatureWidth, signatureHeight);
                }
                else
                {
                    textRectangle = new Rectangle(0f, 0f, signatureWidth, signatureHeight);
                    imageRectangle = new Rectangle(0f, 0f, signatureWidth - 0f, signatureHeight * 1f - 0f);
                }
            }
            else
            {
                textRectangle = new Rectangle(0f, 0f, signatureWidth / 2f, signatureHeight);
                imageRectangle = new Rectangle(signatureWidth / 2f, 0f, signatureWidth, signatureHeight);

                if (signatureHeight > signatureWidth)
                {
                    textRectangle = new Rectangle(0f, signatureHeight / 2f, signatureWidth, signatureHeight);
                    imageRectangle = new Rectangle(0f, 0f, signatureWidth, signatureHeight / 2f);
                }
            }
            PdfTemplate contentLayer = signatureAppearance.GetLayer(2);
            if (rotation != 0)
            {
                contentLayer.Transform(transformationMatrix);
            }
            if (signatureAppearance.SignatureRenderingMode == PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION)
            {
                ColumnText columnText = new ColumnText(contentLayer);
                columnText.RunDirection = 1;
                columnText.SetSimpleColumn(textRectangle.Left, textRectangle.Bottom, textRectangle.Right, textRectangle.Top, 0f, Element.ALIGN_CENTER);
                Image signatureGraphic = signatureAppearance.SignatureGraphic;
                signatureGraphic.ScaleToFit(textRectangle.Width, textRectangle.Height);
                Paragraph paragraph = new Paragraph();
                float imageOffsetX = 0f;
                float imageOffsetY = 0f - signatureGraphic.ScaledHeight + 15f;
                imageOffsetX += (textRectangle.Width - signatureGraphic.ScaledWidth) / 2f;
                paragraph.Add(new Chunk(offsetY: imageOffsetY - (textRectangle.Height - signatureGraphic.ScaledHeight) / 2f, image: signatureGraphic, offsetX: imageOffsetX + (textRectangle.Width - signatureGraphic.ScaledWidth) / 2f, changeLeading: false));
                columnText.AddElement(paragraph);
                columnText.Go();
            }
            else if (signatureAppearance.SignatureRenderingMode == PdfSignatureAppearance.RenderingMode.GRAPHIC)
            {
                ColumnText columnText = new ColumnText(contentLayer);
                columnText.RunDirection = 1;
                columnText.SetSimpleColumn(imageRectangle.Left, imageRectangle.Bottom, imageRectangle.Right, imageRectangle.Top, 0f, 5);

                Image signatureGraphic = signatureAppearance.SignatureGraphic;
                signatureGraphic.ScaleToFit(imageRectangle.Width, imageRectangle.Height);
                Paragraph paragraph = new Paragraph(imageRectangle.Height);
                float imageOffsetX = (imageRectangle.Width - signatureGraphic.ScaledWidth) / 2f;
                float imageOffsetY = (imageRectangle.Height - signatureGraphic.ScaledHeight) / 2f;
                paragraph.Add(new Chunk(signatureGraphic, imageOffsetX, imageOffsetY, changeLeading: false));
                columnText.AddElement(paragraph);
                columnText.Go();
                //ColumnText columnText = new ColumnText(contentLayer);
                //columnText.RunDirection = 1;
                //columnText.SetSimpleColumn(textRectangle.Left, textRectangle.Bottom, textRectangle.Right, textRectangle.Top, 0f, 5);
                //Image signatureGraphic = signatureAppearance.SignatureGraphic;
                //signatureGraphic.ScaleToFit(textRectangle.Width, textRectangle.Height);
                //Paragraph paragraph = new Paragraph(textRectangle.Height);
                //float imageOffsetX = (textRectangle.Width - signatureGraphic.ScaledWidth) / 2f;
                //float imageOffsetY = (textRectangle.Height - signatureGraphic.ScaledHeight) / 2f;
                //paragraph.Add(new Chunk(signatureGraphic, imageOffsetX, imageOffsetY, changeLeading: false));
                //columnText.AddElement(paragraph);
                //columnText.Go();
            }
            float fontSize = signatureAppearance.Layer2Font.CalculatedSize;
            if (signatureAppearance.SignatureRenderingMode != PdfSignatureAppearance.RenderingMode.GRAPHIC)
            {
                Rectangle rect = new Rectangle(imageRectangle.Width, imageRectangle.Height);
                fontSize = ColumnText.FitText(signatureAppearance.Layer2Font, signatureAppearance.Layer2Text, rect, 70f, 1);
                ColumnText columnText3 = new ColumnText(contentLayer);
                columnText3.RunDirection = 1;
                columnText3.SetSimpleColumn(new Phrase(signatureAppearance.Layer2Text, signatureAppearance.Layer2Font), imageRectangle.Left, imageRectangle.Bottom, imageRectangle.Right, imageRectangle.Top, fontSize, 0);
                columnText3.Go();
            }
        }
        public void Sign(int iPage, int llx, int lly, int iWidth, int iHeight, int rotation = 0)
        {
            FileStream fileStream = null;
            try
            {
                if (_certificate == null)
                {
                    throw new Exception("Không có chứng thư số ký");
                }
                if (!File.Exists(_inputFilePath))
                {
                    throw new Exception("Tệp đầu vào không tồn tại");
                }
                lock (_syncLock)
                {
                    X509CertificateParser x509CertificateParser = new X509CertificateParser();
                    Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[1]
                    {
                        x509CertificateParser.ReadCertificate(_certificate.RawData)
                    };
                    PdfReader pdfReader = new PdfReader(_inputFilePath);
                    if (pdfReader.IsEncrypted())
                    {
                        pdfReader.Close();
                        throw new Exception("Tài liệu pdf đã được mã hóa");
                    }
                    try
                    {
                        fileStream = new FileStream(_outputFilePath, FileMode.Create);
                    }
                    catch (SecurityException)
                    {
                        throw new Exception("Không có quyền truy cập đường dẫn lưu tệp ký số");
                    }
                    catch (IOException)
                    {
                        throw new Exception("Đường dẫn lưu tệp ký số không hợp lệ");
                    }
                    bool hasExistingSignatures = pdfReader.AcroFields.GetSignatureNames().Count > 0;
                    PdfSignatureAppearance signatureAppearance = PdfStamper.CreateSignature(pdfReader, fileStream, '\0', null, append: true).SignatureAppearance;


                    //signatureAppearance.SetVisibleSignature(new Rectangle(llx, lly, llx + iWidth, lly + iHeight), iPage, signatureAppearance.GetNewSigName());//=> Gốc

                    var olx = llx - iWidth / 2;
                    var oly = lly + iHeight / 2;
                    var urx = llx + iWidth / 2;
                    var ury = lly - iHeight / 2;
                    signatureAppearance.SetVisibleSignature(new Rectangle(olx, oly, urx, ury), iPage, signatureAppearance.GetNewSigName());

                    signatureAppearance.SignatureRenderingMode = _signatureAppearanceMode;
                    signatureAppearance.SignDate = DateTime.Now;
                    signatureAppearance.Reason = _signingReason;
                    signatureAppearance.Location = _signingLocation;
                    signatureAppearance.Acro6Layers = true;
                    if (!hasExistingSignatures)
                    {
                        signatureAppearance.CertificationLevel = 2;
                    }
                    new CertInfo(_certificate.RawData);
                    string signatureText = GenerateSignatureText();
                    float fontSize = 5f;
                    byte[] times = Resources.times;
                    BaseFont baseFont = BaseFont.CreateFont("times.ttf", "Identity-H", embedded: true, cached: true, ttfAfm: times, pfb: null);
                    baseFont.Subset = true;
                    Font font = new Font(baseFont, fontSize, 0);

                    Rectangle rectangle = new Rectangle(llx, lly, llx + iWidth - 2, lly + iHeight - 2);
                    if (_signatureAppearanceMode == PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION || _signatureAppearanceMode == PdfSignatureAppearance.RenderingMode.GRAPHIC)
                    {
                        signatureAppearance.SignatureGraphic = Image.GetInstance(_signatureImage, _signatureImage.RawFormat);
                    }
                    fontSize = ColumnText.FitText(font, signatureText, rectangle, 70f, 1);
                    signatureAppearance.Layer2Text = signatureText;
                    signatureAppearance.Layer2Font = font;

                    DrawSignatureContent(signatureAppearance, rectangle, rotation);



                    ITSAClient tsaClient = null;
                    if (!string.IsNullOrEmpty(_tsaUrl))
                    {
                        tsaClient = new TSAClientBouncyCastle(_tsaUrl, null, null, 4096, Sha1Algorithm.ToUpper());
                    }
                    try
                    {
                        IExternalSignature externalSignature = new X509Signature(_certificate, "SHA-256");
                        MakeSignature.SignDetached(signatureAppearance, externalSignature, chain, null, null, tsaClient, 0, CryptoStandard.CMS);
                    }
                    catch (Exception ex3)
                    {
                        throw new Exception("Ký số không thành công: " + ex3.Message, ex3);
                    }
                }
            }
            catch (Exception ex4)
            {
                fileStream?.Close();
                if (File.Exists(_outputFilePath))
                {
                    try
                    {
                        File.Delete(_outputFilePath);
                    }
                    catch
                    {
                    }
                }
                throw ex4;
            }
            finally
            {
                fileStream?.Close();
            }
        }

        public byte[] SignToByteArray(int iPage, int llx, int lly, int iWidth, int iHeight, int rotation = 0)
        {
            try
            {
                if (_certificate == null)
                {
                    throw new Exception("Không có chứng thư số ký");
                }
                if (!File.Exists(_inputFilePath))
                {
                    throw new Exception("Tệp đầu vào không tồn tại");
                }
                lock (_syncLock)
                {
                    X509CertificateParser x509CertificateParser = new X509CertificateParser();
                    Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[1]
                    {
                        x509CertificateParser.ReadCertificate(_certificate.RawData)
                    };
                    PdfReader pdfReader = new PdfReader(_inputFilePath);
                    if (pdfReader.IsEncrypted())
                    {
                        pdfReader.Close();
                        throw new Exception("Tài liệu pdf đã được mã hóa");
                    }

                    bool num = pdfReader.AcroFields.GetSignatureNames().Count > 0;

                    MemoryStream ms = new MemoryStream();
                    PdfSignatureAppearance signatureAppearance = PdfStamper.CreateSignature(pdfReader, ms, '\0', null, append: true).SignatureAppearance;
                    signatureAppearance.SetVisibleSignature(new Rectangle(llx, lly, llx + iWidth, lly + iHeight), iPage, signatureAppearance.GetNewSigName());
                    signatureAppearance.SignatureRenderingMode = _signatureAppearanceMode;
                    signatureAppearance.SignDate = DateTime.Now;
                    signatureAppearance.Reason = _signingReason;
                    signatureAppearance.Location = _signingLocation;
                    signatureAppearance.Acro6Layers = true;
                    if (!num)
                    {
                        signatureAppearance.CertificationLevel = 2;
                    }
                    new CertInfo(_certificate.RawData);
                    string text = GenerateSignatureText();
                    float size = 5f;
                    byte[] times = Resources.times;
                    BaseFont baseFont = BaseFont.CreateFont("times.ttf", "Identity-H", embedded: true, cached: true, ttfAfm: times, pfb: null);
                    baseFont.Subset = true;
                    Font font = new Font(baseFont, size, 0);
                    Rectangle rectangle = new Rectangle(llx, lly, llx + iWidth - 2, lly + iHeight - 2);
                    if (_signatureAppearanceMode != 0)
                    {
                        signatureAppearance.SignatureGraphic = Image.GetInstance(_signatureImage, _signatureImage.RawFormat);
                    }
                    size = ColumnText.FitText(font, text, rectangle, 70f, 1);
                    signatureAppearance.Layer2Text = text;
                    signatureAppearance.Layer2Font = font;
                    DrawSignatureContent(signatureAppearance, rectangle, rotation);
                    ITSAClient tsaClient = null;
                    if (!string.IsNullOrEmpty(_tsaUrl))
                    {
                        //tsaClient = new TSAClientBouncyCastle(_tsaUrl, null, null, 4096, Sha1Algorithm.ToUpper());
                    }
                    try
                    {
                        IExternalSignature externalSignature = new X509Signature(_certificate, "SHA-256");
                        //MakeSignature.SignDetached(signatureAppearance, externalSignature, chain, null, null, tsaClient, 0, CryptoStandard.CMS);
                    }
                    catch (Exception ex3)
                    {
                        throw new Exception("Ký số không thành công: " + ex3.Message, ex3);
                    }
                    return ms.ToArray();
                }
            }
            catch (Exception ex4)
            {
                if (File.Exists(_outputFilePath))
                {
                    try
                    {
                        File.Delete(_outputFilePath);
                    }
                    catch
                    {
                    }
                }
                throw ex4;
            }
            finally
            {
            }
        }


        /// <summary>
        /// Create by DoanhLV - Thực hiện ký số với input/output là byte
        /// </summary>
        /// <param name="iPage">Trang</param>
        /// <param name="llx">Tọa độ X</param>
        /// <param name="lly">Tọa độ Y</param>
        /// <param name="iWidth">Chiều rộng</param>
        /// <param name="iHeight">Chiều cao</param>
        /// <param name="rotation">Vòng xoay</param>
        /// <returns></returns>
        public byte[] SignByFile(int iPage, int llx, int lly, int iWidth, int iHeight, int rotation = 0)
        {
            try
            {
                if (_certificate == null)
                    throw new Exception("Không có chứng thư số ký");
                if (_fileData == null && string.IsNullOrEmpty(_inputFilePath))
                    throw new Exception("Tệp đầu vào không tồn tại");

                lock (_syncLock)
                {
                    X509CertificateParser x509CertificateParser = new X509CertificateParser();
                    Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[1]
                    {
                        x509CertificateParser.ReadCertificate(_certificate.RawData)
                    };
                    PdfReader pdfReader = string.IsNullOrEmpty(_inputFilePath) ? new PdfReader(_fileData) : new PdfReader(_inputFilePath);
                    if (pdfReader.IsEncrypted())
                    {
                        pdfReader.Close();
                        throw new Exception("Tài liệu pdf đã được mã hóa");
                    }
                    bool num = pdfReader.AcroFields.GetSignatureNames().Count > 0;
                    MemoryStream ms = new MemoryStream();
                    PdfSignatureAppearance signatureAppearance = PdfStamper.CreateSignature(pdfReader, ms, '\0', null, append: true).SignatureAppearance;
                    signatureAppearance.SetVisibleSignature(new Rectangle(llx, lly, llx + iWidth, lly + iHeight), iPage, signatureAppearance.GetNewSigName());
                    signatureAppearance.SignatureRenderingMode = _signatureAppearanceMode;

                    signatureAppearance.SignDate = DateTime.Now;
                    signatureAppearance.Reason = _signingReason;
                    signatureAppearance.Location = _signingLocation;
                    signatureAppearance.Acro6Layers = true;
                    if (!num)
                        signatureAppearance.CertificationLevel = 2;

                    new CertInfo(_certificate.RawData);
                    string text = GenerateSignatureText();
                    float size = 2f;
                    byte[] times = Resources.times;
                    BaseFont baseFont = BaseFont.CreateFont("times.ttf", "Identity-H", embedded: true, cached: true, ttfAfm: times, pfb: null);
                    baseFont.Subset = true;
                    Font font = new Font(baseFont, size, 0);
                    Rectangle rectangle = new Rectangle(llx, lly, llx + iWidth - 2, lly + iHeight - 2);
                    size = ColumnText.FitText(font, text, rectangle, 70f, 1);
                    signatureAppearance.Layer2Text = text;
                    signatureAppearance.Layer2Font = font;

                    if (_signatureAppearanceMode == PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION
                        || _signatureAppearanceMode == PdfSignatureAppearance.RenderingMode.GRAPHIC)
                    {
                        signatureAppearance.SignatureGraphic = Image.GetInstance(_signatureImage, _signatureImage.RawFormat);
                    }
                    DrawSignatureContent(signatureAppearance, rectangle, rotation);


                    ITSAClient tsaClient = null;
                    if (!string.IsNullOrEmpty(_tsaUrl))
                        tsaClient = new TSAClientBouncyCastle(_tsaUrl, null, null, 4096, Sha1Algorithm.ToUpper());
                    try
                    {
                        IExternalSignature externalSignature = new X509Signature(_certificate, "SHA-256");
                        MakeSignature.SignDetached(signatureAppearance, externalSignature, chain, null, null, tsaClient, 0, CryptoStandard.CMS);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Ký số thất bại: " + ex.Message, ex);
                    }
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ký số thất bại: " + ex.Message, ex);
            }
        }

        #region File ký có ảnh
        /// <summary>
        /// Create by DoanhLV - Thực hiện ký số với input/output là byte
        /// </summary>
        /// <param name="iPage">Trang</param>
        /// <param name="llx">Tọa độ X</param>
        /// <param name="lly">Tọa độ Y</param>
        /// <param name="iWidth">Chiều rộng</param>
        /// <param name="iHeight">Chiều cao</param>
        /// <param name="rotation">Vòng xoay</param>
        /// <returns></returns>
        public byte[] SignByFileImg(int iPage, int llx, int lly, int iWidth, int iHeight, int rotation = 0)
        {
            try
            {
                if (_certificate == null)
                {
                    throw new Exception("Không có chứng thư số ký");
                }
                if (_fileData == null && string.IsNullOrEmpty(_inputFilePath))
                {
                    throw new Exception("Tệp đầu vào không tồn tại");
                }
                lock (_syncLock)
                {
                    X509CertificateParser x509CertificateParser = new X509CertificateParser();
                    Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[1]
                    {
                        x509CertificateParser.ReadCertificate(_certificate.RawData)
                    };

                    //PdfReader pdfReader = new PdfReader(_fileData);
                    PdfReader pdfReader = string.IsNullOrEmpty(_inputFilePath) ? new PdfReader(_fileData) : new PdfReader(_inputFilePath);
                    // PdfReader pdfReader = new PdfReader(f1);
                    if (pdfReader.IsEncrypted())
                    {
                        pdfReader.Close();
                        throw new Exception("Tài liệu pdf đã được mã hóa");
                    }

                    bool num = pdfReader.AcroFields.GetSignatureNames().Count > 0;

                    MemoryStream ms = new MemoryStream();
                    PdfSignatureAppearance signatureAppearance = PdfStamper.CreateSignature(pdfReader, ms, '\0', null, append: true).SignatureAppearance;
                    signatureAppearance.SetVisibleSignature(new Rectangle(llx, lly, llx + iWidth, lly + iHeight), iPage, signatureAppearance.GetNewSigName());
                    signatureAppearance.SignatureRenderingMode = _signatureAppearanceMode;
                    signatureAppearance.SignDate = DateTime.Now;
                    signatureAppearance.Reason = _signingReason;
                    signatureAppearance.Location = _signingLocation;
                    signatureAppearance.Acro6Layers = true;
                    if (!num)
                    {
                        signatureAppearance.CertificationLevel = 2;
                    }
                    new CertInfo(_certificate.RawData);
                    string text = GenerateSignatureText();
                    float size = 5f;
                    byte[] times2 = Resources.times;
                    BaseFont baseFont = BaseFont.CreateFont("times.ttf", "Identity-H", embedded: true, cached: true, ttfAfm: times2, pfb: null);
                    baseFont.Subset = true;
                    Font font = new Font(baseFont, size, 0);
                    Rectangle rectangle = new Rectangle(llx, lly, llx + iWidth - 2, lly + iHeight - 2);
                    if (_signatureAppearanceMode != 0)
                    {
                        signatureAppearance.SignatureGraphic = Image.GetInstance(_signatureImage, _signatureImage.RawFormat);
                    }
                    size = ColumnText.FitText(font, text, rectangle, 70f, 1);
                    signatureAppearance.Layer2Text = text;
                    signatureAppearance.Layer2Font = font;
                    DrawSignatureContent(signatureAppearance, rectangle, rotation);
                    ITSAClient tsaClient = null;
                    if (!string.IsNullOrEmpty(_tsaUrl))
                        tsaClient = new TSAClientBouncyCastle(_tsaUrl, null, null, 4096, Sha1Algorithm.ToUpper());
                    try
                    {
                        IExternalSignature externalSignature = new X509Signature(_certificate, "SHA-256");
                        MakeSignature.SignDetached(signatureAppearance, externalSignature, chain, null, null, tsaClient, 0, CryptoStandard.CMS);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Ký số không thành công: " + ex.Message, ex);
                    }
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        public string Encrypt(string PathToPrivateKey)
        {
            X509Certificate2 myCertificate;
            try
            {
                myCertificate = new X509Certificate2(PathToPrivateKey);
            }
            catch (Exception e)
            {
                throw new CryptographicException("Unable to open key file.");
            }

            RSACryptoServiceProvider rsaObj;
            if (myCertificate.HasPrivateKey)
            {
                rsaObj = (RSACryptoServiceProvider)myCertificate.PrivateKey;
            }
            else
                throw new CryptographicException("Private key not contained within certificate.");

            if (rsaObj == null)
                return String.Empty;

            byte[] decryptedBytes;

            byte[] array = File.ReadAllBytes(_inputFilePath);
            try
            {
                decryptedBytes = rsaObj.Encrypt(array, false);
            }
            catch (Exception e)
            {
                throw new CryptographicException("Unable to encrypt data.");
            }

            //    Check to make sure we decrpyted the string 
            if (decryptedBytes.Length == 0)
                return String.Empty;
            else
                return System.Text.Encoding.UTF8.GetString(decryptedBytes);
        }

        public string DecryptEncryptedData(string Base64EncryptedData, string PathToPrivateKey)
        {
            X509Certificate2 myCertificate;
            try
            {
                myCertificate = new X509Certificate2(PathToPrivateKey);
            }
            catch (Exception e)
            {
                throw new CryptographicException("Unable to open key file.");
            }

            RSACryptoServiceProvider rsaObj;
            if (myCertificate.HasPrivateKey)
            {
                rsaObj = (RSACryptoServiceProvider)myCertificate.PrivateKey;
            }
            else
                throw new CryptographicException("Private key not contained within certificate.");

            if (rsaObj == null)
                return String.Empty;

            byte[] decryptedBytes;
            try
            {
                decryptedBytes = rsaObj.Decrypt(Convert.FromBase64String(Base64EncryptedData), false);
            }
            catch (Exception e)
            {
                throw new CryptographicException("Unable to decrypt data.");
            }

            //    Check to make sure we decrpyted the string 
            if (decryptedBytes.Length == 0)
                return String.Empty;
            else
                return System.Text.Encoding.UTF8.GetString(decryptedBytes);
        }
    }

}
