-- Xóa toàn bộ user và role assignments
DELETE FROM AspNetUserRoles;
DELETE FROM AspNetUserClaims;
DELETE FROM AspNetUserLogins;
DELETE FROM AspNetUserTokens;
DELETE FROM AspNetUsers;

-- Xóa roles (optional - seed sẽ tạo lại)
DELETE FROM AspNetRoles;
