using BackEnd_FLOWER_SHOP.DTOs.Request.Address;
using BackEnd_FLOWER_SHOP.DTOs.Response.Address; // Changed to AddressDTO
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Services.Interfaces
{
    public interface IAddressService
    {
        Task<AddressDTO> GetAddressByIdAsync(long addressId); // Changed
        Task<List<AddressDTO>> GetUserAddressesAsync(long userId); // Changed
        Task<AddressDTO> CreateAddressAsync(long userId, AddressCreateDTO addressDto);
        Task<AddressDTO> UpdateAddressAsync(long addressId, AddressUpdateDTO addressDto);
        Task<bool> DeleteAddressAsync(long addressId);
    }
}