using Sign.itext.error_messages;
using Sign.itext.pdf.security;
using Sign.itext.text.log;
using Sign.itext.text.pdf.security;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Tsp;
using Sign.SystemItext.util;
using System.Net;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class TSAClientBouncyCastle : ITSAClient
    {
        private static readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(TSAClientBouncyCastle));

        protected internal string tsaURL;

        protected internal string tsaUsername;

        protected internal string tsaPassword;

        protected ITSAInfoBouncyCastle tsaInfo;

        public const int DEFAULTTOKENSIZE = 4096;

        protected internal int tokenSizeEstimate;

        public const string DEFAULTHASHALGORITHM = "SHA-256";

        protected internal string digestAlgorithm;

        private WebProxy proxy;

        public WebProxy Proxy
        {
            get
            {
                return proxy;
            }
            set
            {
                proxy = value;
            }
        }

        public TSAClientBouncyCastle(string url)
            : this(url, null, null, 4096, "SHA-256")
        {
        }

        public TSAClientBouncyCastle(string url, string username, string password)
            : this(url, username, password, 4096, "SHA-256")
        {
        }

        public TSAClientBouncyCastle(string url, string username, string password, int tokSzEstimate, string digestAlgorithm)
        {
            tsaURL = url;
            tsaUsername = username;
            tsaPassword = password;
            tokenSizeEstimate = tokSzEstimate;
            this.digestAlgorithm = digestAlgorithm;
        }

        public virtual void SetTSAInfo(ITSAInfoBouncyCastle tsaInfo)
        {
            this.tsaInfo = tsaInfo;
        }

        public virtual int GetTokenSizeEstimate()
        {
            return tokenSizeEstimate;
        }

        public virtual IDigest GetMessageDigest()
        {
            return DigestAlgorithms.GetMessageDigest(digestAlgorithm);
        }

        public virtual byte[] GetTimeStampToken(byte[] imprint)
        {
            TimeStampRequestGenerator timeStampRequestGenerator = new TimeStampRequestGenerator();
            timeStampRequestGenerator.SetCertReq(certReq: true);
            TimeStampRequest timeStampRequest = timeStampRequestGenerator.Generate(nonce: BigInteger.ValueOf(DateTime.Now.Ticks + Environment.TickCount), digestAlgorithmOid: DigestAlgorithms.GetAllowedDigests(digestAlgorithm), digest: imprint);
            byte[] encoded = timeStampRequest.GetEncoded();
            TimeStampResponse timeStampResponse = new TimeStampResponse(GetTSAResponse(encoded));
            timeStampResponse.Validate(timeStampRequest);
            int num = timeStampResponse.GetFailInfo()?.IntValue ?? 0;
            if (num != 0)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("invalid.tsa.1.response.code.2", tsaURL, num));
            }

            TimeStampToken timeStampToken = timeStampResponse.TimeStampToken;
            if (timeStampToken == null)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("tsa.1.failed.to.return.time.stamp.token.2", tsaURL, timeStampResponse.GetStatusString()));
            }

            TimeStampTokenInfo timeStampInfo = timeStampToken.TimeStampInfo;
            byte[] encoded2 = timeStampToken.GetEncoded();
            LOGGER.Info("Timestamp generated: " + timeStampInfo.GenTime);
            if (tsaInfo != null)
            {
                tsaInfo.InspectTimeStampTokenInfo(timeStampInfo);
            }

            tokenSizeEstimate = encoded2.Length + 32;
            return encoded2;
        }

        protected internal virtual byte[] GetTSAResponse(byte[] requestBytes)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(tsaURL);
            httpWebRequest.ContentLength = requestBytes.Length;
            httpWebRequest.ContentType = "application/timestamp-query";
            httpWebRequest.Method = "POST";
            httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            if (proxy != null)
            {
                httpWebRequest.Proxy = proxy;
            }

            if (tsaUsername != null && !tsaUsername.Equals(""))
            {
                string s = tsaUsername + ":" + tsaPassword;
                s = Convert.ToBase64String(Encoding.Default.GetBytes(s), Base64FormattingOptions.None);
                httpWebRequest.Headers["Authorization"] = "Basic " + s;
            }

            try
            {
                Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                requestStream.Close();
            }
            catch (Exception innerException)
            {
                throw new WebException("Không thể gửi yêu cầu đến TSA", innerException);
            }

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            if (httpWebResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("invalid.http.response.1", (int)httpWebResponse.StatusCode));
            }

            Stream responseStream = httpWebResponse.GetResponseStream();
            MemoryStream memoryStream = new MemoryStream();
            byte[] array = new byte[1024];
            int num = 0;
            while ((num = responseStream.Read(array, 0, array.Length)) > 0)
            {
                memoryStream.Write(array, 0, num);
            }

            responseStream.Close();

            byte[] array2 = memoryStream.ToArray();
            string contentEncoding = httpWebResponse.ContentEncoding;
            if (contentEncoding != null && Util.EqualsIgnoreCase(contentEncoding, "base64"))
            {
                array2 = Convert.FromBase64String(Encoding.ASCII.GetString(array2));
            }

            httpWebResponse.Close();

            return array2;
        }
    }
}
