namespace Domain.MethodConfigurations;

public enum MethodConfigType : byte
{
    None = 0,
    Interpolation = 1,
    Recursion,
    Weighted
}
