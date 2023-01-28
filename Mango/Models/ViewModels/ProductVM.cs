using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Mango.Models.ViewModels;

public class ProductVM
{
    public Product Product { get; set; }
    //public Category Category { get; set; }
    //public string Image { get; set; }
    public IEnumerable<SelectListItem> CategorySelectList { set; get; }

    public ProductVM()
    {
        Product = new Product();
        //Category = new Category();
        //Image = "";
        CategorySelectList = new List<SelectListItem>();
    }
}
