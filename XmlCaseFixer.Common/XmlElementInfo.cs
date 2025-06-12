using System.Collections.ObjectModel;

namespace XmlCaseFixer.Common;

public record XmlElementInfo(
    string TagName,
    string Namespace,
    ReadOnlyDictionary<string, string> Attributes
    );
