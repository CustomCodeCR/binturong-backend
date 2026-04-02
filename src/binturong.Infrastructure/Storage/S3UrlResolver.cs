using Application.Options;

namespace Infrastructure.Storage;

internal static class S3UrlResolver
{
    public static string BuildPublicUrl(StorageOptions options, string key)
    {
        var normalizedKey = NormalizeKey(key);
        var s3 = options.S3;

        if (string.IsNullOrWhiteSpace(s3.Bucket))
            throw new InvalidOperationException("Storage:S3:Bucket is required.");

        if (!string.IsNullOrWhiteSpace(s3.ServiceUrl))
        {
            var serviceUrl = s3.ServiceUrl.Trim().TrimEnd('/');

            if (IsSupabaseEndpoint(serviceUrl))
            {
                var publicBaseUrl = serviceUrl.Replace(
                    "/storage/v1/s3",
                    "/storage/v1/object/public",
                    StringComparison.OrdinalIgnoreCase
                );

                return $"{publicBaseUrl}/{s3.Bucket}/{normalizedKey}";
            }

            if (s3.ForcePathStyle)
                return $"{serviceUrl}/{s3.Bucket}/{normalizedKey}";

            var uri = new Uri(serviceUrl);
            var hostWithBucket = $"{s3.Bucket}.{uri.Host}";
            var portPart = uri.IsDefaultPort ? string.Empty : $":{uri.Port}";
            return $"{uri.Scheme}://{hostWithBucket}{portPart}/{normalizedKey}";
        }

        if (!string.IsNullOrWhiteSpace(s3.Region))
            return $"https://{s3.Bucket}.s3.{s3.Region}.amazonaws.com/{normalizedKey}";

        return $"https://{s3.Bucket}.s3.amazonaws.com/{normalizedKey}";
    }

    public static bool IsSupabaseEndpoint(string serviceUrl)
    {
        return serviceUrl.Contains("/storage/v1/s3", StringComparison.OrdinalIgnoreCase)
            && serviceUrl.Contains(".supabase.co", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Storage key cannot be null or empty.", nameof(key));

        return key.Trim().TrimStart('/');
    }
}
