﻿using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.Cms;
using Sign.Org.BouncyCastle.Asn1.Ecc;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Asn1.X9;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Pkcs;
using Sign.Org.BouncyCastle.Security;
using System.Collections;

namespace Sign.Org.BouncyCastle.Cms
{
    public class KeyAgreeRecipientInformation : RecipientInformation
    {
        private KeyAgreeRecipientInfo info;

        private Asn1OctetString encryptedKey;

        internal static void ReadRecipientInfo(IList infos, KeyAgreeRecipientInfo info, CmsSecureReadable secureReadable)
        {
            try
            {
                foreach (Asn1Encodable recipientEncryptedKey in info.RecipientEncryptedKeys)
                {
                    RecipientEncryptedKey instance = RecipientEncryptedKey.GetInstance(recipientEncryptedKey.ToAsn1Object());
                    RecipientID recipientID = new RecipientID();
                    KeyAgreeRecipientIdentifier identifier = instance.Identifier;
                    IssuerAndSerialNumber issuerAndSerialNumber = identifier.IssuerAndSerialNumber;
                    if (issuerAndSerialNumber != null)
                    {
                        recipientID.Issuer = issuerAndSerialNumber.Name;
                        recipientID.SerialNumber = issuerAndSerialNumber.SerialNumber.Value;
                    }
                    else
                    {
                        RecipientKeyIdentifier rKeyID = identifier.RKeyID;
                        recipientID.SubjectKeyIdentifier = rKeyID.SubjectKeyIdentifier.GetOctets();
                    }

                    infos.Add(new KeyAgreeRecipientInformation(info, recipientID, instance.EncryptedKey, secureReadable));
                }
            }
            catch (IOException innerException)
            {
                throw new ArgumentException("invalid rid in KeyAgreeRecipientInformation", innerException);
            }
        }

        internal KeyAgreeRecipientInformation(KeyAgreeRecipientInfo info, RecipientID rid, Asn1OctetString encryptedKey, CmsSecureReadable secureReadable)
            : base(info.KeyEncryptionAlgorithm, secureReadable)
        {
            this.info = info;
            base.rid = rid;
            this.encryptedKey = encryptedKey;
        }

        private AsymmetricKeyParameter GetSenderPublicKey(AsymmetricKeyParameter receiverPrivateKey, OriginatorIdentifierOrKey originator)
        {
            OriginatorPublicKey originatorPublicKey = originator.OriginatorPublicKey;
            if (originatorPublicKey != null)
            {
                return GetPublicKeyFromOriginatorPublicKey(receiverPrivateKey, originatorPublicKey);
            }

            OriginatorID originatorID = new OriginatorID();
            IssuerAndSerialNumber issuerAndSerialNumber = originator.IssuerAndSerialNumber;
            if (issuerAndSerialNumber != null)
            {
                originatorID.Issuer = issuerAndSerialNumber.Name;
                originatorID.SerialNumber = issuerAndSerialNumber.SerialNumber.Value;
            }
            else
            {
                SubjectKeyIdentifier subjectKeyIdentifier = originator.SubjectKeyIdentifier;
                originatorID.SubjectKeyIdentifier = subjectKeyIdentifier.GetKeyIdentifier();
            }

            return GetPublicKeyFromOriginatorID(originatorID);
        }

        private AsymmetricKeyParameter GetPublicKeyFromOriginatorPublicKey(AsymmetricKeyParameter receiverPrivateKey, OriginatorPublicKey originatorPublicKey)
        {
            return PublicKeyFactory.CreateKey(new SubjectPublicKeyInfo(PrivateKeyInfoFactory.CreatePrivateKeyInfo(receiverPrivateKey).AlgorithmID, originatorPublicKey.PublicKey.GetBytes()));
        }

        private AsymmetricKeyParameter GetPublicKeyFromOriginatorID(OriginatorID origID)
        {
            throw new CmsException("No support for 'originator' as IssuerAndSerialNumber or SubjectKeyIdentifier");
        }

        private KeyParameter CalculateAgreedWrapKey(string wrapAlg, AsymmetricKeyParameter senderPublicKey, AsymmetricKeyParameter receiverPrivateKey)
        {
            DerObjectIdentifier objectID = keyEncAlg.ObjectID;
            ICipherParameters cipherParameters = senderPublicKey;
            ICipherParameters cipherParameters2 = receiverPrivateKey;
            if (objectID.Id.Equals(CmsEnvelopedGenerator.ECMqvSha1Kdf))
            {
                MQVuserKeyingMaterial instance = MQVuserKeyingMaterial.GetInstance(Asn1Object.FromByteArray(info.UserKeyingMaterial.GetOctets()));
                cipherParameters = new MqvPublicParameters(ephemeralPublicKey: (ECPublicKeyParameters)GetPublicKeyFromOriginatorPublicKey(receiverPrivateKey, instance.EphemeralPublicKey), staticPublicKey: (ECPublicKeyParameters)cipherParameters);
                cipherParameters2 = new MqvPrivateParameters((ECPrivateKeyParameters)cipherParameters2, (ECPrivateKeyParameters)cipherParameters2);
            }

            IBasicAgreement basicAgreementWithKdf = AgreementUtilities.GetBasicAgreementWithKdf(objectID, wrapAlg);
            basicAgreementWithKdf.Init(cipherParameters2);
            BigInteger s = basicAgreementWithKdf.CalculateAgreement(cipherParameters);
            int qLength = GeneratorUtilities.GetDefaultKeySize(wrapAlg) / 8;
            byte[] keyBytes = X9IntegerConverter.IntegerToBytes(s, qLength);
            return ParameterUtilities.CreateKeyParameter(wrapAlg, keyBytes);
        }

        private KeyParameter UnwrapSessionKey(string wrapAlg, KeyParameter agreedKey)
        {
            byte[] octets = encryptedKey.GetOctets();
            IWrapper wrapper = WrapperUtilities.GetWrapper(wrapAlg);
            wrapper.Init(forWrapping: false, agreedKey);
            byte[] keyBytes = wrapper.Unwrap(octets, 0, octets.Length);
            return ParameterUtilities.CreateKeyParameter(GetContentAlgorithmName(), keyBytes);
        }

        internal KeyParameter GetSessionKey(AsymmetricKeyParameter receiverPrivateKey)
        {
            try
            {
                string id = DerObjectIdentifier.GetInstance(Asn1Sequence.GetInstance(keyEncAlg.Parameters)[0]).Id;
                AsymmetricKeyParameter senderPublicKey = GetSenderPublicKey(receiverPrivateKey, info.Originator);
                KeyParameter agreedKey = CalculateAgreedWrapKey(id, senderPublicKey, receiverPrivateKey);
                return UnwrapSessionKey(id, agreedKey);
            }
            catch (SecurityUtilityException e)
            {
                throw new CmsException("couldn't create cipher.", e);
            }
            catch (InvalidKeyException e2)
            {
                throw new CmsException("key invalid in message.", e2);
            }
            catch (Exception e3)
            {
                throw new CmsException("originator key invalid.", e3);
            }
        }

        public override CmsTypedStream GetContentStream(ICipherParameters key)
        {
            if (!(key is AsymmetricKeyParameter))
            {
                throw new ArgumentException("KeyAgreement requires asymmetric key", "key");
            }

            AsymmetricKeyParameter asymmetricKeyParameter = (AsymmetricKeyParameter)key;
            if (!asymmetricKeyParameter.IsPrivate)
            {
                throw new ArgumentException("Expected private key", "key");
            }

            KeyParameter sessionKey = GetSessionKey(asymmetricKeyParameter);
            return GetContentFromSessionKey(sessionKey);
        }
    }
}
