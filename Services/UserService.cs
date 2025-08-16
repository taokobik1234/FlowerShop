using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.DTOs.Request.User;
using BackEnd_FLOWER_SHOP.DTOs.Response.User;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEnd_FLOWER_SHOP.Services
{
    public class UserService : IUserService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<UserService> _logger;

        public UserService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            RoleManager<ApplicationRole> roleManager,
            ILogger<UserService> logger
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _roleManager = roleManager;
            _logger = logger;
        }

        public string GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }

        public long? GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        public async Task<IdentityResult> Create(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<ApplicationUser> GetByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public Task<ApplicationUser> GetByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string roleName)
        {
            // Validate inputs
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "User cannot be null"
                });
            }

            if (string.IsNullOrWhiteSpace(roleName))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Role name cannot be empty"
                });
            }

            // Check if role exists
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"Role '{roleName}' does not exist"
                });
            }

            // Add user to role
            return await _userManager.AddToRoleAsync(user, roleName);
        }
        public async Task<ApplicationUser> GetByUserName(string username)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(username);
            return user;
        }

        public async Task<IdentityResult> CreateUserWithRole(ApplicationUser user, string password, string roleName)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                // Check if the role exists before creating.
                // It's generally better to seed roles at application startup (e.g., in Program.cs or a DbInitializer).
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new ApplicationRole(roleName));
                }
                await _userManager.AddToRoleAsync(user, roleName);
            }
            return result;
        }

        public async Task<string> GetRoleAsync(ApplicationUser user)
        {
            if (user == null)
                return null;

            // This is a more robust way to get roles using UserManager
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }

        // New method to update user role
        public async Task<IdentityResult> UpdateUserRoleAsync(string userId, string newRoleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            // Check if the new role exists
            if (!await _roleManager.RoleExistsAsync(newRoleName))
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Role '{newRoleName}' does not exist." });
            }

            // Get current roles of the user
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove user from all current roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Failed to remove user from existing roles." });
            }

            // Add user to the new role
            var addResult = await _userManager.AddToRoleAsync(user, newRoleName);
            if (!addResult.Succeeded)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Failed to add user to new role." });
            }

            return IdentityResult.Success;
        }

        public async Task<UserInfoResponseDto> GetUserInfoByIdAsync(long userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return null;
                }

                var role = await GetRoleAsync(user);

                return new UserInfoResponseDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    LoyaltyPoints = user.LoyaltyPoints,
                    Role = role,
                    EmailConfirmedAt = user.EmailConfirmed ? DateTime.UtcNow : null, // You might want to track this separately
                    CreatedAt = DateTime.UtcNow, // You might want to add this to ApplicationUser entity
                    UpdatedAt = DateTime.UtcNow  // You might want to add this to ApplicationUser entity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user info for user ID: {userId}");
                throw;
            }
        }

        public async Task<UserInfoResponseDto> UpdateUserInfoAsync(long userId, UpdateUserInfoDto updateUserDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return null;
                }

                // Update user properties
                user.FirstName = updateUserDto.FirstName;
                user.LastName = updateUserDto.LastName;
                user.PhoneNumber = updateUserDto.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    throw new ArgumentException($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                _logger.LogInformation($"User info updated successfully for user ID: {userId}");
                return await GetUserInfoByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user info for user ID: {userId}");
                throw;
            }
        }

        public async Task<decimal> GetUserLoyaltyPointsAsync(long userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                return user?.LoyaltyPoints ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving loyalty points for user ID: {userId}");
                throw;
            }
        }

        public async Task<IdentityResult> ChangePasswordAsync(long userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "User not found" });
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Password changed successfully for user ID: {userId}");
                }
                else
                {
                    _logger.LogWarning($"Password change failed for user ID: {userId}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for user ID: {userId}");
                throw;
            }
        }
    }
}