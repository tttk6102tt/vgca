using Sign.Enums;
using Sign.itext.pdf.security;
using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.Ocsp;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Ocsp;
using Sign.Org.BouncyCastle.Utilities;
using Sign.Org.BouncyCastle.X509;
using System.Collections;
using System.Net;
namespace Sign.Classes
{
    internal class OCSPClient
    {
        private readonly long MaxClockSkew = 360000000L;

        public static readonly int BufferSize = 32768;

        private int port;

        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
            }
        }

        private byte[] SendRequest(string url, byte[] requestData, string contentType, string acceptType)
        {
            if (port != 0)
            {
                Uri uri = new Uri(url);
                url = url.Replace(uri.Host, string.Format("{0}:{1}", uri.Host, port));
            }
            Stream stream = null;
            HttpWebResponse httpWebResponse = null;
            Stream stream2 = null;
            try
            {
                HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(url);
                obj.ProtocolVersion = HttpVersion.Version10;
                obj.Method = "POST";
                obj.ContentType = contentType;
                obj.ContentLength = requestData.Length;
                obj.Accept = acceptType;
                stream = obj.GetRequestStream();
                stream.Write(requestData, 0, requestData.Length);
                stream.Close();
                httpWebResponse = (HttpWebResponse)obj.GetResponse();
                stream2 = httpWebResponse.GetResponseStream();
                byte[] result = ReadAllBytes(stream2);
                stream2.Close();
                return result;
            }
            catch (Exception innerException)
            {
                throw new Exception("Không thể gửi yêu cầu kiểm tra chứng thư số.", innerException);
            }
            finally
            {
                stream?.Close();
                stream2?.Close();
                httpWebResponse?.Close();
            }
        }

        private byte[] ReadAllBytes(Stream inputStream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int num = 0;
                byte[] array2 = new byte[BufferSize];
                while ((num = inputStream.Read(array2, 0, array2.Length)) > 0)
                {
                    memoryStream.Write(array2, 0, num);
                }
                return memoryStream.ToArray();
            }
        }

        public ENUM_CERTIFICATESTATUS CheckCertificateRevocation(X509Certificate certificate, X509Certificate issuerCertificate)
        {
            string oCSPURL = CertificateUtil.GetOCSPURL(certificate);
            if (string.IsNullOrEmpty(oCSPURL))
            {
                throw new Exception("Không tìm thấy thông tin OCSP trên chứng thư số.");
            }
            OcspReq ocspReq = GenerateOCSPRequest(issuerCertificate, certificate.SerialNumber);
            byte[] byte_ = SendRequest(oCSPURL, ocspReq.GetEncoded(), "application/ocsp-request", "application/ocsp-response");
            return ProcessOCSPResponse(certificate, issuerCertificate, byte_);
        }

        private ENUM_CERTIFICATESTATUS ProcessOCSPResponse(X509Certificate certificate, X509Certificate issuerCertificate, byte[] responseBytes)
        {
            OcspResp ocspResp = new OcspResp(responseBytes);
            ENUM_CERTIFICATESTATUS result = ENUM_CERTIFICATESTATUS.Unknown;
            if (ocspResp.Status == 0)
            {
                BasicOcspResp basicOcspResp = (BasicOcspResp)ocspResp.GetResponseObject();
                VerifyResponseSignature(basicOcspResp, issuerCertificate);
                if (basicOcspResp.Responses.Length == 1)
                {
                    SingleResp singleResp = basicOcspResp.Responses[0];
                    VerifyCertificateId(issuerCertificate, certificate, singleResp.GetCertID());
                    object certStatus = singleResp.GetCertStatus();
                    if (certStatus == CertificateStatus.Good)
                    {
                        result = ENUM_CERTIFICATESTATUS.Good;
                    }
                    else if (certStatus is RevokedStatus)
                    {
                        result = ENUM_CERTIFICATESTATUS.Revoked;
                    }
                    else if (certStatus is UnknownStatus)
                    {
                        result = ENUM_CERTIFICATESTATUS.Unknown;
                    }
                }
                return result;
            }
            throw new Exception("Phản hồi OCSP không hợp lệ '" + ocspResp.Status + "'.");
        }

        private void VerifyResponseSignature(BasicOcspResp response, X509Certificate issuerCertificate)
        {
            VerifyResponseSignature(response, issuerCertificate.GetPublicKey());
            VerifyResponderCertificate(issuerCertificate, response.GetCerts()[0]);
        }

        private void VerifyResponderCertificate(X509Certificate issuerCertificate, X509Certificate responderCertificate)
        {
            if (!issuerCertificate.SubjectDN.Equivalent(responderCertificate.IssuerDN))
            {
                throw new Exception("OCSP không xác thực");
            }
        }

        private void VerifyResponseSignature(BasicOcspResp response, AsymmetricKeyParameter asymmetricKeyParameter)
        {
            try
            {
                response.GetCerts()[0].Verify(asymmetricKeyParameter);
            }
            catch
            {
                throw new Exception("OCSP không hợp lệ");
            }
            if (!response.Verify(response.GetCerts()[0].GetPublicKey()))
            {
                throw new Exception("OCSP không hợp lệ");
            }
        }

        private void VerifyResponseFreshness(SingleResp response)
        {
            if (response.NextUpdate != null)
            {
                if (response.NextUpdate.Value.Ticks <= DateTime.Now.Ticks)
                {
                    throw new Exception("Invalid OCSP response: Next update time is in the past.");
                }
            }

            if (Math.Abs(response.ThisUpdate.Ticks - DateTime.Now.Ticks) > MaxClockSkew)
            {
                throw new Exception("Invalid OCSP response: This update time is too far in the past or future. Max clock skew reached.");
            }
        }

        private void VerifyCertificateId(X509Certificate issuerCertificate, X509Certificate certificate, CertificateID responseCertificateId)
        {
            CertificateID certificateID = new CertificateID("1.3.14.3.2.26", issuerCertificate, certificate.SerialNumber);
            if (!certificateID.SerialNumber.Equals(responseCertificateId.SerialNumber))
            {
                throw new Exception("Không đúng chứng thư số cần kiểm tra trên kết qua trả về");
            }
            if (!Arrays.AreEqual(certificateID.GetIssuerNameHash(), responseCertificateId.GetIssuerNameHash()))
            {
                throw new Exception("Không đúng chứng thư số cơ quan cấp phát trên kết quả trả về");
            }
        }

        private OcspReq GenerateOCSPRequest(X509Certificate issuerCertificate, BigInteger serialNumber)
        {
            CertificateID certificateID = new CertificateID("1.3.14.3.2.26", issuerCertificate, serialNumber);
            return GenerateOCSPRequest(certificateID);
        }

        private OcspReq GenerateOCSPRequest(CertificateID certificateId)
        {
            OcspReqGenerator ocspReqGenerator = new OcspReqGenerator();
            ocspReqGenerator.AddRequest(certificateId);
            BigInteger.ValueOf(default(DateTime).Ticks);
            ArrayList arrayList = new ArrayList();
            Hashtable hashtable = new Hashtable();
            arrayList.Add(OcspObjectIdentifiers.PkixOcsp);
            Asn1OctetString value = new DerOctetString(new DerOctetString(new byte[10]
            {
                1,
                3,
                6,
                1,
                5,
                5,
                7,
                48,
                1,
                1
            }));
            hashtable.Add(OcspObjectIdentifiers.PkixOcsp, new X509Extension(critical: false, value: value));
            ocspReqGenerator.SetRequestExtensions(new X509Extensions(arrayList, hashtable));
            return ocspReqGenerator.Generate();
        }
    }
}
