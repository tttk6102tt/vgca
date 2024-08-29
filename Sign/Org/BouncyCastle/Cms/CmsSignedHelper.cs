using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.CryptoPro;
using Sign.Org.BouncyCastle.Asn1.Eac;
using Sign.Org.BouncyCastle.Asn1.Nist;
using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Asn1.TeleTrust;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Asn1.X9;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Oiw;
using Sign.Org.BouncyCastle.Security;
using Sign.Org.BouncyCastle.Utilities;
using Sign.Org.BouncyCastle.X509;
using Sign.Org.BouncyCastle.X509.Store;
using System.Collections;

namespace Sign.Org.BouncyCastle.Cms
{
    internal class CmsSignedHelper
    {
        internal static readonly CmsSignedHelper Instance;

        private static readonly IDictionary encryptionAlgs;

        private static readonly IDictionary digestAlgs;

        private static readonly IDictionary digestAliases;

        private static void AddEntries(DerObjectIdentifier oid, string digest, string encryption)
        {
            string id = oid.Id;
            digestAlgs.Add(id, digest);
            encryptionAlgs.Add(id, encryption);
        }

        static CmsSignedHelper()
        {
            Instance = new CmsSignedHelper();
            encryptionAlgs = Platform.CreateHashtable();
            digestAlgs = Platform.CreateHashtable();
            digestAliases = Platform.CreateHashtable();
            AddEntries(NistObjectIdentifiers.DsaWithSha224, "SHA224", "DSA");
            AddEntries(NistObjectIdentifiers.DsaWithSha256, "SHA256", "DSA");
            AddEntries(NistObjectIdentifiers.DsaWithSha384, "SHA384", "DSA");
            AddEntries(NistObjectIdentifiers.DsaWithSha512, "SHA512", "DSA");
            AddEntries(OiwObjectIdentifiers.DsaWithSha1, "SHA1", "DSA");
            AddEntries(OiwObjectIdentifiers.MD4WithRsa, "MD4", "RSA");
            AddEntries(OiwObjectIdentifiers.MD4WithRsaEncryption, "MD4", "RSA");
            AddEntries(OiwObjectIdentifiers.MD5WithRsa, "MD5", "RSA");
            AddEntries(OiwObjectIdentifiers.Sha1WithRsa, "SHA1", "RSA");
            AddEntries(PkcsObjectIdentifiers.MD2WithRsaEncryption, "MD2", "RSA");
            AddEntries(PkcsObjectIdentifiers.MD4WithRsaEncryption, "MD4", "RSA");
            AddEntries(PkcsObjectIdentifiers.MD5WithRsaEncryption, "MD5", "RSA");
            AddEntries(PkcsObjectIdentifiers.Sha1WithRsaEncryption, "SHA1", "RSA");
            AddEntries(PkcsObjectIdentifiers.Sha224WithRsaEncryption, "SHA224", "RSA");
            AddEntries(PkcsObjectIdentifiers.Sha256WithRsaEncryption, "SHA256", "RSA");
            AddEntries(PkcsObjectIdentifiers.Sha384WithRsaEncryption, "SHA384", "RSA");
            AddEntries(PkcsObjectIdentifiers.Sha512WithRsaEncryption, "SHA512", "RSA");
            AddEntries(X9ObjectIdentifiers.ECDsaWithSha1, "SHA1", "ECDSA");
            AddEntries(X9ObjectIdentifiers.ECDsaWithSha224, "SHA224", "ECDSA");
            AddEntries(X9ObjectIdentifiers.ECDsaWithSha256, "SHA256", "ECDSA");
            AddEntries(X9ObjectIdentifiers.ECDsaWithSha384, "SHA384", "ECDSA");
            AddEntries(X9ObjectIdentifiers.ECDsaWithSha512, "SHA512", "ECDSA");
            AddEntries(X9ObjectIdentifiers.IdDsaWithSha1, "SHA1", "DSA");
            AddEntries(EacObjectIdentifiers.id_TA_ECDSA_SHA_1, "SHA1", "ECDSA");
            AddEntries(EacObjectIdentifiers.id_TA_ECDSA_SHA_224, "SHA224", "ECDSA");
            AddEntries(EacObjectIdentifiers.id_TA_ECDSA_SHA_256, "SHA256", "ECDSA");
            AddEntries(EacObjectIdentifiers.id_TA_ECDSA_SHA_384, "SHA384", "ECDSA");
            AddEntries(EacObjectIdentifiers.id_TA_ECDSA_SHA_512, "SHA512", "ECDSA");
            AddEntries(EacObjectIdentifiers.id_TA_RSA_v1_5_SHA_1, "SHA1", "RSA");
            AddEntries(EacObjectIdentifiers.id_TA_RSA_v1_5_SHA_256, "SHA256", "RSA");
            AddEntries(EacObjectIdentifiers.id_TA_RSA_PSS_SHA_1, "SHA1", "RSAandMGF1");
            AddEntries(EacObjectIdentifiers.id_TA_RSA_PSS_SHA_256, "SHA256", "RSAandMGF1");
            encryptionAlgs.Add(X9ObjectIdentifiers.IdDsa.Id, "DSA");
            encryptionAlgs.Add(PkcsObjectIdentifiers.RsaEncryption.Id, "RSA");
            encryptionAlgs.Add(TeleTrusTObjectIdentifiers.TeleTrusTRsaSignatureAlgorithm, "RSA");
            encryptionAlgs.Add(X509ObjectIdentifiers.IdEARsa.Id, "RSA");
            encryptionAlgs.Add(CmsSignedGenerator.EncryptionRsaPss, "RSAandMGF1");
            encryptionAlgs.Add(CryptoProObjectIdentifiers.GostR3410x94.Id, "GOST3410");
            encryptionAlgs.Add(CryptoProObjectIdentifiers.GostR3410x2001.Id, "ECGOST3410");
            encryptionAlgs.Add("1.3.6.1.4.1.5849.1.6.2", "ECGOST3410");
            encryptionAlgs.Add("1.3.6.1.4.1.5849.1.1.5", "GOST3410");
            digestAlgs.Add(PkcsObjectIdentifiers.MD2.Id, "MD2");
            digestAlgs.Add(PkcsObjectIdentifiers.MD4.Id, "MD4");
            digestAlgs.Add(PkcsObjectIdentifiers.MD5.Id, "MD5");
            digestAlgs.Add(OiwObjectIdentifiers.IdSha1.Id, "SHA1");
            digestAlgs.Add(NistObjectIdentifiers.IdSha224.Id, "SHA224");
            digestAlgs.Add(NistObjectIdentifiers.IdSha256.Id, "SHA256");
            digestAlgs.Add(NistObjectIdentifiers.IdSha384.Id, "SHA384");
            digestAlgs.Add(NistObjectIdentifiers.IdSha512.Id, "SHA512");
            digestAlgs.Add(TeleTrusTObjectIdentifiers.RipeMD128.Id, "RIPEMD128");
            digestAlgs.Add(TeleTrusTObjectIdentifiers.RipeMD160.Id, "RIPEMD160");
            digestAlgs.Add(TeleTrusTObjectIdentifiers.RipeMD256.Id, "RIPEMD256");
            digestAlgs.Add(CryptoProObjectIdentifiers.GostR3411.Id, "GOST3411");
            digestAlgs.Add("1.3.6.1.4.1.5849.1.2.1", "GOST3411");
            digestAliases.Add("SHA1", new string[1] { "SHA-1" });
            digestAliases.Add("SHA224", new string[1] { "SHA-224" });
            digestAliases.Add("SHA256", new string[1] { "SHA-256" });
            digestAliases.Add("SHA384", new string[1] { "SHA-384" });
            digestAliases.Add("SHA512", new string[1] { "SHA-512" });
        }

        internal string GetDigestAlgName(string digestAlgOid)
        {
            string text = (string)digestAlgs[digestAlgOid];
            if (text != null)
            {
                return text;
            }

            return digestAlgOid;
        }

        internal string[] GetDigestAliases(string algName)
        {
            string[] array = (string[])digestAliases[algName];
            if (array != null)
            {
                return (string[])array.Clone();
            }

            return new string[0];
        }

        internal string GetEncryptionAlgName(string encryptionAlgOid)
        {
            string text = (string)encryptionAlgs[encryptionAlgOid];
            if (text != null)
            {
                return text;
            }

            return encryptionAlgOid;
        }

        internal IDigest GetDigestInstance(string algorithm)
        {
            try
            {
                return DigestUtilities.GetDigest(algorithm);
            }
            catch (SecurityUtilityException ex2)
            {
                string[] array = GetDigestAliases(algorithm);
                foreach (string algorithm2 in array)
                {
                    try
                    {
                        return DigestUtilities.GetDigest(algorithm2);
                    }
                    catch (SecurityUtilityException)
                    {
                    }
                }

                throw ex2;
            }
        }

        internal ISigner GetSignatureInstance(string algorithm)
        {
            return SignerUtilities.GetSigner(algorithm);
        }

        internal IX509Store CreateAttributeStore(string type, Asn1Set certSet)
        {
            IList list = Platform.CreateArrayList();
            if (certSet != null)
            {
                foreach (Asn1Encodable item in certSet)
                {
                    try
                    {
                        Asn1Object asn1Object = item.ToAsn1Object();
                        if (asn1Object is Asn1TaggedObject)
                        {
                            Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)asn1Object;
                            if (asn1TaggedObject.TagNo == 2)
                            {
                                list.Add(new X509V2AttributeCertificate(Asn1Sequence.GetInstance(asn1TaggedObject, explicitly: false).GetEncoded()));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new CmsException("can't re-encode attribute certificate!", e);
                    }
                }
            }

            try
            {
                return X509StoreFactory.Create("AttributeCertificate/" + type, new X509CollectionStoreParameters(list));
            }
            catch (ArgumentException e2)
            {
                throw new CmsException("can't setup the X509Store", e2);
            }
        }

        internal IX509Store CreateCertificateStore(string type, Asn1Set certSet)
        {
            IList list = Platform.CreateArrayList();
            if (certSet != null)
            {
                AddCertsFromSet(list, certSet);
            }

            try
            {
                return X509StoreFactory.Create("Certificate/" + type, new X509CollectionStoreParameters(list));
            }
            catch (ArgumentException e)
            {
                throw new CmsException("can't setup the X509Store", e);
            }
        }

        internal IX509Store CreateCrlStore(string type, Asn1Set crlSet)
        {
            IList list = Platform.CreateArrayList();
            if (crlSet != null)
            {
                AddCrlsFromSet(list, crlSet);
            }

            try
            {
                return X509StoreFactory.Create("CRL/" + type, new X509CollectionStoreParameters(list));
            }
            catch (ArgumentException e)
            {
                throw new CmsException("can't setup the X509Store", e);
            }
        }

        private void AddCertsFromSet(IList certs, Asn1Set certSet)
        {
            X509CertificateParser x509CertificateParser = new X509CertificateParser();
            foreach (Asn1Encodable item in certSet)
            {
                try
                {
                    Asn1Object asn1Object = item.ToAsn1Object();
                    if (asn1Object is Asn1Sequence)
                    {
                        certs.Add(x509CertificateParser.ReadCertificate(asn1Object.GetEncoded()));
                    }
                }
                catch (Exception e)
                {
                    throw new CmsException("can't re-encode certificate!", e);
                }
            }
        }

        private void AddCrlsFromSet(IList crls, Asn1Set crlSet)
        {
            X509CrlParser x509CrlParser = new X509CrlParser();
            foreach (Asn1Encodable item in crlSet)
            {
                try
                {
                    crls.Add(x509CrlParser.ReadCrl(item.GetEncoded()));
                }
                catch (Exception e)
                {
                    throw new CmsException("can't re-encode CRL!", e);
                }
            }
        }

        internal AlgorithmIdentifier FixAlgID(AlgorithmIdentifier algId)
        {
            if (algId.Parameters == null)
            {
                return new AlgorithmIdentifier(algId.ObjectID, DerNull.Instance);
            }

            return algId;
        }
    }
}
