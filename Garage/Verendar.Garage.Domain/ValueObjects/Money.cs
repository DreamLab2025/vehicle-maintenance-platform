namespace Verendar.Garage.Domain.ValueObjects;

public class Money
{
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "VND";
}
