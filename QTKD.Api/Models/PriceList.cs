namespace QTKD.Api.Models;

public sealed class PriceList
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly EffectiveDate { get; set; }
    public ICollection<PriceItem> PriceItems { get; set; } = new List<PriceItem>();
}
