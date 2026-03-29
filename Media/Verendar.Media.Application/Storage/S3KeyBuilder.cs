using System.Text;
using System.Text.RegularExpressions;
using Verendar.Media.Domain.Entities;

namespace Verendar.Media.Application.Storage
{
    public static class S3KeyBuilder
    {
        public const int MaxKeyUtf8ByteLength = 1000;

        private static readonly Regex SegmentNonSlug = new(@"[^a-z0-9_-]", RegexOptions.Compiled);

        private static readonly Regex RepeatedUnderscores = new(@"_+", RegexOptions.Compiled);

        public static string BuildMediaUploadKey(
            string? environmentName,
            FileType fileType,
            Guid userId,
            string originalFileName)
        {
            var env = SanitizeSlugSegment(MapEnvironmentName(environmentName));
            const string service = "media";
            var context = SanitizeSlugSegment(MapFileTypeToContext(fileType));
            var entityId = fileType == FileType.Avatar ? userId : Guid.Empty;
            var extension = SanitizeExtension(originalFileName);
            var objectName = $"{Guid.NewGuid():N}{extension}";
            objectName = EnsureNonPeriodOnlySegment(objectName);

            var prefix = $"{env}/{service}/{context}/{entityId:D}/";
            var key = prefix + objectName;
            key = EnsureKeyWithinUtf8ByteLimit(prefix, objectName);

            return key;
        }

        private static string MapEnvironmentName(string? environmentName)
        {
            if (string.IsNullOrWhiteSpace(environmentName))
                return "general";

            return environmentName.Trim() switch
            {
                "Development" => "dev",
                "Staging" => "staging",
                "Production" => "prod",
                _ => environmentName.Trim()
            };
        }

        private static string MapFileTypeToContext(FileType fileType)
        {
            return fileType switch
            {
                FileType.Avatar => "avatar",
                FileType.VehicleType => "vehicle_type",
                FileType.VehicleBrand => "vehicle_brand",
                FileType.VehicleVariant => "vehicle_variant",
                FileType.PartCategory => "part_category",
                FileType.Other => "misc_other",
                _ => "misc_unknown"
            };
        }

        private static string SanitizeExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return ".bin";

            var ext = Path.GetExtension(fileName).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(ext))
                return ".bin";

            // Keep leading dot; only safe chars in extension body (no path segments)
            ext = ext.Length > 1 ? "." + SegmentNonSlug.Replace(ext[1..], "") : ".bin";
            if (ext.Length < 2)
                return ".bin";

            return ext.Length > 32 ? ext[..32] : ext;
        }

        private static string SanitizeSlugSegment(string segment)
        {
            if (string.IsNullOrWhiteSpace(segment))
                return "general";

            var s = segment.Trim().ToLowerInvariant();
            s = SegmentNonSlug.Replace(s, "_");
            s = RepeatedUnderscores.Replace(s, "_").Trim('_');
            return string.IsNullOrEmpty(s) ? "general" : s;
        }

        private static string EnsureNonPeriodOnlySegment(string segment)
        {
            if (segment is "." or "..")
                return "file";

            return segment;
        }

        private static string EnsureKeyWithinUtf8ByteLimit(string prefix, string fileSegment)
        {
            var prefixBytes = Encoding.UTF8.GetByteCount(prefix);
            var fileBytes = Encoding.UTF8.GetByteCount(fileSegment);
            if (prefixBytes + fileBytes <= MaxKeyUtf8ByteLength)
                return prefix + fileSegment;

            var budget = MaxKeyUtf8ByteLength - prefixBytes;
            if (budget < 1)
                return prefix + "f";

            var truncated = TruncateUtf8ByPrefixBytes(fileSegment, budget);
            truncated = EnsureNonPeriodOnlySegment(truncated);
            if (string.IsNullOrEmpty(truncated))
                truncated = "file";

            return prefix + truncated;
        }

        private static string TruncateUtf8ByPrefixBytes(string s, int maxBytes)
        {
            if (maxBytes <= 0)
                return string.Empty;

            var allBytes = Encoding.UTF8.GetBytes(s);
            if (allBytes.Length <= maxBytes)
                return s;

            var cut = maxBytes;
            while (cut > 0 && (allBytes[cut - 1] & 0b1100_0000) == 0b1000_0000)
                cut--;

            return Encoding.UTF8.GetString(allBytes.AsSpan(0, cut));
        }
    }
}
