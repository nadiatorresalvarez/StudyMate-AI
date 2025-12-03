using System.Security.Claims;
using System.Text.Json;

namespace StudyMateAI.Client.Auth;

/// <summary>
/// Utilidad para parsear claims desde un JWT Token
/// </summary>
public static class JwtParser
{
    /// <summary>
    /// Extrae y parsea los claims de un JWT
    /// </summary>
    /// <param name="jwt">Token JWT completo</param>
    /// <returns>Enumeración de claims extraídos del payload</returns>
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs == null)
                return Enumerable.Empty<Claim>();

            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty));
        }
        catch
        {
            return Enumerable.Empty<Claim>();
        }
    }

    /// <summary>
    /// Decodifica una cadena Base64 sin padding
    /// </summary>
    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }
        return Convert.FromBase64String(base64);
    }

    /// <summary>
    /// Obtiene un claim específico del JWT
    /// </summary>
    public static string GetClaim(string jwt, string claimType)
    {
        var claims = ParseClaimsFromJwt(jwt);
        var claim = claims.FirstOrDefault(c => c.Type == claimType);
        return claim?.Value ?? string.Empty;
    }

    /// <summary>
    /// Verifica si el JWT está expirado
    /// </summary>
    public static bool IsTokenExpired(string jwt)
    {
        try
        {
            var expClaim = GetClaim(jwt, "exp");
            if (string.IsNullOrEmpty(expClaim))
                return true;

            if (long.TryParse(expClaim, out var timestamp))
            {
                var expiryDateTime = UnixTimeStampToDateTime(timestamp);
                return DateTime.UtcNow >= expiryDateTime;
            }

            return true;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// Convierte un timestamp Unix a DateTime
    /// </summary>
    private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }
}
