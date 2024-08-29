using Plugin.UI.structs;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Plugin.UI.classess
{
    /// <summary>
    /// ccdb90e2278da0aa91643b4c6d6e4bb1f
    /// </summary>
    internal static class XuLyPDF
    {
        public static Bitmap LayHinhAnhTrang(
          string duongDanFilePDF,
          int soTrang,
          float heSoDieuChinh)
        {
            int chiSoTrang = Math.Max(0, soTrang - 1);
            TaiLieuPDF taiLieu = new TaiLieuPDF(duongDanFilePDF);
            try
            {
                IntPtr conTroTrang = XuLyPDF.MuPDFAPI.TaiTrangPDF(taiLieu.TaiLieuPDFPtr, chiSoTrang);
                Bitmap hinhAnh = XuLyPDF.ChuyenDoiTrangThanhBitmap(taiLieu.NguCanhMuPDF, taiLieu.TaiLieuPDFPtr, conTroTrang, heSoDieuChinh);
                XuLyPDF.MuPDFAPI.GiaiPhongTrangPDF(taiLieu.TaiLieuPDFPtr, conTroTrang);
                return hinhAnh;
            }
            finally
            {
                if (taiLieu != null)
                {
                    taiLieu.Dispose();
                }
            }
        }

        public static SizeF[] LayKichThuocTrang(
          string duongDanFilePDF,
          ImageRotation gocXoay)
        {
            TaiLieuPDF taiLieu = new TaiLieuPDF(duongDanFilePDF);
            try
            {
                int length = MuPDFAPI.DemSoTrang(taiLieu.TaiLieuPDFPtr);
                SizeF[] sizeFArray = new SizeF[length];
                for (int i = 0; i < length; ++i)
                {
                    IntPtr conTroTrang = XuLyPDF.MuPDFAPI.TaiTrangPDF(taiLieu.TaiLieuPDFPtr, i);
                    ThongTinTrang thongTinTrang = new ThongTinTrang();
                    MuPDFAPI.LayThongTinTrang(taiLieu.TaiLieuPDFPtr, conTroTrang, ref thongTinTrang);
                    SizeF sizeF = new SizeF(thongTinTrang.ChieuRong, thongTinTrang.ChieuCao);
                    if (gocXoay != ImageRotation.None && gocXoay != ImageRotation.Rotate180)
                    {
                        sizeF = new SizeF(thongTinTrang.ChieuCao, thongTinTrang.ChieuRong);
                    }
                    sizeFArray[i] = sizeF;
                    MuPDFAPI.GiaiPhongTrangPDF(taiLieu.TaiLieuPDFPtr, conTroTrang);
                }
                return sizeFArray;
            }
            finally
            {
                if (taiLieu != null)
                {
                    taiLieu.Dispose();
                }
            }
        }

        public static int DemSoTrang(string duongDanFilePDF)
        {
            using (XuLyPDF.TaiLieuPDF taiLieu = new XuLyPDF.TaiLieuPDF(duongDanFilePDF))
                return XuLyPDF.MuPDFAPI.DemSoTrang(taiLieu.TaiLieuPDFPtr);
        }

        private static unsafe Bitmap ChuyenDoiTrangThanhBitmap(
          IntPtr nguCanhMuPDF,
          IntPtr taiLieuPDF,
          IntPtr conTroTrang,
          float heSoDieuChinh)
        {
            ThongTinTrang thongTinTrang = new ThongTinTrang();
            MuPDFAPI.LayThongTinTrang(taiLieuPDF, conTroTrang, ref thongTinTrang);
            ThongTinVe thongTinVe = new ThongTinVe();
            IntPtr conTroThietBi = IntPtr.Zero;
            IntPtr conTroKhongGianMau = IntPtr.Zero;
            DoPhanGiai doPhanGiai = DoPhanGiaiHienTai.LayDoPhanGiai();
            float tyLeDPI_X = heSoDieuChinh * (doPhanGiai.DPI_Ngang / 96f);
            float tyLeDPI_Y = heSoDieuChinh * (doPhanGiai.DPI_Doc / 96f);
            int chieuRongBitmap = (int)((double)thongTinTrang.ChieuRong * (double)tyLeDPI_X);
            int chieuCaoBitmap = (int)((double)thongTinTrang.ChieuCao * (double)tyLeDPI_Y);
            thongTinVe.TyLeDPI_X = tyLeDPI_X;
            thongTinVe.TyLeDPI_Y = tyLeDPI_Y;
            IntPtr conTroPixmap = MuPDFAPI.TaoPixmap(nguCanhMuPDF, MuPDFAPI.TimKhongGianMau(nguCanhMuPDF, "DeviceRGB"), chieuRongBitmap, chieuCaoBitmap);
            MuPDFAPI.XoaPixmap(nguCanhMuPDF, conTroPixmap, (int)byte.MaxValue);
            IntPtr conTroThietBiVe = XuLyPDF.MuPDFAPI.TaoThietBiVe(nguCanhMuPDF, conTroPixmap);
            MuPDFAPI.VeTrangPDF(taiLieuPDF, conTroTrang, conTroThietBiVe, ref thongTinVe, IntPtr.Zero);
            MuPDFAPI.GiaiPhongThietBi(conTroThietBiVe);
            conTroKhongGianMau = IntPtr.Zero;
            Bitmap bitmap = new Bitmap(chieuRongBitmap, chieuCaoBitmap, PixelFormat.Format24bppRgb);
            BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, chieuRongBitmap, chieuCaoBitmap), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            byte* diaChiDuLieuPixmap = (byte*)(void*)XuLyPDF.MuPDFAPI.LayDiaChiDuLieuPixmap(nguCanhMuPDF, conTroPixmap);
            byte* diaChiDuLieuBitmap = (byte*)(void*)bitmapdata.Scan0;
            for (int index1 = 0; index1 < chieuCaoBitmap; ++index1)
            {
                byte* dongBitmap = diaChiDuLieuBitmap;
                byte* dongPixmap = diaChiDuLieuPixmap;
                for (int index2 = 0; index2 < chieuRongBitmap; ++index2)
                {
                    dongBitmap[2] = *dongPixmap;
                    dongBitmap[1] = dongPixmap[1];
                    *dongBitmap = dongPixmap[2];
                    dongBitmap += 3;
                    dongPixmap += 4;
                }
                diaChiDuLieuBitmap += bitmapdata.Stride;
                diaChiDuLieuPixmap += chieuRongBitmap * 4;
                continue;
            }
            bitmap.UnlockBits(bitmapdata);
            XuLyPDF.MuPDFAPI.GiaiPhongPixmap(nguCanhMuPDF, conTroPixmap);
            bitmap.SetResolution(doPhanGiai.DPI_Ngang, doPhanGiai.DPI_Doc);
            return bitmap;
        }

        private sealed class TaiLieuPDF : IDisposable
        {
            private const uint KICH_THUOC_BO_NHO_DEM = 268435456;
            private IntPtr nguCanhMuPDF;
            private IntPtr luongDuLieu;
            private IntPtr taiLieuPDF;

            public TaiLieuPDF(string duongDanFilePDF)
            {
                this.nguCanhMuPDF = XuLyPDF.MuPDFAPI.TaoNguCanhMuPDF();
                this.luongDuLieu = XuLyPDF.MuPDFAPI.MoFile(this.nguCanhMuPDF, duongDanFilePDF);
                this.taiLieuPDF = XuLyPDF.MuPDFAPI.MoTaiLieuPDF(this.nguCanhMuPDF, this.luongDuLieu);
            }

            public IntPtr NguCanhMuPDF
            {
                get => this.nguCanhMuPDF;
                private set => this.nguCanhMuPDF = value;
            }

            public IntPtr LuongDuLieu
            {
                get => this.luongDuLieu;
                private set => this.luongDuLieu = value;
            }

            public IntPtr TaiLieuPDFPtr
            {
                get => this.taiLieuPDF;
                private set => this.taiLieuPDF = value;
            }

            public void Dispose()
            {
                XuLyPDF.MuPDFAPI.DongTaiLieuPDF(this.TaiLieuPDFPtr);
                XuLyPDF.MuPDFAPI.DongFile(this.LuongDuLieu);
                XuLyPDF.MuPDFAPI.GiaiPhongNguCanhMuPDF(this.NguCanhMuPDF);
            }
        }


        // Cấu trúc dữ liệu giả lập


        private class MuPDFAPI
        {
            private const uint KICH_THUOC_BO_NHO_DEM = 268435456;
            private const string TEN_THU_VIEN = "libmupdf.dll";
            private const string PHIEN_BAN_THU_VIEN = "1.6";

            [DllImport(TEN_THU_VIEN, EntryPoint = "fz_new_context_imp", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr fz_new_context_imp(IntPtr parm1, IntPtr parm2, uint parm3, string parm4);

            public static IntPtr TaoNguCanhMuPDF() => XuLyPDF.MuPDFAPI.fz_new_context_imp(IntPtr.Zero, IntPtr.Zero, KICH_THUOC_BO_NHO_DEM, PHIEN_BAN_THU_VIEN);

            [DllImport(TEN_THU_VIEN, EntryPoint = "fz_free_context", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr GiaiPhongNguCanhMuPDF(IntPtr nguCanh);

            [DllImport(TEN_THU_VIEN, EntryPoint = "fz_open_file_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MoFile(
              IntPtr nguCanh,
              string duongDanFile);

            [DllImport(TEN_THU_VIEN, EntryPoint = "pdf_open_document_with_stream", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MoTaiLieuPDF(
                IntPtr nguCanh,
                IntPtr luongDuLieu);

            [DllImport(TEN_THU_VIEN, EntryPoint = "fz_close", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr DongFile(
              IntPtr luongDuLieu);

            [DllImport(TEN_THU_VIEN, EntryPoint = "pdf_close_document", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr DongTaiLieuPDF(
              IntPtr taiLieuPDF);

            [DllImport(TEN_THU_VIEN, EntryPoint = "pdf_count_pages", CallingConvention = CallingConvention.Cdecl)]
            public static extern int DemSoTrang(
              IntPtr taiLieuPDF);

            [DllImport(TEN_THU_VIEN, EntryPoint = "pdf_bound_page", CallingConvention = CallingConvention.Cdecl)]
            public static extern void LayThongTinTrang(
              IntPtr taiLieuPDF,
              IntPtr conTroTrang,
              ref ThongTinTrang thongTinTrang);

            [DllImport(TEN_THU_VIEN, EntryPoint = "fz_clear_pixmap_with_value", CallingConvention = CallingConvention.Cdecl)]
            public static extern void XoaPixmap(
              IntPtr nguCanh,
              IntPtr pixmap,
              int giaTri);

            [DllImport(TEN_THU_VIEN, EntryPoint = "fz_lookup_device_colorspace", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr TimKhongGianMau(
              IntPtr nguCanh,
          string tenKhongGianMau);

            [DllImport(TEN_THU_VIEN, EntryPoint = "fz_free_device", CallingConvention = CallingConvention.Cdecl)]
            public static extern void GiaiPhongThietBi(
              IntPtr thietBi);

            [DllImport(TEN_THU_VIEN, EntryPoint = "pdf_free_page", CallingConvention = CallingConvention.Cdecl)]
            public static extern void GiaiPhongTrangPDF(
              IntPtr taiLieuPDF,
              IntPtr conTroTrang);

            [DllImport(TEN_THU_VIEN, EntryPoint = "pdf_load_page", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr TaiTrangPDF(
              IntPtr taiLieuPDF,
              int chiSoTrang);

            [DllImport(TEN_THU_VIEN, EntryPoint = "fz_new_draw_device", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr TaoThietBiVe(
               IntPtr nguCanh,
          IntPtr pixmap);

            [DllImport(TEN_THU_VIEN, EntryPoint = "fz_new_pixmap", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr TaoPixmap(
               IntPtr nguCanh,
          IntPtr khongGianMau,
          int chieuRong,
          int chieuCao);

            [DllImport(TEN_THU_VIEN, EntryPoint = "pdf_run_page", CallingConvention = CallingConvention.Cdecl)]
            public static extern void VeTrangPDF(
              IntPtr taiLieuPDF,
          IntPtr conTroTrang,
          IntPtr thietBiVe,
          ref ThongTinVe thongTinVe,
          IntPtr cookie);

            [DllImport(TEN_THU_VIEN, EntryPoint = "fz_drop_pixmap", CallingConvention = CallingConvention.Cdecl)]
            public static extern void GiaiPhongPixmap(
                IntPtr nguCanh,
          IntPtr pixmap);

            [DllImport(TEN_THU_VIEN, EntryPoint = "fz_pixmap_samples", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr LayDiaChiDuLieuPixmap(
               IntPtr nguCanh,
          IntPtr pixmap);
        }

    }
}
