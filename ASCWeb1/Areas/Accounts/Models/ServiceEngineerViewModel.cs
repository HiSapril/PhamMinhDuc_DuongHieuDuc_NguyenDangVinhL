
using Microsoft.AspNetCore.Identity;

namespace ASCWeb1.Areas.Accounts.Models
{
    public class ServiceEngineerViewModel
    {
        public List<IdentityUser>? ServiceEngineers { get; set; }  // Lưu trữ danh sách nhân viên

        public ServiceEngineerRegistrationViewModel Registration { get; set; } = new();  // Lưu trữ nhân viên thêm mới hoặc cập nhật
    }
}
