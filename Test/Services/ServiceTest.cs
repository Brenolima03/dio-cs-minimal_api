using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Services;
using minimal_api.Infra.Db;

namespace Test.Domain.Entities;

[TestClass]
public class AdminServiceTest
{
  public TestContext TestContext { get; set; } = null!;

  // Creates an in-memory database context for testing purposes
  private static Db CreateInMemoryDatabaseContext()
  {
    var dbContextOptions = new DbContextOptionsBuilder<Db>()
      .UseInMemoryDatabase(databaseName: "TestDatabase")
      .Options;

    var inMemoryDbContext = new Db(
      dbContextOptions, new ConfigurationBuilder().Build()
    );

    // Ensure the database schema is created and
    // the Admin entity is included in the model
    inMemoryDbContext.Database.EnsureCreated();

    return inMemoryDbContext;
  }

  // Test to save an Admin record to the in-memory database
  [TestMethod]
  public void TestSaveAdmin()
  {
    // Arrange: Create the in-memory database context
    var inMemoryDbContext = CreateInMemoryDatabaseContext();

    // Clear the Admins DbSet in the in-memory database to ensure a clean state
    inMemoryDbContext.Set<Admin>().RemoveRange(inMemoryDbContext.Set<Admin>());
    inMemoryDbContext.SaveChanges();

    // Prepare a new Admin instance to insert
    var adminToInsert = new Admin
    {
      Id = 1,
      Email = "adm@test.com",
      Password = "test",
      Profile = "Adm"
    };

    var adminService = new AdminService((Db)inMemoryDbContext);

    // Act: Insert the Admin record into the database
    adminService.Insert(adminToInsert);

    // Assert: Verify that exactly one Admin exists in the database
    var adminCount = adminService.GetAll(1).Count;
    Assert.AreEqual(1, adminCount);
  }

  // Test to retrieve an Admin by its ID from the in-memory database
  [TestMethod]
  public void TestGetById()
  {
    // Arrange: Create the in-memory database context
    var inMemoryDbContext = CreateInMemoryDatabaseContext();

    // Clear the Admins DbSet in the in-memory database to ensure a clean state
    inMemoryDbContext.Set<Admin>().RemoveRange(inMemoryDbContext.Set<Admin>());
    inMemoryDbContext.SaveChanges();

    // Prepare a new Admin instance to insert
    var adminToTest = new Admin
    {
      Email = "adm@test.com",
      Password = "test",
      Profile = "Adm"
    };

    var adminService = new AdminService((Db)inMemoryDbContext);

    // Act: Insert the Admin record into the database
    adminService.Insert(adminToTest);

    // Retrieve the Admin record from the database using its ID
    var retrievedAdmin = adminService.GetById(adminToTest.Id);

    // Assert: Verify that the retrieved Admin ID matches the original Admin ID
    Assert.AreEqual(adminToTest.Id, retrievedAdmin?.Id);
  }
}
