using System.Collections;

namespace Sign.Org.BouncyCastle.Cms
{
    public interface CmsAttributeTableGenerator
    {
        AttributeTable GetAttributes(IDictionary parameters);
    }
}
