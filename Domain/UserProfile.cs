using AvaloniaFirstApp.Models;
using Domain.MethodConfigurations;

namespace Domain;

public class UserProfile : Entity
{
    public string Title { get; set; } = "Без названия";

    public MethodConfiguration MethodConfiguration { get; set; } = null!;

    public SearchObjectType SearchObjectType { get; set; }
}
