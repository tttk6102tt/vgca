using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Plugin.UI.classess
{
    internal static class ImageHelper
    {
        /// <summary>
        /// Chuyển đổi một đối tượng <see cref="Bitmap"/> thành mảng byte.
        /// </summary>
        /// <param name="bitmap">Đối tượng <see cref="Bitmap"/> cần chuyển đổi.</param>
        /// <returns>Mảng byte biểu diễn dữ liệu hình ảnh.</returns>
        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapdata = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            IntPtr scan0 = bitmapdata.Scan0;
            int length1 = Math.Abs(bitmapdata.Stride) * bitmap.Height;
            byte[] numArray = new byte[length1];
            byte[] destination = numArray;
            int length2 = length1;
            Marshal.Copy(scan0, destination, 0, length2);
            bitmap.UnlockBits(bitmapdata);
            return numArray;
        }
    }
}
