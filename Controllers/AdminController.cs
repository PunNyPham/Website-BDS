using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_BDS.Models;
using Website_BDS.Models.ViewModel;
namespace Website_BDS.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        private RealEstateDBEntities db = new RealEstateDBEntities();
        public ActionResult Index_Admin()
        {
            var model = new AdminDashboardViewModel();
            // --- 1. TỔNG BẤT ĐỘNG SẢN ---
            // Dựa trên bảng [Product]
            model.TongSoBDS = db.Products.Count();
            // --- 2. DOANH THU THÁNG NÀY ---
            // Dựa trên bảng [Contracts]
            // Lưu ý: Cần chắc chắn trong DB cột Status bạn lưu chữ gì (ví dụ: 'Completed', 'Done', 'Paid')
            // Ở đây tôi giả định trạng thái thành công là "Completed"
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            model.DoanhThuThangNay = db.Contracts
                                       .Where(c => c.CreatedAt >= startOfMonth
                                                && c.Status == "Completed")
                                       .Sum(c => (decimal?)c.TotalPrice) ?? 0;

            // --- 3. KHÁCH HÀNG ---
            // Dựa trên bảng [Users]. Ta loại bỏ những user là Admin hoặc Staff nếu cần
            // Trong script của bạn TeamRole check: 'Renter', 'Agent', 'Seller', 'Admin'
            model.TongSoKhachHang = db.Users
                                      .Count(u => u.TeamRole != "Admin");

            // --- 4. GIAO DỊCH HOÀN THÀNH ---
            // Dựa trên bảng [Contracts]
            model.GiaoDichHoanThanh = db.Contracts
                                        .Count(c => c.Status == "Completed");

            // --- 5. BẢNG BẤT ĐỘNG SẢN MỚI NHẤT ---
            // Lấy 5 sản phẩm mới nhất từ bảng [Product]
            model.DanhSachBDSMoiNhat = db.Products
                                         .OrderByDescending(p => p.CreatedAt)
                                         .Take(5)
                                         .ToList();
            model.LabelsDoanhThu = new List<string>();
            model.DataDoanhThu = new List<decimal>();

            DateTime today = DateTime.Now;
            // Vòng lặp lùi 5 tháng trước đến tháng hiện tại (Tổng 6 tháng)
            for (int i = 5; i >= 0; i--)
            {
                DateTime monthToCheck = today.AddMonths(-i);
                // Xác định ngày đầu tháng và cuối tháng
                DateTime start = new DateTime(monthToCheck.Year, monthToCheck.Month, 1);
                DateTime end = start.AddMonths(1).AddDays(-1);

                // Tính tổng tiền trong khoảng thời gian đó
                decimal doanhThu = db.Contracts
                                     .Where(c => c.CreatedAt >= start && c.CreatedAt <= end && c.Status == "Completed")
                                     .Sum(c => (decimal?)c.TotalPrice) ?? 0;

                // Thêm vào danh sách để vẽ
                model.LabelsDoanhThu.Add("Thg " + monthToCheck.Month);
                model.DataDoanhThu.Add(doanhThu);
            }

            // --- XỬ LÝ BIỂU ĐỒ 2: PHÂN LOẠI BẤT ĐỘNG SẢN ---
            // Nhóm theo loại (Type) và đếm số lượng
            var thongKeLoai = db.Products
                                .GroupBy(p => p.Type)
                                .Select(g => new { LoaiBDS = g.Key, SoLuong = g.Count() })
                                .ToList();

            // Tách ra 2 list riêng biệt cho Chart.js
            model.LabelsPhanLoai = thongKeLoai.Select(x => x.LoaiBDS ?? "Khác").ToList();
            model.DataPhanLoai = thongKeLoai.Select(x => x.SoLuong).ToList();


            // --- DANH SÁCH MỚI NHẤT (Giữ nguyên) ---
            model.DanhSachBDSMoiNhat = db.Products.OrderByDescending(p => p.CreatedAt).Take(5).ToList();

            return View(model);


        }

        public ActionResult List_RealEstate(string searchString, string type, string status)
        {
            // 1. Lấy tất cả sản phẩm
            var products = db.Products.AsQueryable();

            // 2. Xử lý tìm kiếm theo tên (nếu có nhập)
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Title.Contains(searchString) || p.Address.Contains(searchString));
            }

            // 3. Xử lý lọc theo Loại (Căn hộ, Đất nền...)
            if (!string.IsNullOrEmpty(type) && type != "Tất cả loại")
            {
                products = products.Where(p => p.Type == type);
            }

            // 4. Xử lý lọc theo Trạng thái
            if (!string.IsNullOrEmpty(status) && status != "Tất cả trạng thái")
            {
                products = products.Where(p => p.Status == status);
            }

            // 5. Trả kết quả về View (Sắp xếp mới nhất lên đầu)
            return View(products.OrderByDescending(p => p.CreatedAt).ToList());
        }
        public ActionResult Analysis()
        {
            var model = new AnalysisViewModel();
            var today = DateTime.Now;
            var oneYearAgo = today.AddYears(-1);

            // --- 1. TÍNH TOÁN KPI ---

            // Lấy tất cả hợp đồng hoàn thành trong 12 tháng qua
            var completedContracts = db.Contracts
                .Where(c => c.Status == "Completed" && c.CreatedAt >= oneYearAgo)
                .ToList();

            decimal totalRevenueYear = completedContracts.Sum(c => c.TotalPrice ?? 0);
            int totalTransactions = completedContracts.Count;

            // KPI 1: Doanh thu trung bình/tháng (Chia 12)
            model.DoanhThuTrungBinhThang = totalRevenueYear / 12;

            // KPI 2: Giao dịch trung bình/tháng
            model.GiaoDichTrungBinhThang = Math.Round((double)totalTransactions / 12, 1);

            // KPI 3: Giá trị giao dịch trung bình
            model.GiaTriGiaoDichTB = totalTransactions > 0
                ? totalRevenueYear / totalTransactions
                : 0;

            // KPI 4: Tỷ lệ chuyển đổi (Số Hợp đồng / Tổng số Liên hệ Inquiries)
            int totalInquiries = db.Inquiries.Count(i => i.CreatedAt >= oneYearAgo);
            model.TyLeChuyenDoi = totalInquiries > 0
                ? Math.Round(((double)totalTransactions / totalInquiries) * 100, 1)
                : 0;


            // --- 2. DỮ LIỆU BIỂU ĐỒ DOANH THU (12 Tháng) ---
            model.LabelsThang = new List<string>();
            model.DataDoanhThuThucTe = new List<decimal>();
            model.DataDoanhThuMucTieu = new List<decimal>();

            for (int i = 11; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                model.LabelsThang.Add("T" + month.Month);

                // Tính doanh thu tháng đó
                var revenue = db.Contracts
                    .Where(c => c.CreatedAt.Value.Month == month.Month
                             && c.CreatedAt.Value.Year == month.Year
                             && c.Status == "Completed")
                    .Sum(c => (decimal?)c.TotalPrice) ?? 0;

                model.DataDoanhThuThucTe.Add(revenue);

                // Giả lập mục tiêu: Mục tiêu = Doanh thu thực + 20% (Để vẽ cho đẹp)
                model.DataDoanhThuMucTieu.Add(revenue * 1.2m);
            }


            // --- 3. HIỆU SUẤT THEO LOẠI BĐS (Bảng + Biểu đồ cột) ---
            // Join bảng Contracts với Product để lấy Type
            var performanceData = db.Contracts
                .Where(c => c.Status == "Completed")
                .GroupBy(c => c.Product.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(x => x.TotalPrice) ?? 0
                })
                .ToList();

            decimal grandTotalRevenue = performanceData.Sum(x => x.Revenue); // Tổng doanh thu toàn bộ để tính %

            model.HieuSuatTheoLoai = performanceData.Select(x => new TypePerformance
            {
                LoaiBDS = x.Type ?? "Khác",
                SoLuongDaBan = x.Count,
                TongDoanhThu = x.Revenue,
                GiaTrungBinh = x.Count > 0 ? x.Revenue / x.Count : 0,
                HieuSuat = grandTotalRevenue > 0 ? (double)(x.Revenue / grandTotalRevenue * 100) : 0
            }).OrderByDescending(x => x.TongDoanhThu).ToList();


            // --- 4. DOANH THU THEO KHU VỰC (Top 5 Quận) ---
            var locationData = db.Contracts
                .Where(c => c.Status == "Completed")
                .GroupBy(c => c.Product.District)
                .Select(g => new { District = g.Key, Revenue = g.Sum(x => x.TotalPrice) ?? 0 })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToList();

            model.LabelsKhuVuc = locationData.Select(x => x.District).ToList();
            model.DataDoanhThuKhuVuc = locationData.Select(x => x.Revenue).ToList();

            return View(model);
        }
        public ActionResult Customer(string searchString, string type, string status)
        {
            // Khởi tạo ViewModel
            var model = new CustomerPageViewModel();

            // 1. Lấy Base Query: Lấy tất cả user KHÔNG PHẢI là Admin
            var query = db.Users.Where(u => u.TeamRole != "Admin");

            // --- XỬ LÝ LỌC (FILTER) ---

            // Tìm kiếm theo tên hoặc email
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.FullName.Contains(searchString) || u.Email.Contains(searchString));
            }

            // Lọc theo loại (Dựa vào TeamRole: Seller hoặc Renter/Buyer)
            if (!string.IsNullOrEmpty(type) && type != "Tất cả loại")
            {
                // Giả sử: Seller là "Bán", còn lại là "Mua"
                if (type == "Bán")
                    query = query.Where(u => u.TeamRole == "Seller");
                else if (type == "Mua")
                    query = query.Where(u => u.TeamRole != "Seller");
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && status != "Tất cả trạng thái")
            {
                bool isActive = status == "Hoạt động";
                query = query.Where(u => u.Status == isActive);
            }

            // --- TÍNH TOÁN THỐNG KÊ (Dựa trên toàn bộ dữ liệu, không bị ảnh hưởng bởi search hiện tại) ---
            var allUsers = db.Users.Where(u => u.TeamRole != "Admin").ToList();

            model.TongKhachHang = allUsers.Count();
            model.KhachHoatDong = allUsers.Count(u => u.Status == true);
            model.KhachBan = allUsers.Count(u => u.TeamRole == "Seller");
            model.KhachMua = allUsers.Count(u => u.TeamRole != "Seller"); // Các role còn lại coi là Mua/Thuê

            // --- TẠO DANH SÁCH HIỂN THỊ ---
            // Execute query để lấy list user cần hiển thị
            var userList = query.OrderByDescending(u => u.CreatedAt).ToList();

            model.DanhSachKhachHang = new List<CustomerItem>();

            foreach (var u in userList)
            {
                var item = new CustomerItem();
                item.UserID = u.UserID;
                item.FullName = u.FullName;
                item.Email = u.Email;
                item.PhoneNumber = u.PhoneNumber;
                item.Avatar = u.Avatar_User;
                item.Status = u.Status ?? false;
                item.CreatedAt = u.CreatedAt ?? DateTime.Now;

                // Xử lý Logic Loại khách & Số liệu
                if (u.TeamRole == "Seller")
                {
                    item.Type = "Bán";
                    // Nếu là người bán: Đếm số sản phẩm họ đăng
                    item.SoLuongBDS = db.Products.Count(p => p.OwnerID == u.UserID);
                    // Tổng giá trị: Tổng tiền các hợp đồng bán thành công
                    item.TongGiaTri = db.Contracts
                                        .Where(c => c.SellerID == u.UserID && c.Status == "Completed")
                                        .Sum(c => (decimal?)c.TotalPrice) ?? 0;
                }
                else
                {
                    item.Type = "Mua"; // Bao gồm cả Renter
                                       // Nếu là người mua: Đếm số hợp đồng họ đã mua
                    item.SoLuongBDS = db.Contracts.Count(c => c.BuyerID == u.UserID && c.Status == "Completed");
                    // Tổng giá trị: Tổng tiền họ đã chi
                    item.TongGiaTri = db.Contracts
                                        .Where(c => c.BuyerID == u.UserID && c.Status == "Completed")
                                        .Sum(c => (decimal?)c.TotalPrice) ?? 0;
                }

                // Lưu ý: DB của bạn chưa có bảng Address cho User, nên tạm thời để trống hoặc lấy từ Product mới nhất
                item.Address = "Chưa cập nhật";

                model.DanhSachKhachHang.Add(item);
            }

            return View(model);
        }
        public ActionResult transaction_management(string searchString, string type, string status)
        {
            var model = new TransactionViewModel();

            // 1. Lấy dữ liệu cơ bản từ bảng Contracts, kết hợp Product và Users
            var query = db.Contracts.AsQueryable();

            // --- XỬ LÝ LỌC (FILTER) ---

            // Tìm kiếm theo tên Khách hoặc tên BĐS
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.Product.Title.Contains(searchString) ||
                                         c.User.FullName.Contains(searchString)); // User ở đây là Buyer
            }

            // Lọc theo loại (Dựa vào ContractType: Mua/Thuê)
            if (!string.IsNullOrEmpty(type) && type != "Tất cả loại")
            {
                query = query.Where(c => c.ContractType == type);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && status != "Tất cả trạng thái")
            {
                query = query.Where(c => c.Status == status);
            }

            // --- TÍNH TOÁN THỐNG KÊ (Dựa trên toàn bộ dữ liệu gốc) ---
            var allContracts = db.Contracts.ToList();

            model.TongDoanhThu = allContracts
                                 .Where(c => c.Status == "Completed")
                                 .Sum(c => (decimal?)c.TotalPrice) ?? 0;

            // Giả sử hoa hồng là 3% doanh thu
            model.TongHoaHong = model.TongDoanhThu * 0.03m;

            model.DaHoanThanh = allContracts.Count(c => c.Status == "Completed");
            model.DangXuLy = allContracts.Count(c => c.Status == "Pending"); // Hoặc trạng thái khác tùy DB bạn


            // --- TẠO DANH SÁCH HIỂN THỊ ---
            var resultList = query.OrderByDescending(c => c.CreatedAt).ToList();
            model.DanhSachGiaoDich = new List<TransactionItem>();

            foreach (var item in resultList)
            {
                var trans = new TransactionItem();
                trans.ContractID = item.ContractID;
                trans.MaGD = "GD" + item.ContractID.ToString("D4"); // Ví dụ: GD0001

                // Null check để tránh lỗi nếu Product hoặc Buyer bị xóa
                trans.TenBDS = item.Product != null ? item.Product.Title : "BĐS không tồn tại";
                trans.TenKhachHang = item.User != null ? item.User.FullName : "Khách ẩn"; // User là Buyer

                trans.LoaiGiaoDich = item.ContractType ?? "Mua";
                trans.SoTien = item.TotalPrice ?? 0;

                // Tính hoa hồng từng giao dịch (3%)
                trans.HoaHong = trans.SoTien * 0.03m;

                trans.NgayGD = item.CreatedAt ?? DateTime.Now;
                trans.TrangThai = item.Status ?? "Pending";

                // Giả lập phương thức thanh toán vì DB chưa có cột này
                trans.PTThanhToan = "Chuyển khoản";

                model.DanhSachGiaoDich.Add(trans);
            }

            return View(model);
        }
    }
}