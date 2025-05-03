using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface ITokenService
    {
        Task<bool> IsTokenExpiring(string token);
        Task<string> GenerateAccessToken(IEnumerable<Claim> claims);
        Task<string> GenerateRefreshToken();
        Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string accessToken);
        Task<DateTime> GetRefreshTokenExpiryTime();
        Task<TokenInfo> CreateToken(string userId);
        Task<TokenInfo> GetTokenByRefreshToken(string refreshToken);
        Task<bool> ValidateRefreshToken(string refreshToken);
        Task RevokeToken(string refreshToken);
        Task RevokeAllUserTokens(string userId);

    }
}
