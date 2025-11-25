using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website_BDS.Models.ViewModel
{
    public class TransactionViewModel
    {
        // 4 Chỉ số thống kê trên cùng
        public decimal TongDoanhThu { get; set; }
        public decimal TongHoaHong { get; set; } // Giả sử hoa hồng là 1% hoặc 3%
        public int DaHoanThanh { get; set; }
        public int DangXuLy { get; set; }

        // Danh sách giao dịch
        public List<TransactionItem> DanhSachGiaoDich { get; set; }
    }

    // Class chi tiết từng dòng
    public class TransactionItem
    {
        public int ContractID { get; set; }
        public string MaGD { get; set; } // Tạo mã giả lập dạng GD001
        public string TenBDS { get; set; }
        public string TenKhachHang { get; set; }
        public string LoaiGiaoDich { get; set; } // Mua/Bán/Thuê
        public decimal SoTien { get; set; }
        public decimal HoaHong { get; set; }
        public DateTime NgayGD { get; set; }
        public string PTThanhToan { get; set; } // Chuyển khoản/Tiền mặt (Dữ liệu giả lập nếu DB chưa có)
        public string TrangThai { get; set; } // Hoàn thành/Đang xử lý
    }
}
