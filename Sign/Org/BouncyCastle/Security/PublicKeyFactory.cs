using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.CryptoPro;
using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Asn1.X9;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Generators;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Math.EC;
using Sign.Org.BouncyCastle.Oiw;

namespace Sign.Org.BouncyCastle.Security
{
    public sealed class PublicKeyFactory
    {
        private PublicKeyFactory()
        {
        }

        public static AsymmetricKeyParameter CreateKey(byte[] keyInfoData)
        {
            return CreateKey(SubjectPublicKeyInfo.GetInstance(Asn1Object.FromByteArray(keyInfoData)));
        }

        public static AsymmetricKeyParameter CreateKey(Stream inStr)
        {
            return CreateKey(SubjectPublicKeyInfo.GetInstance(Asn1Object.FromStream(inStr)));
        }

        public static AsymmetricKeyParameter CreateKey(SubjectPublicKeyInfo keyInfo)
        {
            AlgorithmIdentifier algorithmID = keyInfo.AlgorithmID;
            DerObjectIdentifier objectID = algorithmID.ObjectID;
            if (objectID.Equals(PkcsObjectIdentifiers.RsaEncryption) || objectID.Equals(X509ObjectIdentifiers.IdEARsa) || objectID.Equals(PkcsObjectIdentifiers.IdRsassaPss) || objectID.Equals(PkcsObjectIdentifiers.IdRsaesOaep))
            {
                RsaPublicKeyStructure instance = RsaPublicKeyStructure.GetInstance(keyInfo.GetPublicKey());
                return new RsaKeyParameters(isPrivate: false, instance.Modulus, instance.PublicExponent);
            }

            if (objectID.Equals(X9ObjectIdentifiers.DHPublicNumber))
            {
                Asn1Sequence instance2 = Asn1Sequence.GetInstance(algorithmID.Parameters.ToAsn1Object());
                BigInteger value = DHPublicKey.GetInstance(keyInfo.GetPublicKey()).Y.Value;
                if (IsPkcsDHParam(instance2))
                {
                    return ReadPkcsDHParam(objectID, value, instance2);
                }

                DHDomainParameters instance3 = DHDomainParameters.GetInstance(instance2);
                BigInteger value2 = instance3.P.Value;
                BigInteger value3 = instance3.G.Value;
                BigInteger value4 = instance3.Q.Value;
                BigInteger j = null;
                if (instance3.J != null)
                {
                    j = instance3.J.Value;
                }

                DHValidationParameters validation = null;
                DHValidationParms validationParms = instance3.ValidationParms;
                if (validationParms != null)
                {
                    byte[] bytes = validationParms.Seed.GetBytes();
                    BigInteger value5 = validationParms.PgenCounter.Value;
                    validation = new DHValidationParameters(bytes, value5.IntValue);
                }

                return new DHPublicKeyParameters(value, new DHParameters(value2, value3, value4, j, validation));
            }

            if (objectID.Equals(PkcsObjectIdentifiers.DhKeyAgreement))
            {
                Asn1Sequence instance4 = Asn1Sequence.GetInstance(algorithmID.Parameters.ToAsn1Object());
                DerInteger derInteger = (DerInteger)keyInfo.GetPublicKey();
                return ReadPkcsDHParam(objectID, derInteger.Value, instance4);
            }

            if (objectID.Equals(OiwObjectIdentifiers.ElGamalAlgorithm))
            {
                ElGamalParameter elGamalParameter = new ElGamalParameter(Asn1Sequence.GetInstance(algorithmID.Parameters.ToAsn1Object()));
                return new ElGamalPublicKeyParameters(((DerInteger)keyInfo.GetPublicKey()).Value, new ElGamalParameters(elGamalParameter.P, elGamalParameter.G));
            }

            if (objectID.Equals(X9ObjectIdentifiers.IdDsa) || objectID.Equals(OiwObjectIdentifiers.DsaWithSha1))
            {
                DerInteger obj = (DerInteger)keyInfo.GetPublicKey();
                Asn1Encodable parameters = algorithmID.Parameters;
                DsaParameters parameters2 = null;
                if (parameters != null)
                {
                    DsaParameter instance5 = DsaParameter.GetInstance(parameters.ToAsn1Object());
                    parameters2 = new DsaParameters(instance5.P, instance5.Q, instance5.G);
                }

                return new DsaPublicKeyParameters(obj.Value, parameters2);
            }

            if (objectID.Equals(X9ObjectIdentifiers.IdECPublicKey))
            {
                X962Parameters x962Parameters = new X962Parameters(algorithmID.Parameters.ToAsn1Object());
                X9ECParameters x9ECParameters = ((!x962Parameters.IsNamedCurve) ? new X9ECParameters((Asn1Sequence)x962Parameters.Parameters) : ECKeyPairGenerator.FindECCurveByOid((DerObjectIdentifier)x962Parameters.Parameters));
                Asn1OctetString s = new DerOctetString(keyInfo.PublicKeyData.GetBytes());
                ECPoint point = new X9ECPoint(x9ECParameters.Curve, s).Point;
                if (x962Parameters.IsNamedCurve)
                {
                    return new ECPublicKeyParameters("EC", point, (DerObjectIdentifier)x962Parameters.Parameters);
                }

                ECDomainParameters parameters3 = new ECDomainParameters(x9ECParameters.Curve, x9ECParameters.G, x9ECParameters.N, x9ECParameters.H, x9ECParameters.GetSeed());
                return new ECPublicKeyParameters(point, parameters3);
            }

            if (objectID.Equals(CryptoProObjectIdentifiers.GostR3410x2001))
            {
                Gost3410PublicKeyAlgParameters gost3410PublicKeyAlgParameters = new Gost3410PublicKeyAlgParameters((Asn1Sequence)algorithmID.Parameters);
                Asn1OctetString asn1OctetString;
                try
                {
                    asn1OctetString = (Asn1OctetString)keyInfo.GetPublicKey();
                }
                catch (IOException)
                {
                    throw new ArgumentException("invalid info structure in GOST3410 public key");
                }

                byte[] octets = asn1OctetString.GetOctets();
                byte[] array = new byte[32];
                byte[] array2 = new byte[32];
                for (int i = 0; i != array2.Length; i++)
                {
                    array[i] = octets[31 - i];
                }

                for (int k = 0; k != array.Length; k++)
                {
                    array2[k] = octets[63 - k];
                }

                ECDomainParameters byOid = ECGost3410NamedCurves.GetByOid(gost3410PublicKeyAlgParameters.PublicKeyParamSet);
                if (byOid == null)
                {
                    return null;
                }

                ECPoint q = byOid.Curve.CreatePoint(new BigInteger(1, array), new BigInteger(1, array2), withCompression: false);
                return new ECPublicKeyParameters("ECGOST3410", q, gost3410PublicKeyAlgParameters.PublicKeyParamSet);
            }

            if (objectID.Equals(CryptoProObjectIdentifiers.GostR3410x94))
            {
                Gost3410PublicKeyAlgParameters gost3410PublicKeyAlgParameters2 = new Gost3410PublicKeyAlgParameters((Asn1Sequence)algorithmID.Parameters);
                DerOctetString derOctetString;
                try
                {
                    derOctetString = (DerOctetString)keyInfo.GetPublicKey();
                }
                catch (IOException)
                {
                    throw new ArgumentException("invalid info structure in GOST3410 public key");
                }

                byte[] octets2 = derOctetString.GetOctets();
                byte[] array3 = new byte[octets2.Length];
                for (int l = 0; l != octets2.Length; l++)
                {
                    array3[l] = octets2[octets2.Length - 1 - l];
                }

                return new Gost3410PublicKeyParameters(new BigInteger(1, array3), gost3410PublicKeyAlgParameters2.PublicKeyParamSet);
            }

            throw new SecurityUtilityException("algorithm identifier in key not recognised: " + objectID);
        }

        private static bool IsPkcsDHParam(Asn1Sequence seq)
        {
            if (seq.Count == 2)
            {
                return true;
            }

            if (seq.Count > 3)
            {
                return false;
            }

            DerInteger instance = DerInteger.GetInstance(seq[2]);
            DerInteger instance2 = DerInteger.GetInstance(seq[0]);
            return instance.Value.CompareTo(BigInteger.ValueOf(instance2.Value.BitLength)) <= 0;
        }

        private static DHPublicKeyParameters ReadPkcsDHParam(DerObjectIdentifier algOid, BigInteger y, Asn1Sequence seq)
        {
            DHParameter dHParameter = new DHParameter(seq);
            int l = dHParameter.L?.IntValue ?? 0;
            DHParameters parameters = new DHParameters(dHParameter.P, dHParameter.G, null, l);
            return new DHPublicKeyParameters(y, parameters, algOid);
        }
    }
}
