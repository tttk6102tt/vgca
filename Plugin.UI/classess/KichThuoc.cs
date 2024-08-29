namespace Plugin.UI.classess
{
    /// <summary>
    /// c83a1eeb3a7dd367bf8a3d036a395b92f
    /// </summary>
    internal class KichThuoc
    {
        private SizeF kichThuocGoc;
        private float paddingDoc;
        private float paddingNgang;

        public KichThuoc(
          SizeF kichThuocGoc,
          float paddingDoc,
          float paddingNgang)
        {
            this.kichThuocGoc = kichThuocGoc;
            this.paddingDoc = paddingDoc;
            this.paddingNgang = paddingNgang;
        }

        public SizeF KichThuocGoc
        {
            get => this.kichThuocGoc;
            private set => this.kichThuocGoc = value;
        }

        public float PaddingDoc
        {
            get => this.paddingDoc;
            private set => this.paddingDoc = value;
        }

        public float PaddingNgang
        {
            get => this.paddingNgang;
            private set => this.paddingNgang = value;
        }

        public SizeF KichThuocSauCung
        {
            //get
            //{
            //    return new SizeF(
            //        kichThuocGoc.Width + paddingNgang,
            //        kichThuocGoc.Height + paddingDoc
            //    );
            //}
            get
            {
                SizeF var1 = this.KichThuocGoc;
                double width = (double)var1.Width + (double)this.paddingNgang;
                var1 = this.KichThuocGoc;
                double height = (double)var1.Height + (double)this.paddingDoc;
                return new SizeF((float)width, (float)height);
            }
        }
    }
}
