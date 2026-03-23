namespace Verendar.Garage.Domain.ValueObjects;

/// <summary>Owned entity — embedded columns (Amount, Currency) on parent table.</summary>
public class Money
{
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "VND";
}
