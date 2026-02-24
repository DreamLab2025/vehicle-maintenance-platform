using System.Reflection;

namespace Verendar.Vehicle.Infrastructure.Data;


public static class SeedDataLoader
{
    private static readonly Assembly Assembly = typeof(SeedDataLoader).Assembly;
    private const string SeedDataPrefix = "Verendar.Vehicle.Infrastructure.Data.SeedData.";

    public static Stream? GetSeedDataStream(string fileName)
    {
        var resourceName = SeedDataPrefix + fileName;
        var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream != null) return stream;
        var suffix = "Data.SeedData." + fileName;
        var fullName = Assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        return fullName != null ? Assembly.GetManifestResourceStream(fullName) : null;
    }

    public static List<Dictionary<string, string>> ReadCsvAsDictionaries(string fileName)
    {
        using var stream = GetSeedDataStream(fileName);
        if (stream == null)
            throw new FileNotFoundException($"Seed data file not found: {fileName}", fileName);

        using var reader = new StreamReader(stream);
        var line = reader.ReadLine();
        if (line != null && line.StartsWith("sep=", StringComparison.OrdinalIgnoreCase))
            line = reader.ReadLine(); // skip sep line
        if (line == null)
            return [];

        var headers = ParseCsvLine(line);
        var result = new List<Dictionary<string, string>>();

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var values = ParseCsvLine(line);
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Count && i < values.Count; i++)
                dict[headers[i]] = values[i];
            result.Add(dict);
        }

        return result;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var list = new List<string>();
        var i = 0;
        while (i < line.Length)
        {
            if (line[i] == '"')
            {
                var start = i + 1;
                var end = start;
                while (end < line.Length)
                {
                    if (line[end] == '"')
                    {
                        if (end + 1 < line.Length && line[end + 1] == '"')
                        { end += 2; continue; }
                        break;
                    }
                    end++;
                }
                list.Add(line[start..end].Replace("\"\"", "\"", StringComparison.Ordinal));
                i = end + 1;
                if (i < line.Length && line[i] == ',') i++;
            }
            else
            {
                var start = i;
                while (i < line.Length && line[i] != ',') i++;
                list.Add(line[start..i].Trim());
                if (i < line.Length && line[i] == ',') i++;
            }
        }
        return list;
    }
}
