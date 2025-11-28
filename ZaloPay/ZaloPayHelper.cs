using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Website_BDS.ZaloPay
{
    public class ZaloPayHelper
    {
        // THÔNG TIN TÀI KHOẢN TEST (SANDBOX) CỦA ZALOPAY
        public static string AppId = "2553";
        public static string Key1 = "PcY4iZIKFCIdgZvA6ueMcMHHUbRLYjPL";
        public static string Key2 = "kLtgPl8HHhfvMuDHPwKfgfsY4Ydm9eIz";
        public static string CreateOrderUrl = "https://sb-openapi.zalopay.vn/v2/create";

        public static string HmacSHA256(string inputData, string key)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA256(keyByte))
            {
                byte[] hashMessage = hmac.ComputeHash(messageBytes);
                var hex = BitConverter.ToString(hashMessage);
                return hex.Replace("-", "").ToLower();
            }
        }
    }
}