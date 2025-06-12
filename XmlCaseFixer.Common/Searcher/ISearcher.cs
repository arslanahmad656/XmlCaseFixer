namespace XmlCaseFixer.Common.Searcher;

public interface ISearcher
{
    SearcherSettings Settings { get; }
    IAsyncEnumerable<SearchResult> Search(string attributeName, CancellationToken cancellationToken);
}
