﻿using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.Nist;
using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Asn1.TeleTrust;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Crypto.Encodings;
using Sign.Org.BouncyCastle.Crypto.Engines;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Security;
using Sign.Org.BouncyCastle.Utilities;
using System.Collections;

namespace Sign.Org.BouncyCastle.Crypto.Signers
{
    public class RsaDigestSigner : ISigner
    {
        private readonly IAsymmetricBlockCipher rsaEngine = new Pkcs1Encoding(new RsaBlindedEngine());

        private readonly AlgorithmIdentifier algId;

        private readonly IDigest digest;

        private bool forSigning;

        private static readonly IDictionary oidMap;

        public string AlgorithmName => digest.AlgorithmName + "withRSA";

        static RsaDigestSigner()
        {
            oidMap = Platform.CreateHashtable();
            oidMap["RIPEMD128"] = TeleTrusTObjectIdentifiers.RipeMD128;
            oidMap["RIPEMD160"] = TeleTrusTObjectIdentifiers.RipeMD160;
            oidMap["RIPEMD256"] = TeleTrusTObjectIdentifiers.RipeMD256;
            oidMap["SHA-1"] = X509ObjectIdentifiers.IdSha1;
            oidMap["SHA-224"] = NistObjectIdentifiers.IdSha224;
            oidMap["SHA-256"] = NistObjectIdentifiers.IdSha256;
            oidMap["SHA-384"] = NistObjectIdentifiers.IdSha384;
            oidMap["SHA-512"] = NistObjectIdentifiers.IdSha512;
            oidMap["MD2"] = PkcsObjectIdentifiers.MD2;
            oidMap["MD4"] = PkcsObjectIdentifiers.MD4;
            oidMap["MD5"] = PkcsObjectIdentifiers.MD5;
        }

        public RsaDigestSigner(IDigest digest)
        {
            this.digest = digest;
            if (digest.AlgorithmName.Equals("NULL"))
            {
                algId = null;
            }
            else
            {
                algId = new AlgorithmIdentifier((DerObjectIdentifier)oidMap[digest.AlgorithmName], DerNull.Instance);
            }
        }

        public void Init(bool forSigning, ICipherParameters parameters)
        {
            this.forSigning = forSigning;
            AsymmetricKeyParameter asymmetricKeyParameter = ((!(parameters is ParametersWithRandom)) ? ((AsymmetricKeyParameter)parameters) : ((AsymmetricKeyParameter)((ParametersWithRandom)parameters).Parameters));
            if (forSigning && !asymmetricKeyParameter.IsPrivate)
            {
                throw new InvalidKeyException("Signing requires private key.");
            }

            if (!forSigning && asymmetricKeyParameter.IsPrivate)
            {
                throw new InvalidKeyException("Verification requires public key.");
            }

            Reset();
            rsaEngine.Init(forSigning, parameters);
        }

        public void Update(byte input)
        {
            digest.Update(input);
        }

        public void BlockUpdate(byte[] input, int inOff, int length)
        {
            digest.BlockUpdate(input, inOff, length);
        }

        public byte[] GenerateSignature()
        {
            if (!forSigning)
            {
                throw new InvalidOperationException("RsaDigestSigner not initialised for signature generation.");
            }

            byte[] array = new byte[digest.GetDigestSize()];
            digest.DoFinal(array, 0);
            byte[] array2 = DerEncode(array);
            return rsaEngine.ProcessBlock(array2, 0, array2.Length);
        }

        public bool VerifySignature(byte[] signature)
        {
            if (forSigning)
            {
                throw new InvalidOperationException("RsaDigestSigner not initialised for verification");
            }

            byte[] array = new byte[digest.GetDigestSize()];
            digest.DoFinal(array, 0);
            byte[] array2;
            byte[] array3;
            try
            {
                array2 = rsaEngine.ProcessBlock(signature, 0, signature.Length);
                array3 = DerEncode(array);
            }
            catch (Exception)
            {
                return false;
            }

            if (array2.Length == array3.Length)
            {
                for (int i = 0; i < array2.Length; i++)
                {
                    if (array2[i] != array3[i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (array2.Length != array3.Length - 2)
                {
                    return false;
                }

                int num = array2.Length - array.Length - 2;
                int num2 = array3.Length - array.Length - 2;
                array3[1] -= 2;
                array3[3] -= 2;
                for (int j = 0; j < array.Length; j++)
                {
                    if (array2[num + j] != array3[num2 + j])
                    {
                        return false;
                    }
                }

                for (int k = 0; k < num; k++)
                {
                    if (array2[k] != array3[k])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void Reset()
        {
            digest.Reset();
        }

        private byte[] DerEncode(byte[] hash)
        {
            if (algId == null)
            {
                return hash;
            }

            return new DigestInfo(algId, hash).GetDerEncoded();
        }
    }
}
