namespace PraktiskaisDarbs3.Models
{
    public class QueriesViewModel
    {
        public Item? LowestStockItem { get; set; }
        public Item? HighestStockItem { get; set; }
        public Order? NewestOrder { get; set; }

        public List<Item>? ItemsByCategory { get; set; }
        public List<Item>? ItemsByNameSearch { get; set; }
        public List<Order>? OrdersByItemName { get; set; }

        public string? CategoryNameInput { get; set; }
        public string? ItemNameInput { get; set; }
        public string? OrderItemSearch { get; set; }
    }
}
