using ASCWeb1.Data;
using ASCWeb1.Models;
using Microsoft.AspNetCore.Mvc;

namespace ASCWeb1.Navigation
{
    [ViewComponent(Name = "ASCWeb1.Navigation.LeftNavigation")]
    public class LeftNavigationViewComponent : ViewComponent
    {
        private readonly INavigationCacheOperations _navigationCache;

        public LeftNavigationViewComponent(INavigationCacheOperations navigationCache)
        {
            _navigationCache = navigationCache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menu = await _navigationCache.GetNavigationCacheAsync();
            menu.MenuItems = menu.MenuItems.OrderBy(p => p.Sequence).ToList();
            return View(menu);
        }
    }
}
