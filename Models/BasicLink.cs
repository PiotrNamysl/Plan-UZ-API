namespace PlanUzApi.Models;

public record BasicLink
{
    public required string Name { get; init; }
    public required string Url { get; init; }
}