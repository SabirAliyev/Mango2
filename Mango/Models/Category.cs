using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mango.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name  { get; set; }

    [DisplayName("Display Order")]
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Display Order must be greather then 0")]
    public string DisplayOrder { get; set; }
}
