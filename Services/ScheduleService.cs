using HtmlAgilityPack;
using PlanUzApi.Models;
using PlanUzApi.Services.Abstraction;
using PlanUzApi.Utilities;
using IResult = PlanUzApi.Utilities.IResult;

namespace PlanUzApi.Services;

public class ScheduleService : IScheduleService
{
    private readonly HtmlWeb _browser = new();

    private List<BasicLink> _courses = [];
    private DateTime _coursesLastUpdate;

    private List<BasicLink> _groups = [];
    private DateTime _groupsLastUpdate;

    public async Task<IResult<string>> GetWholeWebsiteSourceCodeAsync(string websiteUrl)
    {
        var browser = new HtmlWeb();
        var htmlDocument = await browser.LoadFromWebAsync(websiteUrl);

        return Result<string>.Success(htmlDocument.Text);
    }

    public async Task<IResult<IEnumerable<BasicLink>>> GetCoursesAsync()
    {
        if (_courses.Count == 0)
        {
            var updateCoursesResult = await UpdateCoursesAsync();
            return updateCoursesResult.IsSuccess
                ? Result<IEnumerable<BasicLink>>.Success(_courses, lastUpdate: _coursesLastUpdate)
                : Result<IEnumerable<BasicLink>>.Warning(updateCoursesResult.StatusCode, updateCoursesResult.Message);
        }

        return Result<IEnumerable<BasicLink>>.Success(_courses, lastUpdate: _coursesLastUpdate);
    }

    public async Task<IResult<IEnumerable<BasicLink>>> GetCourseGroupsAsync()
    {
        if (_groups.Count == 0)
        {
            var updateCoursesResult = await UpdateCourseGroupsAsync();
            return updateCoursesResult.IsSuccess
                ? Result<IEnumerable<BasicLink>>.Success(_groups, lastUpdate: _groupsLastUpdate)
                : Result<IEnumerable<BasicLink>>.Warning(updateCoursesResult.StatusCode, updateCoursesResult.Message);
        }

        return Result<IEnumerable<BasicLink>>.Success(_groups, lastUpdate: _groupsLastUpdate);
    }

    public async Task<IResult<IEnumerable<ClassSession>>> GetClassesByGroupUrlAsync(string groupUrl)
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
                        Date = classDay,
                        Subgroup = string.IsNullOrWhiteSpace(classAsList[2]) ? null : classAsList[2].Trim(),
                        Since = classSince,
                        To = classTo,
                        Subject = classAsList[5].Trim(),
                        TypeOfClass = Helpers.GetTypeOfClass(classAsList[6]),
                        Teacher = string.IsNullOrWhiteSpace(classAsList[7]) ? null : classAsList[7].Trim(),
                        Location = string.IsNullOrWhiteSpace(classAsList[8]) ? null : classAsList[8].Trim(),
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

    public async Task<IResult> UpdateCoursesAsync()
    {
        var browser = new HtmlWeb();
        try
        {
            var htmlDocument = await browser.LoadFromWebAsync("http://www.plan.uz.zgora.pl/grupy_lista_kierunkow.php");

            var aElements = htmlDocument.DocumentNode
                .SelectNodes("//a[@href]")
                .Where(n => n.OuterHtml.Contains("lista_grup_kierunku"))
                .Select(n => n.OuterHtml);

            var courses = new List<BasicLink>();

            foreach (var element in aElements)
            {
                var courseGroupsName = element.Split(">")[1].Split("<")[0];
                var courseGroupsUrl = "http://www.plan.uz.zgora.pl/" +
                                      element.Replace("<a href=", "").Split(">")[0].Replace("\\", "").Replace("\"", "");

                courses.Add(new BasicLink
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

    public async Task<IResult> UpdateCourseGroupsAsync()
    {
        if (_courses.Count is 0)
            await Task.Run(async () => await GetCoursesAsync());

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

                var temporaryGroups = new List<BasicLink>();

                foreach (var aElement in aElements)
                {
                    var courseGroupsName = aElement.Split('>')[1].Split('<')[0];
                    var courseGroupsUrl = "http://www.plan.uz.zgora.pl/" + aElement
                        .Replace("<a href=", "")
                        .Split('>')[0]
                        .Replace("\\", "")
                        .Replace("\"", "");

                    temporaryGroups.Add(new BasicLink
                    {
                        Name = courseGroupsName,
                        Url = courseGroupsUrl
                    });
                }

                lock (_groups)
                    _groups.AddRange(temporaryGroups);
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