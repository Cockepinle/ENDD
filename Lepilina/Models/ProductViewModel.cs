using System.ComponentModel.DataAnnotations;

namespace Lepilina.Models
{
    public class ProductViewModel
    {
        [Key]
        public Products Product { get; set; }
        public List<Images> Images { get; set; } = new List<Images>();
    }
}
