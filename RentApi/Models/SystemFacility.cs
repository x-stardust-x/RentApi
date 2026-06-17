using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    [Table("System_Facility")]
    public class SystemFacility
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string IconClass { get; set; } = null!;
        public bool IsFilterable { get; set; }
    }
}
