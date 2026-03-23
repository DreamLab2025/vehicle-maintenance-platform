namespace Verendar.Garage.Domain.ValueObjects;

public class Address
{
    [Required, MaxLength(10)]
    public string ProvinceCode { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string WardCode { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string StreetDetail { get; set; } = string.Empty;
}
