using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Data;
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
        private readonly RoleManager<ApplicationRole> _roleManager; // Added this line

        public UserService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            RoleManager<ApplicationRole> roleManager // Added this parameter to the constructor
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _roleManager = roleManager; // Assigned the injected RoleManager
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

            // Original code using _dbContext.Roles with user.RoleId might be problematic
            // if roles are managed through IdentityUserRoles and not directly via ApplicationUser.RoleId.
            // The GetRolesAsync method of UserManager is the standard way.
            // if (user == null || user.RoleId == null)
            //     return null;
            // var role = await _dbContext.Roles
            //     .Where(r => r.Id == user.RoleId)
            //     .Select(r => r.Name)
            //     .FirstOrDefaultAsync();
            // return role;
        }
    }
}