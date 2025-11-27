using System.ComponentModel.DataAnnotations;
namespace Website_BDS.Models.ViewModel
{
    public class AdminSettingsViewModel
    {
        // 1. Thông tin cá nhân
        public int AdminID { get; set; }

        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        // 2. Đổi mật khẩu (Không bắt buộc nhập nếu chỉ sửa tên)
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }

        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}
