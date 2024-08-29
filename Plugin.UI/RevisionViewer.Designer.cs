using Plugin.UI.classess;
using Plugin.UI.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Plugin.UI
{
    public partial class RevisionViewer : UserControl
    {
        private bool _hasOpenFile;
        private const int c35ccc5c29af1c4933bf3c466fb1a7933 = 5;
        private int countMouseWeelLastPosition;
        private int countMouseWeelHeadPosition;
        private PdfPageRenderer _pdfPageRenderer;
        private float zoomStep = 0.25f;
        private float minImageResolution = 0.15f;
        private float maxImageResolution = 6f;
        private ZoomType zoomType;
        private int _totalPage;
        private int clientPanelWidth;
        private ImageRotation imageRotation;
        private float _zoomStep = 2f;
        private Padding padding = new Padding(0, 2, 4, 2);
        private KichThuoc[] arrClass10;
        private int _selectedPageNum = -1;
        private int _currentPageIndex;
        private string _absolutePath;
        private IContainer component;
        private Button btnPageUp;
        private Label label3;
        private Label label1;
        private Label label2;
        private Panel ClientPanel;
        private Label label5;
        private Label lbTotalPage;
        private Panel controlPanel;
        private TextBox txtCurrentPage;
        private Button btnPrint;
        private Button btnFitWidth;
        private Button btnFitPage;
        private Button btnZoomIn;
        private Button btnZoomOut;
        private Button btnPageDown;
        private PictureBox pbView;

        public RevisionViewer()
        {
            this.InitializeComponent();
            this.setEnableButton(false);
            this.BackColor = SystemColors.Control;
            this.ClientPanel.AutoScroll = true;
            this.ClientPanel.MouseWheel += new MouseEventHandler(this.ClientPanel_MouseWheel);
            this.initPanelViewPdf();
        }

        public ZoomType ZoomType
        {
            get => this.zoomType;
            private set
            {
                if (this.zoomType == value)
                    return;
                this.zoomType = value;
            }
        }

        public int TotalPages
        {
            get => this._totalPage;
            private set => this._totalPage = value;
        }

        internal KichThuoc[] ArrayClass10 => this.arrClass10;

        public int CurrentPageIndex => this._currentPageIndex;

        public string AbsolutePath
        {
            get => this._absolutePath;
            set => this._absolutePath = value;
        }

        public void RevisionShow() => this.revisionShow();

        private void setEnableButton(bool status)
        {
            IEnumerator enumerator = this.controlPanel.Controls.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    ((Control)enumerator.Current).Enabled = status;
            }
            finally
            {
                if (enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            this.btnPrint.Enabled = false;
        }

        private void Form_SizeChanged(
          object sender,
          EventArgs e)
        {
            if (this._hasOpenFile)
            {
                PictureBox pictureBox = this.PbView();

                if (this.ZoomType == ZoomType.FitToWidth && pictureBox.Image != null)
                {
                    this.ZoomToWidth();
                }
                else if (this.ZoomType == ZoomType.Fixed && pictureBox.Image != null)
                {
                    this.Zoom(this._pdfPageRenderer.PageLayout.ImageResolution * (float)this.ClientPanel.Width / (float)this.clientPanelWidth);
                }
            }
            this.clientPanelWidth = this.ClientPanel.Width;
        }

        private void btnPageDown_Click(
          object sender,
          EventArgs e)
        {
            Application.DoEvents();
            if (this._currentPageIndex >= this._totalPage)
            {
                
            }
            else
            {
                this._selectedPageNum = this._currentPageIndex;
                ++this._currentPageIndex;
                this.initPbView();
            }
        }

        private void btnPageUp_Click(
          object sender,
          EventArgs e)
        {
            Application.DoEvents();
            if (this._currentPageIndex <= 1)
            {
               
            }
            else
            {
                this._selectedPageNum = this._currentPageIndex;
                --this._currentPageIndex;
                this.initPbView();
            }
        }

        private void txtCurrentPage_KeyPress(
          object sender,
          KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                int.TryParse(this.txtCurrentPage.Text, out this._currentPageIndex);
                if (this._currentPageIndex <= 0)
                {
                    this._currentPageIndex = 1;
                }
                this.ClientPanel.Focus();
                e.Handled = true;
            }
            if (char.IsControl(e.KeyChar))
                return;
            if (!char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
                return;
            }
           
        }

        private void txtCurrentPage_Leave(
          object sender,
          EventArgs e)
        {
            Application.DoEvents();
            int result = this._currentPageIndex;
            if (!int.TryParse(this.txtCurrentPage.Text, out result))
                return;
            if (!(result <= 0 || result > this._totalPage))
            {
                this._currentPageIndex = result;
                this.initPbView();
                return;
            }
        }

        private void btnZoomOut_Click(
          object sender,
          EventArgs e)
        {
            this.ZoomOut();
        }

        private void btnZoomIn_Click(
          object sender,
          EventArgs e)
        {
            this.ZoomIn();
        }

        private void btnFitPage_Click(
          object sender,
          EventArgs e)
        {
            this.ZoomToHeight();
        }

        private void btnFitWidth_Click(
          object sender,
          EventArgs e)
        {
            this.ZoomToWidth();
        }

        public void GotoFirstPage()
        {
            if (!this._hasOpenFile)
                return;
            this.txtCurrentPage.Text = "1";
            this.txtCurrentPage_Leave((object)null, (EventArgs)null);
        }

        public void GotoLastPage()
        {
            if (!this._hasOpenFile)
                return;
            this.txtCurrentPage.Text = this._totalPage.ToString();
            this.txtCurrentPage_Leave((object)null, (EventArgs)null);
        }

        public void GotoNextPage()
        {
            if (!this._hasOpenFile)
                return;
            this.btnPageDown_Click((object)null, (EventArgs)null);
        }

        public void GotoPreviousPage()
        {
            if (!this._hasOpenFile)
                return;
            this.btnPageUp_Click((object)null, (EventArgs)null);
        }

        public void ZoomToWidth()
        {
            Application.DoEvents();
            int num1;
            if (!this.ClientPanel.VerticalScroll.Visible)
            {
                num1 = 0;
            }
            else
                num1 = SystemInformation.VerticalScrollBarWidth;
            int num2 = num1;
            float num3 = (float)(this.ClientPanel.ClientSize.Width - num2) / this.ArrayClass10[this._currentPageIndex - 1].KichThuocSauCung.Width;
            KichThuoc Class10 = this.ArrayClass10[this._currentPageIndex - 1];
            if (num2 == 0 && (double)Class10.KichThuocGoc.Height * (double)num3 + (double)Class10.PaddingDoc >= (double)this.ClientPanel.ClientSize.Height)
            {
                num2 += SystemInformation.VerticalScrollBarWidth;
            }
            this.Zoom((float)(this.ClientPanel.ClientSize.Width - (num2 + 2)) / this.ArrayClass10[this._currentPageIndex - 1].KichThuocSauCung.Width);
            this.ZoomType = ZoomType.FitToWidth;
        }

        public void ZoomToHeight()
        {
            int num1;
            if (!this.ClientPanel.HorizontalScroll.Visible)
            {
                num1 = 0;
            }
            else
                num1 = SystemInformation.HorizontalScrollBarHeight;
            int num2 = num1;
            float num3 = (float)(this.ClientPanel.ClientSize.Height - num2) / this.ArrayClass10[this._currentPageIndex - 1].KichThuocSauCung.Height;
            KichThuoc Class10 = this.ArrayClass10[this._currentPageIndex - 1];
            if (num2 == 0)
            {
                if ((double)Class10.KichThuocGoc.Width * (double)num3 + (double)Class10.PaddingNgang >= (double)this.ClientPanel.ClientSize.Width)
                {
                    num2 += SystemInformation.HorizontalScrollBarHeight;
                }
            }
            this.Zoom((float)(this.ClientPanel.ClientSize.Height - num2) / this.ArrayClass10[this._currentPageIndex - 1].KichThuocSauCung.Height);
            this.ZoomType = ZoomType.FitToHeight;
        }

        public void ZoomIn()
        {
            Application.DoEvents();
            this._pdfPageRenderer.PageLayout.ImageResolution += this.zoomStep;
            if ((double)this._pdfPageRenderer.PageLayout.ImageResolution > (double)this.maxImageResolution)
                this._pdfPageRenderer.PageLayout.ImageResolution = this.maxImageResolution;
            PictureBox pPicBox = this.PbView();
            ImageUtil.PictureBoxZoomIn(ref pPicBox);
            pPicBox.Refresh();
            this.initPbView();
            this.zoomType = ZoomType.Fixed;
        }

        public void ZoomOut()
        {
            Application.DoEvents();
            this._pdfPageRenderer.PageLayout.ImageResolution -= this.zoomStep;
            if ((double)this._pdfPageRenderer.PageLayout.ImageResolution < (double)this.minImageResolution)
            {
                this._pdfPageRenderer.PageLayout.ImageResolution = this.minImageResolution;
            }
            PictureBox pPicBox = this.PbView();
            ImageUtil.PictureBoxZoomOut(ref pPicBox);
            pPicBox.Refresh();
            this.initPbView();
            this.ZoomType = ZoomType.Fixed;
        }

        public void Zoom(float zoomFactor)
        {
            Application.DoEvents();
            this._pdfPageRenderer.PageLayout.ImageResolution = zoomFactor;
            PictureBox pPicBox = this.PbView();
            ImageUtil.PictureBoxZoomActual(ref pPicBox);
            pPicBox.Refresh();
            this.initPbView();
            this.ZoomType = ZoomType.Fixed;
        }

        private static void setSizePictureBoxByImage(
          ref PictureBox pictureBox,
          Image img)
        {
            if (img != null)
            {
                pictureBox.Width = img.Width;
                pictureBox.Height = img.Height;
            }
            else
            {
                pictureBox.Width = pictureBox.Image.Width;
                pictureBox.Height = pictureBox.Image.Height;
            }
        }
        
        private static void setSizePictureBoxByImageNotNull(ref PictureBox pictureBox, Image img)
        {
            int pictureBoxHeight = pictureBox.Height;
            int pictureBoxWidth = pictureBox.Width;
            int imageHeight = img.Height;
            int imageWidth = img.Width;

            if (pictureBoxWidth < pictureBoxHeight && imageWidth > imageHeight)
            {
                // Hoán đổi chiều cao và chiều rộng của PictureBox 
                // khi PictureBox có chiều rộng nhỏ hơn chiều cao 
                // và Image có chiều rộng lớn hơn chiều cao.
                pictureBox.Width = pictureBoxHeight;
                pictureBox.Height = pictureBoxWidth;
            }
        }
        private static void renderImageToPictureBox(
          Image img,
          ref PictureBox pictrueBox)
        {
            if (img == null)
                return;
            RevisionViewer.setSizePictureBoxByImageNotNull(ref pictrueBox, img);
            RevisionViewer.setSizePictureBoxByImage(ref pictrueBox, img);
            ImageUtil.TinhLaiViTriTrang(ref pictrueBox);
            pictrueBox.Image = img;
        }

        private PictureBox PbView() => this.pbView;
        
        private void UpdatePageNavigationButtons()
        {
            if (this._currentPageIndex >= this._totalPage)
            {
                this._currentPageIndex = this._totalPage;
                this.btnPageDown.Enabled = false;
            }
            else if (this._currentPageIndex <= 1)
            {
                this._currentPageIndex = 1;
                this.btnPageUp.Enabled = false;
            }
            if (this._currentPageIndex < this._totalPage)
            {
                if (this._totalPage > 1 && this._currentPageIndex > 1)
                {
                    this.btnPageDown.Enabled = true;
                    this.btnPageUp.Enabled = true;
                }
            }
            if (this._currentPageIndex == this._totalPage && this._totalPage > 1)
            {
                if (this._currentPageIndex > 1)
                {
                    this.btnPageUp.Enabled = true;
                }
            }
            if (this._currentPageIndex == 1)
            {
                if (this._totalPage > 1)
                {
                    this.btnPageDown.Enabled = true;
                }
            }
            if (this._totalPage != 1)
                return;
            this.btnPageDown.Enabled = false;
            this.btnPageUp.Enabled = false;
        }
        
       
        private void setCurrentPageSelected()
        {
            this.lbTotalPage.Text = "/ " + this._totalPage.ToString();
            this.txtCurrentPage.Text = this._currentPageIndex.ToString();
        }

        private Image renderPdfToPictureBoxInPageNum(
          int curPageNum,
          ref PictureBox pictrueBox)
        {
            if (this.InvokeRequired)
            {
                this.Cursor = Cursors.WaitCursor;
            }
            else
                this.Cursor = Cursors.WaitCursor;
            Bitmap bitmap = XuLyPDF.LayHinhAnhTrang(this._absolutePath, curPageNum, this._pdfPageRenderer.PageLayout.ImageResolution);
            if (this.InvokeRequired)
            {
                return (Image)bitmap;
            }
            else
            {
                this.Cursor = Cursors.Default;
                return (Image)bitmap;
            }
        }

        private void initPbView()
        {
            PictureBox pictrueBox = this.PbView();
            if (this._selectedPageNum != this._currentPageIndex)
            {
                this.UpdatePageNavigationButtons();
                this.setCurrentPageSelected();
                RevisionViewer.renderImageToPictureBox(this.renderPdfToPictureBoxInPageNum(this._currentPageIndex, ref pictrueBox), ref pictrueBox);
                pictrueBox.Refresh();
                this.ClientPanel.Focus();
                this.ClientPanel.VerticalScroll.Value = 0;
            }
            else
                RevisionViewer.renderImageToPictureBox(pictrueBox.Image, ref pictrueBox);
        }
        
        private void ClientPanel_MouseWheel(
          object sender,
          MouseEventArgs e)
        {
            Application.DoEvents();
            if (e.Delta == 0)
                return;
            if (!this._hasOpenFile)
            {
                
            }
            else
            {
                bool flag1 = false;
                bool flag2 = false;
                if (this.ClientPanel.VerticalScroll.Visible)
                {
                    if (this.ClientPanel.VerticalScroll.Value >= Math.Abs(this.ClientPanel.VerticalScroll.Maximum - this.ClientPanel.VerticalScroll.LargeChange))
                    {
                        ++this.countMouseWeelLastPosition;
                        int num;
                        if (e.Delta < 0)
                        {
                            num = this.countMouseWeelLastPosition >= 5 ? 1 : 0;
                        }
                        else
                            num = 0;
                        flag1 = num != 0;
                    }
                    else if (this.ClientPanel.VerticalScroll.Value <= 0)
                    {
                        ++this.countMouseWeelHeadPosition;
                        flag2 = e.Delta > 0 && this.countMouseWeelHeadPosition >= 5;
                    }
                }
                else
                {
                    ++this.countMouseWeelLastPosition;
                    ++this.countMouseWeelHeadPosition;
                    flag1 = e.Delta < 0 && this.countMouseWeelLastPosition >= 5;
                    int num;
                    if (e.Delta > 0)
                    {
                        num = this.countMouseWeelHeadPosition >= 5 ? 1 : 0;
                    }
                    else
                        num = 0;
                    flag2 = num != 0;
                }
                if (flag1)
                {
                    try
                    {
                        this.GotoNextPage();
                        this.countMouseWeelLastPosition = 0;
                    }
                    catch
                    {
                    }
                }
                if (!flag2)
                    return;
                try
                {
                    this.GotoPreviousPage();
                    this.countMouseWeelHeadPosition = 0;
                }
                catch
                {
                }
            }
        }
       
        private void initPanelViewPdf()
        {
            Application.DoEvents();
            this._currentPageIndex = 0;
            this._pdfPageRenderer = (PdfPageRenderer)null;
            this._totalPage = 0;
            this._selectedPageNum = -1;
            this.arrClass10 = (KichThuoc[])null;
            this.zoomType = ZoomType.Fixed;
            this._hasOpenFile = false;
            this.txtCurrentPage.Text = "0";
            this.lbTotalPage.Text = "/ 0";
            this.setEnableButton(false);
        }

        internal int checkSinglePage() => 1;

        private KichThuoc[] CalculatePageSizes(
          SizeF[] lstSizePage,
          ViewType viewType)
        {
            Math.Min(this.checkSinglePage(), lstSizePage.Length);
            List<KichThuoc> calculatedPageSizes = new List<KichThuoc>();
            int paddingVertical = this.padding.Top + this.padding.Bottom;
            foreach (SizeF pageSize in lstSizePage)
            {
                KichThuoc calculatedPageSize = new KichThuoc(pageSize, (float)paddingVertical, 0.0f);
                calculatedPageSizes.Add(calculatedPageSize);
            }
            return calculatedPageSizes.ToArray();
        }

        private void configPbView()
        {
            this.pbView.SizeMode = PictureBoxSizeMode.Zoom;
            this.pbView.Dock = DockStyle.None;
            this.pbView.Height = this.ClientPanel.Height - SystemInformation.HorizontalScrollBarHeight;
            this.pbView.Width = this.ClientPanel.Width - SystemInformation.VerticalScrollBarWidth;
            this.pbView.Location = new Point(0, 0);
            this.ClientPanel.Invalidate();
            this.clientPanelWidth = this.ClientPanel.Width;
        }

        private void revisionShow()
        {
            this._hasOpenFile = false;
            SizeF[] lstSizePage = XuLyPDF.LayKichThuocTrang(this._absolutePath, this.imageRotation);
            this.arrClass10 = this.CalculatePageSizes(lstSizePage, ViewType.SinglePage);
            this._totalPage = lstSizePage.Length;
            this._pdfPageRenderer = new PdfPageRenderer(this._absolutePath, this._totalPage, new PageLayout(1, ViewType.SinglePage, this._zoomStep, this.imageRotation, 1f), false);
            this._currentPageIndex = 1;
            this.configPbView();
            PictureBox oPictureBox = this.PbView();
            ImageUtil.TinhLaiViTriTrang(ref oPictureBox);
            oPictureBox.Refresh();
            this.initPbView();
            this.setEnableButton(true);
            this._hasOpenFile = true;
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
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(RevisionViewer));
            this.ClientPanel = new Panel();
            this.lbTotalPage = new Label();
            this.controlPanel = new Panel();
            this.txtCurrentPage = new TextBox();
            this.pbView = new PictureBox();
            this.label3 = new Label();
            this.label1 = new Label();
            this.label5 = new Label();
            this.label2 = new Label();
            this.btnPrint = new Button();
            this.btnFitWidth = new Button();
            this.btnFitPage = new Button();
            this.btnZoomIn = new Button();
            this.btnZoomOut = new Button();
            this.btnPageUp = new Button();
            this.btnPageDown = new Button();
            this.ClientPanel.SuspendLayout();
            this.controlPanel.SuspendLayout();
            ((ISupportInitialize)this.pbView).BeginInit();
            this.SuspendLayout();
            this.ClientPanel.AutoScroll = true;
            this.ClientPanel.BackColor = SystemColors.ControlDark;
            this.ClientPanel.Controls.Add((Control)this.pbView);
            this.ClientPanel.Dock = DockStyle.Fill;
            this.ClientPanel.Location = new Point(0, 39);
            this.ClientPanel.Name = "ClientPanel";
            this.ClientPanel.Size = new Size(412, 188);
            this.ClientPanel.TabIndex = 4;
            this.lbTotalPage.AutoSize = true;
            this.lbTotalPage.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.lbTotalPage.Location = new Point(136, 10);
            this.lbTotalPage.Name = "lbTotalPage";
            this.lbTotalPage.Size = new Size(22, 16);
            this.lbTotalPage.TabIndex = 2;
            this.lbTotalPage.Text = "/ 0";
            this.controlPanel.BorderStyle = BorderStyle.FixedSingle;
            this.controlPanel.Controls.Add((Control)this.label3);
            this.controlPanel.Controls.Add((Control)this.label1);
            this.controlPanel.Controls.Add((Control)this.label5);
            this.controlPanel.Controls.Add((Control)this.label2);
            this.controlPanel.Controls.Add((Control)this.lbTotalPage);
            this.controlPanel.Controls.Add((Control)this.txtCurrentPage);
            this.controlPanel.Controls.Add((Control)this.btnPrint);
            this.controlPanel.Controls.Add((Control)this.btnFitWidth);
            this.controlPanel.Controls.Add((Control)this.btnFitPage);
            this.controlPanel.Controls.Add((Control)this.btnZoomIn);
            this.controlPanel.Controls.Add((Control)this.btnZoomOut);
            this.controlPanel.Controls.Add((Control)this.btnPageUp);
            this.controlPanel.Controls.Add((Control)this.btnPageDown);
            this.controlPanel.Dock = DockStyle.Top;
            this.controlPanel.Location = new Point(0, 0);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new Size(412, 39);
            this.controlPanel.TabIndex = 3;
            this.txtCurrentPage.BorderStyle = BorderStyle.FixedSingle;
            this.txtCurrentPage.Font = new Font("Times New Roman", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.txtCurrentPage.Location = new Point(96, 6);
            this.txtCurrentPage.Multiline = true;
            this.txtCurrentPage.Name = "txtCurrentPage";
            this.txtCurrentPage.Size = new Size(37, 25);
            this.txtCurrentPage.TabIndex = 1;
            this.txtCurrentPage.Text = "0";
            this.txtCurrentPage.TextAlign = HorizontalAlignment.Center;
            this.txtCurrentPage.KeyPress += new KeyPressEventHandler(this.txtCurrentPage_KeyPress);
            this.txtCurrentPage.Leave += new EventHandler(this.txtCurrentPage_Leave);
            this.pbView.Dock = DockStyle.Fill;
            this.pbView.Location = new Point(0, 0);
            this.pbView.Name = "pbView";
            this.pbView.Size = new Size(412, 188);
            this.pbView.TabIndex = 0;
            this.pbView.TabStop = false;
            this.label3.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label3.Image = (Image)componentResourceManager.GetObject("label3.Image");
            this.label3.Location = new Point(80, 6);
            this.label3.Name = "label3";
            this.label3.Size = new Size(10, 24);
            this.label3.TabIndex = 2;
            this.label1.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label1.Image = (Image)componentResourceManager.GetObject("label1.Image");
            this.label1.Location = new Point(354, 6);
            this.label1.Name = "label1";
            this.label1.Size = new Size(10, 24);
            this.label1.TabIndex = 2;
            this.label5.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label5.Image = (Image)componentResourceManager.GetObject("label5.Image");
            this.label5.Location = new Point(263, 6);
            this.label5.Name = "label5";
            this.label5.Size = new Size(10, 24);
            this.label5.TabIndex = 2;
            this.label2.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label2.Image = (Image)componentResourceManager.GetObject("label2.Image");
            this.label2.Location = new Point(171, 6);
            this.label2.Name = "label2";
            this.label2.Size = new Size(10, 24);
            this.label2.TabIndex = 2;
            this.btnPrint.FlatAppearance.BorderColor = Color.Silver;
            this.btnPrint.FlatAppearance.MouseDownBackColor = Color.Gray;
            this.btnPrint.FlatAppearance.MouseOverBackColor = Color.Gray;
            this.btnPrint.FlatStyle = FlatStyle.Flat;
            this.btnPrint.Image = (Image)Resources.printer;
            this.btnPrint.Location = new Point(369, 3);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new Size(32, 31);
            this.btnPrint.TabIndex = 0;
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Visible = false;
            this.btnFitWidth.FlatAppearance.BorderColor = Color.Silver;
            this.btnFitWidth.FlatAppearance.MouseDownBackColor = Color.Gray;
            this.btnFitWidth.FlatAppearance.MouseOverBackColor = Color.Gray;
            this.btnFitWidth.FlatStyle = FlatStyle.Flat;
            this.btnFitWidth.Image = (Image)Resources.fitwidth;
            this.btnFitWidth.Location = new Point(316, 3);
            this.btnFitWidth.Name = "btnFitWidth";
            this.btnFitWidth.Size = new Size(32, 31);
            this.btnFitWidth.TabIndex = 0;
            this.btnFitWidth.UseVisualStyleBackColor = true;
            this.btnFitWidth.Click += new EventHandler(this.btnFitWidth_Click);
            this.btnFitPage.FlatAppearance.BorderColor = Color.Silver;
            this.btnFitPage.FlatAppearance.MouseDownBackColor = Color.Gray;
            this.btnFitPage.FlatAppearance.MouseOverBackColor = Color.Gray;
            this.btnFitPage.FlatStyle = FlatStyle.Flat;
            this.btnFitPage.Image = (Image)Resources.fitpage;
            this.btnFitPage.Location = new Point(278, 3);
            this.btnFitPage.Name = "btnFitPage";
            this.btnFitPage.Size = new Size(32, 31);
            this.btnFitPage.TabIndex = 0;
            this.btnFitPage.UseVisualStyleBackColor = true;
            this.btnFitPage.Click += new EventHandler(this.btnFitPage_Click);
            this.btnZoomIn.FlatAppearance.BorderColor = Color.Silver;
            this.btnZoomIn.FlatAppearance.MouseDownBackColor = Color.Gray;
            this.btnZoomIn.FlatAppearance.MouseOverBackColor = Color.Gray;
            this.btnZoomIn.FlatStyle = FlatStyle.Flat;
            this.btnZoomIn.Image = (Image)Resources.zoomin;
            this.btnZoomIn.Location = new Point(225, 3);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new Size(32, 31);
            this.btnZoomIn.TabIndex = 0;
            this.btnZoomIn.UseVisualStyleBackColor = true;
            this.btnZoomIn.Click += new EventHandler(this.btnZoomIn_Click);
            this.btnZoomOut.FlatAppearance.BorderColor = Color.Silver;
            this.btnZoomOut.FlatAppearance.MouseDownBackColor = Color.Gray;
            this.btnZoomOut.FlatAppearance.MouseOverBackColor = Color.Gray;
            this.btnZoomOut.FlatStyle = FlatStyle.Flat;
            this.btnZoomOut.Image = (Image)Resources.zoomout;
            this.btnZoomOut.Location = new Point(187, 3);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new Size(32, 31);
            this.btnZoomOut.TabIndex = 0;
            this.btnZoomOut.UseVisualStyleBackColor = true;
            this.btnZoomOut.Click += new EventHandler(this.btnZoomOut_Click);
            this.btnPageUp.FlatAppearance.BorderColor = Color.Silver;
            this.btnPageUp.FlatAppearance.MouseDownBackColor = Color.Gray;
            this.btnPageUp.FlatAppearance.MouseOverBackColor = Color.Gray;
            this.btnPageUp.FlatStyle = FlatStyle.Flat;
            this.btnPageUp.Image = (Image)Resources.arrowup32;
            this.btnPageUp.Location = new Point(5, 3);
            this.btnPageUp.Name = "btnPageUp";
            this.btnPageUp.Size = new Size(32, 31);
            this.btnPageUp.TabIndex = 0;
            this.btnPageUp.UseVisualStyleBackColor = true;
            this.btnPageUp.Click += new EventHandler(this.btnPageUp_Click);
            this.btnPageDown.FlatAppearance.BorderColor = Color.Silver;
            this.btnPageDown.FlatAppearance.MouseDownBackColor = Color.Gray;
            this.btnPageDown.FlatAppearance.MouseOverBackColor = Color.Gray;
            this.btnPageDown.FlatStyle = FlatStyle.Flat;
            this.btnPageDown.Image = (Image)Resources.arrowdown32;
            this.btnPageDown.Location = new Point(43, 3);
            this.btnPageDown.Name = "btnPageDown";
            this.btnPageDown.Size = new Size(32, 31);
            this.btnPageDown.TabIndex = 0;
            this.btnPageDown.UseVisualStyleBackColor = true;
            this.btnPageDown.Click += new EventHandler(this.btnPageDown_Click);
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add((Control)this.ClientPanel);
            this.Controls.Add((Control)this.controlPanel);
            this.Name = "RevisionViewer";
            this.Size = new Size(412, 227);
            this.SizeChanged += new EventHandler(this.Form_SizeChanged);
            this.ClientPanel.ResumeLayout(false);
            this.controlPanel.ResumeLayout(false);
            this.controlPanel.PerformLayout();
            ((ISupportInitialize)this.pbView).EndInit();
            this.ResumeLayout(false);
        }
    }
}
