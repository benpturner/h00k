using System;
using System.Xml.Linq;

public static class XElementExtensionMethods
{
    public static string ElementValueNull(this XElement element)
    {
        if (element != null)
            return element.Value;

        return "";
    }

    //This method is to handle if attribute is missing
    public static string AttributeValueNull(this XElement element, string attributeName)
    {
        if (element == null)
            return "";
        else
        {
            XAttribute attr = element.Attribute(attributeName);
            return attr == null ? "" : attr.Value;
        }
    }

}
