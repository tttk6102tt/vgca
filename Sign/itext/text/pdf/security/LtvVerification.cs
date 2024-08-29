using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.pdf.security;
using Sign.itext.text.log;
using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.Ocsp;
using Sign.Org.BouncyCastle.Utilities;
using Sign.Org.BouncyCastle.X509;
using Sign.X509;

namespace Sign.itext.text.pdf.security
{
    public class LtvVerification
    {
        public enum Level
        {
            OCSP,
            CRL,
            OCSP_CRL,
            OCSP_OPTIONAL_CRL
        }

        public enum CertificateOption
        {
            SIGNING_CERTIFICATE,
            WHOLE_CHAIN
        }

        public enum CertificateInclusion
        {
            YES,
            NO
        }

        private class ValidationData
        {
            public IList<byte[]> crls = new List<byte[]>();

            public IList<byte[]> ocsps = new List<byte[]>();

            public IList<byte[]> certs = new List<byte[]>();
        }

        private ILogger LOGGER = LoggerFactory.GetLogger(typeof(LtvVerification));

        private PdfStamper stp;

        private PdfWriter writer;

        private PdfReader reader;

        private AcroFields acroFields;

        private IDictionary<PdfName, ValidationData> validated = new Dictionary<PdfName, ValidationData>();

        private bool used;

        public LtvVerification(PdfStamper stp)
        {
            this.stp = stp;
            writer = stp.Writer;
            reader = stp.Reader;
            acroFields = stp.AcroFields;
        }

        public virtual bool AddVerification(string signatureName, IOcspClient ocsp, ICrlClient crl, CertificateOption certOption, Level level, CertificateInclusion certInclude)
        {
            if (used)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("verification.already.output"));
            }

            PdfPKCS7 pdfPKCS = acroFields.VerifySignature(signatureName);
            LOGGER.Info("Adding verification for " + signatureName);
            X509Certificate[] certificates = pdfPKCS.Certificates;
            X509Certificate signingCertificate = pdfPKCS.SigningCertificate;
            ValidationData validationData = new ValidationData();
            for (int i = 0; i < certificates.Length; i++)
            {
                X509Certificate x509Certificate = certificates[i];
                LOGGER.Info("Certificate: " + x509Certificate.SubjectDN);
                if (certOption == CertificateOption.SIGNING_CERTIFICATE && !x509Certificate.Equals(signingCertificate))
                {
                    continue;
                }

                byte[] array = null;
                if (ocsp != null && level != Level.CRL)
                {
                    array = ocsp.GetEncoded(x509Certificate, GetParent(x509Certificate, certificates), null);
                    if (array != null)
                    {
                        validationData.ocsps.Add(BuildOCSPResponse(array));
                        LOGGER.Info("OCSP added");
                    }
                }

                if (crl != null && (level == Level.CRL || level == Level.OCSP_CRL || (level == Level.OCSP_OPTIONAL_CRL && array == null)))
                {
                    ICollection<byte[]> encoded = crl.GetEncoded(certificates[i], null);
                    if (encoded != null)
                    {
                        foreach (byte[] item in encoded)
                        {
                            bool flag = false;
                            foreach (byte[] crl2 in validationData.crls)
                            {
                                if (Arrays.AreEqual(crl2, item))
                                {
                                    flag = true;
                                    break;
                                }
                            }

                            if (!flag)
                            {
                                validationData.crls.Add(item);
                                LOGGER.Info("CRL added");
                            }
                        }
                    }
                }

                if (certInclude == CertificateInclusion.YES)
                {
                    validationData.certs.Add(certificates[i].GetEncoded());
                }
            }

            if (validationData.crls.Count == 0 && validationData.ocsps.Count == 0)
            {
                return false;
            }

            validated[GetSignatureHashKey(signatureName)] = validationData;
            return true;
        }

        private X509Certificate GetParent(X509Certificate cert, X509Certificate[] certs)
        {
            foreach (X509Certificate x509Certificate in certs)
            {
                if (cert.IssuerDN.Equals(x509Certificate.SubjectDN))
                {
                    try
                    {
                        cert.Verify(x509Certificate.GetPublicKey());
                        return x509Certificate;
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }

        public virtual bool AddVerification(string signatureName, ICollection<byte[]> ocsps, ICollection<byte[]> crls, ICollection<byte[]> certs)
        {
            if (used)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("verification.already.output"));
            }

            ValidationData validationData = new ValidationData();
            if (ocsps != null)
            {
                foreach (byte[] ocsp in ocsps)
                {
                    validationData.ocsps.Add(BuildOCSPResponse(ocsp));
                }
            }

            if (crls != null)
            {
                foreach (byte[] crl in crls)
                {
                    validationData.crls.Add(crl);
                }
            }

            if (certs != null)
            {
                foreach (byte[] cert in certs)
                {
                    validationData.certs.Add(cert);
                }
            }

            validated[GetSignatureHashKey(signatureName)] = validationData;
            return true;
        }

        private static byte[] BuildOCSPResponse(byte[] BasicOCSPResponse)
        {
            DerOctetString derOctetString = new DerOctetString(BasicOCSPResponse);
            Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
            asn1EncodableVector.Add(OcspObjectIdentifiers.PkixOcspBasic);
            asn1EncodableVector.Add(derOctetString);
            DerEnumerated derEnumerated = new DerEnumerated(0);
            Asn1EncodableVector asn1EncodableVector2 = new Asn1EncodableVector();
            asn1EncodableVector2.Add(derEnumerated);
            asn1EncodableVector2.Add(new DerTaggedObject(explicitly: true, 0, new DerSequence(asn1EncodableVector)));
            return new DerSequence(asn1EncodableVector2).GetEncoded();
        }

        private PdfName GetSignatureHashKey(string signatureName)
        {
            PdfDictionary signatureDictionary = acroFields.GetSignatureDictionary(signatureName);
            byte[] array = signatureDictionary.GetAsString(PdfName.CONTENTS).GetOriginalBytes();
            if (PdfName.ETSI_RFC3161.Equals(signatureDictionary.GetAsName(PdfName.SUBFILTER)))
            {
                array = new Asn1InputStream(new MemoryStream(array)).ReadObject().GetEncoded();
            }

            return new PdfName(Utilities.ConvertToHex(HashBytesSha1(array)));
        }

        private static byte[] HashBytesSha1(byte[] b)
        {
            return DigestAlgorithms.Digest("SHA1", b);
        }

        public virtual void Merge()
        {
            if (!used && validated.Count != 0)
            {
                used = true;
                if (reader.Catalog.Get(PdfName.DSS) == null)
                {
                    CreateDss();
                }
                else
                {
                    UpdateDss();
                }
            }
        }

        private void UpdateDss()
        {
            PdfDictionary catalog = reader.Catalog;
            stp.MarkUsed(catalog);
            PdfDictionary asDict = catalog.GetAsDict(PdfName.DSS);
            PdfArray pdfArray = asDict.GetAsArray(PdfName.OCSPS);
            PdfArray pdfArray2 = asDict.GetAsArray(PdfName.CRLS);
            PdfArray pdfArray3 = asDict.GetAsArray(PdfName.CERTS);
            asDict.Remove(PdfName.OCSPS);
            asDict.Remove(PdfName.CRLS);
            asDict.Remove(PdfName.CERTS);
            PdfDictionary asDict2 = asDict.GetAsDict(PdfName.VRI);
            if (asDict2 != null)
            {
                foreach (PdfName key in asDict2.Keys)
                {
                    if (validated.ContainsKey(key))
                    {
                        PdfDictionary asDict3 = asDict2.GetAsDict(key);
                        if (asDict3 != null)
                        {
                            DeleteOldReferences(pdfArray, asDict3.GetAsArray(PdfName.OCSP));
                            DeleteOldReferences(pdfArray2, asDict3.GetAsArray(PdfName.CRL));
                            DeleteOldReferences(pdfArray3, asDict3.GetAsArray(PdfName.CERT));
                        }
                    }
                }
            }

            if (pdfArray == null)
            {
                pdfArray = new PdfArray();
            }

            if (pdfArray2 == null)
            {
                pdfArray2 = new PdfArray();
            }

            if (pdfArray3 == null)
            {
                pdfArray3 = new PdfArray();
            }

            OutputDss(asDict, asDict2, pdfArray, pdfArray2, pdfArray3);
        }

        private static void DeleteOldReferences(PdfArray all, PdfArray toDelete)
        {
            if (all == null || toDelete == null)
            {
                return;
            }

            foreach (PdfObject item in toDelete)
            {
                if (!item.IsIndirect())
                {
                    continue;
                }

                PRIndirectReference pRIndirectReference = (PRIndirectReference)item;
                for (int i = 0; i < all.Size; i++)
                {
                    PdfObject pdfObject = all[i];
                    if (pdfObject.IsIndirect())
                    {
                        PRIndirectReference pRIndirectReference2 = (PRIndirectReference)pdfObject;
                        if (pRIndirectReference.Number == pRIndirectReference2.Number)
                        {
                            all.Remove(i);
                            i--;
                        }
                    }
                }
            }
        }

        private void CreateDss()
        {
            OutputDss(new PdfDictionary(), new PdfDictionary(), new PdfArray(), new PdfArray(), new PdfArray());
        }

        private void OutputDss(PdfDictionary dss, PdfDictionary vrim, PdfArray ocsps, PdfArray crls, PdfArray certs)
        {
            writer.AddDeveloperExtension(PdfDeveloperExtension.ESIC_1_7_EXTENSIONLEVEL5);
            PdfDictionary catalog = reader.Catalog;
            stp.MarkUsed(catalog);
            foreach (PdfName key in validated.Keys)
            {
                PdfArray pdfArray = new PdfArray();
                PdfArray pdfArray2 = new PdfArray();
                PdfArray pdfArray3 = new PdfArray();
                PdfDictionary pdfDictionary = new PdfDictionary();
                foreach (byte[] crl in validated[key].crls)
                {
                    PdfStream pdfStream = new PdfStream(crl);
                    pdfStream.FlateCompress();
                    PdfIndirectReference indirectReference = writer.AddToBody(pdfStream, inObjStm: false).IndirectReference;
                    pdfArray2.Add(indirectReference);
                    crls.Add(indirectReference);
                }

                foreach (byte[] ocsp in validated[key].ocsps)
                {
                    PdfStream pdfStream2 = new PdfStream(ocsp);
                    pdfStream2.FlateCompress();
                    PdfIndirectReference indirectReference2 = writer.AddToBody(pdfStream2, inObjStm: false).IndirectReference;
                    pdfArray.Add(indirectReference2);
                    ocsps.Add(indirectReference2);
                }

                foreach (byte[] cert in validated[key].certs)
                {
                    PdfStream pdfStream3 = new PdfStream(cert);
                    pdfStream3.FlateCompress();
                    PdfIndirectReference indirectReference3 = writer.AddToBody(pdfStream3, inObjStm: false).IndirectReference;
                    pdfArray3.Add(indirectReference3);
                    certs.Add(indirectReference3);
                }

                if (pdfArray.Size > 0)
                {
                    pdfDictionary.Put(PdfName.OCSP, writer.AddToBody(pdfArray, inObjStm: false).IndirectReference);
                }

                if (pdfArray2.Size > 0)
                {
                    pdfDictionary.Put(PdfName.CRL, writer.AddToBody(pdfArray2, inObjStm: false).IndirectReference);
                }

                if (pdfArray3.Size > 0)
                {
                    pdfDictionary.Put(PdfName.CERT, writer.AddToBody(pdfArray3, inObjStm: false).IndirectReference);
                }

                vrim.Put(key, writer.AddToBody(pdfDictionary, inObjStm: false).IndirectReference);
            }

            dss.Put(PdfName.VRI, writer.AddToBody(vrim, inObjStm: false).IndirectReference);
            if (ocsps.Size > 0)
            {
                dss.Put(PdfName.OCSPS, writer.AddToBody(ocsps, inObjStm: false).IndirectReference);
            }

            if (crls.Size > 0)
            {
                dss.Put(PdfName.CRLS, writer.AddToBody(crls, inObjStm: false).IndirectReference);
            }

            if (certs.Size > 0)
            {
                dss.Put(PdfName.CERTS, writer.AddToBody(certs, inObjStm: false).IndirectReference);
            }

            catalog.Put(PdfName.DSS, writer.AddToBody(dss, inObjStm: false).IndirectReference);
        }
    }
}
