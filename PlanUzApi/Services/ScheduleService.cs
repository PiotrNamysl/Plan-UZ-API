using System.Data;
using System.Globalization;
using HtmlAgilityPack;
using Newtonsoft.Json;
using PlanUzApi.Models;
using PlanUzApi.Services.Abstraction;

namespace PlanUzApi.Services;

public class ScheduleService : IScheduleService
{
    private HttpClient _httpClient = new HttpClient();

    private List<Course> _courses = [];
    private DateTime _coursesLastUpdate;

    private List<Group> _groups = [];
    private DateTime _groupsLastUpdate;

    // public async Task<IResult<string>> GetWholeWebsiteSourceCode(string websiteUrl)
    // {
    //     return Result<string>.Success("");
    // }
    public async Task<IResult<IEnumerable<Course>>> GetCourses()
    {
        if (!_courses.Any())
        {
            var updateCoursesResult = await UpdateCourses();
            if (updateCoursesResult.IsSuccess)
            {
                return Result<IEnumerable<Course>>.Success(_courses, lastUpdate: _coursesLastUpdate);
            }
            else
            {
                return Result<IEnumerable<Course>>.Warning(updateCoursesResult.StatusCode, updateCoursesResult.Message);
            }
        }

        return Result<IEnumerable<Course>>.Success(_courses, lastUpdate: _coursesLastUpdate);
    }

    public async Task<IResult<IEnumerable<Group>>> GetCourseGroups()
    {
        if (!_groups.Any())
        {
            var updateCoursesResult = await UpdateCourseGroups();
            if (updateCoursesResult.IsSuccess)
            {
                return Result<IEnumerable<Group>>.Success(_groups, lastUpdate: _groupsLastUpdate);
            }
            else
            {
                return Result<IEnumerable<Group>>.Warning(updateCoursesResult.StatusCode, updateCoursesResult.Message);
            }
        }

        return Result<IEnumerable<Group>>.Success(_groups, lastUpdate: _groupsLastUpdate);
    }

    public async Task<IResult<IEnumerable<ClassSession>>> GetClassesByGroupUrl(string groupUrl)
    {
        try
        {
            List<ClassSession> groupClasses = [];

            var browser = new HtmlWeb();
            var htmlDocument = await browser.LoadFromWebAsync(groupUrl);

            var classesAsStringList = htmlDocument.GetElementbyId("table_details").InnerText.Split("\r\n\r\n").ToList();
            classesAsStringList.RemoveAt(0);
            classesAsStringList.RemoveAt(classesAsStringList.Count - 1);

            foreach (var item in classesAsStringList)
            {
                var classAsList = item.Split("\r\n").ToList().Select(c => c.Replace("  ", "").Replace("&nbsp;", null)).ToList();
                classAsList.RemoveAt(0);
                classAsList.RemoveAt(classAsList.Count - 1);

                var newClass = new ClassSession
                {
                    Day = DateOnly.Parse(classAsList[0]),
                    Subgroup = string.IsNullOrWhiteSpace(classAsList[2]) ? null : classAsList[2],
                    Since = TimeOnly.Parse(classAsList[3]),
                    To = TimeOnly.Parse(classAsList[4]),
                    Subject = classAsList[5],
                    TypeOfClass = Helpers.GetTypeOfClass(classAsList[6]),
                    Teacher = classAsList[7],
                    Location = classAsList[8]
                };
                groupClasses.Add(newClass);
            }

            return Result<IEnumerable<ClassSession>>.Success(groupClasses);
        }
        catch (Exception exception)
        {
            return Result<IEnumerable<ClassSession>>.Error(ResultStatusCode.UnknownError, $"Unexpected error occured: {exception.Message}");
        }
    }

    public async Task<IResult> UpdateCourses()
    {
        var browser = new HtmlWeb();
        try
        {
            var htmlDocument = await browser.LoadFromWebAsync("http://www.plan.uz.zgora.pl/grupy_lista_kierunkow.php");

            var aElements = htmlDocument.DocumentNode
                .SelectNodes("//a[@href]")
                .Where(n => n.OuterHtml.Contains("lista_grup_kierunku"))
                .Select(n => n.OuterHtml);

            var courses = new List<Course>();

            foreach (var element in aElements)
            {
                var courseGroupsName = element.Split(">")[1].Split("<")[0];
                var courseGroupsUrl = "http://www.plan.uz.zgora.pl/" +
                                      element.Replace("<a href=", "").Split(">")[0].Replace("\\", "").Replace("\"", "");

                courses.Add(new Course
                {
                    Name = courseGroupsName,
                    Url = courseGroupsUrl
                });
            }

            _courses = courses;
            _coursesLastUpdate = DateTime.Now.ToUniversalTime();
            return Result.Success();
        }
        catch (Exception exception)
        {
            return Result.Error(ResultStatusCode.UnknownError, $"Unexpected error occured: {exception.Message}");
        }
    }

    public async Task<IResult> UpdateCourseGroups()
    {
        if (!_courses.Any())
            await Task.Run(async () => await GetCourses());
        try
        {
            var tasks = _courses.Select(async course =>
            {
                var browser = new HtmlWeb();
                var htmlDocument = await browser.LoadFromWebAsync(course.Url);

                var aElements = htmlDocument.DocumentNode
                    .SelectNodes("//a[@href]")
                    .Where(n => n.OuterHtml.Contains("grupy_plan"))
                    .Select(n => n.OuterHtml);

                var temporaryGroups = new List<Group>();

                foreach (var aElement in aElements)
                {
                    var courseGroupsName = aElement.Split('>')[1].Split('<')[0];
                    var courseGroupsUrl = "http://www.plan.uz.zgora.pl/" + aElement
                        .Replace("<a href=", "")
                        .Split('>')[0]
                        .Replace("\\", "")
                        .Replace("\"", "");

                    temporaryGroups.Add(new Group
                    {
                        Name = courseGroupsName,
                        Url = courseGroupsUrl
                    });
                }

                lock (_groups)
                {
                    _groups.AddRange(temporaryGroups);
                }
            });

            await Task.WhenAll(tasks);

            _groups = _groups.OrderBy(g => g.Name).ToList();
            _groupsLastUpdate = DateTime.Now.ToUniversalTime();

            return Result.Success();
        }
        catch (Exception exception)
        {
            return Result.Error(ResultStatusCode.UnknownError, $"Unexpected error occured: {exception.Message}");
        }
    }
}