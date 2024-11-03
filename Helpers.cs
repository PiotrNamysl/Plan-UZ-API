using PlanUzApi.Models;

namespace PlanUzApi;

public static class Helpers
{
    public static TypeOfClass GetTypeOfClass(string typeOfClassAsString)
    {
        switch (typeOfClassAsString)
        {
            case "W":
            {
                return TypeOfClass.Lecture;
            }
            case "Ć":
            {
                return TypeOfClass.Tutorial;
            }
            case "E":
            {
                return TypeOfClass.Exam;
            }
            case "L":
            {
                return TypeOfClass.Laboratory;
            }
            case "I":
            {
                return TypeOfClass.Tutorial;
            }
            default:
                return TypeOfClass.Tutorial;
        }
    }
}