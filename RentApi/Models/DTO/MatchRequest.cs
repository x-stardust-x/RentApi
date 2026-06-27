namespace RentApi.Models.DTO
{
    public class MatchRequest
    {
        // 這裡先用 object 接收，之後可以換成你資料庫實際的 User 和 House 型別
        public object User { get; set; } = null!;
        public object House { get; set; } = null!;
    }
}
