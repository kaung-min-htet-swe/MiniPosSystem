namespace MiniPos.Frontend.Models;

public class SigninRequestDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class SigninResponseDto
{
    public Guid Id { get; set; }
    public TokenResponse Token { get; set; } = null!;
}

public class TokenResponse
{
    public string AccessToken { get; set; } = null!;
    public DateTimeOffset AccessTokenExpiresAtUtc { get; set; }
    public string RefreshToken { get; set; } = null!;
    public DateTimeOffset RefreshTokenExpiresAtUtc { get; set; }
}
