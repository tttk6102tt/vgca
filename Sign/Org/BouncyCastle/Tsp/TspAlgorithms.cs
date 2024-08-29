using Sign.Org.BouncyCastle.Asn1.CryptoPro;
using Sign.Org.BouncyCastle.Asn1.Nist;
using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Asn1.TeleTrust;
using Sign.Org.BouncyCastle.Oiw;
using Sign.Org.BouncyCastle.Utilities;
using System.Collections;

namespace Sign.Org.BouncyCastle.Tsp
{
    public abstract class TspAlgorithms
    {
        public static readonly string MD5;

        public static readonly string Sha1;

        public static readonly string Sha224;

        public static readonly string Sha256;

        public static readonly string Sha384;

        public static readonly string Sha512;

        public static readonly string RipeMD128;

        public static readonly string RipeMD160;

        public static readonly string RipeMD256;

        public static readonly string Gost3411;

        public static readonly IList Allowed;

        static TspAlgorithms()
        {
            MD5 = PkcsObjectIdentifiers.MD5.Id;
            Sha1 = OiwObjectIdentifiers.IdSha1.Id;
            Sha224 = NistObjectIdentifiers.IdSha224.Id;
            Sha256 = NistObjectIdentifiers.IdSha256.Id;
            Sha384 = NistObjectIdentifiers.IdSha384.Id;
            Sha512 = NistObjectIdentifiers.IdSha512.Id;
            RipeMD128 = TeleTrusTObjectIdentifiers.RipeMD128.Id;
            RipeMD160 = TeleTrusTObjectIdentifiers.RipeMD160.Id;
            RipeMD256 = TeleTrusTObjectIdentifiers.RipeMD256.Id;
            Gost3411 = CryptoProObjectIdentifiers.GostR3411.Id;
            string[] obj = new string[10] { Gost3411, MD5, Sha1, Sha224, Sha256, Sha384, Sha512, RipeMD128, RipeMD160, RipeMD256 };
            Allowed = Platform.CreateArrayList();
            string[] array = obj;
            foreach (string value in array)
            {
                Allowed.Add(value);
            }
        }
    }
}
