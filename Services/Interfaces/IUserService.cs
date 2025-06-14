using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Entities;
using Microsoft.AspNetCore.Identity;

namespace BackEnd_FLOWER_SHOP.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUser> GetByIdAsync(long id);
        Task<ApplicationUser> GetByEmail(string email);
        Task<ApplicationUser> GetByUserName(string username);
        Task<IdentityResult> Create(ApplicationUser user, string password);
        Task<string> GetRoleAsync(ApplicationUser user);
    }
}