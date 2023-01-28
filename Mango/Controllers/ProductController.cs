using Mango.Data;
using Mango.Models;
using Mango.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Mango.Controllers;

[Authorize(Roles = WebConstants.AdminRole)]
public class ProductController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> objList = _db.Product.Include(u=>u.Category);
        
        return View(objList);
    }

    // GET - Upsert (universal method for Create and Edit Product)
    public IActionResult Upsert(int? id)
    {
        ProductVM productVM = new ProductVM()
        {
            Product = new Product(),
            CategorySelectList = _db.Category.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            }),
        };


        if (id == null) {
            // this is for create
            return View(productVM);
        } else {
            productVM.Product = _db.Product.Find(id);
            if (productVM.Product == null) {
                return NotFound();
            }

            return View(productVM);
        }
    }

    //POST - Upsert
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(ProductVM productVM)
    {
        ModelState.Remove("Product.Category");
        ModelState.Remove("Product.Image");
        // Temporary ModelState Validation fix - TODO in Product ViewModel.

        if (ModelState.IsValid) {
            IFormFileCollection files = HttpContext.Request.Form.Files;
            string webRootPath = _webHostEnvironment.WebRootPath;

            if (productVM.Product.Id == 0) {
                // creating
                string upload = webRootPath + WebConstants.ImagePath;
                string fileName = Guid.NewGuid().ToString();
                string extention = Path.GetExtension(files[0].FileName);

                using (var fileStream = new FileStream(Path.Combine(upload, fileName + extention), FileMode.Create)) {
                    files[0].CopyTo(fileStream);
                }

                productVM.Product.Image = fileName + extention;

                _db.Product.Add(productVM.Product);

            }
            else {
                // update
                Product objFromDB = _db.Product.AsNoTracking().FirstOrDefault(u => u.Id == productVM.Product.Id);
                //( IMPORTANT! ) - AsNoTracking() method used to prevent EF of confusing witch object to be updated.

                if (objFromDB != null) {
                    if (files.Count > 0) {
                        string upload = webRootPath + WebConstants.ImagePath;
                        string fileName = Guid.NewGuid().ToString();
                        string extention = Path.GetExtension(files[0].FileName);

                        string oldFile = Path.Combine(upload, objFromDB.Image);

                        if (System.IO.File.Exists(oldFile)) {
                            System.IO.File.Delete(oldFile);
                        }

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extention), FileMode.Create)) {
                            files[0].CopyTo(fileStream);
                        }

                        productVM.Product.Image = fileName + extention;
                    }
                    else {
                        productVM.Product.Image = objFromDB.Image;
                    }

                    _db.Product.Update(productVM.Product);
                }
            }

            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        productVM.CategorySelectList = _db.Category.Select(i => new SelectListItem
        {
            Text = i.Name,
            Value = i.Id.ToString()
        });

        return View(productVM);
    }


    // GET - Delete
    public IActionResult Delete(int? id)
    {
        if (id == null || id == 0) {
            return View();
        }

        // using "eager loading" with JOIN specific Category
        Product product = _db.Product.Include(u=>u.Category).FirstOrDefault(u=>u.Id==id);        

        if (product == null) {
            return View();
        }

        return View(product);
    }

    //POST - Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int? id)
    {
        var objFromDB = _db.Product.Find(id);
        if (objFromDB == null) {
            return NotFound();
        }

        if (objFromDB.Image != null) {
            string imageRepo = _webHostEnvironment.WebRootPath + WebConstants.ImagePath;

            string oldFile = Path.Combine(imageRepo, objFromDB.Image);

            if (System.IO.File.Exists(oldFile)) {
                System.IO.File.Delete(oldFile);
            }
        }

        _db.Product.Remove(objFromDB);
        _db.SaveChanges();

        return RedirectToAction("Index");
    }


}
