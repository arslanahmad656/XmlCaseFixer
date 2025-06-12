namespace XmlCaseFixer.Common.Searcher;

public static class Validators
{
    public static void ValidateSearcherSettings(Settings settings)
    {
        if (!Directory.Exists(settings.RootPath))
        {
            throw new Exception($"Directory {settings.RootPath} does not exist.");
        }
    }

    public static void ValidateAttributeName(string attributeName)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new Exception("Attribute name cannot be empty.");
        }
    }
}
