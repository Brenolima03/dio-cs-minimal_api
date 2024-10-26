using minimal_api.Domain.Entities;

namespace Test.Domain.Entities;

[TestClass]
public class AdminTest
{
  [TestMethod]
  public void TestGetSetProperties()
  {
    // Arrange
    var adm = new Admin
    {
      // Act
      Id = 1,
      Email = "adm@test.com",
      Password = "test",
      Profile = "Adm"
    };

    // Assert
    Assert.AreEqual(1, adm.Id);
    Assert.AreEqual("adm@test.com", adm.Email);
    Assert.AreEqual("test", adm.Password);
    Assert.AreEqual("Adm", adm.Profile);
  }
}
