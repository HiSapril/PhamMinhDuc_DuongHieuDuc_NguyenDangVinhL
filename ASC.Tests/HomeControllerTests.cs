using ASCWeb1.Configuration;
using ASCWeb1.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http; // Thêm thư viện này
using Moq;
using Xunit;
using ASC.Tests.TestUtilities; // Namespace chứa FakeSession
using ASC.Utilities;       // Namespace chứa SessionExtensions

namespace ASC.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<IOptions<ApplicationSettings>> optionsMock;
        private readonly Mock<ILogger<HomeController>> loggerMock;
        private readonly Mock<HttpContext> mockHttpContext; // THÊM MỚI

        public HomeControllerTests()
        {
            optionsMock = new Mock<IOptions<ApplicationSettings>>();
            loggerMock = new Mock<ILogger<HomeController>>();

            // 1. Khởi tạo Mock HttpContext
            mockHttpContext = new Mock<HttpContext>();

            // 2. Thiết lập Session giả cho HttpContext
            mockHttpContext.Setup(p => p.Session).Returns(new FakeSession());

            optionsMock.Setup(ap => ap.Value).Returns(new ApplicationSettings
            {
                ApplicationTitle = "ASC",
                AdminEmail = "admin@test.com",
                AdminName = "Admin",
                AdminPassword = "Admin123!",
                Roles = "Admin,User,Engineer",
                EngineerEmail = "engineer@test.com",
                EngineerName = "Engineer",
                EngineerPassword = "Engineer123!",
                SMTPServer = "smtp.test.com",
                SMTPPort = 587,
                SMTPAccount = "test@test.com",
                SMTPPassword = "password"
            });
        }

        [Fact]
        public void HomeController_Index_View_Test()
        {
            var controller = new HomeController(loggerMock.Object, optionsMock.Object);
            // Gán HttpContext giả vào Controller
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            var result = controller.Index();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void HomeController_Index_NoModel_Test()
        {
            var controller = new HomeController(loggerMock.Object, optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            var viewResult = controller.Index() as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Null(viewResult.ViewData.Model);
        }

        [Fact]
        public void HomeController_Index_Validation_Test()
        {
            var controller = new HomeController(loggerMock.Object, optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            var viewResult = controller.Index() as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(0, viewResult.ViewData.ModelState.ErrorCount);
        }

        [Fact] // TEST CASE MỚI CHO SESSION
        public void HomeController_Index_Session_Test()
        {
            // Arrange
            var controller = new HomeController(loggerMock.Object, optionsMock.Object);

            // BẮT BUỘC: Gán HttpContext giả vào trước khi gọi Index
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // Act
            controller.Index();

            // Assert
            // Kiểm tra xem Controller có thực sự lưu settings vào key "Test" không
            var sessionValue = controller.HttpContext.Session.GetSession<ApplicationSettings>("Test");

            Assert.NotNull(sessionValue);
            Assert.Equal("ASC", sessionValue.ApplicationTitle);

        }
    }
}