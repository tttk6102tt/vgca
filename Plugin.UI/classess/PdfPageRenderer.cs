using Plugin.UI.interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Plugin.UI.classess
{
    /// <summary>
    /// cd0b8e48b6b7e8085536fa3c936eb04bd
    /// </summary>
    internal class PdfPageRenderer : IPagedData<IEnumerable<PageImage>>
    {
        private string _pdfFilePath;
        private int _totalPages = -1;
        private int _imagesPerPage;
        private bool _isBookView;
        private PageLayout _pageLayout;

        public PdfPageRenderer(
          string pdfFilePath,
          int imagesPerPage,
          PageLayout pageLayout,
          bool isBookView)
        {
            this._pdfFilePath = pdfFilePath;
            this._imagesPerPage = imagesPerPage;
            this._pageLayout = pageLayout;
            this._isBookView = isBookView;
        }

        public PageLayout PageLayout
        {
            get => this._pageLayout;
            private set => this._pageLayout = value;
        }

        public int GetTotalPages()
        {
            if (this._totalPages == -1)
            {
                this._totalPages = XuLyPDF.DemSoTrang(this._pdfFilePath);
            }
            return this._totalPages;
        }

        public IList<IEnumerable<PageImage>> GetPageImages(
          int startPage,
          int endPage)
        {
            int imagesPerRow = this._pageLayout.ImagesPerRow;
            ViewType viewType = this._pageLayout.ViewType;
            startPage = startPage * imagesPerRow + 1;
            if (this._isBookView)
                endPage *= imagesPerRow;

            if (viewType == ViewType.BookView && startPage == 1)
            {
                endPage = Math.Min(_imagesPerPage, _isBookView ? 1 + imagesPerRow : 0);
            }
            else
            {
                --startPage;
            }

            int lastPage = Math.Min(this.GetTotalPages(), startPage + endPage - 1);
            List<IEnumerable<PageImage>> pageImages = new List<IEnumerable<PageImage>>();
            List<PageImage> currentRowImages = new List<PageImage>(imagesPerRow);
            int firstPageIndex = viewType == ViewType.BookView ? 1 : 0;
            int currentPageIndex = firstPageIndex;
            for (int i = Math.Min(this.GetTotalPages(), startPage); i <= Math.Min(this.GetTotalPages(), Math.Max(startPage, lastPage)); ++i)
            {
                Padding padding = new Padding(0, 0, (int)this._pageLayout.ImagePaddingRight, 0);
                using (Bitmap pageImage = XuLyPDF.LayHinhAnhTrang(this._pdfFilePath, i, this._pageLayout.ImageResolution))
                {
                    if (this._pageLayout.ImageRotation != ImageRotation.None)
                    {
                        RotateFlipType rotateFlipType = RotateFlipType.Rotate90FlipNone;
                        switch (_pageLayout.ImageRotation)
                        {
                            case ImageRotation.Rotate90:
                                break;
                            case ImageRotation.Rotate180:
                                rotateFlipType = RotateFlipType.Rotate180FlipNone;
                                break;
                            case ImageRotation.Rotate270:
                                rotateFlipType = RotateFlipType.Rotate270FlipNone;
                                break;
                        }
                        pageImage.RotateFlip(rotateFlipType);
                    }
                    if (i == 1 && viewType == ViewType.BookView)
                    {
                        padding.Right = 0;
                    }
                    // Thiết lập padding cho ảnh chẵn trong chế độ BookView
                    else if ((i + currentPageIndex) % 2 == 0)
                    {
                        padding.Right = 0;
                    }
                    PageImage imageData = new PageImage();
                    imageData.Image = ImageHelper.BitmapToByteArray(pageImage);
                    imageData.Padding = padding;

                    // Xử lý riêng cho chế độ BookView
                    if (viewType == ViewType.BookView)
                    {
                        if (i == 1)
                        {
                            pageImages.Add(new PageImage[1] { imageData });
                            continue;
                        }
                    }
                    else
                    {
                        currentRowImages.Add(imageData);
                    }
                }
                if (currentRowImages.Count % imagesPerRow != 0)
                {
                    if (i == lastPage)
                    {
                        if (currentRowImages.Count % imagesPerRow != 0)
                        {
                            currentRowImages[currentRowImages.Count - 1].Padding = new Padding(0);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                pageImages.Add((IEnumerable<PageImage>)currentRowImages);
                if (i == lastPage)
                {
                    if (currentRowImages.Count % imagesPerRow != 0)
                    {
                        currentRowImages[currentRowImages.Count - 1].Padding = new Padding(0);
                    }
                }
                if (i < lastPage)
                    currentRowImages = new List<PageImage>(imagesPerRow);
            }
            return (IList<IEnumerable<PageImage>>)pageImages;
        }
    }
}
