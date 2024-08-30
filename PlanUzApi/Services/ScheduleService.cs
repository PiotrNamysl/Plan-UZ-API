using HtmlAgilityPack;
using Newtonsoft.Json;
using PlanUzApi.Models;

namespace PlanUzApi.Services;

public class ScheduleService : IScheduleService
{
    private HttpClient _httpClient = new HttpClient();

    // public async Task<IResult<string>> GetWholeWebsiteSourceCode(string websiteUrl)
    // {
    //     return Result<string>.Success("");
    // }

    public async Task<IResult<IEnumerable<Course>>> GetCourses()
    {
        var browser = new HtmlWeb();
        try
        {
            var htmlDocument = await browser.LoadFromWebAsync("http://www.plan.uz.zgora.pl/grupy_lista_kierunkow.php");
            var idk = htmlDocument.DocumentNode
                .SelectNodes("//a[@href]")
                .Where(n => n.OuterHtml.Contains("lista_grup_kierunku"))
                .Select(n => n.OuterHtml);

            var courses = new List<Course>();

            foreach (var line in idk)
            {
                var courseGroupsName = line.Split(">")[1].Split("<")[0];
                var courseGroupsUrl = "http://www.plan.uz.zgora.pl/" + line.Replace("<a href=", "").Split(">")[0].Replace("\\", "").Replace("\"", "");

                courses.Add(new Course
                {
                    Name = courseGroupsName,
                    Url = courseGroupsUrl
                });
            }

            return Result<IEnumerable<Course>>.Success(courses);
        }
        catch (Exception exception)
        {
            return Result<IEnumerable<Course>>.Error(ResultStatusCode.UnknownError, $"Unexpected error occured: {exception.Message}");
        }
    }
}