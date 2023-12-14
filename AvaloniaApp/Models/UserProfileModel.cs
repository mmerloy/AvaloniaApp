using Domain.MethodConfigurations;

namespace AvaloniaFirstApp.Models;

public class UserProfileModel
{
    public string Title { get; set; } = null!;

    public int Id { get; set; }

    public MethodConfigType MethodConfigType { get; set; }

    public SearchObjectType SearchObjectType { get; set; }
}
