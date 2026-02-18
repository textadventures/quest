using System.Security.Cryptography;
using System.Text;
using QuestViva.WebPlayer;

namespace QuestViva.WebPlayerTests;

[TestClass]
public class PlayTokenValidatorTests
{
    private const string Secret = "test-secret";
    private const string GameId = "abc123";

    private static string CreateToken(string gameId, DateTimeOffset timestamp, string secret)
    {
        var payload = $"play:{gameId}:{timestamp.ToUnixTimeSeconds()}";
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var key = Encoding.UTF8.GetBytes(secret);
        var signature = HMACSHA256.HashData(key, payloadBytes);
        return $"{Base64UrlEncode(payloadBytes)}.{Base64UrlEncode(signature)}";
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');

    [TestMethod]
    public void ValidToken_ReturnsTrue()
    {
        var token = CreateToken(GameId, DateTimeOffset.UtcNow.AddMinutes(-5), Secret);
        Assert.IsTrue(PlayTokenValidator.Validate(token, GameId, Secret, 60));
    }

    [TestMethod]
    public void TokenJustWithinMaxAge_ReturnsTrue()
    {
        var token = CreateToken(GameId, DateTimeOffset.UtcNow.AddMinutes(-59), Secret);
        Assert.IsTrue(PlayTokenValidator.Validate(token, GameId, Secret, 60));
    }

    [TestMethod]
    public void ExpiredToken_ReturnsFalse()
    {
        var token = CreateToken(GameId, DateTimeOffset.UtcNow.AddMinutes(-61), Secret);
        Assert.IsFalse(PlayTokenValidator.Validate(token, GameId, Secret, 60));
    }

    [TestMethod]
    public void WrongGameId_ReturnsFalse()
    {
        var token = CreateToken("other-game", DateTimeOffset.UtcNow.AddMinutes(-5), Secret);
        Assert.IsFalse(PlayTokenValidator.Validate(token, GameId, Secret, 60));
    }

    [TestMethod]
    public void WrongSecret_ReturnsFalse()
    {
        var token = CreateToken(GameId, DateTimeOffset.UtcNow.AddMinutes(-5), "wrong-secret");
        Assert.IsFalse(PlayTokenValidator.Validate(token, GameId, Secret, 60));
    }

    [TestMethod]
    public void TamperedSignature_ReturnsFalse()
    {
        var token = CreateToken(GameId, DateTimeOffset.UtcNow.AddMinutes(-5), Secret);
        var parts = token.Split('.');
        var tampered = parts[0] + ".AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        Assert.IsFalse(PlayTokenValidator.Validate(tampered, GameId, Secret, 60));
    }

    [TestMethod]
    public void TamperedPayload_ReturnsFalse()
    {
        var token = CreateToken(GameId, DateTimeOffset.UtcNow.AddMinutes(-5), Secret);
        var parts = token.Split('.');
        var fakePayload = Base64UrlEncode(Encoding.UTF8.GetBytes($"play:{GameId}:9999999999"));
        var tampered = fakePayload + "." + parts[1];
        Assert.IsFalse(PlayTokenValidator.Validate(tampered, GameId, Secret, 60));
    }

    [TestMethod]
    public void NoDotSeparator_ReturnsFalse()
    {
        Assert.IsFalse(PlayTokenValidator.Validate("notavalidtoken", GameId, Secret, 60));
    }

    [TestMethod]
    public void InvalidBase64_ReturnsFalse()
    {
        Assert.IsFalse(PlayTokenValidator.Validate("not-valid-base64!.not-valid-base64!", GameId, Secret, 60));
    }

    [TestMethod]
    public void GameIdWithColon_ValidToken_ReturnsTrue()
    {
        // Game IDs that contain colons should still work because we split on the last colon
        const string colonGameId = "foo:bar";
        var token = CreateToken(colonGameId, DateTimeOffset.UtcNow.AddMinutes(-5), Secret);
        Assert.IsTrue(PlayTokenValidator.Validate(token, colonGameId, Secret, 60));
    }
}
