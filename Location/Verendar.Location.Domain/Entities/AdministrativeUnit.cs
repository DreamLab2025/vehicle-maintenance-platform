namespace Verendar.Location.Domain.Entities;

public class AdministrativeUnit
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Abbreviation { get; set; }
}
