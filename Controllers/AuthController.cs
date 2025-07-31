using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request;
using BackEnd_FLOWER_SHOP.DTOs.Request.User;
using BackEnd_FLOWER_SHOP.DTOs.Response.User;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using BackEnd_FLOWER_SHOP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [Route("api/")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public AuthController(IUserService usersService, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IConfiguration configuration,
            ApplicationDbContext dbContext) // Add this parameter
        {
            _userService = usersService;
            _signInManager = signInManager;
            _configuration = configuration;
            _dbContext = dbContext; // Initialize this field
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid input data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            var existingUserByEmail = await _userService.GetByEmail(registerDto.Email);
            if (existingUserByEmail != null)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = new List<string> { "Email already exists" }
                });
            }
            var existingUserByUserName = await _userService.GetByUserName(registerDto.UserName);
            if (existingUserByUserName != null)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = new List<string> { "Username already exists" }
                });
            }
            var user = new ApplicationUser
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                PhoneNumber = registerDto.PhoneNumber,
                EmailConfirmed = true, // Set to false if you want email confirmation
                RoleId = 2 // Set default RoleId to 2 (User)
            };
            var result = await _userService.CreateUserWithRole(user, registerDto.Password, "User");
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Registration successful"
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDtoRequest loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid input data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var user = await _userService.GetByEmail(loginDto.Email);

            if (user == null)
            {
                return Unauthorized(new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid credentials",
                    Errors = new List<string> { "User not found" }
                });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (result.Succeeded)
            {
                var token = await GenerateJwtToken(user);
                var refreshToken = await GenerateRefreshToken(user);
                var roles = await _userService.GetRoleAsync(user);

                return Ok(new LoginResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    RefreshToken = refreshToken,
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Role = roles
                    }
                });
            }
            else
            {
                return Unauthorized(new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid credentials",
                    Errors = new List<string> { "Invalid email/username or password" }
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var user = await ValidateRefreshToken(request.RefreshToken);
            if (user == null)
            {
                return Unauthorized(new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid refresh token"
                });
            }

            var newToken = await GenerateJwtToken(user);
            var newRefreshToken = await GenerateRefreshToken(user);
            var roles = await _userService.GetRoleAsync(user);

            return Ok(new LoginResponseDto
            {
                Success = true,
                Message = "Token refreshed successfully",
                Token = newToken,
                RefreshToken = newRefreshToken,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Role = roles
                }
            });
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
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

        private async Task<string> GenerateRefreshToken(ApplicationUser user)
        {
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.UtcNow.AddDays(90), // 90 days for refresh token
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            return refreshToken.Token;
        }

        private async Task<ApplicationUser> ValidateRefreshToken(string refreshToken)
        {
            var storedToken = await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken &&
                                      !rt.IsRevoked &&
                                      rt.ExpiryDate > DateTime.UtcNow);

            if (storedToken != null)
            {
                // Optionally revoke the old token when generating a new one
                storedToken.IsRevoked = true;
                await _dbContext.SaveChangesAsync();
            }

            return storedToken?.User;
        }

        private async Task CleanupExpiredTokens()
        {
            var expiredTokens = await _dbContext.RefreshTokens
                .Where(rt => rt.ExpiryDate < DateTime.UtcNow)
                .ToListAsync();

            _dbContext.RefreshTokens.RemoveRange(expiredTokens);
            await _dbContext.SaveChangesAsync();
        }

        [HttpGet]
        [Authorize] // This route requires a valid JWT token
        [Authorize(Roles = "User")]
        public IActionResult GetProfile()
        {
            return Ok(new { message = "This is a protected route." });
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            var storedToken = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid token" });

            storedToken.IsRevoked = true;
            await _dbContext.SaveChangesAsync();

            return Ok(new ApiResponse { Success = true, Message = "Token revoked successfully" });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            await RevokeToken(request);
            await _signInManager.SignOutAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Logged out successfully"
            });
        }
    }
}