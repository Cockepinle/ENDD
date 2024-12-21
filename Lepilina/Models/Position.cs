using System.ComponentModel.DataAnnotations;

namespace Lepilina.Models
{
    public class Position
    {
        [Key]
        public int position_id {  get; set; }
        public string position { get; set; }
    }
}
