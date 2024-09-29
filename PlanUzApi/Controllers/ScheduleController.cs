using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanUzApi.Models;
using PlanUzApi.Services.Abstraction;
using PlanUzApi.Utilities;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace PlanUzApi.Controllers;

[ApiController]
public class ScheduleController : BaseController
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet("GetWebsiteSourceCode")]
    public async Task<string> GetWebsiteSourceCode([FromQuery] string websiteUrl)
    {
        return (await _scheduleService.GetWholeWebsiteSourceCode(websiteUrl)).Data;
    }

    [HttpGet("GetCourses")]
    public async Task<IEnumerable<Course>> GetCourses() => (await _scheduleService.GetCourses()).Data;

    [HttpGet("GetCourseGroups")]
    public async Task<IEnumerable<Group>> GetCourseGroups() => (await _scheduleService.GetCourseGroups()).Data;

    [HttpGet("GetClassesByGroupUrl")]
    public async Task<IEnumerable<ClassSession>> GetClassesByGroupUrl([FromQuery] string websiteUrl)
    {
        return (await _scheduleService.GetClassesByGroupUrl(websiteUrl)).Data;
    }

    [Authorize]
    [HttpGet("UpdateCourses")]
    public async Task<StatusCodeResult> UpdateCourses()
    {
        var result = await _scheduleService.UpdateCourses();

        return result.IsOkStatusCode() ? StatusCode(200) : StatusCode(500);
    }

    [Authorize]
    [HttpGet("UpdateCourseGroups")]
    public async Task<StatusCodeResult> UpdateCourseGroups()
    {
        var result = await _scheduleService.UpdateCourseGroups();

        return result.IsOkStatusCode() ? StatusCode(200) : StatusCode(500);
    }
}