using System.Collections.Generic;

namespace Lepilina.Models
{
    public class OrderViewModel
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string ShippingAddress { get; set; }
        public List<CartItem> CartItems { get; set; }

    }
}
