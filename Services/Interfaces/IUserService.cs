using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request.User;
using BackEnd_FLOWER_SHOP.DTOs.Response.User;
using BackEnd_FLOWER_SHOP.Entities;
using Microsoft.AspNetCore.Identity;

namespace BackEnd_FLOWER_SHOP.Services.Interfaces
{
    public interface IUserService
    {
        string GetCurrentUserId();
        public long? GetUserId();
        Task<ApplicationUser> GetByIdAsync(long id);
        Task<ApplicationUser> GetByEmail(string email);
        Task<ApplicationUser> GetByUserName(string username);
        Task<IdentityResult> Create(ApplicationUser user, string password);
        Task<IdentityResult> CreateUserWithRole(ApplicationUser user, string password, string roleName);
        Task<IdentityResult> UpdateUserRoleAsync(string userId, string newRoleName); // New method 
        Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string roleName);
        Task<string> GetRoleAsync(ApplicationUser user);

        Task<UserInfoResponseDto> GetUserInfoByIdAsync(long userId);
        Task<UserInfoResponseDto> UpdateUserInfoAsync(long userId, UpdateUserInfoDto updateUserDto);
        Task<decimal> GetUserLoyaltyPointsAsync(long userId);
        Task<IdentityResult> ChangePasswordAsync(long userId, ChangePasswordDto changePasswordDto);
    }
}