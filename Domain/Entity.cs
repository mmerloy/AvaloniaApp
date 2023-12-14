using System.ComponentModel.DataAnnotations;

namespace Domain;

public class Entity
{
    [Key]
    public int Id { get; set; }
}
