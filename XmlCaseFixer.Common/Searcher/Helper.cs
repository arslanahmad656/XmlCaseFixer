using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Xml;

namespace XmlCaseFixer.Common.Searcher;

public static class Helper
{
    public static async Task Search(
        Settings settings,
        Func<XmlElementInfo, Task<(bool Include, object? Data)>> filter,
        Func<SearchResult, Task> onResultReceived,
        IProgress<SearchProgress>? progress = null,
        Func<Task>? onSearchStart = null,
        Func<Task>? onSearchComplete = null,
        Func<Exception, Task>? onFileError = null,
        Func<Exception, Task>? onElementError = null,
        CancellationToken cancellationToken = default)
    {
        var xmlReaderSettings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreWhitespace = true,
            Async = true,
        };

        if (onSearchStart is not null)
        {
            await onSearchStart().ConfigureAwait(false);
        }

        foreach (var file in Directory.EnumerateFiles(settings.RootPath, settings.XmlFilesOnly ? "*.xml" : "*.*", SearchOption.AllDirectories))
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report(new(file, string.Empty, 0, 0));

                using var reader = XmlReader.Create(file, xmlReaderSettings);
                var lineInfo = (IXmlLineInfo)reader;

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    try
                    {
                        if (settings.FineGrainedControl)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            progress?.Report(new(file, reader.Name, (uint)lineInfo.LineNumber, (uint)lineInfo.LinePosition));
                        }

                        if (reader.NodeType is XmlNodeType.Element)
                        {
                            var attributes = GetElementAttributes(reader);
                            var filterResult = await filter(new XmlElementInfo(reader.Name, reader.NamespaceURI, new ReadOnlyDictionary<string, string>(attributes))).ConfigureAwait(false);
                            
                            if (filterResult.Include)
                            {
                                await onResultReceived(new (new Location(file, (uint)lineInfo.LineNumber, (uint)lineInfo.LinePosition), filterResult.Data)).ConfigureAwait(false);
                            }
                        }
                    }
                    catch(OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        if (onElementError is not null)
                        {
                            await onElementError(new Exception($"Error occurred while searching the element {reader.Name} in the file {file}.", ex)).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (onFileError is not null)
                {
                    await onFileError(new Exception($"Error occurred while searching the file {file}.", ex)).ConfigureAwait(false);
                }
            }
        }

        if (onSearchComplete is not null)
        {
            await onSearchComplete().ConfigureAwait(false);
        }
    }

    public static Dictionary<string, List<(XmlAttributeInfo Attribute, Location Location)>> FindDuplicates(IEnumerable<SearchResult> searchResults, bool requireCaseDiff)
    {
        var lookup = new Dictionary<string, List<(XmlAttributeInfo Attribute, Location Location)>>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var searchResult in searchResults)
        {
            if (searchResult.Data is not XmlAttributeInfo attribute)
            {
                throw new Exception("Data must be of type XmlAttributeInfo.");
            }

            if (!lookup.TryGetValue(attribute.AttributeValue, out var list))
            {
                list = [];
                lookup[attribute.AttributeValue] = list;
            }

            list.Add((attribute, searchResult.Location));
        }

        var duplicates = new Dictionary<string, List<(XmlAttributeInfo Attribute, Location Location)>>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var pair in lookup)
        {
            bool shouldAdd = false;
            if (!requireCaseDiff)
            {
                shouldAdd = pair.Value.Count > 1;
            }
            else
            {
                var allValues = pair.Value
                    .Select(v => v.Attribute.AttributeValue)
                    .Distinct(StringComparer.InvariantCulture);
                if (!allValues.TryGetNonEnumeratedCount(out int count))
                {
                    count = allValues.Count();
                }

                shouldAdd = count > 1;
            }

            if (shouldAdd)
            {
                duplicates[pair.Key] = pair.Value;
            }
        }

        //var duplicates = lookup.Where(x => x.Value.Count > 1).ToDictionary(x => x.Key, x => x.Value, StringComparer.InvariantCultureIgnoreCase);

        return duplicates;
    }

    private static Dictionary<string, string> GetElementAttributes(XmlReader reader)
    {
        var attributes = new Dictionary<string, string>();

        for (var i = 0; i < reader.AttributeCount; i++)
        {
            reader.MoveToAttribute(i);
            attributes[reader.Name] = reader.Value;
        }

        reader.MoveToElement();

        return attributes;
    }
}
