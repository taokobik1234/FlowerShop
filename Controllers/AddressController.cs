using BackEnd_FLOWER_SHOP.DTOs.Request.Address;
using BackEnd_FLOWER_SHOP.DTOs.Response.Address; // Changed to AddressDTO
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All address endpoints require authentication
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                throw new InvalidOperationException("User ID not found in claims or is invalid.");
            }
            return userId;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AddressDTO), StatusCodes.Status200OK)] // Changed
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAddressById(long id)
        {
            var address = await _addressService.GetAddressByIdAsync(id);
            if (address == null)
            {
                return NotFound($"Address with ID {id} not found.");
            }
            // Ensure the retrieved address belongs to the authenticated user or if the user is an Admin
            var currentUserId = GetUserId();
            if (address.ApplicationUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid("You do not have permission to access this address.");
            }
            return Ok(address);
        }

        [HttpGet("user")]
        [ProducesResponseType(typeof(List<AddressDTO>), StatusCodes.Status200OK)] // Changed
        public async Task<IActionResult> GetUserAddresses()
        {
            var userId = GetUserId();
            var addresses = await _addressService.GetUserAddressesAsync(userId);
            return Ok(addresses);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AddressDTO), StatusCodes.Status201Created)] // Changed
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAddress([FromBody] AddressCreateDTO addressDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetUserId();
                var createdAddress = await _addressService.CreateAddressAsync(userId, addressDto);
                return CreatedAtAction(nameof(GetAddressById), new { id = createdAddress.Id }, createdAddress);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AddressDTO), StatusCodes.Status200OK)] // Changed
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateAddress(long id, [FromBody] AddressUpdateDTO addressDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingAddress = await _addressService.GetAddressByIdAsync(id);
            if (existingAddress == null)
            {
                return NotFound($"Address with ID {id} not found.");
            }

            // Ensure the address being updated belongs to the authenticated user
            var currentUserId = GetUserId();
            if (existingAddress.ApplicationUserId != currentUserId)
            {
                return Forbid("You do not have permission to update this address.");
            }

            var updatedAddress = await _addressService.UpdateAddressAsync(id, addressDto);
            if (updatedAddress == null)
            {
                return NotFound(); // Should not happen if existingAddress was found
            }
            return Ok(updatedAddress);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteAddress(long id)
        {
            var existingAddress = await _addressService.GetAddressByIdAsync(id);
            if (existingAddress == null)
            {
                return NotFound($"Address with ID {id} not found.");
            }

            // Ensure the address being deleted belongs to the authenticated user
            var currentUserId = GetUserId();
            if (existingAddress.ApplicationUserId != currentUserId)
            {
                return Forbid("You do not have permission to delete this address.");
            }

            var deleted = await _addressService.DeleteAddressAsync(id);
            if (!deleted)
            {
                return NotFound(); // Should not happen if existingAddress was found
            }
            return NoContent();
        }
    }
}