using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request;
using BackEnd_FLOWER_SHOP.DTOs.Request.User;
using BackEnd_FLOWER_SHOP.DTOs.Response.User;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints require authentication
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user's information
        /// </summary>
        /// <returns>User information</returns>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponse<UserInfoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                var currentUserId = _userService.GetCurrentUserId();
                var userInfo = await _userService.GetUserInfoByIdAsync(long.Parse(currentUserId));

                if (userInfo == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "User not found",
                        Errors = new List<string> { "User information could not be retrieved" }
                    });
                }

                return Ok(new ApiResponse<UserInfoResponseDto>
                {
                    Success = true,
                    Message = "User information retrieved successfully",
                    Data = userInfo
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to get user info");
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "User not authenticated",
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user information");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving user information",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Update current user's information
        /// </summary>
        /// <param name="updateUserDto">User information to update</param>
        /// <returns>Updated user information</returns>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(ApiResponse<UserInfoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UpdateUserInfoDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid input data",
                        Errors = errors
                    });
                }

                var currentUserId = _userService.GetCurrentUserId();
                var updatedUser = await _userService.UpdateUserInfoAsync(long.Parse(currentUserId), updateUserDto);

                if (updatedUser == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "User not found",
                        Errors = new List<string> { "User could not be found for update" }
                    });
                }

                return Ok(new ApiResponse<UserInfoResponseDto>
                {
                    Success = true,
                    Message = "User information updated successfully",
                    Data = updatedUser
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to update user info");
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "User not authenticated",
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument provided for user update");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid data provided",
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user information");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while updating user information",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Get user's loyalty points
        /// </summary>
        /// <returns>User's current loyalty points</returns>
        [HttpGet("loyalty-points")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetLoyaltyPoints()
        {
            try
            {
                var currentUserId = _userService.GetCurrentUserId();
                var loyaltyPoints = await _userService.GetUserLoyaltyPointsAsync(long.Parse(currentUserId));

                return Ok(new ApiResponse<decimal>
                {
                    Success = true,
                    Message = "Loyalty points retrieved successfully",
                    Data = loyaltyPoints
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to get loyalty points");
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "User not authenticated",
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loyalty points");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving loyalty points",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="changePasswordDto">Password change request</param>
        /// <returns>Success message</returns>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid input data",
                        Errors = errors
                    });
                }

                var currentUserId = _userService.GetCurrentUserId();
                var result = await _userService.ChangePasswordAsync(long.Parse(currentUserId), changePasswordDto);

                if (!result.Succeeded)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Password change failed",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Password changed successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to change password");
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "User not authenticated",
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while changing password",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }
    }
}