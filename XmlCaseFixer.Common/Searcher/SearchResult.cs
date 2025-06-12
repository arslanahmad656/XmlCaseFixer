namespace XmlCaseFixer.Common.Searcher;

public record SearchResult(
    string File,
    uint Line,
    uint Column,
    object? Data
    );
