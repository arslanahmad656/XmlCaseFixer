using XmlCaseFixer.Common.Searcher;

namespace XmlCaseFixer.Searcher;

public class Searcher : ISearcher
{
    public Searcher(SearcherSettings settings)
    {
        Validators.ValidateSearcherSettings(settings);
        Settings = settings;
    }

    public SearcherSettings Settings { get; }

    public IAsyncEnumerable<SearchResult> Search(string attributeName, CancellationToken cancellationToken)
    {
        Validators.ValidateAttributeName(attributeName);


    }
}
