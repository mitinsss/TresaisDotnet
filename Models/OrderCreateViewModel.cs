using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PraktiskaisDarbs3.Models
{
    public class OrderCreateViewModel
    {
        public List<OrderItemInput> OrderItems { get; set; } = new() { new OrderItemInput() };
    }

    public class OrderItemInput
    {
        [Required(ErrorMessage = "Prece ir obligāta")]
        public int? ItemId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Daudzumam jābūt lielākam par 0")]
        public int Amount { get; set; } = 1;
    }
}

