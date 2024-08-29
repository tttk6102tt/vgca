using Plugin.UI.PDF;
using Plugin.UI.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Plugin.UI
{
    public partial class frmConverter 
    {
        private BackgroundWorker bgWorker;
        private IContainer components;
        private TextBox txtWordPath;
        private Label label1;
        private Button btnBrowseWord;
        private Label label2;
        private TextBox txtPdfPath;
        private Button btnBrsPdf;
        private Button btnConvert;
        private Button btnClose;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private PictureBox pictureBox4;

        public frmConverter()
        {
            this.InitializeComponent();
            this.bgWorker = new BackgroundWorker();
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.WorkerSupportsCancellation = true;
            this.bgWorker.DoWork += new DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.ProgressChanged += new ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
            this.bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
        }

        private void bgWorker_DoWork(
          object sender,
          DoWorkEventArgs e)
        {
            try
            {
                string str1 = this.txtWordPath.Text.Trim();
                string destFilename = this.txtPdfPath.Text.Trim();
                if (!File.Exists(str1))
                {
                    throw new Exception("Tệp đầu vào không tồn tại");
                }
                else
                {
                    string extension = Path.GetExtension(str1);
                    string str2 = "*.doc, *.docx, *.xls, *.xlsx, *.ppt, *.pptx";
                    if (!string.IsNullOrEmpty(extension))
                    {
                        if (str2.Contains(extension))
                        {
                            if (extension.Equals(".doc", StringComparison.InvariantCultureIgnoreCase) ||
                                extension.Equals(".docx", StringComparison.InvariantCultureIgnoreCase))
                            {
                                PdfConverter.Word2PDF(str1, destFilename);
                            }
                            else if (extension.Equals(".xls", StringComparison.InvariantCultureIgnoreCase) ||
                                     extension.Equals(".xlsx", StringComparison.InvariantCultureIgnoreCase))
                            {
                                PdfConverter.Excel2PDF(str1, destFilename);
                            }
                            else if (extension.Equals(".ppt", StringComparison.InvariantCultureIgnoreCase) ||
                                     extension.Equals(".pptx", StringComparison.InvariantCultureIgnoreCase))
                            {
                                PdfConverter.PowerPoint2PDF(str1, destFilename);
                            }

                            e.Result = null;
                            return;
                        }
                       
                    }
                    throw new Exception("Tệp đầu vào không đúng định dạng");
                }
            }
            catch (Exception ex)
            {
                e.Result = (object)ex;
            }
        }

        private void bgWorker_ProgressChanged(
          object sender,
          ProgressChangedEventArgs e)
        {
        }

        private void bgWorker_RunWorkerCompleted(
          object sender,
          RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Default;
            this.btnConvert.Enabled = true;
            if (e.Result != null)
            {
                try
                {
                    int num = (int)MessageBox.Show((e.Result as Exception).Message, "Lỗi quá trình chuyển đổi", MessageBoxButtons.OK, MessageBoxIcon.Question);
                }
                catch
                {
                    int num = (int)MessageBox.Show("Quá trình chuyển đổi tài liệu sang pdf không thành công!", "Lỗi quá trình chuyển đổi", MessageBoxButtons.OK, MessageBoxIcon.Question);
                }
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        public string PdfPath => this.txtPdfPath.Text.Trim();

        private void btnClose_Click(
          object sender,
          EventArgs e)
        {
            this.Close();
        }

        private void btnBrowseWord_Click(
          object sender,
          EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "MS Office Files (*.doc, *.docx, *.xls, *.xlsx, *.ppt, *.pptx) | *.doc;*.docx;  *.xls; *.xlsx; *.ppt; *.pptx";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            this.txtWordPath.Text = openFileDialog.FileName;
            this.txtPdfPath.Text = string.Format("{0}\\{1}.pdf", (object)Path.GetDirectoryName(openFileDialog.FileName), (object)Path.GetFileNameWithoutExtension(openFileDialog.FileName));
        }

        private void btnBrsPdf_Click(
          object sender,
          EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Files (*.pdf) | *.pdf";
            if (this.txtPdfPath.Text != "")
            {
                try
                {
                    saveFileDialog.InitialDirectory = Path.GetDirectoryName(this.txtPdfPath.Text);
                    saveFileDialog.FileName = Path.GetFileNameWithoutExtension(this.txtPdfPath.Text);
                }
                catch
                {
                }
            }
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;
            this.txtPdfPath.Text = saveFileDialog.FileName;
        }

        private void btnConvert_Click(
          object sender,
          EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            this.btnConvert.Enabled = false;
            if (this.bgWorker.IsBusy)
                return;
            this.bgWorker.RunWorkerAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(frmConverter));
            this.txtWordPath = new TextBox();
            this.label1 = new Label();
            this.btnBrowseWord = new Button();
            this.label2 = new Label();
            this.txtPdfPath = new TextBox();
            this.btnBrsPdf = new Button();
            this.btnConvert = new Button();
            this.btnClose = new Button();
            this.pictureBox1 = new PictureBox();
            this.pictureBox2 = new PictureBox();
            this.pictureBox3 = new PictureBox();
            this.pictureBox4 = new PictureBox();
            ((ISupportInitialize)this.pictureBox1).BeginInit();
            ((ISupportInitialize)this.pictureBox2).BeginInit();
            ((ISupportInitialize)this.pictureBox3).BeginInit();
            ((ISupportInitialize)this.pictureBox4).BeginInit();
            this.SuspendLayout();
            this.txtWordPath.BackColor = SystemColors.Info;
            this.txtWordPath.Location = new Point(17, 115);
            this.txtWordPath.Name = "txtWordPath";
            this.txtWordPath.Size = new Size(325, 20);
            this.txtWordPath.TabIndex = 0;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(19, 98);
            this.label1.Name = "label1";
            this.label1.Size = new Size(185, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Chọn đường dẫn tệp cần chuyển đổi:";
            this.btnBrowseWord.BackColor = SystemColors.Control;
            this.btnBrowseWord.Location = new Point(348, 113);
            this.btnBrowseWord.Name = "btnBrowseWord";
            this.btnBrowseWord.Size = new Size(75, 23);
            this.btnBrowseWord.TabIndex = 2;
            this.btnBrowseWord.Text = "Chọn...";
            this.btnBrowseWord.UseVisualStyleBackColor = false;
            this.btnBrowseWord.Click += new EventHandler(this.btnBrowseWord_Click);
            this.label2.AutoSize = true;
            this.label2.Location = new Point(19, 148);
            this.label2.Name = "label2";
            this.label2.Size = new Size(149, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Chọn đường dẫn lưu tệp PDF:";
            this.txtPdfPath.BackColor = SystemColors.Info;
            this.txtPdfPath.Location = new Point(17, 164);
            this.txtPdfPath.Name = "txtPdfPath";
            this.txtPdfPath.Size = new Size(325, 20);
            this.txtPdfPath.TabIndex = 0;
            this.btnBrsPdf.BackColor = SystemColors.Control;
            this.btnBrsPdf.Location = new Point(348, 161);
            this.btnBrsPdf.Name = "btnBrsPdf";
            this.btnBrsPdf.Size = new Size(75, 23);
            this.btnBrsPdf.TabIndex = 2;
            this.btnBrsPdf.Text = "Chọn...";
            this.btnBrsPdf.UseVisualStyleBackColor = false;
            this.btnBrsPdf.Click += new EventHandler(this.btnBrsPdf_Click);
            this.btnConvert.BackColor = SystemColors.Control;
            this.btnConvert.Location = new Point(214, 204);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new Size(82, 29);
            this.btnConvert.TabIndex = 3;
            this.btnConvert.Text = "Chuyển đổi...";
            this.btnConvert.UseVisualStyleBackColor = false;
            this.btnConvert.Click += new EventHandler(this.btnConvert_Click);
            this.btnClose.BackColor = SystemColors.Control;
            this.btnClose.Location = new Point(343, 204);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(82, 29);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Đóng";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new EventHandler(this.btnClose_Click);
            this.pictureBox1.Image = (Image)Resources.pdf1;
            this.pictureBox1.Location = new Point(358, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new Size(65, 69);
            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            this.pictureBox2.Image = (Image)Resources.word64;
            this.pictureBox2.Location = new Point(17, 12);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new Size(63, 69);
            this.pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 5;
            this.pictureBox2.TabStop = false;
            this.pictureBox3.Image = (Image)Resources.excel64;
            this.pictureBox3.Location = new Point(86, 12);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new Size(63, 69);
            this.pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 5;
            this.pictureBox3.TabStop = false;
            this.pictureBox4.Image = (Image)Resources.ppt64;
            this.pictureBox4.Location = new Point(155, 12);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new Size(63, 69);
            this.pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox4.TabIndex = 5;
            this.pictureBox4.TabStop = false;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = SystemColors.ControlLightLight;
            this.ClientSize = new Size(444, 250);
            this.Controls.Add((Control)this.pictureBox4);
            this.Controls.Add((Control)this.pictureBox3);
            this.Controls.Add((Control)this.pictureBox2);
            this.Controls.Add((Control)this.pictureBox1);
            this.Controls.Add((Control)this.btnClose);
            this.Controls.Add((Control)this.btnConvert);
            this.Controls.Add((Control)this.btnBrsPdf);
            this.Controls.Add((Control)this.btnBrowseWord);
            this.Controls.Add((Control)this.label2);
            this.Controls.Add((Control)this.label1);
            this.Controls.Add((Control)this.txtPdfPath);
            this.Controls.Add((Control)this.txtWordPath);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            this.MaximizeBox = false;
            this.Name = "frmConverter";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Chuyển đổi tài liệu Word, Excel, PowerPoint sang PDF";
            ((ISupportInitialize)this.pictureBox1).EndInit();
            ((ISupportInitialize)this.pictureBox2).EndInit();
            ((ISupportInitialize)this.pictureBox3).EndInit();
            ((ISupportInitialize)this.pictureBox4).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}