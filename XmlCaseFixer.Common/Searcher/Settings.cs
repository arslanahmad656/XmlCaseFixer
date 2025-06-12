namespace XmlCaseFixer.Common.Searcher;

public record Settings(
    string RootPath,
    bool XmlFilesOnly,
    bool Recursive,
    bool FineGrainedControl,
    bool UniqueOnCaseDiffOnly
    );
