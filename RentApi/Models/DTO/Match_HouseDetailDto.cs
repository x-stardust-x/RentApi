namespace RentApi.Models.DTO
{
    public class Match_HouseDetailDto
    {
        public string ProductType { get; } = "House";

        
        #region 房屋

        public int Id { get; set; } // ID
        public string? Name { get; set; } // 名稱
        public string? Url { get; set; } // 縮圖路徑
        public int? RentPrice { get; set; } // 價格
        public string? Address { get; set; } // 地址

        // =============== 生活習館規範 標籤群組 ===============

        //public int? Id { get; set; }
        public int HouseId { get; set; } // 房屋編號
        public int SleepTime { get; set; } // 熄燈時間
        public int WakeTime { get; set; } // 起床時間
        public int CleanLevel { get; set; } // 需達到整潔程度
        public int NoiseTolerance { get; set; } // 噪音程度
        public bool? Pet { get; set; } // 是否可養寵物
        public bool? Smoke { get; set; } // 是否禁菸

        // =======================================

        public string? Description { get; set; } // 簡介

        #endregion


        #region 側邊篩選器

        public int? DistrictId { get; set; } // 地點


        #endregion
    }
}
