using CliApp;
using Microsoft.Extensions.Configuration;
using System.Text;
using XmlCaseFixer.Common;
using XmlCaseFixer.Common.Searcher;
using XmlCaseFixer.Searcher;

var directory = DateTime.Now.ToString("yyyyMMdd_HHmmss.fff");
var progressLocker = new object();
var errorLocker = new object();
var elementFoundLocker = new object();

try
{
    Directory.CreateDirectory(directory);
    var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
    var searcherSettings = config.GetSection("SearcherSettings").Get<Settings>() ?? throw new Exception("Could not read the searcher settings.");
    var searchInput = config.GetSection("SearcherInput").Get<SearcherInput>() ?? throw new Exception("Could not read the searcher input.");
    var searcher = new Searcher(searcherSettings);

    searcher.SearchStarted += Searcher_SearchStarted;
    searcher.SearchCompleted += Searcher_SearchCompleted;
    searcher.ElementFound += Searcher_ElementFound;
    searcher.ErrorOccurred += Searcher_ErrorOccurred;
    searcher.ProgressChanged += Searcher_ProgressChanged;

    Console.WriteLine($"Searcher Settings: {searcherSettings}");
    Console.WriteLine($"Searcher Input: {searchInput}");

    Console.WriteLine($"Finding duplicates.");
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();

    var duplicates = await searcher.FindDuplicates(searchInput.TagName, searchInput.AttributeName, CancellationToken.None);

    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();

    Console.WriteLine($"Total duplicates: {duplicates.Keys.Count}");
    var fileName = GetLogFileName("duplicate");
    var sb = new StringBuilder();
    sb.AppendLine($"Total duplicates: {duplicates.Keys.Count}");
    
    for (var i = 0; i < duplicates.Keys.Count; i++)
    {
        var key = duplicates.Keys.ElementAt(i);
        sb.AppendLine($"Item# {i + 1}: {key}. Duplicates ({duplicates[key].Count}): ");
        foreach (var duplicate in duplicates[key])
        {
            sb.AppendLine($"""

                    Element: {duplicate.Attribute.TagName}
                    Attribute: {duplicate.Attribute.AttributeName}
                    Value: {duplicate.Attribute.AttributeValue}
                    File: {duplicate.Location.File}
                    Location: {duplicate.Location.Line} ({duplicate.Location.Column})

                """);
        }
    }

    var text = sb.ToString();
    File.AppendAllText(fileName, text);
    Console.WriteLine(text);
}
catch (Exception ex)
{
	Console.WriteLine($"Error occurred in the application: {ex.GetType().FullName}: {ex.Message}");
	Console.WriteLine(ex.StackTrace);
}

Console.WriteLine();
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("Program end. Press any key to exit...");
Console.ReadKey(true);

void Searcher_ProgressChanged(object? sender, SearchProgress e)
{
    var fileName = GetLogFileName("progress");
    var line = $"""

        Searching in element: {e.TagName} located at:
            {e.FileName} at line {e.LineNumber} column {e.LineColumn}.
        """;
    lock (progressLocker)
    {
        File.AppendAllText(fileName, line);
    }
}

void Searcher_ErrorOccurred(object? sender, Exception e)
{
    var fileName = GetLogFileName("error");
    var line = $"""
        Error: {e.Message}. Inner error:
            {e.InnerException?.Message}
        """;

    lock (errorLocker)
    {
        File.AppendAllText(fileName, line);
    }
    
    Console.WriteLine(line);
}

void Searcher_ElementFound(object? sender, SearchResult e)
{
    var fileName = GetLogFileName("element");
    var line = $"""

            ELEMENT FOUND:
                File: {e.Location.File}
                Location: {e.Location.Line} ({e.Location.Column})
                Element: {(e.Data is not XmlAttributeInfo element ? string.Empty : $"{element.TagName} {element.AttributeName} {element.AttributeValue}")}

        """;

    lock (elementFoundLocker)
    {
        File.AppendAllText(fileName, line);
    }
    
    Console.WriteLine(line);
}

void Searcher_SearchCompleted(object? sender, EventArgs e)
{
    Console.WriteLine("Search Completed.");
}

void Searcher_SearchStarted(object? sender, EventArgs e)
{
    Console.WriteLine("Search Started.");
}

string GetLogFileName(string type) => $@"{directory}\{type}.log";