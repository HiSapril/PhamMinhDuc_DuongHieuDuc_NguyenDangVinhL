using Microsoft.AspNetCore.Mvc;

namespace ASCWeb1.Controllers
{
    public class AnonymousController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
