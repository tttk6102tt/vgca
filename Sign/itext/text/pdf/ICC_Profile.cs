using Sign.itext.error_messages;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class ICC_Profile
    {
        protected byte[] data;

        protected int numComponents;

        private static Dictionary<string, int> cstags;

        public virtual byte[] Data => data;

        public virtual int NumComponents => numComponents;

        protected ICC_Profile()
        {
        }

        public static ICC_Profile GetInstance(byte[] data, int numComponents)
        {
            if (data.Length < 128 || data[36] != 97 || data[37] != 99 || data[38] != 115 || data[39] != 112)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.icc.profile"));
            }

            ICC_Profile iCC_Profile = new ICC_Profile();
            iCC_Profile.data = data;
            if (!cstags.TryGetValue(Encoding.ASCII.GetString(data, 16, 4), out iCC_Profile.numComponents))
            {
                iCC_Profile.numComponents = 0;
            }

            if (iCC_Profile.numComponents != numComponents)
            {
                throw new ArgumentException("ICC profile contains " + iCC_Profile.numComponents + " component(s), the image data contains " + numComponents + " component(s)");
            }

            return iCC_Profile;
        }

        public static ICC_Profile GetInstance(byte[] data)
        {
            if (!cstags.TryGetValue(Encoding.ASCII.GetString(data, 16, 4), out var value))
            {
                value = 0;
            }

            return GetInstance(data, value);
        }

        public static ICC_Profile GetInstance(Stream file)
        {
            byte[] array = new byte[128];
            int num = array.Length;
            int num2 = 0;
            while (num > 0)
            {
                int num3 = file.Read(array, num2, num);
                if (num3 <= 0)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.icc.profile"));
                }

                num -= num3;
                num2 += num3;
            }

            if (array[36] != 97 || array[37] != 99 || array[38] != 115 || array[39] != 112)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.icc.profile"));
            }

            num = ((array[0] & 0xFF) << 24) | ((array[1] & 0xFF) << 16) | ((array[2] & 0xFF) << 8) | (array[3] & 0xFF);
            byte[] array2 = new byte[num];
            Array.Copy(array, 0, array2, 0, array.Length);
            num -= array.Length;
            num2 = array.Length;
            while (num > 0)
            {
                int num4 = file.Read(array2, num2, num);
                if (num4 <= 0)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.icc.profile"));
                }

                num -= num4;
                num2 += num4;
            }

            return GetInstance(array2);
        }

        public static ICC_Profile GetInstance(string fname)
        {
            FileStream fileStream = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile instance = GetInstance(fileStream);
            fileStream.Close();
            return instance;
        }

        static ICC_Profile()
        {
            cstags = new Dictionary<string, int>();
            cstags["XYZ "] = 3;
            cstags["Lab "] = 3;
            cstags["Luv "] = 3;
            cstags["YCbr"] = 3;
            cstags["Yxy "] = 3;
            cstags["RGB "] = 3;
            cstags["GRAY"] = 1;
            cstags["HSV "] = 3;
            cstags["HLS "] = 3;
            cstags["CMYK"] = 4;
            cstags["CMY "] = 3;
            cstags["2CLR"] = 2;
            cstags["3CLR"] = 3;
            cstags["4CLR"] = 4;
            cstags["5CLR"] = 5;
            cstags["6CLR"] = 6;
            cstags["7CLR"] = 7;
            cstags["8CLR"] = 8;
            cstags["9CLR"] = 9;
            cstags["ACLR"] = 10;
            cstags["BCLR"] = 11;
            cstags["CCLR"] = 12;
            cstags["DCLR"] = 13;
            cstags["ECLR"] = 14;
            cstags["FCLR"] = 15;
        }
    }
}
