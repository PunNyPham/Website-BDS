using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Web;
using System.Net;
using System;

public static class CloudinaryService
{
    // Đảm bảo thông tin này chuẩn 100% (Copy paste cẩn thận)
    private static readonly string CloudName = "dsteobggx";
    private static readonly string ApiKey = "144995475238323";
    private static readonly string ApiSecret = "10_PSpmdRintu7nx-eOgamCmdXo"; // Nhớ check khoảng trắng

    public static string UploadImage(HttpPostedFileBase file)
    {
        // 1. Ép buộc dùng giao thức bảo mật mới nhất (TLS 1.2)
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        if (file == null || file.ContentLength == 0) return "ERROR: File rỗng";

        try
        {
            Account account = new Account(CloudName, ApiKey, ApiSecret);
            Cloudinary cloudinary = new Cloudinary(account);

            // Reset vị trí đọc file về đầu (Tránh lỗi file đã bị đọc trước đó)
            file.InputStream.Position = 0;

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.InputStream),
                Folder = "BDS_Images"
            };

            var uploadResult = cloudinary.Upload(uploadParams);

            // Kiểm tra lỗi từ phía Cloudinary trả về
            if (uploadResult.Error != null)
            {
                return "ERROR_CLOUD: " + uploadResult.Error.Message;
            }

            return uploadResult.SecureUrl.AbsoluteUri;
        }
        catch (Exception ex)
        {
            // Lỗi do code hoặc mạng
            return "ERROR_SYS: " + ex.Message + (ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : "");
        }
    }
}