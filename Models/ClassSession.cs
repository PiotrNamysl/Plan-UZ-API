using PlanUzApi.Models.Enums;

namespace PlanUzApi.Models;

public class ClassSession
{
    public DateOnly? Date { get; set; }
    public TimeOnly? Since { get; set; }
    public TimeOnly? To { get; set; }
    public string? Subgroup { get; set; }
    public string? Subject { get; set; }
    public TypeOfClass TypeOfClass { get; set; }
    public string? Teacher { get; set; }
    public string? Location { get; set; }
}