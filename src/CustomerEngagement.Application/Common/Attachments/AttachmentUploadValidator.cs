using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Application.Common.Attachments;

/// <summary>
/// Shared validation and sanitisation for files uploaded through the widget
/// and dashboard. Centralising these rules keeps the two endpoints in sync.
/// </summary>
public static class AttachmentUploadValidator
{
    public const long MaxSizeBytes = 10L * 1024 * 1024; // 10 MB

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Images
        "image/png", "image/jpeg", "image/jpg", "image/gif", "image/webp", "image/svg+xml",
        // Documents
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        // Plain text / csv / json
        "text/plain", "text/csv", "application/json",
        // Audio / video (short clips)
        "audio/mpeg", "audio/ogg", "audio/wav", "audio/webm",
        "video/mp4", "video/webm", "video/quicktime",
        // Archives
        "application/zip",
    };

    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".bat", ".cmd", ".com", ".scr", ".msi", ".ps1",
        ".sh", ".bash", ".php", ".jsp", ".asp", ".aspx", ".jar",
    };

    public static void Validate(string? fileName, string? contentType, long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException("File name is required.");

        if (sizeBytes <= 0)
            throw new InvalidOperationException("File is empty.");

        if (sizeBytes > MaxSizeBytes)
            throw new InvalidOperationException($"File exceeds the {MaxSizeBytes / (1024 * 1024)} MB limit.");

        var ext = Path.GetExtension(fileName);
        if (!string.IsNullOrEmpty(ext) && BlockedExtensions.Contains(ext))
            throw new InvalidOperationException($"Files with extension '{ext}' are not allowed.");

        if (!string.IsNullOrWhiteSpace(contentType) && !AllowedContentTypes.Contains(contentType))
            throw new InvalidOperationException($"Content type '{contentType}' is not supported.");
    }

    /// <summary>
    /// Strips path components and unsafe characters from a client-supplied
    /// filename so it is safe to use in an object storage key. Preserves
    /// the extension.
    /// </summary>
    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "file";

        // Drop any directory component the client may have sent.
        var bare = Path.GetFileName(fileName);

        // Replace characters that are not safe for S3 keys / URLs.
        var sanitized = new string(bare
            .Select(c => char.IsLetterOrDigit(c) || c is '.' or '-' or '_' ? c : '_')
            .ToArray());

        // Collapse repeated underscores to keep keys tidy.
        while (sanitized.Contains("__"))
            sanitized = sanitized.Replace("__", "_");

        sanitized = sanitized.Trim('_', '.');

        // Guard against degenerate inputs like "....".
        if (string.IsNullOrEmpty(sanitized))
            return "file";

        // Keep keys bounded.
        const int maxLength = 200;
        if (sanitized.Length > maxLength)
        {
            var ext = Path.GetExtension(sanitized);
            var stem = Path.GetFileNameWithoutExtension(sanitized);
            sanitized = stem[..Math.Min(stem.Length, maxLength - ext.Length)] + ext;
        }

        return sanitized;
    }

    public static AttachmentType ResolveFileType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return AttachmentType.File;

        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return AttachmentType.Image;
        if (contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            return AttachmentType.Audio;
        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return AttachmentType.Video;

        return AttachmentType.File;
    }
}
