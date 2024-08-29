using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Security;

namespace Sign.itext.pdf.security
{
    public static class DigestAlgorithms
    {
        public const string SHA1 = "SHA-1";

        public const string SHA256 = "SHA-256";

        public const string SHA384 = "SHA-384";

        public const string SHA512 = "SHA-512";

        public const string RIPEMD160 = "RIPEMD160";

        private static readonly Dictionary<string, string> digestNames;

        private static readonly Dictionary<string, string> allowedDigests;

        static DigestAlgorithms()
        {
            digestNames = new Dictionary<string, string>();
            allowedDigests = new Dictionary<string, string>();
            digestNames["1.2.840.113549.2.5"] = "MD5";
            digestNames["1.2.840.113549.2.2"] = "MD2";
            digestNames["1.3.14.3.2.26"] = "SHA1";
            digestNames["2.16.840.1.101.3.4.2.4"] = "SHA224";
            digestNames["2.16.840.1.101.3.4.2.1"] = "SHA256";
            digestNames["2.16.840.1.101.3.4.2.2"] = "SHA384";
            digestNames["2.16.840.1.101.3.4.2.3"] = "SHA512";
            digestNames["1.3.36.3.2.2"] = "RIPEMD128";
            digestNames["1.3.36.3.2.1"] = "RIPEMD160";
            digestNames["1.3.36.3.2.3"] = "RIPEMD256";
            digestNames["1.2.840.113549.1.1.4"] = "MD5";
            digestNames["1.2.840.113549.1.1.2"] = "MD2";
            digestNames["1.2.840.113549.1.1.5"] = "SHA1";
            digestNames["1.2.840.113549.1.1.14"] = "SHA224";
            digestNames["1.2.840.113549.1.1.11"] = "SHA256";
            digestNames["1.2.840.113549.1.1.12"] = "SHA384";
            digestNames["1.2.840.113549.1.1.13"] = "SHA512";
            digestNames["1.2.840.113549.2.5"] = "MD5";
            digestNames["1.2.840.113549.2.2"] = "MD2";
            digestNames["1.2.840.10040.4.3"] = "SHA1";
            digestNames["2.16.840.1.101.3.4.3.1"] = "SHA224";
            digestNames["2.16.840.1.101.3.4.3.2"] = "SHA256";
            digestNames["2.16.840.1.101.3.4.3.3"] = "SHA384";
            digestNames["2.16.840.1.101.3.4.3.4"] = "SHA512";
            digestNames["1.3.36.3.3.1.3"] = "RIPEMD128";
            digestNames["1.3.36.3.3.1.2"] = "RIPEMD160";
            digestNames["1.3.36.3.3.1.4"] = "RIPEMD256";
            digestNames["1.2.643.2.2.9"] = "GOST3411";
            allowedDigests["MD2"] = "1.2.840.113549.2.2";
            allowedDigests["MD-2"] = "1.2.840.113549.2.2";
            allowedDigests["MD5"] = "1.2.840.113549.2.5";
            allowedDigests["MD-5"] = "1.2.840.113549.2.5";
            allowedDigests["SHA1"] = "1.3.14.3.2.26";
            allowedDigests["SHA-1"] = "1.3.14.3.2.26";
            allowedDigests["SHA224"] = "2.16.840.1.101.3.4.2.4";
            allowedDigests["SHA-224"] = "2.16.840.1.101.3.4.2.4";
            allowedDigests["SHA256"] = "2.16.840.1.101.3.4.2.1";
            allowedDigests["SHA-256"] = "2.16.840.1.101.3.4.2.1";
            allowedDigests["SHA384"] = "2.16.840.1.101.3.4.2.2";
            allowedDigests["SHA-384"] = "2.16.840.1.101.3.4.2.2";
            allowedDigests["SHA512"] = "2.16.840.1.101.3.4.2.3";
            allowedDigests["SHA-512"] = "2.16.840.1.101.3.4.2.3";
            allowedDigests["RIPEMD128"] = "1.3.36.3.2.2";
            allowedDigests["RIPEMD-128"] = "1.3.36.3.2.2";
            allowedDigests["RIPEMD160"] = "1.3.36.3.2.1";
            allowedDigests["RIPEMD-160"] = "1.3.36.3.2.1";
            allowedDigests["RIPEMD256"] = "1.3.36.3.2.3";
            allowedDigests["RIPEMD-256"] = "1.3.36.3.2.3";
            allowedDigests["GOST3411"] = "1.2.643.2.2.9";
        }

        public static IDigest GetMessageDigestFromOid(string digestOid)
        {
            return DigestUtilities.GetDigest(digestOid);
        }

        public static IDigest GetMessageDigest(string hashAlgorithm)
        {
            return DigestUtilities.GetDigest(hashAlgorithm);
        }

        public static byte[] Digest(Stream data, string hashAlgorithm)
        {
            IDigest messageDigest = GetMessageDigest(hashAlgorithm);
            return Digest(data, messageDigest);
        }

        public static byte[] Digest(Stream data, IDigest messageDigest)
        {
            byte[] array = new byte[8192];
            int length;
            while ((length = data.Read(array, 0, array.Length)) > 0)
            {
                messageDigest.BlockUpdate(array, 0, length);
            }

            byte[] array2 = new byte[messageDigest.GetDigestSize()];
            messageDigest.DoFinal(array2, 0);
            return array2;
        }

        public static string GetDigest(string oid)
        {
            if (digestNames.TryGetValue(oid, out var value))
            {
                return value;
            }

            return oid;
        }

        public static string GetAllowedDigests(string name)
        {
            allowedDigests.TryGetValue(name.ToUpperInvariant(), out var value);
            return value;
        }

        public static byte[] Digest(string algo, byte[] b, int offset, int len)
        {
            return Digest(DigestUtilities.GetDigest(algo), b, offset, len);
        }

        public static byte[] Digest(string algo, byte[] b)
        {
            return Digest(DigestUtilities.GetDigest(algo), b, 0, b.Length);
        }

        public static byte[] Digest(IDigest d, byte[] b, int offset, int len)
        {
            d.BlockUpdate(b, offset, len);
            byte[] array = new byte[d.GetDigestSize()];
            d.DoFinal(array, 0);
            return array;
        }

        public static byte[] Digest(IDigest d, byte[] b)
        {
            return Digest(d, b, 0, b.Length);
        }
    }
}
