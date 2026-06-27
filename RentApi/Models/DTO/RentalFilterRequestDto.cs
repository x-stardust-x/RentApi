using System;
using System.Collections.Generic;

namespace YourProjectNamespace.Dtos
{
    public class RentalFilterRequestDto
    {

        // ==========================================
        // 1. 通用篩選條件 (不限 / 房屋 / 工具技能)
        // ==========================================
             
        /// 類別：all (不限), room (房間出租), product (工具/技能)
        public string Category { get; set; } = "all";

        /// 縣市名稱（例如：taipei, new-taipei）
        public string? City { get; set; }

        /// 價格範圍 - 最低價（台幣）
        public decimal PriceMin { get; set; } = 0;

        /// 價格範圍 - 最高價（台幣）
        public decimal PriceMax { get; set; } = 50000;
                 
        /// 排序方式：newest (由新至舊), oldest (由舊至新)
        public string SortOrder { get; set; } = "newest";


        // ==========================================
        // 2. 房屋出租 - 基本風格篩選 (預設為 null 代表使用者未指定)
        // ==========================================

        /// 是否啟用個人檔案智能配對
        public bool IsSmartMatch { get; set; } = false;

        /// 是否禁菸：true (禁菸), false (可菸), null (不限)
        public bool? Smoking { get; set; }

        /// 是否要安靜：true (要求安靜), null (不限)
        public bool? Quiet { get; set; }

        /// 是否可養寵物：true (可寵), false (禁寵), null (不限)
        public bool? Pets { get; set; }


        // ==========================================
        // 3. 房屋出租 - 六項進階生活規範複選 (對應 JSON 內容)
        // 使用 List<string> 接收，這樣前端勾選多個時（如：早睡早起+正常作息）都能處理
        // ==========================================

        /// 作息型態：早睡早起, 正常作息, 夜貓子
        public List<string> Routines { get; set; } = new List<string>();

        /// 深夜洗衣/洗澡限制：無限制, 22:00後禁止, 23:00後禁止
        public List<string> ShowerRestrictions { get; set; } = new List<string>();
        public List<string> LimitLaundry { get; set; } = new List<string>();

        /// 訪客留宿規範：完全謝絕訪客, 僅限白天拜訪, 可帶異性或同性過夜
        public List<string> VisitorPolicies { get; set; } = new List<string>();

        /// 廚房與飲食文化：可大火快炒, 僅限輕食微波, 禁開伙
        public List<string> CookingHabits { get; set; } = new List<string>();

        /// 冰箱使用分配：各自獨立分層, 貼標籤即可, 共同分享
        public List<string> FridgeAllocations { get; set; } = new List<string>();

        /// 期望交流頻率：純租屋互不打擾, 願意每週共餐聊天, 期待技能傳承交流
        public List<string> InteractionFrequencies { get; set; } = new List<string>();

        public List<string>? LifeStyle { get; set; }


        // 在 RentalFilterRequestDto.cs 中
        public List<string> AdvancedRules { get; set; } = new List<string>();
        public List<int> CleanLevels { get; set; } = new();
        public List<int> NoiseToleranceLevels { get; set; } = new();

        public bool? LivingWithLessor { get; set; }
    }
}