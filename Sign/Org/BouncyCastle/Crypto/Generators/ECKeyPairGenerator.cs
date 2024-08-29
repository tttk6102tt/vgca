using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.Nist;
using Sign.Org.BouncyCastle.Asn1.Sec;
using Sign.Org.BouncyCastle.Asn1.TeleTrust;
using Sign.Org.BouncyCastle.Asn1.X9;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Math.EC;
using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Crypto.Generators
{
    public class ECKeyPairGenerator : IAsymmetricCipherKeyPairGenerator
    {
        private readonly string algorithm;

        private ECDomainParameters parameters;

        private DerObjectIdentifier publicKeyParamSet;

        private SecureRandom random;

        public ECKeyPairGenerator()
            : this("EC")
        {
        }

        public ECKeyPairGenerator(string algorithm)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException("algorithm");
            }

            this.algorithm = ECKeyParameters.VerifyAlgorithmName(algorithm);
        }

        public void Init(KeyGenerationParameters parameters)
        {
            if (parameters is ECKeyGenerationParameters)
            {
                ECKeyGenerationParameters eCKeyGenerationParameters = (ECKeyGenerationParameters)parameters;
                publicKeyParamSet = eCKeyGenerationParameters.PublicKeyParamSet;
                this.parameters = eCKeyGenerationParameters.DomainParameters;
            }
            else
            {
                X9ECParameters x9ECParameters = FindECCurveByOid(parameters.Strength switch
                {
                    192 => X9ObjectIdentifiers.Prime192v1,
                    224 => SecObjectIdentifiers.SecP224r1,
                    239 => X9ObjectIdentifiers.Prime239v1,
                    256 => X9ObjectIdentifiers.Prime256v1,
                    384 => SecObjectIdentifiers.SecP384r1,
                    521 => SecObjectIdentifiers.SecP521r1,
                    _ => throw new InvalidParameterException("unknown key size."),
                });
                this.parameters = new ECDomainParameters(x9ECParameters.Curve, x9ECParameters.G, x9ECParameters.N, x9ECParameters.H, x9ECParameters.GetSeed());
            }

            random = parameters.Random;
        }

        public AsymmetricCipherKeyPair GenerateKeyPair()
        {
            BigInteger n = parameters.N;
            BigInteger bigInteger;
            do
            {
                bigInteger = new BigInteger(n.BitLength, random);
            }
            while (bigInteger.SignValue == 0 || bigInteger.CompareTo(n) >= 0);
            ECPoint q = parameters.G.Multiply(bigInteger);
            if (publicKeyParamSet != null)
            {
                return new AsymmetricCipherKeyPair(new ECPublicKeyParameters(algorithm, q, publicKeyParamSet), new ECPrivateKeyParameters(algorithm, bigInteger, publicKeyParamSet));
            }

            return new AsymmetricCipherKeyPair(new ECPublicKeyParameters(algorithm, q, parameters), new ECPrivateKeyParameters(algorithm, bigInteger, parameters));
        }

        internal static X9ECParameters FindECCurveByOid(DerObjectIdentifier oid)
        {
            X9ECParameters byOid = X962NamedCurves.GetByOid(oid);
            if (byOid == null)
            {
                byOid = SecNamedCurves.GetByOid(oid);
                if (byOid == null)
                {
                    byOid = NistNamedCurves.GetByOid(oid);
                    if (byOid == null)
                    {
                        byOid = TeleTrusTNamedCurves.GetByOid(oid);
                    }
                }
            }

            return byOid;
        }

        internal static ECPublicKeyParameters GetCorrespondingPublicKey(ECPrivateKeyParameters privKey)
        {
            ECDomainParameters eCDomainParameters = privKey.Parameters;
            ECPoint q = eCDomainParameters.G.Multiply(privKey.D);
            if (privKey.PublicKeyParamSet != null)
            {
                return new ECPublicKeyParameters(privKey.AlgorithmName, q, privKey.PublicKeyParamSet);
            }

            return new ECPublicKeyParameters(privKey.AlgorithmName, q, eCDomainParameters);
        }
    }
}
