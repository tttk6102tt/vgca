using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.X509;
using System.Text;

namespace Sign.itext.pdf.security
{
    public static class CertificateUtil
    {
        public static string GetCRLURL(X509Certificate certificate)
        {
            try
            {
                Asn1Object extensionValue = GetExtensionValue(certificate, X509Extensions.CrlDistributionPoints.Id);
                if (extensionValue == null)
                {
                    return null;
                }

                DistributionPoint[] distributionPoints = CrlDistPoint.GetInstance(extensionValue).GetDistributionPoints();
                for (int i = 0; i < distributionPoints.Length; i++)
                {
                    DistributionPointName distributionPointName = distributionPoints[i].DistributionPointName;
                    if (distributionPointName.PointType != 0)
                    {
                        continue;
                    }

                    GeneralName[] names = ((GeneralNames)distributionPointName.Name).GetNames();
                    foreach (GeneralName generalName in names)
                    {
                        if (generalName.TagNo == 6)
                        {
                            return DerIA5String.GetInstance((Asn1TaggedObject)generalName.ToAsn1Object(), isExplicit: false).GetString();
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        public static string GetOCSPURL(X509Certificate certificate)
        {
            try
            {
                Asn1Object extensionValue = GetExtensionValue(certificate, X509Extensions.AuthorityInfoAccess.Id);
                if (extensionValue == null)
                {
                    return null;
                }

                Asn1Sequence asn1Sequence = (Asn1Sequence)extensionValue;
                for (int i = 0; i < asn1Sequence.Count; i++)
                {
                    Asn1Sequence asn1Sequence2 = (Asn1Sequence)asn1Sequence[i];
                    if (asn1Sequence2.Count == 2 && asn1Sequence2[0] is DerObjectIdentifier && ((DerObjectIdentifier)asn1Sequence2[0]).Id.Equals("1.3.6.1.5.5.7.48.1"))
                    {
                        string stringFromGeneralName = GetStringFromGeneralName((Asn1Object)asn1Sequence2[1]);
                        if (stringFromGeneralName == null)
                        {
                            return "";
                        }

                        return stringFromGeneralName;
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        public static string GetTSAURL(X509Certificate certificate)
        {
            Asn1OctetString extensionValue = certificate.GetExtensionValue("1.2.840.113583.1.1.9.1");
            if (extensionValue == null)
            {
                return null;
            }

            byte[] octets = extensionValue.GetOctets();
            if (octets == null)
            {
                return null;
            }

            try
            {
                Asn1Object asn1Object = Asn1Object.FromByteArray(octets);
                if (asn1Object is DerOctetString)
                {
                    asn1Object = Asn1Object.FromByteArray(((DerOctetString)asn1Object).GetOctets());
                }

                return GetStringFromGeneralName(Asn1Sequence.GetInstance(asn1Object)[1].ToAsn1Object());
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static Asn1Object GetExtensionValue(X509Certificate cert, string oid)
        {
            byte[] derEncoded = cert.GetExtensionValue(new DerObjectIdentifier(oid)).GetDerEncoded();
            if (derEncoded == null)
            {
                return null;
            }

            return new Asn1InputStream(new MemoryStream(((Asn1OctetString)new Asn1InputStream(new MemoryStream(derEncoded)).ReadObject()).GetOctets())).ReadObject();
        }

        private static string GetStringFromGeneralName(Asn1Object names)
        {
            Asn1TaggedObject obj = (Asn1TaggedObject)names;
            return Encoding.GetEncoding(1252).GetString(Asn1OctetString.GetInstance(obj, isExplicit: false).GetOctets());
        }
    }
}
