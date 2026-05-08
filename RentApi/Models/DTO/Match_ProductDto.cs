namespace RentApi.Models.DTO
{
    public class Match_ProductDto
    {
        public string ProductType { get; } = "Skill/Tool";

        #region 技能 / 工具

        public string? Name { get; set; } // 技能名稱 / 工具名稱
        public string? Url { get; set; } // 縮圖路徑
        public int? RentPrice { get; set; } // 價格
        public string? Address { get; set; } // 地址



        public string? Description { get; set; } // 簡介

        #endregion
    }
}
