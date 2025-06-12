namespace XmlCaseFixer.Common.Searcher;

public record SearchProgress(
    string FileName,
    string AttributeName,
    uint LineNumber,
    uint LineColumn
    );
