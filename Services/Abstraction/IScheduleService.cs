using PlanUzApi.Models;
using PlanUzApi.Utilities;
using IResult = PlanUzApi.Utilities.IResult;

namespace PlanUzApi.Services.Abstraction;

public interface IScheduleService
{
    Task<IResult<string>> GetWholeWebsiteSourceCodeAsync(string websiteUrl);
    Task<IResult<IEnumerable<BasicLink>>> GetCoursesAsync();
    Task<IResult<IEnumerable<BasicLink>>> GetCourseGroupsAsync();
    Task<IResult<IEnumerable<ClassSession>>> GetClassesByGroupUrlAsync(string groupUrl);
    Task<IResult> UpdateCoursesAsync();
    Task<IResult> UpdateCourseGroupsAsync();
}