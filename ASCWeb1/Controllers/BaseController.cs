using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASCWeb1.Controllers
{
    public class BaseController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
