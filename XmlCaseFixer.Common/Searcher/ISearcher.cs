namespace XmlCaseFixer.Common.Searcher;

public interface ISearcher
{
    event EventHandler<SearchResult>? ElementFound;
    event EventHandler? SearchStarted;
    event EventHandler? SearchCompleted;
    event EventHandler<Exception>? ErrorOccurred;
    event EventHandler<OperationCanceledException>? SearchCanceled;
    event EventHandler<SearchProgress>? ProgressChanged;

    SearcherSettings Settings { get; }
    Task Search(string attributeName, CancellationToken cancellationToken);

    Task<List<SearchResult>> GetSearchResults(string attributeName, CancellationToken cancellationToken);
}
