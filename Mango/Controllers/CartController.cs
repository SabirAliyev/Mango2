using Mango.Data;
using Mango.Models;
using Mango.Models.ViewModels;
using Mango.Utility; // using our own implementation
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Security.Claims;
using System.Text;

namespace Mango.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ApplicationDbContext _db;

    private readonly IWebHostEnvironment _webHostEnvironment;

    private readonly IEmailSender _emailSender;

    /// <summary>
    /// BindProperty allows us not to psecify this field in action methods as a parameter
    /// </summary>
    [BindProperty]
    public ProductUserVM ProductUserVM { get; set; }

    public CartController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
    {
        _db= db;
        _webHostEnvironment= webHostEnvironment;
        _emailSender= emailSender;
    }


    public IActionResult Index()
    {
        List<ShoppingCart> shoppingCarttList = new List<ShoppingCart>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.SessionCart) != null 
            && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.SessionCart).Count() > 0) 
        {
            shoppingCarttList = HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.SessionCart).ToList();
        }
        
        List<int> prodsInCart = shoppingCarttList.Select(i => i.ProductId).ToList();

        // Get products list from DB.
        IEnumerable<Product> productList = _db.Product.Where(u => prodsInCart.Contains(u.Id));

        return View(productList);
    }

    public IActionResult Remove(int id)
    {
        List<ShoppingCart> shoppingCarttList = new List<ShoppingCart>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.SessionCart) != null
            && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.SessionCart).Count() > 0) {
            shoppingCarttList = HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.SessionCart).ToList();
        }

        // remove approptiate product from cart list
        shoppingCarttList.Remove(shoppingCarttList.FirstOrDefault(u => u.ProductId == id));

        // update shopping cart list
        HttpContext.Session.Set(WebConstants.SessionCart, shoppingCarttList);

        // redirect to Action Index
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName(nameof(Index))]
    public IActionResult IndexPost()
    {
        return RedirectToAction(nameof(Summary));
    }

    public IActionResult Summary()
    {
        ClaimsIdentity claimIdentity = (ClaimsIdentity)User.Identity;

        // if User logged into the system we can get user identifer
        Claim claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

        // another way go get user identity id
        // string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        List<ShoppingCart> shoppingCarttList = new List<ShoppingCart>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.SessionCart) != null
            && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.SessionCart).Count() > 0) {
            shoppingCarttList = HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.SessionCart).ToList();
        }

        List<int> prodsInCart = shoppingCarttList.Select(i => i.ProductId).ToList();
        IEnumerable<Product> productList = _db.Product.Where(u => prodsInCart.Contains(u.Id));

        // query debug
        //var query = _db.ApplicationUser
        //    .Where(r => r.Id == claim.Value)
        //    .Select(r => r);

        // using claim object value we get identity information of logged user and put it into ProductUser View Model
        ProductUserVM = new ProductUserVM()
        {
            ApplicationUser = _db.ApplicationUser.FirstOrDefault(u => u.Id == claim.Value),
            ProductList = productList.ToList()
        };

        return View(ProductUserVM);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName(nameof(Summary))]
    public async Task<IActionResult> SummaryPost(ProductUserVM ProductUserVM)
    {
        string pathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
            + "templates" + Path.DirectorySeparatorChar.ToString() +
            "Inquiry.html";

        string subject = "New Inquiry";
        string htmlBody = "";
        using (StreamReader sr = System.IO.File.OpenText(pathToTemplate)) {
            htmlBody = sr.ReadToEnd();
        }

        //Name: { 0}
        //Email: { 1}
        //Phone: { 2}
        //Products: { 3}

        // filling Enquiry html templpate by user data
        StringBuilder productListSB = new StringBuilder();
        foreach (var prod in ProductUserVM.ProductList) {
            productListSB.Append($" - Name: {prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})</span><br />");
        }
        string messageBody = string.Format(
            ProductUserVM.ApplicationUser.UserName,
            ProductUserVM.ApplicationUser.Email,
            ProductUserVM.ApplicationUser.PhoneNumber,
            productListSB.ToString());

        await _emailSender.SendEmailAsync(WebConstants.EmailAdmin, subject, messageBody);

        return RedirectToAction(nameof(InquiryConfirmation));
    }

    public IActionResult InquiryConfirmation()
    {
        HttpContext.Session.Clear();

        return View(ProductUserVM);
    }
}
