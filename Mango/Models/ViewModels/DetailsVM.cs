namespace Mango.Models.ViewModels;

public class DetailsVM
{
    public Product Product { get; set; }
    public bool ExistInCard { get; set; }

    public DetailsVM()
    {
        Product = new Product();
    }
}
