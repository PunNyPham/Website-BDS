using System.Web;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

public class CloudinaryService
{
    private static readonly Account account = new Account(
        "dzohe9x2k", // Thay bằng Cloud Name
        "959156927325493",       // Thay bằng API Key
        "Ib3sb537-INy6MOsmAIPQH1pZ2g"     // Thay bằng API Secret
    );

    private static readonly Cloudinary cloudinary = new Cloudinary(account);

    public static string UploadImage(HttpPostedFileBase file)
    {
        if (file == null || file.ContentLength == 0) return null;

        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, file.InputStream),
            Folder = "bds_vietnam_products" // Tên thư mục bạn muốn tạo trên Cloudinary
        };

        var uploadResult = cloudinary.Upload(uploadParams);

        // Trả về đường dẫn ảnh (URL) để lưu vào Database
        return uploadResult.SecureUrl.ToString();
    }
}