using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    // 💡 請注意：[Table("這裡要填寫你資料庫真正的資料表名稱")] 
    // 根據你的圖片右側頁籤，如果是 HouseViewing_Order 或 Viewing_Order，請記得修正括號內的字串
    [Table("House_Viewing_Order")]
    public class HouseViewing
    {
        [Key]
        public int Id { get; set; }

        //[Required]
        [StringLength(50)]
        public string ReservationNo { get; set; } // 補上欄位

        public int HouseId { get; set; }

        public int LesseeId { get; set; }  // 配合妳 DB 的拼法：LesseId (少一個 e)
        
        public int LessorId { get; set; }

        public DateTime? ViewingTime { get; set; } // 補上欄位

        public DateTime? ExpectedMoveIn { get; set; } // 配合 DB 拼法

        public string? Message { get; set; }

        public int? MatchScore { get; set; }

        public int? Status { get; set; } // 2. 修正為 int！解決 CS0029 轉型錯誤

        public string? RejectReason { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; } // 補上欄位

        // ==========================================
        // 導覽屬性 (Navigation Property)
        // 3. 補上這個，就能完美解決 image_b7913d 的 CS1061 報錯！
        // ==========================================
        [ForeignKey("HouseId")]
        public virtual Rent_House RentHouse { get; set; }
    }
}