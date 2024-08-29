namespace Sign.itext.pdf.fonts.cmaps
{
    public class CMapCache
    {
        private static readonly Dictionary<string, CMapUniCid> cacheUniCid = new Dictionary<string, CMapUniCid>();

        private static readonly Dictionary<string, CMapCidUni> cacheCidUni = new Dictionary<string, CMapCidUni>();

        private static readonly Dictionary<string, CMapCidByte> cacheCidByte = new Dictionary<string, CMapCidByte>();

        private static readonly Dictionary<string, CMapByteCid> cacheByteCid = new Dictionary<string, CMapByteCid>();

        public static CMapUniCid GetCachedCMapUniCid(string name)
        {
            CMapUniCid value = null;
            lock (cacheUniCid)
            {
                cacheUniCid.TryGetValue(name, out value);
            }

            if (value == null)
            {
                value = new CMapUniCid();
                CMapParserEx.ParseCid(name, value, new CidResource());
                lock (cacheUniCid)
                {
                    cacheUniCid[name] = value;
                    return value;
                }
            }

            return value;
        }

        public static CMapCidUni GetCachedCMapCidUni(string name)
        {
            CMapCidUni value = null;
            lock (cacheCidUni)
            {
                cacheCidUni.TryGetValue(name, out value);
            }

            if (value == null)
            {
                value = new CMapCidUni();
                CMapParserEx.ParseCid(name, value, new CidResource());
                lock (cacheCidUni)
                {
                    cacheCidUni[name] = value;
                    return value;
                }
            }

            return value;
        }

        public static CMapCidByte GetCachedCMapCidByte(string name)
        {
            CMapCidByte value = null;
            lock (cacheCidByte)
            {
                cacheCidByte.TryGetValue(name, out value);
            }

            if (value == null)
            {
                value = new CMapCidByte();
                CMapParserEx.ParseCid(name, value, new CidResource());
                lock (cacheCidByte)
                {
                    cacheCidByte[name] = value;
                    return value;
                }
            }

            return value;
        }

        public static CMapByteCid GetCachedCMapByteCid(string name)
        {
            CMapByteCid value = null;
            lock (cacheByteCid)
            {
                cacheByteCid.TryGetValue(name, out value);
            }

            if (value == null)
            {
                value = new CMapByteCid();
                CMapParserEx.ParseCid(name, value, new CidResource());
                lock (cacheByteCid)
                {
                    cacheByteCid[name] = value;
                    return value;
                }
            }

            return value;
        }
    }
}
