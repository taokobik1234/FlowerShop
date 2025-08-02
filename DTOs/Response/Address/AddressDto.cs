namespace BackEnd_FLOWER_SHOP.DTOs.Response.Address
{
    public class AddressDTO
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public long? ApplicationUserId { get; set; }
    }
}