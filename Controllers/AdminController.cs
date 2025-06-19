// Controllers/AdminController.cs
using BackEnd_FLOWER_SHOP.DTOs.Request.User;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [Route("api/admin")]
    [Authorize(Roles = "Admin")] // Only users with the "Admin" role can access this controller
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AdminController(IUserService userService, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userService = userService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("register-new-user")]
        public async Task<IActionResult> RegisterNewUser([FromBody] AdminRegisterUserDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUserByEmail = await _userService.GetByEmail(registerDto.Email);
            if (existingUserByEmail != null)
            {
                return BadRequest("Email already exists.");
            }

            var existingUserByUserName = await _userService.GetByUserName(registerDto.UserName);
            if (existingUserByUserName != null)
            {
                return BadRequest("Username already exists.");
            }

            // Check if the role exists
            var roleExists = await _roleManager.RoleExistsAsync(registerDto.RoleName);
            if (!roleExists)
            {
                return BadRequest($"Role '{registerDto.RoleName}' does not exist.");
            }

            var user = new ApplicationUser
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                PhoneNumber = registerDto.PhoneNumber,
                EmailConfirmed = true,
            };

            var result = await _userService.CreateUserWithRole(user, registerDto.Password, registerDto.RoleName);

            if (!result.Succeeded)
            {
                return StatusCode(500, new { message = "Failed to register user.", errors = result.Errors.Select(e => e.Description) });
            }

            return Ok("User registered successfully with the specified role.");
        }

        // Example: Admin can get all users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.UserName,
                u.PhoneNumber,
                // You might want to get roles here too, but it requires another call per user
            }).ToList();

            // To get roles for each user
            var usersWithRoles = new List<object>();
            foreach (var user in _userManager.Users.ToList())
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.UserName,
                    user.PhoneNumber,
                    Roles = roles
                });
            }


            return Ok(usersWithRoles);
        }
        [HttpPost("update-user-role")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.UpdateUserRoleAsync(updateDto.UserId, updateDto.NewRoleName);

            if (result.Succeeded)
            {
                return Ok($"User role updated successfully for User ID: {updateDto.UserId} to role: {updateDto.NewRoleName}.");
            }
            else
            {
                return BadRequest(new { message = "Failed to update user role.", errors = result.Errors.Select(e => e.Description) });
            }
        }
    }
}
// sample to add to any controller to change access level from all user to only admin
// // Example: In your ProductController.cs (hypothetical)
// [Route("api/[controller]")]
// [ApiController]
// public class ProductController : ControllerBase
// {
//     // ... constructor and other methods

//     [HttpGet]
//     [Authorize(Roles = "Admin")] // Now only Admin can access this
//     public IActionResult GetProducts()
//     {
//         // ... logic to get products
//         return Ok("List of products (admin-only).");
//     }

//     [HttpGet("{id}")]
//     [AllowAnonymous] // This one can still be public
//     public IActionResult GetProductById(long id)
//     {
//         // ... logic
//         return Ok($"Product with ID {id}.");
//     }
// }