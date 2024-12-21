using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lepilina.Models
{
    public class Images
    {
        [Key]
        public int images_id { get; set; }
        public string image_data { get; set; }
        public int? products_id { get; set; }
        [ForeignKey("products_id")]
        public Products Products { get; set; }
    }
}
