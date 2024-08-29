using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Plugin.UI
{
    public class ImageUtil
    {
        public static Bitmap MakeGrayscale(Bitmap original)
        {
            Bitmap bitmap = new Bitmap(original.Width, original.Height);
            Graphics graphics = Graphics.FromImage((Image)bitmap);
            ColorMatrix newColorMatrix = new ColorMatrix(new float[5][]
            {
        new float[5]{ 0.3f, 0.3f, 0.3f, 0.0f, 0.0f },
        new float[5]{ 0.59f, 0.59f, 0.59f, 0.0f, 0.0f },
        new float[5]{ 0.11f, 0.11f, 0.11f, 0.0f, 0.0f },
        new float[5]{ 0.0f, 0.0f, 0.0f, 1f, 0.0f },
        new float[5]{ 0.0f, 0.0f, 0.0f, 0.0f, 1f }
            });
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(newColorMatrix);
            graphics.DrawImage((Image)original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, imageAttr);
            graphics.Dispose();
            return bitmap;
        }

        public static Bitmap BitmapTo1Bpp(Bitmap img)
        {
            if (img.PixelFormat != PixelFormat.Format32bppPArgb)
            {
                Bitmap bitmap = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppPArgb);
                Graphics graphics = Graphics.FromImage((Image)bitmap);
                graphics.DrawImage((Image)img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
                img.Dispose();
                graphics.Dispose();
                img = bitmap;
            }
            Image image = (Image)img;
            BitmapData bitmapdata = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, img.PixelFormat);
            Bitmap bitmap1 = new Bitmap(image.Width, image.Height, PixelFormat.Format1bppIndexed);
            BitmapData bitmapData = bitmap1.LockBits(new Rectangle(0, 0, bitmap1.Width, bitmap1.Height), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
            DateTime now = DateTime.Now;
            for (int y = 0; y < img.Height - 1; ++y)
            {
                for (int x = 0; x < img.Width - 1; ++x)
                {
                    int ofs = y * bitmapdata.Stride + x * 4;
                    if ((double)Color.FromArgb((int)Marshal.ReadByte(bitmapdata.Scan0, ofs + 2), (int)Marshal.ReadByte(bitmapdata.Scan0, ofs + 1), (int)Marshal.ReadByte(bitmapdata.Scan0, ofs)).GetBrightness() > 0.5)
                    {
                        new ImageUtil().SetIndexedPixel(x, y, bitmapData, true);
                        continue;
                    }
                }
                continue;
            }
            bitmap1.UnlockBits(bitmapData);
            img.UnlockBits(bitmapdata);
            return bitmap1;
        }

        protected void SetIndexedPixel(int x, int y, BitmapData bmd, bool pixel)
        {
            int ofs = y * bmd.Stride + (x >> 3);
            byte num = Marshal.ReadByte(bmd.Scan0, ofs);
            byte x1 = (byte)(128 >> (x & 7));
            byte val;
            if (pixel)
            {
                val = (byte)((uint)num | (uint)x1);
            }
            else
                val = (byte)((uint)num & (uint)Convert.ToByte(Math.Pow((double)x1, (double)byte.MaxValue)));
            Marshal.WriteByte(bmd.Scan0, ofs, val);
        }

        public static void ThayDoiKichThuocAnh(ref Image hinhAnh, float heSoThayDoi)
        {
            Bitmap bitmapGoc = new Bitmap(hinhAnh);
            Bitmap bitmapMoi = new Bitmap(Convert.ToInt32((float)bitmapGoc.Width * heSoThayDoi), Convert.ToInt32((float)bitmapGoc.Height * heSoThayDoi));
            Graphics doHoa = Graphics.FromImage((Image)bitmapMoi);
            doHoa.DrawImage((Image)bitmapGoc, 0, 0, bitmapMoi.Width + 1, bitmapMoi.Height + 1);
            doHoa.Dispose();
            hinhAnh = (Image)bitmapMoi;
        }

        public static void ThayDoiKichThuocAnhTheoKhung(ref PictureBox khungAnh, ref Image hinhAnh)
        {
            float tyLeRong = (float)(khungAnh.Width / hinhAnh.Width);
            float tyLeCao = (float)(khungAnh.Height / hinhAnh.Height);
            float heSoThayDoi;
            if ((double)tyLeRong > (double)tyLeCao)
            {
                heSoThayDoi = tyLeCao;
            }
            else
                heSoThayDoi = tyLeRong;
            ImageUtil.ThayDoiKichThuocAnh(ref hinhAnh, heSoThayDoi);
        }

        public static void RotateImageClockwise(ref PictureBox pPicBox)
        {
            pPicBox.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            ImageUtil.FlipDimensions(ref pPicBox);
            ImageUtil.TinhLaiViTriTrang(ref pPicBox);
            pPicBox.Refresh();
        }

        public static void RotateImageCounterclockwise(ref PictureBox pPicBox)
        {
            pPicBox.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            ImageUtil.FlipDimensions(ref pPicBox);
            ImageUtil.TinhLaiViTriTrang(ref pPicBox);
            pPicBox.Refresh();
        }

        public static void FlipDimensions(ref PictureBox pPicbox)
        {
            int height = pPicbox.Height;
            int width = pPicbox.Width;
            pPicbox.Height = width;
            pPicbox.Width = height;
        }

        public static void TinhLaiViTriTrang(ref PictureBox khungAnh)
        {
            Size kichThuocKhungAnh = khungAnh.Size;
            Size kichThuocCha = khungAnh.Parent.ClientSize;
            Point viTri = khungAnh.Location;
            int luotCuonNgang = ((ScrollableControl)khungAnh.Parent).HorizontalScroll.Value;
            int luotCuonDoc = ((ScrollableControl)khungAnh.Parent).VerticalScroll.Value;
            int leTraiCha = khungAnh.Parent.Margin.Left;
            int leTrenCha = khungAnh.Parent.Margin.Top;
            if (kichThuocKhungAnh.Width < kichThuocCha.Width && kichThuocKhungAnh.Height > kichThuocCha.Height)
            {
                viTri = new Point((kichThuocCha.Width - kichThuocKhungAnh.Width) / 2 + leTraiCha, leTrenCha - luotCuonDoc);
            }
            else
            {
                if (kichThuocKhungAnh.Width > kichThuocCha.Width)
                {
                    if (kichThuocKhungAnh.Height < kichThuocCha.Height)
                    {
                        viTri = new Point(leTraiCha - luotCuonNgang, (kichThuocCha.Height - kichThuocKhungAnh.Height) / 2 + leTrenCha);
                    }
                }
                viTri = kichThuocKhungAnh.Width >= kichThuocCha.Width || kichThuocKhungAnh.Height >= kichThuocCha.Height
                    ? new Point(leTraiCha - luotCuonNgang, leTrenCha - luotCuonDoc)
                    : new Point((kichThuocCha.Width - kichThuocKhungAnh.Width) / 2 + leTraiCha, (kichThuocCha.Height - kichThuocKhungAnh.Height) / 2 + leTrenCha);
            }
            khungAnh.Location = viTri;
            khungAnh.Bounds = new Rectangle(viTri, kichThuocKhungAnh);
        }

        public static void ApDungXoay(ref Image hinhAnh, int soLuongXoay)
        {
            if (soLuongXoay < 0)
            {
                for (int index = 1; index <= Math.Abs(soLuongXoay); ++index)
                    hinhAnh.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }
            if (soLuongXoay <= 0)
                return;
            for (int index = 1; index <= soLuongXoay; ++index)
                hinhAnh.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }

        public static void PictureBoxZoomActual(ref PictureBox pPicBox)
        {
            if (pPicBox == null || pPicBox.Image == null)
                return;
            pPicBox.Width = pPicBox.Image.Width;
            pPicBox.Height = pPicBox.Image.Height;
        }

        public static void PictureBoxZoomPageWidth(ref PictureBox pPicBox)
        {
            if (pPicBox == null || pPicBox.Parent == null)
                return;
            pPicBox.Width = pPicBox.Parent.ClientSize.Width - 18;
            pPicBox.Height = Convert.ToInt32(pPicBox.Image.Height * pPicBox.Width / pPicBox.Image.Width);
        }

        public static void PictureBoxZoomFit(ref PictureBox pPicBox)
        {
            if (pPicBox == null || pPicBox.Parent == null)
                return;
            ImageUtil.PictureBoxCenter(ref pPicBox, pPicBox.Parent.ClientSize.Width - 7, pPicBox.Parent.ClientSize.Height - 7);
            pPicBox.Location = new Point(0, 0);
        }

        public static void PictureBoxCenter(ref PictureBox oPictureBox, int psWidth, int psHeight)
        {
            oPictureBox.Width = psWidth;
            oPictureBox.Height = psHeight;
            ImageUtil.TinhLaiViTriTrang(ref oPictureBox);
        }

        public static void PictureBoxZoomIn(ref PictureBox pPicBox)
        {
            if (pPicBox == null)
                return;
            if (pPicBox.Parent != null)
                ImageUtil.PictureBoxCenter(ref pPicBox, (int)((double)pPicBox.Width * 1.25), (int)((double)pPicBox.Height * 1.25));
            return;
        }

        public static void PictureBoxZoomOut(ref PictureBox pPicBox)
        {
            if (pPicBox == null)
                return;
            ImageUtil.PictureBoxCenter(ref pPicBox, (int)((double)pPicBox.Width / 1.25), (int)((double)pPicBox.Height / 1.25));
        }

        public static void PictureBoxZoomFitMany(ref PictureBox pPicBox)
        {
            if (pPicBox.Parent == null)
                return;
            Size clientSize = pPicBox.Parent.ClientSize;
            int num1 = clientSize.Height - 14;
            clientSize = pPicBox.Parent.ClientSize;
            int num2 = clientSize.Width - 14;
            Point point = new Point(0, 0);
            System.Collections.IEnumerator enumerator = pPicBox.Parent.Controls.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Control current = (Control)enumerator.Current;
                    if (current is PictureBox)
                    {
                        current.Height = num1;
                        current.Width = num2;
                        current.Location = point;
                        continue;
                    }
                }
                return;
            }
            finally
            {
                if (enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public static Image GenerateThumbnail(Image original, int percentage)
        {
            if (percentage < 1)
            {
                percentage = 1;
            }
            Bitmap thumbnail = new Bitmap(Convert.ToInt32((float)original.Width * 0.01f * (float)percentage), Convert.ToInt32((float)original.Height * 0.01f * (float)percentage));
            Graphics graphics = Graphics.FromImage((Image)thumbnail);
            graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            graphics.DrawImage(original, new Rectangle(0, 0, thumbnail.Width, thumbnail.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel);
            graphics.Dispose();
            return (Image)thumbnail;
        }

        public static void DeleteFile(string filename)
        {
            try
            {
                File.Delete(filename);
            }
            catch (Exception ex)
            {
            }
        }

        public static bool IsTiff(string filename) => !string.IsNullOrEmpty(filename) && Regex.IsMatch(filename, @"\.tiff*$", RegexOptions.IgnoreCase);

        public static bool IsPDF(string filename) => !string.IsNullOrEmpty(filename) && Regex.IsMatch(filename, @"\.pdf$", RegexOptions.IgnoreCase);

        public static Bitmap CropBitmap(
          ref Bitmap bmp,
          int cropX,
          int cropY,
          int cropWidth,
          int cropHeight)
        {
            Rectangle rect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            return bmp.Clone(rect, bmp.PixelFormat);
        }

        private static ImageCodecInfo GetImageCodecInfoByMimeType(
          string mimeType)
        {
            ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
            for (int index = 0; index < imageEncoders.Length; ++index)
            {
                if (imageEncoders[index].MimeType == mimeType)
                {
                    return imageEncoders[index];
                }
            }
            return (ImageCodecInfo)null;
        }
    }
}
