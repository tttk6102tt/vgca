﻿using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.CryptoPro;
using Sign.Org.BouncyCastle.Asn1.Kisa;
using Sign.Org.BouncyCastle.Asn1.Misc;
using Sign.Org.BouncyCastle.Asn1.Nist;
using Sign.Org.BouncyCastle.Asn1.Ntt;
using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Oiw;
using Sign.Org.BouncyCastle.Utilities;
using System.Collections;

namespace Sign.Org.BouncyCastle.Security
{
    public sealed class ParameterUtilities
    {
        private static readonly IDictionary algorithms;

        private static readonly IDictionary basicIVSizes;

        private ParameterUtilities()
        {
        }

        static ParameterUtilities()
        {
            algorithms = Platform.CreateHashtable();
            basicIVSizes = Platform.CreateHashtable();
            AddAlgorithm("AES", "AESWRAP");
            AddAlgorithm("AES128", "2.16.840.1.101.3.4.2", NistObjectIdentifiers.IdAes128Cbc, NistObjectIdentifiers.IdAes128Cfb, NistObjectIdentifiers.IdAes128Ecb, NistObjectIdentifiers.IdAes128Ofb, NistObjectIdentifiers.IdAes128Wrap);
            AddAlgorithm("AES192", "2.16.840.1.101.3.4.22", NistObjectIdentifiers.IdAes192Cbc, NistObjectIdentifiers.IdAes192Cfb, NistObjectIdentifiers.IdAes192Ecb, NistObjectIdentifiers.IdAes192Ofb, NistObjectIdentifiers.IdAes192Wrap);
            AddAlgorithm("AES256", "2.16.840.1.101.3.4.42", NistObjectIdentifiers.IdAes256Cbc, NistObjectIdentifiers.IdAes256Cfb, NistObjectIdentifiers.IdAes256Ecb, NistObjectIdentifiers.IdAes256Ofb, NistObjectIdentifiers.IdAes256Wrap);
            AddAlgorithm("BLOWFISH", "1.3.6.1.4.1.3029.1.2");
            AddAlgorithm("CAMELLIA", "CAMELLIAWRAP");
            AddAlgorithm("CAMELLIA128", NttObjectIdentifiers.IdCamellia128Cbc, NttObjectIdentifiers.IdCamellia128Wrap);
            AddAlgorithm("CAMELLIA192", NttObjectIdentifiers.IdCamellia192Cbc, NttObjectIdentifiers.IdCamellia192Wrap);
            AddAlgorithm("CAMELLIA256", NttObjectIdentifiers.IdCamellia256Cbc, NttObjectIdentifiers.IdCamellia256Wrap);
            AddAlgorithm("CAST5", "1.2.840.113533.7.66.10");
            AddAlgorithm("CAST6");
            AddAlgorithm("DES", OiwObjectIdentifiers.DesCbc, OiwObjectIdentifiers.DesCfb, OiwObjectIdentifiers.DesEcb, OiwObjectIdentifiers.DesOfb);
            AddAlgorithm("DESEDE", "DESEDEWRAP", "TDEA", OiwObjectIdentifiers.DesEde, PkcsObjectIdentifiers.IdAlgCms3DesWrap);
            AddAlgorithm("DESEDE3", PkcsObjectIdentifiers.DesEde3Cbc);
            AddAlgorithm("GOST28147", "GOST", "GOST-28147", CryptoProObjectIdentifiers.GostR28147Cbc);
            AddAlgorithm("HC128");
            AddAlgorithm("HC256");
            AddAlgorithm("NOEKEON");
            AddAlgorithm("RC2", PkcsObjectIdentifiers.RC2Cbc, PkcsObjectIdentifiers.IdAlgCmsRC2Wrap);
            AddAlgorithm("RC4", "ARC4", "1.2.840.113549.3.4");
            AddAlgorithm("RC5", "RC5-32");
            AddAlgorithm("RC5-64");
            AddAlgorithm("RC6");
            AddAlgorithm("RIJNDAEL");
            AddAlgorithm("SALSA20");
            AddAlgorithm("SEED", KisaObjectIdentifiers.IdNpkiAppCmsSeedWrap, KisaObjectIdentifiers.IdSeedCbc);
            AddAlgorithm("SERPENT");
            AddAlgorithm("SKIPJACK");
            AddAlgorithm("TEA");
            AddAlgorithm("TWOFISH");
            AddAlgorithm("VMPC");
            AddAlgorithm("VMPC-KSA3");
            AddAlgorithm("XTEA");
            AddBasicIVSizeEntries(8, "BLOWFISH", "DES", "DESEDE", "DESEDE3");
            AddBasicIVSizeEntries(16, "AES", "AES128", "AES192", "AES256", "CAMELLIA", "CAMELLIA128", "CAMELLIA192", "CAMELLIA256", "NOEKEON", "SEED");
        }

        private static void AddAlgorithm(string canonicalName, params object[] aliases)
        {
            algorithms[canonicalName] = canonicalName;
            foreach (object obj in aliases)
            {
                algorithms[obj.ToString()] = canonicalName;
            }
        }

        private static void AddBasicIVSizeEntries(int size, params string[] algorithms)
        {
            foreach (string key in algorithms)
            {
                basicIVSizes.Add(key, size);
            }
        }

        public static string GetCanonicalAlgorithmName(string algorithm)
        {
            return (string)algorithms[Platform.ToUpperInvariant(algorithm)];
        }

        public static KeyParameter CreateKeyParameter(DerObjectIdentifier algOid, byte[] keyBytes)
        {
            return CreateKeyParameter(algOid.Id, keyBytes, 0, keyBytes.Length);
        }

        public static KeyParameter CreateKeyParameter(string algorithm, byte[] keyBytes)
        {
            return CreateKeyParameter(algorithm, keyBytes, 0, keyBytes.Length);
        }

        public static KeyParameter CreateKeyParameter(DerObjectIdentifier algOid, byte[] keyBytes, int offset, int length)
        {
            return CreateKeyParameter(algOid.Id, keyBytes, offset, length);
        }

        public static KeyParameter CreateKeyParameter(string algorithm, byte[] keyBytes, int offset, int length)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException("algorithm");
            }

            string canonicalAlgorithmName = GetCanonicalAlgorithmName(algorithm);
            if (canonicalAlgorithmName == null)
            {
                throw new SecurityUtilityException("Algorithm " + algorithm + " not recognised.");
            }

            switch (canonicalAlgorithmName)
            {
                case "DES":
                    return new DesParameters(keyBytes, offset, length);
                case "DESEDE":
                case "DESEDE3":
                    return new DesEdeParameters(keyBytes, offset, length);
                case "RC2":
                    return new RC2Parameters(keyBytes, offset, length);
                default:
                    return new KeyParameter(keyBytes, offset, length);
            }
        }

        public static ICipherParameters GetCipherParameters(DerObjectIdentifier algOid, ICipherParameters key, Asn1Object asn1Params)
        {
            return GetCipherParameters(algOid.Id, key, asn1Params);
        }

        public static ICipherParameters GetCipherParameters(string algorithm, ICipherParameters key, Asn1Object asn1Params)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException("algorithm");
            }

            string canonicalAlgorithmName = GetCanonicalAlgorithmName(algorithm);
            if (canonicalAlgorithmName == null)
            {
                throw new SecurityUtilityException("Algorithm " + algorithm + " not recognised.");
            }

            byte[] array = null;
            try
            {
                if (FindBasicIVSize(canonicalAlgorithmName) == -1)
                {
                    switch (canonicalAlgorithmName)
                    {
                        default:
                            goto end_IL_0030;
                        case "RIJNDAEL":
                        case "SKIPJACK":
                        case "TWOFISH":
                            break;
                        case "CAST5":
                            array = Cast5CbcParameters.GetInstance(asn1Params).GetIV();
                            goto end_IL_0030;
                        case "RC2":
                            array = RC2CbcParameter.GetInstance(asn1Params).GetIV();
                            goto end_IL_0030;
                    }
                }

                array = ((Asn1OctetString)asn1Params).GetOctets();
                end_IL_0030:;
            }
            catch (Exception innerException)
            {
                throw new ArgumentException("Could not process ASN.1 parameters", innerException);
            }

            if (array != null)
            {
                return new ParametersWithIV(key, array);
            }

            throw new SecurityUtilityException("Algorithm " + algorithm + " not recognised.");
        }

        public static Asn1Encodable GenerateParameters(DerObjectIdentifier algID, SecureRandom random)
        {
            return GenerateParameters(algID.Id, random);
        }

        public static Asn1Encodable GenerateParameters(string algorithm, SecureRandom random)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException("algorithm");
            }

            string canonicalAlgorithmName = GetCanonicalAlgorithmName(algorithm);
            if (canonicalAlgorithmName == null)
            {
                throw new SecurityUtilityException("Algorithm " + algorithm + " not recognised.");
            }

            int num = FindBasicIVSize(canonicalAlgorithmName);
            if (num != -1)
            {
                return CreateIVOctetString(random, num);
            }

            if (canonicalAlgorithmName == "CAST5")
            {
                return new Cast5CbcParameters(CreateIV(random, 8), 128);
            }

            if (canonicalAlgorithmName == "RC2")
            {
                return new RC2CbcParameter(CreateIV(random, 8));
            }

            throw new SecurityUtilityException("Algorithm " + algorithm + " not recognised.");
        }

        private static Asn1OctetString CreateIVOctetString(SecureRandom random, int ivLength)
        {
            return new DerOctetString(CreateIV(random, ivLength));
        }

        private static byte[] CreateIV(SecureRandom random, int ivLength)
        {
            byte[] array = new byte[ivLength];
            random.NextBytes(array);
            return array;
        }

        private static int FindBasicIVSize(string canonicalName)
        {
            if (!basicIVSizes.Contains(canonicalName))
            {
                return -1;
            }

            return (int)basicIVSizes[canonicalName];
        }
    }
}
