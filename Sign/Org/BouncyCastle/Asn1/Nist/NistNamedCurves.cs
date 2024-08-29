using Sign.Org.BouncyCastle.Asn1.Sec;
using Sign.Org.BouncyCastle.Asn1.Utilities.Collections;
using Sign.Org.BouncyCastle.Asn1.X9;
using Sign.Org.BouncyCastle.Utilities;
using System.Collections;

namespace Sign.Org.BouncyCastle.Asn1.Nist
{
    public sealed class NistNamedCurves
    {
        private static readonly IDictionary objIds;

        private static readonly IDictionary names;

        public static IEnumerable Names => new EnumerableProxy(objIds.Keys);

        private NistNamedCurves()
        {
        }

        private static void DefineCurve(string name, DerObjectIdentifier oid)
        {
            objIds.Add(name, oid);
            names.Add(oid, name);
        }

        static NistNamedCurves()
        {
            objIds = Platform.CreateHashtable();
            names = Platform.CreateHashtable();
            DefineCurve("B-571", SecObjectIdentifiers.SecT571r1);
            DefineCurve("B-409", SecObjectIdentifiers.SecT409r1);
            DefineCurve("B-283", SecObjectIdentifiers.SecT283r1);
            DefineCurve("B-233", SecObjectIdentifiers.SecT233r1);
            DefineCurve("B-163", SecObjectIdentifiers.SecT163r2);
            DefineCurve("K-571", SecObjectIdentifiers.SecT571k1);
            DefineCurve("K-409", SecObjectIdentifiers.SecT409k1);
            DefineCurve("K-283", SecObjectIdentifiers.SecT283k1);
            DefineCurve("K-233", SecObjectIdentifiers.SecT233k1);
            DefineCurve("K-163", SecObjectIdentifiers.SecT163k1);
            DefineCurve("P-521", SecObjectIdentifiers.SecP521r1);
            DefineCurve("P-384", SecObjectIdentifiers.SecP384r1);
            DefineCurve("P-256", SecObjectIdentifiers.SecP256r1);
            DefineCurve("P-224", SecObjectIdentifiers.SecP224r1);
            DefineCurve("P-192", SecObjectIdentifiers.SecP192r1);
        }

        public static X9ECParameters GetByName(string name)
        {
            DerObjectIdentifier derObjectIdentifier = (DerObjectIdentifier)objIds[Platform.ToUpperInvariant(name)];
            if (derObjectIdentifier != null)
            {
                return GetByOid(derObjectIdentifier);
            }

            return null;
        }

        public static X9ECParameters GetByOid(DerObjectIdentifier oid)
        {
            return SecNamedCurves.GetByOid(oid);
        }

        public static DerObjectIdentifier GetOid(string name)
        {
            return (DerObjectIdentifier)objIds[Platform.ToUpperInvariant(name)];
        }

        public static string GetName(DerObjectIdentifier oid)
        {
            return (string)names[oid];
        }
    }
}
