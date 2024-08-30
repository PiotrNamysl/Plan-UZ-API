namespace PlanUzApi;

public interface IScheduleService
{
    Task<string> GetWholeWebsiteSourceCode(string websiteUrl);
}