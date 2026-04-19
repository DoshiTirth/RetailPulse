using System.ComponentModel.DataAnnotations;

namespace RetailPulse.Web.Models.ViewModels;

public class CreateOrderViewModel
{
    [Required]
    public int CustomerId { get; set; }

    public string? Notes { get; set; }

    public List<OrderLineItem> Items { get; set; } = Enumerable
        .Range(0, 5)
        .Select(_ => new OrderLineItem())
        .ToList();
}

public class OrderLineItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}