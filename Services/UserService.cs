using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEnd_FLOWER_SHOP.Services
{
    public class UserService : IUserService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        public UserService(
       SignInManager<ApplicationUser> signInManager,
       UserManager<ApplicationUser> userManager,
       ApplicationDbContext dbContext
   )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dbContext = dbContext;
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

        public async Task<string> GetRoleAsync(ApplicationUser user)
        {
            if (user == null || user.RoleId == null)
                return null;

            var role = await _dbContext.Roles
                .Where(r => r.Id == user.RoleId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync();

            return role;
        }
    }
}