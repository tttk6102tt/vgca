using Sign.Org.BouncyCastle.Asn1.CryptoPro;
using Sign.Org.BouncyCastle.Asn1.Nist;
using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Asn1.TeleTrust;
using Sign.Org.BouncyCastle.Asn1.X9;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Oiw;

namespace Sign.Org.BouncyCastle.Asn1.X509
{
    internal class X509SignatureUtilities
    {
        private static readonly Asn1Null derNull = DerNull.Instance;

        internal static void SetSignatureParameters(ISigner signature, Asn1Encodable parameters)
        {
            if (parameters != null)
            {
                derNull.Equals(parameters);
            }
        }

        internal static string GetSignatureName(AlgorithmIdentifier sigAlgId)
        {
            Asn1Encodable parameters = sigAlgId.Parameters;
            if (parameters != null && !derNull.Equals(parameters))
            {
                if (sigAlgId.ObjectID.Equals(PkcsObjectIdentifiers.IdRsassaPss))
                {
                    return GetDigestAlgName(RsassaPssParameters.GetInstance(parameters).HashAlgorithm.ObjectID) + "withRSAandMGF1";
                }

                if (sigAlgId.ObjectID.Equals(X9ObjectIdentifiers.ECDsaWithSha2))
                {
                    return GetDigestAlgName((DerObjectIdentifier)Asn1Sequence.GetInstance(parameters)[0]) + "withECDSA";
                }
            }

            return sigAlgId.ObjectID.Id;
        }

        private static string GetDigestAlgName(DerObjectIdentifier digestAlgOID)
        {
            if (PkcsObjectIdentifiers.MD5.Equals(digestAlgOID))
            {
                return "MD5";
            }

            if (OiwObjectIdentifiers.IdSha1.Equals(digestAlgOID))
            {
                return "SHA1";
            }

            if (NistObjectIdentifiers.IdSha224.Equals(digestAlgOID))
            {
                return "SHA224";
            }

            if (NistObjectIdentifiers.IdSha256.Equals(digestAlgOID))
            {
                return "SHA256";
            }

            if (NistObjectIdentifiers.IdSha384.Equals(digestAlgOID))
            {
                return "SHA384";
            }

            if (NistObjectIdentifiers.IdSha512.Equals(digestAlgOID))
            {
                return "SHA512";
            }

            if (TeleTrusTObjectIdentifiers.RipeMD128.Equals(digestAlgOID))
            {
                return "RIPEMD128";
            }

            if (TeleTrusTObjectIdentifiers.RipeMD160.Equals(digestAlgOID))
            {
                return "RIPEMD160";
            }

            if (TeleTrusTObjectIdentifiers.RipeMD256.Equals(digestAlgOID))
            {
                return "RIPEMD256";
            }

            if (CryptoProObjectIdentifiers.GostR3411.Equals(digestAlgOID))
            {
                return "GOST3411";
            }

            return digestAlgOID.Id;
        }
    }
}
