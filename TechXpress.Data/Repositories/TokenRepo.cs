
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;

namespace TechXpress.Data.Repositories
{
    public class TokenRepo : Repository<TokenInfo>, ITokenRepo
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _dp;
        public TokenRepo(AppDbContext dp, ILogger<Repository<TokenInfo>> logger) : base(dp, logger)
        {
            _dp = dp;
            _logger = logger;
        }

        public async Task<TokenInfo> GetTokenByUserId(string userId) => await _dp.TokenInfos
                .FirstOrDefaultAsync(t => t.UserId == userId);

        public async Task<TokenInfo> GetTokenByRefreshToken(string refreshToken) => await _dp.TokenInfos
                .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken);


        public async Task<bool> IsTokenValid(string refreshToken)
        {
            var token = await GetTokenByRefreshToken(refreshToken);
            return token != null && !token.IsRevoked && token.ExpiryDate > DateTime.UtcNow;
        }

        public async Task<bool> IsTokenExpired(string refreshToken)
        {
            var token = await GetTokenByRefreshToken(refreshToken);
            return token != null && token.ExpiryDate <= DateTime.UtcNow;
        }

        public async Task<bool> IsTokenRevoked(string refreshToken)
        {
            var token = await GetTokenByRefreshToken(refreshToken);
            return token != null && token.IsRevoked;
        }

        public async Task<bool> IsTokenUsed(string refreshToken)
        {
            return await _dp.TokenInfos.AnyAsync(t => t.RefreshToken == refreshToken);
        }

        public async Task RevokeToken(string refreshToken)
        {
            var token = await GetTokenByRefreshToken(refreshToken);
            if (token != null)
            {
                token.IsRevoked = true;
                await _dp.SaveChangesAsync();
            }
        }

        public async Task Add(TokenInfo tokenInfo)
        {
            await _dp.TokenInfos.AddAsync(tokenInfo);
            await _dp.SaveChangesAsync();
        }

        public async Task Update(TokenInfo tokenInfo)
        {
            _dp.TokenInfos.Update(tokenInfo);
            await _dp.SaveChangesAsync();
        }

        public async Task Delete(TokenInfo tokenInfo)
        {
            _dp.TokenInfos.Remove(tokenInfo);
            await _dp.SaveChangesAsync();
        }
    }
}
