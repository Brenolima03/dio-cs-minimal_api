using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infra.Db;

namespace minimal_api.Domain.Services;

public class AdminService(Db context) : IAdminService
{
  private readonly Db _context = context;

  public Admin? Login(LoginDTO loginDTO)
  {
    // Fetch matching admins
    var adm = _context.Admins.FirstOrDefault(
      admin => admin.Email == loginDTO.Email &&
      admin.Password == loginDTO.Password
    );
    return adm;
  }
  
  public Admin Insert(Admin admin)
  {
    _context.Admins.Add(admin);
    _context.SaveChanges();
    return admin;
  }
  public List<Admin> GetAll(int? page)
  {
    int pageSize = 10;

    var query = _context.Admins.AsQueryable();
    if (page != null)
      query = query.Skip(((int)page - 1) * pageSize).Take(pageSize);
    return [.. query];
  }
  public Admin? GetById(int id)
  {
    return _context.Admins.Where(admin => admin.Id == id).FirstOrDefault();
  }
}
