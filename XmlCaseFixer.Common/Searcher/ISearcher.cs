namespace XmlCaseFixer.Common.Searcher;

public interface ISearcher
{
    event EventHandler<SearchResult>? ElementFound;
    event EventHandler? SearchStarted;
    event EventHandler? SearchCompleted;
    event EventHandler<Exception>? ErrorOccurred;
    event EventHandler<OperationCanceledException>? SearchCanceled;
    event EventHandler<SearchProgress>? ProgressChanged;

    Settings Settings { get; }
    Task Search(string attributeName, CancellationToken cancellationToken);
    Task Search(string tagName, string attributeName, CancellationToken cancellationToken);

    Task<List<SearchResult>> GetSearchResults(string attributeName, CancellationToken cancellationToken);
    Task<List<SearchResult>> GetSearchResults(string tatgName, string attributeName, CancellationToken cancellationToken);

    Task<Dictionary<string, List<(XmlAttributeInfo Attribute, Location Location)>>> FindDuplicates(string attributeName, CancellationToken cancellationToken);
    Task<Dictionary<string, List<(XmlAttributeInfo Attribute, Location Location)>>> FindDuplicates(string tagName, string attributeName, CancellationToken cancellationToken);
}
