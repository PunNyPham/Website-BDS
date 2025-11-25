using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website_BDS.Models.ViewModel
{
    public class AdminDashboardViewModel
    {
        public int TongSoBDS { get; set; }
        public decimal DoanhThuThangNay { get; set; }
        public int TongSoKhachHang { get; set; }
        public int GiaoDichHoanThanh { get; set; }

        // Danh sách này chứa các object từ bảng Product
        public List<Product> DanhSachBDSMoiNhat { get; set; }
        //Dữ liệu cho biểu đồ Doanh thu(Line Chart)
        public List<string> LabelsDoanhThu { get; set; } 
        public List<decimal> DataDoanhThu { get; set; }  

        // THÊM MỚI: Dữ liệu cho biểu đồ Phân loại (Pie Chart)
        public List<string> LabelsPhanLoai { get; set; }
        public List<int> DataPhanLoai { get; set; }
    }
}