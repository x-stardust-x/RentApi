namespace RentApi.Models {
    public class FAQ_Category {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = false;
    }
}
