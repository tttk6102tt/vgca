using Sign.itext;
using Sign.itext.io;
using Sign.itext.pdf;
using Sign.itext.pdf.security;
using Sign.itext.text.log;
using Sign.itext.text.pdf;
using Sign.itext.text.pdf.security;
using Sign.Org.BouncyCastle.Security;
using Sign.Org.BouncyCastle.X509;

namespace Sign.PDF
{
    public static class MakeSignature
    {
        private static readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(MakeSignature));

        public static void SignDetached(PdfSignatureAppearance sap, IExternalSignature externalSignature, ICollection<X509Certificate> chain, ICollection<ICrlClient> crlList, IOcspClient ocspClient, ITSAClient tsaClient, int estimatedSize, CryptoStandard sigtype)
        {
            List<X509Certificate> list = new List<X509Certificate>(chain);
            ICollection<byte[]> collection = null;
            int num = 0;
            while (collection == null && num < list.Count)
            {
                collection = ProcessCrl(list[num++], crlList);
            }

            if (estimatedSize == 0)
            {
                estimatedSize = 8192;
                if (collection != null)
                {
                    foreach (byte[] item in collection)
                    {
                        estimatedSize += item.Length + 10;
                    }
                }

                if (ocspClient != null)
                {
                    estimatedSize += 4192;
                }

                if (tsaClient != null)
                {
                    estimatedSize += 4192;
                }
            }

            sap.Certificate = list[0];
            if (sigtype == CryptoStandard.CADES)
            {
                sap.AddDeveloperExtension(PdfDeveloperExtension.ESIC_1_7_EXTENSIONLEVEL2);
            }

            PdfSignature pdfSignature = new PdfSignature(PdfName.ADOBE_PPKLITE, (sigtype == CryptoStandard.CADES) ? PdfName.ETSI_CADES_DETACHED : PdfName.ADBE_PKCS7_DETACHED);
            pdfSignature.Reason = sap.Reason;
            pdfSignature.Location = sap.Location;
            pdfSignature.SignatureCreator = sap.SignatureCreator;
            pdfSignature.Contact = sap.Contact;
            pdfSignature.Date = new PdfDate(sap.SignDate);
            sap.CryptoDictionary = pdfSignature;
            Dictionary<PdfName, int> dictionary = new Dictionary<PdfName, int>();
            dictionary[PdfName.CONTENTS] = estimatedSize * 2 + 2;
            sap.PreClose(dictionary);
            string hashAlgorithm = externalSignature.GetHashAlgorithm();
            PdfPKCS7 pdfPKCS = new PdfPKCS7(null, chain, hashAlgorithm, hasRSAdata: false);
            DigestUtilities.GetDigest(hashAlgorithm);
            byte[] secondDigest = DigestAlgorithms.Digest(sap.GetRangeStream(), hashAlgorithm);
            DateTime now = DateTime.Now;
            byte[] ocsp = null;
            if (chain.Count >= 2 && ocspClient != null)
            {
                ocsp = ocspClient.GetEncoded(list[0], list[1], null);
            }

            byte[] authenticatedAttributeBytes = pdfPKCS.getAuthenticatedAttributeBytes(secondDigest, now, ocsp, collection, sigtype);
            byte[] digest = externalSignature.Sign(authenticatedAttributeBytes);
            pdfPKCS.SetExternalDigest(digest, null, externalSignature.GetEncryptionAlgorithm());
            byte[] encodedPKCS = pdfPKCS.GetEncodedPKCS7(secondDigest, now, tsaClient, ocsp, collection, sigtype);
            if (estimatedSize < encodedPKCS.Length)
            {
                throw new IOException("Không đủ không gian lưu chữ ký. 0x002");
            }

            byte[] array = new byte[estimatedSize];
            Array.Copy(encodedPKCS, 0, array, 0, encodedPKCS.Length);
            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.CONTENTS, new PdfString(array).SetHexWriting(hexWriting: true));
            sap.Close(pdfDictionary);
        }

        public static ICollection<byte[]> ProcessCrl(X509Certificate cert, ICollection<ICrlClient> crlList)
        {
            if (crlList == null)
            {
                return null;
            }

            List<byte[]> list = new List<byte[]>();
            foreach (ICrlClient crl in crlList)
            {
                if (crl != null)
                {
                    ICollection<byte[]> encoded = crl.GetEncoded(cert, null);
                    if (encoded != null)
                    {
                        LOGGER.Info("Processing " + crl.GetType().Name);
                        list.AddRange(encoded);
                    }
                }
            }

            if (list.Count == 0)
            {
                return null;
            }

            return list;
        }

        public static void SignExternalContainer(PdfSignatureAppearance sap, IExternalSignatureContainer externalSignatureContainer, int estimatedSize)
        {
            PdfSignature pdfSignature = new PdfSignature(null, null);
            pdfSignature.Reason = sap.Reason;
            pdfSignature.Location = sap.Location;
            pdfSignature.SignatureCreator = sap.SignatureCreator;
            pdfSignature.Contact = sap.Contact;
            pdfSignature.Date = new PdfDate(sap.SignDate);
            externalSignatureContainer.ModifySigningDictionary(pdfSignature);
            sap.CryptoDictionary = pdfSignature;
            Dictionary<PdfName, int> dictionary = new Dictionary<PdfName, int>();
            dictionary[PdfName.CONTENTS] = estimatedSize * 2 + 2;
            sap.PreClose(dictionary);
            Stream rangeStream = sap.GetRangeStream();
            byte[] array = externalSignatureContainer.Sign(rangeStream);
            if (estimatedSize < array.Length)
            {
                throw new IOException("Not enough space");
            }

            byte[] array2 = new byte[estimatedSize];
            Array.Copy(array, 0, array2, 0, array.Length);
            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.CONTENTS, new PdfString(array2).SetHexWriting(hexWriting: true));
            sap.Close(pdfDictionary);
        }

        public static void SignDeferred(PdfReader reader, string fieldName, Stream outs, IExternalSignatureContainer externalSignatureContainer)
        {
            AcroFields acroFields = reader.AcroFields;
            PdfDictionary signatureDictionary = acroFields.GetSignatureDictionary(fieldName);
            if (signatureDictionary == null)
            {
                throw new DocumentException("No field");
            }

            if (!acroFields.SignatureCoversWholeDocument(fieldName))
            {
                throw new DocumentException("Not the last signature");
            }

            PdfArray asArray = signatureDictionary.GetAsArray(PdfName.BYTERANGE);
            long[] array = asArray.AsLongArray();
            if (asArray.Size != 4 || array[0] != 0L)
            {
                throw new DocumentException("Single exclusion space supported");
            }

            IRandomAccessSource source = reader.SafeFile.CreateSourceView();
            Stream data = new RASInputStream(new RandomAccessSourceFactory().CreateRanged(source, array));
            byte[] array2 = externalSignatureContainer.Sign(data);
            int num = (int)(array[2] - array[1]) - 2;
            if (((uint)num & (true ? 1u : 0u)) != 0)
            {
                throw new DocumentException("Gap is not a multiple of 2");
            }

            num /= 2;
            if (num < array2.Length)
            {
                throw new DocumentException("Not enough space");
            }

            StreamUtil.CopyBytes(source, 0L, array[1] + 1, outs);
            ByteBuffer byteBuffer = new ByteBuffer(num * 2);
            byte[] array3 = array2;
            foreach (byte b in array3)
            {
                byteBuffer.AppendHex(b);
            }

            int num2 = (num - array2.Length) * 2;
            for (int j = 0; j < num2; j++)
            {
                byteBuffer.Append((byte)48);
            }

            byteBuffer.WriteTo(outs);
            StreamUtil.CopyBytes(source, array[2] - 1, array[3] + 1, outs);
        }
    }

}
