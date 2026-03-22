using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Verendar.Common.Shared;

public static class SlugUtils
{
    private const int DefaultRandomEntropyBytes = 4;
    private const int DefaultMaxCollisionAttempts = 32;

    public static string ToSlug(string? source, int maxLength = 100)
    {
        if (string.IsNullOrWhiteSpace(source))
            return string.Empty;

        var working = RemoveDiacritics(source.Trim());
        var sb = new StringBuilder(working.Length);
        var lastWasHyphen = false;

        foreach (var c in working)
        {
            if (char.IsAsciiLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
                lastWasHyphen = false;
            }
            else if (c is '_' or '-' || char.IsWhiteSpace(c))
            {
                if (!lastWasHyphen && sb.Length > 0)
                {
                    sb.Append('-');
                    lastWasHyphen = true;
                }
            }
        }

        while (sb.Length > 0 && sb[^1] == '-')
            sb.Length--;
        while (sb.Length > 0 && sb[0] == '-')
            sb.Remove(0, 1);

        var result = sb.ToString();
        if (maxLength > 0 && result.Length > maxLength)
        {
            result = result[..maxLength].TrimEnd('-');
            while (result.Length > 0 && result[^1] == '-')
                result = result[..^1];
        }

        return result;
    }

    public static string EnsureUnique(
        string? baseSlug,
        Func<string, bool> isTaken,
        int maxLength = 100,
        int randomEntropyBytes = DefaultRandomEntropyBytes,
        int maxCollisionAttempts = DefaultMaxCollisionAttempts)
    {
        ArgumentNullException.ThrowIfNull(isTaken);
        if (randomEntropyBytes < 2)
            throw new ArgumentOutOfRangeException(nameof(randomEntropyBytes), "Use at least 2 bytes of entropy.");

        var slug = NormalizeBaseSlug(baseSlug, maxLength);

        if (!isTaken(slug))
            return slug;

        for (var attempt = 0; attempt < maxCollisionAttempts; attempt++)
        {
            var suffix = "-" + CreateRandomHexSuffix(randomEntropyBytes);
            var candidate = BuildCandidateWithSuffix(slug, suffix, maxLength);
            if (!isTaken(candidate))
                return candidate;
        }

        throw new InvalidOperationException("Could not allocate a unique slug after many attempts.");
    }

    /// <summary>
    /// Async variant of <see cref="EnsureUnique"/> for database checks.
    /// </summary>
    public static async Task<string> EnsureUniqueAsync(
        string? baseSlug,
        Func<string, Task<bool>> isTakenAsync,
        int maxLength = 100,
        int randomEntropyBytes = DefaultRandomEntropyBytes,
        int maxCollisionAttempts = DefaultMaxCollisionAttempts,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(isTakenAsync);
        if (randomEntropyBytes < 2)
            throw new ArgumentOutOfRangeException(nameof(randomEntropyBytes), "Use at least 2 bytes of entropy.");

        var slug = NormalizeBaseSlug(baseSlug, maxLength);

        if (!await isTakenAsync(slug).ConfigureAwait(false))
            return slug;

        for (var attempt = 0; attempt < maxCollisionAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var suffix = "-" + CreateRandomHexSuffix(randomEntropyBytes);
            var candidate = BuildCandidateWithSuffix(slug, suffix, maxLength);
            if (!await isTakenAsync(candidate).ConfigureAwait(false))
                return candidate;
        }

        throw new InvalidOperationException("Could not allocate a unique slug after many attempts.");
    }

    private static string NormalizeBaseSlug(string? baseSlug, int maxLength)
    {
        var slug = string.IsNullOrWhiteSpace(baseSlug)
            ? "item"
            : baseSlug.Trim().Trim('-');

        if (slug.Length == 0)
            slug = "item";

        if (maxLength > 0 && slug.Length > maxLength)
            slug = slug[..maxLength].TrimEnd('-');

        if (slug.Length == 0)
            slug = "item";

        return slug;
    }

    private static string BuildCandidateWithSuffix(string baseSlug, string suffix, int maxLength)
    {
        if (maxLength <= 0)
            return baseSlug + suffix;

        if (suffix.Length >= maxLength)
            return suffix.TrimStart('-'); // degenerate; caller should use longer maxLength

        var prefixLen = maxLength - suffix.Length;
        if (prefixLen < 1)
            prefixLen = 1;

        var prefix = baseSlug.Length > prefixLen ? baseSlug[..prefixLen].TrimEnd('-') : baseSlug;
        if (prefix.Length == 0)
            prefix = "x";

        var candidate = prefix + suffix;
        return candidate.Length > maxLength ? candidate[..maxLength].TrimEnd('-') : candidate;
    }

    private static string CreateRandomHexSuffix(int byteCount)
    {
        Span<byte> buffer = stackalloc byte[byteCount];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToHexString(buffer).ToLowerInvariant();
    }

    private static string RemoveDiacritics(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            builder.Append(c switch
            {
                '\u0110' or '\u0111' => 'd', // Đ, đ
                _ => c,
            });
        }

        var normalized = builder.ToString().Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                result.Append(c);
        }

        return result.ToString().Normalize(NormalizationForm.FormC);
    }
}
