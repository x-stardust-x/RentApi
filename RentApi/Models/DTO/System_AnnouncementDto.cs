namespace RentApi.Models.DTO {
    public class System_AnnouncementDto {

        public int AdminId { get; set; }            // 發布的管理員

        public string? Category { get; set; } = ""; // 分類

        public string? Title { get; set; } = "";    // 文章標題

        public string? Cover { get; set; } // 封面縮圖

        public string? Intro { get; set; } = "";    // 文章前言

        public string? Content { get; set; } = "";  // 內文

        public string? SEOTitle { get; set; } = ""; // SEO 標題

        public string? SEODesc { get; set; } = "";  // SEO 描述

        public int Status { get; set; }            // 狀態 (0:草稿, 1:排程, 2:已發布)

    }
}
