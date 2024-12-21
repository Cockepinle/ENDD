using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lepilina.Models
{
    public class Products
    {
        [Key]
        public int products_id { get; set; }
        public string name_products { get; set; }
        public string descriptions { get; set; }
        public decimal price { get; set; }
        public int stocks_quantity { get; set; }

        public int? category_id { get; set; }
        [ForeignKey("category_id")]
        public Category Category { get; set; }
        public List<Images> Images { get; set; } = new List<Images>();
    }
}
