using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website_BDS.Models.ViewModel
{
    public class AnalysisViewModel
    {
        // 1. BỐN THẺ KPI
        public decimal DoanhThuTrungBinhThang { get; set; }
        public double GiaoDichTrungBinhThang { get; set; }
        public decimal GiaTriGiaoDichTB { get; set; }
        public double TyLeChuyenDoi { get; set; } // (Số giao dịch / Số liên hệ) * 100

        // 2. BIỂU ĐỒ DOANH THU 12 THÁNG (LINE CHART)
        public List<string> LabelsThang { get; set; }
        public List<decimal> DataDoanhThuThucTe { get; set; }
        public List<decimal> DataDoanhThuMucTieu { get; set; } // Giả lập mục tiêu

        // 3. BIỂU ĐỒ HIỆU SUẤT LOẠI BĐS (BAR CHART) & BẢNG CHI TIẾT
        public List<TypePerformance> HieuSuatTheoLoai { get; set; }

        // 4. BIỂU ĐỒ DOANH THU THEO KHU VỰC (HORIZONTAL BAR CHART)
        public List<string> LabelsKhuVuc { get; set; }
        public List<decimal> DataDoanhThuKhuVuc { get; set; }
    }

    // Class phụ để chứa thông tin từng dòng trong bảng chi tiết
    public class TypePerformance
    {
        public string LoaiBDS { get; set; }
        public int SoLuongDaBan { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal GiaTrungBinh { get; set; }
        public double HieuSuat { get; set; } // % đóng góp vào tổng doanh thu
    }
}
