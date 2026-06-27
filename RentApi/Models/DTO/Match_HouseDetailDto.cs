namespace RentApi.Models.DTO
{
    public class Match_HouseDetailDto
    {
        public string ProductType { get; } = "House";

        #region 房屋

        public int? Id { get; set; } // ID
        public int? AccountId { get; set; } // 所屬房東
        public string? Name { get; set; } // 房屋名稱 (標題)
        public int? DistrictID { get; set; } // 關聯區域 (用於地區篩選)
        public string? Address { get; set; } // 詳細地址
        public string? Description { get; set; } // 描述/簡介
        public int? RentPrice { get; set; } // 每月租金
        public bool? IncludeUtilities { get; set; } // 是否含 水電
        public bool? IncludeWifi { get; set; } // 是否含 網路
        public bool? IncludeManagementFee { get; set; } // 是否含 管理費
        public decimal? AreaSize { get; set; } // 坪數
        public int? LeaseTerm { get; set; } // 租期時長需求 (如：6個月起，單位：月)
        public string? FloorInfo { get; set; } // 樓層/電梯資訊
        public string? HouseType { get; set; } // 房屋類型 (套房/雅房/活動空間)
        public string? ViewCount { get; set; } // 瀏覽次數
        public int? Status { get; set; } // 狀態 (0:待審, 1:已上架, 2:退回, 3:已租出)




        public string? Url { get; set; } // 縮圖路徑 (或許要再新增欄位)


        // 生活習慣規範 (來自 HouseRules)
        public int? HouseId { get; set; }
        public TimeOnly? SleepTime { get; set; }
        public TimeOnly? WakeTime { get; set; }
        public int? CleanLevel { get; set; }
        public int? NoiseTolerance { get; set; }
        public bool? Pet { get; set; }
        public bool? Smoke { get; set; }


        // 真實名字
        public string? RealName { get; set; }


        #endregion


        #region 側邊篩選器

        public int? DistrictId { get; set; } // 地點


        #endregion
    }
}
