-- Kiểm tra tất cả claims của users
SELECT 
    u.Email,
    u.UserName,
    c.ClaimType,
    c.ClaimValue
FROM AspNetUsers u
LEFT JOIN AspNetUserClaims c ON u.Id = c.UserId
WHERE u.Email LIKE '%gmail.com'
ORDER BY u.Email, c.ClaimType;

-- Nếu cần xóa user để test lại từ đầu (thay [email] bằng email thực tế):
-- DELETE FROM AspNetUserClaims WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email = '[email]');
-- DELETE FROM AspNetUserLogins WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email = '[email]');
-- DELETE FROM AspNetUserRoles WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email = '[email]');
-- DELETE FROM AspNetUsers WHERE Email = '[email]';
