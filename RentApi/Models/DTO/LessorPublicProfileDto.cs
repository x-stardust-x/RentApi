namespace RentApi.Models.DTO
{
    public class LessorPublicProfileDto
    {
        // 帳號與個人核心資料 (對應 Account 與 User 表)
        public int AccountId { get; set; }     // 改用你的 AccountId
        public string Username { get; set; }   // 帳號暱稱
        public string RealName { get; set; }   // 真實姓名
        public string AvatarUrl { get; set; }  // 大頭貼
        public string BannerUrl { get; set; }  // 建議新增的橫幅圖片
        public string ProfileTitle { get; set; }    // 建議新增：如"退休家具設計師"
        public string ProfileSubTitle { get; set; } // 建議新增：如"40年以上經驗"
        public string Bio { get; set; }        // 你的欄位叫 Bio (個人簡介)

        // 統計數據 (對應 User 表)
        public decimal Rating { get; set; }     // 你的欄位叫 Rating
        public int ReviewCount { get; set; }   // 你的欄位叫 ReviewCount
        public int JoinYears { get; set; }      // 由 DateTime.Now.Year - User.CreatedAt.Year 計算得出

        // 認證與身分狀態
        public int Identity { get; set; }       // 0:老人 Mentor, 1:年輕人 Learner
        public bool IsVerified { get; set; }    // 由 Identity_Verification.Status == 1 判斷

        // 生活習慣 (對應 User_Habit 表，型別皆依你的規格)
        public int SleepTime { get; set; }      // 睡覺時間 (0-23)
        public int WakeTime { get; set; }       // 起床時間 (0-23)
        public int CleanLevel { get; set; }     // 整潔度 (1-5)
        public int NoiseTolerance { get; set; } // 噪音容忍度 (1-5)
        public bool Pet { get; set; }           // 是否養/接受寵物
        public bool Smoke { get; set; }         // 是否抽菸
        public string Interests { get; set; }   // 興趣標籤 (JSON 字串)

        // 刊登資源與評價清單
        public List<HouseDto> ActiveHouses { get; set; }
        public List<ProductDto> ActiveProducts { get; set; }
        public List<ReviewDto> Reviews { get; set; }


        // ============= 子類別 =============
        public class HouseDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int RentPrice { get; set; }
            public string MainImageUrl { get; set; }
        }

        public class ProductDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Price { get; set; }          // 你的欄位叫 Price
            public string PriceUnit { get; set; }   // 單位 (次/小時/日)
            public string MainImageUrl { get; set; } // 從 Image_Store 撈取 Type='Product' 且 IsMain=true 的 Url
        }

        public class ReviewDto
        {
            public string ReviewerName { get; set; }     // 評價者的 Username 或 RealName
            public string ReviewerOccupation { get; set; } // 建議新增的評價者職業 (如：建築系學生)
            public string ReviewerAvatarUrl { get; set; }  // 評價者大頭貼
            public int Score { get; set; }               // 你的欄位叫 Score (星等)
            public string Comment { get; set; }           // 你的欄位叫 Comment (評語)
        }
    }
}
