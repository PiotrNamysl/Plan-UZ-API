using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanUzApi.Models;
using PlanUzApi.Services.Abstraction;
using PlanUzApi.Utilities;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace PlanUzApi.Controllers;

[ApiController]
public class ScheduleController(IScheduleService scheduleService) : BaseController
{
    [HttpGet("GetWebsiteSourceCode")]
    public async Task<string> GetWebsiteSourceCodeAsync([FromQuery] string websiteUrl)
    {
        return (await scheduleService.GetWholeWebsiteSourceCodeAsync(websiteUrl)).Data;
    }

    [HttpGet("GetCourses")]
    public async Task<IEnumerable<BasicLink>> GetCoursesAsync() => (await scheduleService.GetCoursesAsync()).Data;

    [HttpGet("GetCourseGroups")]
    public async Task<IEnumerable<BasicLink>> GetCourseGroupsAsync() => (await scheduleService.GetCourseGroupsAsync()).Data;

    [HttpGet("GetClassesByGroupUrl")]
    public async Task<IEnumerable<ClassSession>> GetClassesByGroupUrlAsync([FromQuery] string websiteUrl)
    {
        return (await scheduleService.GetClassesByGroupUrlAsync(websiteUrl)).Data;
    }

    [HttpGet("UpdateCourses")]
    public async Task<StatusCodeResult> UpdateCoursesAsync()
    {
        var result = await scheduleService.UpdateCoursesAsync();

        return result.IsOkStatusCode() ? StatusCode(200) : StatusCode(500);
    }

    [HttpGet("UpdateCourseGroups")]
    public async Task<StatusCodeResult> UpdateCourseGroupsAsync()
    {
        var result = await scheduleService.UpdateCourseGroupsAsync();

        return result.IsOkStatusCode() ? StatusCode(200) : StatusCode(500);
    }
}