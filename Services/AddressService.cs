using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.DTOs.Request.Address;
using BackEnd_FLOWER_SHOP.DTOs.Response.Address; // Changed to AddressDTO
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Services
{
    public class AddressService : IAddressService
    {
        private readonly ApplicationDbContext _context;

        public AddressService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AddressDTO> GetAddressByIdAsync(long addressId) // Changed
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null)
            {
                return null;
            }

            return new AddressDTO // Changed
            {
                Id = address.Id,
                FirstName = address.FirstName,
                LastName = address.LastName,
                StreetAddress = address.StreetAddress,
                Country = address.Country,
                City = address.City,
                ZipCode = address.ZipCode,
                ApplicationUserId = address.ApplicationUserId
            };
        }

        public async Task<List<AddressDTO>> GetUserAddressesAsync(long userId) // Changed
        {
            var addresses = await _context.Addresses
                .Where(a => a.ApplicationUserId == userId)
                .ToListAsync();

            return addresses.Select(address => new AddressDTO // Changed
            {
                Id = address.Id,
                FirstName = address.FirstName,
                LastName = address.LastName,
                StreetAddress = address.StreetAddress,
                Country = address.Country,
                City = address.City,
                ZipCode = address.ZipCode,
                ApplicationUserId = address.ApplicationUserId
            }).ToList();
        }

        public async Task<AddressDTO> CreateAddressAsync(long userId, AddressCreateDTO addressDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            var address = new Address
            {
                FirstName = addressDto.FirstName,
                LastName = addressDto.LastName,
                StreetAddress = addressDto.StreetAddress,
                Country = addressDto.Country,
                City = addressDto.City,
                ZipCode = addressDto.ZipCode,
                ApplicationUserId = userId
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return new AddressDTO // Changed
            {
                Id = address.Id,
                FirstName = address.FirstName,
                LastName = address.LastName,
                StreetAddress = address.StreetAddress,
                Country = address.Country,
                City = address.City,
                ZipCode = address.ZipCode,
                ApplicationUserId = address.ApplicationUserId
            };
        }

        public async Task<AddressDTO> UpdateAddressAsync(long addressId, AddressUpdateDTO addressDto)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null)
            {
                return null;
            }

            address.FirstName = addressDto.FirstName;
            address.LastName = addressDto.LastName;
            address.StreetAddress = addressDto.StreetAddress;
            address.Country = addressDto.Country;
            address.City = addressDto.City;
            address.ZipCode = addressDto.ZipCode;

            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();

            return new AddressDTO // Changed
            {
                Id = address.Id,
                FirstName = address.FirstName,
                LastName = address.LastName,
                StreetAddress = address.StreetAddress,
                Country = address.Country,
                City = address.City,
                ZipCode = address.ZipCode,
                ApplicationUserId = address.ApplicationUserId
            };
        }

        public async Task<bool> DeleteAddressAsync(long addressId)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null)
            {
                return false;
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}