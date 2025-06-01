using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;
        // add dynamic settings
        private readonly IDynamicSettingsService _dynamicSettings;
        public TokenService(IUnitOfWork unitOfWork, IConfiguration configuration, IOptions<JwtSettings> jwtSettings, IDynamicSettingsService dynamicSettings)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings.Value;
            _dynamicSettings = dynamicSettings;
        }

        public async Task<string> GenerateAccessToken(IEnumerable<Claim> claims)
        {
            _jwtSettings = await _dynamicSettings.GetSectionAsync<JwtSettings>("JwtSettings");
            var tokenHandler = new JwtSecurityTokenHandler();

            // Create a symmetric security key using the secret key from the configuration.
            //var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            //var signingCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256);
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var tokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtSettings.Issuer,//_configuration["JwtSettings:Issuer"],
                Audience = _jwtSettings.Audience,//_configuration["JwtSettings:Audience"],
                Subject = new ClaimsIdentity(claims),
                Expires = tokenExpiration,
                SigningCredentials = signingCredentials
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            return jwtToken;
        }
        public async Task<string> GenerateRefreshToken()
        {
            // Create a 32-byte array to hold cryptographically secure random bytes
            var randomNumber = new byte[32];

            // Use a cryptographically secure random number generator 
            // to fill the byte array with random values
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);

            // Convert the random bytes to a base64 encoded string 
            return Convert.ToBase64String(randomNumber);
        }
        public async Task<TokenInfo> GetTokenByRefreshToken(string refreshToken)
        {
            return await _unitOfWork.Tokens.GetTokenByRefreshToken(refreshToken);
        }
        public async Task<TokenInfo> CreateToken(string userId)
        {
            // First revoke any existing tokens for this user
            var existingToken = await _unitOfWork.Tokens.GetTokenByUserId(userId);
            if (existingToken != null)
            {
                await _unitOfWork.Tokens.RevokeToken(existingToken.RefreshToken);
            }

            // Create new token
            var refreshToken = await GenerateRefreshToken();
            var expiryTime = await GetRefreshTokenExpiryTime();

            var tokenInfo = new TokenInfo
            {
                UserId = userId,
                RefreshToken = refreshToken,
                ExpiryDate = expiryTime,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Tokens.Add(tokenInfo, log => Console.WriteLine(log));
            await _unitOfWork.SaveAsync();
            return tokenInfo;
        }
        public async Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string accessToken)
        {
            _jwtSettings = await _dynamicSettings.GetSectionAsync<JwtSettings>("JwtSettings");
            //var signing = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var signing = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            // Define the token validation parameters used to validate the token.
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,//_configuration["JwtSettings:Audience"],
                ValidIssuer = _jwtSettings.Issuer,//_configuration["JwtSettings:Issuer"],
                ValidateLifetime = false, // don't forget to add this line.
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = signing
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            // Validate the token and extract the claims principal and the security token.
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);

            // Cast the security token to a JwtSecurityToken for further validation.
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            // Ensure the token is a valid JWT and uses the HmacSha256 signing algorithm.
            // If no throw new SecurityTokenException
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            // return the principal
            return principal;
        }
        public async Task<DateTime> GetRefreshTokenExpiryTime()
        {
            _jwtSettings = await _dynamicSettings.GetSectionAsync<JwtSettings>("JwtSettings");
            //int expiryDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpiryDays", 7);
            int expiryDays = _jwtSettings.RefreshTokenExpiryDays;
            return DateTime.UtcNow.AddDays(expiryDays);
        }
        public async Task<bool> ValidateRefreshToken(string refreshToken)
        {
            return await _unitOfWork.Tokens.IsTokenValid(refreshToken);
        }
        public async Task RevokeToken(string refreshToken)
        {
            await _unitOfWork.Tokens.RevokeToken(refreshToken);
            await _unitOfWork.SaveAsync();

        }
        public async Task RevokeAllUserTokens(string userId)
        {
            var token = await _unitOfWork.Tokens.GetTokenByUserId(userId);
            if (token != null)
            {
                await _unitOfWork.Tokens.RevokeToken(token.RefreshToken);
            }
            await _unitOfWork.SaveAsync();
        }
        public async Task<bool> IsTokenExpiring(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Return true if token expires in less than 5 minutes
                return jwtToken.ValidTo < DateTime.UtcNow.AddMinutes(1);
            }
            catch
            {
                return true; // Invalid tokens are considered expired
            }
        }
    }
}
