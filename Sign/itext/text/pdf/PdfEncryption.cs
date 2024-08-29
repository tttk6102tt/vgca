using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.pdf.crypto;
using Sign.itext.pdf.security;
using Sign.itext.text.exceptions;
using Sign.itext.text.pdf.crypto;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Security;
using Sign.Org.BouncyCastle.X509;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class PdfEncryption
    {
        public const int STANDARD_ENCRYPTION_40 = 2;

        public const int STANDARD_ENCRYPTION_128 = 3;

        public const int AES_128 = 4;

        public const int AES_256 = 5;

        private static byte[] pad = new byte[32]
        {
            40, 191, 78, 94, 78, 117, 138, 65, 100, 0,
            78, 86, 255, 250, 1, 8, 46, 46, 0, 182,
            208, 104, 62, 128, 47, 12, 169, 254, 100, 83,
            105, 122
        };

        private static readonly byte[] salt = new byte[4] { 115, 65, 108, 84 };

        internal static readonly byte[] metadataPad = new byte[4] { 255, 255, 255, 255 };

        internal byte[] key;

        internal int keySize;

        internal byte[] mkey = new byte[0];

        internal byte[] extra = new byte[5];

        internal IDigest md5;

        internal byte[] ownerKey = new byte[32];

        internal byte[] userKey = new byte[32];

        internal byte[] oeKey;

        internal byte[] ueKey;

        internal byte[] perms;

        protected PdfPublicKeySecurityHandler publicKeyHandler;

        internal long permissions;

        internal byte[] documentID;

        internal static long seq = DateTime.Now.Ticks + Environment.TickCount;

        private int revision;

        private ARCFOUREncryption rc4 = new ARCFOUREncryption();

        private int keyLength;

        private bool encryptMetadata;

        private bool embeddedFilesOnly;

        private int cryptoMode;

        private const int VALIDATION_SALT_OFFSET = 32;

        private const int KEY_SALT_OFFSET = 40;

        private const int SALT_LENGHT = 8;

        private const int OU_LENGHT = 48;

        public PdfEncryption()
        {
            md5 = DigestUtilities.GetDigest("MD5");
            publicKeyHandler = new PdfPublicKeySecurityHandler();
        }

        public PdfEncryption(PdfEncryption enc)
            : this()
        {
            if (enc.key != null)
            {
                key = (byte[])enc.key.Clone();
            }

            keySize = enc.keySize;
            mkey = (byte[])enc.mkey.Clone();
            ownerKey = (byte[])enc.ownerKey.Clone();
            userKey = (byte[])enc.userKey.Clone();
            permissions = enc.permissions;
            if (enc.documentID != null)
            {
                documentID = (byte[])enc.documentID.Clone();
            }

            revision = enc.revision;
            keyLength = enc.keyLength;
            encryptMetadata = enc.encryptMetadata;
            embeddedFilesOnly = enc.embeddedFilesOnly;
            publicKeyHandler = enc.publicKeyHandler;
        }

        public virtual void SetCryptoMode(int mode, int kl)
        {
            cryptoMode = mode;
            encryptMetadata = (mode & 8) != 8;
            embeddedFilesOnly = (mode & 0x18) == 24;
            mode &= 7;
            switch (mode)
            {
                case 0:
                    encryptMetadata = true;
                    embeddedFilesOnly = false;
                    keyLength = 40;
                    revision = 2;
                    break;
                case 1:
                    embeddedFilesOnly = false;
                    if (kl > 0)
                    {
                        keyLength = kl;
                    }
                    else
                    {
                        keyLength = 128;
                    }

                    revision = 3;
                    break;
                case 2:
                    keyLength = 128;
                    revision = 4;
                    break;
                case 3:
                    keyLength = 256;
                    keySize = 32;
                    revision = 5;
                    break;
                default:
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("no.valid.encryption.mode"));
            }
        }

        public virtual int GetCryptoMode()
        {
            return cryptoMode;
        }

        public virtual bool IsMetadataEncrypted()
        {
            return encryptMetadata;
        }

        public virtual long GetPermissions()
        {
            return permissions;
        }

        public virtual bool IsEmbeddedFilesOnly()
        {
            return embeddedFilesOnly;
        }

        private byte[] PadPassword(byte[] userPassword)
        {
            byte[] array = new byte[32];
            if (userPassword == null)
            {
                Array.Copy(pad, 0, array, 0, 32);
            }
            else
            {
                Array.Copy(userPassword, 0, array, 0, Math.Min(userPassword.Length, 32));
                if (userPassword.Length < 32)
                {
                    Array.Copy(pad, 0, array, userPassword.Length, 32 - userPassword.Length);
                }
            }

            return array;
        }

        private byte[] ComputeOwnerKey(byte[] userPad, byte[] ownerPad)
        {
            byte[] array = new byte[32];
            byte[] array2 = DigestAlgorithms.Digest("MD5", ownerPad);
            if (revision == 3 || revision == 4)
            {
                byte[] array3 = new byte[keyLength / 8];
                for (int i = 0; i < 50; i++)
                {
                    Array.Copy(DigestAlgorithms.Digest("MD5", array2, 0, array3.Length), 0, array2, 0, array3.Length);
                }

                Array.Copy(userPad, 0, array, 0, 32);
                for (int j = 0; j < 20; j++)
                {
                    for (int k = 0; k < array3.Length; k++)
                    {
                        array3[k] = (byte)(array2[k] ^ j);
                    }

                    rc4.PrepareARCFOURKey(array3);
                    rc4.EncryptARCFOUR(array);
                }
            }
            else
            {
                rc4.PrepareARCFOURKey(array2, 0, 5);
                rc4.EncryptARCFOUR(userPad, array);
            }

            return array;
        }

        private void SetupGlobalEncryptionKey(byte[] documentID, byte[] userPad, byte[] ownerKey, long permissions)
        {
            this.documentID = documentID;
            this.ownerKey = ownerKey;
            this.permissions = permissions;
            mkey = new byte[keyLength / 8];
            md5.Reset();
            md5.BlockUpdate(userPad, 0, userPad.Length);
            md5.BlockUpdate(ownerKey, 0, ownerKey.Length);
            byte[] input = new byte[4]
            {
                (byte)permissions,
                (byte)(permissions >> 8),
                (byte)(permissions >> 16),
                (byte)(permissions >> 24)
            };
            md5.BlockUpdate(input, 0, 4);
            if (documentID != null)
            {
                md5.BlockUpdate(documentID, 0, documentID.Length);
            }

            if (!encryptMetadata)
            {
                md5.BlockUpdate(metadataPad, 0, metadataPad.Length);
            }

            byte[] array = new byte[md5.GetDigestSize()];
            md5.DoFinal(array, 0);
            byte[] array2 = new byte[mkey.Length];
            Array.Copy(array, 0, array2, 0, mkey.Length);
            md5.Reset();
            if (revision == 3 || revision == 4)
            {
                for (int i = 0; i < 50; i++)
                {
                    Array.Copy(DigestAlgorithms.Digest("MD5", array2), 0, array2, 0, mkey.Length);
                }
            }

            Array.Copy(array2, 0, mkey, 0, mkey.Length);
        }

        private void SetupUserKey()
        {
            if (revision == 3 || revision == 4)
            {
                md5.BlockUpdate(pad, 0, pad.Length);
                md5.BlockUpdate(documentID, 0, documentID.Length);
                byte[] array = new byte[md5.GetDigestSize()];
                md5.DoFinal(array, 0);
                md5.Reset();
                Array.Copy(array, 0, userKey, 0, 16);
                for (int i = 16; i < 32; i++)
                {
                    userKey[i] = 0;
                }

                for (int j = 0; j < 20; j++)
                {
                    for (int k = 0; k < mkey.Length; k++)
                    {
                        array[k] = (byte)(mkey[k] ^ j);
                    }

                    rc4.PrepareARCFOURKey(array, 0, mkey.Length);
                    rc4.EncryptARCFOUR(userKey, 0, 16);
                }
            }
            else
            {
                rc4.PrepareARCFOURKey(mkey);
                rc4.EncryptARCFOUR(pad, userKey);
            }
        }

        public virtual void SetupAllKeys(byte[] userPassword, byte[] ownerPassword, int permissions)
        {
            if (ownerPassword == null || ownerPassword.Length == 0)
            {
                ownerPassword = DigestAlgorithms.Digest("MD5", CreateDocumentId());
            }

            md5.Reset();
            permissions |= ((revision == 3 || revision == 4 || revision == 5) ? (-3904) : (-64));
            permissions &= -4;
            this.permissions = permissions;
            if (revision == 5)
            {
                if (userPassword == null)
                {
                    userPassword = new byte[0];
                }

                documentID = CreateDocumentId();
                byte[] iV = IVGenerator.GetIV(8);
                byte[] iV2 = IVGenerator.GetIV(8);
                key = IVGenerator.GetIV(32);
                IDigest digest = DigestUtilities.GetDigest("SHA-256");
                digest.BlockUpdate(userPassword, 0, Math.Min(userPassword.Length, 127));
                digest.BlockUpdate(iV, 0, iV.Length);
                userKey = new byte[48];
                digest.DoFinal(userKey, 0);
                Array.Copy(iV, 0, userKey, 32, 8);
                Array.Copy(iV2, 0, userKey, 40, 8);
                digest.BlockUpdate(userPassword, 0, Math.Min(userPassword.Length, 127));
                digest.BlockUpdate(iV2, 0, iV2.Length);
                byte[] output = new byte[32];
                digest.DoFinal(output, 0);
                AESCipherCBCnoPad aESCipherCBCnoPad = new AESCipherCBCnoPad(forEncryption: true, output);
                ueKey = aESCipherCBCnoPad.ProcessBlock(key, 0, key.Length);
                byte[] iV3 = IVGenerator.GetIV(8);
                byte[] iV4 = IVGenerator.GetIV(8);
                digest.BlockUpdate(ownerPassword, 0, Math.Min(ownerPassword.Length, 127));
                digest.BlockUpdate(iV3, 0, iV3.Length);
                digest.BlockUpdate(userKey, 0, userKey.Length);
                ownerKey = new byte[48];
                digest.DoFinal(ownerKey, 0);
                Array.Copy(iV3, 0, ownerKey, 32, 8);
                Array.Copy(iV4, 0, ownerKey, 40, 8);
                digest.BlockUpdate(ownerPassword, 0, Math.Min(ownerPassword.Length, 127));
                digest.BlockUpdate(iV4, 0, iV4.Length);
                digest.BlockUpdate(userKey, 0, userKey.Length);
                digest.DoFinal(output, 0);
                aESCipherCBCnoPad = new AESCipherCBCnoPad(forEncryption: true, output);
                oeKey = aESCipherCBCnoPad.ProcessBlock(key, 0, key.Length);
                byte[] iV5 = IVGenerator.GetIV(16);
                iV5[0] = (byte)permissions;
                iV5[1] = (byte)(permissions >> 8);
                iV5[2] = (byte)(permissions >> 16);
                iV5[3] = (byte)(permissions >> 24);
                iV5[4] = byte.MaxValue;
                iV5[5] = byte.MaxValue;
                iV5[6] = byte.MaxValue;
                iV5[7] = byte.MaxValue;
                iV5[8] = (byte)(encryptMetadata ? 84 : 70);
                iV5[9] = 97;
                iV5[10] = 100;
                iV5[11] = 98;
                aESCipherCBCnoPad = new AESCipherCBCnoPad(forEncryption: true, key);
                perms = aESCipherCBCnoPad.ProcessBlock(iV5, 0, iV5.Length);
            }
            else
            {
                byte[] userPad = PadPassword(userPassword);
                byte[] ownerPad = PadPassword(ownerPassword);
                ownerKey = ComputeOwnerKey(userPad, ownerPad);
                documentID = CreateDocumentId();
                SetupByUserPad(documentID, userPad, ownerKey, permissions);
            }
        }

        public virtual bool ReadKey(PdfDictionary enc, byte[] password)
        {
            if (password == null)
            {
                password = new byte[0];
            }

            byte[] iSOBytes = DocWriter.GetISOBytes(enc.Get(PdfName.O).ToString());
            byte[] iSOBytes2 = DocWriter.GetISOBytes(enc.Get(PdfName.U).ToString());
            byte[] iSOBytes3 = DocWriter.GetISOBytes(enc.Get(PdfName.OE).ToString());
            byte[] iSOBytes4 = DocWriter.GetISOBytes(enc.Get(PdfName.UE).ToString());
            byte[] iSOBytes5 = DocWriter.GetISOBytes(enc.Get(PdfName.PERMS).ToString());
            IDigest digest = DigestUtilities.GetDigest("SHA-256");
            digest.BlockUpdate(password, 0, Math.Min(password.Length, 127));
            digest.BlockUpdate(iSOBytes, 32, 8);
            digest.BlockUpdate(iSOBytes2, 0, 48);
            byte[] array = DigestUtilities.DoFinal(digest);
            bool flag = CompareArray(array, iSOBytes, 32);
            AESCipherCBCnoPad aESCipherCBCnoPad;
            if (flag)
            {
                digest.BlockUpdate(password, 0, Math.Min(password.Length, 127));
                digest.BlockUpdate(iSOBytes, 40, 8);
                digest.BlockUpdate(iSOBytes2, 0, 48);
                digest.DoFinal(array, 0);
                aESCipherCBCnoPad = new AESCipherCBCnoPad(forEncryption: false, array);
                key = aESCipherCBCnoPad.ProcessBlock(iSOBytes3, 0, iSOBytes3.Length);
            }
            else
            {
                digest.BlockUpdate(password, 0, Math.Min(password.Length, 127));
                digest.BlockUpdate(iSOBytes2, 32, 8);
                digest.DoFinal(array, 0);
                if (!CompareArray(array, iSOBytes2, 32))
                {
                    throw new BadPasswordException(MessageLocalization.GetComposedMessage("bad.user.password"));
                }

                digest.BlockUpdate(password, 0, Math.Min(password.Length, 127));
                digest.BlockUpdate(iSOBytes2, 40, 8);
                digest.DoFinal(array, 0);
                aESCipherCBCnoPad = new AESCipherCBCnoPad(forEncryption: false, array);
                key = aESCipherCBCnoPad.ProcessBlock(iSOBytes4, 0, iSOBytes4.Length);
            }

            aESCipherCBCnoPad = new AESCipherCBCnoPad(forEncryption: false, key);
            byte[] array2 = aESCipherCBCnoPad.ProcessBlock(iSOBytes5, 0, iSOBytes5.Length);
            if (array2[9] != 97 || array2[10] != 100 || array2[11] != 98)
            {
                throw new BadPasswordException(MessageLocalization.GetComposedMessage("bad.user.password"));
            }

            permissions = (array2[0] & 0xFF) | ((array2[1] & 0xFF) << 8) | ((array2[2] & 0xFF) << 16) | ((array2[2] & 0xFF) << 24);
            encryptMetadata = array2[8] == 84;
            return flag;
        }

        private static bool CompareArray(byte[] a, byte[] b, int len)
        {
            for (int i = 0; i < len; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static byte[] CreateDocumentId()
        {
            long num = DateTime.Now.Ticks + Environment.TickCount;
            long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
            string s = num + "+" + totalMemory + "+" + seq++;
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            return DigestAlgorithms.Digest("MD5", bytes);
        }

        public virtual void SetupByUserPassword(byte[] documentID, byte[] userPassword, byte[] ownerKey, long permissions)
        {
            SetupByUserPad(documentID, PadPassword(userPassword), ownerKey, permissions);
        }

        private void SetupByUserPad(byte[] documentID, byte[] userPad, byte[] ownerKey, long permissions)
        {
            SetupGlobalEncryptionKey(documentID, userPad, ownerKey, permissions);
            SetupUserKey();
        }

        public virtual void SetupByOwnerPassword(byte[] documentID, byte[] ownerPassword, byte[] userKey, byte[] ownerKey, long permissions)
        {
            SetupByOwnerPad(documentID, PadPassword(ownerPassword), userKey, ownerKey, permissions);
        }

        private void SetupByOwnerPad(byte[] documentID, byte[] ownerPad, byte[] userKey, byte[] ownerKey, long permissions)
        {
            byte[] userPad = ComputeOwnerKey(ownerKey, ownerPad);
            SetupGlobalEncryptionKey(documentID, userPad, ownerKey, permissions);
            SetupUserKey();
        }

        public virtual void SetKey(byte[] key)
        {
            this.key = key;
        }

        public virtual void SetupByEncryptionKey(byte[] key, int keylength)
        {
            mkey = new byte[keylength / 8];
            Array.Copy(key, 0, mkey, 0, mkey.Length);
        }

        public virtual void SetHashKey(int number, int generation)
        {
            if (revision != 5)
            {
                md5.Reset();
                extra[0] = (byte)number;
                extra[1] = (byte)(number >> 8);
                extra[2] = (byte)(number >> 16);
                extra[3] = (byte)generation;
                extra[4] = (byte)(generation >> 8);
                md5.BlockUpdate(mkey, 0, mkey.Length);
                md5.BlockUpdate(extra, 0, extra.Length);
                if (revision == 4)
                {
                    md5.BlockUpdate(salt, 0, salt.Length);
                }

                key = new byte[md5.GetDigestSize()];
                md5.DoFinal(key, 0);
                md5.Reset();
                keySize = mkey.Length + 5;
                if (keySize > 16)
                {
                    keySize = 16;
                }
            }
        }

        public static PdfObject CreateInfoId(byte[] id, bool modified)
        {
            ByteBuffer byteBuffer = new ByteBuffer(90);
            byteBuffer.Append('[').Append('<');
            if (id.Length != 16)
            {
                id = CreateDocumentId();
            }

            for (int i = 0; i < 16; i++)
            {
                byteBuffer.AppendHex(id[i]);
            }

            byteBuffer.Append('>').Append('<');
            if (modified)
            {
                id = CreateDocumentId();
            }

            for (int j = 0; j < 16; j++)
            {
                byteBuffer.AppendHex(id[j]);
            }

            byteBuffer.Append('>').Append(']');
            byteBuffer.Close();
            return new PdfLiteral(byteBuffer.ToByteArray());
        }

        public virtual PdfDictionary GetEncryptionDictionary()
        {
            PdfDictionary pdfDictionary = new PdfDictionary();
            if (publicKeyHandler.GetRecipientsSize() > 0)
            {
                PdfArray pdfArray = null;
                pdfDictionary.Put(PdfName.FILTER, PdfName.PUBSEC);
                pdfDictionary.Put(PdfName.R, new PdfNumber(revision));
                pdfArray = publicKeyHandler.GetEncodedRecipients();
                if (revision == 2)
                {
                    pdfDictionary.Put(PdfName.V, new PdfNumber(1));
                    pdfDictionary.Put(PdfName.SUBFILTER, PdfName.ADBE_PKCS7_S4);
                    pdfDictionary.Put(PdfName.RECIPIENTS, pdfArray);
                }
                else if (revision == 3 && encryptMetadata)
                {
                    pdfDictionary.Put(PdfName.V, new PdfNumber(2));
                    pdfDictionary.Put(PdfName.LENGTH, new PdfNumber(128));
                    pdfDictionary.Put(PdfName.SUBFILTER, PdfName.ADBE_PKCS7_S4);
                    pdfDictionary.Put(PdfName.RECIPIENTS, pdfArray);
                }
                else
                {
                    if (revision == 5)
                    {
                        pdfDictionary.Put(PdfName.R, new PdfNumber(5));
                        pdfDictionary.Put(PdfName.V, new PdfNumber(5));
                    }
                    else
                    {
                        pdfDictionary.Put(PdfName.R, new PdfNumber(4));
                        pdfDictionary.Put(PdfName.V, new PdfNumber(4));
                    }

                    pdfDictionary.Put(PdfName.SUBFILTER, PdfName.ADBE_PKCS7_S5);
                    PdfDictionary pdfDictionary2 = new PdfDictionary();
                    pdfDictionary2.Put(PdfName.RECIPIENTS, pdfArray);
                    if (!encryptMetadata)
                    {
                        pdfDictionary2.Put(PdfName.ENCRYPTMETADATA, PdfBoolean.PDFFALSE);
                    }

                    if (revision == 4)
                    {
                        pdfDictionary2.Put(PdfName.CFM, PdfName.AESV2);
                        pdfDictionary2.Put(PdfName.LENGTH, new PdfNumber(128));
                    }
                    else if (revision == 5)
                    {
                        pdfDictionary2.Put(PdfName.CFM, PdfName.AESV3);
                        pdfDictionary2.Put(PdfName.LENGTH, new PdfNumber(256));
                    }
                    else
                    {
                        pdfDictionary2.Put(PdfName.CFM, PdfName.V2);
                    }

                    PdfDictionary pdfDictionary3 = new PdfDictionary();
                    pdfDictionary3.Put(PdfName.DEFAULTCRYPTFILTER, pdfDictionary2);
                    pdfDictionary.Put(PdfName.CF, pdfDictionary3);
                    if (embeddedFilesOnly)
                    {
                        pdfDictionary.Put(PdfName.EFF, PdfName.DEFAULTCRYPTFILTER);
                        pdfDictionary.Put(PdfName.STRF, PdfName.IDENTITY);
                        pdfDictionary.Put(PdfName.STMF, PdfName.IDENTITY);
                    }
                    else
                    {
                        pdfDictionary.Put(PdfName.STRF, PdfName.DEFAULTCRYPTFILTER);
                        pdfDictionary.Put(PdfName.STMF, PdfName.DEFAULTCRYPTFILTER);
                    }
                }

                IDigest digest = ((revision != 5) ? DigestUtilities.GetDigest("SHA-1") : DigestUtilities.GetDigest("SHA-256"));
                byte[] array = null;
                byte[] seed = publicKeyHandler.GetSeed();
                digest.BlockUpdate(seed, 0, seed.Length);
                for (int i = 0; i < publicKeyHandler.GetRecipientsSize(); i++)
                {
                    array = publicKeyHandler.GetEncodedRecipient(i);
                    digest.BlockUpdate(array, 0, array.Length);
                }

                if (!encryptMetadata)
                {
                    digest.BlockUpdate(metadataPad, 0, metadataPad.Length);
                }

                byte[] output = new byte[digest.GetDigestSize()];
                digest.DoFinal(output, 0);
                if (revision == 5)
                {
                    key = output;
                }
                else
                {
                    SetupByEncryptionKey(output, keyLength);
                }
            }
            else
            {
                pdfDictionary.Put(PdfName.FILTER, PdfName.STANDARD);
                pdfDictionary.Put(PdfName.O, new PdfLiteral(StringUtils.EscapeString(ownerKey)));
                pdfDictionary.Put(PdfName.U, new PdfLiteral(StringUtils.EscapeString(userKey)));
                pdfDictionary.Put(PdfName.P, new PdfNumber(permissions));
                pdfDictionary.Put(PdfName.R, new PdfNumber(revision));
                if (revision == 2)
                {
                    pdfDictionary.Put(PdfName.V, new PdfNumber(1));
                }
                else if (revision == 3 && encryptMetadata)
                {
                    pdfDictionary.Put(PdfName.V, new PdfNumber(2));
                    pdfDictionary.Put(PdfName.LENGTH, new PdfNumber(128));
                }
                else if (revision == 5)
                {
                    if (!encryptMetadata)
                    {
                        pdfDictionary.Put(PdfName.ENCRYPTMETADATA, PdfBoolean.PDFFALSE);
                    }

                    pdfDictionary.Put(PdfName.OE, new PdfLiteral(StringUtils.EscapeString(oeKey)));
                    pdfDictionary.Put(PdfName.UE, new PdfLiteral(StringUtils.EscapeString(ueKey)));
                    pdfDictionary.Put(PdfName.PERMS, new PdfLiteral(StringUtils.EscapeString(perms)));
                    pdfDictionary.Put(PdfName.V, new PdfNumber(revision));
                    pdfDictionary.Put(PdfName.LENGTH, new PdfNumber(256));
                    PdfDictionary pdfDictionary4 = new PdfDictionary();
                    pdfDictionary4.Put(PdfName.LENGTH, new PdfNumber(32));
                    if (embeddedFilesOnly)
                    {
                        pdfDictionary4.Put(PdfName.AUTHEVENT, PdfName.EFOPEN);
                        pdfDictionary.Put(PdfName.EFF, PdfName.STDCF);
                        pdfDictionary.Put(PdfName.STRF, PdfName.IDENTITY);
                        pdfDictionary.Put(PdfName.STMF, PdfName.IDENTITY);
                    }
                    else
                    {
                        pdfDictionary4.Put(PdfName.AUTHEVENT, PdfName.DOCOPEN);
                        pdfDictionary.Put(PdfName.STRF, PdfName.STDCF);
                        pdfDictionary.Put(PdfName.STMF, PdfName.STDCF);
                    }

                    pdfDictionary4.Put(PdfName.CFM, PdfName.AESV3);
                    PdfDictionary pdfDictionary5 = new PdfDictionary();
                    pdfDictionary5.Put(PdfName.STDCF, pdfDictionary4);
                    pdfDictionary.Put(PdfName.CF, pdfDictionary5);
                }
                else
                {
                    if (!encryptMetadata)
                    {
                        pdfDictionary.Put(PdfName.ENCRYPTMETADATA, PdfBoolean.PDFFALSE);
                    }

                    pdfDictionary.Put(PdfName.R, new PdfNumber(4));
                    pdfDictionary.Put(PdfName.V, new PdfNumber(4));
                    pdfDictionary.Put(PdfName.LENGTH, new PdfNumber(128));
                    PdfDictionary pdfDictionary6 = new PdfDictionary();
                    pdfDictionary6.Put(PdfName.LENGTH, new PdfNumber(16));
                    if (embeddedFilesOnly)
                    {
                        pdfDictionary6.Put(PdfName.AUTHEVENT, PdfName.EFOPEN);
                        pdfDictionary.Put(PdfName.EFF, PdfName.STDCF);
                        pdfDictionary.Put(PdfName.STRF, PdfName.IDENTITY);
                        pdfDictionary.Put(PdfName.STMF, PdfName.IDENTITY);
                    }
                    else
                    {
                        pdfDictionary6.Put(PdfName.AUTHEVENT, PdfName.DOCOPEN);
                        pdfDictionary.Put(PdfName.STRF, PdfName.STDCF);
                        pdfDictionary.Put(PdfName.STMF, PdfName.STDCF);
                    }

                    if (revision == 4)
                    {
                        pdfDictionary6.Put(PdfName.CFM, PdfName.AESV2);
                    }
                    else
                    {
                        pdfDictionary6.Put(PdfName.CFM, PdfName.V2);
                    }

                    PdfDictionary pdfDictionary7 = new PdfDictionary();
                    pdfDictionary7.Put(PdfName.STDCF, pdfDictionary6);
                    pdfDictionary.Put(PdfName.CF, pdfDictionary7);
                }
            }

            return pdfDictionary;
        }

        public virtual PdfObject GetFileID(bool modified)
        {
            return CreateInfoId(documentID, modified);
        }

        public virtual OutputStreamEncryption GetEncryptionStream(Stream os)
        {
            return new OutputStreamEncryption(os, key, 0, keySize, revision);
        }

        public virtual int CalculateStreamSize(int n)
        {
            if (revision == 4 || revision == 5)
            {
                return (n & 0x7FFFFFF0) + 32;
            }

            return n;
        }

        public virtual byte[] EncryptByteArray(byte[] b)
        {
            MemoryStream memoryStream = new MemoryStream();
            OutputStreamEncryption encryptionStream = GetEncryptionStream(memoryStream);
            encryptionStream.Write(b, 0, b.Length);
            encryptionStream.Finish();
            return memoryStream.ToArray();
        }

        public virtual StandardDecryption GetDecryptor()
        {
            return new StandardDecryption(key, 0, keySize, revision);
        }

        public virtual byte[] DecryptByteArray(byte[] b)
        {
            MemoryStream memoryStream = new MemoryStream();
            StandardDecryption decryptor = GetDecryptor();
            byte[] array = decryptor.Update(b, 0, b.Length);
            if (array != null)
            {
                memoryStream.Write(array, 0, array.Length);
            }

            array = decryptor.Finish();
            if (array != null)
            {
                memoryStream.Write(array, 0, array.Length);
            }

            return memoryStream.ToArray();
        }

        public virtual void AddRecipient(X509Certificate cert, int permission)
        {
            documentID = CreateDocumentId();
            publicKeyHandler.AddRecipient(new PdfPublicKeyRecipient(cert, permission));
        }

        public virtual byte[] ComputeUserPassword(byte[] ownerPassword)
        {
            byte[] array = ComputeOwnerKey(ownerKey, PadPassword(ownerPassword));
            for (int i = 0; i < array.Length; i++)
            {
                bool flag = true;
                for (int j = 0; j < array.Length - i; j++)
                {
                    if (array[i + j] != pad[j])
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    byte[] array2 = new byte[i];
                    Array.Copy(array, 0, array2, 0, i);
                    return array2;
                }
            }

            return array;
        }
    }
}
