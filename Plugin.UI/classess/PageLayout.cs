namespace Plugin.UI.classess
{
    /// <summary>
    /// Class2
    /// </summary>
    internal class PageLayout
    {
        private int _imagesPerRow;
        private float _imagePaddingRight;
        private ViewType _viewType;
        private float _imageResolution;
        private ImageRotation _imageRotation;

        public PageLayout(
          int imagesPerRow,
          ViewType viewType,
          float imageResolution,
          ImageRotation imageRotation,
         float imagePaddingRight = 0)
        {
            this._imagesPerRow = imagesPerRow;
            this._imageResolution = imagePaddingRight;
            this._viewType = viewType;
            double num;
            if (viewType != ViewType.SinglePage)
            {
                num = (double)imageResolution;
            }
            else
                num = 0.0;
            this._imagePaddingRight = (float)num;
            this._imageRotation = imageRotation;
        }

        public int ImagesPerRow
        {
            get => this._imagesPerRow;
            set => this._imagesPerRow = value;
        }

        public float ImagePaddingRight
        {
            get => this._imagePaddingRight;
            set => this._imagePaddingRight = value;
        }

        public ViewType ViewType
        {
            get => this._viewType;
            set => this._viewType = value;
        }

        public float ImageResolution
        {
            get => this._imageResolution;
            set => this._imageResolution = value;
        }

        public ImageRotation ImageRotation
        {
            get => this._imageRotation;
            set => this._imageRotation = value;
        }
    }
}
