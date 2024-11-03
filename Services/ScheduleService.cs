using System.Data;
using System.Globalization;
using HtmlAgilityPack;
using Newtonsoft.Json;
using PlanUzApi.Models;
using PlanUzApi.Services.Abstraction;
using PlanUzApi.Utilities;
using IResult = PlanUzApi.Utilities.IResult;

namespace PlanUzApi.Services;

public class ScheduleService : IScheduleService
{
    private readonly HtmlWeb _browser = new HtmlWeb();

    private List<Course> _courses = [];
    private DateTime _coursesLastUpdate;

    private List<Group> _groups = [];
    private DateTime _groupsLastUpdate;

    public async Task<IResult<string>> GetWholeWebsiteSourceCode(string websiteUrl)
    {
        var browser = new HtmlWeb();
        var htmlDocument = await browser.LoadFromWebAsync(websiteUrl);

        return Result<string>.Success(htmlDocument.Text);
    }

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

            var htmlDocument = await _browser.LoadFromWebAsync(groupUrl);

            var classesAsString = htmlDocument.GetElementbyId("table_details").InnerText.Split("\r\n\r\n").ToList();
            classesAsString.RemoveAt(0);
            classesAsString.RemoveAt(classesAsString.Count - 1);

            var classesAsHtml = string.Join("", htmlDocument.GetElementbyId("table_details").OuterHtml.Split("\r\n")).Split("</tr>").ToList();
            classesAsHtml.RemoveAt(0);
            classesAsHtml.RemoveAt(classesAsString.Count - 1);

            for (var i = 0; i < classesAsString.Count; i++)
            {
                var classAsList = classesAsString[i].Split("\r\n").ToList().Select(c => c.Replace("  ", "").Replace("&nbsp;", null)).ToList();
                if (classAsList.Count > 4)
                {
                    classAsList.RemoveAt(0);
                    classAsList.RemoveAt(classAsList.Count - 1);

                    DateOnly? classDay = null;
                    TimeOnly? classSince = null;
                    TimeOnly? classTo = null;

                    if (!string.IsNullOrWhiteSpace(classAsList[0]))
                        classDay = DateOnly.Parse(classAsList[0]);

                    if (!string.IsNullOrWhiteSpace(classAsList[3]))
                        classSince = TimeOnly.Parse(classAsList[3]);

                    if (!string.IsNullOrWhiteSpace(classAsList[4]))
                        classTo = TimeOnly.Parse(classAsList[4]);

                    var newClass = new ClassSession
                    {
                        Day = classDay,
                        Subgroup = string.IsNullOrWhiteSpace(classAsList[2]) ? null : classAsList[2].Trim(),
                        Since = classSince,
                        To = classTo,
                        Subject = classAsList[5].Trim(),
                        TypeOfClass = Helpers.GetTypeOfClass(classAsList[6]),
                        Teacher = classAsList[7].Trim(),
                        Location = classAsList[8].Trim()
                    };

                    var isClassRemote = classesAsHtml[i].Contains("Zajęcia zdalne");
                    if (isClassRemote)
                        newClass.Location = "Zdalnie";

                    groupClasses.Add(newClass);
                }
                else
                    groupClasses[^1].Location = classAsList[2].Trim();
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

        _groups.Clear();
        
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