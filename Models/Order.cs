using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PraktiskaisDarbs3.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
