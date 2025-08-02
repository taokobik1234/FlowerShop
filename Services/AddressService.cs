using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.DTOs.Request.Address;
using BackEnd_FLOWER_SHOP.DTOs.Response.Address;
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

        public async Task<AddressDTO> GetAddressByIdAsync(long addressId)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null)
            {
                return null;
            }

            return new AddressDTO
            {
                Id = address.Id,
                FullName = address.FullName,
                StreetAddress = address.StreetAddress,
                City = address.City,
                PhoneNumber = address.PhoneNumber,
                ApplicationUserId = address.ApplicationUserId
            };
        }

        public async Task<List<AddressDTO>> GetUserAddressesAsync(long userId)
        {
            var addresses = await _context.Addresses
                .Where(a => a.ApplicationUserId == userId)
                .ToListAsync();

            return addresses.Select(address => new AddressDTO
            {
                Id = address.Id,
                FullName = address.FullName,
                StreetAddress = address.StreetAddress,
                City = address.City,
                PhoneNumber = address.PhoneNumber,
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
                FullName = addressDto.FullName,
                StreetAddress = addressDto.StreetAddress,
                City = addressDto.City,
                PhoneNumber = addressDto.PhoneNumber,
                ApplicationUserId = userId
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return new AddressDTO
            {
                Id = address.Id,
                FullName = address.FullName,
                StreetAddress = address.StreetAddress,
                City = address.City,
                PhoneNumber = address.PhoneNumber,
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

            address.FullName = addressDto.FullName;
            address.StreetAddress = addressDto.StreetAddress;
            address.City = addressDto.City;
            address.PhoneNumber = addressDto.PhoneNumber;

            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();

            return new AddressDTO
            {
                Id = address.Id,
                FullName = address.FullName,
                StreetAddress = address.StreetAddress,
                City = address.City,
                PhoneNumber = address.PhoneNumber,
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