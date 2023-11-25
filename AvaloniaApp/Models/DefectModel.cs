using Domain.Defects;

namespace AvaloniaFirstApp.Models;

public class DefectModel
{
    public int Id { get; set; }

    public required RectangleInfo Location { get; set; }

    public DefectType Type { get; set; }
}
