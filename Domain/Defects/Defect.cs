namespace Domain.Defects;

public class Defect : Entity
{
    public DefectType Type { get; set; }

    public DefectLocation Location { get; set; } = null!;

    public ImageEntity? PredictedFromImage { get; set; }
}
