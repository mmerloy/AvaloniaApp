using AvaloniaFirstApp.ViewModels;

namespace AvaloniaFirstApp.Models;

public class UserProfileModel
{
    public string Title { get; set; } = null!;

    public int Id { get; set; }

    public MethodConfigurationViewModel MethodConfig { get; set; } = null!;

    public SearchObjectType SearchObjectType { get; set; }
}
