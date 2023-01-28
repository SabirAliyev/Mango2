using Microsoft.AspNetCore.Identity;

namespace Mango.Models.ViewModels;

public class ProductUserVM
{
    public IdentityUser ApplicationUser { get; set; }
    public List<Product> ProductList { get; set; }

    public ProductUserVM()
    {
        ProductList = new List<Product>();
    }
}
