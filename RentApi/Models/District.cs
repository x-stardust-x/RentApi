namespace RentApi.Models {
    public class District {
        public int Id { get; set; }
        public int CityId { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public int ZipCode { get; set; }
        public City? City { get; set; }
    }
}
