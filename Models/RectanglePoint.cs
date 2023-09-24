namespace AvaloniaFirstApp.Models;

public class RectanglePoint : ICloneable<RectanglePoint>
{
    public double X { get; set; }
    public double Y { get; set; }

    public RectanglePoint Clone()
        => new()
        {
            X = X,
            Y = Y
        };
}
