using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website_BDS.Models.ViewModel
{
    // BỎ CLASS CustomerViewModel BAO Ở NGOÀI
    // Đưa 2 class này ra ngoài cùng cấp namespace

    // 1. Model chính chứa cả Thống kê và Danh sách
    public class CustomerPageViewModel
    {
        // Số liệu thống kê
        public int TongKhachHang { get; set; }
        public int KhachHoatDong { get; set; }
        public int KhachMua { get; set; }
        public int KhachBan { get; set; }

        // Danh sách khách hàng
        public List<CustomerItem> DanhSachKhachHang { get; set; }
    }

    // 2. Model chi tiết cho từng dòng
    public class CustomerItem
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }
        public int SoLuongBDS { get; set; }
        public decimal TongGiaTri { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}