using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;

namespace Test.Mocks;

public class AdminServiceMock : IAdminService
{
  private static readonly List<Admin> admins =
    [
      new Admin
      {
        Id = 1,
        Email = "adm@test.com",
        Password = "123456",
        Profile = "Adm"
      },
      new Admin
      {
        Id = 2,
        Email = "editor@test.com",
        Password = "123456",
        Profile = "Editor"
      }
    ];

  public Admin? Login(LoginDTO loginDTO)
  {
    var admin = admins.Find(
      a => a.Email == loginDTO.Email && a.Password == loginDTO.Password
    );

    // Optional: Log or throw an error for debugging
    if (admin == null)
    {
      // Log or handle the invalid login attempt
      Console.WriteLine($"Invalid login attempt for email: {loginDTO.Email}");
    }

    return admin;
  }

  // Inserts a new admin into the list and assigns a new ID
  public Admin Insert(Admin admin)
  {
    admin.Id = admins.Count + 1;
    admins.Add(admin);
    return admin;
  }

  public List<Admin> GetAll(int? page)
  {
    return admins;
  }

  // Retrieves an admin by ID from the list
  public Admin? GetById(int id)
  {
    return admins.Find(a => a.Id == id);
  }
}
