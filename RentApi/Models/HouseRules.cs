using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    [Table("House_Rules")]
    public class HouseRules
    {

        [Key]
        public int Id { get; set; }
        public int? HouseId { get; set; } // 關聯至 Rent_House 表，標記是哪間房屋的規則
        public TimeOnly? SleepTime {  get; set; } // 要求的安靜/熄燈時間基準
        public TimeOnly? WakeTime {  get; set; } // 要求的起床/活動時間基準
        public int? CleanLevel { get; set; } // 要求租客需達到的整潔程度（1 ~ 5）
        public int? NoiseTolerance { get; set; } // 該環境對噪音的限制（1 ~ 5）
        public bool? Pet { get; set; } // 是否接受租客養寵物
        public bool? Smoke { get; set; } // 是否允許室內/陽台抽菸
    }
}
