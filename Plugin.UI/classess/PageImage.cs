using System.Windows.Forms;

namespace Plugin.UI.classess
{
    /// <summary>
    /// cc46b673aac40f11812b9ceca48e8ed73
    /// </summary>
    internal class PageImage
    {
        private byte[] _image;
        private Padding _padding;

        public byte[] Image
        {
            get => this._image;
            set => this._image = value;
        }

        public Padding Padding
        {
            get => this._padding;
            set => this._padding = value;
        }
    }
}
