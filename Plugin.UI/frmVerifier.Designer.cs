using Plugin.UI.Configurations;
using Plugin.UI.PDF;
using Plugin.UI.Properties;
using Sign.PDF;
using Sign.X509;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace Plugin.UI
{
    partial class frmVerifier : Form
    {
        private string FileName;
        private string SignatureName;
        private Configuration _configuration;
        private IContainer component;
        private GroupBox groupBox1;
        private Label lbValidityStatus;
        private PictureBox picAllStatus;
        private Label lbSigningTime;
        private Label lbSigner;
        private GroupBox groupBox2;
        private Label lbCheckingTime;
        private Label lbKTCTSTSA;
        private Label lbKTCTSKy;
        private Label lbToanVen;
        private Button btnClose;
        private PictureBox picTSACertStatus;
        private PictureBox picSignerCertStatus;
        private PictureBox picToanvenStatus;
        private GroupBox groupBox3;
        private LinkLabel lklbTsaCert;
        private Label label1;
        private Label lbTimeStampDate;
        private LinkLabel lklbSignerCert;
        private Panel panel1;

        public frmVerifier(string srcFile, string signatureName)
        {
            this.InitializeComponent();
            this.FileName = srcFile;
            this.SignatureName = signatureName;
            this._configuration = new Configuration();
            this.Text = "Xác thực chữ ký (" + signatureName + ")";
        }

        private void initFormVerify()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                PdfVerifier verifierInfo = new PdfVerifier(this.FileName, this.SignatureName);
                verifierInfo.AdditionalCRLs = this._configuration.AdditionalCrls;
                verifierInfo.AllowedOnlineChecking = this._configuration.AllowedOnlineCheckingCert;
                if (this._configuration.UsedProxy)
                {
                    if (this._configuration.AutoDetectProxy)
                    {
                        IWebProxy systemWebProxy = WebRequest.GetSystemWebProxy();
                        if (systemWebProxy.Credentials == null)
                        {
                            systemWebProxy.Credentials = (ICredentials)CredentialCache.DefaultNetworkCredentials;
                        }
                        WebRequest.DefaultWebProxy = systemWebProxy;
                    }
                    verifierInfo.Proxy = this._configuration.Proxy;
                }
                else
                {
                    WebRequest.DefaultWebProxy = (IWebProxy)null;
                    verifierInfo.Proxy = (WebProxy)null;
                }
                verifierInfo.AsyncPdfProcessing -= new ProgressChangedEventHandler(this.verifierInfo_AsyncPdfProcessing);
                verifierInfo.AsyncPdfProcessing += new ProgressChangedEventHandler(this.verifierInfo_AsyncPdfProcessing);
                verifierInfo.AsyncProcessCompleted -= new RunWorkerCompletedEventHandler(this.verifierInfo_AsyncProcessCompleted);
                verifierInfo.AsyncProcessCompleted += new RunWorkerCompletedEventHandler(this.verifierInfo_AsyncProcessCompleted);
                verifierInfo.AsyncVerify();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void verifierInfo_AsyncPdfProcessing(
          object sender,
          ProgressChangedEventArgs e)
        {
            if (e.UserState is PdfProccessEventArgs)
            {
                PdfProccessEventArgs userState = e.UserState as PdfProccessEventArgs;
                if (userState.ValidateProccess == ValidityProccess.VerifyDocument)
                {
                    this.picToanvenStatus.Visible = true;
                    this.picToanvenStatus.Image = (Image)Resources.loading2;
                    this.lbToanVen.Text = "Đang kiểm tra tình toàn vẹn của tài liệu...";
                    this.lbToanVen.Visible = true;
                }
                else if (userState.ValidateProccess == ValidityProccess.CheckingSigningCert)
                {
                    this.picSignerCertStatus.Image = (Image)Resources.loading2;
                    this.picSignerCertStatus.Visible = true;
                    this.lbKTCTSKy.Text = "Đang kiểm tra chứng thư số ký...";
                    this.lbKTCTSKy.Visible = true;
                }
                else
                {
                    if (userState.ValidateProccess != ValidityProccess.VerifyTimeStamp)
                        return;
                    this.picTSACertStatus.Visible = true;
                    this.lbKTCTSTSA.Visible = true;
                    this.picTSACertStatus.Image = (Image)Resources.loading2;
                    this.lbKTCTSTSA.Text = "Đang kiểm tra chứng thư số máy chủ cấp dấu thời gian...";
                }
            }
            else
            {
                if (!(e.UserState is PdfProccessCompletedEventArgs))
                    return;
                PdfProccessCompletedEventArgs userState = e.UserState as PdfProccessCompletedEventArgs;
                if (userState.Proccess == ValidityProccess.VerifyDocument)
                {
                    this.picToanvenStatus.Visible = true;
                    if (userState.State == SignatureValidity.None)
                    {
                        this.picToanvenStatus.Image = (Image)Resources.ok16;
                        this.lbToanVen.Text = "Tài liệu chưa bị thay đổi.";
                    }
                    else
                    {
                        if (userState.State == SignatureValidity.NonCoversWholeDocument)
                        {
                            this.picToanvenStatus.Image = (Image)Resources.okinfo;
                            this.lbToanVen.Text = "Nội dung đã ký số chưa bị thay đổi. Tài liệu đã được thêm chú thích, hoặc ký số, hoặc thay đổi thông tin trên trường nhập liệu.";
                        }
                        if (userState.State == SignatureValidity.FatalError)
                        {
                            int num = (int)MessageBox.Show("Lỗi kiểm tra chữ ký: " + userState.Message, "Kiểm tra chữ ký", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            this.Close();
                        }
                        else
                        {
                            this.picToanvenStatus.Image = (Image)Resources.alert2;
                            this.lbToanVen.Text = userState.Message;
                        }
                    }
                }
                else if (userState.Proccess == ValidityProccess.CheckingSigningCert)
                {
                    this.picSignerCertStatus.Visible = true;
                    if (userState.State == SignatureValidity.None)
                    {
                        this.picSignerCertStatus.Image = (Image)Resources.ok16;
                        this.lbKTCTSKy.Text = "Chứng thư số ký hợp lệ.";
                    }
                    else if (userState.State == SignatureValidity.InvalidSigningCertificate)
                    {
                        this.picSignerCertStatus.Image = (Image)Resources.alert2;
                        this.lbKTCTSKy.Text = "Chứng thư số ký không hợp lệ: " + userState.Message;
                    }
                    else if (userState.State == SignatureValidity.ErrorCheckingSigningCertificate)
                    {
                        this.picSignerCertStatus.Image = (Image)Resources.alert2;
                        this.lbKTCTSKy.Text = "Lỗi quá trình kiểm tra: " + userState.Message;
                    }
                    else
                    {
                        if (userState.State != SignatureValidity.NonCheckingRevokedSigningCert)
                        {

                        }
                        else
                        {
                            this.picSignerCertStatus.Image = (Image)Resources.alert2;
                            this.lbKTCTSKy.Text = "Chứng thư số ký không được kiểm tra tình trạng hủy bỏ.";
                        }
                    }
                }
                else
                {
                    if (userState.Proccess != ValidityProccess.VerifyTimeStamp)
                        return;
                    if (userState.State == SignatureValidity.None)
                    {
                        this.picTSACertStatus.Image = (Image)Resources.ok16;
                        this.lbKTCTSTSA.Text = "Dấu thời gian trên chữ ký hợp lệ.";
                    }
                    else if ((userState.State & SignatureValidity.InvalidTimestampImprint) != SignatureValidity.None)
                    {
                        this.picTSACertStatus.Image = (Image)Resources.alert2;
                        this.lbKTCTSTSA.Text = "Dấu thời gian trên chữ ký không hợp lệ.";
                    }
                    else if ((userState.State & SignatureValidity.InvalidTSACertificate) != SignatureValidity.None)
                    {
                        this.picTSACertStatus.Image = (Image)Resources.alert2;
                        this.lbKTCTSTSA.Text = "Chứng thư số máy chủ cấp dấu thời gian không hợp lệ: " + userState.Message;
                    }
                    else if ((userState.State & SignatureValidity.ErrorCheckingTSACertificate) != SignatureValidity.None)
                    {
                        this.picTSACertStatus.Image = (Image)Resources.alert2;
                        this.lbKTCTSTSA.Text = "Lỗi quá trình kiểm tra: " + userState.Message;
                    }
                    else if ((userState.State & SignatureValidity.NotTimestamped) != SignatureValidity.None)
                    {
                        this.picTSACertStatus.Image = (Image)Resources.alert2;
                        this.lbKTCTSTSA.Text = "Chữ ký không được gắn dấu thời gian.";
                    }
                    else if ((userState.State & SignatureValidity.NonCheckingRevokedTSACert) != SignatureValidity.None)
                    {
                        this.picTSACertStatus.Image = (Image)Resources.alert2;
                        this.lbKTCTSTSA.Text = "Chứng thư số máy chủ cấp dấu thời gian không được kiểm tra tình trạng hủy bỏ.";
                    }
                }
            }
        }

        private void verifierInfo_AsyncProcessCompleted(
          object sender,
          RunWorkerCompletedEventArgs e)
        {
            try
            {
                PdfVerifier verifierInfo = (PdfVerifier)sender;
                DateTime dateTime1;
                if (!verifierInfo.Signature.IsTsp)
                {
                    dateTime1 = verifierInfo.Signature.SigningTime;
                }
                else
                    dateTime1 = verifierInfo.Signature.TimeStampDate;
                DateTime dateTime2 = dateTime1;
                this.lbSigningTime.Text = string.Format("Thời gian ký: {0}", (object)dateTime2.ToString("dd/MM/yyyy HH:mm:ss zzz"));
                CertInfo certInfo1 = new CertInfo(verifierInfo.Signature.SigningCertificate);
                this.lklbSignerCert.Tag = (object)certInfo1;
                this.lklbSignerCert.Text = certInfo1.ToString();
                if ((verifierInfo.VerifyResult & SignatureValidity.NotTimestamped) == SignatureValidity.None)
                {
                    this.lbCheckingTime.Text = string.Format("Thời gian kiểm tra: {0}", (object)verifierInfo.Signature.TimeStampDate.ToString("dd/MM/yyyy HH:mm:ss zzz"));
                    this.groupBox3.Visible = true;
                    this.Size = this.MaximumSize;
                    this.Invalidate();
                    this.lbTimeStampDate.Text = string.Format("Dấu thời gian: {0}", (object)verifierInfo.Signature.TimeStampDate.ToString("dd/MM/yyyy HH:mm:ss zzz"));
                    CertInfo certInfo2 = new CertInfo(verifierInfo.Signature.TimeStampCertificate);
                    this.lklbTsaCert.Text = certInfo2.ToString();
                    this.lklbTsaCert.Tag = (object)certInfo2;
                }
                else
                {
                    this.lbCheckingTime.Text = string.Format("Thời gian kiểm tra: {0}", (object)verifierInfo.Signature.SigningTime.ToString("dd/MM/yyyy HH:mm:ss zzz"));
                    this.groupBox3.Visible = false;
                    this.Size = this.MinimumSize;
                    this.Invalidate();
                }
                if ((verifierInfo.VerifyResult & (SignatureValidity.DocumentModified | SignatureValidity.InvalidSigningCertificate)) != SignatureValidity.None)
                {
                    this.picAllStatus.Image = (Image)Resources.alert2;
                    this.lbValidityStatus.Text = "Chữ ký không hợp lệ!";
                }
                else
                {
                    if (verifierInfo.VerifyResult != SignatureValidity.None)
                    {
                        if (verifierInfo.VerifyResult != SignatureValidity.NonCoversWholeDocument)
                        {
                            this.picAllStatus.Image = (Image)Resources.alert2;
                            this.lbValidityStatus.Text = "Không đủ thông tin xác thực chữ ký!";
                            return;
                        }
                    }
                    this.picAllStatus.Image = (Image)Resources.ok16;
                    this.lbValidityStatus.Text = "Chữ ký hợp lệ!";
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void btnClose_Click(
          object sender,
          EventArgs e)
        {
            this.Close();
        }

        private void lklbSignerCert_LinkClicked(
          object sender,
          LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                X509Certificate2UI.DisplayCertificate((((Control)sender).Tag as CertInfo).Certificate, this.Handle);
            }
            catch
            {
            }
        }

        private void lklbTsaCert_LinkClicked(
          object sender,
          LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                X509Certificate2UI.DisplayCertificate((((Control)sender).Tag as CertInfo).Certificate, this.Handle);
            }
            catch
            {
            }
        }

        private void Form_Shown(
          object sender,
          EventArgs e)
        {
            this.initFormVerify();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.component != null)
                {
                    this.component.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(frmVerifier));
            this.groupBox1 = new GroupBox();
            this.lklbSignerCert = new LinkLabel();
            this.picAllStatus = new PictureBox();
            this.lbSigner = new Label();
            this.lbSigningTime = new Label();
            this.lbValidityStatus = new Label();
            this.groupBox2 = new GroupBox();
            this.picTSACertStatus = new PictureBox();
            this.picSignerCertStatus = new PictureBox();
            this.picToanvenStatus = new PictureBox();
            this.lbCheckingTime = new Label();
            this.lbKTCTSTSA = new Label();
            this.lbKTCTSKy = new Label();
            this.lbToanVen = new Label();
            this.btnClose = new Button();
            this.groupBox3 = new GroupBox();
            this.lklbTsaCert = new LinkLabel();
            this.label1 = new Label();
            this.lbTimeStampDate = new Label();
            this.panel1 = new Panel();
            this.groupBox1.SuspendLayout();
            ((ISupportInitialize)this.picAllStatus).BeginInit();
            this.groupBox2.SuspendLayout();
            ((ISupportInitialize)this.picTSACertStatus).BeginInit();
            ((ISupportInitialize)this.picSignerCertStatus).BeginInit();
            ((ISupportInitialize)this.picToanvenStatus).BeginInit();
            this.groupBox3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            this.groupBox1.Controls.Add((Control)this.lklbSignerCert);
            this.groupBox1.Controls.Add((Control)this.picAllStatus);
            this.groupBox1.Controls.Add((Control)this.lbSigner);
            this.groupBox1.Controls.Add((Control)this.lbSigningTime);
            this.groupBox1.Controls.Add((Control)this.lbValidityStatus);
            this.groupBox1.Location = new Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(386, 104);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Thông tin chữ ký";
            this.lklbSignerCert.ActiveLinkColor = Color.FromArgb(64, 0, 0);
            this.lklbSignerCert.AutoEllipsis = true;
            this.lklbSignerCert.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.lklbSignerCert.LinkColor = Color.FromArgb(64, 0, 0);
            this.lklbSignerCert.Location = new Point(148, 77);
            this.lklbSignerCert.Name = "lklbSignerCert";
            this.lklbSignerCert.Size = new Size(229, 13);
            this.lklbSignerCert.TabIndex = 1;
            this.lklbSignerCert.TabStop = true;
            this.lklbSignerCert.Text = "Đang cập nhật...";
            this.lklbSignerCert.VisitedLinkColor = Color.FromArgb(64, 0, 0);
            this.lklbSignerCert.LinkClicked += new LinkLabelLinkClickedEventHandler(this.lklbSignerCert_LinkClicked);
            this.picAllStatus.Image = (Image)Resources.loading2;
            this.picAllStatus.Location = new Point(13, 19);
            this.picAllStatus.Name = "picAllStatus";
            this.picAllStatus.Size = new Size(45, 45);
            this.picAllStatus.SizeMode = PictureBoxSizeMode.Zoom;
            this.picAllStatus.TabIndex = 1;
            this.picAllStatus.TabStop = false;
            this.lbSigner.AutoSize = true;
            this.lbSigner.Location = new Point(75, 77);
            this.lbSigner.Name = "lbSigner";
            this.lbSigner.Size = new Size(67, 13);
            this.lbSigner.TabIndex = 0;
            this.lbSigner.Text = "Được ký bởi:";
            this.lbSigningTime.AutoSize = true;
            this.lbSigningTime.Location = new Point(75, 50);
            this.lbSigningTime.Name = "lbSigningTime";
            this.lbSigningTime.Size = new Size(151, 13);
            this.lbSigningTime.TabIndex = 0;
            this.lbSigningTime.Text = "Thời gian ký: Đang cập nhật...";
            this.lbValidityStatus.Location = new Point(75, 19);
            this.lbValidityStatus.Name = "lbValidityStatus";
            this.lbValidityStatus.Size = new Size(305, 28);
            this.lbValidityStatus.TabIndex = 0;
            this.lbValidityStatus.Text = "Đang kiểm tra chữ ký...";
            this.groupBox2.Controls.Add((Control)this.picTSACertStatus);
            this.groupBox2.Controls.Add((Control)this.picSignerCertStatus);
            this.groupBox2.Controls.Add((Control)this.picToanvenStatus);
            this.groupBox2.Controls.Add((Control)this.lbCheckingTime);
            this.groupBox2.Controls.Add((Control)this.lbKTCTSTSA);
            this.groupBox2.Controls.Add((Control)this.lbKTCTSKy);
            this.groupBox2.Controls.Add((Control)this.lbToanVen);
            this.groupBox2.Location = new Point(12, 122);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new Size(386, 143);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Chi tiết xác thực";
            this.picTSACertStatus.Image = (Image)Resources.ok16;
            this.picTSACertStatus.Location = new Point(13, 86);
            this.picTSACertStatus.Name = "picTSACertStatus";
            this.picTSACertStatus.Size = new Size(24, 23);
            this.picTSACertStatus.SizeMode = PictureBoxSizeMode.Zoom;
            this.picTSACertStatus.TabIndex = 1;
            this.picTSACertStatus.TabStop = false;
            this.picTSACertStatus.Visible = false;
            this.picSignerCertStatus.Image = (Image)Resources.ok16;
            this.picSignerCertStatus.Location = new Point(13, 54);
            this.picSignerCertStatus.Name = "picSignerCertStatus";
            this.picSignerCertStatus.Size = new Size(24, 23);
            this.picSignerCertStatus.SizeMode = PictureBoxSizeMode.Zoom;
            this.picSignerCertStatus.TabIndex = 1;
            this.picSignerCertStatus.TabStop = false;
            this.picSignerCertStatus.Visible = false;
            this.picToanvenStatus.Image = (Image)Resources.loading2;
            this.picToanvenStatus.Location = new Point(13, 22);
            this.picToanvenStatus.Name = "picToanvenStatus";
            this.picToanvenStatus.Size = new Size(24, 23);
            this.picToanvenStatus.SizeMode = PictureBoxSizeMode.Zoom;
            this.picToanvenStatus.TabIndex = 1;
            this.picToanvenStatus.TabStop = false;
            this.picToanvenStatus.Visible = false;
            this.lbCheckingTime.AutoSize = true;
            this.lbCheckingTime.Location = new Point(53, 119);
            this.lbCheckingTime.Name = "lbCheckingTime";
            this.lbCheckingTime.Size = new Size(177, 13);
            this.lbCheckingTime.TabIndex = 0;
            this.lbCheckingTime.Text = "Thời gian kiểm tra: Đang cập nhật...";
            this.lbKTCTSTSA.Location = new Point(53, 88);
            this.lbKTCTSTSA.Name = "lbKTCTSTSA";
            this.lbKTCTSTSA.Size = new Size(321, 28);
            this.lbKTCTSTSA.TabIndex = 0;
            this.lbKTCTSTSA.Text = "Chứng thư số máy chủ cấp dấu thời gian hợp lệ";
            this.lbKTCTSTSA.Visible = false;
            this.lbKTCTSKy.Location = new Point(53, 58);
            this.lbKTCTSKy.Name = "lbKTCTSKy";
            this.lbKTCTSKy.Size = new Size(324, 28);
            this.lbKTCTSKy.TabIndex = 0;
            this.lbKTCTSKy.Text = "Chứng thư số ký hợp lệ";
            this.lbKTCTSKy.Visible = false;
            this.lbToanVen.Location = new Point(53, 26);
            this.lbToanVen.Name = "lbToanVen";
            this.lbToanVen.Size = new Size(327, 28);
            this.lbToanVen.TabIndex = 0;
            this.lbToanVen.Text = "Tài liệu chưa bị thay đổi";
            this.lbToanVen.Visible = false;
            this.btnClose.Location = new Point(300, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(80, 26);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Đóng";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new EventHandler(this.btnClose_Click);
            this.groupBox3.Controls.Add((Control)this.lklbTsaCert);
            this.groupBox3.Controls.Add((Control)this.label1);
            this.groupBox3.Controls.Add((Control)this.lbTimeStampDate);
            this.groupBox3.Location = new Point(12, 271);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new Size(386, 100);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Thông tin dấu thời gian";
            this.groupBox3.Visible = false;
            this.lklbTsaCert.ActiveLinkColor = Color.FromArgb(64, 0, 0);
            this.lklbTsaCert.AutoSize = true;
            this.lklbTsaCert.Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.lklbTsaCert.LinkColor = Color.FromArgb(64, 0, 0);
            this.lklbTsaCert.Location = new Point(87, 70);
            this.lklbTsaCert.Name = "lklbTsaCert";
            this.lklbTsaCert.Size = new Size(175, 15);
            this.lklbTsaCert.TabIndex = 1;
            this.lklbTsaCert.TabStop = true;
            this.lklbTsaCert.Text = "Máy chủ cấp dấu thời gian";
            this.lklbTsaCert.VisitedLinkColor = Color.FromArgb(64, 0, 0);
            this.lklbTsaCert.LinkClicked += new LinkLabelLinkClickedEventHandler(this.lklbTsaCert_LinkClicked);
            this.label1.AutoSize = true;
            this.label1.Location = new Point(53, 52);
            this.label1.Name = "label1";
            this.label1.Size = new Size(201, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Chứng thư số máy chủ cấp dấu thời gian:";
            this.lbTimeStampDate.AutoSize = true;
            this.lbTimeStampDate.Location = new Point(53, 25);
            this.lbTimeStampDate.Name = "lbTimeStampDate";
            this.lbTimeStampDate.Size = new Size(228, 13);
            this.lbTimeStampDate.TabIndex = 0;
            this.lbTimeStampDate.Text = "Dấu thời gian: 30/06/2013 11:20:20 AM +7:00";
            this.panel1.Controls.Add((Control)this.btnClose);
            this.panel1.Dock = DockStyle.Bottom;
            this.panel1.Location = new Point(10, 376);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(390, 48);
            this.panel1.TabIndex = 4;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(410, 424);
            this.Controls.Add((Control)this.panel1);
            this.Controls.Add((Control)this.groupBox3);
            this.Controls.Add((Control)this.groupBox2);
            this.Controls.Add((Control)this.groupBox1);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            this.MaximizeBox = false;
            this.MaximumSize = new Size(416, 452);
            this.MinimizeBox = false;
            this.MinimumSize = new Size(416, 346);
            this.Name = "frmVerifier";
            this.Padding = new Padding(10, 10, 10, 0);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Xác thực chữ ký";
            this.Shown += new EventHandler(this.Form_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((ISupportInitialize)this.picAllStatus).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((ISupportInitialize)this.picTSACertStatus).EndInit();
            ((ISupportInitialize)this.picSignerCertStatus).EndInit();
            ((ISupportInitialize)this.picToanvenStatus).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
