namespace XmlCaseFixer.Common.Searcher;

public record SearchProgress(
    string FileName,
    string TagName,
    uint LineNumber,
    uint LineColumn
    );
