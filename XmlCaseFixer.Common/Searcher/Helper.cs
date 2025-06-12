using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Xml;

namespace XmlCaseFixer.Common.Searcher;

public static class Helper
{
    public static async Task Search(
        SearcherSettings settings,
        Func<XmlElementInfo, Task<bool>> filter,
        Func<SearchResult, Task> onResultReceived,
        IProgress<SearchProgress>? progress = null,
        Func<Exception, Task>? onFileError = null,
        Func<Exception, Task>? onElementError = null,
        CancellationToken cancellationToken = default)
    {
        var xmlReaderSettings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreWhitespace = true
        };

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
                            var include = await filter(new XmlElementInfo(reader.Name, reader.NamespaceURI, new ReadOnlyDictionary<string, string>(attributes))).ConfigureAwait(false);
                            
                            if (include)
                            {
                                await onResultReceived(new SearchResult(file, (uint)lineInfo.LineNumber, (uint)lineInfo.LinePosition)).ConfigureAwait(false);
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
