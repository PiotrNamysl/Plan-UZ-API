using PlanUzApi.Models;
using PlanUzApi.Models.Enums;

namespace PlanUzApi;

public static class Helpers
{
    public static TypeOfClass GetTypeOfClass(string typeOfClassAsString)
    {
        return typeOfClassAsString switch
        {
            "W" => TypeOfClass.Lecture,
            "Ć" => TypeOfClass.Tutorial,
            "E" => TypeOfClass.Exam,
            "L" => TypeOfClass.Laboratory,
            "I" => TypeOfClass.Tutorial,
            _ => TypeOfClass.Tutorial
        };
    }
}