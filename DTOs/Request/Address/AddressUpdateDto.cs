namespace BackEnd_FLOWER_SHOP.DTOs.Request.Address
{
    public class AddressUpdateDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StreetAddress { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }
}