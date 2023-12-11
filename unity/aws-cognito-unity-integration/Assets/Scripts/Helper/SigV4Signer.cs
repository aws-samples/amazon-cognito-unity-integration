using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
public class SigV4Signer 
{
    private string region = ExternalParameters.region.SystemName;
    private const string ALGORITHM = "AWS4-HMAC-SHA256";
    private const string EMPTY_STRING_HASH = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
    private static string ToHexString(ReadOnlySpan<byte> bytes)
    {
        var hex = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            hex.AppendFormat("{0:x2}", b);
        }
        return hex.ToString();
    }

    private static byte[] GetSignatureKey(string secretKey, string dateStamp, string region, string service)
    {
        byte[] kSecret = Encoding.UTF8.GetBytes("AWS4" + secretKey);
        byte[] kDate = HmacSha256(kSecret, Encoding.UTF8.GetBytes(dateStamp));
        byte[] kRegion = HmacSha256(kDate, Encoding.UTF8.GetBytes(region));
        byte[] kService = HmacSha256(kRegion, Encoding.UTF8.GetBytes(service));
        byte[] kSigning = HmacSha256(kService, Encoding.UTF8.GetBytes("aws4_request"));
        return kSigning;
    }

    private static byte[] HmacSha256(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data)
    {
        using (var hashAlgorithm = new HMACSHA256(key.ToArray()))
        {
            return hashAlgorithm.ComputeHash(data.ToArray());
        }
    }

    private string Hash(ReadOnlySpan<byte> bytesToHash)
    {
        using (var sha256 = SHA256.Create())
        {
            var result = sha256.ComputeHash(bytesToHash.ToArray());
            return ToHexString(result);
        }
    }

    public async Task<HttpRequestMessage> Sign(HttpRequestMessage request, string service)
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));
        _ = service ?? throw new ArgumentOutOfRangeException(nameof(service), service, "Not a valid service.");
        request.Headers.Host ??= request.RequestUri.Host;

        var content = request.Content != null ? await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false) : Array.Empty<byte>();

        var payloadHash = content.Length != 0 ? Hash(content) : EMPTY_STRING_HASH;

        request.Headers.TryAddWithoutValidation("x-amz-content-sha256", payloadHash);
        
        var t = DateTimeOffset.UtcNow + (TimeSpan.Zero);
        var amzDate = t.ToString("yyyyMMddTHHmmssZ");
        request.Headers.Add("x-amz-date", amzDate);
        request.Headers.Add("X-Amz-Security-Token", CredentialsManager.SessionToken);
        var dateStamp = t.ToString("yyyyMMdd");

        var canonicalRequest = new StringBuilder();
        canonicalRequest.Append($"{request.Method}\n");

        canonicalRequest.Append(string.Join("/", request.RequestUri.AbsolutePath.Split('/').Select(Uri.EscapeDataString)) + "\n");

        var canonicalQueryParams = GetCanonicalQueryParams(request);

        canonicalRequest.Append($"{canonicalQueryParams}\n");

        var signedHeadersList = request.Headers.OrderBy(a => a.Key.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase).Select(header =>
        {
            var value = string.Join(",", header.Value.Select(s => s.Trim()));
            canonicalRequest.Append($"{header.Key.ToLowerInvariant()}:{value}\n");
            return header.Key.ToLowerInvariant();
        }).ToList();

        canonicalRequest.Append("\n");

        var signedHeaders = string.Join(";", signedHeadersList);

        canonicalRequest.Append($"{signedHeaders}\n{payloadHash}");

        var credentialScope = $"{dateStamp }/{region}/{service}/aws4_request";

        var stringToSign = $"{ALGORITHM}\n{amzDate}\n{credentialScope}\n{Hash(Encoding.UTF8.GetBytes(canonicalRequest.ToString()))}";
        var stringToSignBytes = Encoding.UTF8.GetBytes(stringToSign);

        var signingKey = GetSignatureKey(CredentialsManager.SecretKey, dateStamp, region, service);
        var signature = ToHexString(HmacSha256(signingKey, new ReadOnlySpan<byte>(stringToSignBytes)));
        request.Headers.TryAddWithoutValidation("Authorization", $"{ALGORITHM} Credential={CredentialsManager.AccessKeyId}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}");

        return request;
    }

    private static string GetCanonicalQueryParams(HttpRequestMessage request)
    {
        var queryParams = request.RequestUri.Query;
        if (string.IsNullOrEmpty(queryParams))
        {
            return string.Empty;
        }

        var queryParamPairs = queryParams.TrimStart('?').Split('&');
        var values = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var queryParamPair in queryParamPairs)
        {
            var parts = queryParamPair.Split('=');
            var key = Uri.EscapeDataString(parts[0]);

            if (parts.Length == 1) // Handles keys without values
            {
                values.Add(key, new List<string> { $"{key}=" });
            }
            else
            {
                var value = Uri.EscapeDataString(parts[1]);
                if (!values.TryGetValue(key, out var valuesList))
                {
                    valuesList = new List<string>();
                    values.Add(key, valuesList);
                }
                valuesList.Add($"{key}={value}");
            }
        }

        var canonicalQueryParams = new StringBuilder();
        foreach (var entry in values)
        {
            var key = entry.Key;
            var valueList = entry.Value;
            valueList.Sort(); // Sort the values for correct canonical string
            foreach (var value in valueList)
            {
                if (canonicalQueryParams.Length > 0)
                {
                    canonicalQueryParams.Append('&');
                }
                canonicalQueryParams.Append(value);
            }
        }

        return canonicalQueryParams.ToString();
    }
}