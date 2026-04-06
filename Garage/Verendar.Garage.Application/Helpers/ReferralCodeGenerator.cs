using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Verendar.Garage.Application.Helpers;

public static class ReferralCodeGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int SuffixLength = 4;
    private const int MaxRetries = 5;

    public static async Task<string?> GenerateAsync(
        string slug,
        Func<string, Task<bool>> isUnique,
        CancellationToken ct = default)
    {
        var prefix = BuildPrefix(slug);

        for (var i = 0; i < MaxRetries; i++)
        {
            ct.ThrowIfCancellationRequested();
            var code = $"{prefix}-{RandomSuffix()}";
            if (await isUnique(code))
                return code;
        }

        return null;
    }

    private static string BuildPrefix(string slug)
    {
        var clean = Regex.Replace(slug.ToUpperInvariant(), "[^A-Z0-9]", string.Empty);
        if (clean.Length == 0)
            clean = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

        return clean.Length >= 6 ? clean[..6] : clean;
    }

    private static string RandomSuffix()
    {
        return new string(Enumerable.Range(0, SuffixLength)
            .Select(_ => Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)])
            .ToArray());
    }
}
