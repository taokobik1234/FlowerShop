namespace BackEnd_FLOWER_SHOP.DTOs.Response.Address
{
    public class AddressDTO
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StreetAddress { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public long? ApplicationUserId { get; set; }
    }
}