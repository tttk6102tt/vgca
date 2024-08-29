using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Plugin.UI.Configurations;
using Plugin.UI.PDF;
using Plugin.UI.Properties;
using Sign.itext.text.pdf;
using Sign.PDF;
using Sign.X509;

namespace Plugin.UI
{
    public partial class frmSign
    {
        private const string FormName = "Quản lý mẫu chữ ký ...";
        private BackgroundWorker bgWorker = new BackgroundWorker();
        private bool isCtsValid;
        private CertInfo selectedCertInfo;
        private string PdfPathFile;
        private int _signedPageNumber;
        private int Pdf_llx;
        private int Pdf_lly;
        private int Pdf_width;
        private int Pdf_height;
        private List<string> lstTempFile;
        private Configuration _configuration;
        private int Rotation_Type;
        private string _saveFileName;
        private IContainer components;
        private GroupBox groupBox1;
        private ComboBox cbbSignerCerts;
        private Button btnSign;
        private Button btnClose;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel barStatic;
        private PictureBox picCertState;
        private PictureBox picProgress;
        private Label lbProgressing;
        private PictureBox picSigImage;
        private GroupBox groupBox2;
        private GroupBox grbCertDetails;
        private Label lbCertStatus;
        private Label lbCertPeriod;
        private Label lbCertIssuer;
        private Label lbCertSubject;
        private LinkLabel lkViewCertDetails;
        private Button btnSelectSavingPath;
        private TextBox txtSavingPath;
        private GroupBox groupBox4;
        private Label lbSigDescription;
        private ToolTip tooltipSelectSignature;
        private Panel panel1;
        private ComboBox cbbSigAppearances;
        private Label lbErrorSelectionCert;
        private byte[] _signedFile;

        public byte[] SignedFile => this._signedFile;

        public frmSign(string srcFile, int page, int llx, int lly, int width, int height)
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.InitializeComponent();
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.WorkerSupportsCancellation = true;
            this.PdfPathFile = srcFile;
            this._signedPageNumber = page;
            this.Pdf_llx = llx;
            this.Pdf_lly = lly;
            this.Pdf_width = width;
            this.Pdf_height = height;
            this.Rotation_Type = 0;
            this.txtSavingPath.Text = Path.Combine(Path.GetDirectoryName(this.PdfPathFile), string.Format("{0}.signed.pdf", (object)Path.GetFileNameWithoutExtension(this.PdfPathFile)));
            this.tooltipSelectSignature.SetToolTip((Control)this.picCertState, "Chưa chọn chứng thư số ký!");
            this.tooltipSelectSignature.ToolTipIcon = ToolTipIcon.Warning;
            this.lstTempFile = new List<string>();
            this._configuration = new Configuration();
        }

        public frmSign(
          string srcFile,
          int page,
          int llx,
          int lly,
          int width,
          int height,
          int rotation)
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.InitializeComponent();
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.WorkerSupportsCancellation = true;
            this.PdfPathFile = srcFile;
            this._signedPageNumber = page;
            this.Pdf_llx = llx;
            this.Pdf_lly = lly;
            this.Pdf_width = width;
            this.Pdf_height = height;
            this.Rotation_Type = rotation;
            this.txtSavingPath.Text = Path.Combine(Path.GetDirectoryName(this.PdfPathFile), string.Format("{0}.signed.pdf", (object)Path.GetFileNameWithoutExtension(this.PdfPathFile)));
            this.tooltipSelectSignature.SetToolTip((Control)this.picCertState, "Chưa chọn chứng thư số ký!");
            this.tooltipSelectSignature.ToolTipIcon = ToolTipIcon.Warning;
            this.lstTempFile = new List<string>();
            this._configuration = new Configuration();
        }

        public string SavedFileName => this._saveFileName;

        public int SignedPageNumber => this._signedPageNumber;

        private void bgWorker_DoWork(
          object sender,
          DoWorkEventArgs e)
        {

            if (this.bgWorker.CancellationPending)
            {
                e.Cancel = true;
                this.bgWorker.ReportProgress(0, (object)"Quá trình kiểm tra CTS bị hủy bỏ");
            }
            else
            {
                try
                {
                    if (this.selectedCertInfo == null)
                    {
                        throw new Exception("Không tìm thấy thiết bị GCA-01.");
                    }
                    else
                    {
                        CertChecker certChecker = new CertChecker(this.selectedCertInfo.Certificate, DateTime.Now);
                        certChecker.OnlineCheckingAllowed = this._configuration.AllowedOnlineCheckingCert;
                        certChecker.CheckingViaOcsp = this._configuration.AllowedOCSPForCheckingSigningCert;
                        certChecker.AdditionalCRLs = this._configuration.AdditionalCrls;
                        if (this._configuration.UsedProxy)
                        {
                            if (this._configuration.AutoDetectProxy)
                            {
                                IWebProxy systemWebProxy = WebRequest.GetSystemWebProxy();
                                if (systemWebProxy.Credentials == null)
                                    systemWebProxy.Credentials = (ICredentials)CredentialCache.DefaultNetworkCredentials;
                                WebRequest.DefaultWebProxy = systemWebProxy;
                            }
                            else
                            {
                                certChecker.Proxy = this._configuration.Proxy;
                            }
                        }
                        this.bgWorker.ReportProgress(100);
                    }
                }
                catch (Exception ex)
                {
                    this.bgWorker.ReportProgress(100, (object)ex);
                }
            }

        }

        private void bgWorker_ProgressChanged(
          object sender,
          ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != 100)
                return;
            if (e.UserState == null)
            {
                this.lbCertStatus.Text = "Tình trạng: Chứng thư số hợp lệ";
                this.lbCertStatus.ForeColor = Color.Black;
                this.tooltipSelectSignature.SetToolTip((Control)this.picCertState, "Chứng thư số hợp lệ!");
                this.tooltipSelectSignature.ToolTipIcon = ToolTipIcon.Info;
                this.btnSign.Enabled = true;
                this.barStatic.Text = "OK";
                this.picCertState.Image = (Image)Resources.ok16;
                this.btnSign.Enabled = true;
                this.isCtsValid = true;
            }
            if (e.UserState is string)
            {
                this.tooltipSelectSignature.SetToolTip((Control)this.picCertState, (string)e.UserState);
                this.tooltipSelectSignature.ToolTipIcon = ToolTipIcon.Info;
                this.btnSign.Enabled = true;
                this.barStatic.Text = (string)e.UserState;
                this.picCertState.Image = (Image)Resources.ok16;
                this.btnSign.Enabled = true;
                this.isCtsValid = true;
                this.lbCertStatus.Text = string.Format("Tình trạng: {0}", (object)(string)e.UserState);
                this.lbCertStatus.ForeColor = Color.Black;
            }
            else
            {
                if (!(e.UserState is Exception))
                {

                }
                else
                {
                    Exception userState = e.UserState as Exception;
                    this.tooltipSelectSignature.SetToolTip((Control)this.picCertState, userState.Message);
                    this.tooltipSelectSignature.ToolTipIcon = ToolTipIcon.Warning;
                    this.btnSign.Enabled = false;
                    this.barStatic.Text = "Trạng thái CTS: " + userState.Message;
                    this.picCertState.Image = (Image)Resources.alert2;
                    this.isCtsValid = false;
                    this.lbCertStatus.Text = string.Format("Tình trạng: {0}", (object)userState.Message);
                    this.lbCertStatus.ForeColor = Color.Red;
                }
            }
        }

        private void bgWorker_RunWorkerCompleted(
          object sender,
          RunWorkerCompletedEventArgs e)
        {
            this.cbbSignerCerts.Enabled = true;
            this.bgWorker.DoWork -= new DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.ProgressChanged -= new ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
            this.bgWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
        }

        private void startBackgroundWorker()
        {
            if (this.bgWorker.IsBusy)
            {

            }
            else
            {
                Application.DoEvents();
                this.bgWorker.DoWork += new DoWorkEventHandler(this.bgWorker_DoWork);
                this.bgWorker.ProgressChanged += new ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
                this.bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
                this.bgWorker.RunWorkerAsync();
                this.picCertState.Image = (Image)Resources.loading2;
                this.barStatic.Text = "Đang kiểm tra chứng thư số ...";
                this.btnSign.Enabled = false;
                this.cbbSignerCerts.Enabled = false;
            }
        }

        private void cbbSignerCerts_Selected()
        {
            this.cbbSignerCerts.Items.Clear();
            X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            x509Store.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 certificate in x509Store.Certificates)
            {
                CertInfo certInfo = new CertInfo(certificate);

                this.cbbSignerCerts.Items.Add((object)certInfo);
                continue;
                if (false)
                {
                    // check expired
                    if ((certInfo.KeyUsages & X509KeyUsageFlags.NonRepudiation) != X509KeyUsageFlags.None)
                    {

                    }
                    if (certificate.NotAfter.CompareTo(DateTime.Now) >= 0)
                    {
                        if (certificate.NotBefore.CompareTo(DateTime.Now) <= 0)
                        {
                            if ((certInfo.KeyUsages & X509KeyUsageFlags.NonRepudiation) != X509KeyUsageFlags.None)
                            {
                                this.cbbSignerCerts.Items.Add((object)certInfo);
                                continue;
                            }
                            continue;
                        }
                        else
                            continue;
                    }
                }
            }
            x509Store.Close();
            ComboBox cbbSignerCerts = this.cbbSignerCerts;
            int num;
            if (this.cbbSignerCerts.Items.Count != 0)
            {
                num = 0;
            }
            else
                num = -1;
            cbbSignerCerts.SelectedIndex = num;
        }
        private void cbbSignerCerts_LoadData()
        {
            this.cbbSigAppearances.Items.Clear();
            IEnumerator<SignerProfile> enumerator = this._configuration.PdfSignerProfiles.GetEnumerator();

            try
            {
                // add mẫu có sẵn
                while (enumerator.MoveNext())
                {
                    this.cbbSigAppearances.Items.Add((object)enumerator.Current);
                }
                    
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }
            this.cbbSigAppearances.Items.Add((object)"Quản lý mẫu chữ ký ...");
            if (this.cbbSigAppearances.Items.Count > 1)
            {
                if (this._configuration.PdfSignerProfiles.DefaultSigner > -1 && this._configuration.PdfSignerProfiles.DefaultSigner < this.cbbSigAppearances.Items.Count - 1)
                {
                    this.cbbSigAppearances.SelectedIndex = this._configuration.PdfSignerProfiles.DefaultSigner;
                }
                else
                {
                    this.cbbSigAppearances.SelectedIndex = 0;
                }
            }
            else
                this.cbbSigAppearances.SelectedIndex = -1;
        }

        private void setpicSigImage(
          CertInfo parmCertInfo, SignerProfile parmSignerProfile)
        {

            SignerProfile signerProfile = parmSignerProfile;
            if (signerProfile == null)
            {
                signerProfile = new SignerProfile("Default", (string)null);
            }
            if (signerProfile.AppearanceMode == 0)
            {
                this.lbSigDescription.Visible = true;
                this.lbSigDescription.Dock = DockStyle.Fill;
                this.picSigImage.Visible = false;
            }
            else if (signerProfile.AppearanceMode == 1)
            {
                this.lbSigDescription.Visible = false;
                this.picSigImage.Visible = true;
                this.picSigImage.Dock = DockStyle.Fill;
            }
            else if (signerProfile.AppearanceMode == 2)
            {
                this.lbSigDescription.Visible = true;
                this.lbSigDescription.Dock = DockStyle.None;
                this.lbSigDescription.Location = new Point(189, 0);
                this.lbSigDescription.Size = new Size(183, 98);
                this.picSigImage.Visible = true;
                this.picSigImage.Dock = DockStyle.None;
                this.picSigImage.Location = new Point(0, 0);
                this.picSigImage.Size = new Size(183, 99);
            }
            if (signerProfile.AppearanceMode != 1)
            {
                StringBuilder stringBuilder = new StringBuilder();
                string str1 = "";
                string str2 = "";
                string str3 = "";
                string str4 = "";
                if (parmCertInfo == null)
                {
                    if (!signerProfile.IsOrgProfile)
                    {
                        str2 = "Tên chứng thư số ký";
                    }
                    else
                    {
                        str2 = "Tên tổ chức";
                    }

                    if (signerProfile.ShowCQ3)
                    {
                        str4 = "CQ Cấp 3";
                    }

                    if (signerProfile.ShowCQ2)
                    {
                        str4 += string.IsNullOrEmpty(str4) ? "CQ Cấp 2" : ", CQ Cấp 2";
                    }

                    if (signerProfile.ShowCQ1)
                    {
                        str4 += string.IsNullOrEmpty(str4) ? "CQ Cấp 1" : ", CQ Cấp 1";
                    }
                }
                else
                {
                    str2 = parmCertInfo.CommonName;
                    str3 = parmCertInfo.Email;
                    str4 = "";
                    if (signerProfile.ShowCQ3)
                    {
                        if (parmCertInfo.OUs.Count > 0)
                        {
                            str4 = parmCertInfo.OUs[0];
                        }
                    }
                    if (signerProfile.ShowCQ2)
                    {
                        if (parmCertInfo.OUs.Count > 1)
                        {
                            if (!string.IsNullOrEmpty(str4))
                                str4 += ", ";
                            str4 += str1 = parmCertInfo.OUs[1];
                        }
                    }
                    if (signerProfile.ShowCQ1)
                    {
                        if (!string.IsNullOrEmpty(str4))
                        {
                            str4 += ", ";
                        }
                        str4 += parmCertInfo.O;
                    }
                }
                if (signerProfile.IsOrgProfile)
                {
                    if (signerProfile.ShowLabel)
                    {
                        stringBuilder.Append("Cơ quan: " + str2);
                        if (!string.IsNullOrEmpty(str4))
                        {
                            stringBuilder.Append(", ");
                            stringBuilder.Append(str4);
                        }
                        if (signerProfile.ShowEmail)
                        {
                            if (!string.IsNullOrEmpty(str3))
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append("Email: ");
                                stringBuilder.Append(str3);
                            }
                        }
                        if (signerProfile.ShowDate)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append("Thời gian ký: ");
                            stringBuilder.Append(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss zzz", (IFormatProvider)CultureInfo.CreateSpecificCulture("en-US")));
                        }
                    }
                    else
                    {
                        stringBuilder.Append(str2);
                        if (!string.IsNullOrEmpty(str4))
                        {
                            stringBuilder.Append(", ");
                            stringBuilder.Append(str4);
                        }
                        if (signerProfile.ShowEmail)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(str3);
                        }
                        if (signerProfile.ShowDate)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss zzz", (IFormatProvider)CultureInfo.CreateSpecificCulture("en-US")));
                        }

                    }
                }
                else if (signerProfile.ShowLabel)
                {
                    stringBuilder.Append("Người ký: " + str2);
                    if (signerProfile.ShowEmail && !string.IsNullOrEmpty(str3))
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append("Email: ");
                        stringBuilder.Append(str3);
                    }
                    if (!string.IsNullOrEmpty(str4))
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append("Cơ quan: ");
                        stringBuilder.Append(str4);
                    }
                    if (signerProfile.ShowDate)
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append("Thời gian ký: ");
                        stringBuilder.Append(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss zzz", (IFormatProvider)CultureInfo.CreateSpecificCulture("en-US")));
                    }
                }
                else
                {
                    stringBuilder.Append(str2);
                    if (signerProfile.ShowEmail)
                    {
                        if (!string.IsNullOrEmpty(str3))
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(str3);
                        }
                    }
                    if (!string.IsNullOrEmpty(str4))
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append(str4);
                    }
                    if (signerProfile.ShowDate)
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss zzz", (IFormatProvider)CultureInfo.CreateSpecificCulture("en-US")));
                    }
                }
                float emSize = 7f;
                int width = this.lbSigDescription.Width / 2 > this.lbSigDescription.Height ?
                            this.lbSigDescription.Height :
                            this.lbSigDescription.Width / 2;

                while (TextRenderer.MeasureText(stringBuilder.ToString(),
                                                new System.Drawing.Font("Tahoma", emSize, FontStyle.Bold, GraphicsUnit.Pixel),
                                                new Size(width, width - 10),
                                                TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak).Height
                                                <= this.lbSigDescription.Height)
                {
                    if (emSize <= 30.0)
                    {
                        emSize++;
                    }
                    else
                    {
                        break;
                    }
                }
                this.lbSigDescription.Font = new System.Drawing.Font("Tahoma", emSize + 1f, FontStyle.Regular, GraphicsUnit.Pixel);
                this.lbSigDescription.Text = stringBuilder.ToString();
            }
            if (signerProfile.AppearanceMode == 0)
                return;
            if (!string.IsNullOrEmpty(signerProfile.ImageBase64))
            {
                try
                {
                    if (this.picSigImage.Image != null)
                    {
                        this.picSigImage.Image.Dispose();
                    }
                    if (signerProfile.Image != null)
                    {
                        this.picSigImage.Image = (Image)signerProfile.Image;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(signerProfile.ImageBase64))
                        {
                            this.picSigImage.Image = SignerProfile.Base64ToBitmap(signerProfile.ImageBase64);
                        }
                    }
                    
                    return;
                }
                catch
                {
                    return;
                }
            }   
        }

        private void cbbSignerCerts_SelectedIndexChanged(
          object sender,
          EventArgs e)
        {
            if (this.cbbSignerCerts.SelectedItem == null)
            {
                
            }
            else
            {
                this.lbCertSubject.Visible = true;
                this.lbCertIssuer.Visible = true;
                this.lbCertPeriod.Visible = true;
                this.lkViewCertDetails.Visible = true;
                this.lbCertStatus.Visible = true;
                this.lbErrorSelectionCert.Visible = false;
                this.selectedCertInfo = this.cbbSignerCerts.SelectedItem as CertInfo;
                this.lbCertIssuer.Text = string.Format("Cơ quan cấp phát: {0}", (object)this.selectedCertInfo.IssuerName);
                this.lbCertSubject.Text = string.Format("Chủ sở hữu: {0}", (object)this.selectedCertInfo.ToString());
                this.lbCertPeriod.Text = string.Format("Thời gian hiệu lực: Từ {0}", (object)this.selectedCertInfo.Period);
                this.picCertState.Image = (Image)Resources.loading2;
                this.startBackgroundWorker();
                SignerProfile signnerDefault = new SignerProfile("Default", (string)null);
                try
                {
                    signnerDefault = this.cbbSigAppearances.SelectedItem as SignerProfile;
                }
                catch
                {
                }
                this.setpicSigImage(this.selectedCertInfo, signnerDefault);
                this.grbCertDetails.Update();
            }
        }

        private void cbbSignerCerts_TextChanged(
          object sender,
          EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.cbbSignerCerts.Text))
                return;
            this.selectedCertInfo = (CertInfo)null;
            this.lbCertSubject.Visible = false;
            this.lbCertIssuer.Visible = false;
            this.lbCertPeriod.Visible = false;
            this.lkViewCertDetails.Visible = false;
            this.lbCertStatus.Visible = false;
            this.lbErrorSelectionCert.Visible = true;
            this.tooltipSelectSignature.SetToolTip((Control)this.picCertState, "Chưa chọn chứng thư số ký!");
            this.tooltipSelectSignature.ToolTipIcon = ToolTipIcon.Warning;
            this.grbCertDetails.Update();
        }

        private void cbbSigAppearances_SelectedIndexChanged(
          object sender,
          EventArgs e)
        {
            // mở config
            if (this.cbbSigAppearances.SelectedItem == null)
                return;

            if (this.cbbSigAppearances.SelectedItem is SignerProfile selectedProfile)
            {
                this.setpicSigImage(this.selectedCertInfo, selectedProfile);
                return;
            }

            if (this.cbbSigAppearances.SelectedIndex == this.cbbSigAppearances.Items.Count - 1)
            {
                int num = (int)new frmConfig(true).ShowDialog();
                this._configuration = new Configuration();
                this.cbbSignerCerts_LoadData();
            }
        }

        private void frmSign_Shown(
          object sender,
          EventArgs e)
        {
            this.cbbSignerCerts_LoadData();
            this.cbbSignerCerts_Selected();
        }

        private void btnSign_Click(
          object sender,
          EventArgs e)
        {
            this.picProgress.Image = (Image)Resources.loading2;
            this.picProgress.Visible = true;
            this.lbProgressing.Visible = true;
            if (this.bgWorker != null)
            {
                if (this.bgWorker.IsBusy)
                {
                    this.picProgress.Image = (Image)Resources.alert2;
                    this.lbProgressing.Text = "Đang kiểm tra chứng thư số!";
                }
            }
            SignerProfile signerProfile = (SignerProfile)null;
            try
            {
                signerProfile = this.cbbSigAppearances.SelectedItem as SignerProfile;
            }
            catch
            {
            }
            if (signerProfile == null)
                signerProfile = new SignerProfile("Default", (string)null);
            Bitmap bitmap = (Bitmap)null;
            try
            {
                this.Cursor = Cursors.WaitCursor;
                this.barStatic.Text = "Đang ký số tài liệu...";
                this.lbProgressing.Text = "Ký số tài liệu...";
                if (!this.isCtsValid)
                {
                    throw new Exception("Chứng thư số người ký không hợp lệ!");
                }
                else
                {
                    string tempFileName = Path.GetTempFileName();
                    this.lstTempFile.Add(tempFileName);
                    this.picSigImage.Image.Save(tempFileName);
                    bitmap = (Bitmap)Image.FromFile(tempFileName);
                    this._saveFileName = this.txtSavingPath.Text;
                    if (Decimal.Round((Decimal)new FileInfo(tempFileName).Length / 1024M / 1024M, 2, MidpointRounding.AwayFromZero).CompareTo(3.5M) > 0)
                    {
                        new Exception("Dung lượng hình ảnh chữ ký quá lớn. Dung lượng tối đa cho phép là 3.5Mb.");
                    }
                    else
                    {
                        PdfSignatureAppearance.RenderingMode renderingMode;
                        if (signerProfile.AppearanceMode == 0)
                        {
                            renderingMode = PdfSignatureAppearance.RenderingMode.DESCRIPTION;
                        }
                        else if (signerProfile.AppearanceMode == 1)
                        {
                            renderingMode = PdfSignatureAppearance.RenderingMode.GRAPHIC;
                        }
                        else
                            renderingMode = PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION;


                        PdfSigner pdfSigner = new PdfSigner(this.PdfPathFile, this._saveFileName, this.selectedCertInfo.Certificate);
                        pdfSigner.SigningReason = string.Format("{0} đã ký lên văn bản này!", (object)this.selectedCertInfo.ToString());
                        pdfSigner.SigningLocation = "Việt Nam";
                        pdfSigner.SignatureImage = (Image)bitmap;
                        pdfSigner.SignatureAppearance = renderingMode;
                        pdfSigner.ShowLabel = signerProfile.ShowLabel;
                        pdfSigner.ShowEmail = signerProfile.ShowEmail;
                        pdfSigner.ShowCQ1 = signerProfile.ShowCQ1;
                        pdfSigner.ShowCQ2 = signerProfile.ShowCQ2;
                        pdfSigner.ShowCQ3 = signerProfile.ShowCQ3;
                        pdfSigner.ShowDate = signerProfile.ShowDate;
                        pdfSigner.IsOrgProfile = signerProfile.IsOrgProfile;

                        this.Pdf_width = bitmap.Width;
                        this.Pdf_height = bitmap.Height;

                        pdfSigner.Sign(this._signedPageNumber, this.Pdf_llx, this.Pdf_lly, this.Pdf_width, this.Pdf_height, this.Rotation_Type);

                        //pdfSigner.SignByFileImg(this._signedPageNumber, this.Pdf_llx, this.Pdf_lly, this.Pdf_width, this.Pdf_height, this.Rotation_Type);

                        this.lbProgressing.Text = "Ký số thành công!";
                        this.picProgress.Image = (Image)Resources.ok16;
                        this.DialogResult = DialogResult.OK;
                    }
                }
            }
            catch (Exception ex)
            {
                this.picProgress.Image = (Image)Resources.alert2;
                this.lbProgressing.Text = string.Format("Lỗi : {0}", (object)ex.Message);
            }
            finally
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
                this.Cursor = Cursors.Default;
                this.barStatic.Text = "Sẵn sàng";
            }
        }

        private void PdfProcessing(
          PdfProcessingEventArgs e)
        {
            Application.DoEvents();
            this.lbProgressing.Text = e.Message;
        }

        private void btnClose_Click(
          object sender,
          EventArgs e)
        {
            this.Close();
        }

        private void btnSelectSavingPath_Click(
          object sender,
          EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Files(*.pdf)|*.pdf";
            try
            {
                if (this.txtSavingPath.Text != "")
                {
                    saveFileDialog.InitialDirectory = Path.GetDirectoryName(this.txtSavingPath.Text);
                    saveFileDialog.FileName = Path.GetFileNameWithoutExtension(this.txtSavingPath.Text);
                }
            }
            catch (Exception ex)
            {
            }
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;
            this.txtSavingPath.Text = saveFileDialog.FileName;
        }

        private void lkViewCertDetails_LinkClicked(
          object sender,
          LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (this.selectedCertInfo == null)
                    return;
                X509Certificate2UI.DisplayCertificate(this.selectedCertInfo.Certificate, this.Handle);
            }
            catch
            {
            }
        }

        private void frmSign_FormClosing(
          object sender,
          FormClosingEventArgs e)
        {
            try
            {
                this.picSigImage.Image.Dispose();
                using (List<string>.Enumerator enumerator = this.lstTempFile.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        System.IO.File.Delete(enumerator.Current);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void cbbSignerCerts_DropDown(
          object sender,
          EventArgs e)
        {
            this.cbbSignerCerts_Selected();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = (IContainer)new Container();
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(frmSign));
            this.groupBox1 = new GroupBox();
            this.grbCertDetails = new GroupBox();
            this.lbErrorSelectionCert = new Label();
            this.lbCertStatus = new Label();
            this.lbCertPeriod = new Label();
            this.lbCertIssuer = new Label();
            this.lbCertSubject = new Label();
            this.lkViewCertDetails = new LinkLabel();
            this.picCertState = new PictureBox();
            this.cbbSignerCerts = new ComboBox();
            this.groupBox4 = new GroupBox();
            this.cbbSigAppearances = new ComboBox();
            this.panel1 = new Panel();
            this.picSigImage = new PictureBox();
            this.lbSigDescription = new Label();
            this.btnSign = new Button();
            this.btnClose = new Button();
            this.statusStrip1 = new StatusStrip();
            this.barStatic = new ToolStripStatusLabel();
            this.picProgress = new PictureBox();
            this.lbProgressing = new Label();
            this.groupBox2 = new GroupBox();
            this.btnSelectSavingPath = new Button();
            this.txtSavingPath = new TextBox();
            this.tooltipSelectSignature = new ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.grbCertDetails.SuspendLayout();
            ((ISupportInitialize)this.picCertState).BeginInit();
            this.groupBox4.SuspendLayout();
            this.panel1.SuspendLayout();
            ((ISupportInitialize)this.picSigImage).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((ISupportInitialize)this.picProgress).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            this.groupBox1.Controls.Add((Control)this.grbCertDetails);
            this.groupBox1.Controls.Add((Control)this.picCertState);
            this.groupBox1.Controls.Add((Control)this.cbbSignerCerts);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.groupBox1.Location = new Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(413, 173);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Thông tin người ký:";
            this.grbCertDetails.Controls.Add((Control)this.lbErrorSelectionCert);
            this.grbCertDetails.Controls.Add((Control)this.lbCertStatus);
            this.grbCertDetails.Controls.Add((Control)this.lbCertPeriod);
            this.grbCertDetails.Controls.Add((Control)this.lbCertIssuer);
            this.grbCertDetails.Controls.Add((Control)this.lbCertSubject);
            this.grbCertDetails.Controls.Add((Control)this.lkViewCertDetails);
            this.grbCertDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.grbCertDetails.Location = new Point(17, 60);
            this.grbCertDetails.Name = "grbCertDetails";
            this.grbCertDetails.Size = new Size(377, 105);
            this.grbCertDetails.TabIndex = 3;
            this.grbCertDetails.TabStop = false;
            this.grbCertDetails.Text = "Thông tin chứng thư số";
            this.lbErrorSelectionCert.Dock = DockStyle.Fill;
            this.lbErrorSelectionCert.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.lbErrorSelectionCert.Location = new Point(3, 16);
            this.lbErrorSelectionCert.Name = "lbErrorSelectionCert";
            this.lbErrorSelectionCert.Size = new Size(371, 86);
            this.lbErrorSelectionCert.TabIndex = 2;
            this.lbErrorSelectionCert.Text = "* Chưa chọn chứng thư số ký!";
            this.lbErrorSelectionCert.TextAlign = ContentAlignment.MiddleCenter;
            this.lbCertStatus.AutoEllipsis = true;
            this.lbCertStatus.Location = new Point(12, 74);
            this.lbCertStatus.Name = "lbCertStatus";
            this.lbCertStatus.Size = new Size(356, 13);
            this.lbCertStatus.TabIndex = 1;
            this.lbCertStatus.Text = "Tình trạng: ...";
            this.lbCertPeriod.AutoSize = true;
            this.lbCertPeriod.Location = new Point(12, 55);
            this.lbCertPeriod.Name = "lbCertPeriod";
            this.lbCertPeriod.Size = new Size(156, 13);
            this.lbCertPeriod.TabIndex = 1;
            this.lbCertPeriod.Text = "Thời gian hiệu lực: Từ ... đến ...";
            this.lbCertIssuer.AutoEllipsis = true;
            this.lbCertIssuer.Location = new Point(13, 35);
            this.lbCertIssuer.Name = "lbCertIssuer";
            this.lbCertIssuer.Size = new Size(355, 13);
            this.lbCertIssuer.TabIndex = 1;
            this.lbCertIssuer.Text = "Cơ quan cấp phát: Tên cơ quan cấp phát";
            this.lbCertSubject.AutoEllipsis = true;
            this.lbCertSubject.Location = new Point(13, 16);
            this.lbCertSubject.Name = "lbCertSubject";
            this.lbCertSubject.Size = new Size(304, 15);
            this.lbCertSubject.TabIndex = 1;
            this.lbCertSubject.Text = "Chủ sở hữu: Tên chủ sở hữu chứng thư số <email>";
            this.lkViewCertDetails.ActiveLinkColor = Color.FromArgb(64, 0, 0);
            this.lkViewCertDetails.AutoSize = true;
            this.lkViewCertDetails.Cursor = Cursors.Hand;
            this.lkViewCertDetails.LinkColor = Color.FromArgb(64, 0, 0);
            this.lkViewCertDetails.Location = new Point(323, 16);
            this.lkViewCertDetails.Name = "lkViewCertDetails";
            this.lkViewCertDetails.Size = new Size(48, 13);
            this.lkViewCertDetails.TabIndex = 0;
            this.lkViewCertDetails.TabStop = true;
            this.lkViewCertDetails.Text = "Chi tiết...";
            this.lkViewCertDetails.LinkClicked += new LinkLabelLinkClickedEventHandler(this.lkViewCertDetails_LinkClicked);
            this.picCertState.Image = (Image)Resources.alert2;
            this.picCertState.Location = new Point(364, 29);
            this.picCertState.Name = "picCertState";
            this.picCertState.Size = new Size(24, 24);
            this.picCertState.SizeMode = PictureBoxSizeMode.Zoom;
            this.picCertState.TabIndex = 2;
            this.picCertState.TabStop = false;
            this.tooltipSelectSignature.SetToolTip((Control)this.picCertState, "Lựa chọn chứng thư số ký");
            this.cbbSignerCerts.BackColor = SystemColors.Control;
            this.cbbSignerCerts.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbbSignerCerts.FormattingEnabled = true;
            this.cbbSignerCerts.Location = new Point(17, 29);
            this.cbbSignerCerts.Name = "cbbSignerCerts";
            this.cbbSignerCerts.Size = new Size(338, 24);
            this.cbbSignerCerts.TabIndex = 0;
            this.cbbSignerCerts.DropDown += new EventHandler(this.cbbSignerCerts_DropDown);
            this.cbbSignerCerts.SelectedIndexChanged += new EventHandler(this.cbbSignerCerts_SelectedIndexChanged);
            this.cbbSignerCerts.TextChanged += new EventHandler(this.cbbSignerCerts_TextChanged);
            this.groupBox4.Controls.Add((Control)this.cbbSigAppearances);
            this.groupBox4.Controls.Add((Control)this.panel1);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.groupBox4.Location = new Point(12, 191);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new Size(413, 151);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Hiển thị chữ ký";
            this.cbbSigAppearances.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbbSigAppearances.FormattingEnabled = true;
            this.cbbSigAppearances.Location = new Point(190, 15);
            this.cbbSigAppearances.Name = "cbbSigAppearances";
            this.cbbSigAppearances.Size = new Size(204, 21);
            this.cbbSigAppearances.TabIndex = 5;
            this.cbbSigAppearances.SelectedIndexChanged += new EventHandler(this.cbbSigAppearances_SelectedIndexChanged);
            this.panel1.BackColor = SystemColors.ControlLightLight;
            this.panel1.BorderStyle = BorderStyle.FixedSingle;
            this.panel1.Controls.Add((Control)this.picSigImage);
            this.panel1.Controls.Add((Control)this.lbSigDescription);
            this.panel1.Location = new Point(17, 43);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(377, 102);
            this.panel1.TabIndex = 4;
            this.picSigImage.Cursor = Cursors.Hand;
            this.picSigImage.Image = (Image)Resources.HiPT;
            this.picSigImage.Location = new Point(0, 0);
            this.picSigImage.Name = "picSigImage";
            this.picSigImage.Size = new Size(179, 100);
            this.picSigImage.SizeMode = PictureBoxSizeMode.Zoom;
            this.picSigImage.TabIndex = 2;
            this.picSigImage.TabStop = false;
            this.lbSigDescription.Font = new System.Drawing.Font("Tahoma", 9f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.lbSigDescription.Location = new Point(181, 0);
            this.lbSigDescription.Name = "lbSigDescription";
            this.lbSigDescription.Size = new Size(191, 100);
            this.lbSigDescription.TabIndex = 3;
            this.lbSigDescription.Text = "Người ký: Tên chứng thư số ký\r\nEmail: Địa chỉ thư điện tử\r\nCơ quan: CQ Cấp 3, CQ Cấp 2, CQ Cấp 1\r\nThời gian ký: 29.05.2013 09:37:36 +7:00";
            this.lbSigDescription.TextAlign = ContentAlignment.MiddleLeft;
            this.btnSign.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnSign.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.btnSign.Image = (Image)Resources.xSign32;
            this.btnSign.ImageAlign = ContentAlignment.MiddleLeft;
            this.btnSign.Location = new Point(239, 412);
            this.btnSign.Name = "btnSign";
            this.btnSign.Size = new Size(90, 33);
            this.btnSign.TabIndex = 1;
            this.btnSign.Text = "Ký số  ";
            this.btnSign.TextAlign = ContentAlignment.MiddleRight;
            this.btnSign.UseVisualStyleBackColor = true;
            this.btnSign.Click += new EventHandler(this.btnSign_Click);
            this.btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnClose.DialogResult = DialogResult.Cancel;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.btnClose.Image = (Image)Resources.cancel;
            this.btnClose.ImageAlign = ContentAlignment.MiddleLeft;
            this.btnClose.Location = new Point(346, 412);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(79, 33);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Đóng";
            this.btnClose.TextAlign = ContentAlignment.MiddleRight;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new EventHandler(this.btnClose_Click);
            this.statusStrip1.Items.AddRange(new ToolStripItem[1]
            {
        (ToolStripItem) this.barStatic
            });
            this.statusStrip1.Location = new Point(0, 449);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new Size(428, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            this.barStatic.Name = "barStatic";
            this.barStatic.Size = new Size(54, 17);
            this.barStatic.Text = "Sẵn sàng";
            this.barStatic.TextAlign = ContentAlignment.MiddleLeft;
            this.picProgress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.picProgress.Image = (Image)Resources.loading2;
            this.picProgress.Location = new Point(12, 412);
            this.picProgress.Name = "picProgress";
            this.picProgress.Size = new Size(24, 24);
            this.picProgress.SizeMode = PictureBoxSizeMode.Zoom;
            this.picProgress.TabIndex = 3;
            this.picProgress.TabStop = false;
            this.picProgress.Visible = false;
            this.lbProgressing.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.lbProgressing.Font = new System.Drawing.Font("Tahoma", 9f, FontStyle.Italic, GraphicsUnit.Point, (byte)0);
            this.lbProgressing.ImageAlign = ContentAlignment.TopLeft;
            this.lbProgressing.Location = new Point(42, 414);
            this.lbProgressing.Name = "lbProgressing";
            this.lbProgressing.Size = new Size(185, 35);
            this.lbProgressing.TabIndex = 1;
            this.lbProgressing.Text = "Ký số tài liệu...";
            this.lbProgressing.Visible = false;
            this.groupBox2.Controls.Add((Control)this.btnSelectSavingPath);
            this.groupBox2.Controls.Add((Control)this.txtSavingPath);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.groupBox2.Location = new Point(12, 348);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new Size(413, 52);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Đường dẫn tệp đã được ký số";
            this.btnSelectSavingPath.Location = new Point(334, 20);
            this.btnSelectSavingPath.Name = "btnSelectSavingPath";
            this.btnSelectSavingPath.Size = new Size(60, 23);
            this.btnSelectSavingPath.TabIndex = 1;
            this.btnSelectSavingPath.Text = "...";
            this.btnSelectSavingPath.UseVisualStyleBackColor = true;
            this.btnSelectSavingPath.Click += new EventHandler(this.btnSelectSavingPath_Click);
            this.txtSavingPath.Location = new Point(17, 21);
            this.txtSavingPath.Name = "txtSavingPath";
            this.txtSavingPath.Size = new Size(311, 22);
            this.txtSavingPath.TabIndex = 0;
            this.tooltipSelectSignature.ToolTipIcon = ToolTipIcon.Info;
            this.tooltipSelectSignature.ToolTipTitle = "Tình trạng chứng thư số";
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = (IButtonControl)this.btnClose;
            this.ClientSize = new Size(428, 471);
            this.Controls.Add((Control)this.groupBox4);
            this.Controls.Add((Control)this.groupBox2);
            this.Controls.Add((Control)this.picProgress);
            this.Controls.Add((Control)this.lbProgressing);
            this.Controls.Add((Control)this.statusStrip1);
            this.Controls.Add((Control)this.btnClose);
            this.Controls.Add((Control)this.btnSign);
            this.Controls.Add((Control)this.groupBox1);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Icon = (Icon)componentResourceManager.GetObject("Icon");
            this.MaximizeBox = false;
            this.MaximumSize = new Size(444, 510);
            this.MinimizeBox = false;
            this.MinimumSize = new Size(444, 503);
            this.Name = "frmSign";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Ký số tài liệu";
            this.FormClosing += new FormClosingEventHandler(this.frmSign_FormClosing);
            this.Shown += new EventHandler(this.frmSign_Shown);
            this.groupBox1.ResumeLayout(false);
            this.grbCertDetails.ResumeLayout(false);
            this.grbCertDetails.PerformLayout();
            ((ISupportInitialize)this.picCertState).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((ISupportInitialize)this.picSigImage).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((ISupportInitialize)this.picProgress).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}