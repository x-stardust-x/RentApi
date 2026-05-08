namespace RentApi.Models.DTO
{
    public class Match_HouseDto
    {
        #region 租賃物

        public string? Name { get; set; } // 名稱
        public string? Url { get; set; } // 縮圖
        public int? RentPrice { get; set; } // 價格
        public string? Address { get; set; } // 地址

        // =============== 標籤群組 ===============

        public bool? Pet { get; set; } // 是否可養寵物
        public bool? Smoke { get; set; } // 是否禁菸
        public int? NoiseTolerance { get; set; } // 噪音程度

        // =======================================

        public string? Description { get; set; } // 簡介

        #endregion



        #region 篩選器

        // 產品類型
        // 地點
        // 價格區間
        // 生活習慣規範( 出租人規定 )




        #endregion


    }
}
