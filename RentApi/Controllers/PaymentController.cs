using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace YourProjectNamespace.Controllers // 💡 記得改成你的專案命名空間
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _config;

        
        private readonly string _merchantID = "2000132";
        private readonly string _hashKey = "5294y06JbISpM5x9";
        private readonly string _hashIV = "v77hoKGq4kWxNNIS";
        private readonly string _ecpayUrl = "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5";

        public PaymentController(IConfiguration config)
        {
            _config = config;
        }

        [Authorize] 
        [HttpPost("ecpay-checkout")]
        public IActionResult ECPayCheckout([FromQuery] int tier)
        {
            // 1. 從 Token 挖出使用者 ID (供後續對照是誰付錢)
            var accountId = User.FindFirst("AccountId")?.Value ?? "0";

            // 2. 根據前端選擇的等級，決定金額與商品名稱
            int amount = tier == 2 ? 199 : 299;
            string itemName = tier == 2 ? "厚厝味-進階會員(月訂閱)" : "厚厝味-尊榮VIP全包方案(月訂閱)";

            // 3. 產生綠界要求的唯一訂單編號 (格式：英數組合，最多20字)
            // 這裡用時間戳記 + 帳號 ID 確保不重複
            string merchantTradeNo = "HC" + DateTime.Now.ToString("yyyyMMddHHmmss") + accountId;
            if (merchantTradeNo.Length > 20) merchantTradeNo = merchantTradeNo.Substring(0, 20);

            // 4. 準備發送給綠界的參數清單 (順序必須依照字母 A-Z 排列，綠界規定的)
            var parameters = new SortedDictionary<string, string>
            {
                { "ChoosePayment", "Credit" }, 
                { "ClientBackURL", "http://localhost:4200/subscription?status=success&tier=" + tier },
                { "EncryptType", "1" },
                { "ItemName", itemName },
                { "MerchantID", _merchantID },
                { "MerchantTradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") },
                { "MerchantTradeNo", merchantTradeNo },
                { "PaymentType", "aio" },
                { "ReturnURL", "https://your-domain.com/api/payment/ecpay-callback" }, 
                { "TotalAmount", amount.ToString() },
                { "TradeDesc", Uri.EscapeDataString("厚厝味會員升級方案") }
            };

            
            parameters.Add("CheckMacValue", GenerateCheckMacValue(parameters));

            
            string htmlForm = BuildAutoSubmitForm(parameters, _ecpayUrl);

            // 7. 將 HTML 回傳給前端，讓前端直接渲染並引渡使用者去綠界
            return Ok(new { form = htmlForm });
        }

        #region 🔒 綠界底層加密演算法 (SHA256)
        private string GenerateCheckMacValue(SortedDictionary<string, string> parameters)
        {
            // Step 1: 用 HashKey 開頭，並將參數組裝成 QueryString
            var sb = new StringBuilder($"HashKey={_hashKey}");
            foreach (var kvp in parameters)
            {
                sb.Append($"&{kvp.Key}={kvp.Value}");
            }
            // Step 2: 用 HashIV 結尾
            sb.Append($"&HashIV={_hashIV}");

            // Step 3: 進行 URL Encode，並轉成全小寫
            string encodedUrl = HttpUtility.UrlEncode(sb.ToString()).ToLower();

            // 修正：綠界的 URL Encode 有些特殊符號跟 C# 內建的不一樣，必須手動修正
            encodedUrl = encodedUrl.Replace("%2d", "-").Replace("%5f", "_").Replace("%2e", ".").Replace("%21", "!").Replace("%2a", "*").Replace("%28", "(").Replace("%29", ")");

            // Step 4: 進行 SHA256 加密
            byte[] source = Encoding.UTF8.GetBytes(encodedUrl);
            byte[] crypto = SHA256.HashData(source);

            // Step 5: 轉成 16 進位字串並全部大寫
            var result = new StringBuilder();
            for (int i = 0; i < crypto.Length; i++)
            {
                result.Append(crypto[i].ToString("X2"));
            }

            return result.ToString().ToUpper();
        }

        private string BuildAutoSubmitForm(SortedDictionary<string, string> parameters, string url)
        {
            var sb = new StringBuilder();
            sb.Append($"<form id='ecpayForm' action='{url}' method='post'>");
            foreach (var kvp in parameters)
            {
                sb.Append($"<input type='hidden' name='{kvp.Key}' value='{kvp.Value}' />");
            }
            sb.Append("</form>");
            sb.Append("<script>document.getElementById('ecpayForm').submit();</script>");
            return sb.ToString();
        }
        #endregion
    }
}