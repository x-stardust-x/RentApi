using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentApi.Models
{
    [Table("FavoriteHouse")]
    public class FavoriteHouse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AccountId { get; set; }

        [Required]
        public int HouseId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now; // 記錄收藏時間
    }

    // DTO
    public class ToggleFavoriteDto
    {
        public int AccountId { get; set; }
        public int HouseId { get; set; }
    }
}