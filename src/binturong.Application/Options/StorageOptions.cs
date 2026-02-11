namespace Application.Options;

public sealed class StorageOptions
{
    public S3Options S3 { get; init; } = new();
    public LocalOptions Local { get; init; } = new();

    public sealed class S3Options
    {
        public bool Enabled { get; init; } = false;
        public string Bucket { get; init; } = string.Empty;
        public string Prefix { get; init; } = "binturong";
        public string Region { get; init; } = string.Empty;
    }

    public sealed class LocalOptions
    {
        public string RootPath { get; init; } = "uploads";
        public string PublicBaseUrl { get; init; } = ""; // optional
    }
}
