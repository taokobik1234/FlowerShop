using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Entities;
using Microsoft.AspNetCore.Identity;

namespace BackEnd_FLOWER_SHOP.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateJwtToken(ApplicationUser user);
        Task<string> GenerateRefreshTokenAsync();
        Task<IdentityResult> SaveRefreshTokenAsync(ApplicationUser user, string refreshToken);
        Task<ApplicationUser?> GetUserByRefreshTokenAsync(string refreshToken);
        Task<IdentityResult> RevokeRefreshTokenAsync(ApplicationUser user);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}