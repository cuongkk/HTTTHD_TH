namespace QTKD.Api.Models;

public sealed class PriceItem
{
    public int PriceListId { get; set; }
    public PriceList PriceList { get; set; } = null!;
    public string ServiceCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}
