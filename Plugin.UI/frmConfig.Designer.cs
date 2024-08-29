using MetroFramework;
using MetroFramework.Controls;
using MetroFramework.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Plugin.UI.Properties;
using Plugin.UI.Configurations;
using Sign.PDF;

namespace Plugin.UI
{
    public partial class frmConfig : MetroForm
    {
        private string tsaUrlDefault = "http://tsa.ca.gov.vn";
        private Dictionary<ConfigKey, object> lstChanged;
        private SignerProfileStore spStore;
        private bool loadedConfigs;
        private const string CREATE_NEW_SIG_APPR = "Tạo mẫu mới...";
        private bool shownSigAppr;
        private string _signersJob;
        private IContainer components;
        private MetroTabControl tabCtrlMain;
        private TabPage tabPageConnection;
        private TabPage tabPagePKIServices;
        private TabPage tabPagePdf;
        private MetroCheckBox ckbUsePx;
        private MetroRadioButton rdbAutoDetectPx;
        private MetroRadioButton rdbUseSpecifiedPx;
        private MetroLabel lbPxAddress;
        private MetroCheckBox ckbUsePxAuth;
        private NumericUpDown nPxPort;
        private TextBox txtPxAddress;
        private MetroLabel lbPxPort;
        private MetroLabel lbPxPassword;
        private TextBox txtPxPassword;
        private TextBox txtPxUsername;
        private MetroLabel lbPxUsername;
        private MetroCheckBox ckbBypassLocal;
        private MetroButton btnClose;
        private MetroCheckBox ckbUseTsa;
        private TextBox txtTsaAddress;
        private MetroLabel metroLabel5;
        private GroupBox gbProfileDetails;
        private MetroLabel metroLabel6;
        private MetroComboBox cbbSignerProfiles;
        private TextBox txtSignerProfileName;
        private MetroButton btnSave;
        private MetroButton btnRestore;
        private MetroButton btnBackup;
        private MetroCheckBox ckbSetProfileAsDefault;
        private MetroCheckBox mcbShowSigLabel;
        private MetroCheckBox mcbShowCQ1;
        private MetroCheckBox mcbShowEmail;
        private ContextMenuStrip CtxtMnTripImage;
        private MetroRadioButton mrbTypeDescription;
        private MetroRadioButton mrbTypeGraphVsDes;
        private MetroRadioButton mrbTypeGraphy;
        private GroupBox groupBox1;
        private Label lbSigDescription;
        private PictureBox pcbSignatureImage;
        private Label label1;
        private ToolStripMenuItem changeImageToolStripMenuItem;
        private ToolStripMenuItem saveImageToolStripMenuItem;
        private MetroButton btnDelProfile;
        private MetroCheckBox mcbShowTime;
        private TabPage tabPageUpdate;
        private MetroCheckBox ckbAutoUpdate;
        private MetroPanel pnlSigAppearance;
        private Panel panel1;
        private MetroLabel mLabelCurrVersion;
        private MetroLabel mLabelLastCheckingUpdate;
        private MetroLabel metroLabel2;
        private MetroLabel metroLabel1;
        private MetroCheckBox mcbShowCQ3;
        private ToolStripMenuItem setDefaultImageToolStripMenuItem;
        private GroupBox gbTSA;
        private MetroCheckBox ckbAllowCertCheckingOnline;
        private GroupBox gbOnlineCertChecking;
        private Label label2;
        private MetroCheckBox ckbCheckingSignerCertViaOcsp;
        private ListBox lstAdditionalCrls;
        private LinkLabel btnDelCrlLink;
        private LinkLabel btnAddCrlLink;
        private MetroCheckBox ckbOrgSigner;
        private MetroCheckBox mcbShowCQ2;

        public frmConfig()
        {
            this.InitializeComponent();
            this.loadedConfigs = false;
        }

        public frmConfig(bool shownSigAppearances)
        {
            this.InitializeComponent();
            this.loadedConfigs = false;
            this.shownSigAppr = shownSigAppearances;
        }

        private void LoadConfig()
        {
            this.lstChanged = new Dictionary<ConfigKey, object>();
            ((Control)this.btnSave).Visible = true;
            ((Control)this.btnSave).Enabled = false;
            ((Control)this.btnBackup).Visible = false;
            ((Control)this.btnRestore).Visible = false;
            ((Control)this.btnDelProfile).Visible = false;
            ((TabControl)this.tabCtrlMain).SelectedIndex = 0;
            object configByKey1 = Configuration.GetConfigByKey(ConfigKey.UseProxy);
            ((CheckBox)this.ckbUsePx).Checked = configByKey1 != null && Convert.ToBoolean(configByKey1);
            object configByKey2 = Configuration.GetConfigByKey(ConfigKey.AutoDetectProxy);
            if ((configByKey2 != null ? (Convert.ToBoolean(configByKey2) ? 1 : 0) : 0) != 0)
                ((RadioButton)this.rdbAutoDetectPx).Checked = true;
            else
                ((RadioButton)this.rdbUseSpecifiedPx).Checked = true;
            this.txtPxAddress.Text = (string)Configuration.GetConfigByKey(ConfigKey.PxAddress);
            object configByKey3 = Configuration.GetConfigByKey(ConfigKey.PxPort);
            try
            {
                this.nPxPort.Value = (Decimal)Convert.ToInt32(configByKey3);
            }
            catch
            {
            }
            object configByKey4 = Configuration.GetConfigByKey(ConfigKey.UsePxAuth);
            ((CheckBox)this.ckbUsePxAuth).Checked = configByKey4 != null && Convert.ToBoolean(configByKey4);
            this.txtPxUsername.Text = (string)Configuration.GetConfigByKey(ConfigKey.PxUsername);
            this.txtPxPassword.Text = (string)Configuration.GetConfigByKey(ConfigKey.PxPassword);
            object configByKey5 = Configuration.GetConfigByKey(ConfigKey.BypassPxForLocal);
            ((CheckBox)this.ckbBypassLocal).Checked = configByKey5 != null && Convert.ToBoolean(configByKey5);
            object configByKey6 = Configuration.GetConfigByKey(ConfigKey.UseTsa);
            ((CheckBox)this.ckbUseTsa).Checked = configByKey6 != null && Convert.ToBoolean(configByKey6);
            this.txtTsaAddress.Text = (string)Configuration.GetConfigByKey(ConfigKey.TsaAddress);
            if (string.IsNullOrEmpty(this.txtTsaAddress.Text))
                this.txtTsaAddress.Text = this.tsaUrlDefault;
            this.txtTsaAddress.Tag = (object)this.txtTsaAddress.Text;
            this.gbTSA.Enabled = ((CheckBox)this.ckbUseTsa).Checked;
            object configByKey7 = Configuration.GetConfigByKey(ConfigKey.AllowedOnlineCheckingCert);
            ((CheckBox)this.ckbAllowCertCheckingOnline).Checked = configByKey7 == null || Convert.ToBoolean(configByKey7);
            object configByKey8 = Configuration.GetConfigByKey(ConfigKey.AllowedOCSPForCheckingSigningCert);
            ((CheckBox)this.ckbCheckingSignerCertViaOcsp).Checked = configByKey8 == null || Convert.ToBoolean(configByKey8);
            string configByKey9 = (string)Configuration.GetConfigByKey(ConfigKey.AdditionalCrls);
            this.lstAdditionalCrls.Items.Clear();
            if (!string.IsNullOrEmpty(configByKey9))
            {
                string str = configByKey9;
                char[] separator = new char[1] { '|' };
                foreach (object obj in str.Split(separator, StringSplitOptions.RemoveEmptyEntries))
                    this.lstAdditionalCrls.Items.Add(obj);
            }
            this.gbOnlineCertChecking.Enabled = ((CheckBox)this.ckbAllowCertCheckingOnline).Checked;
            try
            {
                if (!Directory.Exists(Configuration.WORKING_PATH))
                    Directory.CreateDirectory(Configuration.WORKING_PATH);
            }
            catch
            {
            }
            try
            {
                this.gbProfileDetails.Enabled = false;
                this.spStore = new SignerProfileStore(Path.Combine(Configuration.WORKING_PATH, "signerprofile.xml"));
                this.ReloadSP();
                int num = -1;
                if (this.spStore.DefaultSigner < -1 || this.spStore.DefaultSigner > ((ComboBox)this.cbbSignerProfiles).Items.Count - 1)
                    num = -1;
                ((ListControl)this.cbbSignerProfiles).SelectedIndex = num;
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("Không thể duyệt danh sách profiles ký.", "Cấu hình hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            object configByKey10 = Configuration.GetConfigByKey(ConfigKey.UpdateOnStart);
            ((CheckBox)this.ckbAutoUpdate).Checked = configByKey10 != null && Convert.ToBoolean(configByKey10);
            object configByKey11 = Configuration.GetConfigByKey(ConfigKey.LastCheckingUpdate);
            try
            {
                ((Control)this.mLabelLastCheckingUpdate).Text = configByKey11 != null && !string.IsNullOrEmpty((string)configByKey11) ? (string)configByKey11 : throw new Exception();
            }
            catch
            {
                ((Control)this.mLabelLastCheckingUpdate).Text = "N/A";
            }
            object obj1 = (!VistaTools.Is64BitOperatingSystem() ? new XRegistry("SOFTWARE\\HIPT\\Pdf", Registry.LocalMachine) : new XRegistry("Software\\Wow6432Node\\HIPT\\Pdf", Registry.LocalMachine)).Read("Version");
            try
            {
                ((Control)this.mLabelCurrVersion).Text = obj1 != null && !string.IsNullOrEmpty((string)obj1) ? (string)obj1 : throw new Exception();
            }
            catch
            {
                ((Control)this.mLabelCurrVersion).Text = "N/A";
            }
            this.loadedConfigs = true;
        }

        private void ReloadSP()
        {
            ((ComboBox)this.cbbSignerProfiles).Items.Clear();
            foreach (object obj in this.spStore)
                ((ComboBox)this.cbbSignerProfiles).Items.Add(obj);
            ((ComboBox)this.cbbSignerProfiles).Items.Add((object)"Tạo mẫu mới...");
        }

        private void AddValueChanged(ConfigKey key, object value)
        {
            if (!this.loadedConfigs)
                return;
            if (!this.lstChanged.ContainsKey(key))
                this.lstChanged.Add(key, value);
            else
                this.lstChanged[key] = value;
            ((Control)this.btnSave).Enabled = true;
        }

        private void ckbUsePx_CheckedChanged(object sender, EventArgs e) => this.AddValueChanged(ConfigKey.UseProxy, (object)((CheckBox)this.ckbUsePx).Checked);

        private void rdbAutoDetectPx_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)this.rdbAutoDetectPx).Checked)
                return;
            this.AddValueChanged(ConfigKey.AutoDetectProxy, (object)true);
            this.SetEnabledPx(false);
        }

        private void SetEnabledPx(bool b)
        {
            ((Control)this.lbPxAddress).Enabled = b;
            ((Control)this.lbPxPort).Enabled = b;
            this.txtPxAddress.Enabled = b;
            this.nPxPort.Enabled = b;
        }

        private void rdbUseSpecifiedPx_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)this.rdbUseSpecifiedPx).Checked)
                return;
            this.AddValueChanged(ConfigKey.AutoDetectProxy, (object)false);
            this.SetEnabledPx(true);
        }

        private void SetEnabledPxAuth(bool b)
        {
            ((Control)this.lbPxUsername).Enabled = b;
            ((Control)this.lbPxPassword).Enabled = b;
            this.txtPxUsername.Enabled = b;
            this.txtPxPassword.Enabled = b;
        }

        private void ckbUsePxAuth_CheckedChanged(object sender, EventArgs e)
        {
            this.AddValueChanged(ConfigKey.UsePxAuth, (object)((CheckBox)this.ckbUsePxAuth).Checked);
            this.SetEnabledPxAuth(((CheckBox)this.ckbUsePxAuth).Checked);
        }

        private void ckbBypassLocal_CheckedChanged(object sender, EventArgs e) => this.AddValueChanged(ConfigKey.BypassPxForLocal, (object)((CheckBox)this.ckbBypassLocal).Checked);

        private void txtPxAddress_TextChanged(object sender, EventArgs e) => this.AddValueChanged(ConfigKey.PxAddress, (object)this.txtPxAddress.Text.Trim());

        private void nPxPort_ValueChanged(object sender, EventArgs e) => this.AddValueChanged(ConfigKey.PxPort, (object)this.nPxPort.Value);

        private void txtPxUsername_TextChanged(object sender, EventArgs e) => this.AddValueChanged(ConfigKey.PxUsername, (object)this.txtPxUsername.Text.Trim());

        private void txtPxPassword_TextChanged(object sender, EventArgs e) => this.AddValueChanged(ConfigKey.PxPassword, (object)this.txtPxPassword.Text.Trim());

        private void ckbUseTsa_CheckedChanged(object sender, EventArgs e)
        {
            this.AddValueChanged(ConfigKey.UseTsa, (object)((CheckBox)this.ckbUseTsa).Checked);
            this.gbTSA.Enabled = ((CheckBox)this.ckbUseTsa).Checked;
            if (!((CheckBox)this.ckbUseTsa).Checked)
                return;
            this.txtTsaAddress.Text = string.IsNullOrEmpty((string)this.txtTsaAddress.Tag) ? "http://tsa.ca.gov.vn" : (string)this.txtTsaAddress.Tag;
        }

        private void txtTsaAddress_TextChanged(object sender, EventArgs e)
        {
            this.AddValueChanged(ConfigKey.TsaAddress, (object)this.txtTsaAddress.Text.Trim());
            this.txtTsaAddress.Tag = (object)this.txtTsaAddress.Text;
        }

        private void SelectSigImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Image Files (*.jpeg, *.jpg, *.png, *.bmp)|*.jpeg;*.jpg;*.png;*.bmp";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            try
            {
                if (Decimal.Round((Decimal)new FileInfo(openFileDialog.FileName).Length / 1024M / 1024M, 2, MidpointRounding.AwayFromZero).CompareTo(3.5M) > 0)
                {
                    int num = (int)MessageBox.Show("Dung lượng tệp quá lớn!(Tối đa 3.5Mb).", "Chọn hình ảnh chữ ký", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    if (this.pcbSignatureImage.Image != null)
                    {
                        this.pcbSignatureImage.Image.Dispose();
                    }
                    
                    this.pcbSignatureImage.Image = Image.FromFile(openFileDialog.FileName);
                    this.pcbSignatureImage.Refresh();
                    ((Control)this.btnSave).Enabled = true;
                }
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("Không đúng định dạng hình ảnh chữ ký.", "Chọn hình ảnh chữ ký", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void DelProfile()
        {
            if (((ListControl)this.cbbSignerProfiles).SelectedIndex == ((ComboBox)this.cbbSignerProfiles).Items.Count - 1)
                return;
            if (((ListControl)this.cbbSignerProfiles).SelectedIndex != -1)
            {
                if (MessageBox.Show(string.Format("Bạn có muốn xóa profile \"{0}\"?", (object)this.txtSignerProfileName.Text), "Xóa profile ký", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                    return;
                this.spStore.Remove(((ListControl)this.cbbSignerProfiles).SelectedIndex);
                this.ReloadSP();
                this.ClearFields();
                ((Control)this.cbbSignerProfiles).Text = "";
                ((ListControl)this.cbbSignerProfiles).SelectedIndex = -1;
                ((Control)this.cbbSignerProfiles).Refresh();
                this.gbProfileDetails.Enabled = false;
            }
            else
            {
                int num = (int)MessageBox.Show("Chưa chọn profile.", "Xóa profile ký", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
        }

        private void ClearFields()
        {
            ((CheckBox)this.ckbSetProfileAsDefault).Checked = false;
            this.txtSignerProfileName.Text = "";
            this.pcbSignatureImage.Image.Dispose();
            this.pcbSignatureImage.Image = (Image)Resources.HiPT;
            ((RadioButton)this.mrbTypeGraphVsDes).Checked = true;
            ((CheckBox)this.ckbOrgSigner).Checked = false;
            ((CheckBox)this.mcbShowEmail).Checked = true;
            ((CheckBox)this.mcbShowTime).Checked = true;
            ((CheckBox)this.mcbShowCQ1).Checked = true;
            ((CheckBox)this.mcbShowCQ2).Checked = true;
            ((CheckBox)this.mcbShowSigLabel).Checked = true;
            ((CheckBox)this.mcbShowCQ3).Checked = false;
        }

        private void MeasureSignatureText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string str = "";
            if (((CheckBox)this.mcbShowCQ3).Checked)
                str = "CQ Cấp 3";
            if (((CheckBox)this.mcbShowCQ2).Checked)
            {
                if (!string.IsNullOrEmpty(str))
                    str += ", ";
                str += "CQ Cấp 2";
            }
            if (((CheckBox)this.mcbShowCQ1).Checked)
            {
                if (!string.IsNullOrEmpty(str))
                    str += ", ";
                str += "CQ Cấp 1";
            }
            if (((CheckBox)this.ckbOrgSigner).Checked)
            {
                if (((CheckBox)this.mcbShowSigLabel).Checked)
                {
                    stringBuilder.Append("Cơ quan: Tên tổ chức");
                    if (!string.IsNullOrEmpty(str))
                    {
                        stringBuilder.Append(", ");
                        stringBuilder.Append(str);
                    }
                }
                else
                {
                    stringBuilder.Append("Tên tổ chức");
                    if (!string.IsNullOrEmpty(str))
                    {
                        stringBuilder.Append(", ");
                        stringBuilder.Append(str);
                    }
                }
                if (((CheckBox)this.mcbShowEmail).Checked)
                {
                    stringBuilder.Append(Environment.NewLine);
                    if (((CheckBox)this.mcbShowSigLabel).Checked)
                        stringBuilder.Append("Email: Địa chỉ thư điện tử");
                    else
                        stringBuilder.Append("Địa chỉ thư điện tử");
                }
                if (((CheckBox)this.mcbShowTime).Checked)
                {
                    stringBuilder.Append(Environment.NewLine);
                    if (((CheckBox)this.mcbShowSigLabel).Checked)
                        stringBuilder.Append("Thời gian ký: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss zzz"));
                    else
                        stringBuilder.Append(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss zzz"));
                }
            }
            else
            {
                if (((CheckBox)this.mcbShowSigLabel).Checked)
                    stringBuilder.Append("Người ký: Tên chứng thư số ký");
                else
                    stringBuilder.Append("Tên chứng thư số ký");
                if (((CheckBox)this.mcbShowEmail).Checked)
                {
                    stringBuilder.Append(Environment.NewLine);
                    if (((CheckBox)this.mcbShowSigLabel).Checked)
                        stringBuilder.Append("Email: Địa chỉ thư điện tử");
                    else
                        stringBuilder.Append("Địa chỉ thư điện tử");
                }
                if (!string.IsNullOrEmpty(str))
                {
                    stringBuilder.Append(Environment.NewLine);
                    if (((CheckBox)this.mcbShowSigLabel).Checked)
                    {
                        stringBuilder.Append("Cơ quan: ");
                        stringBuilder.Append(str);
                    }
                    else
                        stringBuilder.Append(str);
                }
                if (((CheckBox)this.mcbShowTime).Checked)
                {
                    stringBuilder.Append(Environment.NewLine);
                    if (((CheckBox)this.mcbShowSigLabel).Checked)
                        stringBuilder.Append("Thời gian ký: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss zzz"));
                    else
                        stringBuilder.Append(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss zzz"));
                }
            }
            float emSize = 7f;
            while (true)
            {
                int width = this.lbSigDescription.Width / 2 > this.lbSigDescription.Height ? this.lbSigDescription.Height : this.lbSigDescription.Width / 2;
                if (TextRenderer.MeasureText(stringBuilder.ToString(), new Font("Tahoma", emSize, FontStyle.Bold, GraphicsUnit.Pixel), new Size(width, width - 10), TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak).Height <= this.lbSigDescription.Height && (double)emSize <= 30.0)
                    ++emSize;
                else
                    break;
            }
            this.lbSigDescription.Font = new Font("Tahoma", emSize + 1f, FontStyle.Regular, GraphicsUnit.Pixel);
            this.lbSigDescription.Text = stringBuilder.ToString();
        }

        private void SigAppearanceChanged()
        {
            if (((RadioButton)this.mrbTypeGraphVsDes).Checked)
            {
                ((Control)this.mcbShowEmail).Enabled = true;
                ((Control)this.mcbShowTime).Enabled = true;
                ((Control)this.mcbShowCQ1).Enabled = true;
                ((Control)this.mcbShowCQ2).Enabled = true;
                ((Control)this.mcbShowSigLabel).Enabled = true;
                ((Control)this.mcbShowCQ3).Enabled = true;
                this.pcbSignatureImage.Dock = DockStyle.None;
                this.pcbSignatureImage.Location = new Point(0, 0);
                this.pcbSignatureImage.Size = new Size(166, 101);
                this.pcbSignatureImage.Visible = true;
                this.lbSigDescription.Dock = DockStyle.None;
                this.lbSigDescription.Location = new Point(166, 0);
                this.lbSigDescription.Size = new Size(189, 100);
                this.lbSigDescription.Visible = true;
            }
            else if (((RadioButton)this.mrbTypeGraphy).Checked)
            {
                ((Control)this.mcbShowEmail).Enabled = false;
                ((Control)this.mcbShowTime).Enabled = false;
                ((Control)this.mcbShowCQ1).Enabled = false;
                ((Control)this.mcbShowCQ2).Enabled = false;
                ((Control)this.mcbShowSigLabel).Enabled = false;
                ((Control)this.mcbShowCQ3).Enabled = false;
                this.pcbSignatureImage.Dock = DockStyle.Fill;
                this.pcbSignatureImage.Visible = true;
                this.lbSigDescription.Visible = false;
            }
            else if (((RadioButton)this.mrbTypeDescription).Checked)
            {
                ((Control)this.mcbShowEmail).Enabled = true;
                ((Control)this.mcbShowTime).Enabled = true;
                ((Control)this.mcbShowCQ1).Enabled = true;
                ((Control)this.mcbShowCQ2).Enabled = true;
                ((Control)this.mcbShowSigLabel).Enabled = true;
                ((Control)this.mcbShowCQ3).Enabled = true;
                this.pcbSignatureImage.Visible = false;
                this.lbSigDescription.Visible = true;
                this.lbSigDescription.Dock = DockStyle.Fill;
            }
            if (((ListControl)this.cbbSignerProfiles).SelectedIndex == -1)
                return;
            ((Control)this.btnSave).Enabled = true;
        }

        private void cbbSignerProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ListControl)this.cbbSignerProfiles).SelectedIndex == -1)
            {
                this.gbProfileDetails.Enabled = false;
            }
            else
            {
                this.gbProfileDetails.Enabled = true;
                if (((ComboBox)this.cbbSignerProfiles).SelectedItem is SignerProfile)
                {
                    SignerProfile selectedItem = ((ComboBox)this.cbbSignerProfiles).SelectedItem as SignerProfile;
                    this.txtSignerProfileName.Text = selectedItem.Name;
                    ((CheckBox)this.mcbShowEmail).Checked = selectedItem.ShowEmail;
                    ((CheckBox)this.mcbShowCQ1).Checked = selectedItem.ShowCQ1;
                    ((CheckBox)this.mcbShowCQ2).Checked = selectedItem.ShowCQ2;
                    ((CheckBox)this.mcbShowCQ3).Checked = selectedItem.ShowCQ3;
                    ((CheckBox)this.mcbShowTime).Checked = selectedItem.ShowDate;
                    ((CheckBox)this.mcbShowSigLabel).Checked = selectedItem.ShowLabel;
                    if (selectedItem.AppearanceMode == 0)
                        ((RadioButton)this.mrbTypeDescription).Checked = true;
                    else if (selectedItem.AppearanceMode == 1)
                        ((RadioButton)this.mrbTypeGraphy).Checked = true;
                    else if (selectedItem.AppearanceMode == 2)
                        ((RadioButton)this.mrbTypeGraphVsDes).Checked = true;
                    ((CheckBox)this.ckbOrgSigner).Checked = selectedItem.IsOrgProfile;
                    if (selectedItem.Image != null)
                    {
                        this.pcbSignatureImage.Image = (Image)selectedItem.Image;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(selectedItem.ImageBase64))
                        {
                            this.pcbSignatureImage.Image = SignerProfile.Base64ToBitmap(selectedItem.ImageBase64);
                        }
                    }
                    
                    this.MeasureSignatureText();
                    ((CheckBox)this.ckbSetProfileAsDefault).Checked = this.spStore.DefaultSigner == ((ListControl)this.cbbSignerProfiles).SelectedIndex;
                    ((Control)this.btnDelProfile).Enabled = true;
                }
                else
                {
                    this.ClearFields();
                    this.gbProfileDetails.Enabled = true;
                    ((Control)this.btnDelProfile).Enabled = false;
                }
            }
        }

        private void pcbSignatureImage_Click(object sender, EventArgs e) => this.SelectSigImage();

        private void tabCtrlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((TabControl)this.tabCtrlMain).SelectedTab.Name == "tabPagePdf")
            {
                ((Control)this.btnSave).Enabled = true;
                ((Control)this.btnBackup).Visible = true;
                ((Control)this.btnRestore).Visible = true;
                ((Control)this.btnDelProfile).Visible = true;
            }
            else
            {
                ((Control)this.btnSave).Enabled = this.lstChanged.Count > 0;
                ((Control)this.btnBackup).Visible = false;
                ((Control)this.btnRestore).Visible = false;
                ((Control)this.btnDelProfile).Visible = false;
            }
        }

        private void frmConfig_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
            this.LoadConfig();
            if (this.shownSigAppr)
                ((TabControl)this.tabCtrlMain).SelectTab("tabPagePdf");
            ((Form)this).TopMost = true;
            ((Form)this).TopMost = false;
        }

        private void txtSignerProfileName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!(((Control)this.cbbSignerProfiles).Text != this.txtSignerProfileName.Text))
                    return;
                ((Control)this.btnSave).Enabled = true;
            }
            catch
            {
            }
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;
            try
            {
                File.Copy(Path.Combine(Configuration.WORKING_PATH, "signerprofile.xml"), saveFileDialog.FileName, true);
                int num = (int)MessageBox.Show("Quá trình backup thành công.", "Backup signer profiles", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (PathTooLongException ex)
            {
                int num = (int)MessageBox.Show("Đường dẫn tệp backup quá dài.", "Backup signer profiles", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            catch (DirectoryNotFoundException ex)
            {
                int num = (int)MessageBox.Show("Không tìm thấy thư mục lưu tệp backup.", "Backup signer profiles", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            catch (FileNotFoundException ex)
            {
                int num = (int)MessageBox.Show("Không tìm thấy tệp lưu profiles ký.", "Backup signer profiles", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("Quá trình backup không thành công.", "Backup signer profiles", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            try
            {
                try
                {
                    SignerProfileStore signerProfileStore = new SignerProfileStore(openFileDialog.FileName);
                }
                catch
                {
                    throw new Exception("Không đúng định dạng tệp cấu hình profile ký.");
                }
                string str = Path.Combine(Configuration.WORKING_PATH, "signerprofile.xml");
                File.Copy(openFileDialog.FileName, str, true);
                int num1 = (int)MessageBox.Show("Quá trình khôi phục thành công.", "Restore signer profiles", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                try
                {
                    this.spStore = new SignerProfileStore(str);
                    this.ReloadSP();
                    ((ListControl)this.cbbSignerProfiles).SelectedIndex = this.spStore.DefaultSigner;
                }
                catch (Exception ex)
                {
                    int num2 = (int)MessageBox.Show("Không thể duyệt danh sách profiles ký.\r\nKhông đúng định dạng tệp cấu hình profile ký.", "Cấu hình hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Question);
                }
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("Quá trình khôi phục không thành công.\r\n- " + ex.Message, "Restore signer profiles", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
        }

        private void btnDelProfile_Click(object sender, EventArgs e) => this.DelProfile();

        private void changeImageToolStripMenuItem_Click(object sender, EventArgs e) => this.SelectSigImage();

        private void mrbTypeGraphVsDes_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)this.mrbTypeGraphVsDes).Checked)
                return;
            this.SigAppearanceChanged();
            this.MeasureSignatureText();
        }

        private void mrbTypeGraphy_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)this.mrbTypeGraphy).Checked)
                return;
            this.SigAppearanceChanged();
            this.MeasureSignatureText();
        }

        private void mrbTypeDescription_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)this.mrbTypeDescription).Checked)
                return;
            this.SigAppearanceChanged();
            this.MeasureSignatureText();
        }

        private void ckbSetProfileAsDefault_CheckedChanged(object sender, EventArgs e) => ((Control)this.btnSave).Enabled = true;

        private void mcbShowSigLabel_CheckedChanged(object sender, EventArgs e)
        {
            this.MeasureSignatureText();
            ((Control)this.btnSave).Enabled = true;
        }

        private void mcbShowEmail_CheckedChanged(object sender, EventArgs e)
        {
            this.MeasureSignatureText();
            ((Control)this.btnSave).Enabled = true;
        }

        private void mcbShowCQ1_CheckedChanged(object sender, EventArgs e)
        {
            this.MeasureSignatureText();
            ((Control)this.btnSave).Enabled = true;
        }

        private void mcbShowTime_CheckedChanged(object sender, EventArgs e)
        {
            this.MeasureSignatureText();
            ((Control)this.btnSave).Enabled = true;
        }

        private void mcbShowCQ3_CheckedChanged(object sender, EventArgs e)
        {
            this.MeasureSignatureText();
            ((Control)this.btnSave).Enabled = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ((Control)this.btnSave).Enabled = false;
            foreach (KeyValuePair<ConfigKey, object> keyValuePair in this.lstChanged)
                Configuration.SetConfigByKey(keyValuePair.Key, (object)keyValuePair.Value.ToString());
            this.lstChanged.Clear();
            if (((TabControl)this.tabCtrlMain).SelectedTab.Name != "tabPagePdf" || ((ComboBox)this.cbbSignerProfiles).SelectedItem == null)
                return;
            if (((ListControl)this.cbbSignerProfiles).SelectedIndex != ((ComboBox)this.cbbSignerProfiles).Items.Count - 1)
            {
                if (string.IsNullOrEmpty(this.txtSignerProfileName.Text.Trim()))
                {
                    int num1 = (int)MessageBox.Show("Chưa nhập tên mẫu chữ ký!", "Sửa đổi mẫu chữ ký", MessageBoxButtons.OK, MessageBoxIcon.Question);
                }
                else if (this.pcbSignatureImage.Image == null)
                {
                    int num2 = (int)MessageBox.Show("Chưa chọn hình ảnh chữ ký cho mẫu chữ ký!", "Sửa đổi mẫu chữ ký", MessageBoxButtons.OK, MessageBoxIcon.Question);
                }
                else
                {
                    int selectedIndex = ((ListControl)this.cbbSignerProfiles).SelectedIndex;
                    string tempFileName = Path.GetTempFileName();
                    try
                    {
                        this.pcbSignatureImage.Image.Save(tempFileName);
                        using (Bitmap img = new Bitmap(tempFileName))
                        {
                            SignerProfile signerProfile = new SignerProfile(this.txtSignerProfileName.Text, SignerProfile.BitmapToBase64(img));
                            if (((RadioButton)this.mrbTypeGraphVsDes).Checked)
                                signerProfile.AppearanceMode = 2;
                            else if (((RadioButton)this.mrbTypeGraphy).Checked)
                                signerProfile.AppearanceMode = 1;
                            else if (((RadioButton)this.mrbTypeDescription).Checked)
                                signerProfile.AppearanceMode = 0;
                            signerProfile.IsOrgProfile = ((CheckBox)this.ckbOrgSigner).Checked;
                            signerProfile.ShowDate = ((CheckBox)this.mcbShowTime).Checked;
                            signerProfile.ShowLabel = ((CheckBox)this.mcbShowSigLabel).Checked;
                            signerProfile.ShowEmail = ((CheckBox)this.mcbShowEmail).Checked;
                            signerProfile.ShowCQ1 = ((CheckBox)this.mcbShowCQ1).Checked;
                            signerProfile.ShowCQ2 = ((CheckBox)this.mcbShowCQ2).Checked;
                            signerProfile.ShowCQ3 = ((CheckBox)this.mcbShowCQ3).Checked;
                            this.spStore[selectedIndex] = signerProfile;
                            if (((CheckBox)this.ckbSetProfileAsDefault).Checked)
                                this.spStore.DefaultSigner = selectedIndex;
                            else if (this.spStore.DefaultSigner == selectedIndex)
                                this.spStore.DefaultSigner = -1;
                            this.ReloadSP();
                            ((ListControl)this.cbbSignerProfiles).SelectedIndex = selectedIndex;
                        }
                    }
                    finally
                    {
                        File.Delete(tempFileName);
                    }
                }
            }
            else if (string.IsNullOrEmpty(this.txtSignerProfileName.Text.Trim()))
            {
                int num3 = (int)MessageBox.Show("Chưa nhập tên mẫu chữ ký!", "Tạo mẫu chữ ký mới", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            else if (this.pcbSignatureImage.Image == null)
            {
                int num4 = (int)MessageBox.Show("Chưa chọn hình ảnh cho mẫu chữ ký!", "Tạo mẫu chữ ký mới", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            else
            {
                SignerProfile sp = new SignerProfile(this.txtSignerProfileName.Text.Trim(), SignerProfile.BitmapToBase64((Bitmap)this.pcbSignatureImage.Image));
                if (((RadioButton)this.mrbTypeGraphVsDes).Checked)
                    sp.AppearanceMode = 2;
                else if (((RadioButton)this.mrbTypeGraphy).Checked)
                    sp.AppearanceMode = 1;
                else if (((RadioButton)this.mrbTypeDescription).Checked)
                    sp.AppearanceMode = 0;
                sp.IsOrgProfile = ((CheckBox)this.ckbOrgSigner).Checked;
                sp.ShowDate = ((CheckBox)this.mcbShowTime).Checked;
                sp.ShowLabel = ((CheckBox)this.mcbShowSigLabel).Checked;
                sp.ShowEmail = ((CheckBox)this.mcbShowEmail).Checked;
                sp.ShowCQ1 = ((CheckBox)this.mcbShowCQ1).Checked;
                sp.ShowCQ2 = ((CheckBox)this.mcbShowCQ2).Checked;
                sp.ShowCQ3 = ((CheckBox)this.mcbShowCQ3).Checked;
                this.spStore.Add(sp);
                if (((CheckBox)this.ckbSetProfileAsDefault).Checked)
                    this.spStore.DefaultSigner = this.spStore.Count - 1;
                this.ReloadSP();
                ((ListControl)this.cbbSignerProfiles).SelectedIndex = this.spStore.Count - 1;
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => ((Form)this).Close();

        private void ckbAutoUpdate_CheckedChanged(object sender, EventArgs e) => this.AddValueChanged(ConfigKey.UpdateOnStart, (object)((CheckBox)this.ckbAutoUpdate).Checked);

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG (*.png)|*.png|Bitmap (*.bmp)|*.bmp|Jpeg (*.jpg, *.jpeg)|*.jpg;*.jpeg|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;
            try
            {
                if (saveFileDialog.FilterIndex == 1)
                    this.pcbSignatureImage.Image.Save(saveFileDialog.FileName, ImageFormat.Png);
                else if (saveFileDialog.FilterIndex == 2)
                    this.pcbSignatureImage.Image.Save(saveFileDialog.FileName, ImageFormat.Bmp);
                else if (saveFileDialog.FilterIndex == 3)
                    this.pcbSignatureImage.Image.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
                else
                    this.pcbSignatureImage.Image.Save(saveFileDialog.FileName);
                int num = (int)MessageBox.Show("Hình ảnh chữ ký đã được lưu.", "Lưu ảnh chữ ký", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("Lỗi quá trình lưu ảnh:\r\n" + ex.Message, "Lưu ảnh chữ ký", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void lbSigDescription_Click(object sender, EventArgs e)
        {
        }

        private void setDefaultImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.pcbSignatureImage.Image.Dispose();
            this.pcbSignatureImage.Image = (Image)Resources.HiPT;
            this.pcbSignatureImage.Refresh();
            ((Control)this.btnSave).Enabled = true;
        }

        private void btnAddCrlLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            InputForm inputForm = new InputForm();
            if (inputForm.ShowDialog() != DialogResult.OK)
                return;
            string message = inputForm.Message;
            Uri result;
            if ((!Uri.TryCreate(message, UriKind.Absolute, out result) ? 0 : (result.Scheme == Uri.UriSchemeHttp ? 1 : 0)) == 0)
            {
                int num = (int)MessageBox.Show("Đường dẫn danh sách thu hồi không hợp lệ!", "Thêm danh sách thu hồi", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            else
            {
                if (this.lstAdditionalCrls.Items.Contains((object)message))
                    return;
                this.lstAdditionalCrls.Items.Add((object)message);
                string str = "";
                for (int index = 0; index < this.lstAdditionalCrls.Items.Count; ++index)
                    str = str + this.lstAdditionalCrls.Items[index] + "|";
                this.AddValueChanged(ConfigKey.AdditionalCrls, (object)str);
            }
        }

        private void btnDelCrlLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.lstAdditionalCrls.SelectedIndex == -1 || MessageBox.Show("Bạn có chắc chắn muốn xóa đường dẫn danh sách thu hồi:\r\n" + this.lstAdditionalCrls.SelectedItem.ToString(), "Xóa đường dẫn danh sách thu hồi", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                return;
            this.lstAdditionalCrls.Items.RemoveAt(this.lstAdditionalCrls.SelectedIndex);
            string str = "";
            for (int index = 0; index < this.lstAdditionalCrls.Items.Count; ++index)
                str = str + this.lstAdditionalCrls.Items[index] + "|";
            this.AddValueChanged(ConfigKey.AdditionalCrls, (object)str);
        }

        private void ckbCheckingSignerCertViaOcsp_CheckedChanged(object sender, EventArgs e) => this.AddValueChanged(ConfigKey.AllowedOCSPForCheckingSigningCert, (object)((CheckBox)this.ckbCheckingSignerCertViaOcsp).Checked);

        private void ckbAllowCertCheckingOnline_CheckedChanged(object sender, EventArgs e)
        {
            this.AddValueChanged(ConfigKey.AllowedOnlineCheckingCert, (object)((CheckBox)this.ckbAllowCertCheckingOnline).Checked);
            this.gbOnlineCertChecking.Enabled = ((CheckBox)this.ckbAllowCertCheckingOnline).Checked;
        }

        private void ckbOrgSigner_CheckedChanged(object sender, EventArgs e)
        {
            this.SigAppearanceChanged();
            if (((ListControl)this.cbbSignerProfiles).SelectedIndex == ((ComboBox)this.cbbSignerProfiles).Items.Count - 1)
            {
                ((CheckBox)this.mcbShowCQ1).Checked = !((CheckBox)this.ckbOrgSigner).Checked;
                ((CheckBox)this.mcbShowCQ2).Checked = !((CheckBox)this.ckbOrgSigner).Checked;
                ((CheckBox)this.mcbShowCQ3).Checked = !((CheckBox)this.ckbOrgSigner).Checked;
            }
            this.MeasureSignatureText();
            ((Control)this.btnSave).Enabled = true;
        }

        private void mcbShowCQ2_CheckedChanged(object sender, EventArgs e)
        {
            this.MeasureSignatureText();
            ((Control)this.btnSave).Enabled = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = (IContainer)new Container();
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(frmConfig));
            this.tabCtrlMain = new MetroTabControl();
            this.tabPageConnection = new TabPage();
            this.ckbBypassLocal = new MetroCheckBox();
            this.lbPxPassword = new MetroLabel();
            this.txtPxPassword = new TextBox();
            this.txtPxUsername = new TextBox();
            this.lbPxUsername = new MetroLabel();
            this.ckbUsePxAuth = new MetroCheckBox();
            this.nPxPort = new NumericUpDown();
            this.txtPxAddress = new TextBox();
            this.lbPxPort = new MetroLabel();
            this.lbPxAddress = new MetroLabel();
            this.rdbUseSpecifiedPx = new MetroRadioButton();
            this.rdbAutoDetectPx = new MetroRadioButton();
            this.ckbUsePx = new MetroCheckBox();
            this.tabPagePdf = new TabPage();
            this.gbProfileDetails = new GroupBox();
            this.label1 = new Label();
            this.groupBox1 = new GroupBox();
            this.ckbOrgSigner = new MetroCheckBox();
            this.pnlSigAppearance = new MetroPanel();
            this.pcbSignatureImage = new PictureBox();
            this.CtxtMnTripImage = new ContextMenuStrip(this.components);
            this.setDefaultImageToolStripMenuItem = new ToolStripMenuItem();
            this.changeImageToolStripMenuItem = new ToolStripMenuItem();
            this.saveImageToolStripMenuItem = new ToolStripMenuItem();
            this.lbSigDescription = new Label();
            this.mrbTypeDescription = new MetroRadioButton();
            this.mrbTypeGraphVsDes = new MetroRadioButton();
            this.mrbTypeGraphy = new MetroRadioButton();
            this.mcbShowSigLabel = new MetroCheckBox();
            this.mcbShowTime = new MetroCheckBox();
            this.mcbShowCQ3 = new MetroCheckBox();
            this.mcbShowCQ2 = new MetroCheckBox();
            this.mcbShowCQ1 = new MetroCheckBox();
            this.mcbShowEmail = new MetroCheckBox();
            this.ckbSetProfileAsDefault = new MetroCheckBox();
            this.txtSignerProfileName = new TextBox();
            this.metroLabel6 = new MetroLabel();
            this.cbbSignerProfiles = new MetroComboBox();
            this.tabPagePKIServices = new TabPage();
            this.gbOnlineCertChecking = new GroupBox();
            this.btnDelCrlLink = new LinkLabel();
            this.btnAddCrlLink = new LinkLabel();
            this.lstAdditionalCrls = new ListBox();
            this.label2 = new Label();
            this.ckbCheckingSignerCertViaOcsp = new MetroCheckBox();
            this.ckbAllowCertCheckingOnline = new MetroCheckBox();
            this.gbTSA = new GroupBox();
            this.metroLabel5 = new MetroLabel();
            this.txtTsaAddress = new TextBox();
            this.ckbUseTsa = new MetroCheckBox();
            this.tabPageUpdate = new TabPage();
            this.panel1 = new Panel();
            this.mLabelCurrVersion = new MetroLabel();
            this.mLabelLastCheckingUpdate = new MetroLabel();
            this.metroLabel2 = new MetroLabel();
            this.metroLabel1 = new MetroLabel();
            this.ckbAutoUpdate = new MetroCheckBox();
            this.btnClose = new MetroButton();
            this.btnSave = new MetroButton();
            this.btnRestore = new MetroButton();
            this.btnBackup = new MetroButton();
            this.btnDelProfile = new MetroButton();
            ((Control)this.tabCtrlMain).SuspendLayout();
            this.tabPageConnection.SuspendLayout();
            this.nPxPort.BeginInit();
            this.tabPagePdf.SuspendLayout();
            this.gbProfileDetails.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((Control)this.pnlSigAppearance).SuspendLayout();
            ((ISupportInitialize)this.pcbSignatureImage).BeginInit();
            this.CtxtMnTripImage.SuspendLayout();
            this.tabPagePKIServices.SuspendLayout();
            this.gbOnlineCertChecking.SuspendLayout();
            this.gbTSA.SuspendLayout();
            this.tabPageUpdate.SuspendLayout();
            this.panel1.SuspendLayout();
            ((Control)this).SuspendLayout();
            ((Control)this.tabCtrlMain).Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ((Control)this.tabCtrlMain).Controls.Add((Control)this.tabPageConnection);
            ((Control)this.tabCtrlMain).Controls.Add((Control)this.tabPagePdf);
            ((Control)this.tabCtrlMain).Controls.Add((Control)this.tabPagePKIServices);
            ((Control)this.tabCtrlMain).Controls.Add((Control)this.tabPageUpdate);
            ((Control)this.tabCtrlMain).Location = new Point(20, 60);
            ((Control)this.tabCtrlMain).Name = "tabCtrlMain";
            ((TabControl)this.tabCtrlMain).SelectedIndex = 2;
            ((Control)this.tabCtrlMain).Size = new Size(594, 373);
            ((Control)this.tabCtrlMain).TabIndex = 0;
            this.tabCtrlMain.UseSelectable = true;
            ((TabControl)this.tabCtrlMain).SelectedIndexChanged += new EventHandler(this.tabCtrlMain_SelectedIndexChanged);
            this.tabPageConnection.BackColor = SystemColors.ControlLightLight;
            this.tabPageConnection.Controls.Add((Control)this.ckbBypassLocal);
            this.tabPageConnection.Controls.Add((Control)this.lbPxPassword);
            this.tabPageConnection.Controls.Add((Control)this.txtPxPassword);
            this.tabPageConnection.Controls.Add((Control)this.txtPxUsername);
            this.tabPageConnection.Controls.Add((Control)this.lbPxUsername);
            this.tabPageConnection.Controls.Add((Control)this.ckbUsePxAuth);
            this.tabPageConnection.Controls.Add((Control)this.nPxPort);
            this.tabPageConnection.Controls.Add((Control)this.txtPxAddress);
            this.tabPageConnection.Controls.Add((Control)this.lbPxPort);
            this.tabPageConnection.Controls.Add((Control)this.lbPxAddress);
            this.tabPageConnection.Controls.Add((Control)this.rdbUseSpecifiedPx);
            this.tabPageConnection.Controls.Add((Control)this.rdbAutoDetectPx);
            this.tabPageConnection.Controls.Add((Control)this.ckbUsePx);
            this.tabPageConnection.Location = new Point(4, 35);
            this.tabPageConnection.Name = "tabPageConnection";
            this.tabPageConnection.Size = new Size(586, 334);
            this.tabPageConnection.TabIndex = 0;
            this.tabPageConnection.Text = "Kết nối mạng";
            ((Control)this.ckbBypassLocal).AutoSize = true;
            ((CheckBox)this.ckbBypassLocal).Checked = true;
            ((CheckBox)this.ckbBypassLocal).CheckState = CheckState.Checked;
            ((Control)this.ckbBypassLocal).Location = new Point(35, 238);
            ((Control)this.ckbBypassLocal).Name = "ckbBypassLocal";
            ((Control)this.ckbBypassLocal).Size = new Size(290, 15);
            ((Control)this.ckbBypassLocal).TabIndex = 13;
            ((Control)this.ckbBypassLocal).Text = "Không sử dụng cấu hình proxy trong mạng cục bộ";
            this.ckbBypassLocal.UseSelectable = true;
            ((CheckBox)this.ckbBypassLocal).CheckedChanged += new EventHandler(this.ckbBypassLocal_CheckedChanged);
            ((Control)this.lbPxPassword).AutoSize = true;
            ((Control)this.lbPxPassword).Enabled = false;
            ((Control)this.lbPxPassword).Location = new Point(379, 179);
            ((Control)this.lbPxPassword).Name = "lbPxPassword";
            ((Control)this.lbPxPassword).Size = new Size(66, 19);
            ((Control)this.lbPxPassword).TabIndex = 12;
            ((Control)this.lbPxPassword).Text = "Mật khẩu:";
            this.txtPxPassword.Enabled = false;
            this.txtPxPassword.Location = new Point(379, 200);
            this.txtPxPassword.Name = "txtPxPassword";
            this.txtPxPassword.PasswordChar = '*';
            this.txtPxPassword.Size = new Size(162, 20);
            this.txtPxPassword.TabIndex = 11;
            this.txtPxPassword.UseSystemPasswordChar = true;
            this.txtPxPassword.TextChanged += new EventHandler(this.txtPxPassword_TextChanged);
            this.txtPxUsername.Enabled = false;
            this.txtPxUsername.Location = new Point(66, 201);
            this.txtPxUsername.Name = "txtPxUsername";
            this.txtPxUsername.Size = new Size(287, 20);
            this.txtPxUsername.TabIndex = 10;
            this.txtPxUsername.TextChanged += new EventHandler(this.txtPxUsername_TextChanged);
            ((Control)this.lbPxUsername).AutoSize = true;
            ((Control)this.lbPxUsername).Enabled = false;
            ((Control)this.lbPxUsername).Location = new Point(63, 179);
            ((Control)this.lbPxUsername).Name = "lbPxUsername";
            ((Control)this.lbPxUsername).Size = new Size(103, 19);
            ((Control)this.lbPxUsername).TabIndex = 9;
            ((Control)this.lbPxUsername).Text = "Tên người dùng:";
            ((Control)this.ckbUsePxAuth).AutoSize = true;
            ((Control)this.ckbUsePxAuth).Location = new Point(34, 157);
            ((Control)this.ckbUsePxAuth).Name = "ckbUsePxAuth";
            ((Control)this.ckbUsePxAuth).Size = new Size(164, 15);
            ((Control)this.ckbUsePxAuth).TabIndex = 8;
            ((Control)this.ckbUsePxAuth).Text = "Máy chủ proxy có xác thực";
            this.ckbUsePxAuth.UseSelectable = true;
            ((CheckBox)this.ckbUsePxAuth).CheckedChanged += new EventHandler(this.ckbUsePxAuth_CheckedChanged);
            this.nPxPort.Enabled = false;
            this.nPxPort.Location = new Point(378, 125);
            this.nPxPort.Maximum = new Decimal(new int[4]
            {
        (int) ushort.MaxValue,
        0,
        0,
        0
            });
            this.nPxPort.Minimum = new Decimal(new int[4]
            {
        1,
        0,
        0,
        0
            });
            this.nPxPort.Name = "nPxPort";
            this.nPxPort.Size = new Size(101, 20);
            this.nPxPort.TabIndex = 7;
            this.nPxPort.TextAlign = HorizontalAlignment.Right;
            this.nPxPort.Value = new Decimal(new int[4]
            {
        80,
        0,
        0,
        0
            });
            this.nPxPort.ValueChanged += new EventHandler(this.nPxPort_ValueChanged);
            this.txtPxAddress.Enabled = false;
            this.txtPxAddress.Location = new Point(66, 125);
            this.txtPxAddress.Name = "txtPxAddress";
            this.txtPxAddress.Size = new Size(287, 20);
            this.txtPxAddress.TabIndex = 6;
            this.txtPxAddress.TextChanged += new EventHandler(this.txtPxAddress_TextChanged);
            ((Control)this.lbPxPort).AutoSize = true;
            ((Control)this.lbPxPort).Enabled = false;
            ((Control)this.lbPxPort).Location = new Point(375, 105);
            ((Control)this.lbPxPort).Name = "lbPxPort";
            ((Control)this.lbPxPort).Size = new Size(44, 19);
            ((Control)this.lbPxPort).TabIndex = 5;
            ((Control)this.lbPxPort).Text = "Cổng:";
            ((Control)this.lbPxAddress).AutoSize = true;
            ((Control)this.lbPxAddress).Enabled = false;
            ((Control)this.lbPxAddress).Location = new Point(65, 105);
            ((Control)this.lbPxAddress).Name = "lbPxAddress";
            ((Control)this.lbPxAddress).Size = new Size(51, 19);
            ((Control)this.lbPxAddress).TabIndex = 4;
            ((Control)this.lbPxAddress).Text = "Địa chỉ:";
            ((Control)this.rdbUseSpecifiedPx).AutoSize = true;
            ((Control)this.rdbUseSpecifiedPx).Location = new Point(35, 83);
            ((Control)this.rdbUseSpecifiedPx).Name = "rdbUseSpecifiedPx";
            ((Control)this.rdbUseSpecifiedPx).Size = new Size(178, 15);
            ((Control)this.rdbUseSpecifiedPx).TabIndex = 2;
            ((Control)this.rdbUseSpecifiedPx).Text = "Sử dụng cấu hình proxy riêng";
            this.rdbUseSpecifiedPx.UseSelectable = true;
            ((RadioButton)this.rdbUseSpecifiedPx).CheckedChanged += new EventHandler(this.rdbUseSpecifiedPx_CheckedChanged);
            ((Control)this.rdbAutoDetectPx).AutoSize = true;
            ((RadioButton)this.rdbAutoDetectPx).Checked = true;
            ((Control)this.rdbAutoDetectPx).Location = new Point(35, 57);
            ((Control)this.rdbAutoDetectPx).Name = "rdbAutoDetectPx";
            ((Control)this.rdbAutoDetectPx).Size = new Size(201, 15);
            ((Control)this.rdbAutoDetectPx).TabIndex = 1;
            ((RadioButton)this.rdbAutoDetectPx).TabStop = true;
            ((Control)this.rdbAutoDetectPx).Text = "Sử dụng cấu hình proxy mặc định";
            this.rdbAutoDetectPx.UseSelectable = true;
            ((RadioButton)this.rdbAutoDetectPx).CheckedChanged += new EventHandler(this.rdbAutoDetectPx_CheckedChanged);
            ((Control)this.ckbUsePx).AutoSize = true;
            ((Control)this.ckbUsePx).Location = new Point(34, 26);
            ((Control)this.ckbUsePx).Name = "ckbUsePx";
            ((Control)this.ckbUsePx).Size = new Size(148, 15);
            ((Control)this.ckbUsePx).TabIndex = 0;
            ((Control)this.ckbUsePx).Text = "Sử dụng máy chủ proxy";
            this.ckbUsePx.UseSelectable = true;
            ((CheckBox)this.ckbUsePx).CheckedChanged += new EventHandler(this.ckbUsePx_CheckedChanged);
            this.tabPagePdf.BackColor = SystemColors.ControlLightLight;
            this.tabPagePdf.Controls.Add((Control)this.gbProfileDetails);
            this.tabPagePdf.Controls.Add((Control)this.metroLabel6);
            this.tabPagePdf.Controls.Add((Control)this.cbbSignerProfiles);
            this.tabPagePdf.Location = new Point(4, 35);
            this.tabPagePdf.Name = "tabPagePdf";
            this.tabPagePdf.Size = new Size(586, 334);
            this.tabPagePdf.TabIndex = 3;
            this.tabPagePdf.Text = "Mẫu chữ ký";
            this.gbProfileDetails.Controls.Add((Control)this.label1);
            this.gbProfileDetails.Controls.Add((Control)this.groupBox1);
            this.gbProfileDetails.Controls.Add((Control)this.ckbSetProfileAsDefault);
            this.gbProfileDetails.Controls.Add((Control)this.txtSignerProfileName);
            this.gbProfileDetails.Location = new Point(3, 40);
            this.gbProfileDetails.Name = "gbProfileDetails";
            this.gbProfileDetails.Size = new Size(580, 291);
            this.gbProfileDetails.TabIndex = 2;
            this.gbProfileDetails.TabStop = false;
            this.label1.AutoSize = true;
            this.label1.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label1.Location = new Point(18, 14);
            this.label1.Name = "label1";
            this.label1.Size = new Size(105, 16);
            this.label1.TabIndex = 17;
            this.label1.Text = "Tên mẫu chữ ký:";
            this.groupBox1.Controls.Add((Control)this.ckbOrgSigner);
            this.groupBox1.Controls.Add((Control)this.pnlSigAppearance);
            this.groupBox1.Controls.Add((Control)this.mrbTypeDescription);
            this.groupBox1.Controls.Add((Control)this.mrbTypeGraphVsDes);
            this.groupBox1.Controls.Add((Control)this.mrbTypeGraphy);
            this.groupBox1.Controls.Add((Control)this.mcbShowSigLabel);
            this.groupBox1.Controls.Add((Control)this.mcbShowTime);
            this.groupBox1.Controls.Add((Control)this.mcbShowCQ3);
            this.groupBox1.Controls.Add((Control)this.mcbShowCQ2);
            this.groupBox1.Controls.Add((Control)this.mcbShowCQ1);
            this.groupBox1.Controls.Add((Control)this.mcbShowEmail);
            this.groupBox1.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.groupBox1.Location = new Point(11, 39);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(563, 217);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Hiển thị chữ ký:";
            this.groupBox1.UseCompatibleTextRendering = true;
            ((Control)this.ckbOrgSigner).AutoSize = true;
            ((Control)this.ckbOrgSigner).Location = new Point(27, 26);
            ((Control)this.ckbOrgSigner).Name = "ckbOrgSigner";
            ((Control)this.ckbOrgSigner).Size = new Size(131, 15);
            ((Control)this.ckbOrgSigner).TabIndex = 19;
            ((Control)this.ckbOrgSigner).Text = "Mẫu chữ ký Tổ chức";
            this.ckbOrgSigner.UseSelectable = true;
            ((CheckBox)this.ckbOrgSigner).CheckedChanged += new EventHandler(this.ckbOrgSigner_CheckedChanged);
            ((Panel)this.pnlSigAppearance).BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            ((Control)this.pnlSigAppearance).Controls.Add((Control)this.pcbSignatureImage);
            ((Control)this.pnlSigAppearance).Controls.Add((Control)this.lbSigDescription);
            this.pnlSigAppearance.HorizontalScrollbarBarColor = true;
            this.pnlSigAppearance.HorizontalScrollbarHighlightOnWheel = false;
            this.pnlSigAppearance.HorizontalScrollbarSize = 10;
            ((Control)this.pnlSigAppearance).Location = new Point(185, 91);
            ((Control)this.pnlSigAppearance).Name = "pnlSigAppearance";
            ((Control)this.pnlSigAppearance).Size = new Size(360, 103);
            ((Control)this.pnlSigAppearance).TabIndex = 18;
            this.pnlSigAppearance.VerticalScrollbarBarColor = true;
            this.pnlSigAppearance.VerticalScrollbarHighlightOnWheel = false;
            this.pnlSigAppearance.VerticalScrollbarSize = 10;
            this.pcbSignatureImage.BackgroundImageLayout = ImageLayout.Zoom;
            this.pcbSignatureImage.ContextMenuStrip = this.CtxtMnTripImage;
            this.pcbSignatureImage.Cursor = Cursors.Hand;
            this.pcbSignatureImage.Image = (Image)Resources.HiPT;
            this.pcbSignatureImage.InitialImage = (Image)Resources.HiPT;
            this.pcbSignatureImage.Location = new Point(0, 0);
            this.pcbSignatureImage.Name = "pcbSignatureImage";
            this.pcbSignatureImage.Size = new Size(166, 101);
            this.pcbSignatureImage.SizeMode = PictureBoxSizeMode.Zoom;
            this.pcbSignatureImage.TabIndex = 16;
            this.pcbSignatureImage.TabStop = false;
            this.pcbSignatureImage.Click += new EventHandler(this.pcbSignatureImage_Click);
            this.CtxtMnTripImage.Items.AddRange(new ToolStripItem[3]
            {
        (ToolStripItem) this.setDefaultImageToolStripMenuItem,
        (ToolStripItem) this.changeImageToolStripMenuItem,
        (ToolStripItem) this.saveImageToolStripMenuItem
            });
            this.CtxtMnTripImage.Name = "contextMenuStrip1";
            this.CtxtMnTripImage.Size = new Size(195, 70);
            this.setDefaultImageToolStripMenuItem.Name = "setDefaultImageToolStripMenuItem";
            this.setDefaultImageToolStripMenuItem.Size = new Size(194, 22);
            this.setDefaultImageToolStripMenuItem.Text = "Sử dụng ảnh mặc định";
            this.setDefaultImageToolStripMenuItem.Click += new EventHandler(this.setDefaultImageToolStripMenuItem_Click);
            this.changeImageToolStripMenuItem.Name = "changeImageToolStripMenuItem";
            this.changeImageToolStripMenuItem.Size = new Size(194, 22);
            this.changeImageToolStripMenuItem.Text = "Thay ảnh khác";
            this.changeImageToolStripMenuItem.Click += new EventHandler(this.changeImageToolStripMenuItem_Click);
            this.saveImageToolStripMenuItem.Name = "saveImageToolStripMenuItem";
            this.saveImageToolStripMenuItem.Size = new Size(194, 22);
            this.saveImageToolStripMenuItem.Text = "Lưu ảnh";
            this.saveImageToolStripMenuItem.Click += new EventHandler(this.saveImageToolStripMenuItem_Click);
            this.lbSigDescription.Cursor = Cursors.Hand;
            this.lbSigDescription.Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.lbSigDescription.Location = new Point(166, 0);
            this.lbSigDescription.Name = "lbSigDescription";
            this.lbSigDescription.Size = new Size(189, 100);
            this.lbSigDescription.TabIndex = 17;
            this.lbSigDescription.Text = "Người ký: Tên chứng thư số ký\r\nEmail: Địa chỉ thư điện tử\r\nCơ quan: CQ cấp 3, CQ cấp 2, CQ cấp 1\r\nThời gian ký: 29.05.2013 09:37:36 +07:00\r\n";
            this.lbSigDescription.TextAlign = ContentAlignment.MiddleLeft;
            this.lbSigDescription.Click += new EventHandler(this.lbSigDescription_Click);
            ((Control)this.mrbTypeDescription).AutoSize = true;
            ((Control)this.mrbTypeDescription).Location = new Point(474, 28);
            ((Control)this.mrbTypeDescription).Name = "mrbTypeDescription";
            ((Control)this.mrbTypeDescription).Size = new Size(75, 15);
            ((Control)this.mrbTypeDescription).TabIndex = 15;
            ((Control)this.mrbTypeDescription).Text = "Thông tin";
            this.mrbTypeDescription.UseSelectable = true;
            ((RadioButton)this.mrbTypeDescription).CheckedChanged += new EventHandler(this.mrbTypeDescription_CheckedChanged);
            ((Control)this.mrbTypeGraphVsDes).AutoSize = true;
            ((RadioButton)this.mrbTypeGraphVsDes).Checked = true;
            ((Control)this.mrbTypeGraphVsDes).Location = new Point(198, 28);
            ((Control)this.mrbTypeGraphVsDes).Name = "mrbTypeGraphVsDes";
            ((Control)this.mrbTypeGraphVsDes).Size = new Size(140, 15);
            ((Control)this.mrbTypeGraphVsDes).TabIndex = 14;
            ((RadioButton)this.mrbTypeGraphVsDes).TabStop = true;
            ((Control)this.mrbTypeGraphVsDes).Text = "Hình ảnh && Thông tin";
            this.mrbTypeGraphVsDes.UseSelectable = true;
            ((RadioButton)this.mrbTypeGraphVsDes).CheckedChanged += new EventHandler(this.mrbTypeGraphVsDes_CheckedChanged);
            ((Control)this.mrbTypeGraphy).AutoSize = true;
            ((Control)this.mrbTypeGraphy).Location = new Point(364, 28);
            ((Control)this.mrbTypeGraphy).Name = "mrbTypeGraphy";
            ((Control)this.mrbTypeGraphy).Size = new Size(72, 15);
            ((Control)this.mrbTypeGraphy).TabIndex = 13;
            ((Control)this.mrbTypeGraphy).Text = "Hình ảnh";
            this.mrbTypeGraphy.UseSelectable = true;
            ((RadioButton)this.mrbTypeGraphy).CheckedChanged += new EventHandler(this.mrbTypeGraphy_CheckedChanged);
            ((Control)this.mcbShowSigLabel).AutoSize = true;
            ((CheckBox)this.mcbShowSigLabel).Checked = true;
            ((CheckBox)this.mcbShowSigLabel).CheckState = CheckState.Checked;
            ((Control)this.mcbShowSigLabel).Location = new Point(27, 57);
            ((Control)this.mcbShowSigLabel).Name = "mcbShowSigLabel";
            ((Control)this.mcbShowSigLabel).Size = new Size(52, 15);
            ((Control)this.mcbShowSigLabel).TabIndex = 12;
            ((Control)this.mcbShowSigLabel).Text = "Nhãn";
            this.mcbShowSigLabel.UseSelectable = true;
            ((CheckBox)this.mcbShowSigLabel).CheckedChanged += new EventHandler(this.mcbShowSigLabel_CheckedChanged);
            ((Control)this.mcbShowTime).AutoSize = true;
            ((CheckBox)this.mcbShowTime).Checked = true;
            ((CheckBox)this.mcbShowTime).CheckState = CheckState.Checked;
            ((Control)this.mcbShowTime).Location = new Point(27, 183);
            ((Control)this.mcbShowTime).Name = "mcbShowTime";
            ((Control)this.mcbShowTime).Size = new Size(88, 15);
            ((Control)this.mcbShowTime).TabIndex = 12;
            ((Control)this.mcbShowTime).Text = "Thời gian ký";
            this.mcbShowTime.UseSelectable = true;
            ((CheckBox)this.mcbShowTime).CheckedChanged += new EventHandler(this.mcbShowTime_CheckedChanged);
            ((Control)this.mcbShowCQ3).AutoSize = true;
            ((CheckBox)this.mcbShowCQ3).Checked = true;
            ((CheckBox)this.mcbShowCQ3).CheckState = CheckState.Checked;
            ((Control)this.mcbShowCQ3).Location = new Point(27, 158);
            ((Control)this.mcbShowCQ3).Name = "mcbShowCQ3";
            ((Control)this.mcbShowCQ3).Size = new Size(102, 15);
            ((Control)this.mcbShowCQ3).TabIndex = 12;
            ((Control)this.mcbShowCQ3).Text = "Cơ quan cấp III";
            this.mcbShowCQ3.UseSelectable = true;
            ((CheckBox)this.mcbShowCQ3).CheckedChanged += new EventHandler(this.mcbShowCQ3_CheckedChanged);
            ((Control)this.mcbShowCQ2).AutoSize = true;
            ((CheckBox)this.mcbShowCQ2).Checked = true;
            ((CheckBox)this.mcbShowCQ2).CheckState = CheckState.Checked;
            ((Control)this.mcbShowCQ2).Location = new Point(27, 133);
            ((Control)this.mcbShowCQ2).Name = "mcbShowCQ2";
            ((Control)this.mcbShowCQ2).Size = new Size(99, 15);
            ((Control)this.mcbShowCQ2).TabIndex = 12;
            ((Control)this.mcbShowCQ2).Text = "Cơ quan cấp II";
            this.mcbShowCQ2.UseSelectable = true;
            ((CheckBox)this.mcbShowCQ2).CheckedChanged += new EventHandler(this.mcbShowCQ2_CheckedChanged);
            ((Control)this.mcbShowCQ1).AutoSize = true;
            ((CheckBox)this.mcbShowCQ1).Checked = true;
            ((CheckBox)this.mcbShowCQ1).CheckState = CheckState.Checked;
            ((Control)this.mcbShowCQ1).Location = new Point(27, 107);
            ((Control)this.mcbShowCQ1).Name = "mcbShowCQ1";
            ((Control)this.mcbShowCQ1).Size = new Size(96, 15);
            ((Control)this.mcbShowCQ1).TabIndex = 12;
            ((Control)this.mcbShowCQ1).Text = "Cơ quan cấp I";
            this.mcbShowCQ1.UseSelectable = true;
            ((CheckBox)this.mcbShowCQ1).CheckedChanged += new EventHandler(this.mcbShowCQ1_CheckedChanged);
            ((Control)this.mcbShowEmail).AutoSize = true;
            ((CheckBox)this.mcbShowEmail).Checked = true;
            ((CheckBox)this.mcbShowEmail).CheckState = CheckState.Checked;
            ((Control)this.mcbShowEmail).Location = new Point(27, 82);
            ((Control)this.mcbShowEmail).Name = "mcbShowEmail";
            ((Control)this.mcbShowEmail).Size = new Size(52, 15);
            ((Control)this.mcbShowEmail).TabIndex = 12;
            ((Control)this.mcbShowEmail).Text = "Email";
            this.mcbShowEmail.UseSelectable = true;
            ((CheckBox)this.mcbShowEmail).CheckedChanged += new EventHandler(this.mcbShowEmail_CheckedChanged);
            ((Control)this.ckbSetProfileAsDefault).AutoSize = true;
            this.ckbSetProfileAsDefault.FontWeight = (MetroCheckBoxWeight)2;
            ((Control)this.ckbSetProfileAsDefault).Location = new Point(11, 262);
            ((Control)this.ckbSetProfileAsDefault).Name = "ckbSetProfileAsDefault";
            ((Control)this.ckbSetProfileAsDefault).Size = new Size(141, 15);
            ((Control)this.ckbSetProfileAsDefault).TabIndex = 12;
            ((Control)this.ckbSetProfileAsDefault).Text = "Mẫu chữ ký mặc định";
            this.ckbSetProfileAsDefault.UseSelectable = true;
            ((CheckBox)this.ckbSetProfileAsDefault).CheckedChanged += new EventHandler(this.ckbSetProfileAsDefault_CheckedChanged);
            this.txtSignerProfileName.Location = new Point(207, 13);
            this.txtSignerProfileName.Name = "txtSignerProfileName";
            this.txtSignerProfileName.Size = new Size(366, 20);
            this.txtSignerProfileName.TabIndex = 1;
            this.txtSignerProfileName.WordWrap = false;
            this.txtSignerProfileName.TextChanged += new EventHandler(this.txtSignerProfileName_TextChanged);
            ((Control)this.metroLabel6).AutoSize = true;
            this.metroLabel6.FontSize = (MetroLabelSize)2;
            ((Control)this.metroLabel6).Location = new Point(6, 10);
            ((Control)this.metroLabel6).Name = "metroLabel6";
            ((Control)this.metroLabel6).Size = new Size(165, 25);
            ((Control)this.metroLabel6).TabIndex = 1;
            ((Control)this.metroLabel6).Text = "Quản lý mẫu chữ ký";
            ((ComboBox)this.cbbSignerProfiles).FlatStyle = FlatStyle.Flat;
            this.cbbSignerProfiles.FontSize = (MetroComboBoxSize)0;
            ((ListControl)this.cbbSignerProfiles).FormattingEnabled = true;
            ((ComboBox)this.cbbSignerProfiles).ItemHeight = 19;
            ((Control)this.cbbSignerProfiles).Location = new Point(319, 15);
            ((Control)this.cbbSignerProfiles).Name = "cbbSignerProfiles";
            ((Control)this.cbbSignerProfiles).Size = new Size(261, 25);
            ((Control)this.cbbSignerProfiles).TabIndex = 0;
            this.cbbSignerProfiles.UseSelectable = true;
            ((ComboBox)this.cbbSignerProfiles).SelectedIndexChanged += new EventHandler(this.cbbSignerProfiles_SelectedIndexChanged);
            this.tabPagePKIServices.BackColor = SystemColors.ControlLightLight;
            this.tabPagePKIServices.Controls.Add((Control)this.gbOnlineCertChecking);
            this.tabPagePKIServices.Controls.Add((Control)this.ckbAllowCertCheckingOnline);
            this.tabPagePKIServices.Controls.Add((Control)this.gbTSA);
            this.tabPagePKIServices.Controls.Add((Control)this.ckbUseTsa);
            this.tabPagePKIServices.Location = new Point(4, 35);
            this.tabPagePKIServices.Name = "tabPagePKIServices";
            this.tabPagePKIServices.Size = new Size(586, 334);
            this.tabPagePKIServices.TabIndex = 1;
            this.tabPagePKIServices.Text = "Dịch vụ chứng thực";
            this.gbOnlineCertChecking.Controls.Add((Control)this.btnDelCrlLink);
            this.gbOnlineCertChecking.Controls.Add((Control)this.btnAddCrlLink);
            this.gbOnlineCertChecking.Controls.Add((Control)this.lstAdditionalCrls);
            this.gbOnlineCertChecking.Controls.Add((Control)this.label2);
            this.gbOnlineCertChecking.Controls.Add((Control)this.ckbCheckingSignerCertViaOcsp);
            this.gbOnlineCertChecking.Location = new Point(56, 123);
            this.gbOnlineCertChecking.Name = "gbOnlineCertChecking";
            this.gbOnlineCertChecking.Size = new Size(512, 148);
            this.gbOnlineCertChecking.TabIndex = 5;
            this.gbOnlineCertChecking.TabStop = false;
            this.btnDelCrlLink.AutoSize = true;
            this.btnDelCrlLink.Location = new Point(471, 40);
            this.btnDelCrlLink.Name = "btnDelCrlLink";
            this.btnDelCrlLink.Size = new Size(26, 13);
            this.btnDelCrlLink.TabIndex = 3;
            this.btnDelCrlLink.TabStop = true;
            this.btnDelCrlLink.Text = "Xóa";
            this.btnDelCrlLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.btnDelCrlLink_LinkClicked);
            this.btnAddCrlLink.AutoSize = true;
            this.btnAddCrlLink.Location = new Point(424, 40);
            this.btnAddCrlLink.Name = "btnAddCrlLink";
            this.btnAddCrlLink.Size = new Size(34, 13);
            this.btnAddCrlLink.TabIndex = 3;
            this.btnAddCrlLink.TabStop = true;
            this.btnAddCrlLink.Text = "Thêm";
            this.btnAddCrlLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.btnAddCrlLink_LinkClicked);
            this.lstAdditionalCrls.FormattingEnabled = true;
            this.lstAdditionalCrls.Location = new Point(16, 57);
            this.lstAdditionalCrls.Name = "lstAdditionalCrls";
            this.lstAdditionalCrls.ScrollAlwaysVisible = true;
            this.lstAdditionalCrls.Size = new Size(484, 82);
            this.lstAdditionalCrls.TabIndex = 2;
            this.label2.AutoSize = true;
            this.label2.Location = new Point(13, 41);
            this.label2.Name = "label2";
            this.label2.Size = new Size(248, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Đường dẫn danh sách chứng thư bị thu hồi (CRLs):";
            ((Control)this.ckbCheckingSignerCertViaOcsp).AutoSize = true;
            ((CheckBox)this.ckbCheckingSignerCertViaOcsp).Checked = true;
            ((CheckBox)this.ckbCheckingSignerCertViaOcsp).CheckState = CheckState.Checked;
            ((Control)this.ckbCheckingSignerCertViaOcsp).Location = new Point(14, 15);
            ((Control)this.ckbCheckingSignerCertViaOcsp).Name = "ckbCheckingSignerCertViaOcsp";
            ((Control)this.ckbCheckingSignerCertViaOcsp).Size = new Size(299, 15);
            ((Control)this.ckbCheckingSignerCertViaOcsp).TabIndex = 0;
            ((Control)this.ckbCheckingSignerCertViaOcsp).Text = "Cho phép kiểm tra chứng thư số người ký qua OCSP";
            this.ckbCheckingSignerCertViaOcsp.UseSelectable = true;
            ((CheckBox)this.ckbCheckingSignerCertViaOcsp).CheckedChanged += new EventHandler(this.ckbCheckingSignerCertViaOcsp_CheckedChanged);
            ((Control)this.ckbAllowCertCheckingOnline).AutoSize = true;
            ((CheckBox)this.ckbAllowCertCheckingOnline).Checked = true;
            ((CheckBox)this.ckbAllowCertCheckingOnline).CheckState = CheckState.Checked;
            ((Control)this.ckbAllowCertCheckingOnline).Location = new Point(37, 102);
            ((Control)this.ckbAllowCertCheckingOnline).Name = "ckbAllowCertCheckingOnline";
            ((Control)this.ckbAllowCertCheckingOnline).Size = new Size(345, 15);
            ((Control)this.ckbAllowCertCheckingOnline).TabIndex = 4;
            ((Control)this.ckbAllowCertCheckingOnline).Text = "Sử dụng dịch vụ kiểm tra trạng thái thu hồi của chứng thư số";
            this.ckbAllowCertCheckingOnline.UseSelectable = true;
            ((CheckBox)this.ckbAllowCertCheckingOnline).CheckedChanged += new EventHandler(this.ckbAllowCertCheckingOnline_CheckedChanged);
            this.gbTSA.Controls.Add((Control)this.metroLabel5);
            this.gbTSA.Controls.Add((Control)this.txtTsaAddress);
            this.gbTSA.Location = new Point(56, 38);
            this.gbTSA.Name = "gbTSA";
            this.gbTSA.Size = new Size(512, 53);
            this.gbTSA.TabIndex = 3;
            this.gbTSA.TabStop = false;
            this.gbTSA.Text = "Máy chủ dịch vụ cấp dấu thời gian (TSA)";
            ((Control)this.metroLabel5).AutoSize = true;
            ((Control)this.metroLabel5).Location = new Point(8, 19);
            ((Control)this.metroLabel5).Name = "metroLabel5";
            ((Control)this.metroLabel5).Size = new Size(51, 19);
            ((Control)this.metroLabel5).TabIndex = 1;
            ((Control)this.metroLabel5).Text = "Địa chỉ:";
            this.txtTsaAddress.Location = new Point(122, 21);
            this.txtTsaAddress.Name = "txtTsaAddress";
            this.txtTsaAddress.Size = new Size(378, 20);
            this.txtTsaAddress.TabIndex = 2;
            this.txtTsaAddress.Text = "http://tsa.ca.gov.vn";
            this.txtTsaAddress.TextChanged += new EventHandler(this.txtTsaAddress_TextChanged);
            ((Control)this.ckbUseTsa).AutoSize = true;
            ((CheckBox)this.ckbUseTsa).Checked = true;
            ((CheckBox)this.ckbUseTsa).CheckState = CheckState.Checked;
            ((Control)this.ckbUseTsa).Location = new Point(34, 17);
            ((Control)this.ckbUseTsa).Name = "ckbUseTsa";
            ((Control)this.ckbUseTsa).Size = new Size(236, 15);
            ((Control)this.ckbUseTsa).TabIndex = 0;
            ((Control)this.ckbUseTsa).Text = "Sử dụng dịch vụ cấp dấu thời gian (TSA)";
            this.ckbUseTsa.UseSelectable = true;
            ((CheckBox)this.ckbUseTsa).CheckedChanged += new EventHandler(this.ckbUseTsa_CheckedChanged);
            this.tabPageUpdate.BackColor = SystemColors.ControlLightLight;
            this.tabPageUpdate.Controls.Add((Control)this.panel1);
            this.tabPageUpdate.Controls.Add((Control)this.ckbAutoUpdate);
            this.tabPageUpdate.Location = new Point(4, 35);
            this.tabPageUpdate.Name = "tabPageUpdate";
            this.tabPageUpdate.Size = new Size(586, 334);
            this.tabPageUpdate.TabIndex = 4;
            this.tabPageUpdate.Text = "Cập nhật phần mềm";
            this.panel1.Controls.Add((Control)this.mLabelCurrVersion);
            this.panel1.Controls.Add((Control)this.mLabelLastCheckingUpdate);
            this.panel1.Controls.Add((Control)this.metroLabel2);
            this.panel1.Controls.Add((Control)this.metroLabel1);
            this.panel1.Location = new Point(33, 73);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(461, 189);
            this.panel1.TabIndex = 1;
            ((Control)this.mLabelCurrVersion).AutoSize = true;
            ((Control)this.mLabelCurrVersion).Location = new Point(258, 45);
            ((Control)this.mLabelCurrVersion).Name = "mLabelCurrVersion";
            ((Control)this.mLabelCurrVersion).Size = new Size(47, 19);
            ((Control)this.mLabelCurrVersion).TabIndex = 3;
            ((Control)this.mLabelCurrVersion).Text = "3.1.1.16";
            ((Control)this.mLabelLastCheckingUpdate).AutoSize = true;
            ((Control)this.mLabelLastCheckingUpdate).Location = new Point(258, 19);
            ((Control)this.mLabelLastCheckingUpdate).Name = "mLabelLastCheckingUpdate";
            ((Control)this.mLabelLastCheckingUpdate).Size = new Size(126, 19);
            ((Control)this.mLabelLastCheckingUpdate).TabIndex = 2;
            ((Control)this.mLabelLastCheckingUpdate).Text = "30/11/2013 12:00 PM";
            ((Control)this.metroLabel2).AutoSize = true;
            ((Control)this.metroLabel2).Location = new Point(23, 45);
            ((Control)this.metroLabel2).Name = "metroLabel2";
            ((Control)this.metroLabel2).Size = new Size(116, 19);
            ((Control)this.metroLabel2).TabIndex = 1;
            ((Control)this.metroLabel2).Text = "Phiên bản hiện tại:";
            ((Control)this.metroLabel1).AutoSize = true;
            ((Control)this.metroLabel1).Location = new Point(23, 19);
            ((Control)this.metroLabel1).Name = "metroLabel1";
            ((Control)this.metroLabel1).Size = new Size(164, 19);
            ((Control)this.metroLabel1).TabIndex = 0;
            ((Control)this.metroLabel1).Text = "Kiểm tra cập nhật lần cuối:";
            ((Control)this.ckbAutoUpdate).AutoSize = true;
            ((CheckBox)this.ckbAutoUpdate).Checked = true;
            ((CheckBox)this.ckbAutoUpdate).CheckState = CheckState.Checked;
            ((Control)this.ckbAutoUpdate).Location = new Point(35, 28);
            ((Control)this.ckbAutoUpdate).Name = "ckbAutoUpdate";
            ((Control)this.ckbAutoUpdate).Size = new Size(305, 15);
            ((Control)this.ckbAutoUpdate).TabIndex = 0;
            ((Control)this.ckbAutoUpdate).Text = "Tự động kiểm tra cập nhật khi chương trình được mở";
            this.ckbAutoUpdate.UseSelectable = true;
            ((CheckBox)this.ckbAutoUpdate).CheckedChanged += new EventHandler(this.ckbAutoUpdate_CheckedChanged);
            ((Control)this.btnClose).Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ((Control)this.btnClose).Location = new Point(536, 442);
            ((Control)this.btnClose).Name = "btnClose";
            ((Control)this.btnClose).Size = new Size(70, 23);
            ((Control)this.btnClose).TabIndex = 2;
            ((Control)this.btnClose).Text = "Đóng";
            this.btnClose.UseSelectable = true;
            ((Control)this.btnClose).Click += new EventHandler(this.btnClose_Click);
            ((Control)this.btnSave).Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ((Control)this.btnSave).Enabled = false;
            ((Control)this.btnSave).Location = new Point(410, 442);
            ((Control)this.btnSave).Name = "btnSave";
            ((Control)this.btnSave).Size = new Size(109, 23);
            ((Control)this.btnSave).TabIndex = 4;
            ((Control)this.btnSave).Text = "Lưu";
            this.btnSave.UseSelectable = true;
            ((Control)this.btnSave).Visible = false;
            ((Control)this.btnSave).Click += new EventHandler(this.btnSave_Click);
            ((Control)this.btnRestore).Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ((Control)this.btnRestore).Location = new Point(106, 442);
            ((Control)this.btnRestore).Name = "btnRestore";
            ((Control)this.btnRestore).Size = new Size(71, 23);
            ((Control)this.btnRestore).TabIndex = 5;
            ((Control)this.btnRestore).Text = "Khôi phục";
            this.btnRestore.UseSelectable = true;
            ((Control)this.btnRestore).Visible = false;
            ((Control)this.btnRestore).Click += new EventHandler(this.btnRestore_Click);
            ((Control)this.btnBackup).Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ((Control)this.btnBackup).Location = new Point(27, 442);
            ((Control)this.btnBackup).Name = "btnBackup";
            ((Control)this.btnBackup).Size = new Size(67, 23);
            ((Control)this.btnBackup).TabIndex = 6;
            ((Control)this.btnBackup).Text = "Sao lưu";
            this.btnBackup.UseSelectable = true;
            ((Control)this.btnBackup).Visible = false;
            ((Control)this.btnBackup).Click += new EventHandler(this.btnBackup_Click);
            ((Control)this.btnDelProfile).Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ((Control)this.btnDelProfile).Location = new Point(192, 442);
            ((Control)this.btnDelProfile).Name = "btnDelProfile";
            ((Control)this.btnDelProfile).Size = new Size(75, 23);
            ((Control)this.btnDelProfile).TabIndex = 7;
            ((Control)this.btnDelProfile).Text = "Xóa mẫu";
            this.btnDelProfile.UseSelectable = true;
            ((Control)this.btnDelProfile).Visible = false;
            ((Control)this.btnDelProfile).Click += new EventHandler(this.btnDelProfile_Click);
            ((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
            ((ContainerControl)this).AutoScaleMode = AutoScaleMode.Font;
            this.BorderStyle = (MetroFormBorderStyle)1;
            ((Form)this).ClientSize = new Size(634, 485);
            ((Control)this).Controls.Add((Control)this.tabCtrlMain);
            ((Control)this).Controls.Add((Control)this.btnBackup);
            ((Control)this).Controls.Add((Control)this.btnDelProfile);
            ((Control)this).Controls.Add((Control)this.btnClose);
            ((Control)this).Controls.Add((Control)this.btnSave);
            ((Control)this).Controls.Add((Control)this.btnRestore);
            //((Form)this).Icon = (Icon)componentResourceManager.GetObject("Icon");
            ((Form)this).MaximizeBox = false;
            ((Form)this).MinimizeBox = false;
            ((Form)this).TopMost = true;
            ((Control)this).MinimumSize = new Size(573, 434);
            ((Control)this).Name = nameof(frmConfig);
            this.Resizable = false;
            ((Control)this).Text = "Cấu hình hệ thống";
            ((Form)this).Shown += new EventHandler(this.frmConfig_Shown);
            ((Control)this.tabCtrlMain).ResumeLayout(false);
            this.tabPageConnection.ResumeLayout(false);
            this.tabPageConnection.PerformLayout();
            this.nPxPort.EndInit();
            this.tabPagePdf.ResumeLayout(false);
            this.tabPagePdf.PerformLayout();
            this.gbProfileDetails.ResumeLayout(false);
            this.gbProfileDetails.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((Control)this.pnlSigAppearance).ResumeLayout(false);
            ((ISupportInitialize)this.pcbSignatureImage).EndInit();
            this.CtxtMnTripImage.ResumeLayout(false);
            this.tabPagePKIServices.ResumeLayout(false);
            this.tabPagePKIServices.PerformLayout();
            this.gbOnlineCertChecking.ResumeLayout(false);
            this.gbOnlineCertChecking.PerformLayout();
            this.gbTSA.ResumeLayout(false);
            this.gbTSA.PerformLayout();
            this.tabPageUpdate.ResumeLayout(false);
            this.tabPageUpdate.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((Control)this).ResumeLayout(false);
        }
    }
}
