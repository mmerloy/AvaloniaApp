namespace Domain.MethodConfigurations;

public class MethodConfiguration : Entity
{
    public MethodConfigType ConfigType { get; set; } = MethodConfigType.None;

    public double Inaccuracy { get; set; }
}
