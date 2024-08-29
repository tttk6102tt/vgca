using Sign.itext.error_messages;
using System.Net;

namespace Sign.itext.io
{
    public sealed class RandomAccessSourceFactory
    {
        private bool forceRead;

        private bool usePlainRandomAccess;

        private bool exclusivelyLockFile;

        public RandomAccessSourceFactory SetForceRead(bool forceRead)
        {
            this.forceRead = forceRead;
            return this;
        }

        public RandomAccessSourceFactory SetExclusivelyLockFile(bool exclusivelyLockFile)
        {
            this.exclusivelyLockFile = exclusivelyLockFile;
            return this;
        }

        public IRandomAccessSource CreateSource(byte[] data)
        {
            return new ArrayRandomAccessSource(data);
        }

        public IRandomAccessSource CreateSource(FileStream raf)
        {
            return new RAFRandomAccessSource(raf);
        }

        public IRandomAccessSource CreateSource(Uri url)
        {
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Credentials = CredentialCache.DefaultCredentials;
            Stream responseStream = webRequest.GetResponse().GetResponseStream();
            try
            {
                return CreateSource(responseStream);
            }
            finally
            {
                try
                {
                    responseStream.Close();
                }
                catch
                {
                }
            }
        }

        public IRandomAccessSource CreateSource(Stream inp)
        {
            try
            {
                return CreateSource(StreamUtil.InputStreamToArray(inp));
            }
            finally
            {
                try
                {
                    inp.Close();
                }
                catch
                {
                }
            }
        }

        public IRandomAccessSource CreateBestSource(string filename)
        {
            if (!File.Exists(filename))
            {
                if (filename.StartsWith("file:/") || filename.StartsWith("http://") || filename.StartsWith("https://"))
                {
                    return CreateSource(new Uri(filename));
                }

                return CreateByReadingToMemory(filename);
            }

            if (forceRead)
            {
                return CreateByReadingToMemory(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
            }

            return new RAFRandomAccessSource(new FileStream(filename, FileMode.Open, FileAccess.Read, (!exclusivelyLockFile) ? FileShare.Read : FileShare.None));
        }

        public IRandomAccessSource CreateRanged(IRandomAccessSource source, IList<long> ranges)
        {
            IRandomAccessSource[] array = new IRandomAccessSource[ranges.Count / 2];
            for (int i = 0; i < ranges.Count; i += 2)
            {
                array[i / 2] = new WindowRandomAccessSource(source, ranges[i], ranges[i + 1]);
            }

            return new GroupedRandomAccessSource(array);
        }

        private IRandomAccessSource CreateByReadingToMemory(string filename)
        {
            Stream resourceStream = StreamUtil.GetResourceStream(filename);
            if (resourceStream == null)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("1.not.found.as.file.or.resource", filename));
            }

            return CreateByReadingToMemory(resourceStream);
        }

        private IRandomAccessSource CreateByReadingToMemory(Stream inp)
        {
            try
            {
                return new ArrayRandomAccessSource(StreamUtil.InputStreamToArray(inp));
            }
            finally
            {
                try
                {
                    inp.Close();
                }
                catch
                {
                }
            }
        }
    }
}
