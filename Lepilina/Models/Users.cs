using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lepilina.Models
{
    public class Users
    {
        [Key]
        public int users_id { get; set; }
        public string sername { get; set; }
        public string names { get; set; }
        public string patronymic { get; set; }
        public int? position_id { get; set; }
        [ForeignKey("position_id")]
        public Position Position { get; set; }
    }
}
