using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    // 💡 請注意：[Table("這裡要填寫你資料庫真正的資料表名稱")] 
    // 根據你的圖片右側頁籤，如果是 HouseViewing_Order 或 Viewing_Order，請記得修正括號內的字串
    [Table("HouseViewing")]
    public class HouseViewing
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ReservationNo { get; set; } // ⭕ 解決 CS0117

        public int HouseId { get; set; }

        public int LesseId { get; set; } // ⭕ 解決 CS0117 (單個 e)

        public int LessorId { get; set; }

        public DateTime ViewingTime { get; set; } // ⭕ 解決 CS0117

        public DateTime ExpectedMoveIn { get; set; }

        public string Message { get; set; }

        public int MatchScore { get; set; }

        public int Status { get; set; } // ⭕ 權限修正：改成 int！解決 CS0029

        public string RejectReason { get; set; }

        public DateTime CreatedAt { get; set; } // ⭕ 解決 CS0117

        public DateTime UpdatedAt { get; set; } // ⭕ 解決 CS0117

        // ==========================================
        // 導覽屬性 (Navigation Property) - 用於 JOIN 房源表
        // ==========================================
        [ForeignKey("HouseId")]
        public virtual Rent_House RentHouse { get; set; }
    }
}