using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Website_BDS.Models;
using Website_BDS.ZaloPay;
namespace Website_BDS.Controllers
{
    public class PaymentController : Controller
    {
        private RealEstateDBEntities db = new RealEstateDBEntities();

        [HttpPost]
        public async Task<ActionResult> CreateZaloPayOrder(long amount)
        {
            // 1. Kiểm tra đăng nhập
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            var user = db.Users.Find(userId);

            // 2. Tạo mã đơn hàng (AppTransId) phải là duy nhất: yyMMdd_MãNgẫuNhiên
            Random rnd = new Random();
            string appTransId = DateTime.Now.ToString("yyMMdd") + "_" + rnd.Next(100000, 999999);

            // 3. Chuẩn bị dữ liệu gửi sang ZaloPay
            var param = new Dictionary<string, string>();
            param.Add("app_id", ZaloPayHelper.AppId);
            param.Add("app_user", user.FullName ?? "Guest");
            param.Add("app_time", Utils.GetTimeStamp().ToString()); // Cần hàm lấy timestamp (xem bên dưới)
            param.Add("amount", amount.ToString());
            param.Add("app_trans_id", appTransId);
            param.Add("embed_data", JsonConvert.SerializeObject(new { }));
            param.Add("item", JsonConvert.SerializeObject(new[] { new { itemid = "nap_tien", itemname = "Nạp tiền vào ví", itemprice = amount } }));
            param.Add("description", $"BDS Pro - Thanh toán đơn hàng #{appTransId}");
            param.Add("bank_code", ""); // Để trống để user chọn trên cổng ZaloPay

            // Tạo chữ ký (Mac)
            string data = $"{ZaloPayHelper.AppId}|{param["app_trans_id"]}|{param["app_user"]}|{param["amount"]}|{param["app_time"]}|{param["embed_data"]}|{param["item"]}";
            param.Add("mac", ZaloPayHelper.HmacSHA256(data, ZaloPayHelper.Key1));

            // 4. Gửi Request sang ZaloPay
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(param);
                    var response = await client.PostAsync(ZaloPayHelper.CreateOrderUrl, content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseString);

                    if (result.return_code == 1)
                    {
                        // Thành công -> Trả về link thanh toán (order_url)
                        return Json(new { success = true, payUrl = result.order_url });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Lỗi ZaloPay: " + result.return_message });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }

    // Hàm phụ lấy TimeStamp
    public static class Utils
    {
        public static long GetTimeStamp()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }
    }
}