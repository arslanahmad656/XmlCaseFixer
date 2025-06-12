using System.Runtime.CompilerServices;
using XmlCaseFixer.Common;
using XmlCaseFixer.Common.Searcher;

namespace XmlCaseFixer.Searcher;

public class Searcher : ISearcher
{
    public Searcher(Settings settings)
    {
        Validators.ValidateSearcherSettings(settings);
        Settings = settings;
    }

    public Settings Settings { get; }

    public event EventHandler<SearchResult>? ElementFound;
    public event EventHandler? SearchStarted;
    public event EventHandler? SearchCompleted;
    public event EventHandler<Exception>? ErrorOccurred;
    public event EventHandler<OperationCanceledException>? SearchCanceled;
    public event EventHandler<SearchProgress>? ProgressChanged;


    public async Task Search(string tagName, string attributeName, CancellationToken cancellationToken)
        => await Search(tagName, attributeName, res => ElementFound?.Invoke(this, res), cancellationToken).ConfigureAwait(false);
    public async Task Search(string attributeName, CancellationToken cancellationToken)
        => await Search(string.Empty, attributeName, res => ElementFound?.Invoke(this, res), cancellationToken).ConfigureAwait(false);

    public async Task<List<SearchResult>> GetSearchResults(string attributeName, CancellationToken cancellationToken)
    => await GetSearchResults(string.Empty, attributeName, cancellationToken).ConfigureAwait(false);

    public async Task<List<SearchResult>> GetSearchResults(string tagName, string attributeName, CancellationToken cancellationToken)
    {
        var list = new List<SearchResult>();

        await Search(tagName, attributeName, res => list.Add(res), cancellationToken).ConfigureAwait(false);

        return list;
    }

    public async Task<Dictionary<string, List<(XmlAttributeInfo Attribute, Location Location)>>> FindDuplicates(string attributeName, CancellationToken cancellationToken)
        => await FindDuplicates(string.Empty, attributeName, cancellationToken).ConfigureAwait(false);

    public async Task<Dictionary<string, List<(XmlAttributeInfo Attribute, Location Location)>>> FindDuplicates(string tagName, string attributeName, CancellationToken cancellationToken)
    {
        var searchResults = await GetSearchResults(tagName, attributeName, cancellationToken).ConfigureAwait(false);
        var duplicates = await Task.Run(() =>
        {
            var duplicates = Helper.FindDuplicates(searchResults, Settings.UniqueOnCaseDiffOnly);

            return duplicates;
        }).ConfigureAwait(false);

        return duplicates;
    }

    private async Task Search(string tagName, string attributeName, Action<SearchResult> onSearchResultReceived, CancellationToken cancellationToken)
    {
        SearchStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            Validators.ValidateAttributeName(attributeName);
            await Helper.Search(
                settings: Settings,
                filter: el => Task.FromResult(filterXmlElement(el)),
                onResultReceived: onResultReceived,
                progress: new Progress<SearchProgress>(reportProgress),
                onSearchStart: onSearchStarted,
                onSearchComplete: onSearchCompleted,
                onFileError: onError,
                onElementError: onError,
                cancellationToken: cancellationToken
                ).ConfigureAwait(false);
        }
        catch(OperationCanceledException ex)
        {
            SearchCanceled?.Invoke(this, ex);
        }
        catch (Exception ex)
        {
            await onError(ex).ConfigureAwait(false);
        }

        (bool Include, object? Data) filterXmlElement(XmlElementInfo xmlElementInfo)
        {
            if (!string.IsNullOrWhiteSpace(tagName) && !string.Equals(xmlElementInfo.TagName, tagName, StringComparison.InvariantCulture))
            {
                return (false, null);
            }

            if (string.IsNullOrWhiteSpace(attributeName))
            {
                return (false, null);
            }

            if (!xmlElementInfo.Attributes.TryGetValue(attributeName, out string? value))
            {
                return (false, null);
            }

            return (true, new XmlAttributeInfo(xmlElementInfo.TagName, xmlElementInfo.TagName, attributeName, value));
        }

        Task onResultReceived(SearchResult searchResult)
        {
            onSearchResultReceived(searchResult);
            return Task.CompletedTask;
        }
        
        void reportProgress(SearchProgress searchProgress)
        {
            ProgressChanged?.Invoke(this, searchProgress);
        }

        Task onSearchStarted()
        {
            SearchStarted?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        Task onSearchCompleted()
        {
            SearchCompleted?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        Task onError(Exception exception)
        {
            ErrorOccurred?.Invoke(this, exception);
            return Task.CompletedTask;
        }
    }
}
