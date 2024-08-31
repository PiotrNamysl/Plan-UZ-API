namespace PlanUzApi.Models;

public class ClassSession
{
    public DateTime SinceDateTime { get; set; }
    public DateTime ToDateTime { get; set; }
    public string? Subgroup { get; set; }
    public string? Subject { get; set; }
    public TypeOfClass TypeOfClass { get; set; }
    public string? Teacher { get; set; }
    public string? Location { get; set; }
    public bool AreStacionary { get; set; }
}

public enum TypeOfClass
{
    Lecture,
    Tutorial,
    Laboratory,
    Exam,
}