using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;

namespace TechXpress.Data.Repositories.Base
{
    public interface ITokenRepo : IRepository<TokenInfo>
    {
        Task<TokenInfo> GetTokenByUserId(string userId);
        Task<TokenInfo> GetTokenByRefreshToken(string refreshToken);
        Task<bool> IsTokenValid(string refreshToken);
        Task<bool> IsTokenExpired(string refreshToken);
        Task<bool> IsTokenRevoked(string refreshToken);
        Task<bool> IsTokenUsed(string refreshToken);
        Task RevokeToken(string refreshToken);
    }
}
