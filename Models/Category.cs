using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PraktiskaisDarbs3.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = null!;

        public List<Item> Items { get; set; } = new();
    }
}
