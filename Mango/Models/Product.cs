using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [MaxLength(100)]
    public string Description { get; set; }

    public string Image { get; set; }

    [Range(1, int.MaxValue)]
    public double Price { get; set; }

    [Required]
    [Range(0, 5, ErrorMessage = "Status must be in range of 0 - 5")]
    public int Status { get; set; }

    [DisplayName("Category Type")]
    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; }

    public bool IsManaged { get; set; }
}
