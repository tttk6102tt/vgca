using Sign.itext.pdf.security;
using Sign.itext.text.pdf.security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Sign.itext
{
    public class X509Signature : IExternalSignature
    {
        private byte[] message;

        private X509Certificate2 certificate;

        //private string hashAlgorithm;
        private readonly HashAlgorithmName hashAlgorithm;
        private string encryptionAlgorithm;

        //public X509Signature(X509Certificate2 certificate, string hashAlgorithm)
        //{
        //    if (!certificate.HasPrivateKey)
        //    {
        //        throw new ArgumentException("Không tìm thấy khóa ký.");
        //    }
        //    this.certificate = certificate;
        //    this.hashAlgorithm = DigestAlgorithms.GetDigest(DigestAlgorithms.GetAllowedDigests(hashAlgorithm));
        //    if (certificate.PrivateKey is RSACryptoServiceProvider)
        //    {
        //        encryptionAlgorithm = "RSA";
        //        return;
        //    }
        //    if (!(certificate.PrivateKey is DSACryptoServiceProvider))
        //    {
        //        throw new ArgumentException("Thuật toán mã hóa không xác định " + certificate.PrivateKey);
        //    }
        //    encryptionAlgorithm = "DSA";
        //}
        public X509Signature(X509Certificate2 certificate, string hashAlgorithm)
        {
            if (!certificate.HasPrivateKey)
            {
                throw new ArgumentException("Không tìm thấy khóa ký.");
            }

            this.certificate = certificate;

            // Sử dụng HashAlgorithmName thay vì DigestAlgorithms trong .NET 6
            this.hashAlgorithm = HashAlgorithmName.FromOid(DigestAlgorithms.GetAllowedDigests(hashAlgorithm));

            // Sử dụng RSA hoặc DSA dựa trên loại khóa riêng
            if (certificate.GetRSAPrivateKey() is not null)
            {
                encryptionAlgorithm = "RSA";
            }
            else if (certificate.GetDSAPrivateKey() is not null)
            {
                encryptionAlgorithm = "DSA";
            }
            else
            {
                throw new ArgumentException("Thuật toán mã hóa không xác định " + certificate.PrivateKey);
            }
        }
        private static RSACryptoServiceProvider UpgradeCsp(RSACryptoServiceProvider currentKey)
        {
            CspKeyContainerInfo cspKeyContainerInfo = currentKey.CspKeyContainerInfo;
            if (cspKeyContainerInfo.HardwareDevice)
            {
                return currentKey;
            }
            CspParameters cspParameters = new CspParameters(24)
            {
                KeyContainerName = cspKeyContainerInfo.KeyContainerName,
                KeyNumber = (int)cspKeyContainerInfo.KeyNumber,
                Flags = CspProviderFlags.UseExistingKey
            };
            if (cspKeyContainerInfo.MachineKeyStore)
            {
                cspParameters.Flags |= CspProviderFlags.UseMachineKeyStore;
            }
            if (cspKeyContainerInfo.ProviderType == 24)
            {
                cspParameters.ProviderName = cspKeyContainerInfo.ProviderName;
            }
            return new RSACryptoServiceProvider(cspParameters);
        }

        //public virtual byte[] Sign(byte[] message)
        //{
        //    this.message = message;
        //    if (certificate.PrivateKey is RSACryptoServiceProvider)
        //    {
        //        return UpgradeCsp((RSACryptoServiceProvider)certificate.PrivateKey).SignData(message, hashAlgorithm);
        //    }
        //    return ((DSACryptoServiceProvider)certificate.PrivateKey).SignData(message);
        //}
        public virtual byte[] Sign(byte[] message)
        {
            this.message = message;

            if (certificate.GetRSAPrivateKey() is RSA rsa)
            {
                return rsa.SignData(message, hashAlgorithm, RSASignaturePadding.Pkcs1);
            }
            else if (certificate.GetDSAPrivateKey() is DSA dsa)
            {
                return dsa.SignData(message, hashAlgorithm);
            }
            else
            {
                throw new NotSupportedException("Chỉ hỗ trợ khóa RSA hoặc DSA.");
            }
        }

        public virtual byte[] SignHash(byte[] message)
        {
            this.message = message;

            using var rsa = certificate.GetRSAPrivateKey();

            if (rsa == null)
            {
                throw new NotSupportedException("Chỉ hỗ trợ khóa RSA.");
            }

            return rsa.SignHash(message, hashAlgorithm, RSASignaturePadding.Pkcs1);
        }

        //public virtual byte[] SignHash(byte[] message)
        //{
        //    this.message = message;
        //    if (!(certificate.PrivateKey is RSACryptoServiceProvider))
        //    {
        //        throw new NotSupportedException("CryptoServiceProvider is not supported");
        //    }
        //    return UpgradeCsp((RSACryptoServiceProvider)certificate.PrivateKey).SignHash(message, DigestAlgorithms.GetAllowedDigests(hashAlgorithm));
        //}
        public virtual HashAlgorithmName GetHashAlgorithm()
        {
            return hashAlgorithm;
        }

        //public virtual string GetHashAlgorithm()
        //{
        //    return hashAlgorithm;
        //}

        public virtual string GetEncryptionAlgorithm()
        {
            return encryptionAlgorithm;
        }

        public byte[] GetDataToBeSigned()
        {
            return message;
        }

        string IExternalSignature.GetHashAlgorithm()
        {
            return hashAlgorithm.Name;
        }
    }
}
