using AutoMapper;
using Newtonsoft.Json;
using Tema_Grafuri.Dtos;

public class TeachersData : ITypeConverter<TeachersData, TeachersDataDto>
{
    [JsonProperty("Teacher")]
    public string Teacher { get; set; } = string.Empty;

    [JsonProperty("Subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonProperty("Hours")]
    public string Hours { get; set; } = string.Empty;

    [JsonProperty("Groups")]
    public string Groups { get; set; } = string.Empty;

    public TeachersDataDto Convert(TeachersData source, TeachersDataDto destination, ResolutionContext context)
    {
        destination = destination ?? new TeachersDataDto();
        destination.Hours = source.Hours;
        destination.Subject = source.Subject;
        destination.Teacher = source.Teacher;
        destination.Groups = source.Groups.Split(' ').ToList();
        return destination;
    }
}

public class TeachersDataRoot
{
    [JsonProperty("TimetableData")]
    public List<TeachersData> Teachers { get; set; } = new();
}