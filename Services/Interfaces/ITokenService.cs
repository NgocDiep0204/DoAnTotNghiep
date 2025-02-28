namespace api.Services.Interfaces;

public interface ITokenService
{
    Task<bool> IsTokenRevokedAsync(string token);
    Task RevokeTokenAsync(string token, DateTime expiration);
}