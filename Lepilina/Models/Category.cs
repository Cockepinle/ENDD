using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lepilina.Models
{
    [Table("Category", Schema = "dbo")]
    public class Category
    {
        [Key]
        public int category_id { get; set; }
        public string name_category { get; set; }
        public ICollection<Products> Products { get; set; }
    }
}
