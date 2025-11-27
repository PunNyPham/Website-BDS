using System;
using System.Collections.Generic;
using System.Linq;
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
            
            model.TongSoBDS = db.Products.Count();
           
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            model.DoanhThuThangNay = db.Contracts
                                       .Where(c => c.CreatedAt >= startOfMonth
                                                && c.Status == "Completed")
                                       .Sum(c => (decimal?)c.TotalPrice) ?? 0;
            model.TongSoKhachHang = db.Users
                                      .Count(u => u.TeamRole != "Admin");
            model.GiaoDichHoanThanh = db.Contracts
                                        .Count(c => c.Status == "Completed");
            model.DanhSachBDSMoiNhat = db.Products
                                         .OrderByDescending(p => p.CreatedAt)
                                         .Take(5)
                                         .ToList();
            model.LabelsDoanhThu = new List<string>();
            model.DataDoanhThu = new List<decimal>();

            DateTime today = DateTime.Now;
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
                model.LabelsDoanhThu.Add("Thg " + monthToCheck.Month);
                model.DataDoanhThu.Add(doanhThu);
            }
            var thongKeLoai = db.Products
                                .GroupBy(p => p.Type)
                                .Select(g => new { LoaiBDS = g.Key, SoLuong = g.Count() })
                                .ToList();

            model.LabelsPhanLoai = thongKeLoai.Select(x => x.LoaiBDS ?? "Khác").ToList();
            model.DataPhanLoai = thongKeLoai.Select(x => x.SoLuong).ToList();

            model.DanhSachBDSMoiNhat = db.Products.OrderByDescending(p => p.CreatedAt).Take(5).ToList();

            return View(model);


        }

        public ActionResult List_RealEstate(string searchString, string type, string status)
        {
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Title.Contains(searchString) || p.Address.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(type) && type != "Tất cả loại")
            {
                products = products.Where(p => p.Type == type);
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả trạng thái")
            {
                products = products.Where(p => p.Status == status);
            }

            return View(products.OrderByDescending(p => p.CreatedAt).ToList());
        }
        public ActionResult Analysis()
        {
            var model = new AnalysisViewModel();
            var today = DateTime.Now;
            var oneYearAgo = today.AddYears(-1);


            var completedContracts = db.Contracts
                .Where(c => c.Status == "Completed" && c.CreatedAt >= oneYearAgo)
                .ToList();

            decimal totalRevenueYear = completedContracts.Sum(c => c.TotalPrice ?? 0);
            int totalTransactions = completedContracts.Count;

            model.DoanhThuTrungBinhThang = totalRevenueYear / 12;

            model.GiaoDichTrungBinhThang = Math.Round((double)totalTransactions / 12, 1);

            model.GiaTriGiaoDichTB = totalTransactions > 0
                ? totalRevenueYear / totalTransactions
                : 0;

            int totalInquiries = db.Inquiries.Count(i => i.CreatedAt >= oneYearAgo);
            model.TyLeChuyenDoi = totalInquiries > 0
                ? Math.Round(((double)totalTransactions / totalInquiries) * 100, 1)
                : 0;


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

                model.DataDoanhThuMucTieu.Add(revenue * 1.2m);
            }

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
            var model = new CustomerPageViewModel();

            var query = db.Users.Where(u => u.TeamRole != "Admin");


            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.FullName.Contains(searchString) || u.Email.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(type) && type != "Tất cả loại")
            {
                if (type == "Bán")
                    query = query.Where(u => u.TeamRole == "Seller");
                else if (type == "Mua")
                    query = query.Where(u => u.TeamRole != "Seller");
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả trạng thái")
            {
                bool isActive = status == "Hoạt động";
                query = query.Where(u => u.Status == isActive);
            }

            var allUsers = db.Users.Where(u => u.TeamRole != "Admin").ToList();

            model.TongKhachHang = allUsers.Count();
            model.KhachHoatDong = allUsers.Count(u => u.Status == true);
            model.KhachBan = allUsers.Count(u => u.TeamRole == "Seller");
            model.KhachMua = allUsers.Count(u => u.TeamRole != "Seller"); 

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

            var query = db.Contracts.AsQueryable();


            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.Product.Title.Contains(searchString) ||
                                         c.User.FullName.Contains(searchString)); // User ở đây là Buyer
            }

            if (!string.IsNullOrEmpty(type) && type != "Tất cả loại")
            {
                query = query.Where(c => c.ContractType == type);
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả trạng thái")
            {
                query = query.Where(c => c.Status == status);
            }

            var allContracts = db.Contracts.ToList();

            model.TongDoanhThu = allContracts
                                 .Where(c => c.Status == "Completed")
                                 .Sum(c => (decimal?)c.TotalPrice) ?? 0;

            model.TongHoaHong = model.TongDoanhThu * 0.03m;

            model.DaHoanThanh = allContracts.Count(c => c.Status == "Completed");
            model.DangXuLy = allContracts.Count(c => c.Status == "Pending"); 


            var resultList = query.OrderByDescending(c => c.CreatedAt).ToList();
            model.DanhSachGiaoDich = new List<TransactionItem>();

            foreach (var item in resultList)
            {
                var trans = new TransactionItem();
                trans.ContractID = item.ContractID;
                trans.MaGD = "GD" + item.ContractID.ToString("D4"); 

                trans.TenBDS = item.Product != null ? item.Product.Title : "BĐS không tồn tại";
                trans.TenKhachHang = item.User != null ? item.User.FullName : "Khách ẩn"; 

                trans.LoaiGiaoDich = item.ContractType ?? "Mua";
                trans.SoTien = item.TotalPrice ?? 0;

                trans.HoaHong = trans.SoTien * 0.03m;

                trans.NgayGD = item.CreatedAt ?? DateTime.Now;
                trans.TrangThai = item.Status ?? "Pending";

                trans.PTThanhToan = "Chuyển khoản";

                model.DanhSachGiaoDich.Add(trans);
            }

            return View(model);
        }
        public ActionResult Settings()
        {
            int currentAdminId = 1; 

            var admin = db.AdminUsers.Find(currentAdminId);
            if (admin == null) return HttpNotFound();

            var model = new AdminSettingsViewModel
            {
                AdminID = admin.AdminID,
                FullName = admin.FullName,
                Email = admin.Email
            };

            return View(model);
        }

        // POST: Xử lý lưu dữ liệu
        [HttpPost]
        public ActionResult Settings(AdminSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var admin = db.AdminUsers.Find(model.AdminID);
                if (admin != null)
                {
                    admin.FullName = model.FullName;
                    admin.Email = model.Email;

                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        if (admin.HashPassword == model.CurrentPassword)
                        {
                            admin.HashPassword = model.NewPassword; 
                        }
                        else
                        {
                            ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng!");
                            return View("Settings", model);
                        }
                    }

                    db.SaveChanges();
                    ViewBag.Message = "Cập nhật thành công!";
                }
            }
            return View("Settings", model);
        }


        public ActionResult Approve_RealEstate()
        {
            if (Session["UserRole"]?.ToString() != "Admin") return RedirectToAction("Login", "Account");
            var pendingList = db.Products
                                .Where(p => p.Status == "Pending")
                                .OrderByDescending(p => p.CreatedAt)
                                .ToList();

            return View(pendingList);


        }

        public ActionResult Confirm_Approve(int id)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                product.Status = "Active"; 
                db.SaveChanges();
            }
            return RedirectToAction("Approve_RealEstate");
        }

        public ActionResult Confirm_Reject(int id)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                db.Products.Remove(product);


                db.SaveChanges();
            }
            return RedirectToAction("Approve_RealEstate");
        }


        [HttpPost] // Bắt buộc dùng POST để bảo mật
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                // 1. Tìm sản phẩm
                var product = db.Products.Find(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Tin không tồn tại!" });
                }

                // 2. Xóa các dữ liệu liên quan trước (Cascade Delete thủ công để tránh lỗi FK)

                // Xóa ảnh trong bảng PropertyImage
                var images = db.PropertyImages.Where(x => x.ProductID == id).ToList();
                db.PropertyImages.RemoveRange(images);

                // Xóa yêu thích (Favorites)
                var favs = db.Favorites.Where(x => x.ProductID == id).ToList();
                db.Favorites.RemoveRange(favs);

                // Xóa bình luận (Reviews) - Nếu có
                var reviews = db.Reviews.Where(x => x.ProductID == id).ToList();
                db.Reviews.RemoveRange(reviews);

                // (Tùy chọn) Kiểm tra Hợp đồng (Contracts)
                // Nếu tin đã có hợp đồng thì KHÔNG cho xóa, chỉ cho ẩn (Status = Sold)
                bool hasContract = db.Contracts.Any(x => x.ProductID == id);
                if (hasContract)
                {
                    return Json(new { success = false, message = "Tin này đã có hợp đồng giao dịch, không thể xóa! Hãy chuyển trạng thái sang 'Đã bán'." });
                }

                // 3. Xóa sản phẩm
                db.Products.Remove(product);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

    }
}