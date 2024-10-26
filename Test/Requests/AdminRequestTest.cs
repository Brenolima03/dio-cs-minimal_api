using minimal_api.Domain.ModelViews;
using minimal_api.Domain.DTOs;
using System.Text.Json;
using System.Text;
using System.Net;
using Test.Helpers;

namespace Test.Requests;

[TestClass]
public class AdminRequestTest
{
  [ClassInitialize]
  public static void ClassInit(TestContext testContext)
  {
    Setup.ClassInit(testContext);
  }

  [ClassCleanup]
  public static void ClassCleanup()
  {
    Setup.ClassCleanup();
  }

  [TestMethod]
  public async Task TestGetSetProperties()
  {
    // Arrange
    var loginDTO = new LoginDTO
    {
      Email = "adm@test.com",
      Password = "123456"
    };

    var content = new StringContent(
      JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "application/json"
    );

    // Act
    var response = await Setup.client.PostAsync("/admins/login", content);

    var responseContent = await response.Content.ReadAsStringAsync();

    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

    var loggedInAdmin = JsonSerializer.Deserialize<LoggedInAdm>(
      responseContent,
      new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
    );

    Assert.IsNotNull(loggedInAdmin?.Email);
    Assert.IsNotNull(loggedInAdmin?.Profile);
    Assert.IsNotNull(loggedInAdmin?.Token);

    Console.WriteLine(loggedInAdmin?.Token);
  }
}