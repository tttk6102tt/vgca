using System.Drawing;

namespace Sign.PDF
{
    public class SignerProfile
    {
        public SignerProfile(string name, string img)
        {
            this.Name = name;
            this.ImageBase64 = img;
        }
        public SignerProfile(string name, string imgBase64, int appearanceMode, bool showLabel, bool showEmail, bool showOrg, bool showOrgUnit, bool showDate, bool showJob)
        {
            this.Name = name;
            this.ImageBase64 = imgBase64;
            this.AppearanceMode = appearanceMode;
            this.ShowLabel = showLabel;
            this.ShowEmail = showEmail;
            this.ShowCQ1 = showOrg;
            this.ShowCQ2 = showOrgUnit;
            this.ShowDate = showDate;
        }
        public SignerProfile(string name, string imgBase64, int appearanceMode, bool isOrgProfile, bool showLabel, bool showEmail, bool showOrg, bool showOrgUnit, bool showDate, bool showJob)
        {
            this.Name = name;
            this.ImageBase64 = imgBase64;
            this.AppearanceMode = appearanceMode;
            this.ShowLabel = showLabel;
            this.ShowEmail = showEmail;
            this.ShowCQ1 = showOrg;
            this.ShowCQ2 = showOrgUnit;
            this.ShowDate = showDate;
            this.IsOrgProfile = isOrgProfile;
        }
        public string Name { get; set; }
        public int AppearanceMode { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowLabel { get; set; }
        public bool ShowCQ1 { get; set; }
        public bool ShowCQ2 { get; set; }
        public bool ShowCQ3 { get; set; }
        public bool ShowDate { get; set; }
        public bool IsOrgProfile { get; set; }
        public Bitmap Image { get; }
        public string ImageBase64 { get; set; }

        public static string BitmapToBase64(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Lưu trữ bitmap vào MemoryStream dưới dạng PNG
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                // Chuyển đổi byte array thành Base64 string
                byte[] imageBytes = memoryStream.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }
        public static Bitmap Base64ToBitmap(string base64String)
        {
            // Kiểm tra xem chuỗi có hợp lệ hay không
            if (string.IsNullOrEmpty(base64String))
            {
                throw new ArgumentException("Chuỗi Base64 không hợp lệ.", nameof(base64String));
            }

            // Loại bỏ phần header "data:image/png;base64," (hoặc tương tự)
            int dataIndex = base64String.IndexOf(",", StringComparison.Ordinal) + 1;
            string base64Data = base64String.Substring(dataIndex);

            // Chuyển đổi chuỗi Base64 thành byte array
            byte[] imageBytes = Convert.FromBase64String(base64Data);

            // Tạo Bitmap từ byte array
            using (MemoryStream memoryStream = new MemoryStream(imageBytes))
            {
                return new Bitmap(memoryStream);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}", Name);
        }
    }
}
