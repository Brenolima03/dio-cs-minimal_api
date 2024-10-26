namespace minimal_api.Domain.ModelViews;

public record LoggedInAdm
{
  public string Email { get; set; } = default!;
  public string Profile { get; set; } = default!;
  public string Token { get; set; } = default!;
}
