using Sign.itext.pdf;
using Sign.itext.pdf.crypto;
using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.Cms;
using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Security;
using Sign.Org.BouncyCastle.X509;

namespace Sign.itext.text.pdf
{
    public class PdfPublicKeySecurityHandler
    {
        private const int SEED_LENGTH = 20;

        private List<PdfPublicKeyRecipient> recipients;

        private byte[] seed;

        public PdfPublicKeySecurityHandler()
        {
            seed = IVGenerator.GetIV(20);
            recipients = new List<PdfPublicKeyRecipient>();
        }

        public virtual void AddRecipient(PdfPublicKeyRecipient recipient)
        {
            recipients.Add(recipient);
        }

        protected internal virtual byte[] GetSeed()
        {
            return (byte[])seed.Clone();
        }

        public virtual int GetRecipientsSize()
        {
            return recipients.Count;
        }

        public virtual byte[] GetEncodedRecipient(int index)
        {
            PdfPublicKeyRecipient pdfPublicKeyRecipient = recipients[index];
            byte[] cms = pdfPublicKeyRecipient.Cms;
            if (cms != null)
            {
                return cms;
            }

            X509Certificate certificate = pdfPublicKeyRecipient.Certificate;
            int permission = pdfPublicKeyRecipient.Permission;
            int num = 3;
            int num2 = ((permission | ((num == 3) ? (-3904) : (-64))) & -4) + 1;
            byte[] array = new byte[24];
            byte b = (byte)num2;
            byte b2 = (byte)(num2 >> 8);
            byte b3 = (byte)(num2 >> 16);
            byte b4 = (byte)(num2 >> 24);
            Array.Copy(seed, 0, array, 0, 20);
            array[20] = b4;
            array[21] = b3;
            array[22] = b2;
            array[23] = b;
            Asn1Object obj = CreateDERForRecipient(array, certificate);
            MemoryStream memoryStream = new MemoryStream();
            new DerOutputStream(memoryStream).WriteObject(obj);
            return pdfPublicKeyRecipient.Cms = memoryStream.ToArray();
        }

        public virtual PdfArray GetEncodedRecipients()
        {
            PdfArray pdfArray = new PdfArray();
            byte[] array = null;
            for (int i = 0; i < recipients.Count; i++)
            {
                try
                {
                    array = GetEncodedRecipient(i);
                    pdfArray.Add(new PdfLiteral(StringUtils.EscapeString(array)));
                }
                catch
                {
                    pdfArray = null;
                }
            }

            return pdfArray;
        }

        private Asn1Object CreateDERForRecipient(byte[] inp, X509Certificate cert)
        {
            byte[] array = new byte[100];
            DerObjectIdentifier derObjectIdentifier = new DerObjectIdentifier("1.2.840.113549.3.2");
            byte[] iV = IVGenerator.GetIV(16);
            IBufferedCipher cipher = CipherUtilities.GetCipher(derObjectIdentifier);
            KeyParameter parameters = new KeyParameter(iV);
            byte[] iV2 = IVGenerator.GetIV(cipher.GetBlockSize());
            ParametersWithIV parameters2 = new ParametersWithIV(parameters, iV2);
            cipher.Init(forEncryption: true, parameters2);
            int num = cipher.DoFinal(inp, array, 0);
            byte[] array2 = new byte[num];
            Array.Copy(array, 0, array2, 0, num);
            DerOctetString encryptedContent = new DerOctetString(array2);
            DerSet recipientInfos = new DerSet(new RecipientInfo(ComputeRecipientInfo(cert, iV)));
            Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
            asn1EncodableVector.Add(new DerInteger(58));
            asn1EncodableVector.Add(new DerOctetString(iV2));
            DerSequence parameters3 = new DerSequence(asn1EncodableVector);
            AlgorithmIdentifier contentEncryptionAlgorithm = new AlgorithmIdentifier(derObjectIdentifier, parameters3);
            EncryptedContentInfo encryptedContentInfo = new EncryptedContentInfo(PkcsObjectIdentifiers.Data, contentEncryptionAlgorithm, encryptedContent);
            Asn1Set unprotectedAttrs = null;
            EnvelopedData content = new EnvelopedData(null, recipientInfos, encryptedContentInfo, unprotectedAttrs);
            return new ContentInfo(PkcsObjectIdentifiers.EnvelopedData, content).ToAsn1Object();
        }

        private KeyTransRecipientInfo ComputeRecipientInfo(X509Certificate x509certificate, byte[] abyte0)
        {
            TbsCertificateStructure instance = TbsCertificateStructure.GetInstance(new Asn1InputStream(new MemoryStream(x509certificate.GetTbsCertificate())).ReadObject());
            AlgorithmIdentifier algorithmID = instance.SubjectPublicKeyInfo.AlgorithmID;
            IssuerAndSerialNumber id = new IssuerAndSerialNumber(instance.Issuer, instance.SerialNumber.Value);
            IBufferedCipher cipher = CipherUtilities.GetCipher(algorithmID.ObjectID);
            cipher.Init(forEncryption: true, x509certificate.GetPublicKey());
            byte[] array = new byte[10000];
            int num = cipher.DoFinal(abyte0, array, 0);
            byte[] array2 = new byte[num];
            Array.Copy(array, 0, array2, 0, num);
            return new KeyTransRecipientInfo(encryptedKey: new DerOctetString(array2), rid: new RecipientIdentifier(id), keyEncryptionAlgorithm: algorithmID);
        }
    }
}
