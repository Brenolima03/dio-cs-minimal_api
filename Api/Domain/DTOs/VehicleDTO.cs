namespace minimal_api.Domain.DTOs;

public record VehicleDTO
{
  public int Id { get; set; } = default!;
  public string Model { get; set; } = default!;
  public string Brand { get; set; } = default!;
  public int Year { get; set; } = default!;
}
