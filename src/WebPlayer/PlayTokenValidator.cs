using System.Security.Cryptography;
using System.Text;

namespace QuestViva.WebPlayer;

public static class PlayTokenValidator
{
    public static bool Validate(string token, string gameId, string secret, int maxAgeMinutes)
    {
        var parts = token.Split('.');
        if (parts.Length != 2)
            return false;

        byte[] payloadBytes;
        byte[] providedSignature;
        try
        {
            payloadBytes = Base64UrlDecode(parts[0]);
            providedSignature = Base64UrlDecode(parts[1]);
        }
        catch
        {
            return false;
        }

        var key = Encoding.UTF8.GetBytes(secret);
        var computedSignature = HMACSHA256.HashData(key, payloadBytes);

        if (!CryptographicOperations.FixedTimeEquals(computedSignature, providedSignature))
            return false;

        var payload = Encoding.UTF8.GetString(payloadBytes);
        var lastColon = payload.LastIndexOf(':');
        if (lastColon < 0)
            return false;

        var gameIdPart = payload[..lastColon];
        var timestampPart = payload[(lastColon + 1)..];

        if (gameIdPart != $"play:{gameId}")
            return false;

        if (!long.TryParse(timestampPart, out var timestamp))
            return false;

        var tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        var age = DateTimeOffset.UtcNow - tokenTime;

        return age.TotalMinutes <= maxAgeMinutes;
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var base64 = input.Replace('-', '+').Replace('_', '/');
        var padding = (4 - base64.Length % 4) % 4;
        base64 += new string('=', padding);
        return Convert.FromBase64String(base64);
    }
}
