﻿using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.CryptoPro;
using Sign.Org.BouncyCastle.Asn1.Nist;
using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Asn1.TeleTrust;
using Sign.Org.BouncyCastle.Asn1.Utilities.Collections;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Asn1.X9;
using Sign.Org.BouncyCastle.Utilities;
using System.Collections;

namespace Sign.Org.BouncyCastle.Ocsp
{
    internal class OcspUtilities
    {
        private static readonly IDictionary algorithms;

        private static readonly IDictionary oids;

        private static readonly ISet noParams;

        internal static IEnumerable AlgNames => new EnumerableProxy(algorithms.Keys);

        static OcspUtilities()
        {
            algorithms = Platform.CreateHashtable();
            oids = Platform.CreateHashtable();
            noParams = new HashSet();
            algorithms.Add("MD2WITHRSAENCRYPTION", PkcsObjectIdentifiers.MD2WithRsaEncryption);
            algorithms.Add("MD2WITHRSA", PkcsObjectIdentifiers.MD2WithRsaEncryption);
            algorithms.Add("MD5WITHRSAENCRYPTION", PkcsObjectIdentifiers.MD5WithRsaEncryption);
            algorithms.Add("MD5WITHRSA", PkcsObjectIdentifiers.MD5WithRsaEncryption);
            algorithms.Add("SHA1WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha1WithRsaEncryption);
            algorithms.Add("SHA1WITHRSA", PkcsObjectIdentifiers.Sha1WithRsaEncryption);
            algorithms.Add("SHA224WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha224WithRsaEncryption);
            algorithms.Add("SHA224WITHRSA", PkcsObjectIdentifiers.Sha224WithRsaEncryption);
            algorithms.Add("SHA256WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha256WithRsaEncryption);
            algorithms.Add("SHA256WITHRSA", PkcsObjectIdentifiers.Sha256WithRsaEncryption);
            algorithms.Add("SHA384WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha384WithRsaEncryption);
            algorithms.Add("SHA384WITHRSA", PkcsObjectIdentifiers.Sha384WithRsaEncryption);
            algorithms.Add("SHA512WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha512WithRsaEncryption);
            algorithms.Add("SHA512WITHRSA", PkcsObjectIdentifiers.Sha512WithRsaEncryption);
            algorithms.Add("RIPEMD160WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160);
            algorithms.Add("RIPEMD160WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160);
            algorithms.Add("RIPEMD128WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128);
            algorithms.Add("RIPEMD128WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128);
            algorithms.Add("RIPEMD256WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256);
            algorithms.Add("RIPEMD256WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256);
            algorithms.Add("SHA1WITHDSA", X9ObjectIdentifiers.IdDsaWithSha1);
            algorithms.Add("DSAWITHSHA1", X9ObjectIdentifiers.IdDsaWithSha1);
            algorithms.Add("SHA224WITHDSA", NistObjectIdentifiers.DsaWithSha224);
            algorithms.Add("SHA256WITHDSA", NistObjectIdentifiers.DsaWithSha256);
            algorithms.Add("SHA1WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha1);
            algorithms.Add("ECDSAWITHSHA1", X9ObjectIdentifiers.ECDsaWithSha1);
            algorithms.Add("SHA224WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha224);
            algorithms.Add("SHA256WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha256);
            algorithms.Add("SHA384WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha384);
            algorithms.Add("SHA512WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha512);
            algorithms.Add("GOST3411WITHGOST3410", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94);
            algorithms.Add("GOST3411WITHGOST3410-94", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94);
            oids.Add(PkcsObjectIdentifiers.MD2WithRsaEncryption, "MD2WITHRSA");
            oids.Add(PkcsObjectIdentifiers.MD5WithRsaEncryption, "MD5WITHRSA");
            oids.Add(PkcsObjectIdentifiers.Sha1WithRsaEncryption, "SHA1WITHRSA");
            oids.Add(PkcsObjectIdentifiers.Sha224WithRsaEncryption, "SHA224WITHRSA");
            oids.Add(PkcsObjectIdentifiers.Sha256WithRsaEncryption, "SHA256WITHRSA");
            oids.Add(PkcsObjectIdentifiers.Sha384WithRsaEncryption, "SHA384WITHRSA");
            oids.Add(PkcsObjectIdentifiers.Sha512WithRsaEncryption, "SHA512WITHRSA");
            oids.Add(TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160, "RIPEMD160WITHRSA");
            oids.Add(TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128, "RIPEMD128WITHRSA");
            oids.Add(TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256, "RIPEMD256WITHRSA");
            oids.Add(X9ObjectIdentifiers.IdDsaWithSha1, "SHA1WITHDSA");
            oids.Add(NistObjectIdentifiers.DsaWithSha224, "SHA224WITHDSA");
            oids.Add(NistObjectIdentifiers.DsaWithSha256, "SHA256WITHDSA");
            oids.Add(X9ObjectIdentifiers.ECDsaWithSha1, "SHA1WITHECDSA");
            oids.Add(X9ObjectIdentifiers.ECDsaWithSha224, "SHA224WITHECDSA");
            oids.Add(X9ObjectIdentifiers.ECDsaWithSha256, "SHA256WITHECDSA");
            oids.Add(X9ObjectIdentifiers.ECDsaWithSha384, "SHA384WITHECDSA");
            oids.Add(X9ObjectIdentifiers.ECDsaWithSha512, "SHA512WITHECDSA");
            oids.Add(CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94, "GOST3411WITHGOST3410");
            noParams.Add(X9ObjectIdentifiers.ECDsaWithSha1);
            noParams.Add(X9ObjectIdentifiers.ECDsaWithSha224);
            noParams.Add(X9ObjectIdentifiers.ECDsaWithSha256);
            noParams.Add(X9ObjectIdentifiers.ECDsaWithSha384);
            noParams.Add(X9ObjectIdentifiers.ECDsaWithSha512);
            noParams.Add(X9ObjectIdentifiers.IdDsaWithSha1);
            noParams.Add(NistObjectIdentifiers.DsaWithSha224);
            noParams.Add(NistObjectIdentifiers.DsaWithSha256);
        }

        internal static DerObjectIdentifier GetAlgorithmOid(string algorithmName)
        {
            algorithmName = Platform.ToUpperInvariant(algorithmName);
            if (algorithms.Contains(algorithmName))
            {
                return (DerObjectIdentifier)algorithms[algorithmName];
            }

            return new DerObjectIdentifier(algorithmName);
        }

        internal static string GetAlgorithmName(DerObjectIdentifier oid)
        {
            if (oids.Contains(oid))
            {
                return (string)oids[oid];
            }

            return oid.Id;
        }

        internal static AlgorithmIdentifier GetSigAlgID(DerObjectIdentifier sigOid)
        {
            if (noParams.Contains(sigOid))
            {
                return new AlgorithmIdentifier(sigOid);
            }

            return new AlgorithmIdentifier(sigOid, DerNull.Instance);
        }
    }
}
