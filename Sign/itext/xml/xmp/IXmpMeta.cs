using Sign.itext.xml.xmp.options;
using Sign.itext.xml.xmp.properties;

namespace Sign.itext.xml.xmp
{
    public interface IXmpMeta : ICloneable
    {
        string ObjectName { get; set; }

        string PacketHeader { get; }

        IXmpProperty GetProperty(string schemaNs, string propName);

        IXmpProperty GetArrayItem(string schemaNs, string arrayName, int itemIndex);

        int CountArrayItems(string schemaNs, string arrayName);

        IXmpProperty GetStructField(string schemaNs, string structName, string fieldNs, string fieldName);

        IXmpProperty GetQualifier(string schemaNs, string propName, string qualNs, string qualName);

        void SetProperty(string schemaNs, string propName, object propValue, PropertyOptions options);

        void SetProperty(string schemaNs, string propName, object propValue);

        void SetArrayItem(string schemaNs, string arrayName, int itemIndex, string itemValue, PropertyOptions options);

        void SetArrayItem(string schemaNs, string arrayName, int itemIndex, string itemValue);

        void InsertArrayItem(string schemaNs, string arrayName, int itemIndex, string itemValue, PropertyOptions options);

        void InsertArrayItem(string schemaNs, string arrayName, int itemIndex, string itemValue);

        void AppendArrayItem(string schemaNs, string arrayName, PropertyOptions arrayOptions, string itemValue, PropertyOptions itemOptions);

        void AppendArrayItem(string schemaNs, string arrayName, string itemValue);

        void SetStructField(string schemaNs, string structName, string fieldNs, string fieldName, string fieldValue, PropertyOptions options);

        void SetStructField(string schemaNs, string structName, string fieldNs, string fieldName, string fieldValue);

        void SetQualifier(string schemaNs, string propName, string qualNs, string qualName, string qualValue, PropertyOptions options);

        void SetQualifier(string schemaNs, string propName, string qualNs, string qualName, string qualValue);

        void DeleteProperty(string schemaNs, string propName);

        void DeleteArrayItem(string schemaNs, string arrayName, int itemIndex);

        void DeleteStructField(string schemaNs, string structName, string fieldNs, string fieldName);

        void DeleteQualifier(string schemaNs, string propName, string qualNs, string qualName);

        bool DoesPropertyExist(string schemaNs, string propName);

        bool DoesArrayItemExist(string schemaNs, string arrayName, int itemIndex);

        bool DoesStructFieldExist(string schemaNs, string structName, string fieldNs, string fieldName);

        bool DoesQualifierExist(string schemaNs, string propName, string qualNs, string qualName);

        IXmpProperty GetLocalizedText(string schemaNs, string altTextName, string genericLang, string specificLang);

        void SetLocalizedText(string schemaNs, string altTextName, string genericLang, string specificLang, string itemValue, PropertyOptions options);

        void SetLocalizedText(string schemaNs, string altTextName, string genericLang, string specificLang, string itemValue);

        bool? GetPropertyBoolean(string schemaNs, string propName);

        int? GetPropertyInteger(string schemaNs, string propName);

        long? GetPropertyLong(string schemaNs, string propName);

        double? GetPropertyDouble(string schemaNs, string propName);

        IXmpDateTime GetPropertyDate(string schemaNs, string propName);

        DateTime GetPropertyCalendar(string schemaNs, string propName);

        sbyte[] GetPropertyBase64(string schemaNs, string propName);

        string GetPropertyString(string schemaNs, string propName);

        void SetPropertyBoolean(string schemaNs, string propName, bool propValue, PropertyOptions options);

        void SetPropertyBoolean(string schemaNs, string propName, bool propValue);

        void SetPropertyInteger(string schemaNs, string propName, int propValue, PropertyOptions options);

        void SetPropertyInteger(string schemaNs, string propName, int propValue);

        void SetPropertyLong(string schemaNs, string propName, long propValue, PropertyOptions options);

        void SetPropertyLong(string schemaNs, string propName, long propValue);

        void SetPropertyDouble(string schemaNs, string propName, double propValue, PropertyOptions options);

        void SetPropertyDouble(string schemaNs, string propName, double propValue);

        void SetPropertyDate(string schemaNs, string propName, IXmpDateTime propValue, PropertyOptions options);

        void SetPropertyDate(string schemaNs, string propName, IXmpDateTime propValue);

        void SetPropertyCalendar(string schemaNs, string propName, DateTime propValue, PropertyOptions options);

        void SetPropertyCalendar(string schemaNs, string propName, DateTime propValue);

        void SetPropertyBase64(string schemaNs, string propName, sbyte[] propValue, PropertyOptions options);

        void SetPropertyBase64(string schemaNs, string propName, sbyte[] propValue);

        IXmpIterator Iterator();

        IXmpIterator Iterator(IteratorOptions options);

        IXmpIterator Iterator(string schemaNs, string propName, IteratorOptions options);

        void Sort();

        void Normalize(ParseOptions options);

        string DumpObject();
    }
}
