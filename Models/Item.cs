using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PraktiskaisDarbs3.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = null!;

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
