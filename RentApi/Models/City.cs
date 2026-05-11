namespace RentApi.Models {
    public class City {
        public int Id { get; set; }
        public string CityName { get; set; } = string.Empty;
        public List<District> Districts { get; set; } = new();
    }
}
