using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lepilina.Models
{
    public class Accounts
    {
        [Key]
        public int accounts_id { get; set; }
        public string logins { get; set; }
        public string passwords { get; set; }
        public int? users_id { get; set; }
        [ForeignKey("users_id")]
        public Users Users { get; set; }
    }
}
