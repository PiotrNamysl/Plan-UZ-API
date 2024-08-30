using PlanUzApi.Models;

namespace PlanUzApi;

public interface IScheduleService
{
    // Task<IResult<string>> GetWholeWebsiteSourceCode(string websiteUrl);
    Task<IResult<IEnumerable<Course>>> GetCourses();
}