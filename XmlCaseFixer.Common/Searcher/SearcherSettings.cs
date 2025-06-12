namespace XmlCaseFixer.Common.Searcher;

public record SearcherSettings(
    string RootPath,
    bool XmlFilesOnly,
    bool Recursive,
    bool FineGrainedControl
    );
