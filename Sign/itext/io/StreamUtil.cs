using System.Reflection;
using System.Resources;

namespace Sign.itext.io
{
    public static class StreamUtil
    {
        internal static List<object> resourceSearch = new List<object>();

        public static byte[] InputStreamToArray(Stream inp)
        {
            byte[] array = new byte[8192];
            MemoryStream memoryStream = new MemoryStream();
            while (true)
            {
                int num = inp.Read(array, 0, array.Length);
                if (num < 1)
                {
                    break;
                }

                memoryStream.Write(array, 0, num);
            }

            memoryStream.Close();
            return memoryStream.ToArray();
        }

        public static void CopyBytes(IRandomAccessSource source, long start, long length, Stream outs)
        {
            if (length <= 0)
            {
                return;
            }

            long num = start;
            byte[] array = new byte[8192];
            while (length > 0)
            {
                long num2 = source.Get(num, array, 0, (int)Math.Min(array.Length, length));
                if (num2 <= 0)
                {
                    throw new EndOfStreamException();
                }

                outs.Write(array, 0, (int)num2);
                num += num2;
                length -= num2;
            }
        }

        public static void AddToResourceSearch(object obj)
        {
            lock (resourceSearch)
            {
                if (obj is Assembly)
                {
                    resourceSearch.Add(obj);
                }
                else if (obj is string)
                {
                    string path = (string)obj;
                    if (Directory.Exists(path) || File.Exists(path))
                    {
                        resourceSearch.Add(obj);
                    }
                }
            }
        }
        public static Stream GetResourceStream(string key)
        {
            Stream stream = null;
            try
            {
                stream = new MemoryStream((new ResourceManager(@"Sign.Properties.Resources", Assembly.GetExecutingAssembly())).GetObject(key) as byte[]);

                //Assembly assembly = Assembly.GetExecutingAssembly();
                //stream = assembly.GetManifestResourceStream(key);
                //stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(key);
            }
            catch (Exception ex)
            {
            }

            if (stream != null)
            {
                return stream;
            }

            int count;
            lock (resourceSearch)
            {
                count = resourceSearch.Count;
            }

            for (int i = 0; i < count; i++)
            {
                object obj2;
                lock (resourceSearch)
                {
                    obj2 = resourceSearch[i];
                }

                try
                {
                    if (obj2 is Assembly)
                    {
                        stream = ((Assembly)obj2).GetManifestResourceStream(key);
                        if (stream != null)
                        {
                            return stream;
                        }
                    }
                    else
                    {
                        if (!(obj2 is string))
                        {
                            continue;
                        }

                        string text = (string)obj2;
                        try
                        {
                            stream = Assembly.LoadFrom(text).GetManifestResourceStream(key);
                        }
                        catch
                        {
                        }

                        if (stream != null)
                        {
                            return stream;
                        }

                        string text2 = key.Replace('.', '/');
                        string path = Path.Combine(text, text2);
                        if (File.Exists(path))
                        {
                            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                        }

                        int num = text2.LastIndexOf('/');
                        if (num >= 0)
                        {
                            text2 = text2.Substring(0, num) + "." + text2.Substring(num + 1);
                            path = Path.Combine(text, text2);
                            if (File.Exists(path))
                            {
                                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                            }
                        }

                        continue;
                    }
                }
                catch
                {
                }
            }

            return stream;
        }
    }
}
