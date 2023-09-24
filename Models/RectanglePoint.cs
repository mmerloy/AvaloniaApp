using System.Diagnostics;

namespace AvaloniaFirstApp.Models;

[DebuggerDisplay($"X={{{nameof(X)}}}, Y={{{nameof(Y)}}}")]
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
