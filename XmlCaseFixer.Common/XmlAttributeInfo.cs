namespace XmlCaseFixer.Common;

public record XmlAttributeInfo(string TagName,
    string Namespace,
    string AttributeName,
    string AttributeValue
    ): XmlElementInfoBase(TagName, Namespace);
