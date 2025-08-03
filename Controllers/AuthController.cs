using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request;
using BackEnd_FLOWER_SHOP.DTOs.Request.Token;
using BackEnd_FLOWER_SHOP.DTOs.Request.User;
using BackEnd_FLOWER_SHOP.DTOs.Response.User;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [Route("api/")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _itokenService;
        public AuthController(IUserService usersService,
            SignInManager<ApplicationUser> signInManager, ITokenService tokenService)
        {
            _userService = usersService;
            _signInManager = signInManager;
            _itokenService = tokenService;
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
                var token = await _itokenService.GenerateJwtToken(user);
                var refreshToken = await _itokenService.GenerateRefreshTokenAsync();
                var roles = await _userService.GetRoleAsync(user);
                await _itokenService.SaveRefreshTokenAsync(user, refreshToken);
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
        [AllowAnonymous]
        public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new RefreshTokenResponseDto
                {
                    Success = false,
                    Message = "Invalid input data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            try
            {
                // Get principal from expired token
                var principal = _itokenService.GetPrincipalFromExpiredToken(request.AccessToken);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new RefreshTokenResponseDto
                    {
                        Success = false,
                        Message = "Invalid token",
                        Errors = new List<string> { "User ID not found in token" }
                    });
                }

                // Get user by refresh token
                var user = await _itokenService.GetUserByRefreshTokenAsync(request.RefreshToken);

                if (user == null || user.Id.ToString() != userId)
                {
                    return Unauthorized(new RefreshTokenResponseDto
                    {
                        Success = false,
                        Message = "Invalid refresh token",
                        Errors = new List<string> { "Refresh token is invalid or expired" }
                    });
                }

                // Generate new tokens
                var newAccessToken = await _itokenService.GenerateJwtToken(user);
                var newRefreshToken = await _itokenService.GenerateRefreshTokenAsync();

                // Save new refresh token
                var saveResult = await _itokenService.SaveRefreshTokenAsync(user, newRefreshToken);

                if (!saveResult.Succeeded)
                {
                    return StatusCode(500, new RefreshTokenResponseDto
                    {
                        Success = false,
                        Message = "Failed to save refresh token",
                        Errors = saveResult.Errors.Select(e => e.Description).ToList()
                    });
                }

                return Ok(new RefreshTokenResponseDto
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized(new RefreshTokenResponseDto
                {
                    Success = false,
                    Message = "Invalid access token",
                    Errors = new List<string> { "Access token is invalid" }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RefreshTokenResponseDto
                {
                    Success = false,
                    Message = "An error occurred while refreshing token",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {

            try
            {
                await _signInManager.SignOutAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Logout successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred during logout",
                    Errors = new List<string> { ex.Message }
                });
            }
        }


    }
}