namespace RentApi.Models.DTO
{
    public class Match_ProductDto
    {
        public string ProductType { get; } = "Skill/Tool";

        //public string? Name { get; set; } // 技能名稱 / 工具名稱
        //public string? Url { get; set; } // 縮圖路徑
        //public int? RentPrice { get; set; } // 價格
        //public string? Address { get; set; } // 地址



        //public string? Description { get; set; } // 簡介



        public string? CoverUrl { get; set; }

        // 主鍵 ID（不允許 Null）
        public int Id { get; set; }

        // 會員帳號 ID（不允許 Null）
        public int AccountId { get; set; }

        // 工具或技能名稱 (nvarchar(100))
        public string? Name { get; set; }

        // 分類 (nvarchar(50))
        public string? Category { get; set; }

        // 詳細描述 (nvarchar(MAX))
        public string? Description { get; set; }

        // 價格/薪資 (int)
        public int? Price { get; set; }

        // 計價單位，例如：小時、天、件 (nvarchar(50))
        public string? PriceUnit { get; set; }

        // 押金 (int)
        public int? Deposit { get; set; }

        // 是否上架 (bit -> bool)
        public int Status { get; set; } = 0;

        // 數量 (int)
        public int? Quantity { get; set; }

        // 建立時間 (datetime)
        public DateTime? CreatedAt { get; set; }

        // 更新時間 (datetime)
        public DateTime? UpdatedAt { get; set; }

        // 自備工具說明 (nvarchar(100))
        public string? OwnTool { get; set; }

        // 具備知識/證照需求 (nvarchar(100))
        public string? RequiredKnowledge { get; set; }

        // 地址
        public string? Address { get; set; }
        public int? DistrictId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;

        // 提供者資料
        public string? UserName { get; set; }
        public string? Bio { get; set; }
        public decimal? Rating { get; set; }
        public int? ReviewCount { get; set; }


        // 真實名字
        public string? RealName { get; set; }

        // 使用條款 & 須知
        public string? UsageRequirements { get; set; }
        public string? UsageTerms { get; set; }
    }
}
