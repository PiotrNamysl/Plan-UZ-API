using Microsoft.AspNetCore.Mvc;
using PlanUzApi.Models;
using PlanUzApi.Services.Abstraction;

namespace PlanUzApi.Controllers;

[ApiController]
public class ScheduleController : BaseController
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    // [HttpGet("GetWebsiteSourceCode")]
    // public async Task<string> GetWebsiteSourceCode([FromQuery] string websiteUrl)
    // {
    //     return (await _scheduleService.GetWholeWebsiteSourceCode(websiteUrl)).Data;
    // }

    [HttpGet("GetCourses")]
    public async Task<IResult<IEnumerable<Course>>> GetCourses() => await _scheduleService.GetCourses();

    [HttpGet("GetCourseGroups")]
    public async Task<IResult<IEnumerable<Group>>> GetCourseGroups() => await _scheduleService.GetCourseGroups();
    
    [HttpGet("GetClassesByGroupUrl")]
    public async Task<IResult<IEnumerable<ClassSession>>> Get() => await _scheduleService.GetClassesByGroupUrl("http://www.plan.uz.zgora.pl/grupy_plan.php?ID=28087");
}