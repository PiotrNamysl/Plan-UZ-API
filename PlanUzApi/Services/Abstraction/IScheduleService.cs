using PlanUzApi.Models;

namespace PlanUzApi.Services.Abstraction;

public interface IScheduleService
{
    // Task<IResult<string>> GetWholeWebsiteSourceCode(string websiteUrl);
    Task<IResult<IEnumerable<Course>>> GetCourses();
    Task<IResult<IEnumerable<Group>>> GetCourseGroups();
    Task<IResult<IEnumerable<ClassSession>>> GetClassesByGroupUrl(string groupUrl);
    Task<IResult> UpdateCourses();
    Task<IResult> UpdateCourseGroups();
}