# Hướng dẫn cấu hình Gmail để gửi email với MailKit

## Bước 1: Bật xác thực 2 bước (2-Step Verification)

1. Truy cập: https://myaccount.google.com/security
2. Tìm mục **"2-Step Verification"**
3. Click **"Get Started"** và làm theo hướng dẫn
4. Xác thực bằng số điện thoại hoặc ứng dụng Google Authenticator

## Bước 2: Tạo App Password

1. Sau khi bật 2-Step Verification, quay lại: https://myaccount.google.com/security
2. Tìm mục **"App passwords"** (Mật khẩu ứng dụng)
3. Có thể cần đăng nhập lại
4. Chọn:
   - **Select app**: Mail
   - **Select device**: Other (Custom name)
   - Nhập tên: "ASC Web Application"
5. Click **"Generate"**
6. Gmail sẽ hiển thị mật khẩu 16 ký tự (ví dụ: `abcd efgh ijkl mnop`)
7. **LƯU LẠI MẬT KHẨU NÀY** - bạn sẽ không thể xem lại

## Bước 3: Cập nhật appsettings.json

```json
{
  "ApplicationSettings": {
    "SMTPServer": "smtp.gmail.com",
    "SMTPPort": "587",
    "SMTPAccount": "your-email@gmail.com",
    "SMTPPassword": "abcdefghijklmnop"
  }
}
```

**Lưu ý:**
- `SMTPAccount`: Email Gmail thực của bạn (ví dụ: `myemail@gmail.com`)
- `SMTPPassword`: App Password 16 ký tự **KHÔNG CÓ KHOẢNG TRẮNG** (ví dụ: `abcdefghijklmnop`)
- `SMTPServer`: Luôn là `smtp.gmail.com`
- `SMTPPort`: Luôn là `587` (STARTTLS)

## Bước 4: Kiểm tra code AuthMessageSender.cs

Code đã được cấu hình đúng:

```csharp
await client.ConnectAsync(
    _configuration["ApplicationSettings:SMTPServer"],  // smtp.gmail.com
    int.Parse(_configuration["ApplicationSettings:SMTPPort"]),  // 587
    MailKit.Security.SecureSocketOptions.StartTls);  // STARTTLS

await client.AuthenticateAsync(
    _configuration["ApplicationSettings:SMTPAccount"],  // your-email@gmail.com
    _configuration["ApplicationSettings:SMTPPassword"]);  // app password
```

## Bước 5: Test gửi email

1. Chạy ứng dụng
2. Đăng nhập với tài khoản admin hoặc engineer
3. Click vào menu **"Reset Password"**
4. Kiểm tra email (cả hộp thư đến và spam)

## Xử lý lỗi thường gặp

### Lỗi: "Authentication failed"
- **Nguyên nhân**: Sai App Password hoặc chưa bật 2-Step Verification
- **Giải pháp**: 
  - Kiểm tra lại App Password (16 ký tự, không có khoảng trắng)
  - Đảm bảo đã bật 2-Step Verification
  - Tạo lại App Password mới

### Lỗi: "Connection refused"
- **Nguyên nhân**: Sai SMTP Server hoặc Port
- **Giải pháp**: 
  - Đảm bảo `SMTPServer` = `smtp.gmail.com`
  - Đảm bảo `SMTPPort` = `587`

### Lỗi: "Less secure app access"
- **Nguyên nhân**: Gmail không cho phép ứng dụng kém bảo mật
- **Giải pháp**: 
  - **KHÔNG** bật "Less secure app access" (không an toàn)
  - **PHẢI** dùng App Password (an toàn hơn)

### Email vào Spam
- **Nguyên nhân**: Email từ ứng dụng thường bị đánh dấu spam
- **Giải pháp**: 
  - Kiểm tra cả hộp thư Spam
  - Đánh dấu "Not spam" để email sau vào hộp thư đến
  - Trong production, nên dùng dịch vụ email chuyên nghiệp (SendGrid, AWS SES, etc.)

## Thông tin bổ sung

### Giới hạn gửi email của Gmail:
- **Tài khoản Gmail thường**: 500 email/ngày
- **Google Workspace**: 2000 email/ngày

### Các SMTP Server khác:
- **Outlook/Hotmail**: `smtp-mail.outlook.com`, Port `587`
- **Yahoo**: `smtp.mail.yahoo.com`, Port `587`
- **Office 365**: `smtp.office365.com`, Port `587`

### Khuyến nghị cho Production:
Không nên dùng Gmail cá nhân cho production. Nên dùng:
- **SendGrid** (https://sendgrid.com) - 100 email/ngày miễn phí
- **AWS SES** (https://aws.amazon.com/ses/) - 62,000 email/tháng miễn phí
- **Mailgun** (https://www.mailgun.com) - 5,000 email/tháng miễn phí
- **SMTP2GO** (https://www.smtp2go.com) - 1,000 email/tháng miễn phí

## Bảo mật

**QUAN TRỌNG:**
- **KHÔNG** commit `appsettings.json` có chứa App Password lên Git
- Dùng **User Secrets** cho development:
  ```bash
  dotnet user-secrets set "ApplicationSettings:SMTPPassword" "your-app-password"
  ```
- Dùng **Environment Variables** hoặc **Azure Key Vault** cho production
