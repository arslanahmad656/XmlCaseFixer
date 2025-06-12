using System.Text;
using XmlCaseFixer.Common;
using XmlCaseFixer.Common.Searcher;
using XmlCaseFixer.Searcher;

var searcher = new Searcher(new (
    RootPath: @"C:\UsercubeDemo\Conf.388225Cx",
    XmlFilesOnly: true,
    Recursive: true,
    FineGrainedControl: false,
    UniqueOnCaseDiffOnly: true));

searcher.SearchStarted += Searcher_SearchStarted;
searcher.ElementFound += Searcher_ElementFound;
searcher.ErrorOccurred += Searcher_ErrorOccurred;
searcher.ProgressChanged += Searcher_ProgressChanged;
searcher.SearchCanceled += Searcher_SearchCanceled;
searcher.SearchCompleted += Searcher_SearchCompleted;

//await searcher.Search("SingleRole", "Identifier", CancellationToken.None).ConfigureAwait(false);
//var results = await searcher.GetSearchResults("SingleRole", "Identifier", CancellationToken.None).ConfigureAwait(false);
//var results = await searcher.GetSearchResults("IdentifierX", CancellationToken.None).ConfigureAwait(false);
//Console.WriteLine($"Total results: {results.Count}");

var duplicates = await searcher.FindDuplicates("SingleRole", "Identifier", CancellationToken.None).ConfigureAwait(false);
//var duplicates = await searcher.FindDuplicates("SingleRole", "Identifier", CancellationToken.None).ConfigureAwait(false);
Console.WriteLine($"Total duplicates: {duplicates.Count}");

void Searcher_SearchCompleted(object? sender, EventArgs e)
{
    Console.WriteLine("\t SEARCH COMPLETED.");
}

void Searcher_SearchCanceled(object? sender, OperationCanceledException e)
{
    Console.WriteLine("\t OPERATION CANCELLED.");
}

void Searcher_ProgressChanged(object? sender, SearchProgress e)
{
    Console.WriteLine($"Progress: Searching in element {e.TagName} from file {e.FileName} at line {e.LineNumber} ({e.LineColumn})");
}

void Searcher_ErrorOccurred(object? sender, Exception e)
{
    Console.WriteLine($"""
            
        ERROR OCCURRED: {e.Message}

        """);
}

void Searcher_ElementFound(object? sender, SearchResult e)
{
    var element = e.Data as XmlAttributeInfo;
    Console.WriteLine($"""

            ELEMENT FOUND:
                File: {e.Location.File}
                Location: {e.Location.Line} ({e.Location.Column})
                Element: {(element is null ? string.Empty : $"{element.TagName} {element.AttributeName} {element.AttributeValue}")}

        """);
}

void Searcher_SearchStarted(object? sender, EventArgs e)
{
    Console.WriteLine("Search started.");
}