using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Database.EfAppDbContextModels;
using Microsoft.IdentityModel.Tokens;

namespace MiniPos.Backend.Features.Auth;

public class TokenService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _db;

    public TokenService(IConfiguration configuration, AppDbContext db)
    {
        _configuration = configuration;
        _db = db;
    }

    public async Task<TokenResponse> IssueTokenAsync(Guid userId, string username, string role, IEnumerable<Claim>? extraClaims = null)
    {
        var accessMinutes = int.Parse(_configuration["Jwt:AccessTokenMinutes"]!);
        var refreshDays = int.Parse(_configuration["Jwt:RefreshTokenDays"]!);
        var now = DateTime.Now;
        var accessExpires = now.AddMinutes(accessMinutes);
        var refreshExpires = now.AddDays(refreshDays);

        var accessToken = CreateAccessToken(userId, username, role, accessExpires, extraClaims);
        var refreshToken = GenerateSecureToken();
        var refreshHash = HashToken(refreshToken);

        return new TokenResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = accessExpires,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = refreshExpires
        };
    }

    // public async Task<TokenResponse> RefreshAsync(string refreshToken)
    // {
    //     var refreshHash = HashToken(refreshToken);
    //     var tokenRecord = await _db.RefreshTokens.FindAsync(refreshHash);
    //     if (tokenRecord == null || tokenRecord.ExpiresAtUtc < DateTime.UtcNow)
    //         throw new Exception("Invalid or expired refresh token");
    //
    //     var username = tokenRecord.Username;
    //     var extraClaims = tokenRecord.Claims != null
    //         ? System.Text.Json.JsonSerializer.Deserialize<List<Claim>>(tokenRecord.Claims)
    //         : null;
    //
    //     // Invalidate the used refresh token
    //     _db.RefreshTokens.Remove(tokenRecord);
    //     await _db.SaveChangesAsync();
    //
    //     return await IssueTokenAsync(username, extraClaims);
    // }
    //
    private string CreateAccessToken(Guid userId, string username, string role, DateTimeOffset expiresAtUtc, IEnumerable<Claim>? extraClaims)
    {
        var issuer = _configuration["Jwt:Issuer"]!;
        var audience = _configuration["Jwt:Audience"]!;
        var key = _configuration["Jwt:Key"]!;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.Name, userId.ToString()),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (extraClaims != null)
            claims.AddRange(extraClaims);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            DateTime.UtcNow,
            expiresAtUtc.UtcDateTime,
            creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToHexString(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}

public class TokenResponse
{
    public string AccessToken { get; set; } = "";
    public DateTimeOffset AccessTokenExpiresAtUtc { get; set; }

    public string RefreshToken { get; set; } = "";
    public DateTimeOffset RefreshTokenExpiresAtUtc { get; set; }
}