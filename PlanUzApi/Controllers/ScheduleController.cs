using Microsoft.AspNetCore.Mvc;
using PlanUzApi.Models;

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
}