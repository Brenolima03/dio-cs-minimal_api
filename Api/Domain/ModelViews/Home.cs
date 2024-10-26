namespace minimal_api.Domain.ModelViews;

public struct Home
{
  public readonly string WelcomeMessage { get => "Welcome!"; }
  public readonly string Doc { get => "/swagger"; }
}