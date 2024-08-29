using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.Cms;
using Sign.Org.BouncyCastle.Asn1.CryptoPro;
using Sign.Org.BouncyCastle.Asn1.Nist;
using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Asn1.TeleTrust;
using Sign.Org.BouncyCastle.Asn1.Utilities.Collections;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Asn1.X9;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Oiw;
using Sign.Org.BouncyCastle.Security;
using Sign.Org.BouncyCastle.Utilities;
using Sign.Org.BouncyCastle.X509;
using Sign.Org.BouncyCastle.X509.Store;
using System.Collections;

namespace Sign.Org.BouncyCastle.Cms
{
    public class CmsSignedGenerator
    {
        public static readonly string Data;

        public static readonly string DigestSha1;

        public static readonly string DigestSha224;

        public static readonly string DigestSha256;

        public static readonly string DigestSha384;

        public static readonly string DigestSha512;

        public static readonly string DigestMD5;

        public static readonly string DigestGost3411;

        public static readonly string DigestRipeMD128;

        public static readonly string DigestRipeMD160;

        public static readonly string DigestRipeMD256;

        public static readonly string EncryptionRsa;

        public static readonly string EncryptionDsa;

        public static readonly string EncryptionECDsa;

        public static readonly string EncryptionRsaPss;

        public static readonly string EncryptionGost3410;

        public static readonly string EncryptionECGost3410;

        private static readonly string EncryptionECDsaWithSha1;

        private static readonly string EncryptionECDsaWithSha224;

        private static readonly string EncryptionECDsaWithSha256;

        private static readonly string EncryptionECDsaWithSha384;

        private static readonly string EncryptionECDsaWithSha512;

        private static readonly ISet noParams;

        private static readonly IDictionary ecAlgorithms;

        internal IList _certs = Platform.CreateArrayList();

        internal IList _crls = Platform.CreateArrayList();

        internal IList _signers = Platform.CreateArrayList();

        internal IDictionary _digests = Platform.CreateHashtable();

        protected readonly SecureRandom rand;

        static CmsSignedGenerator()
        {
            Data = CmsObjectIdentifiers.Data.Id;
            DigestSha1 = OiwObjectIdentifiers.IdSha1.Id;
            DigestSha224 = NistObjectIdentifiers.IdSha224.Id;
            DigestSha256 = NistObjectIdentifiers.IdSha256.Id;
            DigestSha384 = NistObjectIdentifiers.IdSha384.Id;
            DigestSha512 = NistObjectIdentifiers.IdSha512.Id;
            DigestMD5 = PkcsObjectIdentifiers.MD5.Id;
            DigestGost3411 = CryptoProObjectIdentifiers.GostR3411.Id;
            DigestRipeMD128 = TeleTrusTObjectIdentifiers.RipeMD128.Id;
            DigestRipeMD160 = TeleTrusTObjectIdentifiers.RipeMD160.Id;
            DigestRipeMD256 = TeleTrusTObjectIdentifiers.RipeMD256.Id;
            EncryptionRsa = PkcsObjectIdentifiers.RsaEncryption.Id;
            EncryptionDsa = X9ObjectIdentifiers.IdDsaWithSha1.Id;
            EncryptionECDsa = X9ObjectIdentifiers.ECDsaWithSha1.Id;
            EncryptionRsaPss = PkcsObjectIdentifiers.IdRsassaPss.Id;
            EncryptionGost3410 = CryptoProObjectIdentifiers.GostR3410x94.Id;
            EncryptionECGost3410 = CryptoProObjectIdentifiers.GostR3410x2001.Id;
            EncryptionECDsaWithSha1 = X9ObjectIdentifiers.ECDsaWithSha1.Id;
            EncryptionECDsaWithSha224 = X9ObjectIdentifiers.ECDsaWithSha224.Id;
            EncryptionECDsaWithSha256 = X9ObjectIdentifiers.ECDsaWithSha256.Id;
            EncryptionECDsaWithSha384 = X9ObjectIdentifiers.ECDsaWithSha384.Id;
            EncryptionECDsaWithSha512 = X9ObjectIdentifiers.ECDsaWithSha512.Id;
            noParams = new HashSet();
            ecAlgorithms = Platform.CreateHashtable();
            noParams.Add(EncryptionDsa);
            noParams.Add(EncryptionECDsaWithSha1);
            noParams.Add(EncryptionECDsaWithSha224);
            noParams.Add(EncryptionECDsaWithSha256);
            noParams.Add(EncryptionECDsaWithSha384);
            noParams.Add(EncryptionECDsaWithSha512);
            ecAlgorithms.Add(DigestSha1, EncryptionECDsaWithSha1);
            ecAlgorithms.Add(DigestSha224, EncryptionECDsaWithSha224);
            ecAlgorithms.Add(DigestSha256, EncryptionECDsaWithSha256);
            ecAlgorithms.Add(DigestSha384, EncryptionECDsaWithSha384);
            ecAlgorithms.Add(DigestSha512, EncryptionECDsaWithSha512);
        }

        protected CmsSignedGenerator()
            : this(new SecureRandom())
        {
        }

        protected CmsSignedGenerator(SecureRandom rand)
        {
            this.rand = rand;
        }

        protected string GetEncOid(AsymmetricKeyParameter key, string digestOID)
        {
            string text = null;
            if (key is RsaKeyParameters)
            {
                if (!((RsaKeyParameters)key).IsPrivate)
                {
                    throw new ArgumentException("Expected RSA private key");
                }

                text = EncryptionRsa;
            }
            else if (key is DsaPrivateKeyParameters)
            {
                if (!digestOID.Equals(DigestSha1))
                {
                    throw new ArgumentException("can't mix DSA with anything but SHA1");
                }

                text = EncryptionDsa;
            }
            else if (key is ECPrivateKeyParameters)
            {
                if (((ECPrivateKeyParameters)key).AlgorithmName == "ECGOST3410")
                {
                    text = EncryptionECGost3410;
                }
                else
                {
                    text = (string)ecAlgorithms[digestOID];
                    if (text == null)
                    {
                        throw new ArgumentException("can't mix ECDSA with anything but SHA family digests");
                    }
                }
            }
            else
            {
                if (!(key is Gost3410PrivateKeyParameters))
                {
                    throw new ArgumentException("Unknown algorithm in CmsSignedGenerator.GetEncOid");
                }

                text = EncryptionGost3410;
            }

            return text;
        }

        internal static AlgorithmIdentifier GetEncAlgorithmIdentifier(DerObjectIdentifier encOid, Asn1Encodable sigX509Parameters)
        {
            if (noParams.Contains(encOid.Id))
            {
                return new AlgorithmIdentifier(encOid);
            }

            return new AlgorithmIdentifier(encOid, sigX509Parameters);
        }

        protected internal virtual IDictionary GetBaseParameters(DerObjectIdentifier contentType, AlgorithmIdentifier digAlgId, byte[] hash)
        {
            IDictionary dictionary = Platform.CreateHashtable();
            if (contentType != null)
            {
                dictionary[CmsAttributeTableParameter.ContentType] = contentType;
            }

            dictionary[CmsAttributeTableParameter.DigestAlgorithmIdentifier] = digAlgId;
            dictionary[CmsAttributeTableParameter.Digest] = hash.Clone();
            return dictionary;
        }

        protected internal virtual Asn1Set GetAttributeSet(Org.BouncyCastle.Asn1.Cms.AttributeTable attr)
        {
            if (attr != null)
            {
                return new DerSet(attr.ToAsn1EncodableVector());
            }

            return null;
        }

        public void AddCertificates(IX509Store certStore)
        {
            CollectionUtilities.AddRange(_certs, CmsUtilities.GetCertificatesFromStore(certStore));
        }

        public void AddCrls(IX509Store crlStore)
        {
            CollectionUtilities.AddRange(_crls, CmsUtilities.GetCrlsFromStore(crlStore));
        }

        public void AddAttributeCertificates(IX509Store store)
        {
            try
            {
                foreach (IX509AttributeCertificate match in store.GetMatches(null))
                {
                    _certs.Add(new DerTaggedObject(explicitly: false, 2, AttributeCertificate.GetInstance(Asn1Object.FromByteArray(match.GetEncoded()))));
                }
            }
            catch (Exception e)
            {
                throw new CmsException("error processing attribute certs", e);
            }
        }

        public void AddSigners(SignerInformationStore signerStore)
        {
            foreach (SignerInformation signer in signerStore.GetSigners())
            {
                _signers.Add(signer);
                AddSignerCallback(signer);
            }
        }

        public IDictionary GetGeneratedDigests()
        {
            return Platform.CreateHashtable(_digests);
        }

        internal virtual void AddSignerCallback(SignerInformation si)
        {
        }

        internal static SignerIdentifier GetSignerIdentifier(X509Certificate cert)
        {
            return new SignerIdentifier(CmsUtilities.GetIssuerAndSerialNumber(cert));
        }

        internal static SignerIdentifier GetSignerIdentifier(byte[] subjectKeyIdentifier)
        {
            return new SignerIdentifier(new DerOctetString(subjectKeyIdentifier));
        }
    }
}
