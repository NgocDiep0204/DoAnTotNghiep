using api.Data;
using api.Models;
using api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Functions
{
    public class TokenService : ITokenService
    {
        private readonly ApplicationDbContext _context;

        public TokenService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsTokenRevokedAsync(string token)
        {
            return await _context.RevokedTokens.AnyAsync(rt => rt.Token == token);
        }

        public async Task RevokeTokenAsync(string token, DateTime expiration)
        {
            var revokedToken = new RevokedToken
            {
                Token = token,
                RevokedAt = expiration
            };
            _context.RevokedTokens.Add(revokedToken);
            await _context.SaveChangesAsync();
        }

        
        
    }
}