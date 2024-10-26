using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infra.Db;

namespace minimal_api.Domain.Services;

public class VehicleService(Db context) : IVehicleService
{
  private readonly Db _context = context;
  public void Insert(Vehicle vehicle)
  {
    _context.Vehicles.Add(vehicle);
    _context.SaveChanges();
  }
  public List<Vehicle> GetAll(int page = 1, string? model = null, string? brand = null)
  {
    int pageSize = 10;

    var query = _context.Vehicles.AsQueryable();

    if (!string.IsNullOrEmpty(model))
    {
      query = query.Where(v => v.Model.Contains(model));
    }
    if (!string.IsNullOrEmpty(brand))
    {
      query = query.Where(v => v.Brand.Contains(brand));
    }

    // Apply pagination
    return [.. query
    .Skip((page - 1) * pageSize)  // Skip previous pages
    .Take(pageSize)];
  }

  public Vehicle? GetById(int id)
  {
    return _context.Vehicles.Where(vehicle => vehicle.Id == id).FirstOrDefault();
  }

  public void Update(Vehicle vehicle)
  {
    _context.Vehicles.Update(vehicle);
    _context.SaveChanges();
  }
  public void Delete(Vehicle vehicle)
  {
    _context.Vehicles.Remove(vehicle);
    _context.SaveChanges();
  }
}
