namespace Plugin.UI.structs
{
    /// <summary>
    /// c28df71991dd90f8caa975855d53015ff
    /// </summary>
    internal struct ThongTinTrang
    {
        public float ToaDoXTrai;
        public float ToaDoYTren;
        public float ToaDoXPhai;
        public float ToaDoYDuoi;

        public float ChieuRong => this.ToaDoXPhai - this.ToaDoXTrai;

        public float ChieuCao => this.ToaDoYDuoi - this.ToaDoYTren;
    }
}
