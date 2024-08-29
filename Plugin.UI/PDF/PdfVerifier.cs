using Sign.itext.pdf;
using Sign.itext.pdf.security;
using Sign.itext.text.pdf;
using Sign.Org.BouncyCastle.Cms;
using Sign.Org.BouncyCastle.Tsp;
using Sign.Org.BouncyCastle.X509.Store;
using Sign.PDF;
using Sign.X509;
using System.Collections;
using System.ComponentModel;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Plugin.UI.PDF
{
    public class PdfVerifier
    {
        private BackgroundWorker bgWorker;
        private SignatureValidity _verificationResult;
        private string _filePath;
        private PdfProccessDelegate pdfProccessDelegate;
        public delegate void PdfProccessDelegate(object sender, PdfProccessEventArgs e);
        private PdfProccessCompletedDelegate pdfProccessCompletedDelegate;
        public delegate void PdfProccessCompletedDelegate(object sender, PdfProccessCompletedEventArgs e);
        private string[] _additionalCRLs;
        private byte[] _fileData;
        private bool _allowOnlineChecking = true;

        private string _signatureName;

        private SignatureInfo _signature;
        public SignatureInfo Signature => _signature;
        public SignatureValidity VerifyResult => this._verificationResult;
        public WebProxy Proxy { get; set; }
        public string[] AdditionalCRLs
        {
            get { return _additionalCRLs; }
            set { _additionalCRLs = value; }
        }

        public bool AllowedOnlineChecking
        {
            get
            {
                return _allowOnlineChecking;
            }
            set
            {
                _allowOnlineChecking = value;
            }
        }
        public PdfVerifier(string fileName)
        {
            _filePath = fileName;
        }
        public PdfVerifier(byte[] fileData)
        {
            _fileData = fileData;
        }
        public string SignatureName
        {
            get => this._signatureName;
            set => this._signatureName = value;
        }
        public PdfVerifier(string fileName, string signature)
        {
            this._filePath = fileName;
            this._signatureName = signature;
            this.bgWorker = new BackgroundWorker();
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.WorkerSupportsCancellation = true;
        }
        private ProgressChangedEventHandler progressChangedEventHandler;
        private RunWorkerCompletedEventHandler runWorkerCompletedEventHandler;
        public event ProgressChangedEventHandler AsyncPdfProcessing
        {
            add
            {
                ProgressChangedEventHandler changedEventHandler = this.progressChangedEventHandler;
                ProgressChangedEventHandler comparand;
                do
                {
                    comparand = changedEventHandler;
                    changedEventHandler = Interlocked.CompareExchange<ProgressChangedEventHandler>(ref this.progressChangedEventHandler, comparand + value, comparand);
                }
                while (changedEventHandler != comparand);
            }
            remove
            {
                ProgressChangedEventHandler changedEventHandler = this.progressChangedEventHandler;
                ProgressChangedEventHandler comparand;
                do
                {
                    comparand = changedEventHandler;
                    changedEventHandler = Interlocked.CompareExchange<ProgressChangedEventHandler>(ref this.progressChangedEventHandler, comparand - value, comparand);
                }
                while (changedEventHandler != comparand);
            }
        }

        public event RunWorkerCompletedEventHandler AsyncProcessCompleted
        {
            add
            {
                RunWorkerCompletedEventHandler completedEventHandler = this.runWorkerCompletedEventHandler;
                RunWorkerCompletedEventHandler comparand;
                do
                {
                    comparand = completedEventHandler;
                    completedEventHandler = Interlocked.CompareExchange<RunWorkerCompletedEventHandler>(ref this.runWorkerCompletedEventHandler, comparand + value, comparand);
                }
                while (completedEventHandler != comparand);
            }
            remove
            {
                RunWorkerCompletedEventHandler completedEventHandler = this.runWorkerCompletedEventHandler;
                RunWorkerCompletedEventHandler comparand;
                do
                {
                    comparand = completedEventHandler;
                    completedEventHandler = Interlocked.CompareExchange<RunWorkerCompletedEventHandler>(ref this.runWorkerCompletedEventHandler, comparand - value, comparand);
                }
                while (completedEventHandler != comparand);
            }
        }

        protected void OnPdfProccessing(PdfProccessEventArgs e)
        {
            if (pdfProccessDelegate != null)
            {
                pdfProccessDelegate(this, e);
            }
        }
        private int CountEofMarkers(Stream stream_0)
        {
            PRTokeniser pRTokeniser = new PRTokeniser(new RandomAccessFileOrArray(stream_0));
            int num = 0;
            try
            {
                pRTokeniser.Seek(0L);
                byte[] array = new byte[5];
                while (true)
                {
                    long filePointer = pRTokeniser.FilePointer;
                    if (pRTokeniser.ReadLineSegment(array, isNullWhitespace: true))
                    {
                        if (array[0] == 37 && PdfEncodings.ConvertToString(array, null).StartsWith("%%EOF"))
                        {
                            num++;
                            pRTokeniser.Seek(filePointer);
                            pRTokeniser.NextToken();
                            filePointer = pRTokeniser.FilePointer;
                        }
                        continue;
                    }
                    break;
                }
                return num;
            }
            finally
            {
                pRTokeniser.Close();
            }
        }
        protected void OnPdfProccessCompleted(PdfProccessCompletedEventArgs e)
        {
            if (pdfProccessCompletedDelegate != null)
            {
                pdfProccessCompletedDelegate(this, e);
            }
        }
        public static Sign.Org.BouncyCastle.X509.X509Certificate GetCertificate(SignerInformation signer, IX509Store cmsCertificates)
        {
            Sign.Org.BouncyCastle.X509.X509Certificate result = null;
            X509CertStoreSelector x509CertStoreSelector = new X509CertStoreSelector();
            x509CertStoreSelector.Issuer = signer.SignerID.Issuer;
            x509CertStoreSelector.SerialNumber = signer.SignerID.SerialNumber;
            IList list = new ArrayList(cmsCertificates.GetMatches(x509CertStoreSelector));
            if (list.Count > 0)
            {
                result = (Sign.Org.BouncyCastle.X509.X509Certificate)list[0];
            }
            return result;
        }

        public List<SignatureInfo> Verify()
        {
            _verificationResult = SignatureValidity.None;
            List<SignatureInfo> list = new List<SignatureInfo>();
            PdfReader pdfReader = null;
            try
            {
                pdfReader = new PdfReader(_filePath);
                AcroFields acroFields = pdfReader.AcroFields;
                List<string> signatureNames = acroFields.GetSignatureNames();
                if (signatureNames.Count == 0)
                {
                    _verificationResult = SignatureValidity.NotSigned;
                }
                bool flag = false;
                bool flag2 = false;
                List<int> list2 = new List<int>();
                for (int i = 0; i < signatureNames.Count; i++)
                {
                    SignatureInfo item = default(SignatureInfo);
                    item.ValidityErrors = new Dictionary<SignatureValidity, string>();
                    string text = item.SignatureName = signatureNames[i];
                    try
                    {
                        SignatureValidity signatureValidity = SignatureValidity.None;
                        DateTime minValue = DateTime.MinValue;
                        item.SignatureCoversWholeDocument = acroFields.SignatureCoversWholeDocument(text);
                        flag |= item.SignatureCoversWholeDocument;
                        AcroFields.FieldPosition fieldPosition = acroFields.GetFieldPositions(text)[0];
                        item.Position = new Rectangle((int)fieldPosition.position.Left, (int)fieldPosition.position.Top, (int)fieldPosition.position.Width, (int)fieldPosition.position.Height);
                        item.PageIndex = fieldPosition.page;
                        OnPdfProccessing(new PdfProccessEventArgs(ValidityProccess.VerifyDocument));
                        using (Stream stream_ = acroFields.ExtractRevision(text))
                        {
                            int item2 = CountEofMarkers(stream_);
                            list2.Add(item2);
                        }
                        PdfPKCS7 pdfPKCS = acroFields.VerifySignature(text);
                        item.IsTsp = pdfPKCS.IsTsp;
                        item.SigningTime = pdfPKCS.SignDate;
                        minValue = item.SigningTime;
                        item.SigningCertificate = pdfPKCS.SigningCertificate.GetEncoded();
                        TimeStampToken timeStampToken = pdfPKCS.TimeStampToken;
                        if (timeStampToken != null)
                        {
                            item.TimeStampDate = pdfPKCS.TimeStampDate.ToLocalTime();
                            minValue = item.TimeStampDate;
                        }
                        if (!pdfPKCS.Verify())
                        {
                            signatureValidity = SignatureValidity.DocumentModified;
                            item.ValidityErrors.Add(SignatureValidity.DocumentModified, "Tài liệu đã bị thay đổi.");
                        }
                        else if (!item.SignatureCoversWholeDocument)
                        {
                            signatureValidity = SignatureValidity.NonCoversWholeDocument;
                            item.ValidityErrors.Add(SignatureValidity.NonCoversWholeDocument, "Nội dung đã ký số chưa bị thay đổi. Tuy nhiên, tài liệu đã có những thay đổi trong giới hạn cho phép.");
                        }
                        _verificationResult |= signatureValidity;
                        OnPdfProccessCompleted(new PdfProccessCompletedEventArgs(ValidityProccess.VerifyDocument, signatureValidity));
                        OnPdfProccessing(new PdfProccessEventArgs(ValidityProccess.CheckingSigningCert));
                        signatureValidity = SignatureValidity.None;
                        CertChecker certChecker = new CertChecker(new X509Certificate2(item.SigningCertificate), minValue);
                        certChecker.AdditionalCRLs = _additionalCRLs;
                        certChecker.OnlineCheckingAllowed = _allowOnlineChecking;
                        certChecker.CheckingViaOcsp = false;
                        try
                        {
                            int num = certChecker.Check();
                            string value = "";
                            switch (num)
                            {
                                case (int)CertCheckResult.CertExpired:
                                    signatureValidity = SignatureValidity.InvalidSigningCertificate;
                                    value = "Chứng thư số đã hết hạn sử dụng";
                                    break;
                                case (int)CertCheckResult.CertNotYetValid:
                                    signatureValidity = SignatureValidity.InvalidSigningCertificate;
                                    value = "Chứng thư số chưa có hiệu lực";
                                    break;
                                case (int)CertCheckResult.InvalidCertChain:
                                    signatureValidity = SignatureValidity.InvalidSigningCertificate;
                                    value = "Đường dẫn chứng thực không hợp lệ";
                                    break;
                                case (int)CertCheckResult.UntrustedRoot:
                                    signatureValidity = SignatureValidity.InvalidSigningCertificate;
                                    value = "Chứng thư số không tin cậy";
                                    break;
                                case (int)CertCheckResult.InvalidCrlDistPoints:
                                    signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                                    value = "Lỗi cấu trúc CTS - đường dẫn danh sách CTS bị thu hồi không hợp lệ";
                                    break;
                                case (int)CertCheckResult.CouldnotDownloadCrl:
                                    signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                                    value = "Lỗi tải danh sách chứng thư bị thu hồi";
                                    break;
                                case (int)CertCheckResult.OnlineCheckingCertDisabled:
                                    signatureValidity = SignatureValidity.NonCheckingRevokedSigningCert;
                                    value = "Không kiểm tra tình trạng hủy bỏ của chứng thư số ký.";
                                    break;
                                case (int)CertCheckResult.CertIsRevoked:
                                    signatureValidity = SignatureValidity.InvalidSigningCertificate;
                                    value = "Chứng thư số đã bị thu hồi";
                                    break;
                                case (int)CertCheckResult.CaCertIsRevoked:
                                    signatureValidity = SignatureValidity.InvalidSigningCertificate;
                                    value = "Chứng thư số CA đã bị thu hồi";
                                    break;
                                case (int)CertCheckResult.InvalidCrl:
                                    signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                                    value = "Danh sách CTS bị thu hồi không hợp lệ";
                                    break;
                                case (int)CertCheckResult.OcspRespUnknown:
                                    signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                                    value = "Dịch vụ OCSP trả về kết quả UNKNOWN";
                                    break;
                            }
                            if (signatureValidity != 0)
                            {
                                item.ValidityErrors.Add(signatureValidity, value);
                            }
                        }
                        catch (Exception ex)
                        {
                            signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                            item.ValidityErrors.Add(SignatureValidity.ErrorCheckingSigningCertificate, ex.Message);
                        }
                        _verificationResult |= signatureValidity;
                        OnPdfProccessCompleted(new PdfProccessCompletedEventArgs(ValidityProccess.CheckingSigningCert, signatureValidity));
                        OnPdfProccessing(new PdfProccessEventArgs(ValidityProccess.VerifyTimeStamp));
                        signatureValidity = SignatureValidity.None;
                        try
                        {
                            if (timeStampToken != null)
                            {
                                ICollection signers = timeStampToken.ToCmsSignedData().GetSignerInfos().GetSigners();
                                IX509Store certificates = timeStampToken.GetCertificates("Collection");
                                {
                                    IEnumerator enumerator = signers.GetEnumerator();
                                    try
                                    {
                                        if (enumerator.MoveNext())
                                        {
                                            SignerInformation obj = (SignerInformation)enumerator.Current;
                                            Sign.Org.BouncyCastle.X509.X509Certificate certificate = GetCertificate(obj, certificates);
                                            item.TimeStampCertificate = certificate.GetEncoded();
                                            if (!obj.Verify(certificate))
                                            {
                                                signatureValidity |= SignatureValidity.InvalidTimestampImprint;
                                                item.ValidityErrors.Add(SignatureValidity.InvalidTimestampImprint, "Dấu thời gian không hợp lệ.");
                                                throw new Exception("Dấu thời gian không hợp lệ.");
                                            }
                                        }
                                    }
                                    catch (Exception testEx)
                                    {

                                    }
                                    finally
                                    {
                                        IDisposable disposable = enumerator as IDisposable;
                                        if (disposable != null)
                                        {
                                            disposable.Dispose();
                                        }
                                    }
                                }
                                if (!pdfPKCS.IsTsp)
                                {
                                    if (!pdfPKCS.VerifyTimestampImprint())
                                    {
                                        signatureValidity = SignatureValidity.InvalidTimestampImprint;
                                        item.ValidityErrors.Add(SignatureValidity.InvalidTimestampImprint, "Dấu thời gian không hợp lệ.");
                                    }
                                    else
                                    {
                                        certChecker = new CertChecker(new X509Certificate2(item.TimeStampCertificate), minValue);
                                        certChecker.AdditionalCRLs = _additionalCRLs;
                                        certChecker.OnlineCheckingAllowed = _allowOnlineChecking;
                                        certChecker.CheckingViaOcsp = false;
                                        try
                                        {
                                            int num2 = certChecker.Check();
                                            string value2 = "";
                                            SignatureValidity signatureValidity2 = SignatureValidity.None;
                                            switch (num2)
                                            {
                                                case (int)CertCheckResult.CertExpired:
                                                    signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                                    value2 = "Chứng thư số đã hết hạn sử dụng";
                                                    break;
                                                case (int)CertCheckResult.CertNotYetValid:
                                                    signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                                    value2 = "Chứng thư số chưa có hiệu lực";
                                                    break;
                                                case (int)CertCheckResult.InvalidCertChain:
                                                    signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                                    value2 = "Đường dẫn chứng thực không hợp lệ";
                                                    break;
                                                case (int)CertCheckResult.UntrustedRoot:
                                                    signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                                    value2 = "Chứng thư số không tin cậy";
                                                    break;
                                                case (int)CertCheckResult.InvalidCrlDistPoints:
                                                    signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                                                    value2 = "Lỗi cấu trúc CTS - đường dẫn danh sách CTS thu hồi không hợp lệ";
                                                    break;
                                                case (int)CertCheckResult.CouldnotDownloadCrl:
                                                    signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                                                    value2 = "Lỗi tải danh sách chứng thư thu hồi";
                                                    break;
                                                case (int)CertCheckResult.OnlineCheckingCertDisabled:
                                                    signatureValidity2 = SignatureValidity.NonCheckingRevokedTSACert;
                                                    value2 = "Không kiểm tra tình trạng hủy bỏ của chứng thư số máy chủ cấp dấu thời gian.";
                                                    break;
                                                case (int)CertCheckResult.CertIsRevoked:
                                                    signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                                    value2 = "Chứng thư số đã bị thu hồi";
                                                    break;
                                                case (int)CertCheckResult.CaCertIsRevoked:
                                                    signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                                    value2 = "Chứng thư số CA đã thu hồi";
                                                    break;
                                                case (int)CertCheckResult.InvalidCrl:
                                                    signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                                                    value2 = "Danh sách CTS thu hồi không hợp lệ";
                                                    break;
                                                case (int)CertCheckResult.OcspRespUnknown:
                                                    signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                                                    value2 = "Dịch vụ OCSP trả về kết quả UNKNOWN";
                                                    break;
                                            }
                                            if (signatureValidity2 != 0)
                                            {
                                                item.ValidityErrors.Add(signatureValidity2, value2);
                                                signatureValidity = signatureValidity2;
                                            }
                                        }
                                        catch (Exception ex2)
                                        {
                                            signatureValidity = SignatureValidity.ErrorCheckingTSACertificate;
                                            item.ValidityErrors.Add(SignatureValidity.ErrorCheckingTSACertificate, ex2.Message);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                signatureValidity = SignatureValidity.NotTimestamped;
                                item.ValidityErrors.Add(SignatureValidity.NotTimestamped, "Chữ ký không gắn dấu thời gian.");
                            }
                        }
                        catch (Exception ex3)
                        {
                            if (signatureValidity == SignatureValidity.None)
                            {
                                signatureValidity = SignatureValidity.ErrorCheckingTSACertificate;
                                item.ValidityErrors.Add(SignatureValidity.ErrorCheckingTSACertificate, "Lỗi: " + ex3.Message);
                            }
                        }
                        _verificationResult |= signatureValidity;
                        OnPdfProccessCompleted(new PdfProccessCompletedEventArgs(ValidityProccess.VerifyTimeStamp, signatureValidity));
                    }
                    catch (Exception ex4)
                    {
                        item.ValidityErrors.Add(SignatureValidity.FatalError, $"Định dạng chữ ký không hợp lệ ({ex4.Message})");
                    }
                    list.Add(item);
                }
                int num3 = list2.Count - 1;
                while (num3 > 0)
                {
                    int num4 = list2[num3];
                    int num5 = list2[num3 - 1];
                    if (num4 - num5 <= 1)
                    {
                        num3--;
                        continue;
                    }
                    flag2 = true;
                    break;
                }
                if (!flag || flag2)
                {
                    _verificationResult |= SignatureValidity.DocumentModified;
                    {
                        foreach (SignatureInfo item3 in list)
                        {
                            item3.ValidityErrors.Add(SignatureValidity.DocumentModified, "Tài liệu đã bị thay đổi.");
                        }
                        return list;
                    }
                }
                return list;
            }
            finally
            {
                pdfReader?.Close();
            }
        }

        public List<SignatureInfo> VerifyByteSigned()
        {
            _verificationResult = SignatureValidity.None;
            List<SignatureInfo> list = new List<SignatureInfo>();
            PdfReader pdfReader = null;
            try
            {
                pdfReader = new PdfReader(_fileData);
                AcroFields acroFields = pdfReader.AcroFields;
                List<string> signatureNames = acroFields.GetSignatureNames();
                if (signatureNames.Count == 0)
                {
                    _verificationResult = SignatureValidity.NotSigned;
                }
                bool flag = false;
                bool flag2 = false;
                List<int> list2 = new List<int>();
                for (int i = 0; i < signatureNames.Count; i++)
                {
                    SignatureInfo item = default(SignatureInfo);
                    item.ValidityErrors = new Dictionary<SignatureValidity, string>();
                    string text = item.SignatureName = signatureNames[i];
                    try
                    {
                        SignatureValidity signatureValidity = SignatureValidity.None;
                        DateTime minValue = DateTime.MinValue;
                        item.SignatureCoversWholeDocument = acroFields.SignatureCoversWholeDocument(text);
                        flag |= item.SignatureCoversWholeDocument;
                        AcroFields.FieldPosition fieldPosition = acroFields.GetFieldPositions(text)[0];
                        item.Position = new Rectangle((int)fieldPosition.position.Left, (int)fieldPosition.position.Top, (int)fieldPosition.position.Width, (int)fieldPosition.position.Height);
                        item.PageIndex = fieldPosition.page;
                        OnPdfProccessing(new PdfProccessEventArgs(ValidityProccess.VerifyDocument));
                        using (Stream stream_ = acroFields.ExtractRevision(text))
                        {
                            int item2 = CountEofMarkers(stream_);
                            list2.Add(item2);
                        }
                        PdfPKCS7 pdfPKCS = acroFields.VerifySignature(text);
                        item.IsTsp = pdfPKCS.IsTsp;
                        item.SigningTime = pdfPKCS.SignDate;
                        minValue = item.SigningTime;
                        item.SigningCertificate = pdfPKCS.SigningCertificate.GetEncoded();
                        TimeStampToken timeStampToken = pdfPKCS.TimeStampToken;
                        if (timeStampToken != null)
                        {
                            item.TimeStampDate = pdfPKCS.TimeStampDate.ToLocalTime();
                            minValue = item.TimeStampDate;
                        }
                        if (!pdfPKCS.Verify())
                        {
                            signatureValidity = SignatureValidity.DocumentModified;
                            item.ValidityErrors.Add(SignatureValidity.DocumentModified, "Tài liệu đã bị thay đổi.");
                        }
                        else if (!item.SignatureCoversWholeDocument)
                        {
                            signatureValidity = SignatureValidity.NonCoversWholeDocument;
                            item.ValidityErrors.Add(SignatureValidity.NonCoversWholeDocument, "Nội dung đã ký số chưa bị thay đổi. Tuy nhiên, tài liệu đã có những thay đổi trong giới hạn cho phép.");
                        }
                        _verificationResult |= signatureValidity;
                        OnPdfProccessCompleted(new PdfProccessCompletedEventArgs(ValidityProccess.VerifyDocument, signatureValidity));
                        OnPdfProccessing(new PdfProccessEventArgs(ValidityProccess.CheckingSigningCert));
                        signatureValidity = SignatureValidity.None;
                        CertChecker certChecker = new CertChecker(new X509Certificate2(item.SigningCertificate), minValue);
                        certChecker.AdditionalCRLs = _additionalCRLs;
                        certChecker.OnlineCheckingAllowed = _allowOnlineChecking;
                        certChecker.CheckingViaOcsp = false;
                        try
                        {
                            int num = certChecker.Check();
                            string value = "";
                            switch (num)
                            {
                                case (int)CertCheckResult.CertExpired:
                                    signatureValidity = SignatureValidity.InvalidSigningCertificate;
                                    value = "Chứng thư số đã hết hạn sử dụng";
                                    break;
                                case (int)CertCheckResult.CertNotYetValid:
                                    signatureValidity = SignatureValidity.InvalidSigningCertificate;
                                    value = "Chứng thư số chưa có hiệu lực";
                                    break;
                                case (int)CertCheckResult.InvalidCertChain:
                                    signatureValidity = SignatureValidity.InvalidSigningCertificate;
                                    value = "Đường dẫn chứng thực không hợp lệ";
                                    break;
                                case (int)CertCheckResult.UntrustedRoot:
                                    signatureValidity = SignatureValidity.InvalidSigningCertificate;
                                    value = "Chứng thư số không tin cậy";
                                    break;
                                case (int)CertCheckResult.InvalidCrlDistPoints:
                                    signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                                    value = "Lỗi cấu trúc CTS - đường dẫn danh sách CTS bị thu hồi không hợp lệ";
                                    break;
                                case (int)CertCheckResult.CouldnotDownloadCrl:
                                    signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                                    value = "Lỗi tải danh sách chứng thư bị thu hồi";
                                    break;
                                case (int)CertCheckResult.OnlineCheckingCertDisabled:
                                    signatureValidity = SignatureValidity.NonCheckingRevokedSigningCert;
                                    value = "Không kiểm tra tình trạng hủy bỏ của chứng thư số ký.";
                                    break;
                                case (int)CertCheckResult.CertIsRevoked:
                                    signatureValidity = SignatureValidity.InvalidSigningCertificate;
                                    value = "Chứng thư số đã bị thu hồi";
                                    break;
                                case (int)CertCheckResult.CaCertIsRevoked:
                                    signatureValidity = SignatureValidity.InvalidSigningCertificate;
                                    value = "Chứng thư số CA đã bị thu hồi";
                                    break;
                                case (int)CertCheckResult.InvalidCrl:
                                    signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                                    value = "Danh sách CTS bị thu hồi không hợp lệ";
                                    break;
                                case (int)CertCheckResult.OcspRespUnknown:
                                    signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                                    value = "Dịch vụ OCSP trả về kết quả UNKNOWN";
                                    break;
                            }
                            if (signatureValidity != 0)
                            {
                                item.ValidityErrors.Add(signatureValidity, value);
                            }
                        }
                        catch (Exception ex)
                        {
                            signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                            item.ValidityErrors.Add(SignatureValidity.ErrorCheckingSigningCertificate, ex.Message);
                        }
                        _verificationResult |= signatureValidity;
                        OnPdfProccessCompleted(new PdfProccessCompletedEventArgs(ValidityProccess.CheckingSigningCert, signatureValidity));
                        OnPdfProccessing(new PdfProccessEventArgs(ValidityProccess.VerifyTimeStamp));
                        signatureValidity = SignatureValidity.None;
                        try
                        {
                            if (timeStampToken != null)
                            {
                                ICollection signers = timeStampToken.ToCmsSignedData().GetSignerInfos().GetSigners();
                                IX509Store certificates = timeStampToken.GetCertificates("Collection");
                                {
                                    IEnumerator enumerator = signers.GetEnumerator();
                                    try
                                    {
                                        if (enumerator.MoveNext())
                                        {
                                            SignerInformation obj = (SignerInformation)enumerator.Current;
                                            Sign.Org.BouncyCastle.X509.X509Certificate certificate = GetCertificate(obj, certificates);
                                            item.TimeStampCertificate = certificate.GetEncoded();
                                            if (!obj.Verify(certificate))
                                            {
                                                signatureValidity |= SignatureValidity.InvalidTimestampImprint;
                                                item.ValidityErrors.Add(SignatureValidity.InvalidTimestampImprint, "Dấu thời gian không hợp lệ.");
                                                throw new Exception("Dấu thời gian không hợp lệ.");
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        IDisposable disposable = enumerator as IDisposable;
                                        if (disposable != null)
                                        {
                                            disposable.Dispose();
                                        }
                                    }
                                }
                                if (!pdfPKCS.IsTsp)
                                {
                                    if (!pdfPKCS.VerifyTimestampImprint())
                                    {
                                        signatureValidity = SignatureValidity.InvalidTimestampImprint;
                                        item.ValidityErrors.Add(SignatureValidity.InvalidTimestampImprint, "Dấu thời gian không hợp lệ.");
                                    }
                                    else
                                    {
                                        certChecker = new CertChecker(new X509Certificate2(item.TimeStampCertificate), minValue);
                                        certChecker.AdditionalCRLs = _additionalCRLs;
                                        certChecker.OnlineCheckingAllowed = _allowOnlineChecking;
                                        certChecker.CheckingViaOcsp = false;
                                        try
                                        {
                                            int num2 = certChecker.Check();
                                            string value2 = "";
                                            SignatureValidity signatureValidity2 = SignatureValidity.None;
                                            switch (num2)
                                            {
                                                case (int)CertCheckResult.CertExpired:
                                                    signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                                    value2 = "Chứng thư số đã hết hạn sử dụng";
                                                    break;
                                                case (int)CertCheckResult.CertNotYetValid:
                                                    signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                                    value2 = "Chứng thư số chưa có hiệu lực";
                                                    break;
                                                case (int)CertCheckResult.InvalidCertChain:
                                                    signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                                    value2 = "Đường dẫn chứng thực không hợp lệ";
                                                    break;
                                                case (int)CertCheckResult.UntrustedRoot:
                                                    signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                                    value2 = "Chứng thư số không tin cậy";
                                                    break;
                                                case (int)CertCheckResult.InvalidCrlDistPoints:
                                                    signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                                                    value2 = "Lỗi cấu trúc CTS - đường dẫn danh sách CTS thu hồi không hợp lệ";
                                                    break;
                                                case (int)CertCheckResult.CouldnotDownloadCrl:
                                                    signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                                                    value2 = "Lỗi tải danh sách chứng thư thu hồi";
                                                    break;
                                                case (int)CertCheckResult.OnlineCheckingCertDisabled:
                                                    signatureValidity2 = SignatureValidity.NonCheckingRevokedTSACert;
                                                    value2 = "Không kiểm tra tình trạng hủy bỏ của chứng thư số máy chủ cấp dấu thời gian.";
                                                    break;
                                                case (int)CertCheckResult.CertIsRevoked:
                                                    signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                                    value2 = "Chứng thư số đã bị thu hồi";
                                                    break;
                                                case (int)CertCheckResult.CaCertIsRevoked:
                                                    signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                                    value2 = "Chứng thư số CA đã thu hồi";
                                                    break;
                                                case (int)CertCheckResult.InvalidCrl:
                                                    signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                                                    value2 = "Danh sách CTS thu hồi không hợp lệ";
                                                    break;
                                                case (int)CertCheckResult.OcspRespUnknown:
                                                    signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                                                    value2 = "Dịch vụ OCSP trả về kết quả UNKNOWN";
                                                    break;
                                            }
                                            if (signatureValidity2 != 0)
                                            {
                                                item.ValidityErrors.Add(signatureValidity2, value2);
                                                signatureValidity = signatureValidity2;
                                            }
                                        }
                                        catch (Exception ex2)
                                        {
                                            signatureValidity = SignatureValidity.ErrorCheckingTSACertificate;
                                            item.ValidityErrors.Add(SignatureValidity.ErrorCheckingTSACertificate, ex2.Message);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                signatureValidity = SignatureValidity.NotTimestamped;
                                item.ValidityErrors.Add(SignatureValidity.NotTimestamped, "Chữ ký không gắn dấu thời gian.");
                            }
                        }
                        catch (Exception ex3)
                        {
                            if (signatureValidity == SignatureValidity.None)
                            {
                                signatureValidity = SignatureValidity.ErrorCheckingTSACertificate;
                                item.ValidityErrors.Add(SignatureValidity.ErrorCheckingTSACertificate, "Lỗi: " + ex3.Message);
                            }
                        }
                        _verificationResult |= signatureValidity;
                        OnPdfProccessCompleted(new PdfProccessCompletedEventArgs(ValidityProccess.VerifyTimeStamp, signatureValidity));
                    }
                    catch (Exception ex4)
                    {
                        item.ValidityErrors.Add(SignatureValidity.FatalError, $"Định dạng chữ ký không hợp lệ ({ex4.Message})");
                    }
                    list.Add(item);
                }
                int num3 = list2.Count - 1;
                while (num3 > 0)
                {
                    int num4 = list2[num3];
                    int num5 = list2[num3 - 1];
                    if (num4 - num5 <= 1)
                    {
                        num3--;
                        continue;
                    }
                    flag2 = true;
                    break;
                }
                if (!flag || flag2)
                {
                    _verificationResult |= SignatureValidity.DocumentModified;
                    {
                        foreach (SignatureInfo item3 in list)
                        {
                            item3.ValidityErrors.Add(SignatureValidity.DocumentModified, "Tài liệu đã bị thay đổi.");
                        }
                        return list;
                    }
                }
                return list;
            }
            finally
            {
                pdfReader?.Close();
            }
        }

        private void verifyWithBackgroundWorker()
        {
            this._signature = new SignatureInfo();
            this._verificationResult = SignatureValidity.None;
            SignatureValidity signatureValidity = SignatureValidity.None;
            DateTime minValue = DateTime.MinValue;
            PdfReader pdfReader = (PdfReader)null;
            bool coversWholeDocument = false;
            bool flag2 = false;
            List<int> eofPositions = new List<int>();
            try
            {
                pdfReader = new PdfReader(this._filePath);
                AcroFields acroFields = pdfReader.AcroFields;
                List<string> signatureNames = acroFields.GetSignatureNames();
                this._signature.SignatureName = this._signatureName;
                string value = (string)null;
                this.bgWorker.ReportProgress(0, (object)new PdfProccessEventArgs(ValidityProccess.VerifyDocument));
                AcroFields.FieldPosition fieldPosition = acroFields.GetFieldPositions(this._signatureName)[0];
                this._signature.Position = new Rectangle((int)fieldPosition.position.Left, (int)fieldPosition.position.Top, (int)fieldPosition.position.Width, (int)fieldPosition.position.Height);
                this._signature.PageIndex = fieldPosition.page;
                this._signature.SignatureCoversWholeDocument = acroFields.SignatureCoversWholeDocument(this._signatureName);
                PdfPKCS7 pdfPkcS7 = acroFields.VerifySignature(this._signatureName);
                this._signature.IsTsp = pdfPkcS7.IsTsp;
                this._signature.SigningTime = pdfPkcS7.SignDate;
                DateTime checkingDatetime = this._signature.SigningTime;
                this._signature.SigningCertificate = pdfPkcS7.SigningCertificate.GetEncoded();
                TimeStampToken timeStampToken = pdfPkcS7.TimeStampToken;
                if (timeStampToken != null)
                {
                    this._signature.TimeStampDate = pdfPkcS7.TimeStampDate.ToLocalTime();
                    checkingDatetime = this._signature.TimeStampDate;
                }
                if (!pdfPkcS7.Verify())
                {
                    signatureValidity = SignatureValidity.DocumentModified;
                    value = "Tài liệu đã bị thay đổi.";
                }
                else
                {
                    bool hasError = false;
                    foreach (string signatureName in signatureNames)
                    {
                        try
                        {
                            coversWholeDocument |= acroFields.SignatureCoversWholeDocument(signatureName);
                            using (Stream revisionStream = acroFields.ExtractRevision(signatureName))
                            {
                                eofPositions.Add(CountEofMarkers(revisionStream));
                            }
                        }
                        catch (Exception)
                        {
                            hasError = true;
                            break;
                        }
                    }
                    if (coversWholeDocument)
                    {
                        for (int index = eofPositions.Count - 1; index > 0; --index)
                        {
                            if (eofPositions[index] - eofPositions[index - 1] > 1)
                            {
                                flag2 = true;
                                break;
                            }
                        }
                    }
                    if (((hasError ? 1 : (!coversWholeDocument ? 1 : 0)) | (flag2 ? 1 : 0)) != 0)
                    {
                        signatureValidity = SignatureValidity.DocumentModified;
                        value = "Tài liệu đã bị thay đổi.";
                    }
                    else if (!this._signature.SignatureCoversWholeDocument)
                    {
                        signatureValidity = SignatureValidity.NonCoversWholeDocument;
                        value = "Nội dung đã ký số chưa bị thay đổi. Tuy nhiên, tài liệu đã có những thay đổi trong giới hạn cho phép.";
                    }
                }
                this.bgWorker.ReportProgress(0, (object)new PdfProccessCompletedEventArgs(ValidityProccess.VerifyDocument, signatureValidity, value));
                this._verificationResult |= signatureValidity;
                this.bgWorker.ReportProgress(0, (object)new PdfProccessEventArgs(ValidityProccess.CheckingSigningCert));
                string message = (string)null;
                SignatureValidity signatureValidity2 = SignatureValidity.None;
                CertChecker certChecker = new CertChecker(new X509Certificate2(this._signature.SigningCertificate), checkingDatetime);
                certChecker.AdditionalCRLs = this._additionalCRLs;
                certChecker.OnlineCheckingAllowed = this._allowOnlineChecking;
                certChecker.CheckingViaOcsp = false;
                try
                {
                    switch (certChecker.Check())
                    {
                        case (int)CertCheckResult.CertExpired:
                            signatureValidity = SignatureValidity.InvalidSigningCertificate;
                            value = "Chứng thư số đã hết hạn sử dụng";
                            break;
                        case (int)CertCheckResult.CertNotYetValid:
                            signatureValidity = SignatureValidity.InvalidSigningCertificate;
                            value = "Chứng thư số chưa có hiệu lực";
                            break;
                        case (int)CertCheckResult.InvalidCertChain:
                            signatureValidity = SignatureValidity.InvalidSigningCertificate;
                            value = "Đường dẫn chứng thực không hợp lệ";
                            break;
                        case (int)CertCheckResult.UntrustedRoot:
                            signatureValidity = SignatureValidity.InvalidSigningCertificate;
                            value = "Chứng thư số không tin cậy";
                            break;
                        case (int)CertCheckResult.InvalidCrlDistPoints:
                            signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                            value = "Lỗi cấu trúc CTS - đường dẫn danh sách CTS bị thu hồi không hợp lệ";
                            break;
                        case (int)CertCheckResult.CouldnotDownloadCrl:
                            signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                            value = "Lỗi tải danh sách chứng thư bị thu hồi";
                            break;
                        case (int)CertCheckResult.OnlineCheckingCertDisabled:
                            signatureValidity = SignatureValidity.NonCheckingRevokedSigningCert;
                            value = "Không kiểm tra tình trạng hủy bỏ của chứng thư số ký.";
                            break;
                        case (int)CertCheckResult.CertIsRevoked:
                            signatureValidity = SignatureValidity.InvalidSigningCertificate;
                            value = "Chứng thư số đã bị thu hồi";
                            break;
                        case (int)CertCheckResult.CaCertIsRevoked:
                            signatureValidity = SignatureValidity.InvalidSigningCertificate;
                            value = "Chứng thư số CA đã bị thu hồi";
                            break;
                        case (int)CertCheckResult.InvalidCrl:
                            signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                            value = "Danh sách CTS bị thu hồi không hợp lệ";
                            break;
                        case (int)CertCheckResult.OcspRespUnknown:
                            signatureValidity = SignatureValidity.ErrorCheckingSigningCertificate;
                            value = "Dịch vụ OCSP trả về kết quả UNKNOWN";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    signatureValidity2 = SignatureValidity.ErrorCheckingSigningCertificate;
                    message = "Lỗi :" + ex.Message;
                }
                this.bgWorker.ReportProgress(0, (object)new PdfProccessCompletedEventArgs(ValidityProccess.CheckingSigningCert, signatureValidity2, message));
                this._verificationResult |= signatureValidity2;
                this.bgWorker.ReportProgress(0, (object)new PdfProccessEventArgs(ValidityProccess.VerifyTimeStamp));
                try
                {
                    signatureValidity2 = SignatureValidity.None;
                    if (timeStampToken != null)
                    {
                        ICollection signers = timeStampToken.ToCmsSignedData().GetSignerInfos().GetSigners();
                        IX509Store certificates = timeStampToken.GetCertificates("Collection");
                        IEnumerator enumerator = signers.GetEnumerator();
                        try
                        {
                            if (enumerator.MoveNext())
                            {
                                SignerInformation current = (SignerInformation)enumerator.Current;
                                Sign.Org.BouncyCastle.X509.X509Certificate certificate = PdfVerifier.GetCertificate(current, certificates);
                                this._signature.TimeStampCertificate = certificate.GetEncoded();
                                if (!current.Verify(certificate))
                                {
                                    signatureValidity2 = SignatureValidity.InvalidTimestampImprint;
                                    message = "Dấu thời gian không hợp lệ.";
                                    throw new Exception();
                                }
                            }
                            else
                            {

                            }
                        }
                        finally
                        {
                            if (enumerator is IDisposable disposable)
                            {
                                disposable.Dispose();
                            }
                        }
                        if (!pdfPkcS7.IsTsp)
                        {
                            if (!pdfPkcS7.VerifyTimestampImprint())
                            {
                                signatureValidity2 = SignatureValidity.InvalidTimestampImprint;
                                message = "Dấu thời gian không hợp lệ.";
                                throw new Exception(message);
                            }
                            else
                            {
                                CertChecker certChecker2 = new CertChecker(new X509Certificate2(this._signature.TimeStampCertificate), checkingDatetime);
                                certChecker2.AdditionalCRLs = this._additionalCRLs;
                                certChecker2.OnlineCheckingAllowed = this._allowOnlineChecking;
                                certChecker2.CheckingViaOcsp = false;
                                try
                                {
                                    switch (certChecker2.Check())
                                    {
                                        case (int)CertCheckResult.CertExpired:
                                            signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                            message = "Chứng thư số đã hết hạn sử dụng";
                                            break;
                                        case (int)CertCheckResult.CertNotYetValid:
                                            signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                            message = "Chứng thư số chưa có hiệu lực";
                                            break;
                                        case (int)CertCheckResult.InvalidCertChain:
                                            signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                            message = "Đường dẫn chứng thực không hợp lệ";
                                            break;
                                        case (int)CertCheckResult.UntrustedRoot:
                                            signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                            message = "Chứng thư số không tin cậy";
                                            break;
                                        case (int)CertCheckResult.InvalidCrlDistPoints:
                                            signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                                            message = "Lỗi cấu trúc CTS - đường dẫn danh sách CTS thu hồi không hợp lệ";
                                            break;
                                        case (int)CertCheckResult.CouldnotDownloadCrl:
                                            signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                                            message = "Lỗi tải danh sách chứng thư thu hồi";
                                            break;
                                        case (int)CertCheckResult.OnlineCheckingCertDisabled:
                                            signatureValidity2 = SignatureValidity.NonCheckingRevokedTSACert;
                                            message = "Không kiểm tra tình trạng hủy bỏ của chứng thư số máy chủ cấp dấu thời gian.";
                                            break;
                                        case (int)CertCheckResult.CertIsRevoked:
                                            signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                            message = "Chứng thư số đã bị thu hồi";
                                            break;
                                        case (int)CertCheckResult.CaCertIsRevoked:
                                            signatureValidity2 = SignatureValidity.InvalidTSACertificate;
                                            message = "Chứng thư số CA đã thu hồi";
                                            break;
                                        case (int)CertCheckResult.InvalidCrl:
                                            signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                                            message = "Danh sách CTS thu hồi không hợp lệ";
                                            break;
                                        case (int)CertCheckResult.OcspRespUnknown:
                                            signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                                            message = "Dịch vụ OCSP trả về kết quả UNKNOWN";
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                                    message = "Lỗi: " + ex.Message;
                                }
                            }
                        }
                    }
                    else
                    {
                        signatureValidity2 = SignatureValidity.NotTimestamped;
                        message = "Chữ ký không gắn dấu thời gian.";
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    if (signatureValidity2 == SignatureValidity.None)
                    {
                        signatureValidity2 = SignatureValidity.ErrorCheckingTSACertificate;
                        message = "Lỗi :" + ex.Message;
                    }
                }
                this.bgWorker.ReportProgress(0, (object)new PdfProccessCompletedEventArgs(ValidityProccess.VerifyTimeStamp, signatureValidity2, message));
                this._verificationResult |= signatureValidity2;
            }
            catch (Exception ex)
            {
                this._verificationResult = SignatureValidity.FatalError;
                this.bgWorker.ReportProgress(0, (object)new PdfProccessCompletedEventArgs(ValidityProccess.VerifyDocument, SignatureValidity.FatalError, string.Format("Định dạng chữ ký không hợp lệ ({0})", (object)ex.Message)));
            }
            finally
            {
                if (pdfReader != null)
                {
                    pdfReader.Close();
                }
            }
        }
        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // ISSUE: reference to a compiler-generated field
            if (progressChangedEventHandler == null)
                return;
            this.progressChangedEventHandler((object)this, e);
        }
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.verifyWithBackgroundWorker();
        }


        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.bgWorker.DoWork -= new DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.ProgressChanged -= new ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
            this.bgWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
            // ISSUE: reference to a compiler-generated field
            if (this.runWorkerCompletedEventHandler == null)
                return;
            this.runWorkerCompletedEventHandler((object)this, e);
        }

        public void AsyncVerify()
        {
            if (this.bgWorker.IsBusy)
                return;
            if (this.bgWorker.CancellationPending)
            {

            }
            else
            {
                this.bgWorker.DoWork += new DoWorkEventHandler(this.bgWorker_DoWork);
                this.bgWorker.ProgressChanged += new ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
                this.bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
                this.bgWorker.RunWorkerAsync();
            }
        }
    }
}
