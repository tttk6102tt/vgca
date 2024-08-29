using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using Plugin.UI.classess;
using Plugin.UI.PDF;
using Plugin.UI.Helper;
using Plugin.UI.Configurations;
using Sign.X509;
using Sign.itext.pdf;
using Sign.itext.text.pdf;
using Plugin.UI.Properties;
using Sign.PDF;

namespace Plugin.UI
{
    partial class ucPDFViewer : UserControl
    {
        private PdfPageRenderer pdfPageRenderer;
        private bool _hasOpenFile;
        private bool btnSignClicked;
        private Rectangle rectangle1 = Rectangle.Empty;
        private int _selectedPageNum = -1;
        private float _zoomStep = 0.25f;
        private float _minZoomFactor = 0.15f;
        private float _maxZoomFactor = 6f;
        private int _panel2_width;
        private const int c35ccc5c29af1c4933bf3c466fb1a7933 = 5;
        private int countMouseWeelLastPosition;
        private int countMouseWeelHeadPosition;
        private int cursor_X;
        private int cursor_Y;
        private int c18824f83eb56c23b08af29ce49f28835;
        private int c178a4eda9e0439cf469a661bb8a40c9a;
        private string fileName;
        private ImageRotation gocXoayAnh;
        private int _totalPages;
        private KichThuoc[] _listKichThuoc;
        private ViewType viewType1;
        private Padding padding1 = new Padding(0, 2, 4, 2);
        private float _khoangTrangGiuaTrang;
        private int _currentPageIndex;
        private ZoomType zoomType;
        //lstSignInfo
        private List<SignatureInfo> lstSignInfo = new List<SignatureInfo>();
        private IContainer container;
        private Panel controlPanel;
        private Button btnPageDown;
        private Button btnPageUp;
        private TextBox txtCurrentPage;
        private Label lbTotalPage;
        private Label label2;
        private Label label3;
        private Button btnZoomIn;
        private Button btnZoomOut;
        private Button btnFitWidth;
        private Button btnFitPage;
        private Label label5;
        private Panel ClientPanel;
        private SplitContainer splitContainer;
        private TreeView treeView;
        private Panel signatureBarPanel;
        private Button btnSignaturesHide;
        private Button btnSign;
        private Label label1;
        private Button btnPrint;
        private Label label4;
        private Button btnOpenFile;
        private Panel panelLogo;
        private Label label6;
        private Panel panel2;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private LinkLabel lkOpenfile;
        private Panel panel1;
        private Label label9;
        private Label label8;
        private Label label7;
        private LinkLabel lbConvertFromWord;
        private Label label10;
        private Button btnVerify;
        private Panel LstSignaturesPanel;
        private ImageList imageList1;
        private Panel pnInfo;
        private Label lbInfo;
        private Button btnRotation;
        private EventHandler event_FileLoaded;// ADD
        private EventHandler Event_FileClosed; //ADD
        private EventHandler Event_ZoomTypeChanged; //ADD


        public ucPDFViewer()
        {
            this.InitializeComponent();
            this.setEnableButton(false);
            this.BackColor = SystemColors.Control;
            this.splitContainer.Panel1Collapsed = true;
            this.splitContainer.Panel2.MouseWheel += new MouseEventHandler(this.Panel2_MouseWheel);
        }

        public event EventHandler FileLoaded
        {
            add
            {
                EventHandler eventHandler = this.event_FileLoaded;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.event_FileLoaded, comparand + value, comparand);
                }
                while (eventHandler != comparand);

            }
            remove
            {
                EventHandler eventHandler = this.event_FileLoaded;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.event_FileLoaded, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        public event EventHandler FileClosed
        {
            add
            {
                EventHandler eventHandler = this.Event_FileClosed;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.Event_FileClosed, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler eventHandler = this.Event_FileClosed;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.Event_FileClosed, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        public event EventHandler ZoomTypeChanged
        {
            add
            {
                EventHandler eventHandler = this.Event_ZoomTypeChanged;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.Event_ZoomTypeChanged, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler eventHandler = this.Event_ZoomTypeChanged;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.Event_ZoomTypeChanged, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        public string CurrentFilename
        {
            get => this.fileName;
            private set => this.fileName = value;
        }

        public ImageRotation Rotation
        {
            get => this.gocXoayAnh;
            set => this.gocXoayAnh = value;
        }

        public int TotalPages
        {
            get => this._totalPages;
            private set => this._totalPages = value;
        }

        internal KichThuoc[] LstKichThuoc => this._listKichThuoc;

        public ViewType ViewType => this.viewType1;

        public Padding PageMargin
        {
            get => this.padding1;
            set => this.padding1 = value;
        }

        public int HorizontalMargin => this.PageMargin.Right;

        public int CurrentPageIndex => this._currentPageIndex;

        public ZoomType ZoomType
        {
            get => this.zoomType;
            private set
            {
                if (this.zoomType == value)
                    return;
                this.zoomType = value;
                if (this.Event_ZoomTypeChanged == null)
                    return;
                else
                {
                    this.Event_ZoomTypeChanged(this, EventArgs.Empty);
                }
            }
        }

        public float CurrentZoom
        {
            get
            {
                if (this.pdfPageRenderer == null)
                    return 1f;
                else
                {
                    return this.pdfPageRenderer.PageLayout.ImageResolution;
                }
            }
        }

        public float ZoomStep
        {
            get => this._zoomStep;
            set => this._zoomStep = value;
        }

        public float MinZoomFactor
        {
            get => this._minZoomFactor;
            set => this._minZoomFactor = value;
        }

        public float MaxZoomFactor
        {
            get => this._maxZoomFactor;
            set => this._maxZoomFactor = value;
        }

        private void Panel2_MouseWheel(
          object sender,
          MouseEventArgs e)
        {
            Application.DoEvents();
            if (e.Delta == 0)
            {
                
            }
            else
            {
                if (!this._hasOpenFile)
                    return;
                bool flag1 = false;
                bool flag2 = false;
                if (this.splitContainer.Panel2.VerticalScroll.Visible)
                {
                    if (this.splitContainer.Panel2.VerticalScroll.Value >= Math.Abs(this.splitContainer.Panel2.VerticalScroll.Maximum - this.splitContainer.Panel2.VerticalScroll.LargeChange))
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
                    else if (this.splitContainer.Panel2.VerticalScroll.Value <= 0)
                    {
                        ++this.countMouseWeelHeadPosition;
                        int num;
                        if (e.Delta > 0)
                        {
                            num = this.countMouseWeelHeadPosition >= 5 ? 1 : 0;
                        }
                        else
                            num = 0;
                        flag2 = num != 0;
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
                    this.GotoNextPage();
                    this.countMouseWeelLastPosition = 0;
                    return;
                }
                if (!flag2)
                    return;
                else
                {
                    try
                    {
                        this.GotoPreviousPage();
                        this.countMouseWeelHeadPosition = 0;
                        return;
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }

        public void Close()
        {
            this.fileName = string.Empty;
            this._currentPageIndex = 0;
            this.pdfPageRenderer = (PdfPageRenderer)null;
            this._totalPages = 0;
            this._selectedPageNum = -1;
            this._listKichThuoc = (KichThuoc[])null;
            this.zoomType = ZoomType.Fixed;
            this._hasOpenFile = false;
            this.txtCurrentPage.Text = "0";
            this.lbTotalPage.Text = "/ 0";
            this.splitContainer.Panel1Collapsed = true;
            this.splitContainer.Panel2.Controls.Clear();
            this.splitContainer.Panel2.Controls.Add((Control)this.panelLogo);
            this.splitContainer.Panel2.AutoScroll = true;
            this.setEnableButton(false);
            // ISSUE: reference to a compiler-generated field
            if (this.Event_FileClosed != null)
            {
                // ISSUE: reference to a compiler-generated field
                this.Event_FileClosed((object)this, EventArgs.Empty);

            }
            GC.Collect();
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
            this.txtCurrentPage.Text = this._totalPages.ToString();
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
            int verticalScrollbarWidth = this.splitContainer.Panel2.VerticalScroll.Visible
                                           ? SystemInformation.VerticalScrollBarWidth
                                           : 0;
            int panelWidthAdjustment = verticalScrollbarWidth;
            float zoomFactor = (float)(this.splitContainer.Panel2.ClientSize.Width - panelWidthAdjustment) / this.LstKichThuoc[this._currentPageIndex - 1].KichThuocSauCung.Width;
            KichThuoc currentPageDimensions = this.LstKichThuoc[this._currentPageIndex - 1];
            Size panelClientSize;
            if (panelWidthAdjustment == 0)
            {
                double estimatedHeight = (double)currentPageDimensions.KichThuocGoc.Height * (double)zoomFactor + (double)currentPageDimensions.PaddingDoc;
                panelClientSize = this.splitContainer.Panel2.ClientSize;
                double panelHeight = (double)panelClientSize.Height;
                if (estimatedHeight >= panelHeight)
                {
                    panelWidthAdjustment += SystemInformation.VerticalScrollBarWidth;
                }
            }
            int num5 = panelWidthAdjustment + 2;
            panelClientSize = this.splitContainer.Panel2.ClientSize;
            this.Zoom((float)(panelClientSize.Width - num5) / this.LstKichThuoc[this._currentPageIndex - 1].KichThuocSauCung.Width);
            this.ZoomType = ZoomType.FitToWidth;
        }

        public void ZoomToHeight()
        {
            int horizontalScrollbarHeight = this.splitContainer.Panel2.HorizontalScroll.Visible ? SystemInformation.HorizontalScrollBarHeight : 0;
            Size panelClientSize = this.splitContainer.Panel2.ClientSize;
            double availableHeight = (double)(panelClientSize.Height - horizontalScrollbarHeight);
            SizeF finalPageSize = this.LstKichThuoc[this._currentPageIndex - 1].KichThuocSauCung;
            
            float zoomFactor = (float)(availableHeight / finalPageSize.Height);
            KichThuoc kichThuoc = this.LstKichThuoc[this._currentPageIndex - 1];
            if (horizontalScrollbarHeight == 0)
            {
                finalPageSize = kichThuoc.KichThuocGoc;
                double estimatedImageWidth = (double)finalPageSize.Width * (double)zoomFactor + (double)kichThuoc.PaddingNgang;
                panelClientSize = this.splitContainer.Panel2.ClientSize;
                double width = (double)panelClientSize.Width;
                if (estimatedImageWidth >= width)
                {
                    horizontalScrollbarHeight += SystemInformation.HorizontalScrollBarHeight;
                }
            }
            panelClientSize = this.splitContainer.Panel2.ClientSize;
            availableHeight = (double)(panelClientSize.Height - horizontalScrollbarHeight);
            finalPageSize = this.LstKichThuoc[this._currentPageIndex - 1].KichThuocSauCung;
            
            this.Zoom((float)(availableHeight / finalPageSize.Height));
            this.ZoomType = ZoomType.FitToHeight;
        }

        public void ZoomIn()
        {
            Application.DoEvents();
            this.pdfPageRenderer.PageLayout.ImageResolution += this._zoomStep;
            if ((double)this.pdfPageRenderer.PageLayout.ImageResolution > (double)this._maxZoomFactor)
            {
                this.pdfPageRenderer.PageLayout.ImageResolution = this._maxZoomFactor;
            }
            PictureBox pPicBox = this.FindPictrueBoxInPanel2(this._currentPageIndex.ToString());
            ImageUtil.PictureBoxZoomIn(ref pPicBox);
            pPicBox.Refresh();
            this.HienThiChuKyTrenTrang();
            this.zoomType = ZoomType.Fixed;
        }

        public void ZoomOut()
        {
            Application.DoEvents();
            this.pdfPageRenderer.PageLayout.ImageResolution -= this._zoomStep;
            if ((double)this.pdfPageRenderer.PageLayout.ImageResolution < (double)this._minZoomFactor)
            {
                this.pdfPageRenderer.PageLayout.ImageResolution = this._minZoomFactor;
            }
            PictureBox pPicBox = this.FindPictrueBoxInPanel2(this._currentPageIndex.ToString());
            ImageUtil.PictureBoxZoomOut(ref pPicBox);
            pPicBox.Refresh();
            this.HienThiChuKyTrenTrang();
            this.ZoomType = ZoomType.Fixed;
        }

        public void Zoom(float zoomFactor)
        {
            Application.DoEvents();
            this.pdfPageRenderer.PageLayout.ImageResolution = zoomFactor;
            PictureBox pPicBox = this.FindPictrueBoxInPanel2(this._currentPageIndex.ToString());
            ImageUtil.PictureBoxZoomActual(ref pPicBox);
            pPicBox.Refresh();
            this.HienThiChuKyTrenTrang();
            this.ZoomType = ZoomType.Fixed;
        }

        public void Rotate(ImageRotation rot)
        {
            this.Rotation = rot;
            SizeF[] sizeOfFilePdf = XuLyPDF.LayKichThuocTrang(this.fileName, this.Rotation);
            this._listKichThuoc = this.TinhToanKichThuocTrang(sizeOfFilePdf, this.viewType1);
            this._totalPages = sizeOfFilePdf.Length;
            this.pdfPageRenderer = new PdfPageRenderer(this.fileName, this._totalPages, new PageLayout(this.checkSinglePage(), this.ViewType, this._khoangTrangGiuaTrang, this.Rotation, 1f), false);
            this.lstSignInfo = PdfHelper.getListSignInfo(this.fileName);
            this.loadListSignatureOfFile();
            PictureBox oPictureBox = this.FindPictrueBoxInPanel2("SinglePicBox");
            ImageUtil.TinhLaiViTriTrang(ref oPictureBox);
            oPictureBox.Refresh();
            this.HienThiChuKyTrenTrang();
        }

        private void setEnableButton(bool enabled)
        {
            IEnumerator enumerator = this.controlPanel.Controls.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    ((Control)enumerator.Current).Enabled = enabled;
            }
            finally
            {
                if (enumerator is IDisposable disposable)
                    disposable.Dispose();
            }
            this.btnOpenFile.Enabled = true;
            this.btnPrint.Enabled = false;
        }

        private void XuLySauKhiThayDoiKichThuoc(
          object sender,
          EventArgs e)
        {
            Application.DoEvents();
            this.splitContainer.Panel2.AutoScrollMinSize = this.panelLogo.Size;
           
            if (this.panelLogo.Width < this.splitContainer.Panel2.ClientSize.Width)
            {
                this.panelLogo.Left = this.splitContainer.Panel2.ClientSize.Width / 2 - this.panelLogo.Width / 2;
            }
            else
            {
                this.panelLogo.Left = this.splitContainer.Panel2.AutoScrollPosition.X;
            }
            
            Size clientSize = this.splitContainer.Panel2.ClientSize;
           
            if (this.panelLogo.Height < clientSize.Height)
            {
                this.panelLogo.Top = this.splitContainer.Panel2.ClientSize.Height / 2 - this.panelLogo.Height / 2;
            } 
            else
            {
                this.panelLogo.Top = this.splitContainer.Panel2.AutoScrollPosition.Y;
            }
        }

        private void initPanelViewPdf()
        {
            Application.DoEvents();
            this.fileName = string.Empty;
            this._currentPageIndex = 0;
            this.pdfPageRenderer = (PdfPageRenderer)null;
            this._totalPages = 0;
            this._selectedPageNum = -1;
            this._listKichThuoc = (KichThuoc[])null;
            this.zoomType = ZoomType.Fixed;
            this._hasOpenFile = false;
            this.txtCurrentPage.Text = "0";
            this.lbTotalPage.Text = "/ 0";
            this.splitContainer.Panel1Collapsed = true;
            this.treeView.Nodes.Clear();
            this.setEnableButton(false);
            GC.Collect();
        }

        public void OpenFile(string pdfFilename)
        {
            if (!System.IO.File.Exists(pdfFilename))
            {
                int num = (int)MessageBox.Show("Không tìm thấy đường dẫn tệp.", "HiPT Sign PDF", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            else
            {
                try
                {
                    this.initPanelViewPdf();
                    this.fileName = pdfFilename;
                    this._hasOpenFile = false;
                    this.initPreviewPdf(pdfFilename);
                    this.setEnableButton(true);
                    // ISSUE: reference to a compiler-generated field
                    if (this.event_FileLoaded != null)
                    {
                        // ISSUE: reference to a compiler-generated field
                        this.event_FileLoaded((object)this, EventArgs.Empty);
                    }
                    this._hasOpenFile = true;
                }
                catch (AccessViolationException ex)
                {
                    int num1 = (int)MessageBox.Show(@"Không thể mở tệp!Hay kiểm tra lại các nguyên nhân sau:
                                                     - Định dạng tệp đã bị thay đổi.
                                                     - Tệp đã được mã hóa.", "HiPT Sign PDF", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    int num2 = (int)MessageBox.Show(ex.ToString());
                }
                catch (Exception ex)
                {
                    int num = (int)MessageBox.Show("Không thể mở tệp!Hay liên hệ với nhà phát triển để được trợ giúp.", "HiPT Sign PDF", MessageBoxButtons.OK, MessageBoxIcon.Question);
                }
            }
        }

        private void initPreviewPdf(string savedFileName)
        {
            SizeF[] sizeOfFilePdf = XuLyPDF.LayKichThuocTrang(savedFileName, this.Rotation);
            this._listKichThuoc = this.TinhToanKichThuocTrang(sizeOfFilePdf, this.viewType1);
            this._totalPages = sizeOfFilePdf.Length;
            this.pdfPageRenderer = new PdfPageRenderer(savedFileName, this._totalPages, new PageLayout(this.checkSinglePage(), this.ViewType, this._khoangTrangGiuaTrang, this.Rotation, 1f), false);
            // Chưa tạo
            this.lstSignInfo = PdfHelper.getListSignInfo(savedFileName);
            this._currentPageIndex = 1;
            this.loadListSignatureOfFile();
            PictureBox oPictureBox = this.FindPictrueBoxInPanel2("SinglePicBox");
            ImageUtil.TinhLaiViTriTrang(ref oPictureBox);
            oPictureBox.Refresh();
            this.HienThiChuKyTrenTrang();
        }

        private void HienThiChuKyTrenTrang()
        {
            this.btnSignClicked = false;
            this.btnSign.Enabled = true;
            this.rectangle1 = Rectangle.Empty;
            PictureBox khungAnhTrangTrongPanel = this.FindPictrueBoxInPanel2(this._currentPageIndex.ToString());
            if (this._selectedPageNum != this._currentPageIndex)
            {
                this.setStatusButtonOfPdfViewer();
                this.setCurrentPageSelected();
                Image img = this.getPictureBoxInPageNum(this._currentPageIndex, ref khungAnhTrangTrongPanel);
                ImageUtil.ApDungXoay(ref img, (int)this.Rotation);
                ucPDFViewer.HienThiAnhChuKy(img, ref khungAnhTrangTrongPanel);
                khungAnhTrangTrongPanel.Refresh();
                foreach (SignatureInfo pdfSignatureInfo in this.lstSignInfo)
                {
                    Label label = this.setInfoSignatureName(pdfSignatureInfo.SignatureName);
                    if (this._currentPageIndex == pdfSignatureInfo.PageIndex)
                    {
                        try
                        {
                            SizeF kichThuocTrang = this.getSizePageSelected(this._currentPageIndex);
                            float tyLeChieuRong = (float)khungAnhTrangTrongPanel.Image.Width / kichThuocTrang.Width;
                            float tyLeChieuCao = (float)khungAnhTrangTrongPanel.Image.Height / kichThuocTrang.Height;
                            int viTriNhanTheoChieuDoc, chieuRongNhan, chieuCaoNhan, viTriNhanTheoChieuNgang, canhTraiTamThoi, canhTrenTamThoi;
                            double chieuCaoTamThoi;
                            double chieuRongTamThoi;
                            switch (this.gocXoayAnh)
                            {
                                case ImageRotation.Rotate90:
                                    viTriNhanTheoChieuDoc = (int)((double)tyLeChieuCao * (double)pdfSignatureInfo.Position.Left);
                                    int toaDoCanhTrenXoay = (int)((double)tyLeChieuRong * (double)pdfSignatureInfo.Position.Top);

                                    double chieuCaoKhungChuKyGoc = (double)pdfSignatureInfo.Position.Height;
                                    chieuRongNhan = (int)(tyLeChieuRong * chieuCaoKhungChuKyGoc);

                                    double chieuRongKhungChuKyGoc = (double)pdfSignatureInfo.Position.Width;
                                    chieuCaoNhan = (int)(tyLeChieuCao * chieuRongKhungChuKyGoc);

                                    viTriNhanTheoChieuNgang = toaDoCanhTrenXoay - chieuRongNhan;
                                    break;
                                case ImageRotation.Rotate180:
                                    canhTraiTamThoi = (int)((double)tyLeChieuRong * (double)pdfSignatureInfo.Position.Left);
                                    canhTrenTamThoi = (int)((double)tyLeChieuCao * (double)pdfSignatureInfo.Position.Top);
                                    
                                    chieuRongTamThoi = (double)pdfSignatureInfo.Position.Width;
                                    chieuRongNhan = (int)(tyLeChieuRong * chieuRongTamThoi);
                                   
                                    chieuCaoTamThoi = (double)pdfSignatureInfo.Position.Height;
                                    chieuCaoNhan = (int)(tyLeChieuRong * chieuCaoTamThoi);

                                    viTriNhanTheoChieuNgang = khungAnhTrangTrongPanel.Image.Width - chieuRongNhan - canhTraiTamThoi;
                                    viTriNhanTheoChieuDoc = canhTrenTamThoi - chieuCaoNhan;
                                    break;
                                case ImageRotation.Rotate270:
                                    canhTraiTamThoi = (int)((double)tyLeChieuCao * (double)pdfSignatureInfo.Position.Left);
                                    canhTrenTamThoi = (int)((double)tyLeChieuRong * (double)pdfSignatureInfo.Position.Top);
                                    chieuCaoTamThoi = (double)pdfSignatureInfo.Position.Height;
                                    chieuRongNhan = (int)(tyLeChieuRong * chieuCaoTamThoi);

                                    chieuRongTamThoi = (double)pdfSignatureInfo.Position.Width;
                                    chieuCaoNhan = (int)(tyLeChieuCao * chieuRongTamThoi);

                                    viTriNhanTheoChieuDoc = khungAnhTrangTrongPanel.Image.Height - chieuCaoNhan - canhTraiTamThoi;
                                    viTriNhanTheoChieuNgang = khungAnhTrangTrongPanel.Image.Width - canhTrenTamThoi;

                                    break;
                                default:
                                    viTriNhanTheoChieuNgang = (int)((double)tyLeChieuRong * (double)pdfSignatureInfo.Position.Left);
                                    canhTrenTamThoi = (int)((double)tyLeChieuCao * (double)pdfSignatureInfo.Position.Top);
                                    chieuRongTamThoi = (double)pdfSignatureInfo.Position.Width;
                                    chieuRongNhan = (int)(tyLeChieuRong * chieuRongTamThoi);
                                    chieuCaoTamThoi = (double)pdfSignatureInfo.Position.Height;
                                    chieuCaoNhan = (int)(tyLeChieuRong * chieuCaoTamThoi);
                                    viTriNhanTheoChieuDoc = khungAnhTrangTrongPanel.Image.Height - canhTrenTamThoi;

                                    break;
                            }
                            label.AutoSize = false;
                            label.Visible = true;
                            label.Parent = (Control)khungAnhTrangTrongPanel;
                            label.BackColor = Color.Transparent;
                            label.Width = chieuRongNhan;
                            label.Height = chieuCaoNhan;
                            label.Location = new Point(viTriNhanTheoChieuNgang, viTriNhanTheoChieuDoc);
                            label.Cursor = Cursors.Hand;
                            continue;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    else
                        label.Visible = false;
                }
                this.splitContainer.Panel2.Focus();
                this.splitContainer.Panel2.VerticalScroll.Value = 0;
            }
            else
                ucPDFViewer.HienThiAnhChuKy(khungAnhTrangTrongPanel.Image, ref khungAnhTrangTrongPanel);
        }

        private Image getPictureBoxInPageNum(
          int currentPageNum,
          ref PictureBox picBox)
        {

            this.Cursor = Cursors.WaitCursor;
            Bitmap bitmap = XuLyPDF.LayHinhAnhTrang(this.fileName, currentPageNum, this.pdfPageRenderer.PageLayout.ImageResolution);

            this.Cursor = Cursors.Default;
            return (Image)bitmap;
        }

        private void setStatusButtonOfPdfViewer()
        {
            if (this._currentPageIndex >= this._totalPages)
            {
                this._currentPageIndex = this._totalPages;
                this.btnPageDown.Enabled = false;
            }
            else if (this._currentPageIndex <= 1)
            {
                this._currentPageIndex = 1;
                this.btnPageUp.Enabled = false;
            }
            if (this._currentPageIndex < this._totalPages)
            {
                if (this._totalPages > 1)
                {
                    if (this._currentPageIndex > 1)
                    {
                        this.btnPageDown.Enabled = true;
                        this.btnPageUp.Enabled = true;
                    }
                }
            }
            if (this._currentPageIndex == this._totalPages)
            {
                if (this._totalPages > 1)
                {
                    if (this._currentPageIndex > 1)
                    {
                        this.btnPageUp.Enabled = true;
                    }
                }
            }
            if (this._currentPageIndex == 1)
            {
                if (this._totalPages > 1)
                {
                    this.btnPageDown.Enabled = true;
                }
            }
            if (this._totalPages != 1)
                return;
            this.btnPageDown.Enabled = false;
            this.btnPageUp.Enabled = false;
        }

        private static void HienThiAnhChuKy(
          Image anhChuKy,
          ref PictureBox khungAnh)
        {
            if (anhChuKy == null)
                return;
            ucPDFViewer.DieuChinhHuongPictureBox(ref khungAnh, anhChuKy);
            ucPDFViewer.CapNhatKichThuocKhungAnh(ref khungAnh, anhChuKy);
            ImageUtil.TinhLaiViTriTrang(ref khungAnh);
            khungAnh.Image = anhChuKy;
        }

        private static void CapNhatKichThuocKhungAnh(
          ref PictureBox khungAnh,
          Image anhChuKy)
        {
            if (anhChuKy != null)
            {
                khungAnh.Width = anhChuKy.Width;
                khungAnh.Height = anhChuKy.Height;
            }
            else
            {
                khungAnh.Width = khungAnh.Image.Width;
                khungAnh.Height = khungAnh.Image.Height;
            }
        }
        /// <summary>
        /// c8e547949f0cb589f4ae8ab9e7d53ba17
        /// </summary>
        /// <param name="khungAnh"></param>
        /// <param name="anhChuKy"></param>
        private static void DieuChinhHuongPictureBox(
          ref PictureBox khungAnh,
          Image anhChuKy)
        {
            int chieuCaoKhung = khungAnh.Height;
            int chieuRongKhung = khungAnh.Width;
            int chieuCaoAnh = anhChuKy.Height;
            int chieuRongAnh = anhChuKy.Width;
            if (chieuRongKhung > chieuCaoKhung)
            {
                
            }
            if (chieuRongKhung >= chieuCaoKhung)
                return;
            if (chieuRongAnh <= chieuCaoAnh)
                return;
            khungAnh.Width = chieuCaoKhung;
            khungAnh.Height = chieuRongKhung;
        }

        private void loadListSignatureOfFile()
        {
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.Panel2.Controls.Clear();
            PictureBox pictureBox = new PictureBox();
            pictureBox.Name = "SinglePicBox";
            this.splitContainer.Panel2.Controls.Add((Control)pictureBox);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Dock = DockStyle.None;
            pictureBox.Height = this.splitContainer.Panel2.Height - SystemInformation.HorizontalScrollBarHeight;
            pictureBox.Width = this.splitContainer.Panel2.Width - SystemInformation.VerticalScrollBarWidth;
            pictureBox.Location = new Point(0, 0);
            pictureBox.MouseUp += new MouseEventHandler(this.picImage_MouseUp);
            pictureBox.Paint += new PaintEventHandler(this.pictureBox_Paint);
            pictureBox.MouseDown += new MouseEventHandler(this.pictureBox_MouseDown);
            pictureBox.MouseMove += new MouseEventHandler(this.pictureBox_MouseMove);
            foreach (SignatureInfo pdfSignatureInfo in this.lstSignInfo)
            {
                Label label = new Label();
                label.Name = pdfSignatureInfo.SignatureName;
                label.AutoSize = false;
                label.Visible = false;
                label.BackColor = Color.Transparent;
                label.Width = pdfSignatureInfo.Position.Width;
                label.Height = pdfSignatureInfo.Position.Height;
                label.Location = new Point(0, 0);
                label.Cursor = Cursors.Hand;
                label.MouseMove += new MouseEventHandler(this.label_MouseMove);
                label.MouseLeave += new EventHandler(this.label_MouseLeave);
                label.MouseClick += new MouseEventHandler(this.label_MouseClick);
                this.splitContainer.Panel2.Controls.Add((Control)label);
            }
            this.splitContainer.Panel2.ResumeLayout();
            this._panel2_width = this.splitContainer.Panel2.Width;
        }

        private void label_MouseMove(
          object sender,
          MouseEventArgs e)
        {
            ((Label)sender).BorderStyle = BorderStyle.FixedSingle;
        }

        private void label_MouseLeave(
          object sender,
          EventArgs e)
        {
            ((Label)sender).BorderStyle = BorderStyle.None;
        }

        private void label_MouseClick(
          object sender,
          MouseEventArgs e)
        {
            try
            {
                this.XacThuc(((Control)sender).Name);
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message, "Xác thực chữ ký", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
        }

        private void XacThuc(string signatureName)
        {
            int num = (int)new frmVerifier(this.fileName, signatureName).ShowDialog();
        }

        private SizeF getSizePageSelected(int currrentPageNum) => this._listKichThuoc[currrentPageNum - 1].KichThuocGoc;

        private void openFrmSign(
          string savedFileName,
          int signedPageNumber,
          float pageLayout,
          ZoomType zoomType)
        {
            this.Close();
            try
            {
                this.fileName = savedFileName;
                SizeF[] sizeOfFilePdf = XuLyPDF.LayKichThuocTrang(savedFileName, this.Rotation);
                this._listKichThuoc = this.TinhToanKichThuocTrang(sizeOfFilePdf, this.viewType1);
                this._totalPages = sizeOfFilePdf.Length;
                this.pdfPageRenderer = new PdfPageRenderer(savedFileName, this._totalPages, new PageLayout(this.checkSinglePage(), this.ViewType, this._khoangTrangGiuaTrang, this.Rotation, pageLayout), false);
                this.lstSignInfo = PdfHelper.getListSignInfo(savedFileName);
                this._currentPageIndex = signedPageNumber;
                this.loadListSignatureOfFile();
                PictureBox oPictureBox = this.FindPictrueBoxInPanel2("SinglePicBox");
                ImageUtil.TinhLaiViTriTrang(ref oPictureBox);
                oPictureBox.Refresh();
                this.HienThiChuKyTrenTrang();
                this.setEnableButton(true);
                if (this.event_FileLoaded != null)
                {
                    this.event_FileLoaded((object)this, EventArgs.Empty);
                }
                this._hasOpenFile = true;
            }
            catch (AccessViolationException ex)
            {
                int num = (int)MessageBox.Show(@"Không thể mở tệp!Hay kiểm tra lại các nguyên nhân sau:
                                                 -Định dạng tệp đã bị thay đổi.
                                                 -Tệp đã được mã hóa.", "HiPT Sign PDF", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("Không thể mở tệp!Hay liên hệ với nhà phát triển để được trợ giúp.", "HiPT Sign PDF", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
        }
        public void picImage_MouseUp_bk(object sender, MouseEventArgs e)
        {
            frmSign_new frmSign = new frmSign_new(this.fileName, this._currentPageIndex, 0, 0, 0, 0, (int)this.gocXoayAnh);
            if (frmSign.ShowDialog() == DialogResult.OK)
            {
                var x = CreateCursorFromImage(frmSign.PicSigBase64);
                this.Cursor = x;
            };

        }

        private Cursor CreateCursorFromImage(string imageBase64)
        {
            // Kích thước Cursor (điều chỉnh nếu cần)
            Image image = SignerProfile.Base64ToBitmap(imageBase64); 

            // Tạo Bitmap mới với kích thước mong muốn
            Bitmap cursorBitmap = new Bitmap(image.Width, image.Height);

            // Vẽ Image lên Bitmap 
            using (Graphics g = Graphics.FromImage(cursorBitmap))
            {
                g.DrawImage(image, 0, 0, image.Width, image.Height);
            }

            // Tạo Cursor từ Bitmap
            return new Cursor(cursorBitmap.GetHicon());
        }

        public void picImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (!this.btnSignClicked)
                return;
            try
            {
                SizeF sizeF = this.getSizePageSelected(this._currentPageIndex);
                if (!(sender is PictureBox pictureBox))
                {
                }
                else if (this.rectangle1.IsEmpty)
                {
                    
                }
                else
                {
                    float tyLeKhungAnhRong = (float)pictureBox.Image.Width / sizeF.Width;
                    float tyLeKhungAnhCao = (float)pictureBox.Image.Height / sizeF.Height;
                    int toaDoYKhung;
                    int toaDoXKhung;
                    int chieuCaoKhung;
                    int chieuRongKhung;
                    switch (this.gocXoayAnh)
                    {
                        case ImageRotation.Rotate90:
                            toaDoYKhung = (int)((double)this.cursor_X / (double)tyLeKhungAnhRong);
                            toaDoXKhung = (int)((double)this.cursor_Y / (double)tyLeKhungAnhCao);
                            chieuCaoKhung = (int)((double)(e.X - this.cursor_X) / (double)tyLeKhungAnhRong);
                            chieuRongKhung = (int)((double)(e.Y - this.cursor_Y) / (double)tyLeKhungAnhCao);
                            break;
                        case ImageRotation.Rotate180:
                            toaDoXKhung = (int)((double)(pictureBox.Image.Width - e.X) / (double)tyLeKhungAnhRong);
                            toaDoYKhung = (int)((double)this.cursor_Y / (double)tyLeKhungAnhCao);
                            chieuRongKhung = (int)((double)(e.X - this.cursor_X) / (double)tyLeKhungAnhRong);
                            chieuCaoKhung = (int)((double)(e.Y - this.cursor_Y) / (double)tyLeKhungAnhCao);
                            break;
                        case ImageRotation.Rotate270:
                            toaDoYKhung = (int)((double)(pictureBox.Image.Width - e.X) / (double)tyLeKhungAnhRong);
                            toaDoXKhung = (int)((double)(pictureBox.Image.Height - e.Y) / (double)tyLeKhungAnhCao);
                            chieuCaoKhung = (int)((double)(e.X - this.cursor_X) / (double)tyLeKhungAnhRong);
                            chieuRongKhung = (int)((double)(e.Y - this.cursor_Y) / (double)tyLeKhungAnhCao);
                            break;
                        default:
                            toaDoXKhung = (int)((double)this.cursor_X / (double)tyLeKhungAnhRong);
                            toaDoYKhung = (int)((double)sizeF.Height - (double)e.Y / (double)tyLeKhungAnhCao);
                            chieuRongKhung = (int)((double)(e.X - this.cursor_X) / (double)tyLeKhungAnhRong);
                            chieuCaoKhung = (int)((double)(e.Y - this.cursor_Y) / (double)tyLeKhungAnhCao);
                            break;
                    }
                    //toaDoXKhung = 230;
                    //toaDoYKhung = 580;
                    frmSign frmSign = new frmSign(this.fileName, this._currentPageIndex, toaDoXKhung, toaDoYKhung, chieuRongKhung, chieuCaoKhung, (int)this.gocXoayAnh);
                    if (frmSign.ShowDialog() == DialogResult.OK)
                    {
                        // load lại file
                        this.OpenFile(frmSign.SavedFileName);
                        MessageBox.Show("Ký thành công");
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.rectangle1 = Rectangle.Empty;
                this.Cursor = Cursors.Default;   
                this.btnSignClicked = false;
                this.btnSign.Enabled = true;
                ((Control)sender).Invalidate();
                this.splitContainer.Panel2.Focus();
            }

        }

        private void pictureBox_MouseDown(
          object sender,
          MouseEventArgs e)
        {
            if (this.btnSignClicked)
            {
                this.rectangle1.Location = e.Location;
                this.cursor_X = e.X;
                this.cursor_Y = e.Y;
            }
            this.splitContainer.Panel2.Select();
            this.splitContainer.Panel2.Focus();
        }

        private void pictureBox_MouseMove(
          object sender,
          MouseEventArgs e)
        {
            if (this.btnSignClicked)
            {
                this.Cursor = Cursors.Cross;
                if (e.Button == MouseButtons.Left)
                {
                    this.rectangle1.Size = new Size(e.X - this.rectangle1.X, e.Y - this.rectangle1.Y);
                }
            }
          ((Control)sender).Invalidate();
        }

        private void pictureBox_Paint(
          object sender,
          PaintEventArgs e)
        {
            if (!this.btnSignClicked)
                return;
            if (this.rectangle1.IsEmpty)
            {

            }
            else
            {
                int x = this.rectangle1.X;
                int y = this.rectangle1.Y;
                int width = this.rectangle1.Width;
                int height = this.rectangle1.Height;
                if (width < 0)
                {
                    x += width;
                    width = -width;
                }
                if (height < 0)
                {
                    y += height;
                    height = -height;
                }
                e.Graphics.DrawRectangle(Pens.Red, new Rectangle(x, y, width, height));
                return;
            }
            
        }

        private PictureBox FindPictrueBoxInPanel2(
          string pageNum)
        {
            return (PictureBox)this.splitContainer.Panel2.Controls.Find("SinglePicBox", true)[0];
        }

        private Label setInfoSignatureName(string controlName)
        {
            try
            {
                return (Label)this.splitContainer.Panel2.Controls.Find(controlName, true)[0];
            }
            catch
            {
                return (Label)null;
            }
        }

        private KichThuoc[] TinhToanKichThuocTrang(
          SizeF[] kichThuocTrangPdf,
          ViewType kieuHienThi)
        {
            int soTrangTrongNhom = Math.Min(this.checkSinglePage(), kichThuocTrangPdf.Length);
            List<KichThuoc> danhSachKichThuoc = new List<KichThuoc>();
            int paddingDocTong = this.padding1.Top + this.padding1.Bottom;
            if (kieuHienThi == ViewType.SinglePage)
            {
                foreach (SizeF size in kichThuocTrangPdf)
                {
                     KichThuoc kichThuoc = new KichThuoc(size, (float)paddingDocTong, 0.0f);
                    danhSachKichThuoc.Add(kichThuoc);
                }
            }
            else
            {
                int khoangTrangGiuaCacPhanTrang = (int)this._khoangTrangGiuaTrang;
                for (int index1 = 0; index1 < kichThuocTrangPdf.Length; ++index1)
                {
                    if (index1 == 0)
                    {
                        if (kieuHienThi == ViewType.BookView)
                        {
                            danhSachKichThuoc.Add(new KichThuoc(kichThuocTrangPdf[0], (float)paddingDocTong, 0.0f));
                        }
                    }
                    float tongChieuRong = 0.0f;
                    float chieuCaoLonNhat = 0.0f;
                    List<SizeF> danhSachTrangNhom = new List<SizeF>();
                    for (int index2 = index1; index2 < index1 + soTrangTrongNhom; ++index2)
                    {
                        danhSachTrangNhom.Add(kichThuocTrangPdf[index2]);
                        tongChieuRong += kichThuocTrangPdf[index2].Width;
                        if ((double)kichThuocTrangPdf[index2].Height >= (double)chieuCaoLonNhat)
                        {
                            chieuCaoLonNhat = kichThuocTrangPdf[index2].Height;
                        }
                    }
                    danhSachKichThuoc.Add(new KichThuoc(new SizeF(tongChieuRong, chieuCaoLonNhat), (float)paddingDocTong, (float)(khoangTrangGiuaCacPhanTrang * (danhSachTrangNhom.Count - 1))));
                    index1 += soTrangTrongNhom - 1;
                    continue;
                }
            }
            return danhSachKichThuoc.ToArray();
        }

        internal int checkSinglePage()
        {
            if (this.viewType1 == ViewType.SinglePage)
                return 1;
            return 2;
        }

        private void setCurrentPageSelected()
        {
            this.lbTotalPage.Text = "/ " + this._totalPages.ToString();
            this.txtCurrentPage.Text = this._currentPageIndex.ToString();
        }

        private void btnOpenFile_Click(
          object sender,
          EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            this.OpenFile(openFileDialog.FileName);
            btnVerify_Click(sender, e);
        }

        private void btnPageDown_Click(
          object sender,
          EventArgs e)
        {
            Application.DoEvents();
            if (this._currentPageIndex >= this._totalPages)
                return;
            this._selectedPageNum = this._currentPageIndex;
            ++this._currentPageIndex;
            this.HienThiChuKyTrenTrang();
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
                this.HienThiChuKyTrenTrang();
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
                this.splitContainer.Panel2.Focus();
                e.Handled = true;
            }
            if (char.IsControl(e.KeyChar) || char.IsNumber(e.KeyChar))
            {
                return; // Cho phép nhập ký tự điều khiển hoặc số
            }
            else
            {
                e.Handled = true; // Ngăn chặn nhập các ký tự khác
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
            if (result > this._totalPages)
                return;
            this._currentPageIndex = result;
            this.HienThiChuKyTrenTrang();
            return;
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

        private void btnSign_Click(
          object sender,
          EventArgs e)
        {
            this.rectangle1 = Rectangle.Empty;
            this.btnSignClicked = true;
            this.btnSign.Enabled = false;
        }

        private void lkOpenfile_LinkClicked(
          object sender,
          LinkLabelLinkClickedEventArgs e)
        {
            this.btnOpenFile_Click((object)null, (EventArgs)null);
        }

        private void lbConvertFromWord_LinkClicked(
          object sender,
          LinkLabelLinkClickedEventArgs e)
        {
            frmConverter frmConverter = new frmConverter();
            if (frmConverter.ShowDialog() != DialogResult.OK)
                return;
            this.OpenFile(frmConverter.PdfPath);
        }

        private void ucPdfViewer_Resize(
          object sender,
          EventArgs e)
        {
            if (this._hasOpenFile)
            {
                PictureBox pictureBox = this.FindPictrueBoxInPanel2("SinglePicBox");
                if (this.ZoomType == ZoomType.FitToHeight)
                {
                    this.ZoomToHeight();
                }
                else
                {
                    if (this.ZoomType == ZoomType.FitToWidth)
                    {
                        if (pictureBox.Image != null)
                        {
                            this.ZoomToWidth();
                        }
                    }
                    if (this.ZoomType == ZoomType.Fixed)
                    {
                        if (pictureBox.Image != null)
                        {
                            this.Zoom(this.pdfPageRenderer.PageLayout.ImageResolution * (float)this.splitContainer.Panel2.Width / (float)this._panel2_width);
                        }
                    }
                }
            }
            this._panel2_width = this.splitContainer.Panel2.Width;
            this.XuLySauKhiThayDoiKichThuoc((object)null, (EventArgs)null);
        }

        //TODO Ham Xac Thuc
        private void XacThuc()
        {
            this.splitContainer.Panel1Collapsed = false;
            ZoomType zoomType = this.zoomType;
            this.Zoom(this.CurrentZoom);
            this.zoomType = zoomType;
            this.treeView.Nodes.Clear();
            Configuration configuration = new Configuration();
            this.Cursor = Cursors.WaitCursor;
            try
            {
                Application.DoEvents();
                PdfVerifier pdfVerifier = new PdfVerifier(this.fileName);
                pdfVerifier.AdditionalCRLs = configuration.AdditionalCrls;
                pdfVerifier.AllowedOnlineChecking = configuration.AllowedOnlineCheckingCert;
                if (configuration.UsedProxy)
                {
                    if (configuration.AutoDetectProxy)
                    {
                        WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
                    }
                    else
                        pdfVerifier.Proxy = configuration.Proxy;
                }
                else
                    WebRequest.DefaultWebProxy = (IWebProxy)null;
                List<SignatureInfo> pdfSignatureInfoList = pdfVerifier.Verify();
                if (pdfSignatureInfoList.Count <= 0)
                {
                    this.treeView.Nodes.Add(new TreeNode("Văn bản chưa được ký số!", 2, 2));
                }
                else
                {
                    using (List<SignatureInfo>.Enumerator enumerator = pdfSignatureInfoList.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            SignatureInfo current = enumerator.Current;
                            Application.DoEvents();
                            TreeNode node1 = new TreeNode();
                            string empty1 = string.Empty;
                            string empty2 = string.Empty;
                            CertInfo certInfo1 = (CertInfo)null;
                            bool flag1 = false;
                            bool flag2 = false;
                            if (current.ValidityErrors.ContainsKey(SignatureValidity.FatalError))
                            {
                                node1.Text = current.SignatureName;
                                node1.ImageIndex = 0;
                                node1.SelectedImageIndex = 0;
                                TreeNode node2 = new TreeNode(current.ValidityErrors[SignatureValidity.FatalError], 10, 10);
                                node1.Nodes.Add(node2);
                                this.treeView.Nodes.Add(node1);
                            }
                            else
                            {
                                string str1;
                                try
                                {
                                    certInfo1 = new CertInfo(current.SigningCertificate);
                                    str1 = certInfo1.ToString();
                                }
                                catch
                                {
                                    str1 = "N/A";
                                }
                                string str2 = string.Format("{0}: Người ký {1}", (object)current.SignatureName, (object)str1);
                                node1.Text = str2;
                                TreeNode node3 = new TreeNode("Tình trạng xác thực", 10, 10);
                                if (current.ValidityErrors.ContainsKey(SignatureValidity.DocumentModified))
                                {
                                    TreeNode node4 = new TreeNode("Tài liệu đã bị thay đổi.", 10, 10);
                                    node3.Nodes.Add(node4);
                                    flag1 = true;
                                }
                                else if (current.ValidityErrors.ContainsKey(SignatureValidity.NonCoversWholeDocument))
                                {
                                    node1.ImageIndex = 3;
                                    node1.SelectedImageIndex = 3;
                                    TreeNode node5 = new TreeNode("Nội dung đã ký số chưa bị thay đổi. Tài liệu đã được thêm chú thích, hoặc ký số, hoặc thay đổi thông tin trên trường nhập liệu.", 10, 10);
                                    node3.Nodes.Add(node5);
                                }
                                else
                                {
                                    TreeNode node6 = new TreeNode("Tài liệu chưa bị thay đổi.", 10, 10);
                                    node3.Nodes.Add(node6);
                                }
                                if (current.ValidityErrors.ContainsKey(SignatureValidity.InvalidSigningCertificate))
                                {
                                    TreeNode node7 = new TreeNode("Chứng thư số ký không hợp lệ: " + current.ValidityErrors[SignatureValidity.InvalidSigningCertificate], 10, 10);
                                    node3.Nodes.Add(node7);
                                    flag1 = true;

                                }
                                else if (current.ValidityErrors.ContainsKey(SignatureValidity.ErrorCheckingSigningCertificate))
                                {
                                    TreeNode node8 = new TreeNode("Lỗi quá trình kiểm tra: " + current.ValidityErrors[SignatureValidity.ErrorCheckingSigningCertificate], 10, 10);
                                    node3.Nodes.Add(node8);
                                    flag2 = true;
                                }
                                else if (current.ValidityErrors.ContainsKey(SignatureValidity.NonCheckingRevokedSigningCert))
                                {
                                    TreeNode node9 = new TreeNode("Chứng thư số ký không được kiểm tra tình trạng hủy bỏ.", 10, 10);
                                    node3.Nodes.Add(node9);
                                    flag2 = true;
                                }
                                else
                                {
                                    TreeNode node10 = new TreeNode("Chứng thư số ký hợp lệ.", 10, 10);
                                    node3.Nodes.Add(node10);
                                }
                                if (current.ValidityErrors.ContainsKey(SignatureValidity.NotTimestamped))
                                {
                                    TreeNode node11 = new TreeNode("Chữ ký không được gắn dấu thời gian.", 10, 10);
                                    node3.Nodes.Add(node11);
                                    flag2 = true;
                                }
                                else if (current.ValidityErrors.ContainsKey(SignatureValidity.InvalidTimestampImprint))
                                {
                                    TreeNode node12 = new TreeNode("Dấu thời gian không hợp lệ.", 10, 10);
                                    node3.Nodes.Add(node12);
                                    flag2 = true;
                                }
                                else if (current.ValidityErrors.ContainsKey(SignatureValidity.InvalidTSACertificate))
                                {
                                    TreeNode node13 = new TreeNode("Chứng thư số máy chủ cấp dấu thời gian không hợp lệ:" + current.ValidityErrors[SignatureValidity.InvalidTSACertificate], 10, 10);
                                    node3.Nodes.Add(node13);
                                    flag2 = true;
                                }
                                else if (current.ValidityErrors.ContainsKey(SignatureValidity.ErrorCheckingTSACertificate))
                                {
                                    TreeNode node14 = new TreeNode("Lỗi quá trình kiểm tra: " + current.ValidityErrors[SignatureValidity.ErrorCheckingTSACertificate], 10, 10);
                                    node3.Nodes.Add(node14);
                                    flag2 = true;
                                }
                                else if (current.ValidityErrors.ContainsKey(SignatureValidity.NonCheckingRevokedTSACert))
                                {
                                    TreeNode node15 = new TreeNode("Chứng thư số máy chủ cấp dấu thời gian không được kiểm tra tình trạng hủy bỏ.", 10, 10);
                                    node3.Nodes.Add(node15);
                                    flag2 = true;
                                }
                                else
                                {
                                    TreeNode node16 = new TreeNode("Dấu thời gian trên chữ ký hợp lệ.", 10, 10);
                                    node3.Nodes.Add(node16);
                                }
                                node1.Nodes.Add(node3);
                               
                                if (!flag1)
                                {
                                    if (!flag2)
                                    {
                                        if (node1.ImageIndex != 3)
                                        {
                                            node1.ImageIndex = 1;
                                            node1.SelectedImageIndex = 1;
                                        }
                                        node3.Text = "Tình trạng xác thực (Chữ ký hợp lệ!)";
                                    }
                                    else
                                    {
                                        node1.ImageIndex = 0;
                                        node1.SelectedImageIndex = 0;
                                        node3.Text = "Tình trạng xác thực(Không đủ thông tin xác thực!)";
                                    }
                                }
                                else
                                {
                                    node1.ImageIndex = 0;
                                    node1.SelectedImageIndex = 0;
                                    node3.Text = "Tình trạng xác thực(Chữ ký không hợp lệ!)";
                                }

                                TreeNode node17 = new TreeNode("Thông tin chữ ký", 10, 10);
                                if (!current.IsTsp)
                                {
                                    string str3;
                                    try
                                    {
                                        if (current.SigningTime != DateTime.MinValue)
                                        {
                                            str3 = current.SigningTime.ToString("dd/MM/yyyy HH:mm:ss zzz");
                                        }
                                        else
                                            str3 = "N/A";
                                    }
                                    catch
                                    {
                                        str3 = "N/A";
                                    }
                                    TreeNode node18 = new TreeNode(string.Format("Thời gian ký: {0}", (object)str3), 10, 10);
                                    node17.Nodes.Add(node18);
                                }
                                else
                                {
                                    TreeNode node19 = new TreeNode("Chữ ký là một dấu thời gian", 10, 10);
                                    node17.Nodes.Add(node19);
                                }
                                TreeNode node20 = new TreeNode(string.Format("Ký bởi: {0}", (object)str1), 10, 10);
                                TreeNode treeNode = node20;
                                CertInfo certInfo2;
                                if (!str1.Equals("N/A", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    certInfo2 = certInfo1;
                                }
                                else
                                    certInfo2 = (CertInfo)null;
                                treeNode.Tag = (object)certInfo2;
                                node17.Nodes.Add(node20);
                                node1.Nodes.Add(node17);
                                if (!current.ValidityErrors.ContainsKey(SignatureValidity.NotTimestamped))
                                {
                                    TreeNode node21 = new TreeNode("Thông tin dấu thời gian", 10, 10);
                                    string str4;
                                    try
                                    {
                                        if (!(current.TimeStampDate != DateTime.MinValue))
                                            throw new Exception();

                                        str4 = current.TimeStampDate.ToString("dd/MM/yyyy HH:mm:ss zzz");

                                    }
                                    catch
                                    {
                                        str4 = "N/A";
                                    }
                                    TreeNode node22 = new TreeNode(string.Format("Dấu thời gian: {0}", (object)str4), 10, 10);
                                    node21.Nodes.Add(node22);
                                    if (!current.IsTsp)
                                    {
                                        string str5;
                                        try
                                        {
                                            certInfo1 = new CertInfo(current.TimeStampCertificate);
                                            str5 = certInfo1.ToString();
                                        }
                                        catch
                                        {
                                            str5 = "N/A";
                                        }
                                        node21.Nodes.Add(new TreeNode(string.Format("Chứng thư số TSA: {0}", (object)str5), 10, 10)
                                        {
                                            Tag = str5.Equals("N/A", StringComparison.InvariantCultureIgnoreCase) ? (object)(CertInfo)null : (object)certInfo1
                                        });
                                    }
                                    node1.Nodes.Add(node21);
                                }
                                try
                                {
                                    node1.Nodes.Add(new TreeNode(string.Format("Chữ ký trên trang {0}", (object)current.PageIndex.ToString()), 10, 10)
                                    {
                                        Tag = (object)certInfo2//current.PageIndex
                                    });
                                }
                                catch
                                {
                                }
                                try
                                {
                                    node1.Nodes.Add(new TreeNode("Hiển thị nội dung đã ký số", 10, 10)
                                    {
                                        Tag = (object)current
                                    });
                                }
                                catch
                                {
                                }
                                this.treeView.Nodes.Add(node1);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("Quá trình duyệt danh sách chữ ký trên tài liệu gặp vấn đề!", "Xác thực chữ ký", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnVerify_Click(
          object sender,
          EventArgs e)
        {
            this.XacThuc();
            ZoomType zoomType = this.zoomType;
            this.Zoom(this.CurrentZoom);
            this.zoomType = zoomType;
        }

        private void Panel2_Resize(
          object sender,
          EventArgs e)
        {
        }

        private void btnSignaturesHide_Click(
          object sender,
          EventArgs e)
        {
            this.splitContainer.Panel1Collapsed = true;
            if (!this._hasOpenFile)
                return;
            ZoomType zoomType = this.zoomType;
            this.Zoom(this.CurrentZoom);
            this.zoomType = zoomType;
        }

        private void treeView_NodeMouseClick(
          object sender,
          TreeNodeMouseClickEventArgs e)
        {
        }

        private void treeView_NodeMouseDoubleClick(
          object sender,
          TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag == null)
            {
               
            }
            else
            {
                if (e.Node.Tag is CertInfo certInfo && certInfo.Certificate != null)
                {
                    X509Certificate2UI.DisplayCertificate((e.Node.Tag as CertInfo).Certificate, this.Handle);
                    return;
                }
                try
                {
                    int tag = (int)e.Node.Tag;
                    if (tag <= 0)
                        return;
                    if (tag > this._totalPages)
                        return;
                    this.txtCurrentPage.Text = tag.ToString();
                    this._currentPageIndex = tag;
                    this.HienThiChuKyTrenTrang();
                }
                catch
                {
                }
                try
                {
                    SignatureInfo tag = (SignatureInfo)e.Node.Tag;
                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Templates), Guid.NewGuid().ToString() + ".pdf");
                    PdfReader pdfReader = (PdfReader)null;
                    try
                    {
                        pdfReader = new PdfReader(this.fileName);
                        AcroFields acroFields = pdfReader.AcroFields;
                        using (FileStream fileStream = new FileStream(path, FileMode.CreateNew))
                        {
                            byte[] buffer = new byte[4096];
                            Stream revision = acroFields.ExtractRevision(tag.SignatureName);
                            try
                            {
                                int count;
                                do
                                {
                                    count = revision.Read(buffer, 0, buffer.Length);
                                    fileStream.Write(buffer, 0, count);
                                }
                                while (count > 0);
                            }
                            finally
                            {
                                if (revision != null)
                                {
                                    revision.Dispose();
                                }
                            }
                        }
                        CertInfo certInfoSigningCertificate = new CertInfo(tag.SigningCertificate);
                        new frmRevisionViewer(path, string.Format("{0}: Người ký {1}, {2}", (object)tag.SignatureName, (object)certInfoSigningCertificate.ToString(), (object)tag.SigningTime.ToString("dd/MM/yyyy HH:mm:ss zzz"))).Show();
                    }
                    catch (Exception ex)
                    {
                        int num = (int)MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        if (pdfReader != null)
                        {
                            pdfReader.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    int num = (int)MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnRotation_Click(
          object sender,
          EventArgs e)
        {
            if (this.Rotation < (ImageRotation)System.Enum.GetValues(typeof(ImageRotation)).Length)
                ++this.Rotation;
            else
                this.Rotation = ImageRotation.None;
            this.Rotate(this.Rotation);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.container != null)
            {
                this.container.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.container = (IContainer)new Container();
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ucPDFViewer));
            this.controlPanel = new Panel();
            this.label4 = new Label();
            this.label3 = new Label();
            this.label1 = new Label();
            this.label5 = new Label();
            this.label2 = new Label();
            this.lbTotalPage = new Label();
            this.txtCurrentPage = new TextBox();
            this.btnVerify = new Button();
            this.btnSign = new Button();
            this.btnOpenFile = new Button();
            this.btnPrint = new Button();
            this.btnRotation = new Button();
            this.btnFitWidth = new Button();
            this.btnFitPage = new Button();
            this.btnZoomIn = new Button();
            this.btnZoomOut = new Button();
            this.btnPageUp = new Button();
            this.btnPageDown = new Button();
            this.ClientPanel = new Panel();
            this.splitContainer = new SplitContainer();
            this.LstSignaturesPanel = new Panel();
            this.pnInfo = new Panel();
            this.lbInfo = new Label();
            this.signatureBarPanel = new Panel();
            this.label6 = new Label();
            this.btnSignaturesHide = new Button();
            this.treeView = new TreeView();
            this.imageList1 = new ImageList(this.container);
            this.panelLogo = new Panel();
            this.panel1 = new Panel();
            this.label9 = new Label();
            this.label10 = new Label();
            this.label8 = new Label();
            this.label7 = new Label();
            this.lbConvertFromWord = new LinkLabel();
            this.lkOpenfile = new LinkLabel();
            this.panel2 = new Panel();
            this.pictureBox2 = new PictureBox();
            this.pictureBox1 = new PictureBox();
            this.controlPanel.SuspendLayout();
            this.ClientPanel.SuspendLayout();
            this.splitContainer.BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.LstSignaturesPanel.SuspendLayout();
            this.pnInfo.SuspendLayout();
            this.signatureBarPanel.SuspendLayout();
            this.panelLogo.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((ISupportInitialize)this.pictureBox2).BeginInit();
            ((ISupportInitialize)this.pictureBox1).BeginInit();
            this.SuspendLayout();
            this.controlPanel.BorderStyle = BorderStyle.FixedSingle;
            this.controlPanel.Controls.Add((Control)this.label4);
            this.controlPanel.Controls.Add((Control)this.label3);
            this.controlPanel.Controls.Add((Control)this.label1);
            this.controlPanel.Controls.Add((Control)this.label5);
            this.controlPanel.Controls.Add((Control)this.label2);
            this.controlPanel.Controls.Add((Control)this.lbTotalPage);
            this.controlPanel.Controls.Add((Control)this.txtCurrentPage);
            this.controlPanel.Controls.Add((Control)this.btnVerify);
            this.controlPanel.Controls.Add((Control)this.btnSign);
            this.controlPanel.Controls.Add((Control)this.btnOpenFile);
            this.controlPanel.Controls.Add((Control)this.btnPrint);
            this.controlPanel.Controls.Add((Control)this.btnRotation);
            this.controlPanel.Controls.Add((Control)this.btnFitWidth);
            this.controlPanel.Controls.Add((Control)this.btnFitPage);
            this.controlPanel.Controls.Add((Control)this.btnZoomIn);
            this.controlPanel.Controls.Add((Control)this.btnZoomOut);
            this.controlPanel.Controls.Add((Control)this.btnPageUp);
            this.controlPanel.Controls.Add((Control)this.btnPageDown);
            this.controlPanel.Dock = DockStyle.Top;
            this.controlPanel.Location = new Point(0, 0);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new Size(714, 39);
            this.controlPanel.TabIndex = 1;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label4.Image = (Image)componentResourceManager.GetObject("label4.Image");
            this.label4.Location = new Point(48, 6);
            this.label4.Name = "label4";
            this.label4.Size = new Size(10, 24);
            this.label4.TabIndex = 2;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label3.Image = (Image)componentResourceManager.GetObject("label3.Image");
            this.label3.Location = new Point(137, 6);
            this.label3.Name = "label3";
            this.label3.Size = new Size(10, 24);
            this.label3.TabIndex = 2;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label1.Image = (Image)componentResourceManager.GetObject("label1.Image");
            this.label1.Location = new Point(411, 6);
            this.label1.Name = "label1";
            this.label1.Size = new Size(10, 24);
            this.label1.TabIndex = 2;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label5.Image = (Image)componentResourceManager.GetObject("label5.Image");
            this.label5.Location = new Point(320, 6);
            this.label5.Name = "label5";
            this.label5.Size = new Size(10, 24);
            this.label5.TabIndex = 2;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label2.Image = (Image)componentResourceManager.GetObject("label2.Image");
            this.label2.Location = new Point(228, 6);
            this.label2.Name = "label2";
            this.label2.Size = new Size(10, 24);
            this.label2.TabIndex = 2;
            this.lbTotalPage.AutoSize = true;
            this.lbTotalPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.lbTotalPage.Location = new Point(193, 10);
            this.lbTotalPage.Name = "lbTotalPage";
            this.lbTotalPage.Size = new Size(22, 16);
            this.lbTotalPage.TabIndex = 2;
            this.lbTotalPage.Text = "/ 0";
            this.txtCurrentPage.BorderStyle = BorderStyle.FixedSingle;
            this.txtCurrentPage.Font = new System.Drawing.Font("Times New Roman", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.txtCurrentPage.Location = new Point(153, 6);
            this.txtCurrentPage.Multiline = true;
            this.txtCurrentPage.Name = "txtCurrentPage";
            this.txtCurrentPage.Size = new Size(37, 25);
            this.txtCurrentPage.TabIndex = 1;
            this.txtCurrentPage.Text = "0";
            this.txtCurrentPage.TextAlign = HorizontalAlignment.Center;
            this.txtCurrentPage.KeyPress += new KeyPressEventHandler(this.txtCurrentPage_KeyPress);
            this.txtCurrentPage.Leave += new EventHandler(this.txtCurrentPage_Leave);
            this.btnVerify.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnVerify.FlatAppearance.BorderColor = Color.Silver;
            this.btnVerify.FlatAppearance.MouseDownBackColor = Color.Gray;
            this.btnVerify.FlatAppearance.MouseOverBackColor = Color.Gray;
            this.btnVerify.FlatStyle = FlatStyle.Flat;
            this.btnVerify.Font = new System.Drawing.Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.btnVerify.Image = (Image)Resources.verify;
            this.btnVerify.ImageAlign = ContentAlignment.MiddleLeft;
            this.btnVerify.Location = new Point(600, 3);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new Size(98, 31);
            this.btnVerify.TabIndex = 0;
            this.btnVerify.Text = "Xác thực";
            this.btnVerify.TextAlign = ContentAlignment.MiddleRight;
            this.btnVerify.UseVisualStyleBackColor = true;
            this.btnVerify.Click += new EventHandler(this.btnVerify_Click);
            this.btnSign.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnSign.FlatAppearance.BorderColor = Color.Silver;
            this.btnSign.FlatAppearance.MouseDownBackColor = Color.Gray;
            this.btnSign.FlatAppearance.MouseOverBackColor = Color.Gray;
            this.btnSign.FlatStyle = FlatStyle.Flat;
            this.btnSign.Font = new System.Drawing.Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.btnSign.Image = (Image)Resources.sign;
            this.btnSign.ImageAlign = ContentAlignment.MiddleLeft;
            this.btnSign.Location = new Point(516, 3);
            this.btnSign.Name = "btnSign";
            this.btnSign.Size = new Size(75, 31);
            this.btnSign.TabIndex = 0;
            this.btnSign.Text = "Ký số";
            this.btnSign.TextAlign = ContentAlignment.MiddleRight;
            this.btnSign.UseVisualStyleBackColor = true;
            this.btnSign.Click += new EventHandler(this.btnSign_Click);
            this.btnOpenFile.BackColor = SystemColors.Control;
            this.btnOpenFile.FlatAppearance.BorderColor = Color.Silver;
            this.btnOpenFile.FlatAppearance.MouseDownBackColor = Color.Gray;
            this.btnOpenFile.FlatAppearance.MouseOverBackColor = Color.Gray;
            this.btnOpenFile.FlatStyle = FlatStyle.Flat;
            this.btnOpenFile.Image = (Image)Resources.open22;
            this.btnOpenFile.Location = new Point(10, 3);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new Size(32, 31);
            this.btnOpenFile.TabIndex = 0;
            this.btnOpenFile.UseVisualStyleBackColor = false;
            this.btnOpenFile.Click += new EventHandler(this.btnOpenFile_Click);
            this.btnPrint.FlatAppearance.BorderColor = Color.Silver;
            this.btnPrint.FlatAppearance.MouseDownBackColor = Color.Gray;
            this.btnPrint.FlatAppearance.MouseOverBackColor = Color.Gray;
            this.btnPrint.FlatStyle = FlatStyle.Flat;
            this.btnPrint.Image = (Image)Resources.printer;
            this.btnPrint.Location = new Point(478, 3);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new Size(32, 31);
            this.btnPrint.TabIndex = 0;
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Visible = false;
            this.btnRotation.FlatAppearance.BorderColor = Color.Silver;
            this.btnRotation.FlatAppearance.MouseDownBackColor = Color.Gray;
            this.btnRotation.FlatAppearance.MouseOverBackColor = Color.Gray;
            this.btnRotation.FlatStyle = FlatStyle.Flat;
            this.btnRotation.Image = (Image)Resources.ic_rotation;
            this.btnRotation.Location = new Point(424, 3);
            this.btnRotation.Name = "btnRotation";
            this.btnRotation.Size = new Size(32, 31);
            this.btnRotation.TabIndex = 0;
            this.btnRotation.UseVisualStyleBackColor = true;
            this.btnRotation.Click += new EventHandler(this.btnRotation_Click);
            this.btnFitWidth.FlatAppearance.BorderColor = Color.Silver;
            this.btnFitWidth.FlatAppearance.MouseDownBackColor = Color.Gray;
            this.btnFitWidth.FlatAppearance.MouseOverBackColor = Color.Gray;
            this.btnFitWidth.FlatStyle = FlatStyle.Flat;
            this.btnFitWidth.Image = (Image)Resources.fitwidth;
            this.btnFitWidth.Location = new Point(373, 3);
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
            this.btnFitPage.Location = new Point(335, 3);
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
            this.btnZoomIn.Location = new Point(282, 3);
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
            this.btnZoomOut.Location = new Point(244, 3);
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
            this.btnPageUp.Location = new Point(62, 3);
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
            this.btnPageDown.Location = new Point(100, 3);
            this.btnPageDown.Name = "btnPageDown";
            this.btnPageDown.Size = new Size(32, 31);
            this.btnPageDown.TabIndex = 0;
            this.btnPageDown.UseVisualStyleBackColor = true;
            this.btnPageDown.Click += new EventHandler(this.btnPageDown_Click);
            this.ClientPanel.BackColor = SystemColors.ControlDark;
            this.ClientPanel.Controls.Add((Control)this.splitContainer);
            this.ClientPanel.Dock = DockStyle.Fill;
            this.ClientPanel.Location = new Point(0, 39);
            this.ClientPanel.Name = "ClientPanel";
            this.ClientPanel.Size = new Size(714, 276);
            this.ClientPanel.TabIndex = 2;
            this.splitContainer.BorderStyle = BorderStyle.FixedSingle;
            this.splitContainer.Dock = DockStyle.Fill;
            this.splitContainer.FixedPanel = FixedPanel.Panel1;
            this.splitContainer.Location = new Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Panel1.Controls.Add((Control)this.LstSignaturesPanel);
            this.splitContainer.Panel2.AutoScroll = true;
            this.splitContainer.Panel2.Controls.Add((Control)this.panelLogo);
            this.splitContainer.Panel2.Resize += new EventHandler(this.Panel2_Resize);
            this.splitContainer.Size = new Size(714, 276);
            this.splitContainer.SplitterDistance = 217;
            this.splitContainer.TabIndex = 0;
            this.LstSignaturesPanel.Controls.Add((Control)this.pnInfo);
            this.LstSignaturesPanel.Controls.Add((Control)this.signatureBarPanel);
            this.LstSignaturesPanel.Controls.Add((Control)this.treeView);
            this.LstSignaturesPanel.Dock = DockStyle.Fill;
            this.LstSignaturesPanel.Location = new Point(0, 0);
            this.LstSignaturesPanel.Name = "LstSignaturesPanel";
            this.LstSignaturesPanel.Size = new Size(215, 274);
            this.LstSignaturesPanel.TabIndex = 1;
            this.pnInfo.BackColor = SystemColors.Control;
            this.pnInfo.Controls.Add((Control)this.lbInfo);
            this.pnInfo.Dock = DockStyle.Bottom;
            this.pnInfo.Location = new Point(0, 217);
            this.pnInfo.Name = "pnInfo";
            this.pnInfo.Size = new Size(215, 57);
            this.pnInfo.TabIndex = 2;
            this.lbInfo.BackColor = Color.Ivory;
            this.lbInfo.Dock = DockStyle.Fill;
            this.lbInfo.Location = new Point(0, 0);
            this.lbInfo.Name = "lbInfo";
            this.lbInfo.Size = new Size(215, 57);
            this.lbInfo.TabIndex = 0;
            this.signatureBarPanel.BackColor = SystemColors.Control;
            this.signatureBarPanel.Controls.Add((Control)this.label6);
            this.signatureBarPanel.Controls.Add((Control)this.btnSignaturesHide);
            this.signatureBarPanel.Dock = DockStyle.Top;
            this.signatureBarPanel.Location = new Point(0, 0);
            this.signatureBarPanel.Name = "signatureBarPanel";
            this.signatureBarPanel.Size = new Size(215, 21);
            this.signatureBarPanel.TabIndex = 1;
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.label6.ForeColor = SystemColors.ControlText;
            this.label6.Location = new Point(8, 4);
            this.label6.Name = "label6";
            this.label6.Size = new Size(110, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Danh sách chữ ký";
            this.btnSignaturesHide.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnSignaturesHide.FlatAppearance.BorderSize = 0;
            this.btnSignaturesHide.FlatStyle = FlatStyle.Flat;
            this.btnSignaturesHide.Image = (Image)Resources.collapse;
            this.btnSignaturesHide.Location = new Point(195, 2);
            this.btnSignaturesHide.Name = "btnSignaturesHide";
            this.btnSignaturesHide.Size = new Size(16, 16);
            this.btnSignaturesHide.TabIndex = 0;
            this.btnSignaturesHide.UseVisualStyleBackColor = true;
            this.btnSignaturesHide.Click += new EventHandler(this.btnSignaturesHide_Click);
            this.treeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.treeView.BackColor = Color.WhiteSmoke;
            this.treeView.BorderStyle = BorderStyle.None;
            this.treeView.ImageIndex = 0;
            this.treeView.ImageList = this.imageList1;
            this.treeView.Indent = 16;
            this.treeView.Location = new Point(0, 20);
            this.treeView.Name = "trvSignatures";
            this.treeView.SelectedImageIndex = 0;
            this.treeView.ShowLines = false;
            this.treeView.ShowRootLines = false;
            this.treeView.Size = new Size(215, 254);
            this.treeView.TabIndex = 0;
            this.treeView.NodeMouseClick += new TreeNodeMouseClickEventHandler(this.treeView_NodeMouseClick);
            this.treeView.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(this.treeView_NodeMouseDoubleClick);
            //this.imageList1.ImageStream = (ImageListStreamer)componentResourceManager.GetObject("imageList1.ImageStream");
            this.imageList1.TransparentColor = Color.Transparent;
            //this.imageList1.Images.SetKeyName(0, "alert2.png");
            //this.imageList1.Images.SetKeyName(1, "ok16.png");
            //this.imageList1.Images.SetKeyName(2, "about.png");
            //this.imageList1.Images.SetKeyName(3, "okinfo.png");
            this.panelLogo.Anchor = AnchorStyles.None;
            this.panelLogo.BackColor = SystemColors.Control;
            this.panelLogo.BackgroundImage = (Image)Resources.sep2;
            this.panelLogo.BackgroundImageLayout = ImageLayout.None;
            this.panelLogo.BorderStyle = BorderStyle.FixedSingle;
            this.panelLogo.Controls.Add((Control)this.panel1);
            this.panelLogo.Controls.Add((Control)this.panel2);
            this.panelLogo.Location = new Point(23, 5);
            this.panelLogo.Name = "panelLogo";
            this.panelLogo.Size = new Size(444, 266);
            this.panelLogo.TabIndex = 0;
            this.panel1.BackgroundImage = (Image)Resources.sep2;
            this.panel1.BackgroundImageLayout = ImageLayout.Center;
            this.panel1.Controls.Add((Control)this.label9);
            this.panel1.Controls.Add((Control)this.label10);
            this.panel1.Controls.Add((Control)this.label8);
            this.panel1.Controls.Add((Control)this.label7);
            this.panel1.Controls.Add((Control)this.lbConvertFromWord);
            this.panel1.Controls.Add((Control)this.lkOpenfile);
            this.panel1.Dock = DockStyle.Fill;
            this.panel1.Location = new Point(0, 74);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(442, 190);
            this.panel1.TabIndex = 2;
            this.label9.AutoSize = true;
            this.label9.Location = new Point(236, 95);
            this.label9.Name = "label9";
            this.label9.Size = new Size(125, 13);
            this.label9.TabIndex = 2;
            this.label9.Text = "Copyright ©  2023 HiPT";
            this.label10.AutoSize = true;
            this.label10.Location = new Point(236, 46);
            this.label10.Name = "label10";
            this.label10.Size = new Size(142, 13);
            this.label10.TabIndex = 2;
            this.label10.Text = "Phần mềm ký số tài liệu PDF";
            this.label8.AutoSize = true;
            this.label8.Location = new Point(236, 70);
            this.label8.Name = "label8";
            this.label8.Size = new Size(73, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "Phiên bản Beta 1.0";
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.label7.ForeColor = Color.FromArgb(64, 0, 0);
            this.label7.Location = new Point(233, 16);
            this.label7.Name = "label7";
            this.label7.Size = new Size(104, 20);
            this.label7.TabIndex = 2;
            this.label7.Text = "HiPT Sign - PDF";
            this.lbConvertFromWord.ActiveLinkColor = Color.FromArgb(64, 0, 0);
            this.lbConvertFromWord.BackColor = Color.Transparent;
            this.lbConvertFromWord.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.lbConvertFromWord.Image = (Image)Resources.office24;
            this.lbConvertFromWord.ImageAlign = ContentAlignment.MiddleLeft;
            this.lbConvertFromWord.LinkBehavior = LinkBehavior.NeverUnderline;
            this.lbConvertFromWord.LinkColor = Color.FromArgb(64, 0, 0);
            this.lbConvertFromWord.Location = new Point(4, 51);
            this.lbConvertFromWord.Name = "lbConvertFromWord";
            this.lbConvertFromWord.Size = new Size(212, 28);
            this.lbConvertFromWord.TabIndex = 1;
            this.lbConvertFromWord.TabStop = true;
            this.lbConvertFromWord.Text = "        Mở tài liệu Office..";
            this.lbConvertFromWord.TextAlign = ContentAlignment.MiddleLeft;
            this.lbConvertFromWord.LinkClicked += new LinkLabelLinkClickedEventHandler(this.lbConvertFromWord_LinkClicked);
            this.lkOpenfile.ActiveLinkColor = Color.FromArgb(64, 0, 0);
            this.lkOpenfile.BackColor = Color.Transparent;
            this.lkOpenfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.lkOpenfile.Image = (Image)Resources.open22;
            this.lkOpenfile.ImageAlign = ContentAlignment.MiddleLeft;
            this.lkOpenfile.LinkBehavior = LinkBehavior.NeverUnderline;
            this.lkOpenfile.LinkColor = Color.FromArgb(64, 0, 0);
            this.lkOpenfile.Location = new Point(4, 12);
            this.lkOpenfile.Name = "lkOpenfile";
            this.lkOpenfile.Size = new Size(212, 28);
            this.lkOpenfile.TabIndex = 1;
            this.lkOpenfile.TabStop = true;
            this.lkOpenfile.Text = "        Mở tệp..";
            this.lkOpenfile.TextAlign = ContentAlignment.MiddleLeft;
            this.lkOpenfile.LinkClicked += new LinkLabelLinkClickedEventHandler(this.lkOpenfile_LinkClicked);
            this.panel2.BackgroundImage = (Image)Resources.bg2;
            this.panel2.Controls.Add((Control)this.pictureBox2);
            this.panel2.Controls.Add((Control)this.pictureBox1);
            this.panel2.Dock = DockStyle.Top;
            this.panel2.Location = new Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new Size(442, 74);
            this.panel2.TabIndex = 0;
            this.pictureBox2.BackColor = Color.Transparent;
            this.pictureBox2.Image = (Image)Resources.pdf1;
            this.pictureBox2.Location = new Point(377, 11);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new Size(63, 50);
            this.pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            this.pictureBox1.BackColor = Color.Transparent;
            this.pictureBox1.BackgroundImage = (Image)Resources.HiPT;
            this.pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
            this.pictureBox1.Location = new Point(1, 1);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new Size(163, 72);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add((Control)this.ClientPanel);
            this.Controls.Add((Control)this.controlPanel);
            this.MinimumSize = new Size(675, 315);
            this.Name = "ucPdfViewer";
            this.Size = new Size(714, 315);
            this.Resize += new EventHandler(this.ucPdfViewer_Resize);
            this.controlPanel.ResumeLayout(false);
            this.controlPanel.PerformLayout();
            this.ClientPanel.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.EndInit();
            this.splitContainer.ResumeLayout(false);
            this.LstSignaturesPanel.ResumeLayout(false);
            this.pnInfo.ResumeLayout(false);
            this.signatureBarPanel.ResumeLayout(false);
            this.signatureBarPanel.PerformLayout();
            this.panelLogo.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((ISupportInitialize)this.pictureBox2).EndInit();
            ((ISupportInitialize)this.pictureBox1).EndInit();
            this.ResumeLayout(false);
        }
    }
}
