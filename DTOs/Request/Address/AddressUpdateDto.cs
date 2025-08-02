namespace BackEnd_FLOWER_SHOP.DTOs.Request.Address
{
    public class AddressUpdateDTO
    {
        public string FullName { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; } // New property
    }
}