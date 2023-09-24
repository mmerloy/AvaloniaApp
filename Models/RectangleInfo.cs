namespace AvaloniaFirstApp.Models;

public class RectangleInfo : ICloneable<RectangleInfo>
{
    public RectanglePoint? StartPoint { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public RectangleInfo Clone()
        => new()
        {
            StartPoint = StartPoint?.Clone(),
            Width = Width,
            Height = Height
        };
}
