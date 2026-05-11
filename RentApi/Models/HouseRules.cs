using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    [Table("House_Rules")]
    public class HouseRules
    {
        [Key]
        public int? Id { get; set; }
        public int? HouseId { get; set; } // 關聯至 Rent_House 表，標記是哪間房屋的規則
        public int SleepTime { get; set; } // 熄燈時間
        public int WakeTime { get; set; } // 起床時間
        public int CleanLevel { get; set; } // 需達到整潔程度
        public int NoiseTolerance { get; set; } // 噪音程度
        public bool? Pet { get; set; } // 是否可養寵物
        public bool? Smoke { get; set; } // 是否允許室內/陽台抽菸

    }
}
