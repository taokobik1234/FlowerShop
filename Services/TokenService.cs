using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
namespace BackEnd_FLOWER_SHOP.Services
{
    public class TokenService : ITokenService
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        public TokenService(IUserService usersService, IConfiguration configuration, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userService = usersService;
            _configuration = configuration;
            _userManager = userManager;
            _dbContext = dbContext;
        }
        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var role = await _userService.GetRoleAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),

            };

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddDays(Convert.ToDouble(_configuration["JWT:TokenValidityInDays"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<IdentityResult> SaveRefreshTokenAsync(ApplicationUser user, string refreshToken)
        {
            try
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days expiry

                var result = await _userManager.UpdateAsync(user);
                return result;
            }
            catch (Exception)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Failed to save refresh token" });
            }
        }

        public async Task<ApplicationUser?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken &&
                                        u.RefreshTokenExpiryTime > DateTime.UtcNow);
        }

        public async Task<IdentityResult> RevokeRefreshTokenAsync(ApplicationUser user)
        {
            try
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = DateTime.UtcNow; // Set to past date

                var result = await _userManager.UpdateAsync(user);
                return result;
            }
            catch (Exception)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Failed to revoke refresh token" });
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                ValidateLifetime = false // Don't validate lifetime for expired tokens
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}