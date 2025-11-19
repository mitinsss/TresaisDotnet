using System.ComponentModel.DataAnnotations;

namespace PraktiskaisDarbs3.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        [Required]
        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Amount { get; set; }
    }
}

