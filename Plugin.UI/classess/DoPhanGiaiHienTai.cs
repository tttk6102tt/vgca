using Plugin.UI.Enum;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Plugin.UI.classess
{
    /// <summary>
    /// c94810510dfb984b630a0c9d400d8fd90
    /// </summary>
    internal static class DoPhanGiaiHienTai
    {
        public const float DefaultDpi = 96f;

        public static DoPhanGiai LayDoPhanGiai()
        {
            IntPtr hdc = Graphics.FromHwnd(IntPtr.Zero).GetHdc();
            return new DoPhanGiai()
            {
                DPI_Ngang = GetDeviceCaps(hdc, (int)DeviceCap.LOGPIXELSX),
                DPI_Doc = GetDeviceCaps(hdc, (int)DeviceCap.LOGPIXELSY)
            };
        }

        [DllImport("gdi32.dll", EntryPoint = "GetDeviceCaps", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetDeviceCaps(IntPtr hdc, int index);
    }

}
