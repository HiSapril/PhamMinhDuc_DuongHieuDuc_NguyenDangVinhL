using ASCWeb1.Configuration;
using ASCWeb1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
// 1. Thêm namespace này để dùng được GetSession và SetSession
using ASC.Utilities;

namespace ASCWeb1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOptions<ApplicationSettings> _settings;

        public HomeController(ILogger<HomeController> logger, IOptions<ApplicationSettings> settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public IActionResult Index()
        {
            // 2. SET SESSION: Lưu đối tượng settings vào Session
            HttpContext.Session.SetSession("Test", _settings.Value);

            // 3. GET SESSION: Thử lấy ra để kiểm tra
            var settings = HttpContext.Session.GetSession<ApplicationSettings>("Test");

            // 4. Gán Title cho View từ settings
            ViewBag.Title = _settings.Value.ApplicationTitle;

            return View();
        }

        public IActionResult Dashboard()
        {
            // Lấy dữ liệu từ Session ra để hiển thị hoặc kiểm tra
            var firstAccess = HttpContext.Session.GetSession<string>("FirstAccess");
            ViewData["Message"] = "Bạn đã truy cập lần đầu vào lúc: " + firstAccess;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}