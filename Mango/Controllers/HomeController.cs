using Mango.Data;
using Mango.Models;
using Mango.Models.ViewModels;
using Mango.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Mango.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                Products = _db.Product.Include(u => u.Category),
                Categories = _db.Category
            };

            return View(homeVM);
        }

        private List<ShoppingCart> GetShoppingCartList()
        {
            var shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.SessionCart);

            if (shoppingCartList != null && shoppingCartList.Count() > 0) {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.SessionCart);
            }

            shoppingCartList ??= new List<ShoppingCart>(); // (if shoppingCartList == null)
            return shoppingCartList;
        }

        public IActionResult Details(int id)
        {
            List<ShoppingCart> shoppingCartList = GetShoppingCartList();

            DetailsVM detailsVM = new DetailsVM();
            detailsVM.Product = _db.Product.Include(u => u.Category).Where(u => u.Id == id).FirstOrDefault();

            // Checking wether Shopping Contains this Product
            if (shoppingCartList != null) {
                foreach (var item in shoppingCartList) {
                    if (item.ProductId == id) {
                        detailsVM.ExistInCard = true;
                    }
                }
            }

            return View(detailsVM);
        }

        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id)
        {
            List<ShoppingCart> shoppingCartList = GetShoppingCartList();
                
            shoppingCartList?.Add(new ShoppingCart { ProductId = id });
            HttpContext.Session.Set(WebConstants.SessionCart, shoppingCartList);

            return RedirectToAction(nameof(Index));
        }
        
        public IActionResult RemoveFromCart(int id)
        {
            List<ShoppingCart> shoppingCartList = GetShoppingCartList();

            var removalItem = shoppingCartList?.SingleOrDefault(r => r.ProductId == id);
            if (removalItem != null) {
                shoppingCartList?.Remove(removalItem);
            }

            HttpContext.Session.Set(WebConstants.SessionCart, shoppingCartList);

            return RedirectToAction(nameof(Index));
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