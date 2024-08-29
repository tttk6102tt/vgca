using Sign.itext.error_messages;
using Sign.itext.text.pdf;
using Sign.itext.text.pdf.security;
using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.Ess;
using Sign.Org.BouncyCastle.Asn1.Ocsp;
using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Ocsp;
using Sign.Org.BouncyCastle.Security;
using Sign.Org.BouncyCastle.Tsp;
using Sign.Org.BouncyCastle.Utilities;
using Sign.Org.BouncyCastle.X509;
using System.Collections;

namespace Sign.itext.pdf.security
{
    public class PdfPKCS7
    {
        private string signName;

        private string reason;

        private string location;

        private DateTime signDate;

        private int version = 1;

        private int signerversion = 1;

        private string digestAlgorithmOid;

        private IDigest messageDigest;

        private Dictionary<string, object> digestalgos;

        private byte[] digestAttr;

        private PdfName filterSubtype;

        private string digestEncryptionAlgorithmOid;

        private byte[] externalDigest;

        private byte[] externalRSAdata;

        private ISigner sig;

        private byte[] digest;

        private byte[] RSAdata;

        private byte[] sigAttr;

        private byte[] sigAttrDer;

        private IDigest encContDigest;

        private bool verified;

        private bool verifyResult;

        private List<X509Certificate> certs;

        private ICollection<X509Certificate> signCerts;

        private X509Certificate signCert;

        private ICollection<X509Crl> crls;

        private BasicOcspResp basicResp;

        private bool isTsp;

        private bool isCades;

        private TimeStampToken timeStampToken;

        public virtual string SignName
        {
            get
            {
                return signName;
            }
            set
            {
                signName = value;
            }
        }

        public virtual string Reason
        {
            get
            {
                return reason;
            }
            set
            {
                reason = value;
            }
        }

        public virtual string Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }

        public virtual DateTime SignDate
        {
            get
            {
                return signDate;
            }
            set
            {
                signDate = value;
            }
        }

        public virtual int Version => version;

        public virtual int SigningInfoVersion => signerversion;

        public virtual string DigestAlgorithmOid => digestAlgorithmOid;

        public virtual string DigestEncryptionAlgorithmOid => digestEncryptionAlgorithmOid;

        public virtual X509Certificate[] Certificates
        {
            get
            {
                X509Certificate[] array = new X509Certificate[certs.Count];
                certs.CopyTo(array, 0);
                return array;
            }
        }

        public virtual X509Certificate[] SignCertificateChain
        {
            get
            {
                X509Certificate[] array = new X509Certificate[signCerts.Count];
                signCerts.CopyTo(array, 0);
                return array;
            }
        }

        public virtual X509Certificate SigningCertificate => signCert;

        public virtual ICollection<X509Crl> CRLs => crls;

        public virtual BasicOcspResp Ocsp => basicResp;

        public virtual bool IsTsp => isTsp;

        public virtual TimeStampToken TimeStampToken => timeStampToken;

        public virtual DateTime TimeStampDate
        {
            get
            {
                if (timeStampToken == null)
                {
                    return DateTime.MaxValue;
                }

                return timeStampToken.TimeStampInfo.GenTime;
            }
        }

        public PdfPKCS7(ICipherParameters privKey, ICollection<X509Certificate> certChain, string hashAlgorithm, bool hasRSAdata)
        {
            digestAlgorithmOid = DigestAlgorithms.GetAllowedDigests(hashAlgorithm);
            if (digestAlgorithmOid == null)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("unknown.hash.algorithm.1", hashAlgorithm));
            }

            version = (signerversion = 1);
            certs = new List<X509Certificate>(certChain);
            crls = new List<X509Crl>();
            digestalgos = new Dictionary<string, object>();
            digestalgos[digestAlgorithmOid] = null;
            signCert = certs[0];
            if (privKey != null)
            {
                if (privKey is RsaKeyParameters)
                {
                    digestEncryptionAlgorithmOid = "1.2.840.113549.1.1.1";
                }
                else
                {
                    if (!(privKey is DsaKeyParameters))
                    {
                        throw new ArgumentException(MessageLocalization.GetComposedMessage("unknown.key.algorithm.1", privKey.ToString()));
                    }

                    digestEncryptionAlgorithmOid = "1.2.840.10040.4.1";
                }
            }

            if (hasRSAdata)
            {
                RSAdata = new byte[0];
                messageDigest = GetHashClass();
            }

            if (privKey != null)
            {
                sig = InitSignature(privKey);
            }
        }

        internal IDigest GetHashClass()
        {
            return DigestUtilities.GetDigest(GetHashAlgorithm());
        }

        public PdfPKCS7(byte[] contentsKey, byte[] certsKey)
        {
            X509CertificateParser x509CertificateParser = new X509CertificateParser();
            certs = new List<X509Certificate>();
            foreach (X509Certificate item in x509CertificateParser.ReadCertificates(certsKey))
            {
                if (signCert != null)
                {
                    signCert = item;
                }

                certs.Add(item);
            }

            signCerts = certs;
            crls = new List<X509Crl>();
            Asn1InputStream asn1InputStream = new Asn1InputStream(new MemoryStream(contentsKey));
            digest = ((Asn1OctetString)asn1InputStream.ReadObject()).GetOctets();
            sig = SignerUtilities.GetSigner("SHA1withRSA");
            sig.Init(forSigning: false, signCert.GetPublicKey());
            digestAlgorithmOid = "1.2.840.10040.4.3";
            digestEncryptionAlgorithmOid = "1.3.36.3.3.1.2";
        }

        public PdfPKCS7(byte[] contentsKey, PdfName filterSubtype)
        {
            this.filterSubtype = filterSubtype;
            isTsp = PdfName.ETSI_RFC3161.Equals(filterSubtype);
            isCades = PdfName.ETSI_CADES_DETACHED.Equals(filterSubtype);
            Asn1InputStream asn1InputStream = new Asn1InputStream(new MemoryStream(contentsKey));
            Asn1Object asn1Object;
            try
            {
                asn1Object = asn1InputStream.ReadObject();
            }
            catch
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("can.t.decode.pkcs7signeddata.object"));
            }

            if (!(asn1Object is Asn1Sequence))
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("not.a.valid.pkcs.7.object.not.a.sequence"));
            }

            Asn1Sequence asn1Sequence = (Asn1Sequence)asn1Object;
            if (!((DerObjectIdentifier)asn1Sequence[0]).Id.Equals("1.2.840.113549.1.7.2"))
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("not.a.valid.pkcs.7.object.not.signed.data"));
            }

            Asn1Sequence asn1Sequence2 = (Asn1Sequence)((Asn1TaggedObject)asn1Sequence[1]).GetObject();
            version = ((DerInteger)asn1Sequence2[0]).Value.IntValue;
            digestalgos = new Dictionary<string, object>();
            IEnumerator enumerator = ((Asn1Set)asn1Sequence2[1]).GetEnumerator();
            while (enumerator.MoveNext())
            {
                DerObjectIdentifier derObjectIdentifier = (DerObjectIdentifier)((Asn1Sequence)enumerator.Current)[0];
                digestalgos[derObjectIdentifier.Id] = null;
            }

            X509CertificateParser x509CertificateParser = new X509CertificateParser();
            certs = new List<X509Certificate>();
            foreach (X509Certificate item in x509CertificateParser.ReadCertificates(contentsKey))
            {
                certs.Add(item);
            }

            crls = new List<X509Crl>();
            Asn1Sequence asn1Sequence3 = (Asn1Sequence)asn1Sequence2[2];
            if (asn1Sequence3.Count > 1)
            {
                Asn1OctetString asn1OctetString = (Asn1OctetString)((Asn1TaggedObject)asn1Sequence3[1]).GetObject();
                RSAdata = asn1OctetString.GetOctets();
            }

            int i;
            for (i = 3; asn1Sequence2[i] is Asn1TaggedObject; i++)
            {
            }

            Asn1Set asn1Set = (Asn1Set)asn1Sequence2[i];
            if (asn1Set.Count != 1)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("this.pkcs.7.object.has.multiple.signerinfos.only.one.is.supported.at.this.time"));
            }

            Asn1Sequence asn1Sequence4 = (Asn1Sequence)asn1Set[0];
            signerversion = ((DerInteger)asn1Sequence4[0]).Value.IntValue;
            Asn1Sequence obj2 = (Asn1Sequence)asn1Sequence4[1];
            X509Name instance = X509Name.GetInstance(obj2[0]);
            BigInteger value = ((DerInteger)obj2[1]).Value;
            foreach (X509Certificate cert in certs)
            {
                if (instance.Equivalent(cert.IssuerDN) && value.Equals(cert.SerialNumber))
                {
                    signCert = cert;
                    break;
                }
            }

            if (signCert == null)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("can.t.find.signing.certificate.with.serial.1", instance.ToString() + " / " + value.ToString(16)));
            }

            CalcSignCertificateChain();
            digestAlgorithmOid = ((DerObjectIdentifier)((Asn1Sequence)asn1Sequence4[2])[0]).Id;
            i = 3;
            if (asn1Sequence4[i] is Asn1TaggedObject)
            {
                Asn1Set instance2 = Asn1Set.GetInstance((Asn1TaggedObject)asn1Sequence4[i], explicitly: false);
                sigAttr = instance2.GetEncoded();
                sigAttrDer = instance2.GetEncoded("DER");
                for (int j = 0; j < instance2.Count; j++)
                {
                    Asn1Sequence asn1Sequence5 = (Asn1Sequence)instance2[j];
                    string id = ((DerObjectIdentifier)asn1Sequence5[0]).Id;
                    if (id.Equals("1.2.840.113549.1.9.4"))
                    {
                        Asn1Set asn1Set2 = (Asn1Set)asn1Sequence5[1];
                        digestAttr = ((DerOctetString)asn1Set2[0]).GetOctets();
                    }
                    else if (id.Equals("1.2.840.113583.1.1.8"))
                    {
                        Asn1Sequence asn1Sequence6 = (Asn1Sequence)((Asn1Set)asn1Sequence5[1])[0];
                        for (int k = 0; k < asn1Sequence6.Count; k++)
                        {
                            Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)asn1Sequence6[k];
                            if (asn1TaggedObject.TagNo == 1)
                            {
                                Asn1Sequence seq = (Asn1Sequence)asn1TaggedObject.GetObject();
                                FindOcsp(seq);
                            }

                            if (asn1TaggedObject.TagNo == 0)
                            {
                                Asn1Sequence seq2 = (Asn1Sequence)asn1TaggedObject.GetObject();
                                FindCRL(seq2);
                            }
                        }
                    }
                    else if (isCades && id.Equals("1.2.840.113549.1.9.16.2.12"))
                    {
                        EssCertID essCertID = Org.BouncyCastle.Asn1.Ess.SigningCertificate.GetInstance((Asn1Sequence)((Asn1Set)asn1Sequence5[1])[0]).GetCerts()[0];
                        byte[] encoded = signCert.GetEncoded();
                        byte[] a = DigestAlgorithms.Digest(DigestUtilities.GetDigest("SHA-1"), encoded);
                        byte[] certHash = essCertID.GetCertHash();
                        if (!Arrays.AreEqual(a, certHash))
                        {
                            throw new ArgumentException("Signing certificate doesn't match the ESS information.");
                        }
                    }
                    else if (isCades && id.Equals("1.2.840.113549.1.9.16.2.47"))
                    {
                        EssCertIDv2 essCertIDv = SigningCertificateV2.GetInstance((Asn1Sequence)((Asn1Set)asn1Sequence5[1])[0]).GetCerts()[0];
                        AlgorithmIdentifier hashAlgorithm = essCertIDv.HashAlgorithm;
                        byte[] a2 = DigestAlgorithms.Digest(b: signCert.GetEncoded(), d: DigestUtilities.GetDigest(hashAlgorithm.ObjectID.Id));
                        byte[] certHash2 = essCertIDv.GetCertHash();
                        if (!Arrays.AreEqual(a2, certHash2))
                        {
                            throw new ArgumentException("Signing certificate doesn't match the ESS information.");
                        }
                    }
                }

                if (digestAttr == null)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("authenticated.attribute.is.missing.the.digest"));
                }

                i++;
            }

            digestEncryptionAlgorithmOid = ((DerObjectIdentifier)((Asn1Sequence)asn1Sequence4[i++])[0]).Id;
            digest = ((Asn1OctetString)asn1Sequence4[i++]).GetOctets();
            if (i < asn1Sequence4.Count && asn1Sequence4[i] is DerTaggedObject)
            {
                Org.BouncyCastle.Asn1.Cms.Attribute attribute = new Org.BouncyCastle.Asn1.Cms.AttributeTable(Asn1Set.GetInstance((Asn1TaggedObject)asn1Sequence4[i], explicitly: false))[PkcsObjectIdentifiers.IdAASignatureTimeStampToken];
                if (attribute != null && attribute.AttrValues.Count > 0)
                {
                    Org.BouncyCastle.Asn1.Cms.ContentInfo instance3 = Org.BouncyCastle.Asn1.Cms.ContentInfo.GetInstance(Asn1Sequence.GetInstance(attribute.AttrValues[0]));
                    timeStampToken = new TimeStampToken(instance3);
                }
            }

            if (isTsp)
            {
                Org.BouncyCastle.Asn1.Cms.ContentInfo instance4 = Org.BouncyCastle.Asn1.Cms.ContentInfo.GetInstance(asn1Sequence);
                timeStampToken = new TimeStampToken(instance4);
                string messageImprintAlgOid = timeStampToken.TimeStampInfo.MessageImprintAlgOid;
                messageDigest = DigestUtilities.GetDigest(messageImprintAlgOid);
                return;
            }

            if (RSAdata != null || digestAttr != null)
            {
                if (PdfName.ADBE_PKCS7_SHA1.Equals(GetFilterSubtype()))
                {
                    messageDigest = DigestUtilities.GetDigest("SHA1");
                }
                else
                {
                    messageDigest = GetHashClass();
                }

                encContDigest = GetHashClass();
            }

            sig = InitSignature(signCert.GetPublicKey());
        }

        public virtual string GetHashAlgorithm()
        {
            return DigestAlgorithms.GetDigest(digestAlgorithmOid);
        }

        public virtual string GetDigestAlgorithm()
        {
            return GetHashAlgorithm() + "with" + GetEncryptionAlgorithm();
        }

        public virtual void SetExternalDigest(byte[] digest, byte[] RSAdata, string digestEncryptionAlgorithm)
        {
            externalDigest = digest;
            externalRSAdata = RSAdata;
            if (digestEncryptionAlgorithm == null)
            {
                return;
            }

            if (digestEncryptionAlgorithm.Equals("RSA"))
            {
                digestEncryptionAlgorithmOid = "1.2.840.113549.1.1.1";
                return;
            }

            if (digestEncryptionAlgorithm.Equals("DSA"))
            {
                digestEncryptionAlgorithmOid = "1.2.840.10040.4.1";
                return;
            }

            if (digestEncryptionAlgorithm.Equals("ECDSA"))
            {
                digestEncryptionAlgorithmOid = "1.2.840.10045.2.1";
                return;
            }

            throw new ArgumentException(MessageLocalization.GetComposedMessage("unknown.key.algorithm.1", digestEncryptionAlgorithm));
        }

        private ISigner InitSignature(ICipherParameters key)
        {
            ISigner signer = SignerUtilities.GetSigner(GetDigestAlgorithm());
            signer.Init(forSigning: true, key);
            return signer;
        }

        private ISigner InitSignature(AsymmetricKeyParameter key)
        {
            string algorithm = GetDigestAlgorithm();
            if (PdfName.ADBE_X509_RSA_SHA1.Equals(GetFilterSubtype()))
            {
                algorithm = "SHA1withRSA";
            }

            ISigner signer = SignerUtilities.GetSigner(algorithm);
            signer.Init(forSigning: false, signCert.GetPublicKey());
            return signer;
        }

        public virtual void Update(byte[] buf, int off, int len)
        {
            if (RSAdata != null || digestAttr != null || isTsp)
            {
                messageDigest.BlockUpdate(buf, off, len);
            }
            else
            {
                sig.BlockUpdate(buf, off, len);
            }
        }

        public virtual byte[] GetEncodedPKCS1()
        {
            if (externalDigest != null)
            {
                digest = externalDigest;
            }
            else
            {
                digest = sig.GenerateSignature();
            }

            MemoryStream memoryStream = new MemoryStream();
            Asn1OutputStream asn1OutputStream = new Asn1OutputStream(memoryStream);
            asn1OutputStream.WriteObject(new DerOctetString(digest));
            asn1OutputStream.Close();
            return memoryStream.ToArray();
        }

        public virtual byte[] GetEncodedPKCS7()
        {
            return GetEncodedPKCS7(null, DateTime.Now, null, null, null, CryptoStandard.CMS);
        }

        public virtual byte[] GetEncodedPKCS7(byte[] secondDigest, DateTime signingTime)
        {
            return GetEncodedPKCS7(secondDigest, signingTime, null, null, null, CryptoStandard.CMS);
        }

        public virtual byte[] GetEncodedPKCS7(byte[] secondDigest, DateTime signingTime, ITSAClient tsaClient, byte[] ocsp, ICollection<byte[]> crlBytes, CryptoStandard sigtype)
        {
            if (externalDigest != null)
            {
                digest = externalDigest;
                if (RSAdata != null)
                {
                    RSAdata = externalRSAdata;
                }
            }
            else if (externalRSAdata != null && RSAdata != null)
            {
                RSAdata = externalRSAdata;
                sig.BlockUpdate(RSAdata, 0, RSAdata.Length);
                digest = sig.GenerateSignature();
            }
            else
            {
                if (RSAdata != null)
                {
                    RSAdata = new byte[messageDigest.GetDigestSize()];
                    messageDigest.DoFinal(RSAdata, 0);
                    sig.BlockUpdate(RSAdata, 0, RSAdata.Length);
                }

                digest = sig.GenerateSignature();
            }

            Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
            foreach (string key in digestalgos.Keys)
            {
                Asn1EncodableVector asn1EncodableVector2 = new Asn1EncodableVector();
                asn1EncodableVector2.Add(new DerObjectIdentifier(key));
                asn1EncodableVector2.Add(DerNull.Instance);
                asn1EncodableVector.Add(new DerSequence(asn1EncodableVector2));
            }

            Asn1EncodableVector asn1EncodableVector3 = new Asn1EncodableVector();
            asn1EncodableVector3.Add(new DerObjectIdentifier("1.2.840.113549.1.7.1"));
            if (RSAdata != null)
            {
                asn1EncodableVector3.Add(new DerTaggedObject(0, new DerOctetString(RSAdata)));
            }

            DerSequence derSequence = new DerSequence(asn1EncodableVector3);
            asn1EncodableVector3 = new Asn1EncodableVector();
            foreach (X509Certificate cert in certs)
            {
                Asn1InputStream asn1InputStream = new Asn1InputStream(new MemoryStream(cert.GetEncoded()));
                asn1EncodableVector3.Add(asn1InputStream.ReadObject());
            }

            DerSet obj = new DerSet(asn1EncodableVector3);
            Asn1EncodableVector asn1EncodableVector4 = new Asn1EncodableVector();
            asn1EncodableVector4.Add(new DerInteger(signerversion));
            asn1EncodableVector3 = new Asn1EncodableVector();
            asn1EncodableVector3.Add(CertificateInfo.GetIssuer(signCert.GetTbsCertificate()));
            asn1EncodableVector3.Add(new DerInteger(signCert.SerialNumber));
            asn1EncodableVector4.Add(new DerSequence(asn1EncodableVector3));
            asn1EncodableVector3 = new Asn1EncodableVector();
            asn1EncodableVector3.Add(new DerObjectIdentifier(digestAlgorithmOid));
            asn1EncodableVector3.Add(DerNull.Instance);
            asn1EncodableVector4.Add(new DerSequence(asn1EncodableVector3));
            if (secondDigest != null)
            {
                asn1EncodableVector4.Add(new DerTaggedObject(explicitly: false, 0, GetAuthenticatedAttributeSet(secondDigest, signingTime, ocsp, crlBytes, sigtype)));
            }

            asn1EncodableVector3 = new Asn1EncodableVector();
            asn1EncodableVector3.Add(new DerObjectIdentifier(digestEncryptionAlgorithmOid));
            asn1EncodableVector3.Add(DerNull.Instance);
            asn1EncodableVector4.Add(new DerSequence(asn1EncodableVector3));
            asn1EncodableVector4.Add(new DerOctetString(digest));
            if (tsaClient != null)
            {
                byte[] imprint = DigestAlgorithms.Digest(tsaClient.GetMessageDigest(), digest);
                byte[] array = tsaClient.GetTimeStampToken(imprint);
                if (array != null)
                {
                    Asn1EncodableVector asn1EncodableVector5 = BuildUnauthenticatedAttributes(array);
                    if (asn1EncodableVector5 != null)
                    {
                        asn1EncodableVector4.Add(new DerTaggedObject(explicitly: false, 1, new DerSet(asn1EncodableVector5)));
                    }
                }
            }

            Asn1EncodableVector asn1EncodableVector6 = new Asn1EncodableVector();
            asn1EncodableVector6.Add(new DerInteger(version));
            asn1EncodableVector6.Add(new DerSet(asn1EncodableVector));
            asn1EncodableVector6.Add(derSequence);
            asn1EncodableVector6.Add(new DerTaggedObject(explicitly: false, 0, obj));
            asn1EncodableVector6.Add(new DerSet(new DerSequence(asn1EncodableVector4)));
            Asn1EncodableVector asn1EncodableVector7 = new Asn1EncodableVector();
            asn1EncodableVector7.Add(new DerObjectIdentifier("1.2.840.113549.1.7.2"));
            asn1EncodableVector7.Add(new DerTaggedObject(0, new DerSequence(asn1EncodableVector6)));
            MemoryStream memoryStream = new MemoryStream();
            Asn1OutputStream asn1OutputStream = new Asn1OutputStream(memoryStream);
            asn1OutputStream.WriteObject(new DerSequence(asn1EncodableVector7));
            asn1OutputStream.Close();
            return memoryStream.ToArray();
        }

        private Asn1EncodableVector BuildUnauthenticatedAttributes(byte[] timeStampToken)
        {
            if (timeStampToken == null)
            {
                return null;
            }

            string identifier = "1.2.840.113549.1.9.16.2.14";
            Asn1InputStream asn1InputStream = new Asn1InputStream(new MemoryStream(timeStampToken));
            Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
            Asn1EncodableVector asn1EncodableVector2 = new Asn1EncodableVector();
            asn1EncodableVector2.Add(new DerObjectIdentifier(identifier));
            Asn1Sequence obj = (Asn1Sequence)asn1InputStream.ReadObject();
            asn1EncodableVector2.Add(new DerSet(obj));
            asn1EncodableVector.Add(new DerSequence(asn1EncodableVector2));
            return asn1EncodableVector;
        }

        public virtual byte[] getAuthenticatedAttributeBytes(byte[] secondDigest, DateTime signingTime, byte[] ocsp, ICollection<byte[]> crlBytes, CryptoStandard sigtype)
        {
            return GetAuthenticatedAttributeSet(secondDigest, signingTime, ocsp, crlBytes, sigtype).GetEncoded("DER");
        }

        private DerSet GetAuthenticatedAttributeSet(byte[] secondDigest, DateTime signingTime, byte[] ocsp, ICollection<byte[]> crlBytes, CryptoStandard sigtype)
        {
            Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
            Asn1EncodableVector asn1EncodableVector2 = new Asn1EncodableVector();
            asn1EncodableVector2.Add(new DerObjectIdentifier("1.2.840.113549.1.9.3"));
            asn1EncodableVector2.Add(new DerSet(new DerObjectIdentifier("1.2.840.113549.1.7.1")));
            asn1EncodableVector.Add(new DerSequence(asn1EncodableVector2));
            asn1EncodableVector2 = new Asn1EncodableVector();
            asn1EncodableVector2.Add(new DerObjectIdentifier("1.2.840.113549.1.9.5"));
            asn1EncodableVector2.Add(new DerSet(new DerUtcTime(signingTime)));
            asn1EncodableVector.Add(new DerSequence(asn1EncodableVector2));
            asn1EncodableVector2 = new Asn1EncodableVector();
            asn1EncodableVector2.Add(new DerObjectIdentifier("1.2.840.113549.1.9.4"));
            asn1EncodableVector2.Add(new DerSet(new DerOctetString(secondDigest)));
            asn1EncodableVector.Add(new DerSequence(asn1EncodableVector2));
            bool flag = false;
            if (crlBytes != null)
            {
                foreach (byte[] crlByte in crlBytes)
                {
                    if (crlByte != null)
                    {
                        flag = true;
                        break;
                    }
                }
            }

            if (ocsp != null || flag)
            {
                asn1EncodableVector2 = new Asn1EncodableVector();
                asn1EncodableVector2.Add(new DerObjectIdentifier("1.2.840.113583.1.1.8"));
                Asn1EncodableVector asn1EncodableVector3 = new Asn1EncodableVector();
                if (flag)
                {
                    Asn1EncodableVector asn1EncodableVector4 = new Asn1EncodableVector();
                    foreach (byte[] crlByte2 in crlBytes)
                    {
                        if (crlByte2 != null)
                        {
                            Asn1InputStream asn1InputStream = new Asn1InputStream(crlByte2);
                            asn1EncodableVector4.Add(asn1InputStream.ReadObject());
                        }
                    }

                    asn1EncodableVector3.Add(new DerTaggedObject(explicitly: true, 0, new DerSequence(asn1EncodableVector4)));
                }

                if (ocsp != null)
                {
                    DerOctetString derOctetString = new DerOctetString(ocsp);
                    Asn1EncodableVector asn1EncodableVector5 = new Asn1EncodableVector();
                    Asn1EncodableVector asn1EncodableVector6 = new Asn1EncodableVector();
                    asn1EncodableVector6.Add(OcspObjectIdentifiers.PkixOcspBasic);
                    asn1EncodableVector6.Add(derOctetString);
                    DerEnumerated derEnumerated = new DerEnumerated(0);
                    Asn1EncodableVector asn1EncodableVector7 = new Asn1EncodableVector();
                    asn1EncodableVector7.Add(derEnumerated);
                    asn1EncodableVector7.Add(new DerTaggedObject(explicitly: true, 0, new DerSequence(asn1EncodableVector6)));
                    asn1EncodableVector5.Add(new DerSequence(asn1EncodableVector7));
                    asn1EncodableVector3.Add(new DerTaggedObject(explicitly: true, 1, new DerSequence(asn1EncodableVector5)));
                }

                asn1EncodableVector2.Add(new DerSet(new DerSequence(asn1EncodableVector3)));
                asn1EncodableVector.Add(new DerSequence(asn1EncodableVector2));
            }

            if (sigtype == CryptoStandard.CADES)
            {
                asn1EncodableVector2 = new Asn1EncodableVector();
                asn1EncodableVector2.Add(new DerObjectIdentifier("1.2.840.113549.1.9.16.2.47"));
                Asn1EncodableVector asn1EncodableVector8 = new Asn1EncodableVector();
                AlgorithmIdentifier algorithmIdentifier = new AlgorithmIdentifier(new DerObjectIdentifier(digestAlgorithmOid), null);
                asn1EncodableVector8.Add(algorithmIdentifier);
                byte[] str = DigestAlgorithms.Digest(GetHashAlgorithm(), signCert.GetEncoded());
                asn1EncodableVector8.Add(new DerOctetString(str));
                asn1EncodableVector2.Add(new DerSet(new DerSequence(new DerSequence(new DerSequence(asn1EncodableVector8)))));
                asn1EncodableVector.Add(new DerSequence(asn1EncodableVector2));
            }

            return new DerSet(asn1EncodableVector);
        }

        public virtual bool Verify()
        {
            if (verified)
            {
                return verifyResult;
            }

            if (isTsp)
            {
                MessageImprint messageImprint = timeStampToken.TimeStampInfo.TstInfo.MessageImprint;
                byte[] array = new byte[messageDigest.GetDigestSize()];
                messageDigest.DoFinal(array, 0);
                byte[] hashedMessage = messageImprint.GetHashedMessage();
                verifyResult = Arrays.AreEqual(array, hashedMessage);
            }
            else if (sigAttr != null || sigAttrDer != null)
            {
                byte[] array2 = new byte[messageDigest.GetDigestSize()];
                messageDigest.DoFinal(array2, 0);
                bool flag = true;
                bool flag2 = false;
                if (RSAdata != null)
                {
                    flag = Arrays.AreEqual(array2, RSAdata);
                    encContDigest.BlockUpdate(RSAdata, 0, RSAdata.Length);
                    byte[] array3 = new byte[encContDigest.GetDigestSize()];
                    encContDigest.DoFinal(array3, 0);
                    flag2 = Arrays.AreEqual(array3, digestAttr);
                }

                bool flag3 = Arrays.AreEqual(array2, digestAttr) || flag2;
                bool flag4 = VerifySigAttributes(sigAttr) || VerifySigAttributes(sigAttrDer);
                verifyResult = flag3 && flag4 && flag;
            }
            else
            {
                if (RSAdata != null)
                {
                    byte[] array4 = new byte[messageDigest.GetDigestSize()];
                    messageDigest.DoFinal(array4, 0);
                    sig.BlockUpdate(array4, 0, array4.Length);
                }

                verifyResult = sig.VerifySignature(digest);
            }

            verified = true;
            return verifyResult;
        }

        private bool VerifySigAttributes(byte[] attr)
        {
            ISigner signer = InitSignature(signCert.GetPublicKey());
            signer.BlockUpdate(attr, 0, attr.Length);
            return signer.VerifySignature(digest);
        }

        public virtual bool VerifyTimestampImprint()
        {
            if (timeStampToken == null)
            {
                return false;
            }

            TimeStampTokenInfo timeStampInfo = timeStampToken.TimeStampInfo;
            MessageImprint messageImprint = timeStampInfo.TstInfo.MessageImprint;
            byte[] a = DigestAlgorithms.Digest(timeStampInfo.MessageImprintAlgOid, digest);
            byte[] hashedMessage = messageImprint.GetHashedMessage();
            return Arrays.AreEqual(a, hashedMessage);
        }

        private void CalcSignCertificateChain()
        {
            List<X509Certificate> list = new List<X509Certificate>();
            list.Add(signCert);
            List<X509Certificate> list2 = new List<X509Certificate>(certs);
            for (int i = 0; i < list2.Count; i++)
            {
                if (signCert.Equals(list2[i]))
                {
                    list2.RemoveAt(i);
                    i--;
                }
            }

            bool flag = true;
            while (flag)
            {
                X509Certificate x509Certificate = list[list.Count - 1];
                flag = false;
                for (int j = 0; j < list2.Count; j++)
                {
                    X509Certificate x509Certificate2 = list2[j];
                    try
                    {
                        x509Certificate.Verify(x509Certificate2.GetPublicKey());
                        flag = true;
                        list.Add(x509Certificate2);
                        list2.RemoveAt(j);
                    }
                    catch
                    {
                        continue;
                    }

                    break;
                }
            }

            signCerts = list;
        }

        private void FindCRL(Asn1Sequence seq)
        {
            crls = new List<X509Crl>();
            for (int i = 0; i < seq.Count; i++)
            {
                X509Crl item = new X509CrlParser().ReadCrl(seq[i].GetDerEncoded());
                crls.Add(item);
            }
        }

        public virtual bool IsRevocationValid()
        {
            if (basicResp == null)
            {
                return false;
            }

            if (signCerts.Count < 2)
            {
                return false;
            }

            try
            {
                X509Certificate[] signCertificateChain = SignCertificateChain;
                CertificateID certID = basicResp.Responses[0].GetCertID();
                X509Certificate signingCertificate = SigningCertificate;
                X509Certificate issuerCert = signCertificateChain[1];
                return new CertificateID(certID.HashAlgOid, issuerCert, signingCertificate.SerialNumber).Equals(certID);
            }
            catch
            {
            }

            return false;
        }

        private void FindOcsp(Asn1Sequence seq)
        {
            basicResp = null;
            bool flag = false;
            while (!(seq[0] is DerObjectIdentifier) || !((DerObjectIdentifier)seq[0]).Id.Equals(OcspObjectIdentifiers.PkixOcspBasic.Id))
            {
                flag = true;
                for (int i = 0; i < seq.Count; i++)
                {
                    if (seq[i] is Asn1Sequence)
                    {
                        seq = (Asn1Sequence)seq[0];
                        flag = false;
                        break;
                    }

                    if (seq[i] is Asn1TaggedObject)
                    {
                        Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)seq[i];
                        if (asn1TaggedObject.GetObject() is Asn1Sequence)
                        {
                            seq = (Asn1Sequence)asn1TaggedObject.GetObject();
                            flag = false;
                            break;
                        }

                        return;
                    }
                }

                if (flag)
                {
                    return;
                }
            }

            BasicOcspResponse instance = BasicOcspResponse.GetInstance(new Asn1InputStream(((Asn1OctetString)seq[1]).GetOctets()).ReadObject());
            basicResp = new BasicOcspResp(instance);
        }

        public virtual PdfName GetFilterSubtype()
        {
            return filterSubtype;
        }

        public virtual string GetEncryptionAlgorithm()
        {
            string algorithm = EncryptionAlgorithms.GetAlgorithm(digestEncryptionAlgorithmOid);
            if (algorithm == null)
            {
                algorithm = digestEncryptionAlgorithmOid;
            }

            return algorithm;
        }
    }
}
